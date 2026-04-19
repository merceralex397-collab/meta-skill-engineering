---
name: packaging-installers
display_name: Packaging and Installers
category: CLI, Systems, and Ops Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Packaging and Installers

## Purpose
Handles distribution methods, install scripts, path setup, and uninstallation expectations for developer tools.

## Why it belongs
Useful for shareable CLI utilities.

## Suggested trigger situations
- when the user or repo work clearly points at packaging and installers
- when a task in the "CLI, Systems, and Ops Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around packaging and installers

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when packaging and installers would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `bubbletea-go`
- `config-files-xdg`
- `cross-platform-shell`
- `linux-ubuntu-ops`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
