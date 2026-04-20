---
name: premortem
display_name: Premortem
category: Planning, Review, and Critique
priority: P0
status: candidate
sources:
  - invented
  - conversation
---

# Premortem

## Purpose
Assumes the project failed and works backward to identify the most likely causes before work starts.

## Why it belongs
Very effective for avoiding obvious future pain.

## Suggested trigger situations
- when the user or repo work clearly points at premortem
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around premortem

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when premortem would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `steelman`
- `red-team-challenge`
- `assumptions-audit`
- `contradiction-finder`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
