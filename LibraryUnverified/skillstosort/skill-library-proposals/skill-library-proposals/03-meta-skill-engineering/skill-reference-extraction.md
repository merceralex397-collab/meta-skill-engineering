---
name: skill-reference-extraction
display_name: Skill Reference Extraction
category: Meta Skill Engineering
priority: P2
status: candidate
sources:
  - invented
  - architecture-spec
---

# Skill Reference Extraction

## Purpose
Pulls large docs, examples, and schemas out of the main SKILL body into references for progressive disclosure.

## Why it belongs
This follows the recommended architecture pattern.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill reference extraction

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill reference extraction would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `skill-variant-splitting`
- `skill-safety-review`
- `skill-anti-patterns`
- `skill-lifecycle-management`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
