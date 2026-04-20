---
name: go-api-service
description: "Guides Go HTTP API development: net/http handler patterns, middleware chaining (stdlib and chi/gorilla), context.Context propagation, structured error handling, graceful shutdown with signal trapping, dependency injection via struct receivers, database/sql connection pooling, JSON encoding/decoding, and httptest-based testing."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: go-api-service
  maturity: draft
  risk: low
  tags: [go, api, service, http]
---

# Purpose

Guides the design and implementation of Go HTTP API services—from handler structure and middleware chaining through context propagation, structured error handling, graceful shutdown, dependency injection, and production deployment. Ensures Go-idiomatic patterns are used: explicit error returns (not panic), `http.Handler` interface compliance, and proper `context.Context` threading.

# When to use this skill

Use this skill when:

- building or modifying a Go HTTP API service (REST, gRPC-gateway, or JSON API)
- choosing between standard library `net/http`, chi, gorilla/mux, or other routers
- implementing middleware (auth, logging, rate limiting, recovery)
- propagating `context.Context` through handler → service → repository layers
- configuring `database/sql` connection pools or similar resources
- writing handler tests with `httptest.NewRequest` and `httptest.NewRecorder`
- implementing graceful shutdown for containerized deployments
- structuring a Go module for a service (cmd/, internal/, pkg/ layout)

# Do not use this skill when

- the project is a CLI tool or library with no HTTP server component
- the task is about gRPC without an HTTP gateway—adapt or use a gRPC-specific skill
- the task is in a non-Go language—prefer `flask`, `express-node`, `fastapi`, or `python`
- the task is purely database schema design—prefer `postgresql` or `data-model`

# Operating procedure

1. **Define the service struct.** Create a struct that holds all dependencies (logger, database, config). Attach handler methods to this struct so dependencies are available without globals.
2. **Choose a router.** For simple services, use Go 1.22+ stdlib `http.ServeMux` with method-based routing. For path parameters and middleware groups, use `chi` or `gorilla/mux`. Document the choice.
3. **Wire middleware.** Implement middleware as `func(http.Handler) http.Handler`. Chain in order: recovery → logging → auth → rate-limit → application handlers. Use `chi.Chain` or manual wrapping.
4. **Propagate context.** Accept `context.Context` as the first parameter in all service and repository methods. Extract values from `r.Context()` in handlers. Use `context.WithTimeout` for downstream calls.
5. **Handle errors explicitly.** Return `error` from all non-handler functions. In handlers, translate errors to HTTP status codes with a helper. Never use `panic` for operational errors.
6. **Implement JSON helpers.** Create `respondJSON(w, status, data)` and `decodeJSON(r, dst)` helpers to standardize encoding/decoding with proper `Content-Type` headers and error handling.
7. **Configure the database.** Set `db.SetMaxOpenConns()`, `db.SetMaxIdleConns()`, and `db.SetConnMaxLifetime()` on the `*sql.DB` pool. Use `sqlx` or `pgx` for convenience but understand the underlying pool.
8. **Implement graceful shutdown.** Listen for `SIGTERM`/`SIGINT` with `signal.NotifyContext`. Call `server.Shutdown(ctx)` with a timeout to drain in-flight requests.
9. **Write tests.** Use `httptest.NewRequest` + `httptest.NewRecorder` for unit tests. Use `httptest.NewServer` for integration tests. Run `go test -race ./...` to catch race conditions.
10. **Verify production readiness.** Run `golangci-lint run`, `go vet`, and `go test -race`. Confirm health/ready endpoints exist. Verify the binary builds with `CGO_ENABLED=0` for containers.

# Decision rules

- Always return `error` from functions—never use `panic` for control flow or expected failures.
- Use `context.Context` as the first parameter of every function that does I/O, calls downstream services, or may need cancellation.
- Wrap errors with `fmt.Errorf("operation: %w", err)` to preserve the error chain for `errors.Is` and `errors.As`.
- Use struct receivers for handlers to avoid global state. The service struct holds DB, logger, and config.
- Prefer `http.Handler` and `http.HandlerFunc` interfaces over framework-specific types for portability.
- Set explicit timeouts on `http.Server` (`ReadTimeout`, `WriteTimeout`, `IdleTimeout`).
- Use `encoding/json.Decoder` with `DisallowUnknownFields()` for strict input parsing.
- Run `go test -race ./...` in CI—race conditions are common in concurrent Go code.
- Use `golangci-lint` with a project `.golangci.yml` config for consistent linting.
- Table-driven tests are the standard pattern for Go test organization.

# Output requirements

1. `Service Structure` — module layout, service struct, dependency wiring
2. `Handler Implementation` — route registration, request parsing, response format
3. `Error Handling` — error types, status mapping, error response shape
4. `Middleware` — chain order, recovery, logging, auth
5. `Verification` — `go test` commands, `golangci-lint`, build check

# References

Read these only when relevant:

- `references/implementation-patterns.md` — handler, middleware, shutdown, and service struct patterns
- `references/validation-checklist.md` — pre-deploy and pre-merge verification items
- `references/failure-modes.md` — common Go API runtime errors and their fixes

# Related skills

- `api-contracts` — OpenAPI specification and contract testing
- `postgresql` — database integration and SQL patterns
- `observability-logging` — structured logging with slog or zerolog
- `rate-limits-retries` — rate limiting and retry strategies
- `data-model` — data modeling and schema design

# Anti-patterns

- **Global `db` variable.** Declaring a package-level `var db *sql.DB` instead of passing it through the service struct. Makes testing and connection management difficult.
- **Panic-driven error handling.** Using `panic` and `recover` as a control flow mechanism instead of returning errors. Reserve `recover` for unexpected panics in middleware only.
- **Ignoring `context.Context`.** Passing `context.Background()` everywhere instead of threading `r.Context()` from the handler. This breaks cancellation and timeout propagation.
- **Unbounded goroutines.** Spawning goroutines in handlers without tracking or limiting them, leading to goroutine leaks under load.
- **Writing after handler returns.** Passing `http.ResponseWriter` to a goroutine that writes after the handler function returns, causing data races.
- **Empty error checks.** Writing `if err != nil { return }` without wrapping or logging the error, losing all diagnostic context.
- **`init()` for dependency setup.** Using `func init()` to create database connections or configure services, making testing and configuration switching impossible.
- **JSON marshal without Content-Type.** Calling `json.NewEncoder(w).Encode(data)` without setting `Content-Type: application/json` header first.

# Failure handling

- If a handler panics and the client sees a connection reset, add recovery middleware that catches panics, logs the stack, and returns 500.
- If context cancellation is not propagating, verify each layer accepts and passes `ctx` and that downstream calls use `ctx` (e.g., `db.QueryContext(ctx, ...)`).
- If the race detector reports failures, identify shared state accessed without synchronization and protect with mutex or redesign with channels.
- If connection pool exhaustion occurs, check `SetMaxOpenConns` settings and ensure connections are returned (rows closed, transactions committed/rolled back).
- If JSON decoding silently ignores fields, enable `DisallowUnknownFields()` on the decoder.
- If graceful shutdown kills in-flight requests, increase the shutdown timeout context or check that `Shutdown` (not `Close`) is used.
