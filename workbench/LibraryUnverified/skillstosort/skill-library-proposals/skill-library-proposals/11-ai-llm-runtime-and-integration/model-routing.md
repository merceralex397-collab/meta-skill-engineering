---
name: model-routing
display_name: Model Routing
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Model Routing

## Purpose
Routes different requests to different models or runtimes based on capability, latency, or cost.

## Why it belongs
Useful for hybrid local-plus-remote systems.

## Suggested trigger situations
- when the user or repo work clearly points at model routing
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around model routing

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when model routing would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `safety-guardrails`
- `offline-cpu-inference`
- `inference-serving`
- `quantization-strategy`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
