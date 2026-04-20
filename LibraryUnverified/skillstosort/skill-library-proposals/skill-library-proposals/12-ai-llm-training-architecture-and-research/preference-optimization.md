---
name: preference-optimization
display_name: Preference Optimization
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Preference Optimization

## Purpose
Handles preference data, ranking-based training, and alignment-style post-training methods.

## Why it belongs
A wider post-training family worth including.

## Suggested trigger situations
- when the user or repo work clearly points at preference optimization
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around preference optimization

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when preference optimization would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `fine-tuning`
- `instruction-tuning`
- `synthetic-data-generation`
- `dataset-curation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
