---
name: offline-cpu-inference
display_name: Offline CPU Inference
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Offline CPU Inference

## Purpose
Optimizes and constrains local CPU-only inference setups for weak or older machines.

## Why it belongs
Directly aligned with your low-end inference interest.

## Suggested trigger situations
- when the user or repo work clearly points at offline cpu inference
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around offline cpu inference

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when offline cpu inference would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `context-management-memory`
- `safety-guardrails`
- `model-routing`
- `inference-serving`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
