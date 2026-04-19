---
name: ollama
display_name: Ollama
category: AI / LLM Runtime and Integration Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Ollama

## Purpose
Applies Ollama-specific conventions for model management, serving, and local integration.

## Why it belongs
Likely useful if you compare local runtimes.

## Suggested trigger situations
- when the user or repo work clearly points at ollama
- when a task in the "AI / LLM Runtime and Integration Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around ollama

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when ollama would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `llm-integration`
- `local-llm`
- `llama-cpp`
- `vllm-serving`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
