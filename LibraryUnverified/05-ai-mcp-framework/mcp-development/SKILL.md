---
name: mcp-development
description: "End-to-end MCP server development lifecycle — from API analysis through implementation, testing, and deployment. Use when building a multi-tool MCP server that wraps an external API or service, planning the tool/resource/prompt surface for a new MCP project, or iterating on an existing MCP server's design."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-development
  maturity: stable
  risk: low
  tags: [mcp, development, server, lifecycle]
---

# Purpose

Guide the full development lifecycle of an MCP server — from understanding the target API, through designing the tool/resource/prompt surface, to implementation, testing, and deployment. This is the hub skill for MCP server projects that span multiple phases.

# When to use this skill

- Building a new MCP server that wraps an external API or service
- Planning which tools, resources, and prompts an MCP server should expose
- Iterating on an existing MCP server (adding tools, improving schemas, fixing patterns)
- Need a structured development workflow rather than ad hoc implementation

# Do not use this skill when

- Scaffolding a first-ever MCP server → use `get-started`
- The task is narrowly about one tool's schema design → use `mcp-tool-design`
- The task is only about testing → use `mcp-testing-evals`
- Following a specific SDK's patterns → use `mcp-typescript-sdk` or `mcp-python-fastmcp`

# Operating procedure

## Phase 1 — Research and plan

1. **Study the target API.** Read its documentation. Identify key endpoints, auth model, data shapes, rate limits, and pagination patterns.
2. **Map endpoints to MCP primitives:**
   - **Tools** — for actions the LLM should invoke (queries, mutations, computations)
   - **Resources** — for read-only context data the host can present (schemas, configs, documentation)
   - **Prompts** — for reusable interaction templates (e.g., "analyze this data using the available tools")
3. **Decide coverage strategy.** Prioritize comprehensive API coverage over bespoke workflow tools. Comprehensive coverage lets agents compose operations flexibly.
4. **Choose stack.** TypeScript (recommended for broadest compatibility) or Python (FastMCP for rapid development). See `mcp-typescript-sdk` or `mcp-python-fastmcp`.

## Phase 2 — Implement core infrastructure

1. **API client wrapper** — centralized auth, error handling, retry logic
2. **Response formatting** — consistent JSON or Markdown output across tools
3. **Pagination helper** — many APIs and MCP `*/list` methods use cursor-based pagination
4. **Error mapping** — translate API errors to MCP error format:
   - Protocol errors: standard JSON-RPC error codes (-32600 to -32603)
   - Tool execution errors: `{ isError: true, content: [{ type: "text", text: "..." }] }`

## Phase 3 — Implement primitives

For each **tool**:
1. Define `inputSchema` using Zod (TS) or Pydantic/type hints (Python)
2. Write a clear `description` — this is what the LLM reads to decide when to use the tool
3. Set `annotations`: `readOnlyHint`, `destructiveHint`, `idempotentHint`, `openWorldHint`
4. Implement the handler with async I/O, proper error handling, and pagination support
5. Optionally define `outputSchema` for structured responses

For each **resource**:
1. Choose a URI scheme (e.g., `file://`, `https://`, or custom)
2. Implement `resources/list` and `resources/read`
3. Support `resources/subscribe` if the data changes over time

For each **prompt**:
1. Define the prompt name, description, and arguments
2. Return structured `messages` array with `role` and `content`

## Phase 4 — Declare capabilities

In the `initialize` response, declare what the server supports:
```json
{
  "capabilities": {
    "tools": { "listChanged": true },
    "resources": { "subscribe": true, "listChanged": true },
    "prompts": { "listChanged": true },
    "logging": {}
  }
}
```

Only declare capabilities you actually implement.

## Phase 5 — Test and validate

1. **Build:** `npm run build` (TS) or `python -m py_compile` (Python)
2. **Inspector:** `npx @modelcontextprotocol/inspector` — verify all tools list, call correctly, return expected schemas
3. **Host test:** Connect to a real host (Claude Desktop, VS Code, Codex) and verify end-to-end
4. **Edge cases:** Test with invalid inputs, missing auth, rate-limited APIs, empty results

## Phase 6 — Ship

1. Choose transport (stdio for local, Streamable HTTP for remote)
2. Add auth if remote (see `mcp-auth-transports`)
3. Write README with installation and configuration instructions
4. Publish to npm/PyPI or register in an MCP registry

# Decision rules

- **Tool naming:** Use `<domain>_<action>` prefix pattern (e.g., `github_create_issue`, `github_list_repos`) for discoverability
- **Tool count:** Start with the most common 10-15 operations. Expand based on usage, not speculation.
- **Resource vs Tool:** If the LLM needs to take action → tool. If the host needs context → resource.
- **Error messages:** Include what went wrong AND what to try next. "API returned 403: check that the API key has write permissions for this repository."
- **DRY:** Extract shared API client, pagination, and formatting into utility modules

# Output requirements

1. Working MCP server with declared capabilities
2. All tools have `inputSchema`, `description`, and `annotations`
3. Passes MCP Inspector verification for all primitives
4. README with host configuration examples

# Related skills

- `mcp-tool-design` — deep dive on individual tool design
- `mcp-resources-prompts` — resource and prompt implementation patterns
- `mcp-testing-evals` — testing strategy and evaluation creation
- `mcp-builder` — reference implementation guide from Anthropic

# Failure handling

- If the target API has no documentation, use the API's OpenAPI/Swagger spec if available; otherwise explore endpoints manually and document as you go
- If a tool is too complex for a single call, split into a read step and a write step rather than one tool that does both
- If tool count exceeds 30, consider splitting into multiple focused MCP servers rather than one monolith
