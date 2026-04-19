---
name: context-summarization
display_name: Context Summarization
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Context Summarization

## Purpose
Compresses large evidence sets into a handoff-friendly summary without dropping critical facts.

## Why it belongs
Useful for long threads and large docs.

## Suggested trigger situations
- when the user or repo work clearly points at context summarization
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around context summarization

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when context summarization would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `github-prior-art-research`
- `shell-inspection`
- `ticket-audit`
- `implementer-hub`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
