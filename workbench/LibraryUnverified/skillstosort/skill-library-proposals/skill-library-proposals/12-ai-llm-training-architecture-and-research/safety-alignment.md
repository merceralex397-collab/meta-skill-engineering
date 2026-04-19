---
name: safety-alignment
display_name: Safety and Alignment
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Safety and Alignment

## Purpose
Covers risk shaping, refusal behavior, post-training controls, and alignment tradeoffs for model systems.

## Why it belongs
Important if the system gains power.

## Suggested trigger situations
- when the user or repo work clearly points at safety and alignment
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around safety and alignment

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when safety and alignment would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `eval-dataset-design`
- `benchmark-design`
- `reward-modeling`
- `model-merging`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
