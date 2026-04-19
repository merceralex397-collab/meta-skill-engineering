---
name: workflow-observability
display_name: Workflow Observability
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - research-docs
  - conversation
---

# Workflow Observability

## Purpose
Inspects provenance, workflow state, invocation logs, and recent transitions when repo health is in question.

## Why it belongs
This helps diagnose autonomous drift instead of guessing.

## Suggested trigger situations
- when the user or repo work clearly points at workflow observability
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around workflow observability

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when workflow observability would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `review-audit-bridge`
- `docs-and-handoff`
- `research-delegation`
- `local-git-specialist`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
