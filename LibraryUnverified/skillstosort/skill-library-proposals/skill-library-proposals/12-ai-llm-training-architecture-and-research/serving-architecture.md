---
name: serving-architecture
display_name: Serving Architecture
category: AI / LLM Training, Architecture, and Research Skills
priority: P1
status: candidate
sources:
  - user-request
  - invented
---

# Serving Architecture

## Purpose
Designs the system architecture around deployed models: queues, routers, fallback paths, and scaling surfaces.

## Why it belongs
Useful whether the model is local or hosted.

## Suggested trigger situations
- when the user or repo work clearly points at serving architecture
- when a task in the "AI / LLM Training, Architecture, and Research Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around serving architecture

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when serving architecture would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **user-request** — explicitly requested by the user in this conversation
- **invented** — newly invented extension proposed for this project

## Related skills
- `reward-modeling`
- `model-merging`
- `training-infrastructure`
- `dense-to-moe-experiments`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
