---
name: mcp-schema-contracts
display_name: MCP Schema Contracts
category: MCP Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# MCP Schema Contracts

## Purpose
Defines naming, schema evolution, validation, and backward-compatibility expectations for MCP surfaces.

## Why it belongs
Schema breakage is a common hidden failure source.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp schema contracts

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp schema contracts would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-inspector-debugging`
- `mcp-marketplace-publishing`
- `mcp-migration-retrofit`
- `mcp-multi-tenant-design`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
