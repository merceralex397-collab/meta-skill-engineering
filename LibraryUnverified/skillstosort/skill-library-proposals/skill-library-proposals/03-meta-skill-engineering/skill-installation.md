---
name: skill-installation
display_name: Skill Installation
category: Meta Skill Engineering
priority: P1
status: candidate
sources:
  - external-example
  - skill-installer-package
---

# Skill Installation

## Purpose
Installs or links a skill into a target client or local environment using the correct conventions.

## Why it belongs
The skill-installer package in your sources makes this a clear candidate.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill installation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill installation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports
- **skill-installer-package** — present in the uploaded skill-installer package

## Related skills
- `skill-benchmarking`
- `skill-packaging`
- `skill-catalog-curation`
- `skill-provenance`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
