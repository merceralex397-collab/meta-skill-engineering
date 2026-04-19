#!/usr/bin/env bash
set -euo pipefail

# validate-skills.sh — Structural validation for all skill packages
# Checks: frontmatter, cross-references, phantom files, eval format, line counts

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Auto-detect repo root
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root (no AGENTS.md found)"; exit 1; }
ERRORS=0
WARNINGS=0
SKILLS_CHECKED=0

log_error() { echo -e "${RED}  ✗ $1${NC}"; ERRORS=$((ERRORS + 1)); }
log_warn()  { echo -e "${YELLOW}  ⚠ $1${NC}"; WARNINGS=$((WARNINGS + 1)); }
log_ok()    { echo -e "${GREEN}  ✓ $1${NC}"; }

# Collect all skill directories (contain SKILL.md at root level)
SKILL_DIRS=()
for dir in "$REPO_ROOT"/*/; do
  [[ -f "$dir/SKILL.md" ]] && SKILL_DIRS+=("$dir")
done

echo "=== Skill Package Validator ==="
echo "Found ${#SKILL_DIRS[@]} skill packages"
echo ""

for skill_dir in "${SKILL_DIRS[@]}"; do
  skill_name=$(basename "$skill_dir")
  echo "--- $skill_name ---"
  SKILLS_CHECKED=$((SKILLS_CHECKED + 1))

  skill_md="$skill_dir/SKILL.md"

  # 1. Frontmatter check
  if head -1 "$skill_md" | grep -q '^---$'; then
    # Check name field
    fm_name=$(sed -n '/^---$/,/^---$/p' "$skill_md" | grep '^name:' | head -1 | sed 's/^name: *//')
    if [[ -z "$fm_name" ]]; then
      log_error "Missing 'name' in frontmatter"
    elif [[ "$fm_name" != "$skill_name" ]]; then
      log_error "Frontmatter name '$fm_name' does not match directory '$skill_name'"
    else
      log_ok "Frontmatter name matches directory"
    fi

    # Check description field
    if sed -n '/^---$/,/^---$/p' "$skill_md" | grep -q '^description:'; then
      log_ok "Has description"
    else
      log_error "Missing 'description' in frontmatter"
    fi

    # Check for unexpected frontmatter fields (contract: name and description only)
    unexpected_fields=$(sed -n '/^---$/,/^---$/p' "$skill_md" | grep -E '^[a-zA-Z_-]+:' | grep -v '^name:' | grep -v '^description:' | grep -v '^---$' | sed 's/:.*//' || true)
    if [[ -n "$unexpected_fields" ]]; then
      for field in $unexpected_fields; do
        log_warn "Unexpected frontmatter field '$field' (contract allows only name, description)"
      done
    fi
  else
    log_error "Missing YAML frontmatter (no opening ---)"
  fi

  # 2. Canonical section headings check
  required_headings=("Purpose" "When to use" "When NOT to use" "Procedure" "Output contract" "Failure handling")
  for heading in "${required_headings[@]}"; do
    if ! grep -q "^# ${heading}" "$skill_md"; then
      log_warn "Missing canonical heading: # ${heading}"
    fi
  done

  # 3. Stale manifest.yaml check
  if [[ -f "$skill_dir/manifest.yaml" ]]; then
    log_warn "manifest.yaml found (should be removed — distribution artifact)"
  fi

  # 4. Line count check (spec recommends <500)
  line_count=$(wc -l < "$skill_md")
  if (( line_count > 500 )); then
    log_warn "SKILL.md is $line_count lines (recommended <500)"
  else
    log_ok "SKILL.md is $line_count lines"
  fi

  # 5. Cross-reference validation (check skill references point to existing dirs)
  while IFS= read -r ref_skill; do
    ref_dir="$REPO_ROOT/$ref_skill"
    if [[ ! -d "$ref_dir" ]] || [[ ! -f "$ref_dir/SKILL.md" ]]; then
      log_error "References non-existent skill: $ref_skill"
    fi
  done < <(grep -oP '(?<=→ `|→ \*\*|use )[a-z][-a-z]*(?=`|\*\*|\))' "$skill_md" 2>/dev/null | grep '^skill-\|^community-' | sort -u || true)

  # 6. Phantom file references (references/ and scripts/ that don't exist)
  while IFS= read -r ref_path; do
    full_path="$skill_dir/$ref_path"
    if [[ ! -e "$full_path" ]]; then
      log_warn "References non-existent file: $ref_path"
    fi
  done < <(grep -oP '(?:references|scripts)/[a-zA-Z0-9._-]+\.[a-z]+' "$skill_md" 2>/dev/null | sort -u || true)

  # 7. Eval directory check
  if [[ -d "$skill_dir/evals" ]]; then
    tp_count=0; tn_count=0; bh_count=0
    [[ -f "$skill_dir/evals/trigger-positive.jsonl" ]] && tp_count=$(wc -l < "$skill_dir/evals/trigger-positive.jsonl")
    [[ -f "$skill_dir/evals/trigger-negative.jsonl" ]] && tn_count=$(wc -l < "$skill_dir/evals/trigger-negative.jsonl")
    [[ -f "$skill_dir/evals/behavior.jsonl" ]] && bh_count=$(wc -l < "$skill_dir/evals/behavior.jsonl")

    if (( tp_count >= 3 )); then
      log_ok "trigger-positive.jsonl: $tp_count cases"
    else
      log_warn "trigger-positive.jsonl: $tp_count cases (recommend ≥8)"
    fi

    if (( tn_count >= 3 )); then
      log_ok "trigger-negative.jsonl: $tn_count cases"
    else
      log_warn "trigger-negative.jsonl: $tn_count cases (recommend ≥8)"
    fi

    if (( bh_count >= 1 )); then
      log_ok "behavior.jsonl: $bh_count cases"
    else
      log_warn "No behavior.jsonl"
    fi

    # Validate JSONL format
    for jsonl_file in "$skill_dir"/evals/*.jsonl; do
      [[ -f "$jsonl_file" ]] || continue
      fname=$(basename "$jsonl_file")
      while IFS= read -r line; do
        if ! echo "$line" | python3 -c "import sys,json; json.load(sys.stdin)" 2>/dev/null; then
          log_error "$fname contains invalid JSON line"
          break
        fi
      done < "$jsonl_file"
    done
  else
    log_warn "No evals/ directory"
  fi

  echo ""
done

echo "=== Summary ==="
echo "Skills checked: $SKILLS_CHECKED"
echo -e "Errors: ${RED}$ERRORS${NC}"
echo -e "Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

if (( ERRORS > 0 )); then
  echo -e "${RED}FAIL${NC} — $ERRORS error(s) found"
  exit 1
else
  echo -e "${GREEN}PASS${NC} — no errors ($WARNINGS warning(s))"
  exit 0
fi
