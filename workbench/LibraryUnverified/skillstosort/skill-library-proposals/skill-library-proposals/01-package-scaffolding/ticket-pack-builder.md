---
name: ticket-pack-builder
display_name: Ticket Pack Builder
category: Package Scaffolding Skills
priority: P0
status: candidate
sources:
  - scafforge-existing
  - conversation
---

# Ticket Pack Builder

## Purpose
Creates a machine-readable backlog with tickets, dependencies, stages, and acceptance criteria.

## Why it belongs
Small tickets are a core anti-drift mechanism in your workflow.

## Suggested trigger situations
- when the user or repo work clearly points at ticket pack builder
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around ticket pack builder

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when ticket pack builder would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **scafforge-existing** — already exists as a Scafforge package skill in the uploaded repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `repo-scaffold-factory`
- `opencode-team-bootstrap`
- `project-skill-bootstrap`
- `agent-prompt-engineering`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
