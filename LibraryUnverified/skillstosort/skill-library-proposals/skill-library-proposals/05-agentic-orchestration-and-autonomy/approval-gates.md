---
name: approval-gates
display_name: Approval Gates
category: Agentic Orchestration and Autonomy
priority: P0
status: candidate
sources:
  - research-gap
  - invented
---

# Approval Gates

## Purpose
Introduces explicit stop/go gates before implementation, release, destructive mutation, or architectural pivots.

## Why it belongs
Your reports called out the absence of approval gates.

## Suggested trigger situations
- when the user or repo work clearly points at approval gates
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around approval gates

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when approval gates would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **invented** — newly invented extension proposed for this project

## Related skills
- `agent-orchestration`
- `delegation-boundaries`
- `session-resume-rehydration`
- `subagent-research-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
