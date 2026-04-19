---
name: repo-evidence-gathering
display_name: Repo Evidence Gathering
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Repo Evidence Gathering

## Purpose
Collects focused repository evidence for another specialist without making edits.

## Why it belongs
Good as a utility lane.

## Suggested trigger situations
- when the user or repo work clearly points at repo evidence gathering
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around repo evidence gathering

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when repo evidence gathering would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `qa-validation`
- `docs-handoff`
- `github-prior-art-research`
- `shell-inspection`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
