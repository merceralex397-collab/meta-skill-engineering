---
name: dataset-curation
display_name: Dataset Curation
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Dataset Curation

## Purpose
Selects, filters, deduplicates, and documents datasets for model work.

## Why it belongs
Data quality is foundational.

## Suggested trigger situations
- when the user or repo work clearly points at dataset curation
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around dataset curation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when dataset curation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `preference-optimization`
- `synthetic-data-generation`
- `data-cleaning-labeling`
- `model-architecture`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
