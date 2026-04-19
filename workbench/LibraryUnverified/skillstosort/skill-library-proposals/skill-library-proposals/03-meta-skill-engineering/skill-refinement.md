---
name: skill-refinement
display_name: Skill Refinement
category: Meta Skill Engineering
priority: P0
status: candidate
sources:
  - user-request
  - invented
  - claude-skill-creator
---

# Skill Refinement

## Purpose
Tightens, clarifies, and de-bloats an existing skill after real usage reveals rough edges.

## Why it belongs
You explicitly asked for this and the creator docs support it.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill refinement

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill refinement would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project
- **claude-skill-creator** — supported by the uploaded Claude skill creator document

## Related skills
- `skill-creator`
- `skill-adaptation`
- `skill-evaluation`
- `skill-testing-harness`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
