---
name: skill-adaptation
display_name: Skill Adaptation
category: Meta Skill Engineering
priority: P0
status: candidate
sources:
  - user-request
  - invented
---

# Skill Adaptation

## Purpose
Adapts a general skill so it fits one repo, team, or project profile without losing the base pattern.

## Why it belongs
You explicitly asked for this.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill adaptation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill adaptation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `skill-creator`
- `skill-refinement`
- `skill-evaluation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
