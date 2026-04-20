---
name: skill-authoring
display_name: Skill Authoring
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - research-gap
  - conversation
  - invented
---

# Skill Authoring

## Purpose
Creates or updates project-local skills inside a generated repo using the repo's own conventions and evidence.

## Why it belongs
Necessary if repos are supposed to evolve their own local intelligence.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill authoring

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill authoring would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `process-doctor`
- `planning`
- `prompt-crafting`
- `testing`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
