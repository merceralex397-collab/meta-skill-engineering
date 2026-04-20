---
name: background-jobs-queues
display_name: Background Jobs and Queues
category: Backend, API, and Data Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Background Jobs and Queues

## Purpose
Handles deferred work, retries, idempotency, schedule, and worker boundaries.

## Why it belongs
Useful for serious backends.

## Suggested trigger situations
- when the user or repo work clearly points at background jobs and queues
- when a task in the "Backend, API, and Data Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around background jobs and queues

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when background jobs and queues would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `sqlite`
- `orm-patterns`
- `observability-logging`
- `rate-limits-retries`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
