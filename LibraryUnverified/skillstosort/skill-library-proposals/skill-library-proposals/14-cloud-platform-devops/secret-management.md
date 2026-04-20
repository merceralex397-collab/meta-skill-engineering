---
name: secret-management
display_name: Secret Management
category: Cloud, Platform, and DevOps Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Secret Management

## Purpose
Handles environment variables, secret storage, rotation, and accidental exposure prevention.

## Why it belongs
Essential operational hygiene.

## Suggested trigger situations
- when the user or repo work clearly points at secret management
- when a task in the "Cloud, Platform, and DevOps Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around secret management

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when secret management would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `docker-containers`
- `terraform-iac`
- `serverless-patterns`
- `queues-cron-workers`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
