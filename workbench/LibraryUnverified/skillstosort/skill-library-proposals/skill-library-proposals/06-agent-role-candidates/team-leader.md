---
name: team-leader
display_name: Team Leader
category: Agent Role Candidate Skills
priority: P1
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Team Leader

## Purpose
Coordinates specialists, checks evidence first, and advances work without owning all implementation directly.

## Why it belongs
A useful skill or prompt pack for visible orchestration.

## Suggested trigger situations
- when the user or repo work clearly points at team leader
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around team leader

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when team leader would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `planner`
- `backlog-verifier`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
