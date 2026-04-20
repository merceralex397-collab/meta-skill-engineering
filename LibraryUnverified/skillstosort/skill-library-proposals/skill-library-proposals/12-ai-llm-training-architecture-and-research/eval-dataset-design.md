---
name: eval-dataset-design
display_name: Eval Dataset Design
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Eval Dataset Design

## Purpose
Builds targeted evaluation sets that reflect the real task rather than generic benchmarks only.

## Why it belongs
Critical for serious model R&D.

## Suggested trigger situations
- when the user or repo work clearly points at eval dataset design
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around eval dataset design

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when eval dataset design would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `quantization-research`
- `inference-kernel-optimization`
- `benchmark-design`
- `safety-alignment`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
