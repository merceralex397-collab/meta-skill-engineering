---
name: sqlite
display_name: SQLite
category: Backend, API, and Data Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# SQLite

## Purpose
Defines file-backed persistence, migration, concurrency, and testing patterns for SQLite.

## Why it belongs
Relevant to lightweight local systems.

## Suggested trigger situations
- when the user or repo work clearly points at sqlite
- when a task in the "Backend, API, and Data Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around sqlite

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when sqlite would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `go-api-service`
- `postgresql`
- `orm-patterns`
- `background-jobs-queues`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
