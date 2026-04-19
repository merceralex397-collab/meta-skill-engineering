---
name: vllm-serving
display_name: vLLM Serving
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# vLLM Serving

## Purpose
Handles vLLM-based serving, batching, throughput, and API integration where GPU or server setups justify it.

## Why it belongs
Useful for comparison and scaling.

## Suggested trigger situations
- when the work involves models, inference, training, evaluation, or LLM system design
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around vllm serving

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when vllm serving would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `ollama`
- `llama-cpp`
- `model-selection`
- `rag-retrieval`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
