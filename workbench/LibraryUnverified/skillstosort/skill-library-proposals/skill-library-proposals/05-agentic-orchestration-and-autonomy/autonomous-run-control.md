---
name: autonomous-run-control
display_name: Autonomous Run Control
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Autonomous Run Control

## Purpose
Defines how autonomous runs start, checkpoint, pause, recover, and terminate safely.

## Why it belongs
You explicitly asked for something that keeps the AI working autonomously.

## Suggested trigger situations
- when the user or repo work clearly points at autonomous run control
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around autonomous run control

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when autonomous run control would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `subagent-research-patterns`
- `workflow-state-memory`
- `parallel-lane-safety`
- `artifact-contracts`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
