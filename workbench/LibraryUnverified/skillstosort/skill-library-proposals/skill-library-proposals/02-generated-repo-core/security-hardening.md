---
name: security-hardening
display_name: Security Hardening
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - research-gap
  - conversation
---

# Security Hardening

## Purpose
Translates common threat models into concrete stack-specific controls and review checks.

## Why it belongs
This complements but does not replace dedicated security review.

## Suggested trigger situations
- when the user or repo work clearly points at security hardening
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around security hardening

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when security hardening would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-gap** — called out as missing or underdeveloped in the uploaded reports
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `error-handling`
- `performance-baseline`
- `migration-refactor`
- `dependency-upgrades`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
