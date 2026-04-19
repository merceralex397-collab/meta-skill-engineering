---
name: long-run-watchdog
display_name: Long-Run Watchdog
category: Agentic Orchestration and Autonomy
priority: P2
status: candidate
sources:
  - invented
  - conversation
---

# Long-Run Watchdog

## Purpose
Monitors autonomous workflows for loops, inactivity, repeated errors, or missing outputs and intervenes with a bounded recovery policy.

## Why it belongs
Useful for any long-lived agent run.

## Suggested trigger situations
- when the user or repo work clearly points at long-run watchdog
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around long-run watchdog

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when long-run watchdog would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `panel-of-experts`
- `verification-before-advance`
- `human-interrupt-handling`
- `goal-decomposition`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
