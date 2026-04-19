---
name: retrieval-quality
display_name: Retrieval Quality
category: AI / LLM Runtime and Integration Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Retrieval Quality

## Purpose
Measures and improves retrieval relevance, coverage, and grounding quality over time.

## Why it belongs
Useful once RAG exists.

## Suggested trigger situations
- when the user or repo work clearly points at retrieval quality
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around retrieval quality

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when retrieval quality would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `inference-serving`
- `quantization-strategy`
- `structured-output-pipelines`
- `agent-memory`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
