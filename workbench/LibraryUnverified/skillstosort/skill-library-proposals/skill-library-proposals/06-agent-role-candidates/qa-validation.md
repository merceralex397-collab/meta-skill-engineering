---
name: qa-validation
display_name: QA Validation
category: Agent Role Candidate Skills
priority: P1
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# QA Validation

## Purpose
Checks whether the work actually meets acceptance criteria from a tester/QA perspective.

## Why it belongs
A separate skill prevents code review from swallowing QA.

## Suggested trigger situations
- when the user or repo work clearly points at qa validation
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around qa validation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when qa validation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `code-review`
- `security-review`
- `docs-handoff`
- `repo-evidence-gathering`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
