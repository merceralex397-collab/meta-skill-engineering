---
name: multi-agent-debugging
display_name: Multi-Agent Debugging
category: Agentic Orchestration and Autonomy
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# Multi-Agent Debugging

## Purpose
Diagnoses whether a failure came from the prompt layer, routing, state, tooling, or the agents themselves.

## Why it belongs
Necessary once orchestration becomes real.

## Suggested trigger situations
- when the user or repo work clearly points at multi-agent debugging
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around multi-agent debugging

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when multi-agent debugging would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `swarm-patterns`
- `collaboration-checkpoints`
- `autonomous-backlog-maintenance`
- `process-versioning`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
