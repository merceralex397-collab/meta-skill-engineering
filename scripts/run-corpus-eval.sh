#!/usr/bin/env bash
# run-corpus-eval.sh — Test meta-skills against the target skill corpus
#
# Layer 1: Did the meta-skill produce valid output? (structural checks)
# Layer 2: Does the rewritten skill perform better? (eval comparison)
#
# Usage:
#   ./scripts/run-corpus-eval.sh <meta-skill> [corpus-tier]
#   ./scripts/run-corpus-eval.sh skill-improver weak
#   ./scripts/run-corpus-eval.sh skill-anti-patterns adversarial
#   ./scripts/run-corpus-eval.sh skill-improver --all
#
# Requires: jq, python3
# Optional: copilot CLI (for --layer2)

set -euo pipefail

# Auto-detect repo root
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root (no AGENTS.md found)"; exit 1; }
CORPUS_DIR="${REPO_ROOT}/corpus"
RESULTS_DIR="${REPO_ROOT}/eval-results"
CHECK_SCRIPT="${REPO_ROOT}/scripts/check_skill_structure.py"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

usage() {
  echo "Usage: $0 [--layer2] <meta-skill> [corpus-tier]"
  echo ""
  echo "Arguments:"
  echo "  meta-skill    Name of the meta-skill to evaluate (e.g. skill-improver)"
  echo "  corpus-tier   One of: weak, strong, adversarial, --all (default: --all)"
  echo ""
  echo "Options:"
  echo "  --layer2      Enable Layer 2: invoke the meta-skill via copilot CLI on each"
  echo "                corpus fixture, then compare original vs improved output using"
  echo "                an LLM judge. Requires the 'copilot' CLI to be installed."
  echo ""
  echo "Environment variables:"
  echo "  EVAL_MODEL      Model for meta-skill invocation (default: gpt-4.1)"
  echo "  EVAL_TIMEOUT    Timeout in seconds for copilot calls (default: 120)"
  echo "  LAYER2_JUDGE_MODEL  Model for the A/B judge (default: EVAL_MODEL)"
  echo ""
  echo "Examples:"
  echo "  $0 skill-improver weak"
  echo "  $0 --layer2 skill-improver adversarial"
  echo "  $0 skill-improver --all"
  exit 1
}

log_info()  { echo -e "${CYAN}[INFO]${NC}  $1"; }
log_ok()    { echo -e "${GREEN}[PASS]${NC}  $1"; }
log_fail()  { echo -e "${RED}[FAIL]${NC}  $1"; }
log_warn()  { echo -e "${YELLOW}[WARN]${NC}  $1"; }

# --- Argument parsing ---
LAYER2=false
POSITIONAL_ARGS=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --layer2)
      LAYER2=true
      shift
      ;;
    --help|-h)
      usage
      ;;
    *)
      POSITIONAL_ARGS+=("$1")
      shift
      ;;
  esac
done

[[ ${#POSITIONAL_ARGS[@]} -lt 1 ]] && usage
META_SKILL="${POSITIONAL_ARGS[0]}"
TIER="${POSITIONAL_ARGS[1]:---all}"

# Layer 2 configuration
LAYER2_MODEL="${EVAL_MODEL:-gpt-4.1}"
LAYER2_TIMEOUT="${EVAL_TIMEOUT:-120}"
LAYER2_JUDGE_MODEL="${LAYER2_JUDGE_MODEL:-$LAYER2_MODEL}"

# Validate copilot CLI is available if --layer2 was requested
if $LAYER2; then
  if ! command -v copilot &>/dev/null; then
    echo "Error: --layer2 requires the 'copilot' CLI but it is not installed" >&2
    echo "Install copilot CLI or run without --layer2 for structural-only evaluation" >&2
    exit 1
  fi
  log_info "Layer 2 enabled — meta-skill invocation + A/B comparison"
fi

# Validate meta-skill exists
if [[ ! -d "${REPO_ROOT}/${META_SKILL}" ]] || [[ ! -f "${REPO_ROOT}/${META_SKILL}/SKILL.md" ]]; then
  echo "Error: meta-skill '${META_SKILL}' not found (no ${META_SKILL}/SKILL.md)" >&2
  exit 1
fi

# Validate tier
TIERS=()
case "$TIER" in
  --all)
    for t in weak strong adversarial; do
      [[ -d "${CORPUS_DIR}/${t}" ]] && TIERS+=("$t")
    done
    ;;
  weak|strong|adversarial)
    if [[ ! -d "${CORPUS_DIR}/${TIER}" ]]; then
      echo "Error: corpus tier '${TIER}' not found at ${CORPUS_DIR}/${TIER}" >&2
      exit 1
    fi
    TIERS=("$TIER")
    ;;
  *)
    echo "Error: unknown tier '${TIER}'. Use weak, strong, adversarial, or --all" >&2
    exit 1
    ;;
esac

# Validate dependencies
if ! command -v python3 &>/dev/null; then
  echo "Error: python3 is required but not found" >&2
  exit 1
fi
if ! command -v jq &>/dev/null; then
  echo "Error: jq is required but not found" >&2
  exit 1
fi
if [[ ! -f "$CHECK_SCRIPT" ]]; then
  echo "Error: check_skill_structure.py not found at ${CHECK_SCRIPT}" >&2
  exit 1
fi

mkdir -p "$RESULTS_DIR"

# --- Layer 2 helper functions ---

# Build the meta-skill invocation prompt based on which meta-skill is being tested
build_meta_skill_prompt() {
  local meta_skill="$1"
  local skill_content="$2"

  case "$meta_skill" in
    skill-improver)
      echo "You are the skill-improver. Improve the following SKILL.md. Output only the improved SKILL.md content, nothing else."
      echo ""
      echo "$skill_content"
      ;;
    skill-anti-patterns)
      echo "You are the skill-anti-patterns detector. Analyze the following SKILL.md for anti-patterns and produce a corrected version. Output only the improved SKILL.md content, nothing else."
      echo ""
      echo "$skill_content"
      ;;
    skill-evaluation)
      echo "You are the skill-evaluation meta-skill. Evaluate the following SKILL.md and produce an improved version that addresses any issues. Output only the improved SKILL.md content, nothing else."
      echo ""
      echo "$skill_content"
      ;;
    skill-safety-review)
      echo "You are the skill-safety-review meta-skill. Review the following SKILL.md for safety issues and produce a hardened version. Output only the improved SKILL.md content, nothing else."
      echo ""
      echo "$skill_content"
      ;;
    *)
      echo "You are the ${meta_skill} meta-skill. Process the following SKILL.md according to your purpose. Output only the resulting SKILL.md content, nothing else."
      echo ""
      echo "$skill_content"
      ;;
  esac
}

# Run a copilot prompt with timeout and model settings
run_copilot_layer2() {
  local prompt="$1"
  local timeout_sec="${LAYER2_TIMEOUT}"

  timeout "${timeout_sec}" copilot -p "$prompt" -m "$LAYER2_MODEL" 2>/dev/null || {
    local rc=$?
    if [[ $rc -eq 124 ]]; then
      echo "ERROR:TIMEOUT"
    else
      echo "ERROR:EXIT_${rc}"
    fi
  }
}

# LLM judge: compare original vs improved output, return A/B/TIE verdict
run_layer2_judge() {
  local original_content="$1"
  local improved_content="$2"
  local meta_skill="$3"

  local judge_prompt
  judge_prompt="You are an expert judge of agent skill definitions (SKILL.md files).

Compare these two versions of a skill definition. Version A is the original. Version B was produced by the '${meta_skill}' meta-skill.

Rate which version is better as a skill definition for an AI agent. Consider:
- Clarity and specificity of instructions
- Completeness of the procedure
- Quality of failure handling
- Actionability of the output contract
- Appropriate scope (not too broad, not too narrow)

IMPORTANT: On the FIRST line of your response, output EXACTLY one of: A, B, or TIE
On the SECOND line, give a brief (one sentence) reason.

--- VERSION A (original) ---
${original_content}

--- VERSION B (improved by ${meta_skill}) ---
${improved_content}
"

  timeout "${LAYER2_TIMEOUT}" copilot -p "$judge_prompt" -m "$LAYER2_JUDGE_MODEL" 2>/dev/null || {
    local rc=$?
    if [[ $rc -eq 124 ]]; then
      echo "ERROR:TIMEOUT"
    else
      echo "ERROR:EXIT_${rc}"
    fi
  }
}

# --- Main evaluation loop ---
TOTAL_SKILLS=0
TOTAL_PASS=0
TOTAL_FAIL=0
TOTAL_WARN=0

for tier in "${TIERS[@]}"; do
  TIER_DIR="${CORPUS_DIR}/${tier}"
  REPORT_FILE="${RESULTS_DIR}/corpus-${META_SKILL}-${tier}-${TIMESTAMP}.md"
  TIER_PASS=0
  TIER_FAIL=0
  TIER_WARN=0

  # Layer 2 tracking
  L2_RESULTS=()
  L2_IMPROVED_WINS=0
  L2_ORIGINAL_WINS=0
  L2_TIES=0
  L2_ERRORS=0

  log_info "Evaluating tier: ${tier} (meta-skill: ${META_SKILL})"

  # Start the report
  {
    echo "# Corpus Evaluation: ${META_SKILL} → ${tier}"
    echo ""
    echo "- **Meta-skill**: \`${META_SKILL}\`"
    echo "- **Tier**: \`${tier}\`"
    echo "- **Timestamp**: ${TIMESTAMP}"
    if $LAYER2; then
      echo "- **Mode**: Layer 1 (structural) + Layer 2 (meta-skill invocation + A/B judge)"
      echo "- **Invocation model**: \`${LAYER2_MODEL}\`"
      echo "- **Judge model**: \`${LAYER2_JUDGE_MODEL}\`"
    else
      echo "- **Mode**: Layer 1 structural evaluation (pre-scores)"
    fi
    echo ""
    echo "## Layer 1: Structural Results"
    echo ""
    echo "| Skill File | Score | Max | Valid | Issues |"
    echo "|------------|-------|-----|-------|--------|"
  } > "$REPORT_FILE"

  for skill_file in "${TIER_DIR}"/*.md; do
    [[ -f "$skill_file" ]] || continue
    skill_basename="$(basename "$skill_file")"
    TOTAL_SKILLS=$((TOTAL_SKILLS + 1))

    # Create temp working directory
    WORK_DIR="$(mktemp -d)"
    trap "rm -rf '$WORK_DIR'" EXIT

    # Copy to temp dir as baseline
    cp "$skill_file" "${WORK_DIR}/original.md"
    cp "$skill_file" "${WORK_DIR}/working.md"

    # Run structural checks (Layer 1) — pre-scores
    PRE_JSON="$(python3 "$CHECK_SCRIPT" "$skill_file" 2>/dev/null || true)"

    if [[ -z "$PRE_JSON" ]]; then
      log_fail "${tier}/${skill_basename}: check_skill_structure.py produced no output"
      echo "| \`${skill_basename}\` | - | - | ERROR | checker error |" >> "$REPORT_FILE"
      TIER_FAIL=$((TIER_FAIL + 1))
      rm -rf "$WORK_DIR"
      continue
    fi

    # Extract scores from JSON
    pre_score="$(echo "$PRE_JSON" | jq -r '.score // 0')"
    pre_max="$(echo "$PRE_JSON" | jq -r '.max_score // 0')"
    pre_valid="$(echo "$PRE_JSON" | jq -r '.valid')"
    pre_warnings="$(echo "$PRE_JSON" | jq -r '.warnings | length')"

    # Collect failing checks
    failing_checks="$(echo "$PRE_JSON" | jq -r '[.checks | to_entries[] | select(.value.pass == false) | .key] | join(", ")')"
    [[ -z "$failing_checks" ]] && failing_checks="none"

    # Strong tier gate: structural degradation is not acceptable
    if [[ "$tier" == "strong" ]]; then
      if [[ "$pre_valid" != "true" ]]; then
        log_fail "${tier}/${skill_basename}: strong-tier skill is not valid (score: ${pre_score}/${pre_max})"
        echo "| \`${skill_basename}\` | ${pre_score} | ${pre_max} | ✗ FAIL | ${failing_checks} |" >> "$REPORT_FILE"
        TIER_FAIL=$((TIER_FAIL + 1))
      else
        log_ok "${tier}/${skill_basename}: valid (score: ${pre_score}/${pre_max})"
        echo "| \`${skill_basename}\` | ${pre_score} | ${pre_max} | ✓ | ${failing_checks} |" >> "$REPORT_FILE"
        TIER_PASS=$((TIER_PASS + 1))
      fi
    else
      # Weak/adversarial: record baseline state, expect issues
      if [[ "$pre_valid" == "true" ]]; then
        log_ok "${tier}/${skill_basename}: valid (score: ${pre_score}/${pre_max})"
        echo "| \`${skill_basename}\` | ${pre_score} | ${pre_max} | ✓ | ${failing_checks} |" >> "$REPORT_FILE"
        TIER_PASS=$((TIER_PASS + 1))
      else
        log_warn "${tier}/${skill_basename}: issues found (score: ${pre_score}/${pre_max}) — expected for ${tier} tier"
        echo "| \`${skill_basename}\` | ${pre_score} | ${pre_max} | ✗ | ${failing_checks} |" >> "$REPORT_FILE"
        TIER_WARN=$((TIER_WARN + 1))
      fi
    fi

    # Save detailed JSON for this skill
    echo "$PRE_JSON" | jq '.' > "${WORK_DIR}/pre-check.json"

    # --- Layer 2: Meta-skill invocation (requires copilot CLI + --layer2 flag) ---
    if $LAYER2; then
      log_info "  Layer 2: invoking ${META_SKILL} on ${tier}/${skill_basename}..."

      # Read original content
      original_content="$(cat "${WORK_DIR}/original.md")"

      # Build the meta-skill prompt and invoke via copilot
      meta_prompt="$(build_meta_skill_prompt "$META_SKILL" "$original_content")"
      improved_output="$(run_copilot_layer2 "$meta_prompt")"

      if [[ "$improved_output" == ERROR:* ]]; then
        log_fail "  Layer 2: meta-skill invocation failed (${improved_output})"
        L2_RESULTS+=("| \`${skill_basename}\` | ERROR | - | - | - | ${improved_output} |")
        L2_ERRORS=$((L2_ERRORS + 1))
      else
        # Save improved output
        echo "$improved_output" > "${WORK_DIR}/improved.md"

        # Run structural check on improved version
        POST_JSON="$(python3 "$CHECK_SCRIPT" "${WORK_DIR}/improved.md" 2>/dev/null || true)"
        post_score=0
        post_valid="false"
        if [[ -n "$POST_JSON" ]]; then
          post_score="$(echo "$POST_JSON" | jq -r '.score // 0')"
          post_valid="$(echo "$POST_JSON" | jq -r '.valid')"
        fi

        # Score delta
        score_delta=$((post_score - pre_score))
        delta_display="${score_delta}"
        [[ $score_delta -gt 0 ]] && delta_display="+${score_delta}"

        # Run A/B judge comparison
        judge_output="$(run_layer2_judge "$original_content" "$improved_output" "$META_SKILL")"

        if [[ "$judge_output" == ERROR:* ]]; then
          log_fail "  Layer 2: judge failed (${judge_output})"
          L2_RESULTS+=("| \`${skill_basename}\` | ${post_score} | ${delta_display} | ${post_valid} | ERROR | Judge: ${judge_output} |")
          L2_ERRORS=$((L2_ERRORS + 1))
        else
          # Parse verdict from first line of judge output
          verdict_line="$(head -1 <<< "$judge_output" | tr -d '[:space:]' | tr '[:lower:]' '[:upper:]')"
          reason_line="$(sed -n '2p' <<< "$judge_output")"

          case "$verdict_line" in
            A)
              verdict="A (original wins)"
              L2_ORIGINAL_WINS=$((L2_ORIGINAL_WINS + 1))
              ;;
            B)
              verdict="B (improved wins)"
              L2_IMPROVED_WINS=$((L2_IMPROVED_WINS + 1))
              ;;
            TIE)
              verdict="TIE"
              L2_TIES=$((L2_TIES + 1))
              ;;
            *)
              verdict="UNPARSEABLE"
              L2_ERRORS=$((L2_ERRORS + 1))
              reason_line="Could not parse verdict: ${verdict_line}"
              ;;
          esac

          log_info "  Layer 2: ${skill_basename} → ${verdict} (structural Δ${delta_display})"
          L2_RESULTS+=("| \`${skill_basename}\` | ${post_score} | ${delta_display} | ${post_valid} | ${verdict} | ${reason_line} |")
        fi
      fi
    fi

    rm -rf "$WORK_DIR"
  done

  # Tier summary in report
  {
    echo ""
    echo "## Tier Summary: ${tier}"
    echo ""
    echo "### Layer 1 (Structural)"
    echo ""
    echo "- **Pass**: ${TIER_PASS}"
    echo "- **Fail**: ${TIER_FAIL}"
    echo "- **Warnings**: ${TIER_WARN}"
    echo ""
    if [[ "$tier" == "strong" && "$TIER_FAIL" -gt 0 ]]; then
      echo "> **⚠ Strong-tier failure**: ${TIER_FAIL} strong-tier skill(s) have structural issues."
      echo "> Strong-tier skills should always be structurally valid. Investigate corpus integrity."
    fi
    if [[ "$tier" == "weak" ]]; then
      echo "> **Note**: Weak-tier skills are expected to have issues. These are targets for the meta-skill to fix."
    fi
    if [[ "$tier" == "adversarial" ]]; then
      echo "> **Note**: Adversarial-tier skills contain format traps, injection attempts, and contradictions."
      echo "> The meta-skill should handle these gracefully without producing worse output."
    fi

    # Layer 2 results
    if $LAYER2 && [[ ${#L2_RESULTS[@]} -gt 0 ]]; then
      echo ""
      echo "### Layer 2 (Meta-Skill Quality)"
      echo ""

      local l2_total=$((L2_IMPROVED_WINS + L2_ORIGINAL_WINS + L2_TIES + L2_ERRORS))
      local l2_judged=$((L2_IMPROVED_WINS + L2_ORIGINAL_WINS + L2_TIES))

      echo "| Skill File | Post-Score | Δ Score | Post-Valid | Verdict | Reason |"
      echo "|------------|-----------|---------|-----------|---------|--------|"
      for row in "${L2_RESULTS[@]}"; do
        echo "$row"
      done

      echo ""
      echo "**Layer 2 Summary:**"
      echo "- Improved wins: ${L2_IMPROVED_WINS}"
      echo "- Original wins: ${L2_ORIGINAL_WINS}"
      echo "- Ties: ${L2_TIES}"
      echo "- Errors: ${L2_ERRORS}"

      if [[ $l2_judged -gt 0 ]]; then
        # Compute win rate as percentage (bash integer math)
        local win_rate=$(( (L2_IMPROVED_WINS * 100) / l2_judged ))
        echo "- **Win rate**: ${win_rate}% (${L2_IMPROVED_WINS}/${l2_judged} judged cases)"

        if [[ $win_rate -ge 60 ]]; then
          echo ""
          echo "> ✅ Meta-skill \`${META_SKILL}\` is effective on ${tier}-tier corpus (win rate ≥ 60%)"
        elif [[ $win_rate -ge 40 ]]; then
          echo ""
          echo "> ⚠️ Meta-skill \`${META_SKILL}\` shows marginal improvement on ${tier}-tier corpus"
        else
          echo ""
          echo "> ❌ Meta-skill \`${META_SKILL}\` is not improving ${tier}-tier corpus skills (win rate < 40%)"
        fi
      fi
    elif $LAYER2; then
      echo ""
      echo "### Layer 2 (Meta-Skill Quality)"
      echo ""
      echo "No corpus fixtures were processed for Layer 2 evaluation."
    fi

    echo ""
    echo "---"
  } >> "$REPORT_FILE"

  TOTAL_PASS=$((TOTAL_PASS + TIER_PASS))
  TOTAL_FAIL=$((TOTAL_FAIL + TIER_FAIL))
  TOTAL_WARN=$((TOTAL_WARN + TIER_WARN))

  log_info "Tier '${tier}' report written to: ${REPORT_FILE}"
done

# --- Final summary ---
echo ""
echo "=============================="
echo " Corpus Evaluation Summary"
echo "=============================="
echo -e " Meta-skill: ${CYAN}${META_SKILL}${NC}"
echo -e " Tiers:      ${CYAN}${TIERS[*]}${NC}"
echo -e " Skills:     ${TOTAL_SKILLS}"
echo -e " Pass:       ${GREEN}${TOTAL_PASS}${NC}"
echo -e " Fail:       ${RED}${TOTAL_FAIL}${NC}"
echo -e " Warnings:   ${YELLOW}${TOTAL_WARN}${NC}"
echo ""

if [[ "$TOTAL_FAIL" -gt 0 ]]; then
  echo -e "${RED}FAIL${NC} — ${TOTAL_FAIL} failure(s)"
  exit 1
else
  echo -e "${GREEN}PASS${NC} — all checks passed (${TOTAL_WARN} warning(s))"
  exit 0
fi
