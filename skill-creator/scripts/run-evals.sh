#!/usr/bin/env bash
# run-evals.sh — Execute JSONL trigger and behavior test cases against skills
#
# Usage:
#   ./scripts/run-evals.sh [skill-name]       # Run evals for one skill
#   ./scripts/run-evals.sh --all              # Run evals for all skills with evals/
#   ./scripts/run-evals.sh --dry-run [skill]  # Show test cases without running
#
# Requires: copilot CLI, jq
#
# The script reads evals/trigger-positive.jsonl, evals/trigger-negative.jsonl,
# and evals/behavior.jsonl. Trigger tests check skill routing (positive trigger
# rate = TP/(TP+FN), negative rejection rate = TN/(TN+FP)).
# Behavior tests check output format compliance (required patterns, forbidden
# patterns, minimum length).
#
# USEFULNESS EVALUATION (opt-in):
#
# --usefulness: After structural behavior checks, runs an LLM-as-Judge pass
#   on behavior test cases that include a "usefulness_criteria" field. A second
#   LLM call scores the output across four dimensions (correctness, completeness,
#   actionability, conciseness) on a 1–5 scale. Adds a 5th gate (aggregate
#   usefulness score ≥ threshold). Use USEFULNESS_MODEL to set a different judge
#   model and avoid self-evaluation bias.
#
# ROUTING DETECTION MODES:
#
# --observe (default): Parses structured JSON output (--output-format json)
#   from copilot CLI to detect whether the model actually opened the target
#   skill's SKILL.md file via the view tool. This detects actual file reads,
#   not name mentions. Single-run, accurate.
#
# --strict: Differential testing — runs each prompt twice: once normally and
#   once with --no-custom-instructions (disabling AGENTS.md and all project
#   instructions, not just the target skill). If outputs differ meaningfully
#   (>20% character difference), instructions influenced the response.
#   NOTE: This tests whether ANY custom instruction influenced the response,
#   not whether the specific target skill was activated. For per-skill
#   detection, use --observe instead. Slowest (2x prompts).
#
# MULTI-RUN VARIANCE REDUCTION:
#
# --runs N: Run each prompt N times and use majority voting for pass/fail.
#   LLM responses are non-deterministic — a single run can produce false
#   results. With --runs 3, a test case passes only if it passes in ≥2 of
#   3 runs. Default: 1 (single run). Multiplies total API calls by N.
#
# RELIABILITY FLAGS (applied to all copilot calls):
#   -s                          Strip stats/metadata from output
#   --no-ask-user               Prevent model from blocking on questions
#   --max-autopilot-continues 3 Bound runaway agent loops
#   --reasoning-effort          Controlled via EVAL_REASONING_EFFORT env var

set -euo pipefail

# Auto-detect repo root: walk up from script location looking for AGENTS.md
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root (no AGENTS.md found)"; exit 1; }
RESULTS_DIR="${REPO_ROOT}/eval-results"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
DRY_RUN=false
TARGETS=()
MODEL="${EVAL_MODEL:-gpt-4.1}"
TIMEOUT="${EVAL_TIMEOUT:-60}"
ROUTING_MODE="${EVAL_ROUTING:-observe}"
REASONING_EFFORT="${EVAL_REASONING_EFFORT:-}"
RUNS="${EVAL_RUNS:-1}"
JSON_OUTPUT=false
USEFULNESS=false
USEFULNESS_MODEL="${USEFULNESS_MODEL:-}"
USEFULNESS_THRESHOLD="${USEFULNESS_THRESHOLD:-3}"
USEFULNESS_TIMEOUT="${USEFULNESS_TIMEOUT:-45}"
USEFULNESS_RUNS="${USEFULNESS_RUNS:-1}"

# Gate tracking globals (set by test functions, read by run_gates)
GATE_POS_PASS=0
GATE_POS_TOTAL=0
GATE_NEG_PASS=0
GATE_NEG_TOTAL=0
GATE_BEH_PASS=0
GATE_BEH_TOTAL=0
GATE_USE_SCORE_SUM=0
GATE_USE_SCORE_COUNT=0
CURRENT_REPORT=""
OVERALL_FAIL=0

usage() {
  echo "Usage: $0 [--all | --dry-run | --observe | --strict | --json | --runs N | --usefulness] [skill-name ...]"
  echo ""
  echo "Options:"
  echo "  --all         Run evals for all skills that have evals/ directories"
  echo "  --dry-run     List test cases without executing them"
  echo "  --observe     JSON-based routing: detects actual SKILL.md file reads (default)"
  echo "  --strict      Differential testing: with vs without custom instructions (2x slower)"
  echo "  --json        Emit machine-readable JSON summary to stdout after report"
  echo "  --runs N      Run each prompt N times, majority vote for pass/fail (default: 1)"
  echo "  --model X     Override model (default: gpt-4.1)"
  echo "  --timeout N   Seconds per prompt (default: 60)"
  echo "  --usefulness  Enable LLM-as-Judge usefulness scoring for behavior tests"
  echo ""
  echo "Environment variables:"
  echo "  EVAL_MODEL              Model to use (default: gpt-4.1)"
  echo "  EVAL_TIMEOUT            Seconds per prompt (default: 60)"
  echo "  EVAL_ROUTING            Routing mode: observe|strict"
  echo "  EVAL_RUNS               Runs per prompt for majority voting (default: 1)"
  echo "  EVAL_REASONING_EFFORT   Reasoning effort: low|medium|high (omit for model default)"
  echo "  USEFULNESS_MODEL        Model for usefulness judge (default: EVAL_MODEL)"
  echo "  USEFULNESS_THRESHOLD    Minimum average score 1-5 to pass (default: 3)"
  echo "  USEFULNESS_TIMEOUT      Seconds per judge call (default: 45)"
  echo "  USEFULNESS_RUNS         Judge runs per case for median voting (default: 1)"
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
    --observe)
      ROUTING_MODE="observe"
      shift
      ;;
    --strict)
      ROUTING_MODE="strict"
      shift
      ;;
    --json)
      JSON_OUTPUT=true
      shift
      ;;
    --usefulness)
      USEFULNESS=true
      shift
      ;;
    --runs)
      RUNS="$2"
      shift 2
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

# Majority threshold: need >50% of runs to pass
MAJORITY_THRESHOLD=$(( (RUNS + 1) / 2 ))

# ---------------------------------------------------------------------------
# Helper: run a prompt through copilot CLI with standard reliability flags
# Usage: run_copilot_prompt <prompt> [extra_flags...]
# Outputs response text to stdout.
# ---------------------------------------------------------------------------
run_copilot_prompt() {
  local prompt="$1"
  shift
  local args=(-p "$prompt" --model "$MODEL" -s --no-ask-user --allow-all --autopilot --max-autopilot-continues 3)
  if [[ -n "$REASONING_EFFORT" ]]; then
    args+=(--reasoning-effort "$REASONING_EFFORT")
  fi
  if [[ $# -gt 0 ]]; then
    args+=("$@")
  fi
  timeout "$TIMEOUT" copilot "${args[@]}" 2>/dev/null || echo "ERROR: timeout or failure"
}

# ---------------------------------------------------------------------------
# Helper: run a prompt with JSON output for observe mode
# Sets globals: OBSERVE_RESPONSE (response text), OBSERVE_SKILL_READ (bool)
# ---------------------------------------------------------------------------
OBSERVE_RESPONSE=""
OBSERVE_SKILL_READ=false

run_copilot_observe() {
  local prompt="$1"
  local target_skill="$2"

  OBSERVE_RESPONSE=""
  OBSERVE_SKILL_READ=false

  local raw_json
  raw_json=$(run_copilot_prompt "$prompt" --output-format json)

  # Handle error/empty case
  if [[ -z "$raw_json" ]] || [[ "$raw_json" == ERROR:* ]]; then
    OBSERVE_RESPONSE="ERROR: timeout or failure"
    return
  fi

  # Extract response text by concatenating all message deltas
  OBSERVE_RESPONSE=$(echo "$raw_json" | jq -rs \
    '[.[] | select(.type == "assistant.message_delta") | .data.deltaContent // ""] | join("")' \
    2>/dev/null || echo "ERROR: JSON parse failure")

  # Check if the model opened the target skill's SKILL.md via the view tool
  local skill_paths
  skill_paths=$(echo "$raw_json" | jq -rs \
    '[.[] | select(.type == "tool.execution_start") | select(.data.toolName == "view") | .data.arguments.path // ""] | .[]' \
    2>/dev/null || true)

  if echo "$skill_paths" | grep -q "${target_skill}/SKILL.md"; then
    OBSERVE_SKILL_READ=true
  fi
}

# ---------------------------------------------------------------------------
# Helper: check routing activation for a single run
# Sets SINGLE_RUN_ACTIVATED=true/false
# ---------------------------------------------------------------------------
SINGLE_RUN_ACTIVATED=false

check_routing_single_run() {
  local skill="$1"
  local prompt="$2"

  SINGLE_RUN_ACTIVATED=false

  if [[ "$ROUTING_MODE" == "strict" ]]; then
    local response_with response_without
    response_with=$(run_copilot_prompt "$prompt")
    response_without=$(run_copilot_prompt "$prompt" --no-custom-instructions)

    if [[ "$response_with" != "$response_without" ]]; then
      local len_with=${#response_with}
      local len_without=${#response_without}
      local diff_chars
      diff_chars=$(diff <(echo "$response_with") <(echo "$response_without") | wc -c)
      local avg_len=$(( (len_with + len_without) / 2 ))
      if [[ $avg_len -gt 0 ]] && [[ $((diff_chars * 100 / avg_len)) -gt 20 ]]; then
        SINGLE_RUN_ACTIVATED=true
      fi
    fi
  else
    # Observe mode (default)
    run_copilot_observe "$prompt" "$skill"
    if $OBSERVE_SKILL_READ; then
      SINGLE_RUN_ACTIVATED=true
    fi
  fi
}

# ---------------------------------------------------------------------------
# Helper: run LLM-as-Judge usefulness evaluation on skill output
# Usage: run_usefulness_judge <prompt> <output> <criteria> [dimensions_json]
# Sets globals: JUDGE_SCORES (JSON), JUDGE_OVERALL (number), JUDGE_SUMMARY (text)
# ---------------------------------------------------------------------------
JUDGE_SCORES=""
JUDGE_OVERALL=0
JUDGE_SUMMARY=""

run_usefulness_judge() {
  local task_prompt="$1"
  local skill_output="$2"
  local criteria="$3"
  local dimensions_json="${4:-}"

  JUDGE_SCORES=""
  JUDGE_OVERALL=0
  JUDGE_SUMMARY="judge error"

  # Build dimension descriptions
  local dim_text=""
  if [[ -n "$dimensions_json" ]] && [[ "$dimensions_json" != "null" ]]; then
    while IFS= read -r dim; do
      case "$dim" in
        correctness)  dim_text="${dim_text}\n- **Correctness**: Is the output factually right and free of hallucination?" ;;
        completeness) dim_text="${dim_text}\n- **Completeness**: Does it cover everything the task requires?" ;;
        actionability) dim_text="${dim_text}\n- **Actionability**: Could someone act on this output without further clarification?" ;;
        conciseness)  dim_text="${dim_text}\n- **Conciseness**: Is it focused without unnecessary padding or repetition?" ;;
      esac
    done < <(echo "$dimensions_json" | jq -r '.[]' 2>/dev/null)
  fi

  # Default to all four dimensions if none specified
  if [[ -z "$dim_text" ]]; then
    dim_text="
- **Correctness**: Is the output factually right and free of hallucination?
- **Completeness**: Does it cover everything the task requires?
- **Actionability**: Could someone act on this output without further clarification?
- **Conciseness**: Is it focused without unnecessary padding or repetition?"
  fi

  local judge_prompt
  judge_prompt=$(cat <<JUDGEPROMPT
You are evaluating the usefulness of an AI agent's output.

## Task given to the agent
${task_prompt}

## Agent's output
${skill_output}

## Evaluation criteria
${criteria}

## Scoring dimensions
Rate each dimension 1-5 (1=poor, 3=adequate, 5=excellent):
${dim_text}

Respond in JSON only. No markdown fences, no explanation, just the JSON object:
{"correctness": {"score": N, "rationale": "..."}, "completeness": {"score": N, "rationale": "..."}, "actionability": {"score": N, "rationale": "..."}, "conciseness": {"score": N, "rationale": "..."}, "overall": N, "summary": "One-sentence overall assessment"}
JUDGEPROMPT
)

  local judge_model="${USEFULNESS_MODEL:-$MODEL}"
  local judge_args=(-p "$judge_prompt" --model "$judge_model" -s --no-ask-user --allow-all --autopilot --max-autopilot-continues 1)
  if [[ -n "$REASONING_EFFORT" ]]; then
    judge_args+=(--reasoning-effort "$REASONING_EFFORT")
  fi

  local raw_response
  raw_response=$(timeout "$USEFULNESS_TIMEOUT" copilot "${judge_args[@]}" 2>/dev/null || echo "")

  if [[ -z "$raw_response" ]]; then
    JUDGE_SUMMARY="judge error: empty response"
    return 1
  fi

  # Extract JSON from response — handle possible markdown fences
  local json_str
  json_str=$(echo "$raw_response" | sed -n '/^{/,/^}/p' | head -20)
  if [[ -z "$json_str" ]]; then
    # Try stripping markdown fences
    json_str=$(echo "$raw_response" | sed -n '/```/,/```/p' | grep -v '```' | head -20)
  fi
  if [[ -z "$json_str" ]]; then
    json_str="$raw_response"
  fi

  # Validate JSON structure
  if ! echo "$json_str" | jq -e '.overall' >/dev/null 2>&1; then
    JUDGE_SUMMARY="judge error: malformed JSON"
    return 1
  fi

  JUDGE_SCORES="$json_str"
  JUDGE_OVERALL=$(echo "$json_str" | jq -r '.overall // 0')
  JUDGE_SUMMARY=$(echo "$json_str" | jq -r '.summary // "no summary"')
  return 0
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
  echo "  Running ${case_count} ${test_type} cases (${RUNS} run(s) each)..."

  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    total=$((total + 1))

    local prompt expected
    prompt=$(echo "$line" | jq -r '.prompt')
    expected=$(echo "$line" | jq -r '.expected')
    local category
    category=$(echo "$line" | jq -r '.better_skill // .category // "unknown"')

    if $DRY_RUN; then
      echo "    [${total}] ${expected}: ${prompt}"
      continue
    fi

    # Multi-run with majority voting
    local votes=0
    for ((r=1; r<=RUNS; r++)); do
      check_routing_single_run "$skill" "$prompt"
      if $SINGLE_RUN_ACTIVATED; then
        votes=$((votes + 1))
      fi
    done

    local skill_activated=false
    if [[ $votes -ge $MAJORITY_THRESHOLD ]]; then
      skill_activated=true
    fi

    local test_passed=false
    if [[ "$expected" == "trigger" ]] && $skill_activated; then
      test_passed=true
    elif [[ "$expected" == "no_trigger" ]] && ! $skill_activated; then
      test_passed=true
    fi

    local vote_info=""
    if [[ $RUNS -gt 1 ]]; then
      vote_info=" (${votes}/${RUNS} votes)"
    fi

    if $test_passed; then
      pass=$((pass + 1))
      echo "    ✅ [${total}] PASS (${category}): ${prompt:0:60}...${vote_info}"
    else
      fail=$((fail + 1))
      errors+=("    ❌ [${total}] FAIL (${category}): ${prompt:0:60}... [expected=${expected}, activated=${skill_activated}${vote_info}]")
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
    if [[ $RUNS -gt 1 ]]; then
      echo "| Runs/case | ${RUNS} (majority vote) |"
    fi
    echo ""
    if [[ ${#errors[@]} -gt 0 ]]; then
      echo "### Failures"
      for err in "${errors[@]}"; do
        echo "$err"
      done
      echo ""
    fi
  } >> "$CURRENT_REPORT"

  # Expose counts for gate calculation
  if [[ "$test_type" == "positive" ]]; then
    GATE_POS_PASS=$pass
    GATE_POS_TOTAL=$total
  else
    GATE_NEG_PASS=$pass
    GATE_NEG_TOTAL=$total
  fi
}

run_behavior_tests() {
  local skill="$1"
  local jsonl_file="$2"
  local total=0
  local pass=0
  local fail=0
  local errors=()

  # Usefulness tracking
  local use_total=0
  local use_pass=0
  local use_fail=0
  local use_score_sum=0
  local use_results=()
  local use_errors=()

  if [[ ! -f "$jsonl_file" ]]; then
    echo "  ⏭  No behavior test file"
    return
  fi

  local case_count
  case_count=$(wc -l < "$jsonl_file")
  echo "  Running ${case_count} behavior cases (${RUNS} run(s) each)..."
  if $USEFULNESS; then
    echo "  Usefulness evaluation: enabled (judge model: ${USEFULNESS_MODEL:-$MODEL})"
  fi

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
      local has_criteria
      has_criteria=$(echo "$line" | jq -r '.usefulness_criteria // empty')
      if [[ -n "$has_criteria" ]]; then
        echo "           usefulness: criteria defined"
      fi
      continue
    fi

    # Multi-run with majority voting
    local run_passes=0
    local last_fail_reasons=()
    local last_response=""

    for ((r=1; r<=RUNS; r++)); do
      local response
      response=$(run_copilot_prompt "$prompt")

      local response_lines
      response_lines=$(echo "$response" | wc -l)
      local section_pass=true
      local pattern_pass=true
      local forbidden_pass=true
      local length_pass=true
      local fail_reasons=()

      # Check minimum output length (protocol compliance)
      if [[ $response_lines -lt $min_lines ]]; then
        length_pass=false
        fail_reasons+=("protocol: too short (${response_lines} < ${min_lines} lines)")
      fi

      # Check required patterns (protocol compliance)
      while IFS= read -r pattern; do
        [[ -z "$pattern" ]] && continue
        if ! echo "$response" | grep -qi "$pattern"; then
          pattern_pass=false
          fail_reasons+=("protocol: missing required: ${pattern}")
        fi
      done < <(echo "$line" | jq -r '.required_patterns // [] | .[]')

      # Check forbidden patterns (protocol compliance)
      while IFS= read -r pattern; do
        [[ -z "$pattern" ]] && continue
        if echo "$response" | grep -qi "$pattern"; then
          forbidden_pass=false
          fail_reasons+=("protocol: contains forbidden: ${pattern}")
        fi
      done < <(echo "$line" | jq -r '.forbidden_patterns // [] | .[]')

      # Check expected sections (protocol compliance)
      while IFS= read -r section; do
        [[ -z "$section" ]] && continue
        if ! echo "$response" | grep -qi "$section"; then
          section_pass=false
          fail_reasons+=("protocol: missing section: ${section}")
        fi
      done < <(echo "$line" | jq -r '.expected_sections // [] | .[]')

      if $section_pass && $pattern_pass && $forbidden_pass && $length_pass; then
        run_passes=$((run_passes + 1))
      else
        last_fail_reasons=("${fail_reasons[@]}")
      fi

      last_response="$response"
    done

    local vote_info=""
    if [[ $RUNS -gt 1 ]]; then
      vote_info=" (${run_passes}/${RUNS} passed)"
    fi

    if [[ $run_passes -ge $MAJORITY_THRESHOLD ]]; then
      pass=$((pass + 1))
      echo "    ✅ [${total}] PASS: ${prompt:0:60}...${vote_info}"
    else
      fail=$((fail + 1))
      local reason_str=""
      if [[ ${#last_fail_reasons[@]} -gt 0 ]]; then
        reason_str=$(printf '%s; ' "${last_fail_reasons[@]}")
      fi
      errors+=("    ❌ [${total}] FAIL: ${prompt:0:60}... [${reason_str}${vote_info}]")
      echo "${errors[-1]}"
    fi

    # --- Usefulness evaluation (opt-in) ---
    if $USEFULNESS; then
      local criteria
      criteria=$(echo "$line" | jq -r '.usefulness_criteria // empty')

      if [[ -n "$criteria" ]]; then
        use_total=$((use_total + 1))
        local dimensions_json
        dimensions_json=$(echo "$line" | jq -c '.usefulness_dimensions // null')
        local case_threshold
        case_threshold=$(echo "$line" | jq -r '.usefulness_threshold // empty')
        local threshold="${case_threshold:-$USEFULNESS_THRESHOLD}"

        # Multi-run judge with median score
        local judge_scores_list=()
        local best_judge_scores=""
        local best_judge_summary=""

        local judge_runs="${USEFULNESS_RUNS}"
        for ((jr=1; jr<=judge_runs; jr++)); do
          if run_usefulness_judge "$prompt" "$last_response" "$criteria" "$dimensions_json"; then
            judge_scores_list+=("$JUDGE_OVERALL")
            best_judge_scores="$JUDGE_SCORES"
            best_judge_summary="$JUDGE_SUMMARY"
          else
            judge_scores_list+=(0)
            best_judge_summary="${JUDGE_SUMMARY}"
          fi
        done

        # Compute median score
        local median_score=0
        if [[ ${#judge_scores_list[@]} -gt 0 ]]; then
          local sorted_scores
          sorted_scores=$(printf '%s\n' "${judge_scores_list[@]}" | sort -n)
          local mid_idx=$(( ${#judge_scores_list[@]} / 2 ))
          median_score=$(echo "$sorted_scores" | sed -n "$((mid_idx + 1))p")
        fi

        # Extract per-dimension scores for reporting
        local c_score="-" cm_score="-" a_score="-" cn_score="-"
        if [[ -n "$best_judge_scores" ]]; then
          c_score=$(echo "$best_judge_scores" | jq -r '.correctness.score // "-"')
          cm_score=$(echo "$best_judge_scores" | jq -r '.completeness.score // "-"')
          a_score=$(echo "$best_judge_scores" | jq -r '.actionability.score // "-"')
          cn_score=$(echo "$best_judge_scores" | jq -r '.conciseness.score // "-"')
        fi

        use_score_sum=$((use_score_sum + median_score))

        local use_verdict="✗ FAIL"
        if [[ "$median_score" -ge "$threshold" ]]; then
          use_pass=$((use_pass + 1))
          use_verdict="✓ PASS"
          echo "    🔍 [${total}] usefulness: ${median_score}/5 — PASS"
        else
          use_fail=$((use_fail + 1))
          use_errors+=("    ❌ [${total}] usefulness: ${median_score}/5 — \"${best_judge_summary}\"")
          echo "    🔍 [${total}] usefulness: ${median_score}/5 — FAIL: ${best_judge_summary}"
        fi

        use_results+=("| ${total} | ${c_score} | ${cm_score} | ${a_score} | ${cn_score} | ${median_score} | ${use_verdict} |")
      fi
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
    if [[ $RUNS -gt 1 ]]; then
      echo "| Runs/case | ${RUNS} (majority vote) |"
    fi
    echo ""
    if [[ ${#errors[@]} -gt 0 ]]; then
      echo "### Failures"
      for err in "${errors[@]}"; do
        echo "$err"
      done
      echo ""
    fi
  } >> "$CURRENT_REPORT"

  # --- Usefulness report ---
  if $USEFULNESS && [[ $use_total -gt 0 ]]; then
    echo "  usefulness results: ${use_pass}/${use_total} passed (${use_fail} failed)"

    local avg_score=0
    if [[ $use_total -gt 0 ]]; then
      avg_score=$(echo "scale=1; $use_score_sum / $use_total" | bc 2>/dev/null || echo "0")
    fi

    {
      echo "## Usefulness evaluation: ${skill}"
      echo ""
      echo "Judge model: \`${USEFULNESS_MODEL:-$MODEL}\`"
      echo "Threshold: ${USEFULNESS_THRESHOLD}/5"
      if [[ "$USEFULNESS_RUNS" -gt 1 ]]; then
        echo "Judge runs/case: ${USEFULNESS_RUNS} (median score)"
      fi
      echo ""
      echo "| Case | Correctness | Completeness | Actionability | Conciseness | Overall | Verdict |"
      echo "|------|-------------|--------------|---------------|-------------|---------|---------|"
      for row in "${use_results[@]}"; do
        echo "$row"
      done
      echo ""
      echo "Aggregate: ${avg_score} / 5.0 (threshold: ${USEFULNESS_THRESHOLD}) — $(if [[ $use_fail -eq 0 ]]; then echo PASS; else echo FAIL; fi)"
      echo ""
      if [[ ${#use_errors[@]} -gt 0 ]]; then
        echo "### Usefulness failures"
        for err in "${use_errors[@]}"; do
          echo "$err"
        done
        echo ""
      fi
      echo "Scored: ${use_total} cases ($(( total - use_total )) skipped — no usefulness_criteria)"
      echo ""
    } >> "$CURRENT_REPORT"
  elif $USEFULNESS; then
    {
      echo "## Usefulness evaluation: ${skill}"
      echo ""
      echo "No behavior test cases have \`usefulness_criteria\` defined. Skipped."
      echo ""
    } >> "$CURRENT_REPORT"
  fi

  # Expose counts for gate calculation
  GATE_BEH_PASS=$pass
  GATE_BEH_TOTAL=$total
  if $USEFULNESS && [[ $use_total -gt 0 ]]; then
    GATE_USE_SCORE_SUM=$use_score_sum
    GATE_USE_SCORE_COUNT=$use_total
  fi
}

run_gates() {
  local skill="$1"

  if $DRY_RUN; then
    return
  fi

  local pos_trigger_rate=0
  local neg_reject_rate=0
  local beh_rate=0
  local pos_trigger_status="FAIL"
  local neg_reject_status="FAIL"
  local beh_status="FAIL"
  local struct_status="FAIL"
  local struct_detail="not checked"

  # Positive trigger rate: how often the skill triggers on positive cases (TP / (TP+FN))
  if [[ $GATE_POS_TOTAL -gt 0 ]]; then
    pos_trigger_rate=$((GATE_POS_PASS * 100 / GATE_POS_TOTAL))
  fi
  [[ $pos_trigger_rate -ge 80 ]] && pos_trigger_status="PASS"

  # Negative rejection rate: how often the skill stays silent on negative cases (TN / (TN+FP))
  if [[ $GATE_NEG_TOTAL -gt 0 ]]; then
    neg_reject_rate=$((GATE_NEG_PASS * 100 / GATE_NEG_TOTAL))
  fi
  [[ $neg_reject_rate -ge 80 ]] && neg_reject_status="PASS"

  # Behavior pass rate
  if [[ $GATE_BEH_TOTAL -gt 0 ]]; then
    beh_rate=$((GATE_BEH_PASS * 100 / GATE_BEH_TOTAL))
  fi
  [[ $beh_rate -ge 80 ]] && beh_status="PASS"

  # Structural validity
  local skill_md="${REPO_ROOT}/${skill}/SKILL.md"
  if [[ -f "$skill_md" ]]; then
    local struct_json
    struct_json=$(python3 "${REPO_ROOT}/scripts/check_skill_structure.py" "$skill_md" 2>/dev/null || true)
    if [[ -n "$struct_json" ]]; then
      local valid score max_score
      valid=$(echo "$struct_json" | jq -r '.valid')
      score=$(echo "$struct_json" | jq -r '.score')
      max_score=$(echo "$struct_json" | jq -r '.max_score')
      struct_detail="${score}/${max_score}"
      [[ "$valid" == "true" ]] && struct_status="PASS"
    else
      struct_detail="checker error"
    fi
  else
    struct_detail="SKILL.md not found"
  fi

  # Overall verdict
  local verdict="PASS"
  if [[ "$pos_trigger_status" == "FAIL" ]] || [[ "$neg_reject_status" == "FAIL" ]] || \
     [[ "$beh_status" == "FAIL" ]] || [[ "$struct_status" == "FAIL" ]]; then
    verdict="FAIL"
    OVERALL_FAIL=$((OVERALL_FAIL + 1))
  fi

  # Optional usefulness gate (only when --usefulness is active and cases were scored)
  local use_status="SKIP"
  local use_detail="not enabled"
  if $USEFULNESS; then
    if [[ $GATE_USE_SCORE_COUNT -gt 0 ]]; then
      local avg_use
      avg_use=$(echo "scale=1; $GATE_USE_SCORE_SUM / $GATE_USE_SCORE_COUNT" | bc 2>/dev/null || echo "0")
      local avg_int
      avg_int=$(echo "$GATE_USE_SCORE_SUM / $GATE_USE_SCORE_COUNT" | bc 2>/dev/null || echo "0")
      use_detail="${avg_use}/5.0 (${GATE_USE_SCORE_COUNT} cases)"
      if [[ "$avg_int" -ge "$USEFULNESS_THRESHOLD" ]]; then
        use_status="PASS"
      else
        use_status="FAIL"
        verdict="FAIL"
        # Only increment if not already failed
        if [[ "$pos_trigger_status" != "FAIL" ]] && [[ "$neg_reject_status" != "FAIL" ]] && \
           [[ "$beh_status" != "FAIL" ]] && [[ "$struct_status" != "FAIL" ]]; then
          OVERALL_FAIL=$((OVERALL_FAIL + 1))
        fi
      fi
    else
      use_detail="no cases with usefulness_criteria"
    fi
  fi

  # Append gate table
  {
    echo "## Gates"
    echo ""
    echo "| Gate | Status | Detail |"
    echo "|------|--------|--------|"
    echo "| Positive trigger rate ≥ 80% | ${pos_trigger_status} | ${pos_trigger_rate}% |"
    echo "| Negative rejection rate ≥ 80% | ${neg_reject_status} | ${neg_reject_rate}% |"
    echo "| Behavior pass rate ≥ 80% | ${beh_status} | ${beh_rate}% |"
    echo "| Structural validity | ${struct_status} | ${struct_detail} |"
    if $USEFULNESS; then
      echo "| Usefulness ≥ ${USEFULNESS_THRESHOLD}/5 | ${use_status} | ${use_detail} |"
    fi
    echo ""
    echo "## Verdict: ${verdict}"
    echo ""
  } >> "$CURRENT_REPORT"

  echo "  Gate verdict: ${verdict}"
}

# Main loop
echo "═══════════════════════════════════════════"
echo "  Meta-Skill Eval Runner"
echo "  Model: ${MODEL}"
echo "  Routing: ${ROUTING_MODE}"
echo "  Runs/prompt: ${RUNS}"
echo "  Reasoning: ${REASONING_EFFORT:-model default}"
echo "  Usefulness: $(if $USEFULNESS; then echo "enabled (judge: ${USEFULNESS_MODEL:-$MODEL})"; else echo 'disabled'; fi)"
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

  # Set up timestamped report file
  CURRENT_REPORT="${RESULTS_DIR}/${skill}-${TIMESTAMP}.md"
  > "$CURRENT_REPORT"
  echo "# Eval Results: ${skill}" >> "$CURRENT_REPORT"
  echo "Date: $(date -Iseconds)" >> "$CURRENT_REPORT"
  echo "Model: ${MODEL}" >> "$CURRENT_REPORT"
  echo "Routing: ${ROUTING_MODE}" >> "$CURRENT_REPORT"
  echo "Runs/prompt: ${RUNS}" >> "$CURRENT_REPORT"
  if $USEFULNESS; then
    echo "Usefulness: enabled (judge: ${USEFULNESS_MODEL:-$MODEL}, threshold: ${USEFULNESS_THRESHOLD})" >> "$CURRENT_REPORT"
  fi
  echo "" >> "$CURRENT_REPORT"

  # Reset gate tracking
  GATE_POS_PASS=0; GATE_POS_TOTAL=0
  GATE_NEG_PASS=0; GATE_NEG_TOTAL=0
  GATE_BEH_PASS=0; GATE_BEH_TOTAL=0
  GATE_USE_SCORE_SUM=0; GATE_USE_SCORE_COUNT=0

  run_trigger_tests "$skill" "$skill_dir/evals/trigger-positive.jsonl" "positive"
  run_trigger_tests "$skill" "$skill_dir/evals/trigger-negative.jsonl" "negative"
  run_behavior_tests "$skill" "$skill_dir/evals/behavior.jsonl"

  run_gates "$skill"

  # Symlink to latest
  ln -sf "${skill}-${TIMESTAMP}.md" "${RESULTS_DIR}/${skill}-eval.md"

  echo ""
  echo "  Results saved: eval-results/${skill}-${TIMESTAMP}.md"
  echo "  (symlinked from eval-results/${skill}-eval.md)"
  echo ""
done

echo "═══════════════════════════════════════════"
echo "  All eval results in: ${RESULTS_DIR}/"
if [[ $OVERALL_FAIL -gt 0 ]]; then
  echo "  ❌ OVERALL: FAIL (${OVERALL_FAIL} skill(s) failed gates)"
else
  echo "  ✅ OVERALL: PASS"
fi
echo "═══════════════════════════════════════════"

# JSON summary output (for run-baseline-comparison.sh and other tooling)
if $JSON_OUTPUT; then
  echo "{"
  echo "  \"timestamp\": \"${TIMESTAMP}\","
  echo "  \"model\": \"${MODEL}\","
  echo "  \"routing_mode\": \"${ROUTING_MODE}\","
  echo "  \"runs_per_prompt\": ${RUNS},"
  echo "  \"overall\": \"$(if [[ $OVERALL_FAIL -gt 0 ]]; then echo FAIL; else echo PASS; fi)\","
  echo "  \"skills_failed\": ${OVERALL_FAIL},"
  echo "  \"skills_tested\": ${#TARGETS[@]},"
  echo "  \"results_dir\": \"${RESULTS_DIR}\""
  echo "}"
fi

exit $(( OVERALL_FAIL > 0 ? 1 : 0 ))
