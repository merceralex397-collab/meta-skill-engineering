---
name: code-review
display_name: Code Review
category: Agent Role Candidate Skills
priority: P1
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Code Review

## Purpose
Reviews implementation for correctness, regressions, maintainability, and mismatch with the approved plan.

## Why it belongs
Useful as a reusable skill even outside GPTTalker.

## Suggested trigger situations
- when the user or repo work clearly points at code review
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around code review

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when code review would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `backlog-verifier`
- `ticket-creator`
- `security-review`
- `qa-validation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
