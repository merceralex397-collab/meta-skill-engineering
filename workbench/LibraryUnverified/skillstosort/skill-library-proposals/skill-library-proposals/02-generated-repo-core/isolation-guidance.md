---
name: isolation-guidance
display_name: Isolation Guidance
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - conversation
---

# Isolation Guidance

## Purpose
Chooses when work should use in-place edits versus worktrees, temp copies, or isolated lanes.

## Why it belongs
Helps prevent one large change from clobbering unrelated work.

## Suggested trigger situations
- when the user or repo work clearly points at isolation guidance
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around isolation guidance

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when isolation guidance would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `research-delegation`
- `local-git-specialist`
- `process-doctor`
- `planning`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
