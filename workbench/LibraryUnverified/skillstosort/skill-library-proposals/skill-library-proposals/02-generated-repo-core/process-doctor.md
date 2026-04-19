---
name: process-doctor
display_name: Process Doctor
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - gpttalker-existing
  - conversation
---

# Process Doctor

## Purpose
Runs a repo-local process repair or verification pass after workflow changes.

## Why it belongs
This is the generated-repo counterpart to the package-level repair skill.

## Suggested trigger situations
- when the user or repo work clearly points at process doctor
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around process doctor

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when process doctor would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `local-git-specialist`
- `isolation-guidance`
- `planning`
- `skill-authoring`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
