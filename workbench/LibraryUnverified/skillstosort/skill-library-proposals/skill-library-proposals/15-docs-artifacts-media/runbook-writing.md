---
name: runbook-writing
display_name: Runbook Writing
category: Docs, Artifacts, and Media Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Runbook Writing

## Purpose
Creates operational guides for setup, recovery, diagnosis, and routine procedures.

## Why it belongs
Valuable for long-lived systems.

## Suggested trigger situations
- when the user or repo work clearly points at runbook writing
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around runbook writing

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when runbook writing would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `research-synthesis`
- `adr-rfc-writing`
- `release-notes`
- `spec-authoring`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
