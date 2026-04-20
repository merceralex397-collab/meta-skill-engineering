---
name: mcp-multi-tenant-design
display_name: MCP Multi-Tenant Design
category: MCP Skills
priority: P2
status: candidate
sources:
  - conversation
  - invented
---

# MCP Multi-Tenant Design

## Purpose
Designs isolation, auth, state partitioning, and limits when one MCP service serves multiple tenants or users.

## Why it belongs
Advanced but valuable.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp multi-tenant design

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp multi-tenant design would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-schema-contracts`
- `mcp-migration-retrofit`
- `mcp-chatgpt-app-bridge`
- `mcp-builder`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
