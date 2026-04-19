---
name: implementer-hub
display_name: Implementer Hub
category: Agent Role Candidate Skills
priority: P2
status: candidate
sources:
  - gpttalker-agent
  - conversation
---

# Implementer Hub

## Purpose
Implements changes in the hub/core/service layer under an approved plan.

## Why it belongs
Role-specific, but directly relevant to your current MCP repo.

## Suggested trigger situations
- when the user or repo work clearly points at implementer hub
- when a task in the "Agent Role Candidate Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around implementer hub

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when implementer hub would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-agent** — derived from an agent role already present in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `context-summarization`
- `ticket-audit`
- `implementer-node-agent`
- `implementer-context`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
