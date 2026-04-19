---
name: database-persistence
display_name: Database Persistence
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - research-gap
  - conversation
---

# Database Persistence

## Purpose
Covers schema design, migrations, query patterns, and persistence tradeoffs for the chosen data layer.

## Why it belongs
This is one of the most obvious missing domains in the current scaffolded packs.

## Suggested trigger situations
- when the user or repo work clearly points at database persistence
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around database persistence

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when database persistence would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `testing`
- `deployment-pipeline`
- `auth-patterns`
- `api-schema`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
