---
name: docs-handoff
display_name: Docs Handoff
category: Agent Role Candidate Skills
priority: P1
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Docs Handoff

## Purpose
Synchronizes documentation and handoff surfaces after a ticket or milestone is complete.

## Why it belongs
A narrower role-focused variant of docs-and-handoff.

## Suggested trigger situations
- when the user or repo work clearly points at docs handoff
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around docs handoff

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when docs handoff would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `security-review`
- `qa-validation`
- `repo-evidence-gathering`
- `github-prior-art-research`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
