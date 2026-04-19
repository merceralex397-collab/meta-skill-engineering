---
name: mcp-testing-evals
display_name: MCP Testing and Evals
category: MCP Skills
priority: P0
status: candidate
sources:
  - conversation
  - invented
---

# MCP Testing and Evals

## Purpose
Tests MCP tools, transports, permission behavior, and end-to-end host interactions under repeatable scenarios.

## Why it belongs
MCP repos need more than unit tests.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp testing and evals

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp testing and evals would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-development`
- `mcp-tool-design`
- `mcp-security-permissions`
- `mcp-host-integration`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
