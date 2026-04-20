---
name: repo-navigation
display_name: Repo Navigation
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - research-docs
  - conversation
---

# Repo Navigation

## Purpose
Explains which directories are canonical, derived, preserved, or operational inside the repo.

## Why it belongs
Agents drift when they confuse generated surfaces with source material.

## Suggested trigger situations
- when the user or repo work clearly points at repo navigation
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around repo navigation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when repo navigation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `project-context`
- `stack-standards`
- `ticket-execution`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
