#!/usr/bin/env bash
# run-meta-skill-cycle.sh — OPTIONAL / EXPERIMENTAL
#
# Runs copilot in non-interactive mode with all permissions to execute
# the meta-skill orchestrator against the repository's own skill packages.
#
# ⚠️  This script is NOT part of the core evaluation path (run-full-cycle.sh).
# It requires an external skill not included in this repository.
#
# Prerequisites:
#   - copilot CLI installed and authenticated
#   - meta-skill-orchestrator skill installed in ~/.copilot/skills/
#     (this skill is NOT part of this repository — obtain it separately)
#
# Usage: ./scripts/run-meta-skill-cycle.sh [cycle_number]
#   cycle_number: optional, defaults to 1. Used for labeling the output.

set -euo pipefail

CYCLE="${1:-1}"
MODEL="${EVAL_MODEL:-claude-opus-4.6}"
REPO_DIR="$(cd "$(dirname "$0")/.." && pwd)"
LOG_DIR="${REPO_DIR}/tasks/worklogs"

echo "=== Meta-Skill Orchestrator Cycle ${CYCLE} ==="
echo "Repository: ${REPO_DIR}"
echo "Model: ${MODEL}"
echo "Started: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""

mkdir -p "${LOG_DIR}"

cd "${REPO_DIR}"

# Run copilot in non-interactive mode with full permissions
# The prompt invokes the meta-skill-orchestrator entry point skill
copilot -p "You have the meta-skill-orchestrator skill installed. Invoke it now to run a full quality-improvement cycle (cycle ${CYCLE}) against all skill packages in this repository. Follow the orchestrator's procedure exactly through all 7 phases. Apply improvements directly to SKILL.md files where warranted. Create the cycle report at tasks/worklogs/orchestrator-cycle-${CYCLE}-report.md. Commit all changes with message 'cycle-${CYCLE}: meta-skill orchestrator improvements' and the Co-authored-by trailer for Copilot." \
  --model "$MODEL" \
  --reasoning-effort high \
  --allow-all \
  --autopilot \
  2>&1 | tee "${LOG_DIR}/cycle-${CYCLE}-raw-output.log"

EXIT_CODE=${PIPESTATUS[0]}

echo ""
echo "=== Cycle ${CYCLE} Complete ==="
echo "Exit code: ${EXIT_CODE}"
echo "Finished: $(date -u +%Y-%m-%dT%H:%M:%SZ)"

if [ -f "${LOG_DIR}/orchestrator-cycle-${CYCLE}-report.md" ]; then
  echo "Report generated: ${LOG_DIR}/orchestrator-cycle-${CYCLE}-report.md"
elif [ -f "${LOG_DIR}/orchestrator-cycle-report.md" ]; then
  echo "Report generated: ${LOG_DIR}/orchestrator-cycle-report.md"
  mv "${LOG_DIR}/orchestrator-cycle-report.md" "${LOG_DIR}/orchestrator-cycle-${CYCLE}-report.md"
  echo "Renamed to: orchestrator-cycle-${CYCLE}-report.md"
else
  echo "WARNING: No orchestrator report found. Check raw output log."
fi

exit ${EXIT_CODE}
