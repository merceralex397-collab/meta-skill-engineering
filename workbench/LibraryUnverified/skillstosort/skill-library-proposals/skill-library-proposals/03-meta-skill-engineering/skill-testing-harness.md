---
name: skill-testing-harness
display_name: Skill Testing Harness
category: Meta Skill Engineering
priority: P1
status: candidate
sources:
  - invented
  - claude-skill-creator
  - architecture-spec
---

# Skill Testing Harness

## Purpose
Sets up repeatable prompts, fixtures, and comparison runs for skill development.

## Why it belongs
This formalizes the test loop described in the creator docs.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill testing harness

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill testing harness would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **claude-skill-creator** — supported by the uploaded Claude skill creator document
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `skill-refinement`
- `skill-evaluation`
- `skill-trigger-optimization`
- `skill-benchmarking`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
