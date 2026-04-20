---
name: model-selection
display_name: Model Selection
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Model Selection

## Purpose
Chooses a model based on task shape, hardware, latency, cost, context, and risk tolerance.

## Why it belongs
A common recurring decision for you.

## Suggested trigger situations
- when the user or repo work clearly points at model selection
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around model selection

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when model selection would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `llama-cpp`
- `vllm-serving`
- `rag-retrieval`
- `embeddings-indexing`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
