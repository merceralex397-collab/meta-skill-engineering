---
name: panel-of-experts
display_name: Panel of Experts
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Panel of Experts

## Purpose
Runs a structured multi-perspective pass over a problem before choosing a course of action.

## Why it belongs
You have repeatedly liked this style of reasoning.

## Suggested trigger situations
- when the user or repo work clearly points at panel of experts
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around panel of experts

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when panel of experts would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `parallel-lane-safety`
- `artifact-contracts`
- `verification-before-advance`
- `long-run-watchdog`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
