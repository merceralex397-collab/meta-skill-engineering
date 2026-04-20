---
name: skill-safety-review
display_name: Skill Safety Review
category: Meta Skill Engineering
priority: P2
status: candidate
sources:
  - invented
  - claude-skill-creator
  - architecture-spec
---

# Skill Safety Review

## Purpose
Checks a skill for hidden destructive behavior, misleading instructions, or excessive permissions.

## Why it belongs
Useful for any library that might be shared.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill safety review

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill safety review would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **claude-skill-creator** — supported by the uploaded Claude skill creator document
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `skill-provenance`
- `skill-variant-splitting`
- `skill-reference-extraction`
- `skill-anti-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
