---
name: safety-guardrails
display_name: Safety Guardrails
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Safety Guardrails

## Purpose
Defines refusal, escalation, permission, and safe-default controls around model or agent behavior.

## Why it belongs
Useful when systems gain more autonomy.

## Suggested trigger situations
- when the user or repo work clearly points at safety guardrails
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around safety guardrails

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when safety guardrails would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `llm-evals-benchmarks`
- `context-management-memory`
- `offline-cpu-inference`
- `model-routing`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
