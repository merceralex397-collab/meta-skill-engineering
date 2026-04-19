---
name: context-intelligence
display_name: Context Intelligence
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - gpttalker-existing
  - conversation
---

# Context Intelligence

## Purpose
Handles indexing, retrieval, provenance metadata, and cross-repo context intelligence surfaces.

## Why it belongs
Important for repos that do search, memory, or semantic retrieval.

## Suggested trigger situations
- when the user or repo work clearly points at context intelligence
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around context intelligence

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when context intelligence would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `release-engineering`
- `incident-postmortem`
- `mcp-protocol`
- `node-agent-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
