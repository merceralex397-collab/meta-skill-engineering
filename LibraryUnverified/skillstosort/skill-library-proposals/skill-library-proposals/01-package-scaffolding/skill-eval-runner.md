---
name: skill-eval-runner
display_name: Skill Eval Runner
category: Package Scaffolding Skills
priority: P1
status: candidate
sources:
  - invented
  - architecture-spec
  - claude-skill-creator
---

# Skill Eval Runner

## Purpose
Runs trigger tests, behavior tests, baseline comparisons, and report aggregation for skills.

## Why it belongs
The architecture docs and Claude creator loop both imply this as mandatory infrastructure.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill eval runner

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill eval runner would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs
- **claude-skill-creator** — supported by the uploaded Claude skill creator document

## Related skills
- `pr-review-ticket-bridge`
- `skill-registry-manager`
- `skill-description-optimizer`
- `overlay-generator`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
