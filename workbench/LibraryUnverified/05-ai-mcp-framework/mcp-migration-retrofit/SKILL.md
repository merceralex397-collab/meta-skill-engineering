---
name: mcp-migration-retrofit
description: "Convert an existing API, CLI tool, or service into an MCP server without a full rewrite. Use when retrofitting MCP onto existing code, wrapping REST/GraphQL APIs as MCP tools, migrating from the deprecated HTTP+SSE transport to Streamable HTTP, or incrementally adding MCP to a running service."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-migration-retrofit
  maturity: stable
  risk: medium
  tags: [mcp, migration, retrofit, api, wrapper]
---

# Purpose

Retrofit MCP server capabilities onto an existing service, API, or tool without a ground-up rewrite. This covers mapping existing endpoints to MCP primitives, wrapping existing code in MCP handlers, and migrating from deprecated MCP transports.

# When to use this skill

- Wrapping an existing REST or GraphQL API as MCP tools
- Adding an MCP interface to an existing CLI tool or service
- Migrating from HTTP+SSE transport (protocol 2024-11-05) to Streamable HTTP (2025-06-18)
- Incrementally adding MCP support to a running codebase

# Do not use this skill when

- Building a new MCP server from scratch → use `mcp-development` or `get-started`
- The task is about packaging/publishing → use `mcp-marketplace-publishing`
- The existing service should be replaced, not wrapped

# Operating procedure

## Phase 1 — Audit the existing surface

1. **Inventory existing endpoints/functions.** List every API endpoint, CLI command, or function that could become an MCP tool.
2. **Classify each as MCP primitive:**
   - Read-only queries → MCP **tool** with `readOnlyHint: true`, or **resource** if it's static context
   - Mutations/actions → MCP **tool** with `destructiveHint: true/false`
   - Static context data (schemas, configs) → MCP **resource**
   - Interaction templates → MCP **prompt**
3. **Map auth model.** How does the existing API authenticate? Map this to either environment-based credentials (stdio) or OAuth 2.1 (Streamable HTTP).
4. **Identify data shape changes.** Existing API responses may need reformatting for LLM consumption (concise, focused, actionable).

## Phase 2 — Wrap with minimal changes

**Pattern: thin MCP wrapper over existing code**

```typescript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { existingApiClient } from "./existing-client.js"; // your existing code

const server = new McpServer({ name: "my-service", version: "1.0.0" });

// Wrap existing function as MCP tool
server.tool(
  "list_items",
  { status: z.enum(["active", "archived"]).optional() },
  async ({ status }) => {
    const items = await existingApiClient.listItems({ status });
    return {
      content: [{
        type: "text",
        text: JSON.stringify(items.map(i => ({ id: i.id, name: i.name })), null, 2)
      }]
    };
  }
);
```

**Key pattern:** The MCP tool handler delegates to existing code. It only handles:
- Input validation (via Zod/Pydantic schemas mapped from existing API params)
- Response formatting (convert existing API response to MCP content format)
- Error mapping (convert existing errors to MCP error format)

## Phase 3 — Format responses for LLM consumption

Existing API responses are often verbose. MCP tool responses should be concise:

- **Filter fields:** Return only fields the LLM needs, not the full API response
- **Paginate:** If the API returns 1000 items, add pagination params and return 20 at a time
- **Summarize:** For large objects, return a summary with option to fetch details
- **Error messages:** Translate API error codes to actionable human-readable messages

## Phase 4 — Migrate deprecated transports

If migrating from HTTP+SSE (protocol 2024-11-05) to Streamable HTTP (2025-06-18):

1. Replace separate SSE endpoint with unified MCP endpoint (POST + GET on same path)
2. Move from dedicated SSE connection to POST-initiated SSE streams
3. Add `Mcp-Session-Id` header support
4. Update protocol version to `2025-06-18` in initialize handshake
5. Add `MCP-Protocol-Version` header on all HTTP requests

## Phase 5 — Incremental rollout

1. Start by wrapping 3-5 of the most-used endpoints as MCP tools
2. Test with MCP Inspector
3. Connect to a host and verify end-to-end
4. Add more tools incrementally based on usage
5. Add resources for static context (schemas, documentation) last

# Decision rules

- Wrap, don't rewrite. The MCP handler is a thin adapter. Keep business logic in existing code.
- One API endpoint does NOT always map to one MCP tool. Sometimes two endpoints should be one tool; sometimes one endpoint needs two tools (list + get-detail).
- If an endpoint has more than 8 parameters, split it into multiple tools or use optional parameters with sensible defaults.
- Response size: aim for <4KB per tool response. Paginate or summarize if larger.
- Preserve existing auth — just bridge it to MCP's model (env vars for stdio, OAuth for HTTP).

# Output requirements

1. Mapping document: existing endpoints → MCP primitives
2. MCP server wrapping existing code with no business logic duplication
3. Response formatting that works well for LLM consumption
4. MCP Inspector verification of wrapped tools

# Related skills

- `mcp-development` — if the retrofit becomes complex enough to warrant full redesign
- `mcp-tool-design` — designing good tool schemas for wrapped endpoints
- `mcp-schema-contracts` — schema evolution when the underlying API changes
- `mcp-auth-transports` — transport migration details

# Failure handling

- If an existing endpoint is too complex for one MCP tool, split into read + write or list + detail tools
- If existing error codes don't map cleanly, create an error translation layer in the wrapper
- If the existing API is rate-limited, add rate limit awareness to MCP tool handlers and return helpful error messages when throttled
