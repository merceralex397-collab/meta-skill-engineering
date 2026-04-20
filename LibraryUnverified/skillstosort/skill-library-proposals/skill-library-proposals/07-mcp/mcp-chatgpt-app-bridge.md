---
name: mcp-chatgpt-app-bridge
display_name: MCP ChatGPT App Bridge
category: MCP Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# MCP ChatGPT App Bridge

## Purpose
Designs the bridge between a local or hosted MCP server and a ChatGPT app-like experience or tool surface.

## Why it belongs
Directly tied to your questions about ChatGPT and local systems.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp chatgpt app bridge

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp chatgpt app bridge would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-migration-retrofit`
- `mcp-multi-tenant-design`
- `mcp-builder`
- `opencode-primitives`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
