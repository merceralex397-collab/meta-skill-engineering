---
name: skill-variant-splitting
display_name: Skill Variant Splitting
category: Meta Skill Engineering
priority: P2
status: candidate
sources:
  - invented
  - conversation
---

# Skill Variant Splitting

## Purpose
Decides when one broad skill should be split into stack variants, platform variants, or micro-skills.

## Why it belongs
Critical once a skill starts absorbing too many domains.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill variant splitting

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill variant splitting would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `skill-catalog-curation`
- `skill-provenance`
- `skill-safety-review`
- `skill-reference-extraction`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
