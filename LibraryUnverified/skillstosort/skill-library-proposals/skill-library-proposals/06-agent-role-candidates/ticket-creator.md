---
name: ticket-creator
display_name: Ticket Creator
category: Agent Role Candidate Skills
priority: P1
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Ticket Creator

## Purpose
Creates follow-up tickets only from actual evidence, not vague intentions or duplicated complaints.

## Why it belongs
A sensible workflow guardrail.

## Suggested trigger situations
- when the user or repo work clearly points at ticket creator
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around ticket creator

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when ticket creator would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `planner`
- `backlog-verifier`
- `code-review`
- `security-review`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
