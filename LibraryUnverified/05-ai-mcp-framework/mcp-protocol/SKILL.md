---
name: mcp-protocol
description: Applies MCP protocol knowledge — architecture, primitives (tools/resources/prompts), transports (stdio/HTTP+SSE), capability negotiation, and fail-closed policies — when building or debugging MCP servers and clients. Trigger on "build MCP server", "MCP tool", "MCP transport", "JSON-RPC", "MCP client". Do NOT use for node-agent-patterns (Node.js agent patterns beyond MCP), external-api-client (REST API clients), or prompt-crafting (prompt engineering).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-protocol
  maturity: draft
  risk: low
  tags: [mcp, protocol]
---

# Purpose
Provides Model Context Protocol (MCP) fundamentals: architecture, primitives (tools/resources/prompts), transports (stdio/HTTP+SSE), capability negotiation, and lifecycle management. Use this when building, debugging, or integrating MCP servers and clients.

# When to use this skill
Use when:
- Building an MCP server to expose tools/resources to AI clients
- Integrating an MCP client into an AI application
- Debugging MCP communication issues
- Understanding MCP capability negotiation

Do NOT use when:
- Using existing MCP servers (just configure them)
- Building non-MCP integrations
- The project doesn't involve AI tool calling

# Operating procedure

## 1. Understand MCP architecture

### Participants
- **MCP Host**: AI application (Claude Desktop, VS Code, etc.) that coordinates clients
- **MCP Client**: Component in the host that connects to one MCP server
- **MCP Server**: Program providing tools/resources to clients

```
[AI Application (Host)]
    ├── [MCP Client 1] ←→ [MCP Server: filesystem]
    ├── [MCP Client 2] ←→ [MCP Server: database]
    └── [MCP Client 3] ←→ [MCP Server: custom-tools]
```

### Layers
- **Data layer**: JSON-RPC 2.0 protocol for messages
- **Transport layer**: stdio (local) or HTTP+SSE (remote)

## 2. MCP primitives

### Tools (server → client)
Functions that AI can invoke to perform actions:
```json
{
  "name": "read_file",
  "description": "Read contents of a file",
  "inputSchema": {
    "type": "object",
    "properties": {
      "path": { "type": "string", "description": "File path" }
    },
    "required": ["path"]
  }
}
```

Discovery: `tools/list` → Returns all available tools
Execution: `tools/call` → Executes a specific tool

### Resources (server → client)
Data sources providing context:
```json
{
  "uri": "file:///project/README.md",
  "name": "Project README",
  "mimeType": "text/markdown"
}
```

Discovery: `resources/list` → Available resources
Retrieval: `resources/read` → Get resource content

### Prompts (server → client)
Reusable interaction templates:
```json
{
  "name": "summarize",
  "description": "Summarize a document",
  "arguments": [
    { "name": "content", "description": "Text to summarize", "required": true }
  ]
}
```

Discovery: `prompts/list`
Retrieval: `prompts/get`

### Sampling (client → server)
Server requests LLM completion from client:
- `sampling/createMessage` → Request model completion
- Useful for MCP servers that need AI without bundling a model

### Elicitation (client → server)
Server requests user input from client:
- `elicitation/request` → Ask user a question
- Returns user's response

## 3. Transport options

### stdio transport (local)
- Process communication via stdin/stdout
- No network overhead
- Single client per server instance

```bash
# Launch MCP server via stdio
node ./dist/server.js
```

Configuration in claude_desktop_config.json:
```json
{
  "mcpServers": {
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "/path"]
    }
  }
}
```

### Streamable HTTP transport (remote)
- HTTP POST for client → server
- Server-Sent Events for streaming
- Multiple clients per server
- Supports OAuth authentication

## 4. Lifecycle management

### Initialization sequence
1. Client sends `initialize` with capabilities
2. Server responds with its capabilities
3. Client sends `initialized` notification
4. Connection ready for requests

```typescript
// Client capabilities example
{
  "protocolVersion": "2024-11-05",
  "capabilities": {
    "sampling": {}  // Client can handle sampling requests
  },
  "clientInfo": { "name": "my-client", "version": "1.0.0" }
}

// Server response
{
  "protocolVersion": "2024-11-05",
  "capabilities": {
    "tools": {},      // Server provides tools
    "resources": {}   // Server provides resources
  },
  "serverInfo": { "name": "my-server", "version": "1.0.0" }
}
```

### Capability negotiation
Only use features both sides support:
- Client wants tools → Server must have `capabilities.tools`
- Server wants sampling → Client must have `capabilities.sampling`

## 5. Building an MCP server

### Minimal TypeScript server
```typescript
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";

const server = new Server(
  { name: "my-server", version: "1.0.0" },
  { capabilities: { tools: {} } }
);

// Register tool
server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools: [{
    name: "hello",
    description: "Say hello",
    inputSchema: { type: "object", properties: {} }
  }]
}));

server.setRequestHandler(CallToolRequestSchema, async (req) => {
  if (req.params.name === "hello") {
    return { content: [{ type: "text", text: "Hello, world!" }] };
  }
  throw new Error("Unknown tool");
});

// Connect via stdio
const transport = new StdioServerTransport();
await server.connect(transport);
```

## 6. Fail-closed policies
Always implement defensive behavior:
```typescript
// Validate all inputs
if (!isValidPath(request.params.path)) {
  throw new Error("Invalid path");
}

// Whitelist allowed operations
const ALLOWED_COMMANDS = ["ls", "cat", "grep"];
if (!ALLOWED_COMMANDS.includes(cmd)) {
  throw new Error("Command not allowed");
}

// Set timeouts
const result = await Promise.race([
  executeCommand(cmd),
  timeout(30000).then(() => { throw new Error("Timeout"); })
]);
```

# Output defaults
MCP servers should expose:
- `tools/list` → Array of available tools
- `tools/call` → Tool execution results
- Optional: `resources/list`, `resources/read`
- Optional: `prompts/list`, `prompts/get`

# References
- MCP Specification: https://modelcontextprotocol.io/specification/
- MCP Introduction: https://modelcontextprotocol.io/introduction
- MCP Architecture: https://modelcontextprotocol.io/docs/concepts/architecture
- MCP SDK: @modelcontextprotocol/sdk

# Failure handling
- **Capability mismatch**: Check capabilities before using features
- **Transport errors**: Implement reconnection for HTTP transport
- **Tool errors**: Return structured error responses, not crashes
- **Version mismatch**: Check protocolVersion during initialization
