---
name: skill-catalog-curation
display_name: Skill Catalog Curation
category: Meta Skill Engineering
priority: P1
status: candidate
sources:
  - invented
  - conversation
---

# Skill Catalog Curation

## Purpose
Reviews the library as a whole, merges overlaps, and organizes categories and discoverability.

## Why it belongs
A large skill library needs editorial discipline.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill catalog curation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill catalog curation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `skill-packaging`
- `skill-installation`
- `skill-provenance`
- `skill-variant-splitting`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
