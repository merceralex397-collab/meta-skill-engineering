---
name: security-review
display_name: Security Review
category: Agent Role Candidate Skills
priority: P1
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Security Review

## Purpose
Reviews trust boundaries, secrets handling, validation, and attack surfaces in a read-only lane.

## Why it belongs
Important enough to stand alone.

## Suggested trigger situations
- when the user or repo work clearly points at security review
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around security review

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when security review would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `ticket-creator`
- `code-review`
- `qa-validation`
- `docs-handoff`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
