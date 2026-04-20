---
name: skill-description-optimizer
display_name: Skill Description Optimizer
category: Package Scaffolding Skills
priority: P1
status: candidate
sources:
  - invented
  - claude-skill-creator
  - conversation
---

# Skill Description Optimizer

## Purpose
Improves skill descriptions so routing triggers fire more reliably without overfiring.

## Why it belongs
Your research highlighted undertriggering and the need for pushier descriptions.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill description optimizer

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill description optimizer would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **claude-skill-creator** — supported by the uploaded Claude skill creator document
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `skill-registry-manager`
- `skill-eval-runner`
- `overlay-generator`
- `skill-packager`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
