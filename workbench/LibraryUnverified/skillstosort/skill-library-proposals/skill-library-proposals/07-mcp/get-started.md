---
name: get-started
display_name: Get Started
category: MCP Skills
priority: P2
status: candidate
sources:
  - external-example
  - research-docs
---

# Get Started

## Purpose
Provides a deterministic micro-skill for onboarding or initial validation of the local skill environment.

## Why it belongs
A good example of a tiny, narrow, high-confidence skill.

## Suggested trigger situations
- when the user or repo work clearly points at get started
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around get started

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when get started would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns

## Related skills
- `mcp-builder`
- `opencode-primitives`
- `opencode-bridge`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
