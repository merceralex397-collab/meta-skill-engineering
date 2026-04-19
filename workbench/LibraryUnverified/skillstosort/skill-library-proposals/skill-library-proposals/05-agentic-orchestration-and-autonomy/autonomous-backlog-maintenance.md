---
name: autonomous-backlog-maintenance
display_name: Autonomous Backlog Maintenance
category: Agentic Orchestration and Autonomy
priority: P2
status: candidate
sources:
  - invented
  - conversation
---

# Autonomous Backlog Maintenance

## Purpose
Keeps the backlog clean by reclassifying stale, blocked, duplicated, or superseded tickets under defined rules.

## Why it belongs
Autonomy eventually creates maintenance work too.

## Suggested trigger situations
- when the user or repo work clearly points at autonomous backlog maintenance
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around autonomous backlog maintenance

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when autonomous backlog maintenance would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `collaboration-checkpoints`
- `multi-agent-debugging`
- `process-versioning`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
