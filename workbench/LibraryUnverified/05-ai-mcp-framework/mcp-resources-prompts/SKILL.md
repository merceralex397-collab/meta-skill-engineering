---
name: mcp-resources-prompts
description: "Implement MCP Resources and Prompts — the non-tool primitives. Use when exposing read-only context data as MCP resources, creating reusable prompt templates, implementing resource subscriptions and change notifications, or choosing between resources vs tools for a given data surface."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-resources-prompts
  maturity: stable
  risk: low
  tags: [mcp, resources, prompts, context, templates]
---

# Purpose

Implement the two non-tool MCP primitives: **Resources** (read-only context data) and **Prompts** (reusable interaction templates). These complement tools by providing static context and structured conversation starters.

# When to use this skill

- Exposing read-only data (file contents, schemas, configs) as MCP resources
- Creating reusable prompt templates for common interaction patterns
- Implementing resource subscriptions for dynamic data
- Deciding whether something should be a resource, tool, or prompt

# Do not use this skill when

- Implementing executable tools → use `mcp-tool-design`
- General server development → use `mcp-development`
- SDK-specific implementation → use `mcp-typescript-sdk` or `mcp-python-fastmcp`

# Primitive comparison

| Aspect | Tool | Resource | Prompt |
|--------|------|----------|--------|
| Control model | Model-controlled (LLM decides) | Application-controlled (host decides) | User-controlled (user selects) |
| Purpose | Perform actions | Provide context data | Structure interactions |
| Discovery | `tools/list` | `resources/list` | `prompts/list` |
| Invocation | `tools/call` | `resources/read` | `prompts/get` |
| Side effects | May have side effects | Read-only | None (template only) |
| Example | Query database | Return schema definition | "Analyze this data using the query tools" |

**Decision rule:** If the LLM needs to take action → tool. If the host needs context → resource. If the user needs a conversation starter → prompt.

# Resources

## Declaring the capability

```json
{
  "capabilities": {
    "resources": {
      "subscribe": true,
      "listChanged": true
    }
  }
}
```

## Listing resources

Resources are discovered via `resources/list` (supports pagination):

```json
{
  "resources": [
    {
      "uri": "file:///project/schema.sql",
      "name": "schema.sql",
      "description": "Database schema definition",
      "mimeType": "text/sql"
    }
  ]
}
```

## Resource templates (parameterized resources)

Use URI templates (RFC 6570) for dynamic resources:

```json
{
  "resourceTemplates": [
    {
      "uriTemplate": "db://tables/{table_name}/schema",
      "name": "Table Schema",
      "description": "Schema for a specific database table"
    }
  ]
}
```

## Reading resources

`resources/read` returns either text or binary (base64 blob):

**Text:**
```json
{
  "contents": [{ "uri": "file:///schema.sql", "mimeType": "text/sql", "text": "CREATE TABLE..." }]
}
```

**Binary:**
```json
{
  "contents": [{ "uri": "file:///logo.png", "mimeType": "image/png", "blob": "base64..." }]
}
```

## Subscriptions

For data that changes over time, support `resources/subscribe`:

1. Client sends `resources/subscribe` with a URI
2. When the resource changes, server sends `notifications/resources/updated`
3. Client re-reads the resource to get the updated content

## Resource annotations

Annotate resources with audience, priority, and modification time:
```json
{
  "annotations": {
    "audience": ["assistant"],
    "priority": 0.9,
    "lastModified": "2025-01-12T15:00:58Z"
  }
}
```

## URI schemes

- `file://` — filesystem-like resources (need not be actual files)
- `https://` — web resources the client can fetch directly
- `git://` — version control resources
- Custom schemes (e.g., `db://`, `config://`) — follow RFC 3986

## Implementation examples

**TypeScript:**
```typescript
server.resource("schema", "db://schema", async (uri) => ({
  contents: [{ uri: uri.href, mimeType: "text/sql", text: await getSchema() }],
}));
```

**Python (FastMCP):**
```python
@mcp.resource("db://schema")
def get_schema() -> str:
    """Database schema definition."""
    return read_schema_file()
```

# Prompts

## Declaring the capability

```json
{
  "capabilities": {
    "prompts": { "listChanged": true }
  }
}
```

## Defining prompts

Prompts have a name, optional description, and optional arguments:

```json
{
  "prompts": [
    {
      "name": "analyze_data",
      "description": "Analyze a dataset using available query tools",
      "arguments": [
        { "name": "dataset", "description": "Name of the dataset to analyze", "required": true }
      ]
    }
  ]
}
```

## Retrieving prompts

`prompts/get` returns a list of messages that the host inserts into the conversation:

```json
{
  "messages": [
    {
      "role": "user",
      "content": { "type": "text", "text": "Please analyze the 'sales' dataset using the available query tools. Start by listing tables, then examine schema, then run aggregation queries." }
    }
  ]
}
```

## Prompts can embed resources

```json
{
  "messages": [
    {
      "role": "user",
      "content": {
        "type": "resource",
        "resource": { "uri": "db://tables/sales/schema", "mimeType": "text/sql", "text": "CREATE TABLE sales..." }
      }
    },
    {
      "role": "user",
      "content": { "type": "text", "text": "Using the schema above, write a query to find top-selling products." }
    }
  ]
}
```

## Implementation examples

**TypeScript:**
```typescript
server.prompt("analyze_data", { dataset: z.string() }, ({ dataset }) => ({
  messages: [{
    role: "user",
    content: { type: "text", text: `Analyze the '${dataset}' dataset using the query tools.` }
  }],
}));
```

**Python:**
```python
@mcp.prompt()
def analyze_data(dataset: str) -> str:
    """Analyze a dataset using available query tools."""
    return f"Analyze the '{dataset}' dataset using the available query tools."
```

# Decision rules

- Resources are for context the host presents proactively. Tools are for actions the LLM takes.
- Use resource templates when you have a large set of similar resources (e.g., one per table, one per file)
- Prompts are user-triggered (like slash commands). Don't create prompts for things the LLM should decide on its own.
- Annotate resources with `audience: ["assistant"]` for LLM context and `audience: ["user"]` for human display
- Implement `listChanged` notification when resources/prompts can change during a session

# Output requirements

1. Resources with URIs, descriptions, and appropriate MIME types
2. Prompts with clear descriptions and typed arguments
3. Subscription support for dynamic resources
4. Declared capabilities matching implemented features

# Related skills

- `mcp-tool-design` — the third MCP primitive (tools)
- `mcp-schema-contracts` — schema design for resource content
- `mcp-development` — full server development lifecycle

# Failure handling

- If a resource URI doesn't resolve, return JSON-RPC error code `-32002` (resource not found)
- If a prompt argument is missing, return `-32602` (invalid params)
- If host doesn't show resources, verify the `resources` capability is declared in `initialize` response
- Not all hosts support resources and prompts equally — test on each target host
