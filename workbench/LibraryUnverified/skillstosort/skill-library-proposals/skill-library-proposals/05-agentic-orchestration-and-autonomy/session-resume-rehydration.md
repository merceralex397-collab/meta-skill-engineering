---
name: session-resume-rehydration
display_name: Session Resume Rehydration
category: Agentic Orchestration and Autonomy
priority: P0
status: candidate
sources:
  - conversation
  - invented
---

# Session Resume Rehydration

## Purpose
Restores enough project state for work to resume cleanly after interruption, host change, or model swap.

## Why it belongs
Critical for long-running multi-session work.

## Suggested trigger situations
- when the user or repo work clearly points at session resume rehydration
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around session resume rehydration

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when session resume rehydration would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `delegation-boundaries`
- `approval-gates`
- `subagent-research-patterns`
- `workflow-state-memory`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
