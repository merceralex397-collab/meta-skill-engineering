---
name: deployment-pipeline
display_name: Deployment Pipeline
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - research-gap
  - conversation
---

# Deployment Pipeline

## Purpose
Defines CI/CD, release, deployment, and rollback procedure for the chosen target.

## Why it belongs
Every serious repo eventually needs this layer.

## Suggested trigger situations
- when the user or repo work clearly points at deployment pipeline
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around deployment pipeline

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when deployment pipeline would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `prompt-crafting`
- `testing`
- `database-persistence`
- `auth-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
