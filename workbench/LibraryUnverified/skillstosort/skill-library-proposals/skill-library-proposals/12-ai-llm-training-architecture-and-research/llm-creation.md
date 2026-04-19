---
name: llm-creation
display_name: LLM Creation
category: AI / LLM Training, Architecture, and Research Skills
priority: P0
status: candidate
sources:
  - user-request
  - conversation
  - invented
---

# LLM Creation

## Purpose
Covers the broader end-to-end process of creating an LLM system from tokenizer through training and serving.

## Why it belongs
You explicitly asked for LLM creation.

## Suggested trigger situations
- when the work involves models, inference, training, evaluation, or LLM system design
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around llm creation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when llm creation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `data-cleaning-labeling`
- `model-architecture`
- `tokenizer-design`
- `pretraining-pipeline`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
