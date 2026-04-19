#!/usr/bin/env bash
# opencode-full-cycle.sh — Full 5-step evaluation cadence
#
# Usage:
#   ./scripts/opencode-full-cycle.sh [skill-name]
#   ./scripts/opencode-full-cycle.sh --all
#
# Steps:
# 1. Run opencode-eval.sh (trigger + behavior tests)
# 2. Run opencode-corpus-eval.sh (corpus evaluation)
# 3. Run anti-patterns check
# 4. Apply improvements
# 5. Run trigger optimization
#
# Requires: OpenCode server, node, jq

set -euo pipefail

_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done

TARGETS=()
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
LOG_FILE="${REPO_ROOT}/eval-results/full-cycle-${TIMESTAMP}.log"

usage() {
  echo "Usage: $0 [--all | skill-name ...]"
  echo "  --all   Run full cycle on all skills"
  exit 1
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --all)
      for skill_dir in "${REPO_ROOT}"/*/; do
        skill=$(basename "$skill_dir")
        [[ "$skill" != "archive" ]] && TARGETS+=("$skill")
      done
      shift
      ;;
    *) TARGETS+=("$1"); shift ;;
  esac
done

[[ ${#TARGETS[@]} -eq 0 ]] && usage

echo "Full Evaluation Cycle"
echo "Skills: ${TARGETS[*]}"
echo "Log: $LOG_FILE"
echo ""

exec > >(tee -a "$LOG_FILE")
exec 2>&1

for skill in "${TARGETS[@]}"; do
  echo "═══════════════════════════════════════════"
  echo "Skill: $skill"
  echo "═══════════════════════════════════════════"

  echo ""
  echo "=== Step 1: Core Evaluation ==="
  "${REPO_ROOT}/scripts/opencode-eval.sh" "$skill" --runs 3

  echo ""
  echo "=== Step 2: Corpus Evaluation ==="
  "${REPO_ROOT}/scripts/opencode-corpus-eval.sh" --layer1

  echo ""
  echo "=== Step 3: Anti-Pattern Check ==="
  node --input-type=module <<EOF
import { createOpencodeClient } from "@opencode-ai/sdk";
const client = createOpencodeClient({ baseUrl: "${OPENCODE_SERVER:-http://127.0.0.1:4096}" });
const session = await client.session.create({ body: { title: "anti-pattern-${skill}" } });
const prompt = \`Run anti-pattern analysis on skill ${skill}.
Execute @skill-anti-patterns and report issues found.\`;
const result = await client.session.prompt({
  path: { id: session.id },
  body: { parts: [{ type: "text", text: prompt }], model: { providerID: "minimax", modelID: "MiniMax-M2.7" } }
});
console.log(result.data.message?.content || JSON.stringify(result.data));
await client.session.delete({ path: { id: session.id } });
EOF

  echo ""
  echo "=== Step 4: Apply Improvements ==="
  "${REPO_ROOT}/scripts/opencode-meta-cycle.sh" "$skill"

  echo ""
  echo "=== Step 5: Trigger Optimization ==="
  "${REPO_ROOT}/scripts/opencode-trigger-opt.sh" "$skill"

  echo ""
  echo "=== Cycle Complete for $skill ==="
done

echo ""
echo "═══════════════════════════════════════════"
echo "All cycles complete. See eval-results/"
echo "═══════════════════════════════════════════"
