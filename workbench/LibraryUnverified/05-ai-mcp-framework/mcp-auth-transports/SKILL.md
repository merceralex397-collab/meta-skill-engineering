---
name: mcp-auth-transports
description: "Select and implement MCP transports (stdio, Streamable HTTP) and authentication (OAuth 2.1 for remote servers). Use when deciding between local vs remote MCP deployment, implementing OAuth flows for remote MCP servers, debugging transport-level connection issues, or adding auth to an existing MCP server."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-auth-transports
  maturity: stable
  risk: medium
  tags: [mcp, auth, transports, oauth, stdio, http, sse]
---

# Purpose

Guide transport selection and authentication implementation for MCP servers. MCP defines two transports (stdio, Streamable HTTP) and an OAuth 2.1-based auth flow for remote servers. This skill covers when to use each, how to implement them, and how to debug transport-level failures.

# When to use this skill

- Deciding between stdio and Streamable HTTP transport for a new MCP server
- Adding OAuth 2.1 authentication to a remote MCP server
- Debugging transport-level connection failures (handshake, session, SSE stream drops)
- Migrating from the deprecated HTTP+SSE transport to Streamable HTTP

# Do not use this skill when

- Building tool logic or resource schemas (not transport-level) → use `mcp-tool-design` or `mcp-resources-prompts`
- Setting up a first MCP server from scratch → use `get-started`
- Implementing application-level permissions/gating → use `mcp-security-permissions`

# Transport selection

## Decision table

| Factor | stdio | Streamable HTTP |
|--------|-------|-----------------|
| Deployment | Local process, launched by host | Remote server, shared across clients |
| Clients served | One (1:1 with host process) | Many (1:N) |
| Network overhead | None (IPC) | HTTP + optional SSE |
| Auth needed | No (inherit OS process permissions) | Yes (OAuth 2.1 recommended) |
| Session management | Implicit (process lifetime) | Explicit (`Mcp-Session-Id` header) |
| Best for | CLI tools, desktop apps, dev environments | SaaS integrations, shared services, multi-tenant |

**Default rule:** Use stdio unless the server must be remote or serve multiple clients.

## stdio transport

The client launches the server as a subprocess. Messages are newline-delimited JSON-RPC on stdin/stdout.

```
Client → stdin  → Server (JSON-RPC request)
Client ← stdout ← Server (JSON-RPC response)
         stderr → Server logging (not protocol)
```

**Critical rules:**
- Server MUST NOT write anything to stdout that is not a valid JSON-RPC message
- Server MUST NOT use `print()` (Python) or `console.log()` (Node) to stdout — use stderr
- Messages are delimited by newlines and MUST NOT contain embedded newlines

## Streamable HTTP transport

The server exposes a single HTTP endpoint (e.g., `https://example.com/mcp`) supporting POST and GET.

**Client → Server (POST):** Each JSON-RPC message is a separate POST. Server responds with either `application/json` or `text/event-stream` (SSE).

**Server → Client (GET + SSE):** Client opens SSE stream for server-initiated messages (notifications, requests).

**Session management:**
1. Server MAY return `Mcp-Session-Id` header in the `initialize` response
2. Client MUST include `Mcp-Session-Id` on all subsequent requests
3. Server returns `404` when session expires; client must re-initialize
4. Client SHOULD send HTTP DELETE to terminate session cleanly

**Security requirements for Streamable HTTP:**
- MUST validate `Origin` header to prevent DNS rebinding
- Local servers SHOULD bind to `127.0.0.1`, not `0.0.0.0`
- MUST implement authentication for production deployments

# OAuth 2.1 authentication flow

MCP specifies OAuth 2.1 for remote server auth. The flow uses three standards:

1. **RFC 9728** — OAuth 2.0 Protected Resource Metadata (server advertises its auth server)
2. **RFC 8414** — OAuth 2.0 Authorization Server Metadata (client discovers endpoints)
3. **RFC 7591** — Dynamic Client Registration (client auto-registers without manual setup)

## Flow sequence

```
1. Client → MCP Server: request without token
2. MCP Server → Client: 401 + WWW-Authenticate header (resource_metadata URL)
3. Client → MCP Server: GET /.well-known/oauth-protected-resource
4. MCP Server → Client: resource metadata with authorization_servers URL
5. Client → Auth Server: GET /.well-known/oauth-authorization-server
6. Auth Server → Client: server metadata (token endpoint, auth endpoint, etc.)
7. Client → Auth Server: Dynamic Client Registration (RFC 7591) if needed
8. Client ↔ Auth Server: OAuth 2.1 authorization code flow (with PKCE)
9. Auth Server → Client: access token
10. Client → MCP Server: MCP request + Bearer token
```

## Implementation guidance

**TypeScript server with Streamable HTTP:**
```typescript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/streamableHttp.js";
import express from "express";

const app = express();
const server = new McpServer({ name: "my-remote-server", version: "1.0.0" });

// Register tools on server...

app.post("/mcp", async (req, res) => {
  const transport = new StreamableHTTPServerTransport("/mcp");
  await server.connect(transport);
  await transport.handleRequest(req, res);
});

app.listen(3000);
```

**For stdio transport (no auth needed):**
Credentials come from the environment (env vars, OS keychain, file-based tokens). Do NOT implement OAuth for stdio servers.

# Decision rules

- stdio servers MUST NOT implement OAuth — credentials come from the host process environment
- Remote Streamable HTTP servers SHOULD implement OAuth 2.1 with PKCE
- MUST support Dynamic Client Registration (RFC 7591) unless the server has a known, fixed set of clients
- The deprecated HTTP+SSE transport (protocol version 2024-11-05) should be migrated to Streamable HTTP
- Always include `MCP-Protocol-Version` header on HTTP requests after initialization

# Output requirements

1. Transport choice with rationale
2. Auth implementation (OAuth flow or environment-based)
3. Session management strategy (for Streamable HTTP)
4. Security hardening checklist (Origin validation, binding, TLS)

# Related skills

- `mcp-security-permissions` — application-level permissions and tool gating
- `mcp-inspector-debugging` — debugging transport-level issues with MCP Inspector
- `mcp-multi-tenant-design` — session isolation for multi-client Streamable HTTP servers

# Failure handling

- If client gets 401 and cannot parse `WWW-Authenticate`, check the server implements RFC 9728 Protected Resource Metadata
- If SSE stream drops, client should reconnect with `Last-Event-ID` header for resumability
- If session returns 404, the session expired — client must send new `initialize` request without session ID
- If `Mcp-Session-Id` is missing from server response, the server is stateless — do not send session headers
