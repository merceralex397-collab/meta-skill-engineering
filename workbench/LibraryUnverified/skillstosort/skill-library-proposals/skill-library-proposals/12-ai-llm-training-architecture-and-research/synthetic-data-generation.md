---
name: synthetic-data-generation
display_name: Synthetic Data Generation
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Synthetic Data Generation

## Purpose
Creates or critiques synthetic datasets for training, evals, or augmentation under clear quality controls.

## Why it belongs
Useful because dataset acquisition is often the bottleneck.

## Suggested trigger situations
- when the user or repo work clearly points at synthetic data generation
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around synthetic data generation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when synthetic data generation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `instruction-tuning`
- `preference-optimization`
- `dataset-curation`
- `data-cleaning-labeling`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
