---
name: instruction-tuning
display_name: Instruction Tuning
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Instruction Tuning

## Purpose
Builds or evaluates instruction-style supervised fine-tuning datasets and training runs.

## Why it belongs
A natural sub-skill beneath fine-tuning.

## Suggested trigger situations
- when the work involves models, inference, training, evaluation, or LLM system design
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around instruction tuning

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when instruction tuning would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `fine-tuning`
- `preference-optimization`
- `synthetic-data-generation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
