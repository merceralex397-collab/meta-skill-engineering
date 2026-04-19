---
name: node-agent-patterns
display_name: Node Agent Patterns
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - gpttalker-existing
  - conversation
---

# Node Agent Patterns

## Purpose
Encodes rules for lightweight per-machine agents, bounded subprocess work, and safe transport.

## Why it belongs
Directly relevant to your MCP hub/node design.

## Suggested trigger situations
- when the user or repo work clearly points at node agent patterns
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around node agent patterns

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when node agent patterns would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `context-intelligence`
- `mcp-protocol`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
