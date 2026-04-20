---
name: docs-and-handoff
display_name: Docs and Handoff
category: Generated Repo Core Skills
priority: P0
status: candidate
sources:
  - gpttalker-existing
  - research-docs
  - conversation
---

# Docs and Handoff

## Purpose
Keeps README, process docs, ticket views, and restart documents aligned with canonical state.

## Why it belongs
Generated repos should not let documentation drift away from actual state.

## Suggested trigger situations
- when the user or repo work clearly points at docs and handoff
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around docs and handoff

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when docs and handoff would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `ticket-execution`
- `review-audit-bridge`
- `workflow-observability`
- `research-delegation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
