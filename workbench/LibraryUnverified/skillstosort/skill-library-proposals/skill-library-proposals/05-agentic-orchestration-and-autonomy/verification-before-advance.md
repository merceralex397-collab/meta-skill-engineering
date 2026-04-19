---
name: verification-before-advance
display_name: Verification Before Advance
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Verification Before Advance

## Purpose
Requires proof that a stage is actually complete before moving to the next one.

## Why it belongs
This is a direct anti-drift control.

## Suggested trigger situations
- when the user or repo work clearly points at verification before advance
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around verification before advance

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when verification before advance would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `artifact-contracts`
- `panel-of-experts`
- `long-run-watchdog`
- `human-interrupt-handling`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
