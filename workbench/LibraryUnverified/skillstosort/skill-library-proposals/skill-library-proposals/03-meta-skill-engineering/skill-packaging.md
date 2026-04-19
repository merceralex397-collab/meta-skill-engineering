---
name: skill-packaging
display_name: Skill Packaging
category: Meta Skill Engineering
priority: P1
status: candidate
sources:
  - invented
  - architecture-spec
  - claude-skill-creator
---

# Skill Packaging

## Purpose
Packages a skill into installable outputs and validates bundle contents.

## Why it belongs
Needed if you want portable artifacts rather than a loose folder.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill packaging

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill packaging would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs
- **claude-skill-creator** — supported by the uploaded Claude skill creator document

## Related skills
- `skill-trigger-optimization`
- `skill-benchmarking`
- `skill-installation`
- `skill-catalog-curation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
