---
name: mcp-host-integration
display_name: MCP Host Integration
category: MCP Skills
priority: P0
status: candidate
sources:
  - conversation
  - invented
---

# MCP Host Integration

## Purpose
Handles how an MCP server fits into ChatGPT, Claude, OpenCode, Codex, or other hosts and their differing expectations.

## Why it belongs
You explicitly care about host-side integration.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp host integration

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp host integration would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-testing-evals`
- `mcp-security-permissions`
- `mcp-python-fastmcp`
- `mcp-typescript-sdk`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
