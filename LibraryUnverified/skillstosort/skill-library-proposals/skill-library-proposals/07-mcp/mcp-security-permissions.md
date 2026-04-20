---
name: mcp-security-permissions
display_name: MCP Security and Permissions
category: MCP Skills
priority: P0
status: candidate
sources:
  - conversation
  - invented
---

# MCP Security and Permissions

## Purpose
Handles least privilege, tool gating, path restrictions, tenant boundaries, and fail-closed behavior for MCP systems.

## Why it belongs
Security should be explicit for exposed tool surfaces.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp security and permissions

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp security and permissions would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-tool-design`
- `mcp-testing-evals`
- `mcp-host-integration`
- `mcp-python-fastmcp`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
