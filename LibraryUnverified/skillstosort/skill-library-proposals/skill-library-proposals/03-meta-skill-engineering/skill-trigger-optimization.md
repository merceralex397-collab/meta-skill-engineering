---
name: skill-trigger-optimization
display_name: Skill Trigger Optimization
category: Meta Skill Engineering
priority: P1
status: candidate
sources:
  - invented
  - claude-skill-creator
---

# Skill Trigger Optimization

## Purpose
Improves trigger phrases, descriptions, and routing language so a host actually invokes the skill when it should.

## Why it belongs
Important because undertriggering was called out repeatedly.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill trigger optimization

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill trigger optimization would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **claude-skill-creator** — supported by the uploaded Claude skill creator document

## Related skills
- `skill-evaluation`
- `skill-testing-harness`
- `skill-benchmarking`
- `skill-packaging`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
