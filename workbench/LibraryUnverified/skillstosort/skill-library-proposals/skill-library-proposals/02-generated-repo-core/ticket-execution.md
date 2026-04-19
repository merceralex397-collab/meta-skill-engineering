---
name: ticket-execution
display_name: Ticket Execution
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - research-docs
  - conversation
---

# Ticket Execution

## Purpose
Defines the required stage order, artifact proofs, and routing rules for ticket work.

## Why it belongs
This is the core anti-drift execution contract.

## Suggested trigger situations
- when the user or repo work clearly points at ticket execution
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around ticket execution

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when ticket execution would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `repo-navigation`
- `stack-standards`
- `review-audit-bridge`
- `docs-and-handoff`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
