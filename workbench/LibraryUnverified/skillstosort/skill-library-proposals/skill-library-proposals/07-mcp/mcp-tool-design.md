---
name: mcp-tool-design
display_name: MCP Tool Design
category: MCP Skills
priority: P0
status: candidate
sources:
  - conversation
  - invented
---

# MCP Tool Design

## Purpose
Designs MCP tools with safe contracts, good naming, narrow scope, and host-friendly schemas.

## Why it belongs
This is where bland MCP work often fails.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp tool design

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp tool design would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-development`
- `mcp-testing-evals`
- `mcp-security-permissions`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
