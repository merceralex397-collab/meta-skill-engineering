#!/usr/bin/env bash
# run-full-cycle.sh — Run the complete evaluation cadence
#
# Steps:
#   1. Structural validation (validate-skills.sh)
#   2. Trigger and behavior evals (run-evals.sh --all)
#   3. Corpus evaluation (run-corpus-eval.sh)
#   4. Regression suite (run-regression-suite.sh)
#   4.5. Harvest failures into regression corpus (harvest_failures.py)
#   5. Aggregate report
#
# Usage:
#   ./scripts/run-full-cycle.sh [--dry-run]
#
# Exit code: 0 if all steps pass, 1 if any step fails.

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
DRY_RUN=false

# Parse args
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [--dry-run]"
      echo ""
      echo "Run the complete evaluation cadence (structural, trigger/behavior,"
      echo "corpus, regression) and produce an aggregate report."
      echo ""
      echo "Options:"
      echo "  --dry-run  Preview what would be tested without executing"
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      exit 1
      ;;
  esac
done

mkdir -p "$RESULTS_DIR"

SUMMARY_FILE="${RESULTS_DIR}/summary-${TIMESTAMP}.md"
STEP_RESULTS=()
STEP_OUTPUTS=()
ANY_FAIL=false

run_step() {
  local step_num="$1"
  local step_name="$2"
  local script_name="$3"
  shift 3
  local args=("$@")

  echo ""
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "  Step ${step_num}: ${step_name}"
  echo "  Running: ${script_name} ${args[*]:-}"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

  local exit_code=0
  local output
  output=$("${REPO_ROOT}/scripts/${script_name}" "${args[@]}" 2>&1) || exit_code=$?

  if [[ $exit_code -eq 0 ]]; then
    echo "  ✅ Step ${step_num} PASSED"
    STEP_RESULTS+=("PASS")
  else
    echo "  ❌ Step ${step_num} FAILED (exit code ${exit_code})"
    STEP_RESULTS+=("FAIL")
    ANY_FAIL=true
  fi

  STEP_OUTPUTS+=("$output")
}

skip_step() {
  local step_num="$1"
  local step_name="$2"
  local reason="$3"

  echo ""
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "  Step ${step_num}: ${step_name}"
  echo "  ⏭  Skipped: ${reason}"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

  STEP_RESULTS+=("SKIP")
  STEP_OUTPUTS+=("Skipped: ${reason}")
}

echo "═══════════════════════════════════════════"
echo "  Full Evaluation Cycle"
echo "  Timestamp: ${TIMESTAMP}"
echo "  Mode: $(if $DRY_RUN; then echo 'DRY RUN'; else echo 'LIVE'; fi)"
echo "═══════════════════════════════════════════"

# --- Step 1: Structural validation ---
run_step 1 "Structural validation" "validate-skills.sh"

# --- Step 2: Trigger and behavior evals ---
EVAL_ARGS=(--all)
if $DRY_RUN; then
  EVAL_ARGS+=(--dry-run)
fi
run_step 2 "Trigger & behavior evals" "run-evals.sh" "${EVAL_ARGS[@]}"

# --- Step 3: Corpus evaluation ---
if $DRY_RUN; then
  skip_step 3 "Corpus evaluation" "--dry-run not supported by run-corpus-eval.sh"
else
  # Run corpus eval for the two main improvement meta-skills
  echo ""
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "  Step 3: Corpus evaluation"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

  # Enable Layer 2 automatically if copilot CLI is available
  CORPUS_LAYER2_ARGS=()
  if command -v copilot &>/dev/null; then
    echo "  copilot CLI detected — enabling Layer 2 (meta-skill invocation + A/B judge)"
    CORPUS_LAYER2_ARGS=(--layer2)
  else
    echo "  copilot CLI not found — running Layer 1 only (structural checks)"
  fi

  corpus_exit=0
  corpus_output=""

  for meta_skill in skill-improver skill-anti-patterns skill-evaluation skill-safety-review; do
    echo "  Running: run-corpus-eval.sh ${CORPUS_LAYER2_ARGS[*]:-} ${meta_skill} --all"
    local_output=$("${REPO_ROOT}/scripts/run-corpus-eval.sh" "${CORPUS_LAYER2_ARGS[@]}" "$meta_skill" --all 2>&1) || corpus_exit=1
    corpus_output+=$'\n'"--- ${meta_skill} ---"$'\n'"${local_output}"
  done

  if [[ $corpus_exit -eq 0 ]]; then
    echo "  ✅ Step 3 PASSED"
    STEP_RESULTS+=("PASS")
  else
    echo "  ❌ Step 3 FAILED"
    STEP_RESULTS+=("FAIL")
    ANY_FAIL=true
  fi

  STEP_OUTPUTS+=("$corpus_output")
fi

# --- Step 4: Regression suite ---
if $DRY_RUN; then
  skip_step 4 "Regression suite" "--dry-run not supported by run-regression-suite.sh"
else
  run_step 4 "Regression suite" "run-regression-suite.sh"
fi

# --- Step 4.5: Harvest failures into regression corpus ---
if ! $DRY_RUN; then
  echo ""
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "  Step 4.5: Failure harvesting"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

  harvest_count=0
  for report in "${RESULTS_DIR}"/*-"${TIMESTAMP}".md; do
    [[ -f "$report" ]] || continue
    [[ "$(basename "$report")" == summary-* ]] && continue
    output=$(python3 "${REPO_ROOT}/scripts/harvest_failures.py" "$report" 2>&1) || true
    if echo "$output" | grep -q "Harvested"; then
      echo "  $output"
      harvest_count=$((harvest_count + 1))
    fi
  done

  if [[ $harvest_count -gt 0 ]]; then
    echo "  Harvested failures from ${harvest_count} report(s) into corpus/regression/"
  else
    echo "  No failures to harvest"
  fi
fi

# --- Step 5: Aggregate report ---
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Step 5: Generating aggregate report"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

OVERALL_VERDICT="PASS"
if $ANY_FAIL; then
  OVERALL_VERDICT="FAIL"
fi

STEP_NAMES=("Structural validation" "Trigger & behavior evals" "Corpus evaluation" "Regression suite")
STEP_SCRIPTS=("validate-skills.sh" "run-evals.sh --all" "run-corpus-eval.sh" "run-regression-suite.sh")

{
  echo "# Evaluation Cycle Summary"
  echo ""
  echo "Date: ${TIMESTAMP}"
  echo ""
  echo "## Step Results"
  echo ""
  echo "| Step | Script | Result |"
  echo "|------|--------|--------|"
  for i in "${!STEP_NAMES[@]}"; do
    echo "| $((i + 1)). ${STEP_NAMES[$i]} | ${STEP_SCRIPTS[$i]} | ${STEP_RESULTS[$i]} |"
  done
  echo ""
  echo "## Overall Verdict: ${OVERALL_VERDICT}"
  echo ""
  echo "## Details"
  echo ""
  for i in "${!STEP_NAMES[@]}"; do
    echo "### Step $((i + 1)): ${STEP_NAMES[$i]}"
    echo ""
    echo '```'
    # Truncate very long output to keep the report readable
    local_output="${STEP_OUTPUTS[$i]}"
    line_count=$(wc -l <<< "$local_output")
    if [[ $line_count -gt 80 ]]; then
      head -40 <<< "$local_output"
      echo ""
      echo "... (${line_count} total lines, truncated) ..."
      echo ""
      tail -20 <<< "$local_output"
    else
      echo "$local_output"
    fi
    echo '```'
    echo ""
  done
} > "$SUMMARY_FILE"

# Symlink to latest summary
ln -sf "summary-${TIMESTAMP}.md" "${RESULTS_DIR}/summary-latest.md"

echo "  Report saved: eval-results/summary-${TIMESTAMP}.md"
echo "  (symlinked from eval-results/summary-latest.md)"

echo ""
echo "═══════════════════════════════════════════"
echo "  Full Evaluation Cycle Complete"
if [[ "$OVERALL_VERDICT" == "PASS" ]]; then
  echo "  ✅ OVERALL: PASS"
else
  echo "  ❌ OVERALL: FAIL"
fi
echo "  Report: eval-results/summary-${TIMESTAMP}.md"
echo "═══════════════════════════════════════════"

if $ANY_FAIL; then
  exit 1
fi
exit 0
