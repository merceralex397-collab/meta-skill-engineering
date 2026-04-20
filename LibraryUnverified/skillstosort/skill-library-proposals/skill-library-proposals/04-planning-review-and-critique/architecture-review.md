---
name: architecture-review
display_name: Architecture Review
category: Planning, Review, and Critique
priority: P1
status: candidate
sources:
  - invented
  - conversation
---

# Architecture Review

## Purpose
Evaluates whether the system shape, boundaries, dependencies, and responsibilities actually make sense.

## Why it belongs
A natural fit for your multi-component systems.

## Suggested trigger situations
- when the user or repo work clearly points at architecture review
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around architecture review

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when architecture review would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `decision-packet-builder`
- `tradeoff-analysis`
- `drift-detection`
- `blocker-extraction`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
