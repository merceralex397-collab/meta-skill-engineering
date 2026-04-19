---
name: collaboration-checkpoints
display_name: Collaboration Checkpoints
category: Agentic Orchestration and Autonomy
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Collaboration Checkpoints

## Purpose
Introduces synchronization moments where agents reconcile assumptions, artifacts, and blockers.

## Why it belongs
Useful before merging parallel work.

## Suggested trigger situations
- when the user or repo work clearly points at collaboration checkpoints
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around collaboration checkpoints

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when collaboration checkpoints would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `manager-hierarchy-design`
- `swarm-patterns`
- `multi-agent-debugging`
- `autonomous-backlog-maintenance`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
