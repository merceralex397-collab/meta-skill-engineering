---
name: webhooks-events
display_name: Webhooks and Events
category: Backend, API, and Data Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Webhooks and Events

## Purpose
Handles inbound event verification, idempotency, fanout, and asynchronous processing patterns.

## Why it belongs
Good for integrative backends.

## Suggested trigger situations
- when the user or repo work clearly points at webhooks and events
- when a task in the "Backend, API, and Data Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around webhooks and events

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when webhooks and events would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `observability-logging`
- `rate-limits-retries`
- `realtime-websocket`
- `api-debugging`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
