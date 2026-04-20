---
name: skill-creator
display_name: Skill Creator
category: Meta Skill Engineering
priority: P0
status: candidate
sources:
  - external-example
  - claude-skill-creator
  - skill-creator-package
  - conversation
---

# Skill Creator

## Purpose
Creates new skills from user intent, then iterates through testing and improvement loops.

## Why it belongs
This is explicitly present in your source files and should be part of the library.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill creator

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill creator would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports
- **claude-skill-creator** — supported by the uploaded Claude skill creator document
- **skill-creator-package** — present in the uploaded skill-creator package
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `skill-adaptation`
- `skill-refinement`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
