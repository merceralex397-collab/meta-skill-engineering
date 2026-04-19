---
name: agent-memory
display_name: Agent Memory
category: AI / LLM Runtime and Integration Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Agent Memory

## Purpose
Designs memory stores and policies for agents without letting memory become a stale junk drawer.

## Why it belongs
Important for multi-session agent systems.

## Suggested trigger situations
- when the user or repo work clearly points at agent memory
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around agent memory

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when agent memory would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `retrieval-quality`
- `structured-output-pipelines`
- `python-llm-ml-workflow`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
