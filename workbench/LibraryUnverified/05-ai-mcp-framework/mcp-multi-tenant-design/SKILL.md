---
name: mcp-multi-tenant-design
description: "Design MCP servers that serve multiple tenants or users with proper isolation, session management, and access control. Use when building shared MCP services, implementing per-user session state on Streamable HTTP, designing tenant-scoped tool access, or rate-limiting per-tenant usage."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-multi-tenant-design
  maturity: stable
  risk: medium
  tags: [mcp, multi-tenant, isolation, sessions, scaling]
---

# Purpose

Design MCP servers that safely handle multiple tenants or users simultaneously. Covers session isolation, per-tenant auth, state partitioning, rate limiting, and the specific MCP session management mechanisms that enable multi-tenant operation.

# When to use this skill

- Building a shared/hosted MCP server serving multiple organizations or users
- Implementing session isolation for Streamable HTTP MCP servers
- Designing per-tenant tool access or data scoping
- Adding rate limiting or usage quotas to an MCP server
- Scaling an MCP server from single-user to multi-user

# Do not use this skill when

- Building a single-user local MCP server via stdio → use `mcp-development`
- Working on auth flow implementation → use `mcp-auth-transports`
- The server is project-local with one user

# Architecture context

**stdio servers are inherently single-tenant** — one client process, one server process, direct IPC. Multi-tenancy only applies to Streamable HTTP servers.

**Streamable HTTP servers can serve many clients.** Each client gets its own session via `Mcp-Session-Id`. The server must isolate state, auth, and resources between sessions.

# Operating procedure

## Phase 1 — Session isolation

MCP's session management for Streamable HTTP:

1. **Session creation:** Server generates a session ID during `initialize` and returns it in the `Mcp-Session-Id` response header.
2. **Session binding:** Client includes `Mcp-Session-Id` on all subsequent requests.
3. **Session termination:** Client sends HTTP DELETE; server cleans up session state.
4. **Session expiry:** Server returns 404 for expired sessions; client must re-initialize.

**Implementation pattern:**
```typescript
const sessions = new Map<string, SessionState>();

function handleInitialize(req, res) {
  const sessionId = crypto.randomUUID();
  sessions.set(sessionId, {
    tenantId: extractTenantFromToken(req),
    credentials: extractCredentials(req),
    createdAt: Date.now(),
  });
  res.setHeader("Mcp-Session-Id", sessionId);
  // ... return initialize response
}

function handleToolCall(req, res) {
  const sessionId = req.headers["mcp-session-id"];
  const session = sessions.get(sessionId);
  if (!session) {
    return res.status(404).json({ error: "Session expired" });
  }
  // Use session.tenantId to scope all data access
}
```

## Phase 2 — Tenant-scoped data access

Every tool handler must scope data access to the current tenant:

- **Database queries:** Always include `WHERE tenant_id = ?` or equivalent
- **API calls:** Use the tenant's credentials, not shared credentials
- **File access:** Restrict to tenant-specific directories
- **Resource URIs:** Prefix with tenant context (e.g., `tenant://acme/resource`)

**Anti-pattern:** Shared mutable state across sessions. Each session's tool calls must be independent.

## Phase 3 — Per-tenant tool surface

Not all tenants may see the same tools. Use dynamic `tools/list`:

```typescript
server.setRequestHandler("tools/list", async (request, context) => {
  const session = getSession(context);
  const permissions = await getPermissions(session.tenantId);
  const allTools = getRegisteredTools();
  return {
    tools: allTools.filter(t => permissions.includes(t.name))
  };
});
```

When a tenant's tool access changes, send `notifications/tools/list_changed` to their active sessions.

## Phase 4 — Rate limiting and quotas

Per-tenant rate limiting prevents one tenant from starving others:

- Track `tools/call` count per session/tenant per time window
- Return MCP error with helpful message when limit exceeded:
  ```json
  { "isError": true, "content": [{ "type": "text", "text": "Rate limit exceeded: 100 calls/minute. Retry after 32 seconds." }] }
  ```
- Consider separate limits for read-only vs mutating tools

## Phase 5 — Scaling

- **Horizontal scaling:** Use external session store (Redis, database) instead of in-memory Map
- **Sticky sessions:** If using in-memory state, route by `Mcp-Session-Id` hash
- **Connection limits:** Cap concurrent SSE connections per tenant
- **Graceful shutdown:** Drain active sessions before stopping server

# Decision rules

- Session ID must be cryptographically random (UUID v4 or crypto.randomUUID())
- Never trust client-provided tenant identity — derive it from the OAuth token
- Tool handlers must not share mutable state across sessions
- Use `Mcp-Session-Id` for session affinity, not cookies or IP-based routing
- Set session TTL and clean up expired sessions proactively

# Output requirements

1. Session management implementation with per-tenant isolation
2. Tenant-scoped data access in all tool handlers
3. Rate limiting per tenant
4. Scalability plan (session store, connection limits)

# Related skills

- `mcp-auth-transports` — OAuth 2.1 flow that provides tenant identity
- `mcp-security-permissions` — permission models for tool access
- `mcp-development` — general server development lifecycle

# Failure handling

- If session store fails, return 503 to clients rather than serving unscoped requests
- If tenant identity cannot be extracted from token, reject the request at initialize
- If a session's OAuth token expires, return 401 with guidance to re-authenticate
