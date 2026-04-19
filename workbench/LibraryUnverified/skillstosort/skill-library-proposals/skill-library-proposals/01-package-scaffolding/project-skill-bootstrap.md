---
name: project-skill-bootstrap
display_name: Project Skill Bootstrap
category: Package Scaffolding Skills
priority: P0
status: candidate
sources:
  - scafforge-existing
  - conversation
  - research-gap
---

# Project Skill Bootstrap

## Purpose
Synthesizes project-local skills from the brief, stack, and domain signals instead of shipping only generic placeholders.

## Why it belongs
This is the main answer to the 'bland skills' problem.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around project skill bootstrap

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when project skill bootstrap would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **scafforge-existing** — already exists as a Scafforge package skill in the uploaded repo
- **conversation** — mentioned or implied in this conversation/project context
- **research-gap** — called out as missing or underdeveloped in the uploaded reports

## Related skills
- `opencode-team-bootstrap`
- `ticket-pack-builder`
- `agent-prompt-engineering`
- `repo-process-doctor`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
