---
name: context-management-memory
display_name: Context Management and Memory
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Context Management and Memory

## Purpose
Structures what stays in prompt, what becomes retrieved memory, and how stale context is controlled.

## Why it belongs
Important for long-running systems.

## Suggested trigger situations
- when the user or repo work clearly points at context management and memory
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around context management and memory

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when context management and memory would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `multimodal-ai`
- `llm-evals-benchmarks`
- `safety-guardrails`
- `offline-cpu-inference`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
