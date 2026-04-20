---
name: get-started
description: "Bootstrap a first MCP server from zero to working connection. Use when the user says 'get started with MCP', 'create my first MCP server', 'set up MCP', or wants a hello-world MCP experience. Covers project scaffolding, first tool registration, transport wiring, and host verification."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: get-started
  maturity: stable
  risk: low
  tags: [mcp, onboarding, quickstart, scaffold]
---

# Purpose

Get a developer from zero to a working MCP server connected to a host in one pass. This skill produces a runnable server with at least one tool, verified via MCP Inspector or a real host.

# When to use this skill

- User says "get started with MCP", "create my first MCP server", "hello world MCP"
- User wants to scaffold a new MCP server project from scratch
- User needs to verify their MCP development environment works end-to-end

# Do not use this skill when

- An MCP server already exists and needs improvement → use `mcp-development` or `mcp-tool-design`
- The task is about building a production MCP server with many tools → use `mcp-builder`
- The task is about connecting to an existing MCP server as a client → use `mcp-host-integration`

# Operating procedure

## Phase 1 — Choose stack and scaffold

1. Ask: TypeScript or Python? (Default to TypeScript for broadest host compatibility.)
2. Scaffold the project:

**TypeScript (recommended):**
```bash
npx @anthropic-ai/create-mcp-server my-server
cd my-server && npm install
```

**Python:**
```bash
uv init my-server && cd my-server
uv venv && source .venv/bin/activate
uv add "mcp[cli]"
```

3. The scaffolder creates the project structure, dependencies, and a starter server file.

## Phase 2 — Register a first tool

MCP servers expose three primitives: **Tools**, **Resources**, and **Prompts**. Start with a tool.

**TypeScript — using `@modelcontextprotocol/sdk`:**
```typescript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer({ name: "my-server", version: "1.0.0" });

server.tool("greet", { name: z.string() }, async ({ name }) => ({
  content: [{ type: "text", text: `Hello, ${name}!` }],
}));

const transport = new StdioServerTransport();
await server.connect(transport);
```

**Python — using FastMCP:**
```python
from mcp.server.fastmcp import FastMCP

mcp = FastMCP("my-server")

@mcp.tool()
def greet(name: str) -> str:
    """Greet someone by name."""
    return f"Hello, {name}!"

if __name__ == "__main__":
    mcp.run(transport="stdio")
```

## Phase 3 — Verify with MCP Inspector

The MCP Inspector is the official debugging tool. Run it against your server:

```bash
npx @modelcontextprotocol/inspector node dist/index.js        # TypeScript
npx @modelcontextprotocol/inspector uv run python server.py   # Python
```

In the Inspector UI (opens at `http://localhost:6274`):
1. Verify `tools/list` returns your tool
2. Call `tools/call` with test arguments
3. Confirm the response content is correct

## Phase 4 — Connect to a host

Wire the server into a real MCP host to confirm end-to-end:

**Claude Desktop** — edit `~/Library/Application Support/Claude/claude_desktop_config.json`:
```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["/absolute/path/to/dist/index.js"]
    }
  }
}
```

**VS Code / GitHub Copilot** — add to `.vscode/mcp.json`:
```json
{
  "servers": {
    "my-server": {
      "command": "node",
      "args": ["dist/index.js"]
    }
  }
}
```

**OpenCode** — add to `.opencode/config.json` under `mcpServers`.

Restart the host. Verify the tool appears in the host's tool list.

# Decision rules

- Default to **stdio** transport for local development. Only use Streamable HTTP if the server must be remote.
- Default to **TypeScript** unless the user's project is Python-only.
- Register one tool first, verify it works, then expand. Do not build five tools before testing one.
- Use `inputSchema` with Zod (TS) or type hints + docstrings (Python) — the SDK auto-generates JSON Schema from these.

# Output requirements

A completed get-started pass produces:
1. A runnable project directory with dependencies installed
2. At least one registered MCP tool with input schema
3. Successful `tools/list` and `tools/call` via MCP Inspector
4. A working host configuration (Claude Desktop, VS Code, or OpenCode)

# Related skills

- `mcp-builder` — full server development guide with phases and evals
- `mcp-typescript-sdk` — deep TypeScript SDK patterns
- `mcp-python-fastmcp` — deep Python FastMCP patterns
- `mcp-inspector-debugging` — advanced debugging techniques

# Failure handling

- If `npx @anthropic-ai/create-mcp-server` fails, fall back to manual project setup with `npm init` + `npm install @modelcontextprotocol/sdk zod`
- If MCP Inspector shows no tools, check: (a) server compiles without errors, (b) stdio transport is wired, (c) server is not writing non-JSON-RPC to stdout
- If the host does not see tools, verify the config path is absolute and the host was restarted after config change
