---
name: docker-containers
display_name: Docker and Containers
category: Cloud, Platform, and DevOps Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Docker and Containers

## Purpose
Handles container build, layering, runtime config, and local-to-prod consistency.

## Why it belongs
A default operations skill for many repos.

## Suggested trigger situations
- when the user or repo work clearly points at docker and containers
- when a task in the "Cloud, Platform, and DevOps Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around docker and containers

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when docker and containers would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `aws`
- `vercel`
- `terraform-iac`
- `secret-management`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
