---
name: quantization-strategy
display_name: Quantization Strategy
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Quantization Strategy

## Purpose
Chooses quantization levels and tradeoffs based on memory, speed, and quality constraints.

## Why it belongs
A recurring practical concern in your chats.

## Suggested trigger situations
- when the user or repo work clearly points at quantization strategy
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around quantization strategy

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when quantization strategy would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `model-routing`
- `inference-serving`
- `retrieval-quality`
- `structured-output-pipelines`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
