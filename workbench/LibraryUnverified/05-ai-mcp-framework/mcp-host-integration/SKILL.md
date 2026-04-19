---
name: mcp-host-integration
description: "Configure and integrate MCP servers with specific host applications — Claude Desktop, VS Code/Copilot, Cursor, OpenCode, Codex, Gemini CLI. Use when wiring an MCP server into a host, debugging host-specific connection issues, or adapting a server for multiple hosts' differing capabilities."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-host-integration
  maturity: stable
  risk: low
  tags: [mcp, host, integration, claude, copilot, codex, cursor]
---

# Purpose

Wire an MCP server into one or more host applications. Each host has different configuration formats, transport preferences, and capability support. This skill provides the specific config patterns for each major host.

# When to use this skill

- Configuring an MCP server for a specific host (Claude Desktop, VS Code, Cursor, etc.)
- Debugging why a host doesn't see MCP tools
- Making a server work across multiple hosts simultaneously
- Understanding host-specific capability differences

# Do not use this skill when

- Building the MCP server itself → use `mcp-development` or `mcp-builder`
- Specifically targeting ChatGPT/Codex → use `mcp-chatgpt-app-bridge`
- Working on transport or auth implementation → use `mcp-auth-transports`

# Host configuration reference

## Claude Desktop

Config file: `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows)

```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["/absolute/path/to/dist/index.js"],
      "env": {
        "API_KEY": "sk-..."
      }
    }
  }
}
```

- Transport: stdio only (launches server as subprocess)
- Supports: tools, resources, prompts, sampling
- Restart Claude Desktop after config changes

## VS Code / GitHub Copilot

Config file: `.vscode/mcp.json` (workspace) or User Settings

```json
{
  "servers": {
    "my-server": {
      "command": "node",
      "args": ["dist/index.js"],
      "env": {
        "API_KEY": "sk-..."
      }
    }
  }
}
```

For remote servers:
```json
{
  "servers": {
    "remote-server": {
      "url": "https://my-server.example.com/mcp"
    }
  }
}
```

- Transport: stdio (local) or Streamable HTTP (remote)
- Supports: tools (primary), resources (via context inclusion)
- Click "Start" in the MCP server list panel, or reload window

## Cursor

```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["dist/index.js"]
    }
  }
}
```

Config location: Cursor Settings → MCP, or `.cursor/mcp.json`
- Transport: stdio
- Supports: tools

## OpenCode

Config file: `.opencode/config.json`

```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["dist/index.js"]
    }
  }
}
```

- Transport: stdio (primary)
- Supports: tools, resources

## OpenAI Codex

See `mcp-chatgpt-app-bridge` for full details.

## Gemini CLI

Gemini CLI supports MCP servers via its settings file:
```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["dist/index.js"]
    }
  }
}
```

# Operating procedure

1. **Identify the target host(s)** and their config file locations
2. **Choose transport** — most hosts default to stdio for local servers
3. **Write the config** with absolute paths (relative paths cause failures)
4. **Set environment variables** in the config's `env` block (not shell env — the host may not inherit it)
5. **Restart the host** — most hosts only read MCP config at startup
6. **Verify tools appear** in the host's tool/command palette
7. **Test a tool call** to confirm end-to-end connectivity

# Decision rules

- Always use absolute paths in server configs — hosts may launch subprocesses from any working directory
- Set secrets via the config `env` block, not system environment — the subprocess may not inherit shell env
- If targeting multiple hosts, stick to tools only (broadest support). Resources and prompts have uneven host support.
- The `initialize` handshake happens automatically — hosts handle lifecycle. Your server just needs to respond to it correctly.
- If a host shows tools but calls fail, the issue is usually in `inputSchema` — hosts validate arguments before sending `tools/call`

# Output requirements

1. Host configuration file with correct format and absolute paths
2. Verified tool discovery (tools appear in host UI)
3. Verified tool invocation (at least one successful `tools/call`)
4. Documentation of any host-specific limitations

# Related skills

- `mcp-chatgpt-app-bridge` — OpenAI-specific integration
- `mcp-auth-transports` — transport and auth implementation
- `mcp-inspector-debugging` — debugging connection issues
- `opencode-bridge` — OpenCode-specific conventions

# Failure handling

- If host shows "server failed to start": check command path is absolute, binary exists, and `node`/`python`/`uv` is in the host's PATH
- If tools don't appear: check `initialize` response declares `tools` capability, verify with MCP Inspector first
- If tools appear but calls return errors: check server stdout for non-JSON-RPC output pollution
- If env vars aren't working: set them in the config `env` block, not system env. Some hosts sandbox the process environment.
