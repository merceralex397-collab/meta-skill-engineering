---
name: implementer-node-agent
display_name: Implementer Node Agent
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Implementer Node Agent

## Purpose
Implements bounded changes in node-agent or per-machine execution surfaces under an approved plan.

## Why it belongs
Another direct carry-over from the MCP repo.

## Suggested trigger situations
- when the user or repo work clearly points at implementer node agent
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around implementer node agent

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when implementer node agent would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `ticket-audit`
- `implementer-hub`
- `implementer-context`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
