---
name: release-binaries
display_name: Release Binaries
category: CLI, Systems, and Ops Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Release Binaries

## Purpose
Packages compiled CLI artifacts, checksums, and release metadata for distribution.

## Why it belongs
Natural once a CLI matures.

## Suggested trigger situations
- when the user or repo work clearly points at release binaries
- when a task in the "CLI, Systems, and Ops Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around release binaries

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when release binaries would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `ssh-tmux-remote-workflow`
- `systemd-services`
- `proxmox-shell-scripting`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
