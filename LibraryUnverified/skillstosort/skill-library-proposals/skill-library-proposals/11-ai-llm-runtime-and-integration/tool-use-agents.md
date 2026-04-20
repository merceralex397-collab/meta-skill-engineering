---
name: tool-use-agents
display_name: Tool-Use Agents
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Tool-Use Agents

## Purpose
Designs agents that call tools safely, with argument validation, fallback behavior, and observation handling.

## Why it belongs
A core building block for agentic systems.

## Suggested trigger situations
- when the user or repo work clearly points at tool-use agents
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around tool-use agents

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when tool-use agents would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `rag-retrieval`
- `embeddings-indexing`
- `multimodal-ai`
- `llm-evals-benchmarks`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
