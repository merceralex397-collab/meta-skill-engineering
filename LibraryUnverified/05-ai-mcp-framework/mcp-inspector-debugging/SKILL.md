---
name: mcp-inspector-debugging
description: "Debug MCP servers using the MCP Inspector, server logs, and host traces. Use when MCP tools don't appear in the host, tool calls fail silently, transport connections drop, or the initialize handshake doesn't complete. This skill covers the MCP Inspector tool, common failure patterns, and systematic debugging steps."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-inspector-debugging
  maturity: stable
  risk: low
  tags: [mcp, inspector, debugging, troubleshooting]
---

# Purpose

Systematically debug MCP server issues using the MCP Inspector and other diagnostic tools. The MCP Inspector is the official interactive debugging tool for MCP servers — it connects to a server, lists capabilities, and lets you call tools/read resources/get prompts interactively.

# When to use this skill

- MCP tools don't appear in the host application
- Tool calls fail or return unexpected errors
- The server fails to start or the initialize handshake doesn't complete
- SSE/HTTP transport connections drop or timeout
- Need to verify a server works before connecting to a host

# Do not use this skill when

- Building a new server from scratch → use `get-started` or `mcp-development`
- Designing tool schemas → use `mcp-tool-design`
- Working on auth implementation → use `mcp-auth-transports`

# The MCP Inspector

## Installation and basic usage

```bash
npx @modelcontextprotocol/inspector <command> [args...]
```

**For stdio servers:**
```bash
npx @modelcontextprotocol/inspector node dist/index.js
npx @modelcontextprotocol/inspector uv run python server.py
npx @modelcontextprotocol/inspector ./my-go-binary
```

**For Streamable HTTP servers:**
```bash
npx @modelcontextprotocol/inspector --url https://my-server.example.com/mcp
```

The Inspector opens a web UI at `http://localhost:6274` with tabs for:
- **Tools** — list all tools, call them with arguments, see responses
- **Resources** — list resources, read their contents
- **Prompts** — list prompts, get them with arguments
- **Notifications** — see server notifications in real time

## Debugging workflow with Inspector

1. **Verify initialization:** The Inspector shows the `initialize` handshake result. Check that `capabilities` includes the expected primitives (tools, resources, prompts).
2. **List tools:** Click the Tools tab. If tools don't appear, the server's `tools/list` handler has a problem.
3. **Call a tool:** Select a tool, fill in arguments, click Call. Check the response for `isError` flag and content.
4. **Check resources:** Switch to Resources tab. Verify URIs and content.
5. **Monitor notifications:** Watch for `notifications/tools/list_changed` or `notifications/resources/updated`.

# Systematic debugging procedure

## Level 1 — Server doesn't start

| Symptom | Check |
|---------|-------|
| "command not found" | Is `node`/`python`/`uv` in PATH? Use absolute path to binary. |
| Immediate exit | Does the server have compilation errors? Run `npm run build` or `python -m py_compile`. |
| Hangs without output | Server may be waiting for input. Verify it's running stdio transport correctly. |
| Port already in use | For HTTP transport, check if another process is on the same port. |

## Level 2 — Connection fails

| Symptom | Check |
|---------|-------|
| Initialize timeout | Server must respond to `initialize` request. Check JSON-RPC message format. |
| Protocol version mismatch | Server and client must agree on protocol version. Check `protocolVersion` field. |
| No capabilities | `initialize` response must include `capabilities` object with declared features. |

## Level 3 — Tools don't appear

| Symptom | Check |
|---------|-------|
| Empty tool list | `tools` capability not declared in `initialize` response. |
| Tools appear in Inspector but not host | Host may need restart. Check host-specific config format. |
| Partial tools | Tool registration may have thrown an error silently. Add error logging. |

## Level 4 — Tool calls fail

| Symptom | Check |
|---------|-------|
| Invalid params error | `inputSchema` doesn't match the arguments sent. Validate schema with Inspector. |
| Tool execution error | Check `isError: true` in response. Read the error message for guidance. |
| Timeout | Tool handler is taking too long. Check async operations and timeouts. |
| Empty response | Handler returned null/undefined. Must return `{ content: [...] }`. |

## Level 5 — Transport-specific issues

**stdio:**
- `console.log()` or `print()` to stdout corrupts the JSON-RPC stream
- Non-UTF-8 output breaks the protocol
- Embedded newlines in messages break framing

**Streamable HTTP:**
- Missing `Origin` header validation enables DNS rebinding attacks
- SSE stream dropped → client should reconnect with `Last-Event-ID`
- Session expired (404) → client must re-initialize

# Server-side logging

**For stdio servers**, always log to stderr:
```typescript
console.error("Debug: tool called with", args); // Goes to stderr
// NEVER: console.log("...") — this corrupts stdout
```

```python
import sys
print("Debug: tool called", file=sys.stderr)  # Goes to stderr
# NEVER: print("...") — this corrupts stdout
```

**For HTTP servers**, use structured logging:
```typescript
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
// Server supports logging capability — send logs to client
server.sendLoggingMessage({ level: "info", data: "Processing request" });
```

# Decision rules

- Always verify with MCP Inspector BEFORE connecting to a host — it eliminates host-specific variables
- If Inspector works but host doesn't, the issue is host config, not server code
- If neither works, check Level 1 (server starts) → Level 2 (connection) → Level 3 (capabilities) → Level 4 (tool execution)
- Enable `logging` capability for production servers to support remote debugging

# Output requirements

1. Identified failure level (1-5)
2. Specific root cause
3. Fix applied and verified via MCP Inspector
4. Host re-tested after fix

# Related skills

- `mcp-host-integration` — host-specific config patterns
- `mcp-auth-transports` — transport-level debugging
- `mcp-testing-evals` — automated testing beyond manual debugging
- `mcp-schema-contracts` — schema validation issues

# Failure handling

- If MCP Inspector itself fails to install, ensure Node.js 18+ is available and npm registry is reachable
- If Inspector connects but shows no UI, check port 6274 is not blocked
- If the issue is intermittent, add `logging` capability and use `server.sendLoggingMessage()` to trace execution
