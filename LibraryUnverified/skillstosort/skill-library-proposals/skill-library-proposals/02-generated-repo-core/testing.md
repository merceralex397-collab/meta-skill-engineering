---
name: testing
display_name: Testing
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - conversation
  - research-gap
---

# Testing

## Purpose
Chooses and structures appropriate testing layers for the current stack: unit, integration, e2e, snapshot, or property-based.

## Why it belongs
Testing was explicitly identified as missing from current generated packs.

## Suggested trigger situations
- when the user or repo work clearly points at testing
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around testing

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when testing would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **research-gap** — called out as missing or underdeveloped in the uploaded reports

## Related skills
- `skill-authoring`
- `prompt-crafting`
- `deployment-pipeline`
- `database-persistence`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
