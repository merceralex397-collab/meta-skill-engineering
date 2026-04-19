---
name: skill-provenance
display_name: Skill Provenance
category: Meta Skill Engineering
priority: P1
status: candidate
sources:
  - invented
  - architecture-spec
  - conversation
---

# Skill Provenance

## Purpose
Records where a skill came from, what evidence informed it, and what assumptions it encodes.

## Why it belongs
Helps avoid mysterious prompt blobs.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill provenance

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill provenance would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `skill-installation`
- `skill-catalog-curation`
- `skill-variant-splitting`
- `skill-safety-review`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
