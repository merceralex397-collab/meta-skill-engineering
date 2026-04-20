---
name: gap-analysis
display_name: Gap Analysis
category: Planning, Review, and Critique
priority: P1
status: candidate
sources:
  - invented
  - conversation
---

# Gap Analysis

## Purpose
Compares current repo or plan state against target state and lists missing pieces cleanly.

## Why it belongs
Useful during retrofits and scaffold review.

## Suggested trigger situations
- when the user or repo work clearly points at gap analysis
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around gap analysis

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when gap analysis would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `blocker-extraction`
- `acceptance-criteria-hardening`
- `skeptic-pass`
- `failure-mode-analysis`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
