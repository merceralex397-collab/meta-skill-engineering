---
name: dependency-upgrades
display_name: Dependency Upgrades
category: Generated Repo Core Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Dependency Upgrades

## Purpose
Handles version bumps, lockfile churn, changelog review, and compatibility validation.

## Why it belongs
A focused upgrade skill saves a lot of routine failure.

## Suggested trigger situations
- when the user or repo work clearly points at dependency upgrades
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around dependency upgrades

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when dependency upgrades would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `security-hardening`
- `migration-refactor`
- `release-engineering`
- `incident-postmortem`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
