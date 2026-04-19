#!/usr/bin/env bash
# opencode-eval.sh — OpenCode SDK-based evaluation runner
#
# Usage:
#   ./scripts/opencode-eval.sh [skill-name]       # Run evals for one skill
#   ./scripts/opencode-eval.sh --all              # Run evals for all skills with evals/
#   ./scripts/opencode-eval.sh --dry-run [skill]  # Show test cases without running
#
# Requires: OpenCode server running, @opencode-ai/sdk, node, jq
#
# Model: minimax-coding-plan/Minimax-M2.7 (configurable via EVAL_MODEL env var)
#
# This script replaces the copilot CLI-based run-evals.sh with OpenCode SDK calls.

set -euo pipefail

# Auto-detect repo root
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root"; exit 1; }

RESULTS_DIR="${REPO_ROOT}/eval-results"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
DRY_RUN=false
TARGETS=()
MODEL="${EVAL_MODEL:-minimax-coding-plan/Minimax-M2.7}"
TIMEOUT="${EVAL_TIMEOUT:-120}"
ROUTING_MODE="${EVAL_ROUTING:-observe}"
RUNS="${EVAL_RUNS:-1}"
USEFULNESS=false
OPENCODE_SERVER="${OPENCODE_SERVER:-http://127.0.0.1:4096}"

# Gate tracking
GATE_POS_PASS=0; GATE_POS_TOTAL=0
GATE_NEG_PASS=0; GATE_NEG_TOTAL=0
GATE_BEH_PASS=0; GATE_BEH_TOTAL=0
CURRENT_REPORT=""
OVERALL_FAIL=0

usage() {
  echo "Usage: $0 [--all | --dry-run | --observe | --runs N] [skill-name ...]"
  echo ""
  echo "Options:"
  echo "  --all         Run evals for all skills with evals/ directories"
  echo "  --dry-run     List test cases without executing"
  echo "  --observe     Use OpenCode session observation (default)"
  echo "  --runs N      Run each prompt N times (default: 1)"
  echo "  --usefulness  Enable usefulness scoring"
  echo ""
  echo "Environment variables:"
  echo "  EVAL_MODEL              Model to use (default: minimax-coding-plan/Minimax-M2.7)"
  echo "  EVAL_TIMEOUT           Seconds per prompt (default: 120)"
  echo "  OPENCODE_SERVER        OpenCode server URL (default: http://127.0.0.1:4096)"
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
    --dry-run) DRY_RUN=true; shift ;;
    --observe) ROUTING_MODE="observe"; shift ;;
    --usefulness) USEFULNESS=true; shift ;;
    --runs)
      RUNS="$2"; shift 2 ;;
    --help|-h) usage ;;
    *) TARGETS+=("$1"); shift ;;
  esac
done

[[ ${#TARGETS[@]} -eq 0 ]] && usage

# Check dependencies
command -v node >/dev/null 2>&1 || { echo "Error: node is required"; exit 1; }
command -v jq >/dev/null 2>&1 || { echo "Error: jq is required"; exit 1; }

mkdir -p "$RESULTS_DIR"

MAJORITY_THRESHOLD=$(( (RUNS + 1) / 2 ))

# ---------------------------------------------------------------------------
# Helper: Run a prompt via OpenCode SDK (Node.js)
# ---------------------------------------------------------------------------
run_opencode_prompt() {
  local prompt="$1"
  local skill="$2"
  local output_json="${3:-false}"

  node --input-type=module <<EOF
import { createOpencodeClient } from "@opencode-ai/sdk";

const client = createOpencodeClient({ baseUrl: "${OPENCODE_SERVER}" });

async function main() {
  try {
    // Create a new session for this eval
    const session = await client.session.create({
      body: { title: "eval-${skill}" }
    });

    const args = [];
    if ("${output_json}" === "true") {
      // For observe mode - get JSON output
      const result = await client.session.prompt({
        path: { id: session.id },
        body: {
          parts: [{ type: "text", text: ${prompt@Q} }],
          model: { providerID: "minimax", modelID: "MiniMax-M2.7" },
          outputFormat: "json"
        }
      });
      console.log(JSON.stringify(result.data));
    } else {
      // Standard text output
      const result = await client.session.prompt({
        path: { id: session.id },
        body: {
          parts: [{ type: "text", text: ${prompt@Q} }],
          model: { providerID: "minimax", modelID: "MiniMax-M2.7" }
        }
      });
      console.log(result.data.message?.content || JSON.stringify(result.data));
    }

    // Cleanup session
    await client.session.delete({ path: { id: session.id } });
  } catch (error) {
    console.error("ERROR:", error.message);
    process.exit(1);
  }
}

main();
EOF
}

# ---------------------------------------------------------------------------
# Helper: Check if model opened target skill's SKILL.md via view tool
# ---------------------------------------------------------------------------
check_skill_read() {
  local response_json="$1"
  local target_skill="$2"

  echo "$response_json" | jq -rs \
    '[.[] | select(.type == "tool.execution_start") | select(.data.toolName == "view") | .data.arguments.path // ""] | .[]' \
    2>/dev/null | grep -q "${target_skill}/SKILL.md" && echo "true" || echo "false"
}

# ---------------------------------------------------------------------------
# Run trigger tests
# ---------------------------------------------------------------------------
run_trigger_tests() {
  local skill="$1"
  local jsonl_file="$2"
  local test_type="$3"
  local total=0 pass=0 fail=0

  [[ ! -f "$jsonl_file" ]] && return

  local case_count
  case_count=$(wc -l < "$jsonl_file")
  echo "  Running ${case_count} ${test_type} cases (${RUNS} run(s) each)..."

  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    total=$((total + 1))

    local prompt expected
    prompt=$(echo "$line" | jq -r '.prompt')
    expected=$(echo "$line" | jq -r '.expected')

    if $DRY_RUN; then
      echo "    [${total}] ${expected}: ${prompt:0:60}..."
      continue
    fi

    local votes=0
    for ((r=1; r<=RUNS; r++)); do
      local raw_json
      raw_json=$(run_opencode_prompt "$prompt" "$skill" "true")

      if [[ "$ROUTING_MODE" == "observe" ]]; then
        local skill_read
        skill_read=$(check_skill_read "$raw_json" "$skill")
        [[ "$skill_read" == "true" ]] && votes=$((votes + 1))
      fi
    done

    local activated=false
    [[ $votes -ge $MAJORITY_THRESHOLD ]] && activated=true

    local test_passed=false
    if [[ "$expected" == "trigger" ]] && $activated; then
      test_passed=true; pass=$((pass + 1))
    elif [[ "$expected" == "no_trigger" ]] && ! $activated; then
      test_passed=true; pass=$((pass + 1))
    else
      fail=$((fail + 1))
    fi

    local vote_info=""
    [[ $RUNS -gt 1 ]] && vote_info=" (${votes}/${RUNS} votes)"

    if $test_passed; then
      echo "    ✅ [${total}] PASS: ${prompt:0:60}...${vote_info}"
    else
      echo "    ❌ [${total}] FAIL: ${prompt:0:60}... [expected=${expected}]${vote_info}"
    fi
  done < "$jsonl_file"

  echo ""
  echo "  ${test_type} results: ${pass}/${total} passed (${fail} failed)"

  [[ "$test_type" == "positive" ]] && { GATE_POS_PASS=$pass; GATE_POS_TOTAL=$total; }
  [[ "$test_type" == "negative" ]] && { GATE_NEG_PASS=$pass; GATE_NEG_TOTAL=$total; }
}

# ---------------------------------------------------------------------------
# Run behavior tests
# ---------------------------------------------------------------------------
run_behavior_tests() {
  local skill="$1"
  local jsonl_file="$2"
  local total=0 pass=0 fail=0

  [[ ! -f "$jsonl_file" ]] && return

  local case_count
  case_count=$(wc -l < "$jsonl_file")
  echo "  Running ${case_count} behavior cases..."

  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    total=$((total + 1))

    local prompt min_lines
    prompt=$(echo "$line" | jq -r '.prompt')
    min_lines=$(echo "$line" | jq -r '.min_output_lines // 10')

    if $DRY_RUN; then
      echo "    [${total}] behavior: ${prompt:0:60}..."
      continue
    fi

    local response
    response=$(run_opencode_prompt "$prompt" "$skill" "false")

    local response_lines
    response_lines=$(echo "$response" | wc -l)
    local length_pass=true
    [[ $response_lines -lt $min_lines ]] && length_pass=false

    local pattern_pass=true
    while IFS= read -r pattern; do
      [[ -z "$pattern" ]] && continue
      if ! echo "$response" | grep -qi "$pattern"; then
        pattern_pass=false
      fi
    done < <(echo "$line" | jq -r '.required_patterns // [] | .[]')

    local section_pass=true
    while IFS= read -r section; do
      [[ -z "$section" ]] && continue
      if ! echo "$response" | grep -qi "$section"; then
        section_pass=false
      fi
    done < <(echo "$line" | jq -r '.expected_sections // [] | .[]')

    if $section_pass && $pattern_pass && $length_pass; then
      pass=$((pass + 1))
      echo "    ✅ [${total}] PASS: ${prompt:0:60}..."
    else
      fail=$((fail + 1))
      echo "    ❌ [${total}] FAIL: ${prompt:0:60}..."
    fi
  done < "$jsonl_file"

  echo ""
  echo "  behavior results: ${pass}/${total} passed (${fail} failed)"

  GATE_BEH_PASS=$pass; GATE_BEH_TOTAL=$total
}

# ---------------------------------------------------------------------------
# Run gates and report
# ---------------------------------------------------------------------------
run_gates() {
  local skill="$1"

  local pos_rate=0 neg_rate=0 beh_rate=0
  [[ $GATE_POS_TOTAL -gt 0 ]] && pos_rate=$((GATE_POS_PASS * 100 / GATE_POS_TOTAL))
  [[ $GATE_NEG_TOTAL -gt 0 ]] && neg_rate=$((GATE_NEG_PASS * 100 / GATE_NEG_TOTAL))
  [[ $GATE_BEH_TOTAL -gt 0 ]] && beh_rate=$((GATE_BEH_PASS * 100 / GATE_BEH_TOTAL))

  local verdict="PASS"
  [[ $pos_rate -lt 80 ]] || [[ $neg_rate -lt 80 ]] || [[ $beh_rate -lt 80 ]] && { verdict="FAIL"; OVERALL_FAIL=$((OVERALL_FAIL + 1)); }

  echo ""
  echo "## Gates"
  echo ""
  echo "| Gate | Threshold | Actual | Status |"
  echo "|------|-----------|--------|--------|"
  echo "| Positive trigger | >= 80% | ${pos_rate}% | $([[ $pos_rate -ge 80 ]] && echo PASS || echo FAIL) |"
  echo "| Negative rejection | >= 80% | ${neg_rate}% | $([[ $neg_rate -ge 80 ]] && echo PASS || echo FAIL) |"
  echo "| Behavior | >= 80% | ${beh_rate}% | $([[ $beh_rate -ge 80 ]] && echo PASS || echo FAIL) |"
  echo ""
  echo "## Verdict: ${verdict}"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
echo "═══════════════════════════════════════════"
echo "  Meta-Skill Eval Runner (OpenCode SDK)"
echo "  Model: ${MODEL}"
echo "  Routing: ${ROUTING_MODE}"
echo "  Runs/prompt: ${RUNS}"
echo "  Server: ${OPENCODE_SERVER}"
echo "═══════════════════════════════════════════"
echo ""

for skill in "${TARGETS[@]}"; do
  skill_dir="${REPO_ROOT}/${skill}"

  [[ ! -d "$skill_dir" ]] && { echo "⚠️  Skill not found: ${skill}"; continue; }
  [[ ! -d "$skill_dir/evals" ]] && { echo "⚠️  No evals/: ${skill}"; continue; }

  echo "━━━ ${skill} ━━━"

  CURRENT_REPORT="${RESULTS_DIR}/${skill}-${TIMESTAMP}.md"
  echo "# Eval Results: ${skill}" > "$CURRENT_REPORT"
  echo "Date: $(date -Iseconds)" >> "$CURRENT_REPORT"
  echo "Model: ${MODEL}" >> "$CURRENT_REPORT"
  echo "Routing: ${ROUTING_MODE}" >> "$CURRENT_REPORT"
  echo "Runs/prompt: ${RUNS}" >> "$CURRENT_REPORT"
  echo "" >> "$CURRENT_REPORT"

  GATE_POS_PASS=0; GATE_POS_TOTAL=0
  GATE_NEG_PASS=0; GATE_NEG_TOTAL=0
  GATE_BEH_PASS=0; GATE_BEH_TOTAL=0

  run_trigger_tests "$skill" "$skill_dir/evals/trigger-positive.jsonl" "positive"
  run_trigger_tests "$skill" "$skill_dir/evals/trigger-negative.jsonl" "negative"
  run_behavior_tests "$skill" "$skill_dir/evals/behavior.jsonl"

  run_gates "$skill" >> "$CURRENT_REPORT"

  ln -sf "${skill}-${TIMESTAMP}.md" "${RESULTS_DIR}/${skill}-eval.md"

  echo ""
  echo "  Results: eval-results/${skill}-${TIMESTAMP}.md"
done

echo ""
echo "═══════════════════════════════════════════"
[[ $OVERALL_FAIL -gt 0 ]] && echo "❌ OVERALL: FAIL" || echo "✅ OVERALL: PASS"
echo "═══════════════════════════════════════════"

exit $(( OVERALL_FAIL > 0 ? 1 : 0 ))
