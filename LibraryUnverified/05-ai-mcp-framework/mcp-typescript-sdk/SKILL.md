---
name: mcp-typescript-sdk
description: "Build MCP servers in TypeScript using the official @modelcontextprotocol/sdk. Use when creating MCP tools/resources/prompts in TypeScript, using Zod for schema definitions, configuring TypeScript MCP transport, or applying the McpServer high-level API vs the low-level Server class."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-typescript-sdk
  maturity: stable
  risk: low
  tags: [mcp, typescript, sdk, zod, node]
---

# Purpose

Build MCP servers in TypeScript using the official `@modelcontextprotocol/sdk`. This is the recommended SDK for MCP servers — it has the best host compatibility and uses Zod for type-safe schema definitions.

# When to use this skill

- Building an MCP server in TypeScript/Node.js
- Using the McpServer high-level API for tool/resource/prompt registration
- Configuring TypeScript MCP transport (stdio or Streamable HTTP)
- Choosing between McpServer (high-level) vs Server (low-level) API

# Do not use this skill when

- Building in Python → use `mcp-python-fastmcp`
- Building in Go → use `mcp-go-server`
- Need general server design guidance → use `mcp-development`

# Operating procedure

## Phase 1 — Project setup

**Using the scaffolder:**
```bash
npx @anthropic-ai/create-mcp-server my-server
cd my-server && npm install
```

**Manual setup:**
```bash
mkdir my-server && cd my-server
npm init -y
npm install @modelcontextprotocol/sdk zod
npm install -D typescript @types/node
npx tsc --init
```

`tsconfig.json` essentials:
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "Node16",
    "moduleResolution": "Node16",
    "outDir": "dist",
    "strict": true
  }
}
```

## Phase 2 — McpServer API (high-level, recommended)

```typescript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer({
  name: "my-server",
  version: "1.0.0",
});
```

### Register tools

```typescript
server.tool(
  "search_issues",
  {
    query: z.string().describe("Search query text"),
    state: z.enum(["open", "closed", "all"]).default("open").describe("Issue state filter"),
    limit: z.number().int().min(1).max(100).default(20).describe("Max results"),
  },
  async ({ query, state, limit }) => {
    const results = await api.searchIssues(query, state, limit);
    return {
      content: [{
        type: "text",
        text: JSON.stringify(results, null, 2),
      }],
    };
  }
);
```

**Zod schema → JSON Schema:** The SDK auto-converts Zod schemas to JSON Schema for `inputSchema`. Use `.describe()` on every field.

### Register tools with annotations

```typescript
server.tool(
  "delete_issue",
  { issue_id: z.number().describe("Issue ID to delete") },
  {
    annotations: {
      readOnlyHint: false,
      destructiveHint: true,
      idempotentHint: false,
    },
  },
  async ({ issue_id }) => {
    await api.deleteIssue(issue_id);
    return { content: [{ type: "text", text: `Deleted issue #${issue_id}` }] };
  }
);
```

### Register resources

```typescript
server.resource(
  "schema",
  "db://schema",
  { description: "Database schema definition", mimeType: "text/sql" },
  async (uri) => ({
    contents: [{
      uri: uri.href,
      mimeType: "text/sql",
      text: await getSchemaSQL(),
    }],
  })
);
```

### Register prompts

```typescript
server.prompt(
  "analyze_data",
  { dataset: z.string().describe("Dataset name to analyze") },
  ({ dataset }) => ({
    messages: [{
      role: "user",
      content: {
        type: "text",
        text: `Analyze the '${dataset}' dataset using the available query tools.`,
      },
    }],
  })
);
```

### Connect transport

```typescript
// stdio (local servers)
const transport = new StdioServerTransport();
await server.connect(transport);
```

```typescript
// Streamable HTTP (remote servers)
import { StreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/streamableHttp.js";
import express from "express";

const app = express();
app.use(express.json());

app.post("/mcp", async (req, res) => {
  const transport = new StreamableHTTPServerTransport({ sessionIdGenerator: () => crypto.randomUUID() });
  res.on("close", () => transport.close());
  await server.connect(transport);
  await transport.handleRequest(req, res, req.body);
});

app.listen(3000);
```

## Phase 3 — Server API (low-level, for advanced use)

Use the low-level `Server` class when you need custom request handling:

```typescript
import { Server } from "@modelcontextprotocol/sdk/server/index.js";

const server = new Server(
  { name: "my-server", version: "1.0.0" },
  { capabilities: { tools: { listChanged: true } } }
);

server.setRequestHandler("tools/list", async () => ({
  tools: [
    {
      name: "search",
      description: "Search items",
      inputSchema: { type: "object", properties: { query: { type: "string" } }, required: ["query"] },
    },
  ],
}));

server.setRequestHandler("tools/call", async (request) => {
  const { name, arguments: args } = request.params;
  // ... handle tool call
});
```

## Phase 4 — Structured output (outputSchema)

```typescript
server.tool(
  "get_weather",
  { location: z.string().describe("City name") },
  {
    outputSchema: z.object({
      temperature: z.number().describe("Temperature in Celsius"),
      conditions: z.string().describe("Weather conditions"),
    }),
  },
  async ({ location }) => {
    const data = await fetchWeather(location);
    return {
      content: [{ type: "text", text: JSON.stringify(data) }],
      structuredContent: data,
    };
  }
);
```

## Phase 5 — Build and test

```bash
npm run build
npx @modelcontextprotocol/inspector node dist/index.js
```

# Decision rules

- Use `McpServer` (high-level) unless you need custom protocol handling — it handles capabilities, serialization, and transport wiring
- Use `Server` (low-level) when you need to intercept raw JSON-RPC messages or implement custom capabilities
- Always use Zod `.describe()` on every field — this generates the `description` in JSON Schema
- Use `z.enum()` for constrained string values
- Use `z.default()` for optional parameters with sensible defaults
- Use `console.error()` for logging in stdio mode — `console.log()` corrupts the transport
- Set `"type": "module"` in `package.json` or use `.mjs` extensions for ESM imports

# Output requirements

1. TypeScript project with `@modelcontextprotocol/sdk` and `zod`
2. All tools registered with Zod schemas and `.describe()` on every field
3. Transport configured (stdio or Streamable HTTP)
4. Builds with `tsc` and passes MCP Inspector verification

# Related skills

- `mcp-python-fastmcp` — Python alternative
- `mcp-go-server` — Go alternative
- `mcp-tool-design` — tool design patterns
- `mcp-inspector-debugging` — debugging TypeScript MCP servers

# Failure handling

- If import errors: ensure `package.json` has `"type": "module"` and paths use `.js` extension (not `.ts`)
- If Zod validation fails at runtime: check that arguments match the Zod schema (number vs string coercion is common)
- If `StdioServerTransport` doesn't work: ensure no `console.log()` calls in the code
- If `@anthropic-ai/create-mcp-server` doesn't exist: use `npm init` + manual setup as fallback
