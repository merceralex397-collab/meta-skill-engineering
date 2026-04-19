---
name: skill-registry-manager
display_name: Skill Registry Manager
category: Package Scaffolding Skills
priority: P1
status: candidate
sources:
  - invented
  - architecture-spec
---

# Skill Registry Manager

## Purpose
Owns the library catalog, metadata, status, tags, and provenance for all skills in the ecosystem.

## Why it belongs
Once the library gets large, the registry becomes an asset in its own right.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill registry manager

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill registry manager would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `handoff-brief`
- `pr-review-ticket-bridge`
- `skill-eval-runner`
- `skill-description-optimizer`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
