---
name: mcp-security-permissions
description: "Secure MCP servers with least privilege, tool gating, input validation, and fail-closed behavior. Use when hardening an MCP server for production, implementing per-tool permission controls, validating tool inputs against injection attacks, or auditing an MCP server's security posture."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-security-permissions
  maturity: stable
  risk: high
  tags: [mcp, security, permissions, validation, least-privilege]
---

# Purpose

Harden MCP servers for production use. Covers the MCP-specific security model: tool annotations for trust signaling, input validation, path restrictions, prompt injection defense, least-privilege principles, and the security requirements in the MCP specification.

# When to use this skill

- Hardening an MCP server for production deployment
- Implementing per-tool or per-tenant permission controls
- Adding input validation and injection prevention to tool handlers
- Auditing an MCP server's security posture
- Understanding MCP's trust model between hosts, clients, and servers

# Do not use this skill when

- Implementing OAuth auth flows → use `mcp-auth-transports`
- Designing multi-tenant isolation → use `mcp-multi-tenant-design`
- Building a dev/prototype server with no production security needs

# MCP security model

## Trust boundaries

```
User ←→ Host (AI app) ←→ MCP Client ←→ MCP Server ←→ External Services
         ↑                    ↑              ↑
    Trust boundary 1    Trust boundary 2  Trust boundary 3
```

- **Hosts SHOULD** show users which tools are available and get confirmation before tool invocations
- **Clients MUST** consider tool annotations as untrusted unless from trusted servers
- **Servers MUST** validate all inputs — they cannot trust that the LLM or client sent valid data

## Tool annotations for trust signaling

Tool annotations help hosts make safe decisions about tool invocation:

```json
{
  "name": "delete_file",
  "annotations": {
    "readOnlyHint": false,
    "destructiveHint": true,
    "idempotentHint": false,
    "openWorldHint": false
  }
}
```

| Annotation | Purpose | Example |
|-----------|---------|---------|
| `readOnlyHint: true` | Tool doesn't modify state | Search, list, read operations |
| `destructiveHint: true` | Tool makes irreversible changes | Delete, drop, overwrite operations |
| `idempotentHint: true` | Safe to retry | Update with same input = same result |
| `openWorldHint: true` | Interacts with external world | Sends emails, makes API calls |

**Important:** These are *hints*, not enforced constraints. The host decides whether to require user confirmation based on these hints.

# Operating procedure

## Phase 1 — Input validation

Every tool handler must validate inputs before processing:

```typescript
server.tool("read_file", { path: z.string() }, async ({ path }) => {
  // 1. Validate path is within allowed directory
  const resolved = path.resolve(path);
  if (!resolved.startsWith(ALLOWED_BASE_DIR)) {
    throw new McpError(ErrorCode.InvalidParams, "Path outside allowed directory");
  }

  // 2. Prevent path traversal
  if (path.includes("..")) {
    throw new McpError(ErrorCode.InvalidParams, "Path traversal not allowed");
  }

  // 3. Check file exists and is readable
  // ... proceed with operation
});
```

**Common input validation checks:**
- Path traversal (`../`) in file paths
- SQL injection in database query parameters
- Command injection in shell-executing tools
- URI validation for resource URIs
- Size limits on string inputs
- Type coercion (ensure numbers are actually numbers)

## Phase 2 — Least privilege

- Request only the API scopes your tools actually need
- If a tool only reads data, use read-only API credentials
- Separate read and write operations into different tools so read-only deployments can exclude write tools
- Use tool annotations to signal intent: `readOnlyHint: true` for read tools

## Phase 3 — Prompt injection defense

LLMs can be manipulated to call tools with malicious arguments. Defend at the server level:

- **Never trust tool arguments as safe** — validate and sanitize everything
- **Parameterize queries** — never concatenate user input into SQL/shell/URL strings
- **Restrict tool scope** — a "search" tool should not also write files
- **Log all tool calls** — enable audit trail via the MCP `logging` capability

```python
@mcp.tool()
async def search_database(query: str, table: str) -> str:
    """Search a database table."""
    # ✅ Parameterized query
    if table not in ALLOWED_TABLES:
        raise McpError("Table not in allowlist")
    results = await db.execute("SELECT * FROM ? WHERE content LIKE ?", [table, f"%{query}%"])

    # ❌ NEVER do this — SQL injection
    # results = await db.execute(f"SELECT * FROM {table} WHERE content LIKE '%{query}%'")
```

## Phase 4 — Transport-level security

**stdio servers:**
- Inherit OS process permissions — run as restricted user
- Do not expose stdio servers over network
- Credentials come from environment, not from the protocol

**Streamable HTTP servers:**
- MUST validate `Origin` header to prevent DNS rebinding
- MUST bind to `127.0.0.1` (not `0.0.0.0`) for local servers
- MUST use HTTPS for production remote servers
- MUST implement OAuth 2.1 for remote authentication
- SHOULD implement rate limiting per session

## Phase 5 — Audit and logging

Enable the `logging` capability and log security-relevant events:

```typescript
server.sendLoggingMessage({
  level: "warning",
  data: `Tool 'delete_file' called with path: ${path} by session: ${sessionId}`
});
```

# Decision rules

- All tool inputs are untrusted. Validate every parameter, even from "trusted" hosts.
- Tool annotations are hints, not enforcement. Hosts decide how to use them.
- Fail closed: if validation is uncertain, reject the request rather than proceeding.
- Separate read and write tools so deployments can restrict to read-only.
- Never concatenate user input into queries/commands — always parameterize.
- Log all destructive operations for audit trail.

# Output requirements

1. Input validation on all tool handlers
2. Tool annotations set correctly (readOnlyHint, destructiveHint, etc.)
3. Least-privilege API credentials
4. Transport security hardening (Origin validation, HTTPS, binding)
5. Audit logging for destructive operations

# Related skills

- `mcp-auth-transports` — OAuth 2.1 and transport security
- `mcp-multi-tenant-design` — per-tenant security isolation
- `mcp-tool-design` — designing tools with security in mind
- `mcp-inspector-debugging` — testing security controls

# Failure handling

- If a tool receives invalid input, return a clear error message without revealing internal details
- If authentication fails, return 401 with `WWW-Authenticate` header (HTTP) or error message (stdio)
- If a security audit reveals vulnerabilities, prioritize: injection prevention > path traversal > privilege escalation > information disclosure
