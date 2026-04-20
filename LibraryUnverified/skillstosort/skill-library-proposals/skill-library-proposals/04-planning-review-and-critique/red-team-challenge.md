---
name: red-team-challenge
display_name: Red-Team Challenge
category: Planning, Review, and Critique
priority: P0
status: candidate
sources:
  - invented
  - conversation
---

# Red-Team Challenge

## Purpose
Actively looks for weak points, exploitation paths, unsafe assumptions, and failure cases in a design or plan.

## Why it belongs
Pairs naturally with plan review.

## Suggested trigger situations
- when the user or repo work clearly points at red-team challenge
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around red-team challenge

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when red-team challenge would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `plan-review`
- `steelman`
- `premortem`
- `assumptions-audit`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
