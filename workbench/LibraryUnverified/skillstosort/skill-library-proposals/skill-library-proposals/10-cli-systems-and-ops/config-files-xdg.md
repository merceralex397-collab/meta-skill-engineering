---
name: config-files-xdg
display_name: Config Files and XDG
category: CLI, Systems, and Ops Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Config Files and XDG

## Purpose
Standardizes config paths, environment overrides, defaults, and user-editable state.

## Why it belongs
Good hygiene for local tools.

## Suggested trigger situations
- when the user or repo work clearly points at config files and xdg
- when a task in the "CLI, Systems, and Ops Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around config files and xdg

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when config files and xdg would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `cobra-go`
- `bubbletea-go`
- `packaging-installers`
- `cross-platform-shell`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
