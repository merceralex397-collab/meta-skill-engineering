---
name: release-notes
display_name: Release Notes
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Release Notes

## Purpose
Writes useful release notes that summarize what changed, who it affects, and what to verify.

## Why it belongs
Helpful once you ship artifacts.

## Suggested trigger situations
- when the user or repo work clearly points at release notes
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around release notes

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when release notes would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `adr-rfc-writing`
- `runbook-writing`
- `spec-authoring`
- `meeting-notes-decision-log`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
