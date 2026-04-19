#!/usr/bin/env bash
# run-trigger-optimization.sh — Automated trigger optimization with train/test split
#
# Usage:
#   ./scripts/run-trigger-optimization.sh <skill-name>
#   ./scripts/run-trigger-optimization.sh --dry-run <skill-name>
#
# Requires: copilot CLI, jq
#
# This script automates the skill-trigger-optimization workflow using proper
# ML evaluation methodology:
#
#   1. SPLIT:    60/40 train/test split of trigger eval cases
#   2. BASELINE: Evaluate current description on train set (3 runs each)
#   3. ANALYZE:  Collect failures and build improvement context
#   4. PROPOSE:  Use LLM to generate improved description
#   5. EVALUATE: Re-evaluate improved description on train set
#   6. VALIDATE: If train score improves, evaluate on held-out test set
#   7. REPORT:   Compare before/after on both train and test sets
#
# The script does NOT auto-apply changes. It outputs a proposed description
# and a comparison report for human review.

set -euo pipefail

# Auto-detect repo root
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root (no AGENTS.md found)"; exit 1; }
RESULTS_DIR="${REPO_ROOT}/eval-results"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
MODEL="${EVAL_MODEL:-gpt-4.1}"
TIMEOUT="${EVAL_TIMEOUT:-60}"
REASONING_EFFORT="${EVAL_REASONING_EFFORT:-}"
RUNS_PER_PROMPT=3
DRY_RUN=false
SKILL=""

usage() {
  echo "Usage: $0 [--dry-run] [--model X] [--runs N] <skill-name>"
  echo ""
  echo "Automated trigger optimization with 60/40 train/test split."
  echo "Runs each prompt multiple times for variance reduction."
  echo "Proposes improved description, validates on held-out test set."
  echo ""
  echo "Options:"
  echo "  --dry-run   Show split and cases without running"
  echo "  --model X   Override model (default: gpt-4.1)"
  echo "  --runs N    Runs per prompt for variance (default: 3)"
  echo "  --timeout N Seconds per prompt (default: 60)"
  exit 1
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run) DRY_RUN=true; shift ;;
    --model) MODEL="$2"; shift 2 ;;
    --runs) RUNS_PER_PROMPT="$2"; shift 2 ;;
    --timeout) TIMEOUT="$2"; shift 2 ;;
    --help|-h) usage ;;
    *) SKILL="$1"; shift ;;
  esac
done

if [[ -z "$SKILL" ]]; then
  echo "Error: specify a skill name"
  usage
fi

SKILL_DIR="${REPO_ROOT}/${SKILL}"
if [[ ! -d "$SKILL_DIR" ]]; then
  echo "Error: skill directory not found: ${SKILL}"
  exit 1
fi

SKILL_MD="${SKILL_DIR}/SKILL.md"
if [[ ! -f "$SKILL_MD" ]]; then
  echo "Error: SKILL.md not found in ${SKILL_DIR}"
  exit 1
fi

POS_FILE="${SKILL_DIR}/evals/trigger-positive.jsonl"
NEG_FILE="${SKILL_DIR}/evals/trigger-negative.jsonl"

if [[ ! -f "$POS_FILE" ]] || [[ ! -f "$NEG_FILE" ]]; then
  echo "Error: trigger eval files not found in ${SKILL_DIR}/evals/"
  exit 1
fi

command -v jq >/dev/null 2>&1 || { echo "Error: jq is required"; exit 1; }

mkdir -p "$RESULTS_DIR"
REPORT="${RESULTS_DIR}/optimization-${SKILL}-${TIMESTAMP}.md"
TMPDIR=$(mktemp -d)
trap '
  # Restore SKILL.md from backup if it exists (safety net for interrupts)
  if [[ -f "$TMPDIR/SKILL.md.backup" ]] && [[ -f "$SKILL_MD" ]]; then
    cp "$TMPDIR/SKILL.md.backup" "$SKILL_MD" 2>/dev/null || true
  fi
  rm -rf "$TMPDIR"
' EXIT

MAJORITY_THRESHOLD=$(( (RUNS_PER_PROMPT + 1) / 2 ))

# ---------------------------------------------------------------------------
# Step 1: Split eval data 60/40 train/test (interleaved for category diversity)
# ---------------------------------------------------------------------------
split_data() {
  local input="$1"
  local train_out="$2"
  local test_out="$3"

  > "$train_out"
  > "$test_out"

  local i=0
  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    i=$((i + 1))
    # Every 5th line starting at 3 and 5 go to test (lines 3,5,8,10,13,15...)
    # This gives ~40% test with interleaving
    if [[ $((i % 5)) -eq 3 ]] || [[ $((i % 5)) -eq 0 ]]; then
      echo "$line" >> "$test_out"
    else
      echo "$line" >> "$train_out"
    fi
  done < "$input"
}

echo "═══════════════════════════════════════════"
echo "  Trigger Optimization: ${SKILL}"
echo "  Model: ${MODEL}"
echo "  Runs/prompt: ${RUNS_PER_PROMPT}"
echo "═══════════════════════════════════════════"
echo ""

# Split positive and negative files
split_data "$POS_FILE" "$TMPDIR/train-pos.jsonl" "$TMPDIR/test-pos.jsonl"
split_data "$NEG_FILE" "$TMPDIR/train-neg.jsonl" "$TMPDIR/test-neg.jsonl"

TRAIN_POS=$(wc -l < "$TMPDIR/train-pos.jsonl")
TRAIN_NEG=$(wc -l < "$TMPDIR/train-neg.jsonl")
TEST_POS=$(wc -l < "$TMPDIR/test-pos.jsonl")
TEST_NEG=$(wc -l < "$TMPDIR/test-neg.jsonl")

echo "Step 1: Data split"
echo "  Train: ${TRAIN_POS} positive + ${TRAIN_NEG} negative = $((TRAIN_POS + TRAIN_NEG)) cases"
echo "  Test:  ${TEST_POS} positive + ${TEST_NEG} negative = $((TEST_POS + TEST_NEG)) cases (held out)"
echo ""

# Start report
{
  echo "# Trigger Optimization Report: ${SKILL}"
  echo "Date: $(date -Iseconds)"
  echo "Model: ${MODEL}"
  echo "Runs/prompt: ${RUNS_PER_PROMPT}"
  echo ""
  echo "## Data Split"
  echo "| Set | Positive | Negative | Total |"
  echo "|-----|----------|----------|-------|"
  echo "| Train | ${TRAIN_POS} | ${TRAIN_NEG} | $((TRAIN_POS + TRAIN_NEG)) |"
  echo "| Test | ${TEST_POS} | ${TEST_NEG} | $((TEST_POS + TEST_NEG)) |"
  echo ""
} > "$REPORT"

if $DRY_RUN; then
  echo "Train positive cases:"
  cat "$TMPDIR/train-pos.jsonl" | jq -r '.prompt' | nl
  echo ""
  echo "Train negative cases:"
  cat "$TMPDIR/train-neg.jsonl" | jq -r '.prompt' | nl
  echo ""
  echo "Test positive cases (held out):"
  cat "$TMPDIR/test-pos.jsonl" | jq -r '.prompt' | nl
  echo ""
  echo "Test negative cases (held out):"
  cat "$TMPDIR/test-neg.jsonl" | jq -r '.prompt' | nl
  echo ""
  echo "(dry run — no LLM calls made)"
  exit 0
fi

# ---------------------------------------------------------------------------
# Helper: copilot call with reliability flags
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
# Helper: observe-mode routing check
# Sets OBSERVE_SKILL_READ=true/false
# ---------------------------------------------------------------------------
OBSERVE_SKILL_READ=false

run_observe_check() {
  local prompt="$1"
  local target_skill="$2"

  OBSERVE_SKILL_READ=false

  local raw_json
  raw_json=$(run_copilot_prompt "$prompt" --output-format json)

  if [[ -z "$raw_json" ]] || [[ "$raw_json" == ERROR:* ]]; then
    return
  fi

  local skill_paths
  skill_paths=$(echo "$raw_json" | jq -rs \
    '[.[] | select(.type == "tool.execution_start") | select(.data.toolName == "view") | .data.arguments.path // ""] | .[]' \
    2>/dev/null || true)

  if echo "$skill_paths" | grep -q "${target_skill}/SKILL.md"; then
    OBSERVE_SKILL_READ=true
  fi
}

# ---------------------------------------------------------------------------
# Helper: evaluate a JSONL file and return pass count
# Usage: eval_trigger_file <jsonl_file> <expected> <skill>
# Prints "pass_count/total" to stdout
# ---------------------------------------------------------------------------
eval_trigger_file() {
  local jsonl_file="$1"
  local test_type="$2"  # "positive" or "negative"
  local skill="$3"

  local total=0
  local pass=0
  local failures=""

  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    total=$((total + 1))

    local prompt expected
    prompt=$(echo "$line" | jq -r '.prompt')
    expected=$(echo "$line" | jq -r '.expected')

    # Multi-run majority voting
    local votes=0
    for ((r=1; r<=RUNS_PER_PROMPT; r++)); do
      run_observe_check "$prompt" "$skill"
      if $OBSERVE_SKILL_READ; then
        votes=$((votes + 1))
      fi
    done

    local activated=false
    if [[ $votes -ge $MAJORITY_THRESHOLD ]]; then
      activated=true
    fi

    local test_passed=false
    if [[ "$expected" == "trigger" ]] && $activated; then
      test_passed=true
    elif [[ "$expected" == "no_trigger" ]] && ! $activated; then
      test_passed=true
    fi

    if $test_passed; then
      pass=$((pass + 1))
      echo "    ✅ PASS: ${prompt:0:60}... (${votes}/${RUNS_PER_PROMPT})" >&2
    else
      echo "    ❌ FAIL: ${prompt:0:60}... [expected=${expected}, activated=${activated}, ${votes}/${RUNS_PER_PROMPT}]" >&2
      failures="${failures}\n- FAIL: expected=${expected}, got activated=${activated} (${votes}/${RUNS_PER_PROMPT} votes): ${prompt}"
    fi
  done < "$jsonl_file"

  # Return results as structured output
  echo "${pass}|${total}|${failures}"
}

# ---------------------------------------------------------------------------
# Step 2: Baseline evaluation on train set
# ---------------------------------------------------------------------------
echo "Step 2: Baseline evaluation on train set"
echo "  Running positive trigger cases..."
BASELINE_POS_RESULT=$(eval_trigger_file "$TMPDIR/train-pos.jsonl" "positive" "$SKILL")
BASELINE_POS_PASS=$(echo "$BASELINE_POS_RESULT" | cut -d'|' -f1)
BASELINE_POS_TOTAL=$(echo "$BASELINE_POS_RESULT" | cut -d'|' -f2)
BASELINE_POS_FAILURES=$(echo -e "$(echo "$BASELINE_POS_RESULT" | cut -d'|' -f3)")

echo "  Running negative trigger cases..."
BASELINE_NEG_RESULT=$(eval_trigger_file "$TMPDIR/train-neg.jsonl" "negative" "$SKILL")
BASELINE_NEG_PASS=$(echo "$BASELINE_NEG_RESULT" | cut -d'|' -f1)
BASELINE_NEG_TOTAL=$(echo "$BASELINE_NEG_RESULT" | cut -d'|' -f2)
BASELINE_NEG_FAILURES=$(echo -e "$(echo "$BASELINE_NEG_RESULT" | cut -d'|' -f3)")

BASELINE_TRAIN_PASS=$((BASELINE_POS_PASS + BASELINE_NEG_PASS))
BASELINE_TRAIN_TOTAL=$((BASELINE_POS_TOTAL + BASELINE_NEG_TOTAL))

echo ""
echo "  Baseline train score: ${BASELINE_TRAIN_PASS}/${BASELINE_TRAIN_TOTAL}"
echo ""

# Extract current description from SKILL.md frontmatter
CURRENT_DESC=$(sed -n '/^---$/,/^---$/p' "$SKILL_MD" | grep -A 100 'description:' | tail -n +1 | sed '/^---$/d' | sed '/^name:/d' | sed 's/^  //' | tr '\n' ' ' | sed 's/description: *>- *//' | sed 's/description: *//' | xargs)

{
  echo "## Baseline (Train Set)"
  echo ""
  echo "Current description:"
  echo '```'
  echo "$CURRENT_DESC"
  echo '```'
  echo ""
  echo "| Metric | Positive | Negative | Combined |"
  echo "|--------|----------|----------|----------|"
  echo "| Passed | ${BASELINE_POS_PASS}/${BASELINE_POS_TOTAL} | ${BASELINE_NEG_PASS}/${BASELINE_NEG_TOTAL} | ${BASELINE_TRAIN_PASS}/${BASELINE_TRAIN_TOTAL} |"
  echo ""
  if [[ -n "$BASELINE_POS_FAILURES" ]] || [[ -n "$BASELINE_NEG_FAILURES" ]]; then
    echo "### Train Failures"
    echo "$BASELINE_POS_FAILURES"
    echo "$BASELINE_NEG_FAILURES"
    echo ""
  fi
} >> "$REPORT"

# Check if baseline is already perfect
if [[ $BASELINE_TRAIN_PASS -eq $BASELINE_TRAIN_TOTAL ]]; then
  echo "Baseline is already perfect on train set. No optimization needed."
  echo "## Result: No optimization needed (baseline perfect)" >> "$REPORT"
  echo ""
  echo "Report: ${REPORT}"
  exit 0
fi

# ---------------------------------------------------------------------------
# Step 3-4: Analyze failures and propose improved description
# ---------------------------------------------------------------------------
echo "Step 3-4: Analyzing failures and proposing improved description..."

ALL_FAILURES="${BASELINE_POS_FAILURES}${BASELINE_NEG_FAILURES}"

OPTIMIZATION_PROMPT="You are optimizing the trigger description for the skill '${SKILL}'.

Current description:
${CURRENT_DESC}

Train set failures:
${ALL_FAILURES}

The description field determines when this skill is activated. It should:
- Start with a verb + specific object (most discriminating signal)
- Include 2-3 realistic trigger phrases users actually say
- Include what the skill produces
- End with 'Do not use for...' anti-triggers naming alternatives
- Exclude generic filler and marketing language

Analyze the failures above. For each failure:
- If expected=trigger but activated=false: the description is missing trigger words that match the prompt
- If expected=no_trigger but activated=true: the description is too broad and matches prompts it shouldn't

Write ONLY the improved description text (the YAML description value). No explanation, no markdown, just the raw description text."

PROPOSED_DESC=$(run_copilot_prompt "$OPTIMIZATION_PROMPT")

echo "  Proposed description received."
echo ""

{
  echo "## Proposed Description"
  echo ""
  echo '```'
  echo "$PROPOSED_DESC"
  echo '```'
  echo ""
} >> "$REPORT"

# ---------------------------------------------------------------------------
# Step 5: Evaluate improved description on train set
# ---------------------------------------------------------------------------
echo "Step 5: Evaluating improved description on train set..."

# Temporarily patch the SKILL.md with the new description
cp "$SKILL_MD" "$TMPDIR/SKILL.md.backup"

# Replace the description in frontmatter
python3 -c "
import re, sys

with open('$SKILL_MD', 'r') as f:
    content = f.read()

# Find frontmatter
fm_match = re.match(r'^---\n(.*?)\n---', content, re.DOTALL)
if not fm_match:
    sys.exit('No frontmatter found')

fm = fm_match.group(1)
new_desc = '''$(echo "$PROPOSED_DESC" | sed "s/'/\\\\'/g")'''

# Replace description (handles multi-line >- format)
new_fm = re.sub(
    r'description:.*?(?=\n[a-z]|\n---)',
    'description: >-\n  ' + new_desc.strip().replace('\n', '\n  '),
    fm + '\n',
    flags=re.DOTALL
).rstrip()

new_content = '---\n' + new_fm + '\n---' + content[fm_match.end():]
with open('$SKILL_MD', 'w') as f:
    f.write(new_content)
" 2>/dev/null || {
  echo "Warning: failed to patch SKILL.md, skipping improved eval"
  cp "$TMPDIR/SKILL.md.backup" "$SKILL_MD"
  echo "## Result: Failed to patch SKILL.md for evaluation" >> "$REPORT"
  echo "Report: ${REPORT}"
  exit 1
}

echo "  Running positive trigger cases..."
IMPROVED_POS_RESULT=$(eval_trigger_file "$TMPDIR/train-pos.jsonl" "positive" "$SKILL")
IMPROVED_POS_PASS=$(echo "$IMPROVED_POS_RESULT" | cut -d'|' -f1)
IMPROVED_POS_TOTAL=$(echo "$IMPROVED_POS_RESULT" | cut -d'|' -f2)

echo "  Running negative trigger cases..."
IMPROVED_NEG_RESULT=$(eval_trigger_file "$TMPDIR/train-neg.jsonl" "negative" "$SKILL")
IMPROVED_NEG_PASS=$(echo "$IMPROVED_NEG_RESULT" | cut -d'|' -f1)
IMPROVED_NEG_TOTAL=$(echo "$IMPROVED_NEG_RESULT" | cut -d'|' -f2)

IMPROVED_TRAIN_PASS=$((IMPROVED_POS_PASS + IMPROVED_NEG_PASS))
IMPROVED_TRAIN_TOTAL=$((IMPROVED_POS_TOTAL + IMPROVED_NEG_TOTAL))

echo ""
echo "  Improved train score: ${IMPROVED_TRAIN_PASS}/${IMPROVED_TRAIN_TOTAL} (baseline: ${BASELINE_TRAIN_PASS}/${BASELINE_TRAIN_TOTAL})"
echo ""

{
  echo "## Improved Description (Train Set)"
  echo ""
  echo "| Metric | Baseline | Improved |"
  echo "|--------|----------|----------|"
  echo "| Positive | ${BASELINE_POS_PASS}/${BASELINE_POS_TOTAL} | ${IMPROVED_POS_PASS}/${IMPROVED_POS_TOTAL} |"
  echo "| Negative | ${BASELINE_NEG_PASS}/${BASELINE_NEG_TOTAL} | ${IMPROVED_NEG_PASS}/${IMPROVED_NEG_TOTAL} |"
  echo "| Combined | ${BASELINE_TRAIN_PASS}/${BASELINE_TRAIN_TOTAL} | ${IMPROVED_TRAIN_PASS}/${IMPROVED_TRAIN_TOTAL} |"
  echo ""
} >> "$REPORT"

# Check if improved on train set
if [[ $IMPROVED_TRAIN_PASS -le $BASELINE_TRAIN_PASS ]]; then
  echo "  ⚠️  No improvement on train set. Restoring original."
  cp "$TMPDIR/SKILL.md.backup" "$SKILL_MD"
  echo "## Result: No improvement on train set — original preserved" >> "$REPORT"
  echo ""
  echo "Report: ${REPORT}"
  exit 0
fi

# ---------------------------------------------------------------------------
# Step 6: Validate on held-out test set
# ---------------------------------------------------------------------------
echo "Step 6: Validating on held-out test set..."

echo "  Running positive trigger cases (test set)..."
TEST_POS_RESULT=$(eval_trigger_file "$TMPDIR/test-pos.jsonl" "positive" "$SKILL")
TEST_IMPROVED_POS_PASS=$(echo "$TEST_POS_RESULT" | cut -d'|' -f1)
TEST_IMPROVED_POS_TOTAL=$(echo "$TEST_POS_RESULT" | cut -d'|' -f2)

echo "  Running negative trigger cases (test set)..."
TEST_NEG_RESULT=$(eval_trigger_file "$TMPDIR/test-neg.jsonl" "negative" "$SKILL")
TEST_IMPROVED_NEG_PASS=$(echo "$TEST_NEG_RESULT" | cut -d'|' -f1)
TEST_IMPROVED_NEG_TOTAL=$(echo "$TEST_NEG_RESULT" | cut -d'|' -f2)

TEST_IMPROVED_PASS=$((TEST_IMPROVED_POS_PASS + TEST_IMPROVED_NEG_PASS))
TEST_IMPROVED_TOTAL=$((TEST_IMPROVED_POS_TOTAL + TEST_IMPROVED_NEG_TOTAL))

# Restore original SKILL.md for baseline test comparison
cp "$TMPDIR/SKILL.md.backup" "$SKILL_MD"

echo "  Running baseline on test set for comparison..."
echo "  Running positive trigger cases (test set, baseline)..."
TEST_BASE_POS_RESULT=$(eval_trigger_file "$TMPDIR/test-pos.jsonl" "positive" "$SKILL")
TEST_BASE_POS_PASS=$(echo "$TEST_BASE_POS_RESULT" | cut -d'|' -f1)
TEST_BASE_POS_TOTAL=$(echo "$TEST_BASE_POS_RESULT" | cut -d'|' -f2)

echo "  Running negative trigger cases (test set, baseline)..."
TEST_BASE_NEG_RESULT=$(eval_trigger_file "$TMPDIR/test-neg.jsonl" "negative" "$SKILL")
TEST_BASE_NEG_PASS=$(echo "$TEST_BASE_NEG_RESULT" | cut -d'|' -f1)
TEST_BASE_NEG_TOTAL=$(echo "$TEST_BASE_NEG_RESULT" | cut -d'|' -f2)

TEST_BASE_PASS=$((TEST_BASE_POS_PASS + TEST_BASE_NEG_PASS))
TEST_BASE_TOTAL=$((TEST_BASE_POS_TOTAL + TEST_BASE_NEG_TOTAL))

echo ""
echo "  Test set scores:"
echo "    Baseline: ${TEST_BASE_PASS}/${TEST_BASE_TOTAL}"
echo "    Improved: ${TEST_IMPROVED_PASS}/${TEST_IMPROVED_TOTAL}"
echo ""

{
  echo "## Held-Out Test Set Validation"
  echo ""
  echo "| Metric | Baseline | Improved |"
  echo "|--------|----------|----------|"
  echo "| Positive | ${TEST_BASE_POS_PASS}/${TEST_BASE_POS_TOTAL} | ${TEST_IMPROVED_POS_PASS}/${TEST_IMPROVED_POS_TOTAL} |"
  echo "| Negative | ${TEST_BASE_NEG_PASS}/${TEST_BASE_NEG_TOTAL} | ${TEST_IMPROVED_NEG_PASS}/${TEST_IMPROVED_NEG_TOTAL} |"
  echo "| Combined | ${TEST_BASE_PASS}/${TEST_BASE_TOTAL} | ${TEST_IMPROVED_PASS}/${TEST_IMPROVED_TOTAL} |"
  echo ""
} >> "$REPORT"

# ---------------------------------------------------------------------------
# Step 7: Final verdict
# ---------------------------------------------------------------------------
if [[ $TEST_IMPROVED_PASS -ge $TEST_BASE_PASS ]]; then
  VERDICT="ACCEPT"
  echo "  ✅ ACCEPT: Improved description holds on test set"
else
  VERDICT="REJECT"
  echo "  ❌ REJECT: Improvement does not hold on test set (overfitting)"
fi

{
  echo "## Verdict: ${VERDICT}"
  echo ""
  if [[ "$VERDICT" == "ACCEPT" ]]; then
    echo "The improved description performs at least as well on the held-out test set."
    echo "Review the proposed description above and apply it manually if satisfied."
  else
    echo "The improvement on the train set did not generalize to the test set."
    echo "This suggests overfitting to the train cases. The original description is preserved."
  fi
  echo ""
  echo "## Proposed Description (for manual application)"
  echo ""
  echo '```yaml'
  echo "description: >-"
  echo "  ${PROPOSED_DESC}"
  echo '```'
} >> "$REPORT"

# Ensure original is restored
cp "$TMPDIR/SKILL.md.backup" "$SKILL_MD"

echo ""
echo "═══════════════════════════════════════════"
echo "  Verdict: ${VERDICT}"
echo "  Report: ${REPORT}"
echo "═══════════════════════════════════════════"
