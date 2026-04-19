---
name: skill-lifecycle-management
display_name: Skill Lifecycle Management
category: Meta Skill Engineering
priority: P2
status: candidate
sources:
  - invented
  - architecture-spec
---

# Skill Lifecycle Management

## Purpose
Manages skill creation, rollout, revision, deprecation, and archival states over time.

## Why it belongs
A durable library needs a lifecycle, not just a creation phase.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill lifecycle management

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill lifecycle management would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `skill-reference-extraction`
- `skill-anti-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
