#!/usr/bin/env bash
# opencode-meta-cycle.sh — OpenCode SDK-based meta-skill cycle orchestrator
#
# Usage:
#   ./scripts/opencode-meta-cycle.sh [skill-name]
#
# Requires: OpenCode server running, node
#
# Orchestrates the full meta-skill improvement cycle:
# skill-evaluation → skill-anti-patterns → skill-improver → skill-trigger-optimization

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

echo "Meta-Skill Cycle for ${SKILL}"
echo "Using model: ${MODEL}"

node --input-type=module <<EOF
import { createOpencodeClient } from "@opencode-ai/sdk";

const client = createOpencodeClient({ baseUrl: "${OPENCODE_SERVER}" });

async function runPhase(title, prompt) {
  console.log(\`\n=== \${title} ===\`);
  const session = await client.session.create({ body: { title } });
  
  try {
    const result = await client.session.prompt({
      path: { id: session.id },
      body: {
        parts: [{ type: "text", text: prompt }],
        model: { providerID: "minimax", modelID: "MiniMax-M2.7" }
      }
    });
    console.log(result.data.message?.content || JSON.stringify(result.data));
  } finally {
    await client.session.delete({ path: { id: session.id } });
  }
}

async function main() {
  const evalPrompt = \`Run evaluation on skill ${SKILL}.
Execute: ./scripts/opencode-eval.sh ${SKILL} --observe
Report pass/fail gates and specific failures.\`;

  const antiPatternPrompt = \`Run anti-pattern analysis on skill ${SKILL}.
Execute: @skill-anti-patterns for ${SKILL}
Report any structural issues found in SKILL.md.\`;

  const improvePrompt = \`Improve skill ${SKILL} based on eval failures.
Read eval-results/${SKILL}-eval.md and fix identified issues.
Apply fixes to SKILL.md directly.\`;

  const triggerOptPrompt = \`Optimize triggers for skill ${SKILL}.
Execute: ./scripts/opencode-trigger-opt.sh ${SKILL}
Apply any proposed trigger improvements.\`;

  await runPhase("Evaluation", evalPrompt);
  await runPhase("Anti-Pattern Analysis", antiPatternPrompt);
  await runPhase("Improvement", improvePrompt);
  await runPhase("Trigger Optimization", triggerOptPrompt);

  console.log("\n=== Meta-Skill Cycle Complete ===");
  console.log("Check eval-results/${SKILL}-eval.md for final results.");
}

main().catch(console.error);
EOF
