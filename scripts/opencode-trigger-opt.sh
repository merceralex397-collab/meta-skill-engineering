#!/usr/bin/env bash
# opencode-trigger-opt.sh — OpenCode SDK-based trigger optimization
#
# Usage:
#   ./scripts/opencode-trigger-opt.sh [skill-name]
#
# Requires: OpenCode server running, node, jq
#
# Replaces copilot CLI-based run-trigger-optimization.sh

set -euo pipefail

_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done

SKILL="${1:-}"
[[ -z "$SKILL" ]] && { echo "Usage: $0 [skill-name]"; exit 1; }

MODEL="${EVAL_MODEL:-minimax-coding-plan/Minimax-M2.7}"
OPENCODE_SERVER="${OPENCODE_SERVER:-http://127.0.0.1:4096}"

echo "Trigger optimization for ${SKILL}"
echo "Using model: ${MODEL}"
echo "Server: ${OPENCODE_SERVER}"

node --input-type=module <<EOF
import { createOpencodeClient } from "@opencode-ai/sdk";

const client = createOpencodeClient({ baseUrl: "${OPENCODE_SERVER}" });

async function main() {
  const session = await client.session.create({
    body: { title: "trigger-opt-${SKILL}" }
  });

  const prompt = \`Optimize triggers for skill ${SKILL}.

Read ${SKILL}/SKILL.md and its evals/trigger-positive.jsonl and evals/trigger-negative.jsonl.

Generate improved trigger phrases that:
1. Increase positive trigger rate (more accurate activation)
2. Maintain high negative rejection rate (avoid false triggers)
3. Are concise and specific

Output JSON:
{
  "current_triggers": ["..."],
  "proposed_triggers": ["..."],
  "rationale": "..."
}\`;

  try {
    const result = await client.session.prompt({
      path: { id: session.id },
      body: {
        parts: [{ type: "text", text: prompt }],
        model: { providerID: "minimax", modelID: "MiniMax-M2.7" },
        outputFormat: "json"
      }
    });
    console.log(JSON.stringify(result.data, null, 2));
  } catch (error) {
    console.error("Error:", error.message);
  } finally {
    await client.session.delete({ path: { id: session.id } });
  }
}

main();
EOF
