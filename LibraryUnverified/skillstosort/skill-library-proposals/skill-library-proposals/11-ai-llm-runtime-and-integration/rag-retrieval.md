---
name: rag-retrieval
display_name: RAG and Retrieval
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# RAG and Retrieval

## Purpose
Covers retrieval design, chunking, grounding, citation, and failure modes in retrieval-augmented systems.

## Why it belongs
Relevant for context-heavy agents.

## Suggested trigger situations
- when the user or repo work clearly points at rag and retrieval
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around rag and retrieval

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when rag and retrieval would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `vllm-serving`
- `model-selection`
- `embeddings-indexing`
- `tool-use-agents`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
