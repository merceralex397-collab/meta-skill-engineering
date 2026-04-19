---
name: shell-inspection
display_name: Shell Inspection
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Shell Inspection

## Purpose
Uses read-only terminal inspection to confirm repo state, file existence, and runtime facts.

## Why it belongs
A practical support skill for CLI-heavy work.

## Suggested trigger situations
- when the user or repo work clearly points at shell inspection
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around shell inspection

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when shell inspection would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `repo-evidence-gathering`
- `github-prior-art-research`
- `context-summarization`
- `ticket-audit`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
