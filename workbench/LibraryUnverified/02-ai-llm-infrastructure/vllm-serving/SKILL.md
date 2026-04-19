---
name: vllm-serving
description: Handles vLLM-based serving, batching, throughput, and API integration where GPU or server setups justify it. Use this when the work involves models, inference, training, evaluation, or LLM system design or a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting. Do not use for ordinary software tasks with no model, inference, evaluation, or agent-runtime concerns.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: vllm-serving
  maturity: draft
  risk: low
  tags: [vllm, serving]
---

# Purpose

Handles vLLM-based serving, batching, throughput, and API integration where GPU or server setups justify it.

# When to use this skill

Use this skill when:

- the work involves models, inference, training, evaluation, or LLM system design
- a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- a plan, ticket, or repo state would benefit from explicit guardrails around vllm serving

# Do not use this skill when

- the task is really about ordinary software tasks with no model, inference, evaluation, or agent-runtime concerns
- If the task is more specifically about `ollama` or `llama-cpp`, prefer that skill instead.
- the relevant files, runtime, or deliverable type are already covered by a more specific active skill

# Operating procedure

1. Clarify the runtime goal, model boundaries, and interfaces involved in vLLM Serving.
2. Make schemas, prompt contracts, and tool surfaces explicit before iterating on behavior.
3. Constrain costs, latency, and failure fallbacks alongside quality goals.
4. Use representative eval or review cases instead of relying on one attractive demo.
5. Document the tradeoffs and next experiments needed to improve the system safely.

# Decision rules

- Make schemas and prompts serve the product boundary, not the other way around.
- Prefer measurable eval cases over intuition when runtime boundaries or eval coverage matter.
- Handle fallback and refusal paths explicitly.
- Do not hide cost or latency regressions behind quality anecdotes.

# Output requirements

1. `Runtime Context`
2. `Interfaces and Schemas`
3. `Safety or Cost Controls`
4. `Evaluation Plan`

# References

Read these only when relevant:

- `references/runtime-contracts.md`
- `references/eval-cases.md`
- `references/risk-controls.md`

# Related skills

- `ollama`
- `llama-cpp`
- `model-selection`
- `rag-retrieval`

# Failure handling

- If the scope is ambiguous, restate the decision boundary before proceeding.
- If the evidence is weak, say so explicitly and lower confidence instead of smoothing it over.
- If the task would be better served by a narrower skill, redirect to it rather than stretching this one.
