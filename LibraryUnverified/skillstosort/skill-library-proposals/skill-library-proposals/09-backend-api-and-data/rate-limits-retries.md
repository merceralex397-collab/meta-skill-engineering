---
name: rate-limits-retries
display_name: Rate Limits and Retries
category: Backend, API, and Data Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Rate Limits and Retries

## Purpose
Explains client and server-side rate control, exponential backoff, and failure classification.

## Why it belongs
Useful around API consumption or hosted services.

## Suggested trigger situations
- when the user or repo work clearly points at rate limits and retries
- when a task in the "Backend, API, and Data Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around rate limits and retries

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when rate limits and retries would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `background-jobs-queues`
- `observability-logging`
- `webhooks-events`
- `realtime-websocket`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
