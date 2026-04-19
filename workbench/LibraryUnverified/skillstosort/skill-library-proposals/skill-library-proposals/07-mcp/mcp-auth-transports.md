---
name: mcp-auth-transports
display_name: MCP Auth and Transports
category: MCP Skills
priority: P1
status: candidate
sources:
  - conversation
  - research-docs
  - invented
---

# MCP Auth and Transports

## Purpose
Handles stdio versus HTTP/SSE/WebSocket transport decisions and any authentication layer on top.

## Why it belongs
Your reports explicitly called this out as missing in public examples.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp auth and transports

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp auth and transports would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-go-server`
- `mcp-resources-prompts`
- `mcp-inspector-debugging`
- `mcp-marketplace-publishing`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
