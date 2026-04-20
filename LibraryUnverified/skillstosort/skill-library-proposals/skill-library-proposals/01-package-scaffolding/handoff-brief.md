---
name: handoff-brief
display_name: Handoff Brief
category: Package Scaffolding Skills
priority: P0
status: candidate
sources:
  - scafforge-existing
  - conversation
---

# Handoff Brief

## Purpose
Builds a concise restart surface with actual state, next steps, and reading order.

## Why it belongs
Long-running or interrupted work needs a compact re-entry surface.

## Suggested trigger situations
- when the user or repo work clearly points at handoff brief
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around handoff brief

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when handoff brief would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **scafforge-existing** — already exists as a Scafforge package skill in the uploaded repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `agent-prompt-engineering`
- `repo-process-doctor`
- `pr-review-ticket-bridge`
- `skill-registry-manager`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
