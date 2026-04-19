---
name: agent-orchestration
display_name: Agent Orchestration
category: Agentic Orchestration and Autonomy
priority: P0
status: candidate
sources:
  - conversation
  - invented
---

# Agent Orchestration

## Purpose
Coordinates multiple agents, defines responsibility boundaries, and sequences collaborative work safely.

## Why it belongs
This is one of your longest-running interests.

## Suggested trigger situations
- when the user or repo work clearly points at agent orchestration
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around agent orchestration

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when agent orchestration would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `delegation-boundaries`
- `approval-gates`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
