---
name: inference-serving
display_name: Inference Serving
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Inference Serving

## Purpose
Covers API serving, concurrency, queuing, batching, and operational behavior for hosted inference.

## Why it belongs
A runtime-focused complement to model selection.

## Suggested trigger situations
- when the user or repo work clearly points at inference serving
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around inference serving

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when inference serving would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `offline-cpu-inference`
- `model-routing`
- `quantization-strategy`
- `retrieval-quality`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
