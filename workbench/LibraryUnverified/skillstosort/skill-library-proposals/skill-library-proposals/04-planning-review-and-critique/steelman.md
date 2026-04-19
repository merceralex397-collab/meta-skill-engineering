---
name: steelman
display_name: Steelman
category: Planning, Review, and Critique
priority: P0
status: candidate
sources:
  - user-request
  - invented
---

# Steelman

## Purpose
Rebuilds an idea or plan in its strongest plausible form before judging it.

## Why it belongs
You explicitly asked for steelmanning.

## Suggested trigger situations
- when the user or repo work clearly points at steelman
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around steelman

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when steelman would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `plan-review`
- `red-team-challenge`
- `premortem`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
