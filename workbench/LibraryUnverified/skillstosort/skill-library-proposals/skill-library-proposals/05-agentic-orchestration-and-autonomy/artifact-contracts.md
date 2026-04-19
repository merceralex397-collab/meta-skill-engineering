---
name: artifact-contracts
display_name: Artifact Contracts
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Artifact Contracts

## Purpose
Specifies what artifacts each stage must produce and what evidence they must contain.

## Why it belongs
Artifacts are the backbone of your workflow.

## Suggested trigger situations
- when the user or repo work clearly points at artifact contracts
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around artifact contracts

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when artifact contracts would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `autonomous-run-control`
- `parallel-lane-safety`
- `panel-of-experts`
- `verification-before-advance`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
