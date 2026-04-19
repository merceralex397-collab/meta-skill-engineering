---
name: external-api-client
display_name: External API Client
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - research-gap
  - conversation
---

# External API Client

## Purpose
Standardizes retries, backoff, timeout, idempotency, and SDK-versus-raw-call decisions when consuming third-party APIs.

## Why it belongs
This is a recurring source of subtle bugs and cost leaks.

## Suggested trigger situations
- when the user or repo work clearly points at external api client
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around external api client

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when external api client would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `auth-patterns`
- `api-schema`
- `error-handling`
- `performance-baseline`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
