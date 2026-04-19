---
name: project-context
display_name: Project Context
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - research-docs
  - conversation
---

# Project Context

## Purpose
Loads the repo's source-of-truth documents in a deterministic order before planning or editing.

## Why it belongs
This is one of the strongest existing generated skills and should stay.

## Suggested trigger situations
- when the user or repo work clearly points at project context
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around project context

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when project context would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `repo-navigation`
- `stack-standards`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
