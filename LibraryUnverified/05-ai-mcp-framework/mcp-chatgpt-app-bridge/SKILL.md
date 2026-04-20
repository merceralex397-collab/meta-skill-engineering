---
name: mcp-chatgpt-app-bridge
description: "Connect an MCP server to ChatGPT as an MCP App or integrate with OpenAI's Codex agent. Use when building MCP servers that ChatGPT or Codex will consume, configuring OpenAI Apps SDK MCP integration, or adapting an existing MCP server for the ChatGPT/Codex host requirements."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-chatgpt-app-bridge
  maturity: stable
  risk: low
  tags: [mcp, chatgpt, openai, codex, apps-sdk]
---

# Purpose

Bridge an MCP server into OpenAI's ecosystem — either as a ChatGPT MCP App (via the OpenAI Apps SDK) or as a tool source for OpenAI Codex. This covers the specific requirements, transport constraints, and configuration patterns for OpenAI hosts.

# When to use this skill

- Building an MCP server intended for ChatGPT consumption
- Configuring OpenAI Codex to use MCP servers
- Adapting an existing MCP server to meet OpenAI host requirements
- Debugging MCP tool visibility or invocation issues in ChatGPT or Codex

# Do not use this skill when

- The target host is Claude, VS Code, or another non-OpenAI host → use `mcp-host-integration`
- Building a general MCP server with no specific OpenAI integration → use `mcp-builder`
- The task is about OpenAI function calling (not MCP) → this skill does not cover native function calling

# Operating procedure

## Phase 1 — Understand the OpenAI MCP surface

OpenAI supports MCP in two contexts:

**ChatGPT MCP Apps (OpenAI Apps SDK):**
- MCP servers are deployed as remote Streamable HTTP endpoints
- ChatGPT discovers tools via standard MCP `tools/list`
- The server must support Streamable HTTP transport (not stdio)
- OAuth 2.1 is used for authentication
- The app is registered via the OpenAI developer dashboard

**OpenAI Codex (CLI agent):**
- Codex supports MCP servers via stdio transport (local) or Streamable HTTP (remote)
- Configuration goes in the project's `.codex/config.json` or via CLI flags
- Codex discovers available tools and uses them during code generation tasks

## Phase 2 — Implement for the target host

### For ChatGPT MCP Apps:

1. Build the MCP server with Streamable HTTP transport (see `mcp-auth-transports`)
2. Implement OAuth 2.1 auth flow (ChatGPT acts as the OAuth client)
3. Deploy to a publicly accessible HTTPS endpoint
4. Register the app on the OpenAI developer platform
5. Declare your MCP endpoint URL and supported scopes

**Key constraints for ChatGPT:**
- MUST use Streamable HTTP transport (stdio not supported for remote ChatGPT apps)
- MUST implement OAuth 2.1 with Dynamic Client Registration
- Tools MUST have clear, descriptive names and descriptions (ChatGPT's model uses these for tool selection)
- Tool responses SHOULD be concise — ChatGPT has context limits

### For OpenAI Codex:

Configure MCP servers in the Codex sandbox:
```json
{
  "mcpServers": {
    "my-server": {
      "command": "node",
      "args": ["dist/index.js"],
      "env": {
        "API_KEY": "..."
      }
    }
  }
}
```

For remote servers, use the URL form:
```json
{
  "mcpServers": {
    "my-remote-server": {
      "url": "https://my-server.example.com/mcp",
      "headers": {
        "Authorization": "Bearer <token>"
      }
    }
  }
}
```

## Phase 3 — Validate

1. **For ChatGPT:** Use the Apps SDK testing tools to verify tool discovery and invocation
2. **For Codex:** Run `codex --mcp-server my-server` and verify tools appear in the tool list
3. Test that tool responses render correctly in the host UI
4. Verify error responses include `isError: true` with actionable messages

# Decision rules

- ChatGPT MCP Apps require Streamable HTTP + OAuth. No exceptions for production.
- Codex supports both stdio (local) and Streamable HTTP (remote). Default to stdio for local dev.
- Tool descriptions are critical for ChatGPT — the model selects tools based on description text. Write descriptions as if explaining the tool to a human assistant.
- Use tool annotations: `readOnlyHint`, `destructiveHint`, `idempotentHint` to help the host make safe decisions.
- Keep tool count manageable — hosts with many tools may have degraded tool selection accuracy.

# Output requirements

1. MCP server with correct transport for the target host
2. Auth implementation matching host requirements
3. Host configuration file (Codex config or Apps SDK registration)
4. Verification that tools are discovered and callable from the host

# Related skills

- `mcp-auth-transports` — OAuth and transport implementation details
- `mcp-host-integration` — non-OpenAI host integration patterns
- `mcp-tool-design` — designing tools that work well across hosts
- `mcp-builder` — comprehensive MCP server development guide

# Failure handling

- If ChatGPT cannot discover tools, verify: (a) endpoint is publicly reachable, (b) CORS/Origin headers are correct, (c) `initialize` response includes `tools` capability
- If Codex cannot connect, check: (a) command path is absolute, (b) server builds without errors, (c) no stdout pollution
- If tools are discovered but invocation fails, check `inputSchema` validity — ChatGPT validates arguments against the schema before calling
