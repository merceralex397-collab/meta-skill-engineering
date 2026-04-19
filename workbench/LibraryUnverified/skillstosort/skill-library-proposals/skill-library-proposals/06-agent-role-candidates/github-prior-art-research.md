---
name: github-prior-art-research
display_name: GitHub Prior Art Research
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# GitHub Prior Art Research

## Purpose
Looks for similar repos, patterns, and implementations on GitHub and returns grounded findings.

## Why it belongs
Matches your repeated prior-art requests.

## Suggested trigger situations
- when the user or repo work clearly points at github prior art research
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around github prior art research

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when github prior art research would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `docs-handoff`
- `repo-evidence-gathering`
- `shell-inspection`
- `context-summarization`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
