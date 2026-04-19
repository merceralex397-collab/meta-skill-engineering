#!/usr/bin/env bash
# sync-to-skills.sh — Copy root scripts to per-skill scripts/ directories
#
# Root scripts/ is the source of truth (dev copies).
# Per-skill scripts/ directories contain live/deployed copies.
# Run this after editing any root script to propagate changes.
#
# Usage:
#   ./scripts/sync-to-skills.sh           # Sync all
#   ./scripts/sync-to-skills.sh --dry-run # Show what would be copied
#   ./scripts/sync-to-skills.sh --check   # Check for drift (exit 1 if out of sync)

set -euo pipefail

# Auto-detect repo root
_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[[ ! -f "$REPO_ROOT/AGENTS.md" ]] && { echo "Error: cannot find repo root (no AGENTS.md found)"; exit 1; }
ROOT_SCRIPTS="${REPO_ROOT}/scripts"
DRY_RUN=false
CHECK_ONLY=false

# --- Manifest: which root script goes to which skills ---
# Format: "script_name:skill1,skill2,skill3"
MANIFEST=(
  "run-evals.sh:skill-evaluation,skill-benchmarking,skill-creator,skill-testing-harness"
  "run-baseline-comparison.sh:skill-improver,skill-benchmarking"
  "check_skill_structure.py:skill-evaluation,skill-anti-patterns,skill-safety-review,skill-creator"
  "check_preservation.py:skill-improver"
  "validate-skills.sh:skill-safety-review,skill-creator"
  "skill_lint.py:skill-anti-patterns,skill-safety-review"
  "run-trigger-optimization.sh:skill-trigger-optimization"
  "init_eval_files.py:skill-testing-harness,skill-creator"
  "harvest_failures.py:skill-evaluation"
)

# Parse args
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run) DRY_RUN=true; shift ;;
    --check)   CHECK_ONLY=true; shift ;;
    --help|-h)
      echo "Usage: $0 [--dry-run | --check]"
      echo "  --dry-run  Show what would be copied without doing it"
      echo "  --check    Check for drift between root and per-skill copies (exit 1 if out of sync)"
      exit 0
      ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

COPIED=0
SKIPPED=0
DRIFTED=0
CREATED=0

for entry in "${MANIFEST[@]}"; do
  script_name="${entry%%:*}"
  skills_csv="${entry#*:}"
  root_file="${ROOT_SCRIPTS}/${script_name}"

  if [[ ! -f "$root_file" ]]; then
    echo "⚠  Root script not found: ${root_file}"
    continue
  fi

  IFS=',' read -ra skills <<< "$skills_csv"
  for skill in "${skills[@]}"; do
    skill_dir="${REPO_ROOT}/${skill}"
    target_dir="${skill_dir}/scripts"
    target_file="${target_dir}/${script_name}"

    if [[ ! -d "$skill_dir" ]]; then
      echo "⚠  Skill directory not found: ${skill}"
      continue
    fi

    if $CHECK_ONLY; then
      if [[ ! -f "$target_file" ]]; then
        echo "MISSING  ${skill}/scripts/${script_name}"
        DRIFTED=$((DRIFTED + 1))
      elif ! diff -q "$root_file" "$target_file" >/dev/null 2>&1; then
        echo "DRIFTED  ${skill}/scripts/${script_name}"
        DRIFTED=$((DRIFTED + 1))
      else
        SKIPPED=$((SKIPPED + 1))
      fi
      continue
    fi

    # Check if already identical
    if [[ -f "$target_file" ]] && diff -q "$root_file" "$target_file" >/dev/null 2>&1; then
      SKIPPED=$((SKIPPED + 1))
      continue
    fi

    if $DRY_RUN; then
      if [[ -f "$target_file" ]]; then
        echo "UPDATE   ${skill}/scripts/${script_name}"
      else
        echo "CREATE   ${skill}/scripts/${script_name}"
      fi
      COPIED=$((COPIED + 1))
      continue
    fi

    # Create target directory if needed
    if [[ ! -d "$target_dir" ]]; then
      mkdir -p "$target_dir"
      CREATED=$((CREATED + 1))
    fi

    cp "$root_file" "$target_file"
    chmod --reference="$root_file" "$target_file" 2>/dev/null || true
    echo "✓  ${skill}/scripts/${script_name}"
    COPIED=$((COPIED + 1))
  done
done

echo ""
if $CHECK_ONLY; then
  echo "Check complete: ${DRIFTED} drifted, ${SKIPPED} in sync"
  if [[ $DRIFTED -gt 0 ]]; then
    echo "Run ./scripts/sync-to-skills.sh to fix drift"
    exit 1
  fi
  exit 0
fi

if $DRY_RUN; then
  echo "Dry run: ${COPIED} would be copied, ${SKIPPED} already in sync"
else
  echo "Sync complete: ${COPIED} copied, ${SKIPPED} already in sync, ${CREATED} directories created"
fi
