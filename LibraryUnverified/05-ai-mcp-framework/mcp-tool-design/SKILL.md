---
name: mcp-tool-design
description: "Design MCP tools with clear names, narrow scope, safe annotations, and LLM-friendly schemas. Use when designing tool interfaces for an MCP server, choosing between tools/resources/prompts for a given capability, writing tool descriptions that LLMs can understand, or adding tool annotations for host trust signaling."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-tool-design
  maturity: stable
  risk: low
  tags: [mcp, tool, design, naming, schema, annotations]
---

# Purpose

Design MCP tools that LLMs can discover, understand, and use effectively. This covers naming conventions, description writing, schema design, annotations, response formatting, and the patterns that make the difference between tools that work well and tools that confuse the model.

# When to use this skill

- Designing the tool surface for a new MCP server
- Improving tool discoverability (names, descriptions)
- Writing inputSchema that LLMs fill in correctly
- Adding tool annotations (readOnlyHint, destructiveHint, etc.)
- Formatting tool responses for LLM consumption

# Do not use this skill when

- Implementing server infrastructure → use `mcp-development`
- Working on schemas in isolation → use `mcp-schema-contracts`
- Working on resources/prompts → use `mcp-resources-prompts`

# Operating procedure

## Step 1 — Name the tool well

**Convention:** `<domain>_<action>_<object>` or `<action>_<object>`

```
✅ Good names:
  github_create_issue, github_list_repos, github_search_code
  search_documents, get_weather, create_user

❌ Bad names:
  doThing, helper, process, handle_request
  github_api_v3_issues_endpoint
```

**Rules:**
- Use `snake_case` (MCP convention)
- Start with a verb or domain prefix
- Be specific: `search_issues` not `search`
- Keep under 40 characters

## Step 2 — Write the description

The tool description is the most important field for LLM tool selection. The model reads this to decide whether to use the tool.

```
✅ Good description:
  "Search GitHub issues by query. Returns title, state, author, and URL.
   Supports filtering by state (open/closed) and label."

❌ Bad description:
  "Issues search"
  "Searches for things"
  "Use this tool to search for issues in the GitHub API"
```

**Rules:**
- First sentence: what the tool does
- Second sentence: what it returns
- Optional third: notable capabilities or constraints
- Keep under 200 characters for best LLM comprehension
- No meta-commentary ("use this tool when...") — the LLM figures that out from the description

## Step 3 — Design the inputSchema

See `mcp-schema-contracts` for full schema patterns. Key principles:

```json
{
  "inputSchema": {
    "type": "object",
    "properties": {
      "query": {
        "type": "string",
        "description": "Search query — matches against title and body text"
      },
      "state": {
        "type": "string",
        "enum": ["open", "closed", "all"],
        "default": "open",
        "description": "Issue state filter"
      },
      "limit": {
        "type": "integer",
        "minimum": 1,
        "maximum": 100,
        "default": 20,
        "description": "Maximum number of results to return"
      }
    },
    "required": ["query"]
  }
}
```

**Rules:**
- Every property MUST have a `description`
- Use `enum` for parameters with known valid values
- Use `default` for optional parameters
- Set `minimum`/`maximum` for numeric bounds
- Keep `required` minimal — prefer optional params with good defaults
- Max 8 parameters per tool. Beyond that, split into multiple tools.

## Step 4 — Set annotations

```json
{
  "annotations": {
    "readOnlyHint": true,
    "destructiveHint": false,
    "idempotentHint": true,
    "openWorldHint": true
  }
}
```

| Annotation | When true | When false |
|-----------|-----------|------------|
| `readOnlyHint` | Search, list, read, get | Create, update, delete |
| `destructiveHint` | Delete, drop, overwrite | Create, update, read |
| `idempotentHint` | PUT-like updates, reads | POST-like creates, deletes |
| `openWorldHint` | Calls external APIs | Only accesses local data |

Hosts use these to decide about confirmation prompts. Set them accurately.

## Step 5 — Format responses

**For text responses:**
```json
{
  "content": [{
    "type": "text",
    "text": "Found 3 issues:\n1. #42 Fix login bug (open)\n2. #43 Add dark mode (open)\n3. #44 Update docs (closed)"
  }],
  "isError": false
}
```

**For structured responses (with outputSchema):**
```json
{
  "content": [{ "type": "text", "text": "{\"count\": 3, \"issues\": [...]}" }],
  "structuredContent": { "count": 3, "issues": [...] }
}
```

**Response formatting rules:**
- Keep responses concise — the LLM has limited context
- For lists: return the most important fields, not every field
- For errors: include what went wrong AND what to try next
- Set `isError: true` for business logic errors (not protocol errors)
- Paginate large results — return a page with a "next page" indicator

## Step 6 — Error responses

```json
{
  "content": [{
    "type": "text",
    "text": "Error: Repository 'nonexistent' not found. Verify the owner/repo format (e.g., 'octocat/hello-world')."
  }],
  "isError": true
}
```

**Error message pattern:** "Error: <what happened>. <what to try next>."

# Anti-patterns

- **God tool:** One tool that does everything via a `action` parameter. Split into focused tools.
- **Mirror tool:** Exposing every API parameter as a tool parameter. Curate the most useful ones.
- **Silent failure:** Tool returns empty result instead of error message when something goes wrong.
- **Verbose responses:** Returning full API responses instead of curated, relevant fields.
- **Missing descriptions:** Properties without descriptions — the LLM won't know what to pass.

# Decision rules

- One tool = one action. If a tool does two things, split it.
- Prefer enum constraints over free text for predictable parameters
- Test tool selection: does the LLM pick the right tool from a list of 10+?
- Test argument filling: does the LLM fill in the arguments correctly from a natural language request?
- If tools > 30, split into multiple MCP servers

# Output requirements

1. Tools with clear names, descriptions, and schemas
2. Annotations set correctly for all tools
3. Response formatting verified (concise, parseable)
4. Error messages are actionable

# Related skills

- `mcp-schema-contracts` — deep dive on schema design
- `mcp-resources-prompts` — non-tool primitives
- `mcp-development` — full server development lifecycle
- `mcp-testing-evals` — testing tool effectiveness

# Failure handling

- If an LLM picks the wrong tool, improve the description to be more specific
- If an LLM fills arguments incorrectly, add better descriptions and examples to the schema properties
- If responses are too long for the LLM context, add pagination or summary modes
