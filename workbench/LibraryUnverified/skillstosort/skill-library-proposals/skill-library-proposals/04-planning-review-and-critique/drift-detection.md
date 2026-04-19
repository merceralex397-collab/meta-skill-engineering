---
name: drift-detection
display_name: Drift Detection
category: Planning, Review, and Critique
priority: P1
status: candidate
sources:
  - invented
  - conversation
---

# Drift Detection

## Purpose
Checks whether implementation or repo state has drifted away from the approved plan or canonical brief.

## Why it belongs
You explicitly want implementers not to drift.

## Suggested trigger situations
- when the user or repo work clearly points at drift detection
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around drift detection

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when drift detection would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `tradeoff-analysis`
- `architecture-review`
- `blocker-extraction`
- `acceptance-criteria-hardening`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
