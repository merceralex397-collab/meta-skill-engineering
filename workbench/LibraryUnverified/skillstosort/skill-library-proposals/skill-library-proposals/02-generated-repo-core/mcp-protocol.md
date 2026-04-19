---
name: mcp-protocol
display_name: MCP Protocol
category: Generated Repo Core Skills
priority: P1
status: candidate
sources:
  - gpttalker-existing
  - conversation
---

# MCP Protocol

## Purpose
Applies repo-specific MCP protocol, schema, and fail-closed policies.

## Why it belongs
For MCP repos this should probably be upgraded into a deeper pack.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "Generated Repo Core Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp protocol

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp protocol would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **gpttalker-existing** — already exists as a generated skill in the uploaded GPTTalker repo
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `incident-postmortem`
- `context-intelligence`
- `node-agent-patterns`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
