---
name: workflow-state-memory
display_name: Workflow State Memory
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Workflow State Memory

## Purpose
Stores durable workflow state explicitly so agents do not rely on hazy conversational memory.

## Why it belongs
Needed for autonomy that lasts beyond one session.

## Suggested trigger situations
- when the user or repo work clearly points at workflow state memory
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around workflow state memory

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when workflow state memory would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `session-resume-rehydration`
- `subagent-research-patterns`
- `autonomous-run-control`
- `parallel-lane-safety`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
