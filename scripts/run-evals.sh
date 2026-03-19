#!/usr/bin/env bash
# run-evals.sh — Execute JSONL trigger test cases against skills
#
# Usage:
#   ./scripts/run-evals.sh [skill-name]       # Run evals for one skill
#   ./scripts/run-evals.sh --all              # Run evals for all skills with evals/
#   ./scripts/run-evals.sh --dry-run [skill]  # Show test cases without running
#
# Requires: copilot CLI, jq
#
# The script reads evals/trigger-positive.jsonl and evals/trigger-negative.jsonl,
# sends each prompt to the copilot CLI, checks if the target skill was mentioned
# in the response, and reports precision/recall.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RESULTS_DIR="${REPO_ROOT}/eval-results"
DRY_RUN=false
TARGETS=()
MODEL="${EVAL_MODEL:-claude-sonnet-4.5}"
TIMEOUT="${EVAL_TIMEOUT:-60}"

usage() {
  echo "Usage: $0 [--all | --dry-run] [skill-name ...]"
  echo ""
  echo "Options:"
  echo "  --all       Run evals for all skills that have evals/ directories"
  echo "  --dry-run   List test cases without executing them"
  echo "  --model X   Override model (default: claude-sonnet-4.5)"
  echo "  --timeout N Seconds per prompt (default: 60)"
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

mkdir -p "$RESULTS_DIR"

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

    # Run the prompt through copilot CLI
    local response
    response=$(timeout "$TIMEOUT" copilot -p "$prompt" \
      --model "$MODEL" \
      --reasoning-effort low \
      --allow-all \
      --autopilot 2>/dev/null || echo "ERROR: timeout or failure")

    # Check if the target skill was activated
    local skill_mentioned=false
    if echo "$response" | grep -qi "$skill"; then
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

# Main loop
echo "═══════════════════════════════════════════"
echo "  Meta-Skill Eval Runner"
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
  echo "Model: ${MODEL}" >> "${RESULTS_DIR}/${skill}-eval.md"
  echo "" >> "${RESULTS_DIR}/${skill}-eval.md"

  run_trigger_tests "$skill" "$skill_dir/evals/trigger-positive.jsonl" "positive"
  run_trigger_tests "$skill" "$skill_dir/evals/trigger-negative.jsonl" "negative"

  echo ""
  echo "  Results saved: eval-results/${skill}-eval.md"
  echo ""
done

echo "═══════════════════════════════════════════"
echo "  All eval results in: ${RESULTS_DIR}/"
echo "═══════════════════════════════════════════"
