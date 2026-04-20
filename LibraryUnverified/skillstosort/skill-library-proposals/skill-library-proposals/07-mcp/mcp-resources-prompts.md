---
name: mcp-resources-prompts
display_name: MCP Resources and Prompts
category: MCP Skills
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# MCP Resources and Prompts

## Purpose
Covers non-tool MCP surfaces such as resources, prompt templates, and discovery metadata.

## Why it belongs
MCP is broader than just tool calls.

## Suggested trigger situations
- when building, debugging, securing, or packaging an MCP server or MCP tool surface
- when a task in the "MCP Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around mcp resources and prompts

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when mcp resources and prompts would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `mcp-typescript-sdk`
- `mcp-go-server`
- `mcp-auth-transports`
- `mcp-inspector-debugging`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
