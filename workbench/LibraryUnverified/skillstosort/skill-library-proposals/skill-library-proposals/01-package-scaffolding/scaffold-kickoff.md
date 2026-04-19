---
name: scaffold-kickoff
display_name: Scaffold Kickoff
category: Package Scaffolding Skills
priority: P0
status: candidate
sources:
  - scafforge-existing
  - conversation
---

# Scaffold Kickoff

## Purpose
Entry point that coordinates end-to-end repo scaffolding from messy inputs to a usable generated repository.

## Why it belongs
This is the top-level conductor for the whole scaffold chain and should remain explicit rather than implicit.

## Suggested trigger situations
- when the user or repo work clearly points at scaffold kickoff
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around scaffold kickoff

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when scaffold kickoff would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **scafforge-existing** — already exists as a Scafforge package skill in the uploaded repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `spec-pack-normalizer`
- `repo-scaffold-factory`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
