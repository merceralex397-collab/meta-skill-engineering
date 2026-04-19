---
name: goal-decomposition
display_name: Goal Decomposition
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Goal Decomposition

## Purpose
Breaks broad goals into staged work units, subgoals, dependencies, and decision checkpoints.

## Why it belongs
A key planning primitive for autonomous systems.

## Suggested trigger situations
- when the user or repo work clearly points at goal decomposition
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around goal decomposition

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when goal decomposition would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `long-run-watchdog`
- `human-interrupt-handling`
- `manager-hierarchy-design`
- `swarm-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
