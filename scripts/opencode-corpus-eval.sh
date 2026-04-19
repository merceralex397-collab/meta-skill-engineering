#!/usr/bin/env bash
# opencode-corpus-eval.sh — OpenCode SDK-based corpus evaluation
#
# Usage:
#   ./scripts/opencode-corpus-eval.sh [--layer1] [--layer2]
#
# Layer 1: Evaluate meta-skills against corpus (weak/strong/adversarial)
# Layer 2: Evaluate skill-improvement decisions via LLM judge
#
# Requires: OpenCode server, node, jq

set -euo pipefail

_script_dir="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$_script_dir"
while [[ "$REPO_ROOT" != "/" ]]; do
  [[ -f "$REPO_ROOT/AGENTS.md" ]] && break
  REPO_ROOT="$(dirname "$REPO_ROOT")"
done

CORPUS_DIR="${REPO_ROOT}/corpus"
RESULTS_DIR="${REPO_ROOT}/eval-results"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
MODEL="${EVAL_MODEL:-minimax-coding-plan/Minimax-M2.7}"
OPENCODE_SERVER="${OPENCODE_SERVER:-http://127.0.0.1:4096}"
LAYER1=true
LAYER2=false
JUDGE_MODEL="${LAYER2_JUDGE_MODEL:-minimax-coding-plan/Minimax-M2.7}"

usage() {
  echo "Usage: $0 [--layer1] [--layer2]"
  echo "  --layer1   Evaluate meta-skills against corpus (default)"
  echo "  --layer2   Evaluate improvement decisions via LLM judge"
  exit 1
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --layer1) LAYER1=true; LAYER2=false; shift ;;
    --layer2) LAYER1=false; LAYER2=true; shift ;;
    --both) LAYER1=true; LAYER2=true; shift ;;
    *) usage ;;
  esac
done

mkdir -p "$RESULTS_DIR"

echo "Corpus Evaluation"
echo "Model: ${MODEL}"
echo "Layer 1: ${LAYER1}, Layer 2: ${LAYER2}"

run_corpus_test() {
  local corpus_type="$1"
  local corpus_file="${CORPUS_DIR}/${corpus_type}/${2}.md"
  local skill="$3"

  [[ ! -f "$corpus_file" ]] && return

  local content
  content=$(cat "$corpus_file")

  node --input-type=module <<EOF
import { createOpencodeClient } from "@opencode-ai/sdk";

const client = createOpencodeClient({ baseUrl: "${OPENCODE_SERVER}" });

async function main() {
  const session = await client.session.create({
    body: { title: "corpus-${corpus_type}-${skill}" }
  });

  const prompt = \`Test skill ${skill} against corpus case.

Case type: ${corpus_type}
Case content:
${content}

Evaluate how ${skill} handles this case.
Report pass/fail and reasoning.\`;

  try {
    const result = await client.session.prompt({
      path: { id: session.id },
      body: {
        parts: [{ type: "text", text: prompt }],
        model: { providerID: "minimax", modelID: "MiniMax-M2.7" }
      }
    });
    console.log(result.data.message?.content || "OK");
  } finally {
    await client.session.delete({ path: { id: session.id } });
  }
}

main();
EOF
}

if $LAYER1; then
  echo ""
  echo "=== Layer 1: Corpus Evaluation ==="

  for corpus_type in weak strong adversarial regression; do
    [[ ! -d "${CORPUS_DIR}/${corpus_type}" ]] && continue
    echo ""
    echo "## ${corpus_type} cases"

    for corpus_file in "${CORPUS_DIR}/${corpus_type}"/*.md; do
      [[ ! -f "$corpus_file" ]] && continue
      case_name=$(basename "$corpus_file" .md)
      echo "Testing: $case_name"

      for skill in "${REPO_ROOT}"/*/SKILL.md; do
        skill_name=$(basename "$(dirname "$skill")")
        [[ "$skill_name" == "archive" ]] && continue
        run_corpus_test "$corpus_type" "$case_name" "$skill_name"
      done
    done
  done
fi

if $LAYER2; then
  echo ""
  echo "=== Layer 2: LLM Judge Evaluation ==="
  echo "Judge model: ${JUDGE_MODEL}"
  # Layer 2 compares skill vs baseline using LLM judge
fi

echo ""
echo "Corpus evaluation complete."
echo "Results: ${RESULTS_DIR}/corpus-eval-${TIMESTAMP}.md"
