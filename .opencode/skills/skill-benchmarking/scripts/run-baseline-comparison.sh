#!/usr/bin/env bash
# run-baseline-comparison.sh — Compare a skill before and after modification
#
# Usage:
#   ./scripts/run-baseline-comparison.sh <original-skill.md> <modified-skill.md>
#   ./scripts/run-baseline-comparison.sh --usefulness <original-skill.md> <modified-skill.md>
#
# Produces a comparison report showing what changed, what was preserved,
# and whether the modification passed quality gates.
#
# Options:
#   --usefulness  Enable LLM-as-Judge usefulness scoring for eval comparison
#
# Requires: python3, jq

set -euo pipefail

# Auto-detect repo root
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root (no AGENTS.md found)"; exit 1; }
CHECK_SCRIPT="${REPO_ROOT}/scripts/check_skill_structure.py"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

usage() {
  echo "Usage: $0 [--usefulness] <original-skill.md> <modified-skill.md>"
  echo ""
  echo "Compares a skill before and after a meta-skill operation."
  echo "Outputs a markdown comparison report to stdout."
  echo ""
  echo "Options:"
  echo "  --usefulness  Pass --usefulness to run-evals.sh for LLM-based quality scoring"
  exit 1
}

# Parse options
USEFULNESS_FLAG=""
POSITIONAL=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    --usefulness)
      USEFULNESS_FLAG="--usefulness"
      shift
      ;;
    --help|-h)
      usage
      ;;
    *)
      POSITIONAL+=("$1")
      shift
      ;;
  esac
done

[[ ${#POSITIONAL[@]} -lt 2 ]] && usage

ORIGINAL="${POSITIONAL[0]}"
MODIFIED="${POSITIONAL[1]}"

if [[ ! -f "$ORIGINAL" ]]; then
  echo "Error: original file not found: ${ORIGINAL}" >&2
  exit 1
fi
if [[ ! -f "$MODIFIED" ]]; then
  echo "Error: modified file not found: ${MODIFIED}" >&2
  exit 1
fi
if [[ ! -f "$CHECK_SCRIPT" ]]; then
  echo "Error: check_skill_structure.py not found at ${CHECK_SCRIPT}" >&2
  exit 1
fi
if ! command -v jq &>/dev/null; then
  echo "Error: jq is required but not found" >&2
  exit 1
fi

# Eval comparison state
EVAL_SCRIPT="${REPO_ROOT}/scripts/run-evals.sh"
EVAL_TMPBASE=""
EVAL_AVAILABLE=false
EVAL_ORIG_EXIT=""
EVAL_MOD_EXIT=""

cleanup() {
  if [[ -n "$EVAL_TMPBASE" && -d "$EVAL_TMPBASE" ]]; then
    local tmpname
    tmpname="$(basename "$EVAL_TMPBASE")"
    rm -rf "$EVAL_TMPBASE"
    # Clean up any eval result files created for the temp skill
    rm -f "${REPO_ROOT}/eval-results/${tmpname}"*.md 2>/dev/null || true
  fi
}
trap cleanup EXIT

# Run structural checks on both files
ORIG_JSON="$(python3 "$CHECK_SCRIPT" "$ORIGINAL" 2>/dev/null || true)"
MOD_JSON="$(python3 "$CHECK_SCRIPT" "$MODIFIED" 2>/dev/null || true)"

if [[ -z "$ORIG_JSON" ]]; then
  echo "Error: check_skill_structure.py failed on original: ${ORIGINAL}" >&2
  exit 1
fi
if [[ -z "$MOD_JSON" ]]; then
  echo "Error: check_skill_structure.py failed on modified: ${MODIFIED}" >&2
  exit 1
fi

# Extract scores
orig_score="$(echo "$ORIG_JSON" | jq -r '.score')"
orig_max="$(echo "$ORIG_JSON" | jq -r '.max_score')"
orig_valid="$(echo "$ORIG_JSON" | jq -r '.valid')"
mod_score="$(echo "$MOD_JSON" | jq -r '.score')"
mod_max="$(echo "$MOD_JSON" | jq -r '.max_score')"
mod_valid="$(echo "$MOD_JSON" | jq -r '.valid')"

# Extract line counts
orig_lines="$(echo "$ORIG_JSON" | jq -r '.checks.line_count.detail // "unknown"')"
mod_lines="$(echo "$MOD_JSON" | jq -r '.checks.line_count.detail // "unknown"')"

# Extract frontmatter names
orig_name="$(echo "$ORIG_JSON" | jq -r '.checks.frontmatter_fields.detail // ""' | grep -oP 'name=[^,]+' | sed 's/^name=//' || echo "unknown")"
mod_name="$(echo "$MOD_JSON" | jq -r '.checks.frontmatter_fields.detail // ""' | grep -oP 'name=[^,]+' | sed 's/^name=//' || echo "unknown")"

# Collect all check keys from both reports
all_checks="$(echo "$ORIG_JSON $MOD_JSON" | jq -rs '[.[].checks | keys[]] | unique | .[]')"

# --- Eval Comparison ---
skill_name="$orig_name"
skill_dir="${REPO_ROOT}/${skill_name}"

if [[ "$skill_name" != "unknown" && -d "${skill_dir}/evals" && -f "$EVAL_SCRIPT" ]]; then
  EVAL_AVAILABLE=true

  # Create a temporary skill directory inside the repo root
  EVAL_TMPBASE="$(mktemp -d "${REPO_ROOT}/.baseline-eval-XXXXXX")"
  eval_skill_basename="$(basename "$EVAL_TMPBASE")"

  # Copy the evals/ directory from the original skill
  cp -r "${skill_dir}/evals" "${EVAL_TMPBASE}/evals"

  # Run evals with the original SKILL.md
  cp "$ORIGINAL" "${EVAL_TMPBASE}/SKILL.md"
  if EVAL_ORIG_OUTPUT="$("$EVAL_SCRIPT" --json $USEFULNESS_FLAG "$eval_skill_basename" 2>&1)"; then
    EVAL_ORIG_EXIT=0
  else
    EVAL_ORIG_EXIT=$?
  fi

  # Run evals with the modified SKILL.md
  cp "$MODIFIED" "${EVAL_TMPBASE}/SKILL.md"
  if EVAL_MOD_OUTPUT="$("$EVAL_SCRIPT" --json $USEFULNESS_FLAG "$eval_skill_basename" 2>&1)"; then
    EVAL_MOD_EXIT=0
  else
    EVAL_MOD_EXIT=$?
  fi
fi

# --- Quality Gates ---
GATE_PASS=0
GATE_FAIL=0
GATE_RESULTS=()

# Gate 1: Modified must have >= sections as original
orig_section_count="$(echo "$ORIG_JSON" | jq '[.checks | to_entries[] | select(.key | startswith("has_")) | select(.value.pass == true)] | length')"
mod_section_count="$(echo "$MOD_JSON" | jq '[.checks | to_entries[] | select(.key | startswith("has_")) | select(.value.pass == true)] | length')"
if [[ "$mod_section_count" -ge "$orig_section_count" ]]; then
  GATE_RESULTS+=("✓|Section count|${orig_section_count} → ${mod_section_count}|Sections preserved or added")
  GATE_PASS=$((GATE_PASS + 1))
else
  GATE_RESULTS+=("✗|Section count|${orig_section_count} → ${mod_section_count}|Sections were removed")
  GATE_FAIL=$((GATE_FAIL + 1))
fi

# Gate 2: No section deletions — all passing sections in original must pass in modified
deleted_sections=""
while IFS= read -r check_key; do
  orig_pass="$(echo "$ORIG_JSON" | jq -r --arg k "$check_key" '.checks[$k].pass // false')"
  mod_pass="$(echo "$MOD_JSON" | jq -r --arg k "$check_key" '.checks[$k].pass // false')"
  if [[ "$orig_pass" == "true" && "$mod_pass" != "true" ]]; then
    deleted_sections="${deleted_sections}${check_key}, "
  fi
done <<< "$(echo "$ORIG_JSON" | jq -r '.checks | to_entries[] | select(.key | startswith("has_")) | .key')"

if [[ -z "$deleted_sections" ]]; then
  GATE_RESULTS+=("✓|No section deletions|—|All original sections preserved")
  GATE_PASS=$((GATE_PASS + 1))
else
  deleted_sections="${deleted_sections%, }"
  GATE_RESULTS+=("✗|No section deletions|${deleted_sections}|Sections were deleted")
  GATE_FAIL=$((GATE_FAIL + 1))
fi

# Gate 3: Line count — modified must be < 500
mod_line_num="$(echo "$mod_lines" | grep -oP '^\d+' || echo "0")"
if [[ "$mod_line_num" -lt 500 ]]; then
  GATE_RESULTS+=("✓|Line count < 500|${mod_lines}|Within limit")
  GATE_PASS=$((GATE_PASS + 1))
else
  GATE_RESULTS+=("✗|Line count < 500|${mod_lines}|Exceeds limit")
  GATE_FAIL=$((GATE_FAIL + 1))
fi

# Gate 4: Frontmatter name preserved
if [[ "$orig_name" == "$mod_name" ]] || [[ "$orig_name" == "unknown" ]]; then
  GATE_RESULTS+=("✓|Name preserved|${orig_name} → ${mod_name}|Unchanged")
  GATE_PASS=$((GATE_PASS + 1))
else
  GATE_RESULTS+=("✗|Name preserved|${orig_name} → ${mod_name}|Name was changed")
  GATE_FAIL=$((GATE_FAIL + 1))
fi

# Gate 5: Eval regression — modified must not fail evals that original passed
if $EVAL_AVAILABLE; then
  if [[ "$EVAL_ORIG_EXIT" -eq 0 && "$EVAL_MOD_EXIT" -ne 0 ]]; then
    GATE_RESULTS+=("✗|Eval regression|original=PASS modified=FAIL|Modified SKILL.md broke eval suite")
    GATE_FAIL=$((GATE_FAIL + 1))
  elif [[ "$EVAL_ORIG_EXIT" -ne 0 && "$EVAL_MOD_EXIT" -eq 0 ]]; then
    GATE_RESULTS+=("✓|Eval regression|original=FAIL modified=PASS|Modified SKILL.md fixed eval suite")
    GATE_PASS=$((GATE_PASS + 1))
  elif [[ "$EVAL_ORIG_EXIT" -eq 0 && "$EVAL_MOD_EXIT" -eq 0 ]]; then
    GATE_RESULTS+=("✓|Eval regression|original=PASS modified=PASS|Both versions pass eval suite")
    GATE_PASS=$((GATE_PASS + 1))
  else
    GATE_RESULTS+=("✓|Eval regression|original=FAIL modified=FAIL|Both versions fail eval suite (no regression)")
    GATE_PASS=$((GATE_PASS + 1))
  fi
fi

# --- Score delta ---
score_delta=$((mod_score - orig_score))
if [[ "$score_delta" -gt 0 ]]; then
  delta_display="+${score_delta}"
  delta_verdict="improved"
elif [[ "$score_delta" -lt 0 ]]; then
  delta_display="${score_delta}"
  delta_verdict="degraded"
else
  delta_display="0"
  delta_verdict="unchanged"
fi

# --- Generate Report ---
OVERALL="PASS"
if [[ "$GATE_FAIL" -gt 0 ]]; then
  OVERALL="FAIL"
fi

cat <<EOF
# Baseline Comparison Report

## Files

- **Original**: \`${ORIGINAL}\`
- **Modified**: \`${MODIFIED}\`

## Score Summary

| Metric | Original | Modified | Delta |
|--------|----------|----------|-------|
| Score | ${orig_score}/${orig_max} | ${mod_score}/${mod_max} | ${delta_display} (${delta_verdict}) |
| Valid | ${orig_valid} | ${mod_valid} | — |
| Lines | ${orig_lines} | ${mod_lines} | — |

## Check-by-Check Comparison

| Check | Original | Modified | Change |
|-------|----------|----------|--------|
EOF

while IFS= read -r check_key; do
  orig_pass="$(echo "$ORIG_JSON" | jq -r --arg k "$check_key" 'if .checks[$k] then (if .checks[$k].pass then "✓" else "✗" end) else "—" end')"
  mod_pass="$(echo "$MOD_JSON" | jq -r --arg k "$check_key" 'if .checks[$k] then (if .checks[$k].pass then "✓" else "✗" end) else "—" end')"
  if [[ "$orig_pass" == "$mod_pass" ]]; then
    change="—"
  elif [[ "$orig_pass" == "✗" && "$mod_pass" == "✓" ]]; then
    change="🟢 fixed"
  elif [[ "$orig_pass" == "✓" && "$mod_pass" == "✗" ]]; then
    change="🔴 regressed"
  else
    change="changed"
  fi
  echo "| \`${check_key}\` | ${orig_pass} | ${mod_pass} | ${change} |"
done <<< "$all_checks"

if $EVAL_AVAILABLE; then
  orig_eval_status="PASS"; [[ "$EVAL_ORIG_EXIT" -ne 0 ]] && orig_eval_status="FAIL"
  mod_eval_status="PASS"; [[ "$EVAL_MOD_EXIT" -ne 0 ]] && mod_eval_status="FAIL"
  cat <<EVALEOF

## Eval Comparison

| Version | Eval Result | Exit Code |
|---------|-------------|-----------|
| Original | ${orig_eval_status} | ${EVAL_ORIG_EXIT} |
| Modified | ${mod_eval_status} | ${EVAL_MOD_EXIT} |
EVALEOF
  if [[ "$EVAL_ORIG_EXIT" -eq 0 && "$EVAL_MOD_EXIT" -ne 0 ]]; then
    echo ""
    echo "> ⚠ **Eval regression detected**: original SKILL.md passes the eval suite but modified version fails."
  elif [[ "$EVAL_ORIG_EXIT" -ne 0 && "$EVAL_MOD_EXIT" -eq 0 ]]; then
    echo ""
    echo "> 🟢 **Eval improvement**: modified SKILL.md now passes the eval suite."
  fi
  if [[ -n "$USEFULNESS_FLAG" ]]; then
    echo ""
    echo "> 🔍 **Usefulness evaluation**: enabled via \`--usefulness\`. Check per-skill eval reports in \`eval-results/\` for detailed usefulness scores."
  fi
else
  cat <<EVALEOF

## Eval Comparison

No eval suite available — structural comparison only.
EVALEOF
fi

cat <<EOF

## Quality Gates

| Result | Gate | Detail | Note |
|--------|------|--------|------|
EOF

for gate_line in "${GATE_RESULTS[@]}"; do
  IFS='|' read -r result gate detail note <<< "$gate_line"
  echo "| ${result} | ${gate} | ${detail} | ${note} |"
done

cat <<EOF

## Verdict: **${OVERALL}**

- Gates passed: ${GATE_PASS}/$(( GATE_PASS + GATE_FAIL ))
- Score delta: ${delta_display} (${delta_verdict})
EOF

if [[ "$GATE_FAIL" -gt 0 ]]; then
  echo "- ⚠ **${GATE_FAIL} gate(s) failed** — modification did not pass quality standards"
fi

# Exit code reflects verdict
if [[ "$OVERALL" == "PASS" ]]; then
  exit 0
else
  exit 1
fi
