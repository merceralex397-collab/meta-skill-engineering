---
name: stack-standards
display_name: Stack Standards
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - research-gap
  - conversation
---

# Stack Standards

## Purpose
Holds the actual language, framework, validation, and safety rules for the chosen stack.

## Why it belongs
This is currently the most important weak point and needs to become deep and specific.

## Suggested trigger situations
- when the user or repo work clearly points at stack standards
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around stack standards

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when stack standards would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `project-context`
- `repo-navigation`
- `ticket-execution`
- `review-audit-bridge`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
