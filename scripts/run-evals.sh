#!/usr/bin/env bash
# run-evals.sh — Execute JSONL trigger and behavior test cases against skills
#
# Usage:
#   ./scripts/run-evals.sh [skill-name]       # Run evals for one skill
#   ./scripts/run-evals.sh --all              # Run evals for all skills with evals/
#   ./scripts/run-evals.sh --dry-run [skill]  # Show test cases without running
#
# Requires: selected runtime CLI, jq
#
# The script reads evals/trigger-positive.jsonl, evals/trigger-negative.jsonl,
# and evals/behavior.jsonl. Trigger tests check skill routing (precision/recall).
# Behavior tests check output format compliance (required patterns, forbidden
# patterns, minimum length).

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RESULTS_DIR="${REPO_ROOT}/eval-results"
DRY_RUN=false
TARGETS=()
MODEL="${EVAL_MODEL:-claude-sonnet-4.5}"
TIMEOUT="${EVAL_TIMEOUT:-60}"
RUNTIME="${EVAL_RUNTIME:-copilot}"

usage() {
  echo "Usage: $0 [--all | --dry-run] [skill-name ...]"
  echo ""
  echo "Options:"
  echo "  --all       Run evals for all skills that have evals/ directories"
  echo "  --dry-run   List test cases without executing them"
  echo "  --model X   Override model (default: claude-sonnet-4.5)"
  echo "  --timeout N Seconds per prompt (default: 60)"
  echo "  --runtime X Runtime CLI command (default: copilot)"
  exit 1
}

# Parse args
while [[ $# -gt 0 ]]; do
  case "$1" in
    --all)
      for d in "${REPO_ROOT}"/*/evals; do
        skill="$(basename "$(dirname "$d")")"
        TARGETS+=("$skill")
      done
      shift
      ;;
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --model)
      MODEL="$2"
      shift 2
      ;;
    --timeout)
      TIMEOUT="$2"
      shift 2
      ;;
    --runtime)
      RUNTIME="$2"
      shift 2
      ;;
    --help|-h)
      usage
      ;;
    *)
      TARGETS+=("$1")
      shift
      ;;
  esac
done

if [[ ${#TARGETS[@]} -eq 0 ]]; then
  echo "Error: specify skill name(s) or --all"
  usage
fi

# Check dependencies
command -v jq >/dev/null 2>&1 || { echo "Error: jq is required"; exit 1; }
command -v "$RUNTIME" >/dev/null 2>&1 || { echo "Error: runtime not found: $RUNTIME"; exit 1; }

mkdir -p "$RESULTS_DIR"

run_prompt() {
  local prompt="$1"
  local cmd=("$RUNTIME" -p "$prompt")

  if [[ "$RUNTIME" == "copilot" ]]; then
    if [[ "$MODEL" != "auto" ]]; then
      cmd+=("--model" "$MODEL")
    fi
    cmd+=("--reasoning-effort" "low" "--allow-all" "--autopilot")
  else
    if [[ "$MODEL" != "auto" ]]; then
      cmd+=("--model" "$MODEL")
    fi
  fi

  timeout "$TIMEOUT" "${cmd[@]}" 2>/dev/null || echo "ERROR: timeout or failure"
}

run_trigger_tests() {
  local skill="$1"
  local jsonl_file="$2"
  local test_type="$3"  # "positive" or "negative"
  local total=0
  local pass=0
  local fail=0
  local errors=()

  if [[ ! -f "$jsonl_file" ]]; then
    echo "  ⏭  No ${test_type} test file: $(basename "$jsonl_file")"
    return
  fi

  local case_count
  case_count=$(wc -l < "$jsonl_file")
  echo "  Running ${case_count} ${test_type} cases..."

  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    total=$((total + 1))

    local prompt expected
    prompt=$(echo "$line" | jq -r '.prompt')
    expected=$(echo "$line" | jq -r '.expected')
    local category
    category=$(echo "$line" | jq -r '.category // .better_skill // "unknown"')

    if $DRY_RUN; then
      echo "    [${total}] ${expected}: ${prompt}"
      continue
    fi

    # Run the prompt through selected runtime
    local response
    response=$(run_prompt "$prompt")

    # Check if the target skill was activated (use -F for fixed string, not regex)
    local skill_mentioned=false
    if echo "$response" | grep -qFi "$skill"; then
      skill_mentioned=true
    fi

    local test_passed=false
    if [[ "$expected" == "trigger" ]] && $skill_mentioned; then
      test_passed=true
    elif [[ "$expected" == "no_trigger" ]] && ! $skill_mentioned; then
      test_passed=true
    fi

    if $test_passed; then
      pass=$((pass + 1))
      echo "    ✅ [${total}] PASS (${category}): ${prompt:0:60}..."
    else
      fail=$((fail + 1))
      errors+=("    ❌ [${total}] FAIL (${category}): ${prompt:0:60}... [expected=${expected}, mentioned=${skill_mentioned}]")
      echo "${errors[-1]}"
    fi
  done < "$jsonl_file"

  if $DRY_RUN; then
    echo "    Total: ${total} cases (dry run)"
    return
  fi

  echo ""
  echo "  ${test_type} results: ${pass}/${total} passed (${fail} failed)"

  # Write results to file
  {
    echo "## ${test_type^} trigger tests: ${skill}"
    echo ""
    echo "| Metric | Value |"
    echo "|--------|-------|"
    echo "| Total  | ${total} |"
    echo "| Passed | ${pass} |"
    echo "| Failed | ${fail} |"
    if [[ $total -gt 0 ]]; then
      local rate=$((pass * 100 / total))
      echo "| Rate   | ${rate}% |"
    fi
    echo ""
    if [[ ${#errors[@]} -gt 0 ]]; then
      echo "### Failures"
      for err in "${errors[@]}"; do
        echo "$err"
      done
      echo ""
    fi
  } >> "${RESULTS_DIR}/${skill}-eval.md"
}

run_behavior_tests() {
  local skill="$1"
  local jsonl_file="$2"
  local total=0
  local pass=0
  local fail=0
  local errors=()

  if [[ ! -f "$jsonl_file" ]]; then
    echo "  ⏭  No behavior test file"
    return
  fi

  local case_count
  case_count=$(wc -l < "$jsonl_file")
  echo "  Running ${case_count} behavior cases..."

  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    total=$((total + 1))

    local prompt
    prompt=$(echo "$line" | jq -r '.prompt')
    local min_lines
    min_lines=$(echo "$line" | jq -r '.min_output_lines // 10')

    if $DRY_RUN; then
      local sections
      sections=$(echo "$line" | jq -r '.expected_sections // [] | join(", ")')
      echo "    [${total}] behavior: ${prompt:0:60}..."
      echo "           expected: ${sections}"
      continue
    fi

    # Run the prompt through selected runtime
    local response
    response=$(run_prompt "$prompt")

    local response_lines
    response_lines=$(printf '%s' "$response" | grep -c "^")
    local section_pass=true
    local pattern_pass=true
    local forbidden_pass=true
    local length_pass=true
    local fail_reasons=()

    # Check minimum output length
    if [[ $response_lines -lt $min_lines ]]; then
      length_pass=false
      fail_reasons+=("too short (${response_lines} < ${min_lines} lines)")
    fi

    # Check required patterns (use -F for fixed string matching, preventing regex injection)
    while IFS= read -r pattern; do
      [[ -z "$pattern" ]] && continue
      if ! printf '%s\n' "$response" | grep -qFi "$pattern"; then
        pattern_pass=false
        fail_reasons+=("missing required: ${pattern}")
      fi
    done < <(echo "$line" | jq -r '.required_patterns // [] | .[]')

    # Check forbidden patterns (use -F for fixed string matching, preventing regex injection)
    while IFS= read -r pattern; do
      [[ -z "$pattern" ]] && continue
      if printf '%s\n' "$response" | grep -qFi "$pattern"; then
        forbidden_pass=false
        fail_reasons+=("contains forbidden: ${pattern}")
      fi
    done < <(echo "$line" | jq -r '.forbidden_patterns // [] | .[]')

    if $section_pass && $pattern_pass && $forbidden_pass && $length_pass; then
      pass=$((pass + 1))
      echo "    ✅ [${total}] PASS: ${prompt:0:60}..."
    else
      fail=$((fail + 1))
      local reason_str
      reason_str=$(printf '%s; ' "${fail_reasons[@]}")
      errors+=("    ❌ [${total}] FAIL: ${prompt:0:60}... [${reason_str}]")
      echo "${errors[-1]}"
    fi
  done < "$jsonl_file"

  if $DRY_RUN; then
    echo "    Total: ${total} cases (dry run)"
    return
  fi

  echo ""
  echo "  behavior results: ${pass}/${total} passed (${fail} failed)"

  {
    echo "## Behavior tests: ${skill}"
    echo ""
    echo "| Metric | Value |"
    echo "|--------|-------|"
    echo "| Total  | ${total} |"
    echo "| Passed | ${pass} |"
    echo "| Failed | ${fail} |"
    if [[ $total -gt 0 ]]; then
      local rate=$((pass * 100 / total))
      echo "| Rate   | ${rate}% |"
    fi
    echo ""
    if [[ ${#errors[@]} -gt 0 ]]; then
      echo "### Failures"
      for err in "${errors[@]}"; do
        echo "$err"
      done
      echo ""
    fi
  } >> "${RESULTS_DIR}/${skill}-eval.md"
}

# Main loop
echo "═══════════════════════════════════════════"
echo "  Meta-Skill Eval Runner"
echo "  Runtime: ${RUNTIME}"
echo "  Model: ${MODEL}"
echo "  Mode: $(if $DRY_RUN; then echo 'DRY RUN'; else echo 'LIVE'; fi)"
echo "═══════════════════════════════════════════"
echo ""

for skill in "${TARGETS[@]}"; do
  skill_dir="${REPO_ROOT}/${skill}"

  if [[ ! -d "$skill_dir" ]]; then
    echo "⚠️  Skill not found: ${skill}"
    continue
  fi

  if [[ ! -d "$skill_dir/evals" ]]; then
    echo "⚠️  No evals/ directory: ${skill}"
    continue
  fi

  echo "━━━ ${skill} ━━━"

  # Clear previous results
  > "${RESULTS_DIR}/${skill}-eval.md"
  echo "# Eval Results: ${skill}" >> "${RESULTS_DIR}/${skill}-eval.md"
  echo "Date: $(date -Iseconds)" >> "${RESULTS_DIR}/${skill}-eval.md"
  echo "Runtime: ${RUNTIME}" >> "${RESULTS_DIR}/${skill}-eval.md"
  echo "Model: ${MODEL}" >> "${RESULTS_DIR}/${skill}-eval.md"
  echo "" >> "${RESULTS_DIR}/${skill}-eval.md"

  run_trigger_tests "$skill" "$skill_dir/evals/trigger-positive.jsonl" "positive"
  run_trigger_tests "$skill" "$skill_dir/evals/trigger-negative.jsonl" "negative"
  run_behavior_tests "$skill" "$skill_dir/evals/behavior.jsonl"

  echo ""
  echo "  Results saved: eval-results/${skill}-eval.md"
  echo ""
done

echo "═══════════════════════════════════════════"
echo "  All eval results in: ${RESULTS_DIR}/"
echo "═══════════════════════════════════════════"
