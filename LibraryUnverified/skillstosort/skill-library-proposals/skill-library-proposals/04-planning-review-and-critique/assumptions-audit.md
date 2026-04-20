---
name: assumptions-audit
display_name: Assumptions Audit
category: Planning, Review, and Critique
priority: P0
status: candidate
sources:
  - invented
  - conversation
---

# Assumptions Audit

## Purpose
Extracts unstated assumptions from a plan, brief, architecture, or ticket and marks which ones are dangerous.

## Why it belongs
A strong anti-drift and anti-self-deception tool.

## Suggested trigger situations
- when the user or repo work clearly points at assumptions audit
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around assumptions audit

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when assumptions audit would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `red-team-challenge`
- `premortem`
- `contradiction-finder`
- `scope-pressure-test`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
