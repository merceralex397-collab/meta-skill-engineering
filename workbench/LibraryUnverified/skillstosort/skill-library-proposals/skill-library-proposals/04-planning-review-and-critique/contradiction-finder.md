---
name: contradiction-finder
display_name: Contradiction Finder
category: Planning, Review, and Critique
priority: P1
status: candidate
sources:
  - invented
  - conversation
---

# Contradiction Finder

## Purpose
Scans specs, tickets, decisions, and repo docs for conflicting statements or incompatible expectations.

## Why it belongs
Useful because your projects often span many docs.

## Suggested trigger situations
- when the user or repo work clearly points at contradiction finder
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around contradiction finder

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when contradiction finder would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `premortem`
- `assumptions-audit`
- `scope-pressure-test`
- `decision-packet-builder`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
