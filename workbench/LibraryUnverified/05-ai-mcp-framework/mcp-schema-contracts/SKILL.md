---
name: mcp-schema-contracts
description: "Design and evolve MCP tool schemas (inputSchema, outputSchema), resource URIs, and prompt argument contracts. Use when designing JSON Schema for MCP tool parameters, implementing outputSchema for structured tool results, planning schema versioning for published servers, or validating schema backward compatibility."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-schema-contracts
  maturity: stable
  risk: low
  tags: [mcp, schema, contracts, json-schema, versioning]
---

# Purpose

Design robust schemas for MCP tools, resources, and prompts. MCP uses JSON Schema for tool input/output definitions. Good schemas enable LLMs to use tools correctly, hosts to validate arguments, and servers to evolve without breaking clients.

# When to use this skill

- Designing `inputSchema` for MCP tools
- Implementing `outputSchema` for structured tool results
- Planning schema evolution for published MCP servers
- Validating schema backward compatibility before releasing new versions
- Choosing between structured (`structuredContent`) and unstructured (`content`) responses

# Do not use this skill when

- Implementing tool logic → use `mcp-tool-design`
- General server development → use `mcp-development`
- The schemas are trivial (1-2 string params, text output)

# MCP schema fundamentals

## inputSchema

Every tool declares an `inputSchema` — a JSON Schema object that defines what arguments the tool accepts:

```json
{
  "name": "search_issues",
  "inputSchema": {
    "type": "object",
    "properties": {
      "query": { "type": "string", "description": "Search query text" },
      "state": { "type": "string", "enum": ["open", "closed", "all"], "description": "Filter by issue state" },
      "limit": { "type": "integer", "minimum": 1, "maximum": 100, "default": 20, "description": "Max results" }
    },
    "required": ["query"]
  }
}
```

**Critical:** The `description` on each property is what the LLM reads to understand what to pass. Make descriptions clear and specific.

## outputSchema

Optional JSON Schema defining the structured output a tool produces:

```json
{
  "name": "get_weather",
  "outputSchema": {
    "type": "object",
    "properties": {
      "temperature": { "type": "number", "description": "Temperature in Celsius" },
      "conditions": { "type": "string" },
      "humidity": { "type": "number" }
    },
    "required": ["temperature", "conditions", "humidity"]
  }
}
```

When `outputSchema` is declared, the tool response includes `structuredContent`:
```json
{
  "content": [{ "type": "text", "text": "{\"temperature\": 22.5, ...}" }],
  "structuredContent": { "temperature": 22.5, "conditions": "Partly cloudy", "humidity": 65 }
}
```

**Rule:** Always include the JSON-serialized form in `content[].text` for backward compatibility with clients that don't support `structuredContent`.

# Schema design patterns

## Pattern 1 — Constrained enums

Use `enum` for parameters with known valid values:
```json
{ "type": "string", "enum": ["asc", "desc"], "description": "Sort direction" }
```

## Pattern 2 — Optional params with defaults

```json
{
  "page": { "type": "integer", "default": 1, "minimum": 1, "description": "Page number (default: 1)" },
  "per_page": { "type": "integer", "default": 20, "minimum": 1, "maximum": 100 }
}
```

## Pattern 3 — Nested objects

For complex inputs, nest JSON Schema objects:
```json
{
  "filters": {
    "type": "object",
    "properties": {
      "status": { "type": "string", "enum": ["active", "archived"] },
      "created_after": { "type": "string", "format": "date-time" }
    }
  }
}
```

## Pattern 4 — Arrays

```json
{
  "tags": {
    "type": "array",
    "items": { "type": "string" },
    "description": "Tags to filter by",
    "maxItems": 10
  }
}
```

# Schema evolution rules

When a published MCP server changes its schemas:

**Backward-compatible (minor version bump):**
- Adding new optional parameters
- Adding new tools
- Widening an enum (adding new values)
- Relaxing a constraint (e.g., increasing maxLength)

**Breaking (major version bump):**
- Removing or renaming a tool
- Removing or renaming a parameter
- Making an optional parameter required
- Narrowing an enum (removing values)
- Tightening constraints on existing parameters
- Changing a parameter's type

# Schema validation best practices

**Using Zod (TypeScript):**
```typescript
import { z } from "zod";

const SearchParams = z.object({
  query: z.string().describe("Search query"),
  limit: z.number().int().min(1).max(100).default(20).describe("Max results"),
});

server.tool("search", SearchParams.shape, async (params) => { ... });
```

**Using Pydantic (Python):**
```python
from pydantic import BaseModel, Field

class SearchParams(BaseModel):
    query: str = Field(description="Search query")
    limit: int = Field(default=20, ge=1, le=100, description="Max results")
```

# Decision rules

- Every `inputSchema` property MUST have a `description` — the LLM needs this to use the tool correctly
- Use `required` array to distinguish mandatory vs optional parameters
- Prefer `enum` over free-text for parameters with known valid values
- Keep schemas as flat as possible — deep nesting confuses LLMs
- Use `outputSchema` when clients need to parse structured responses programmatically
- Always include backward-compatible `content[].text` alongside `structuredContent`
- If a tool has more than 8 parameters, split into multiple tools or use optional params with defaults

# Output requirements

1. `inputSchema` for every tool with descriptions on all properties
2. `outputSchema` where structured responses are needed
3. Schema evolution plan for published servers
4. Validation using Zod (TS) or Pydantic (Python)

# Related skills

- `mcp-tool-design` — tool design beyond schemas
- `mcp-resources-prompts` — resource and prompt schemas
- `mcp-marketplace-publishing` — versioning published servers
- `mcp-migration-retrofit` — evolving schemas for wrapped APIs

# Failure handling

- If an LLM sends invalid arguments, the host returns `-32602 Invalid params` — check that the schema matches what you expect
- If `structuredContent` is not supported by a client, fall back to `content[].text` (this is automatic if you include both)
- If schema changes break existing clients, issue a major version bump and document the migration
