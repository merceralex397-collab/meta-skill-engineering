---
name: human-interrupt-handling
display_name: Human Interrupt Handling
category: Agentic Orchestration and Autonomy
priority: P2
status: candidate
sources:
  - invented
  - conversation
---

# Human Interrupt Handling

## Purpose
Explains how the system should react when the user injects new instructions mid-run without corrupting state.

## Why it belongs
Important because you often steer live work.

## Suggested trigger situations
- when the user or repo work clearly points at human interrupt handling
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around human interrupt handling

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when human interrupt handling would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `verification-before-advance`
- `long-run-watchdog`
- `goal-decomposition`
- `manager-hierarchy-design`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
