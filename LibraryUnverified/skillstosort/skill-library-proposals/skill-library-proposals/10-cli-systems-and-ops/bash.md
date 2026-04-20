---
name: bash
display_name: Bash
category: CLI, Systems, and Ops Skills
priority: P1
status: candidate
sources:
  - conversation
  - research-docs
---

# Bash

## Purpose
Applies shell scripting safety, quoting, error handling, and portable command composition.

## Why it belongs
You regularly ask shell and Ubuntu questions.

## Suggested trigger situations
- when the user or repo work clearly points at bash
- when a task in the "CLI, Systems, and Ops Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around bash

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when bash would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns

## Related skills
- `cli-development-go`
- `cli-development-python`
- `terminal-debugging`
- `tui-development`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
