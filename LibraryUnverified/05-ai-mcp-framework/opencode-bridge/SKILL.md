---
name: opencode-bridge
description: "Connect MCP servers to the OpenCode agent runtime. Use when configuring MCP servers for OpenCode, adapting existing MCP servers to work with OpenCode's conventions, or troubleshooting OpenCode-MCP integration issues."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: opencode-bridge
  maturity: stable
  risk: low
  tags: [opencode, mcp, bridge, integration]
---

# Purpose

Bridge MCP servers into the OpenCode agent runtime. OpenCode is an open-source terminal-based AI coding agent that supports MCP servers for extending its capabilities. This skill covers OpenCode-specific configuration, conventions, and integration patterns.

# When to use this skill

- Configuring MCP servers for use with OpenCode
- Adapting an existing MCP server to work with OpenCode's agent model
- Troubleshooting OpenCode-MCP connection issues
- Understanding how OpenCode discovers and invokes MCP tools

# Do not use this skill when

- Targeting Claude Desktop, VS Code, Codex, or other hosts → use `mcp-host-integration`
- Building the MCP server itself → use `mcp-development`
- Working on OpenCode's internal primitives → use `opencode-primitives`

# Operating procedure

## Phase 1 — OpenCode MCP configuration

OpenCode reads MCP server configuration from its project config file (`.opencode/config.json`) or environment config.

**Local stdio server:**
```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["/absolute/path/to/dist/index.js"],
      "env": {
        "API_KEY": "your-key"
      }
    }
  }
}
```

**Remote Streamable HTTP server:**
```json
{
  "mcpServers": {
    "remote-server": {
      "url": "https://my-server.example.com/mcp",
      "headers": {
        "Authorization": "Bearer <token>"
      }
    }
  }
}
```

## Phase 2 — OpenCode integration patterns

**Tool usage in OpenCode:**
- OpenCode discovers tools via `tools/list` during initialization
- The agent decides when to call tools based on descriptions and the user's request
- Tool results are incorporated into the agent's context for further reasoning

**Best practices for OpenCode:**
- Keep tool descriptions concise and action-oriented — OpenCode's agent model benefits from clarity
- Return text content (not just structured content) — OpenCode displays text to the user
- Tools that read/write files should use absolute paths — OpenCode runs from the project root
- Error messages should suggest next steps — the agent uses them to recover

**Resources and Prompts:**
- OpenCode supports MCP tools as its primary integration point
- Resource and prompt support varies — test with the specific OpenCode version

## Phase 3 — Debugging OpenCode connections

1. Verify the server works independently with MCP Inspector first
2. Check config file path and JSON validity
3. Ensure command paths are absolute
4. Check that env vars are set in the config `env` block
5. Restart OpenCode after config changes
6. Check OpenCode logs for MCP connection errors

# Decision rules

- Use stdio transport for local servers with OpenCode (default and best-supported)
- Use absolute paths in all config entries
- Set secrets in config `env` block, not system environment
- Test with MCP Inspector before connecting to OpenCode
- Keep tool count reasonable (<30) for best agent tool selection

# Output requirements

1. OpenCode configuration file with MCP server entry
2. Verified tool discovery in OpenCode
3. Verified tool invocation end-to-end

# Related skills

- `opencode-primitives` — OpenCode paths, naming, and config primitives
- `mcp-host-integration` — general host configuration patterns
- `mcp-inspector-debugging` — debugging server issues before connecting to OpenCode

# Failure handling

- If OpenCode doesn't see tools, restart it after config changes — config is read at startup
- If connection times out, check that the server process starts within the timeout window
- If tools are visible but calls fail, check server stdout for non-JSON-RPC output (breaks stdio)
