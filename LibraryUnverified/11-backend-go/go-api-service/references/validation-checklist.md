# Validation Checklist — Go API Service

A Go API change is not ready for merge until every applicable item is verified.

## Code Quality

- [ ] `go vet ./...` passes with no warnings
- [ ] `golangci-lint run` passes (or project-specific linter config)
- [ ] `go build ./...` succeeds with no errors
- [ ] No `//nolint` directives without an accompanying explanation comment
- [ ] No exported functions/types without doc comments

## Error Handling

- [ ] All errors are checked (`if err != nil`)—no discarded errors
- [ ] Errors are wrapped with context: `fmt.Errorf("operation: %w", err)`
- [ ] `errors.Is` and `errors.As` work correctly through the error chain
- [ ] No `panic()` used for operational/expected errors
- [ ] Handler functions translate errors to appropriate HTTP status codes
- [ ] Sentinel errors are defined for domain-specific error cases

## Context Propagation

- [ ] `context.Context` is the first parameter of all I/O functions
- [ ] `r.Context()` is passed from handlers to service layer
- [ ] Database calls use `QueryContext`, `ExecContext`, `PingContext` (not non-context variants)
- [ ] HTTP client calls use `req.WithContext(ctx)` or `http.NewRequestWithContext`
- [ ] Timeouts are set on contexts for downstream calls
- [ ] `context.Background()` is only used at the top level (main, tests)

## Concurrency

- [ ] `go test -race ./...` passes with no data race detections
- [ ] Goroutines spawned in handlers have proper lifecycle management
- [ ] `sync.WaitGroup` or `errgroup.Group` is used to wait for goroutine completion
- [ ] No goroutine leaks (goroutines have exit conditions and cancellation)
- [ ] Shared state is protected by mutex or accessed through channels
- [ ] `defer mu.Unlock()` immediately follows `mu.Lock()` when using mutexes

## Graceful Shutdown

- [ ] `signal.NotifyContext` or `signal.Notify` handles SIGTERM and SIGINT
- [ ] `server.Shutdown(ctx)` is used (not `server.Close()`)
- [ ] Shutdown timeout is configured (e.g., 30 seconds)
- [ ] In-flight requests are allowed to complete during shutdown
- [ ] Database connections are closed after server shutdown
- [ ] Background goroutines are cancelled and awaited

## Database

- [ ] Connection pool is configured: `SetMaxOpenConns`, `SetMaxIdleConns`, `SetConnMaxLifetime`
- [ ] `ConnMaxLifetime` is less than database's connection timeout
- [ ] `rows.Close()` is deferred immediately after `Query` (before error check on rows)
- [ ] Transactions are committed or rolled back in all code paths (use `defer tx.Rollback()`)
- [ ] Prepared statements are used for repeated queries (or rely on pgx auto-prepare)
- [ ] SQL injection is prevented—never use `fmt.Sprintf` for query construction

## HTTP Server

- [ ] `ReadTimeout`, `WriteTimeout`, and `IdleTimeout` are set on `http.Server`
- [ ] `Content-Type: application/json` header is set on all JSON responses
- [ ] Request body is decoded with `DisallowUnknownFields()` for strict parsing
- [ ] Response status code is set before writing the response body
- [ ] Recovery middleware catches panics and returns 500

## Testing

- [ ] Tests run with `go test -race -count=1 ./...`
- [ ] Handlers are tested with `httptest.NewRequest` and `httptest.NewRecorder`
- [ ] Table-driven tests are used for parameterized cases
- [ ] Test helpers use `t.Helper()` for clean stack traces
- [ ] Integration tests use `httptest.NewServer` or test containers
- [ ] Test coverage is checked: `go test -coverprofile=coverage.out ./...`

## Deployment

- [ ] Binary builds with `CGO_ENABLED=0` for static linking (container-friendly)
- [ ] Docker image uses multi-stage build (build stage → scratch/distroless)
- [ ] Health endpoint (`/health`) returns 200
- [ ] Ready endpoint (`/ready`) checks downstream dependencies
- [ ] Configuration is loaded from environment variables (not hardcoded)
- [ ] Structured logging is used (`slog`, `zerolog`, or `zap`)

## Pre-merge Smoke Test

```bash
go vet ./...
golangci-lint run
go test -race -count=1 -coverprofile=coverage.out ./...
go tool cover -func=coverage.out | tail -1  # check total coverage
go build -o /dev/null ./cmd/api/              # verify clean build
```
