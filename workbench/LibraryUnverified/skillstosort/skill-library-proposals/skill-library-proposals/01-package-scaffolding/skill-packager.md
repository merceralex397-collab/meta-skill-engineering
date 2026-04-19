---
name: skill-packager
display_name: Skill Packager
category: Package Scaffolding Skills
priority: P1
status: candidate
sources:
  - invented
  - architecture-spec
  - claude-skill-creator
---

# Skill Packager

## Purpose
Builds distributable bundles, manifests, and checksums for installing or sharing skills.

## Why it belongs
Useful if you want a portable library instead of raw markdown folders.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill packager

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill packager would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs
- **claude-skill-creator** — supported by the uploaded Claude skill creator document

## Related skills
- `skill-description-optimizer`
- `overlay-generator`
- `community-skill-harvester`
- `stack-profile-detector`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
