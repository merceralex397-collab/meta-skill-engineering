---
name: tokenizer-design
display_name: Tokenizer Design
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Tokenizer Design

## Purpose
Handles vocabulary choice, tokenization strategy, domain fit, and measurement of token efficiency.

## Why it belongs
A real sub-area of model creation.

## Suggested trigger situations
- when the work involves models, inference, training, evaluation, or LLM system design
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around tokenizer design

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when tokenizer design would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `model-architecture`
- `llm-creation`
- `pretraining-pipeline`
- `moe-architecture`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
