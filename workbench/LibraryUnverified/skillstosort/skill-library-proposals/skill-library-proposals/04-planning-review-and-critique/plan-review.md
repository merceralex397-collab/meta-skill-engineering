---
name: plan-review
display_name: Plan Review
category: Planning, Review, and Critique
priority: P0
status: candidate
sources:
  - user-request
  - gpttalker-agent
  - conversation
---

# Plan Review

## Purpose
Reviews a proposed plan for completeness, sequencing, hidden assumptions, and execution readiness before implementation begins.

## Why it belongs
You explicitly asked for this and GPTTalker already has a plan-review agent.

## Suggested trigger situations
- when the user or repo work clearly points at plan review
- when a task in the "Planning, Review, and Critique" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around plan review

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when plan review would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `steelman`
- `red-team-challenge`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
