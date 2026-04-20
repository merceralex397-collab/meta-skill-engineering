---
name: parallel-lane-safety
display_name: Parallel Lane Safety
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Parallel Lane Safety

## Purpose
Determines which tickets or change sets can safely proceed in parallel and which ones must serialize.

## Why it belongs
Useful for swarm-style work.

## Suggested trigger situations
- when the user or repo work clearly points at parallel lane safety
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around parallel lane safety

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when parallel lane safety would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `workflow-state-memory`
- `autonomous-run-control`
- `artifact-contracts`
- `panel-of-experts`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
