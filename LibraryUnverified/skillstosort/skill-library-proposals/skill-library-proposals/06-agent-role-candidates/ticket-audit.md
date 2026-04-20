---
name: ticket-audit
display_name: Ticket Audit
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Ticket Audit

## Purpose
Audits ticket state, dependencies, and artifact completeness for inconsistencies or stale transitions.

## Why it belongs
A good maintenance utility.

## Suggested trigger situations
- when the user or repo work clearly points at ticket audit
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around ticket audit

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when ticket audit would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `shell-inspection`
- `context-summarization`
- `implementer-hub`
- `implementer-node-agent`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
