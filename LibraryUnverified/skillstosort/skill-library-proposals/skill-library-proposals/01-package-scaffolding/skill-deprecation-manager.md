---
name: skill-deprecation-manager
display_name: Skill Deprecation Manager
category: Package Scaffolding Skills
priority: P2
status: candidate
sources:
  - invented
  - architecture-spec
---

# Skill Deprecation Manager

## Purpose
Retires or merges obsolete skills while preserving backward references and library clarity.

## Why it belongs
Large libraries decay quickly without a retirement policy.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill deprecation manager

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill deprecation manager would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `stack-profile-detector`
- `migration-pack-builder`
- `provenance-audit`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
