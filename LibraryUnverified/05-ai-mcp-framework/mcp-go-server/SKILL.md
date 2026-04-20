---
name: mcp-go-server
description: "Build MCP servers in Go using the mark/mcp-go SDK. Use when implementing an MCP server in Go, applying Go idioms (interfaces, context, error handling) to MCP tool/resource registration, or choosing between Go and other MCP SDK languages."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-go-server
  maturity: stable
  risk: low
  tags: [mcp, go, server, sdk]
---

# Purpose

Build MCP servers in Go using the community Go SDK (`github.com/mark3labs/mcp-go`). Covers project setup, tool/resource registration, transport wiring, and Go-idiomatic patterns for MCP servers.

# When to use this skill

- Building an MCP server in Go
- Porting an existing Go service to expose MCP tools
- Choosing Go for performance-critical or infrastructure-adjacent MCP servers
- Applying Go patterns (context propagation, interfaces, error values) to MCP

# Do not use this skill when

- Building in TypeScript → use `mcp-typescript-sdk`
- Building in Python → use `mcp-python-fastmcp`
- Deciding which language to use → start with `mcp-development`, then pick the language skill

# Operating procedure

## Phase 1 — Project setup

```bash
mkdir my-mcp-server && cd my-mcp-server
go mod init github.com/yourorg/my-mcp-server
go get github.com/mark3labs/mcp-go
```

Note: The official MCP SDK list is at https://modelcontextprotocol.io/docs/sdk. The Go SDK (`mcp-go` by mark3labs) is the most mature community Go implementation. There is no official first-party Go SDK from the MCP project at this time.

## Phase 2 — Implement the server

```go
package main

import (
    "context"
    "fmt"

    "github.com/mark3labs/mcp-go/mcp"
    "github.com/mark3labs/mcp-go/server"
)

func main() {
    s := server.NewMCPServer(
        "my-server",
        "1.0.0",
        server.WithToolCapabilities(true),
    )

    greetTool := mcp.NewTool("greet",
        mcp.WithDescription("Greet someone by name"),
        mcp.WithString("name", mcp.Required(), mcp.Description("Name to greet")),
    )

    s.AddTool(greetTool, func(ctx context.Context, req mcp.CallToolRequest) (*mcp.CallToolResult, error) {
        name := req.Params.Arguments["name"].(string)
        return mcp.NewToolResultText(fmt.Sprintf("Hello, %s!", name)), nil
    })

    // stdio transport
    if err := server.ServeStdio(s); err != nil {
        panic(err)
    }
}
```

## Phase 3 — Go-idiomatic patterns

**Context propagation:** Pass `context.Context` through all tool handlers. Use it for timeouts, cancellation, and request-scoped values.

**Error handling:** Return `error` from tool handlers for protocol errors. For business logic errors, return a result with `isError: true`:
```go
return mcp.NewToolResultError("API key expired — regenerate at https://..."), nil
```

**Interfaces:** Define service interfaces for your API clients and inject them into tool handlers for testability.

**Structured logging:** Use `slog` or `zerolog` writing to stderr (not stdout, which is the stdio transport).

## Phase 4 — Transport options

**stdio (default):**
```go
server.ServeStdio(s)
```

**Streamable HTTP:**
```go
transport := server.NewStreamableHTTPServer(s,
    server.WithEndpoint("/mcp"),
)
log.Fatal(http.ListenAndServe(":8080", transport))
```

## Phase 5 — Test

```bash
go build -o my-server .
npx @modelcontextprotocol/inspector ./my-server
```

Verify `tools/list` returns your tools and `tools/call` works correctly.

# Decision rules

- Use Go for MCP servers when: the service is infrastructure-adjacent, performance matters, the team is Go-native, or deploying as a single static binary is valuable
- Use `context.Context` for all tool handlers — it enables timeouts and cancellation
- Return structured Go errors for protocol failures; use `mcp.NewToolResultError()` for business logic errors
- Write logs to stderr only — stdout is reserved for JSON-RPC messages on stdio transport

# Output requirements

1. Go module with `go.mod` and compiled binary
2. Registered tools with input schemas (using `mcp.WithString`, `mcp.WithNumber`, etc.)
3. Passes MCP Inspector verification
4. Transport wiring (stdio or Streamable HTTP)

# Related skills

- `mcp-typescript-sdk` — TypeScript alternative
- `mcp-python-fastmcp` — Python alternative
- `mcp-auth-transports` — transport and auth details
- `mcp-tool-design` — tool schema design patterns

# Failure handling

- If `mcp-go` API has changed, check the README at `github.com/mark3labs/mcp-go` for current patterns
- If tools don't appear in Inspector, verify `server.WithToolCapabilities(true)` is set
- If the binary crashes on startup, check for stdout writes in initialization code (conflicts with stdio transport)
