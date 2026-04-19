---
name: systemd-services
display_name: systemd Services
category: CLI, Systems, and Ops Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# systemd Services

## Purpose
Creates and maintains systemd units, restart behavior, logging expectations, and safe service deployment.

## Why it belongs
Useful for headless servers.

## Suggested trigger situations
- when the user or repo work clearly points at systemd services
- when a task in the "CLI, Systems, and Ops Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around systemd services

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when systemd services would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `linux-ubuntu-ops`
- `ssh-tmux-remote-workflow`
- `release-binaries`
- `proxmox-shell-scripting`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
