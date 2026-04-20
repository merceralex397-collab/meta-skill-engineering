# Failure Modes — Go API Service

Common Go HTTP service runtime errors, their root causes, and fixes.

## Goroutine Leaks

**Symptom:** Memory usage grows over time. `runtime.NumGoroutine()` increases monotonically. pprof goroutine profile shows many blocked goroutines.

**Cause:** Goroutines are spawned in handlers or background tasks but have no exit condition. Common culprits: goroutines waiting on a channel that is never closed, goroutines making HTTP calls without context timeout, or ticker/timer goroutines that are never stopped.

**Fix:**
- Always pass `context.Context` to goroutines and check `ctx.Done()`.
- Use `errgroup.Group` to manage goroutine lifecycles.
- Set timeouts on all outgoing HTTP and database calls.
- In tests, check goroutine count before and after with `runtime.NumGoroutine()` or use `goleak.VerifyNone(t)` from `go.uber.org/goleak`.
```go
g, ctx := errgroup.WithContext(r.Context())
g.Go(func() error {
    return fetchUser(ctx, userID)
})
if err := g.Wait(); err != nil {
    respondError(w, http.StatusInternalServerError, "fetch failed")
}
```

## Forgotten Context Cancellation

**Symptom:** Downstream services receive requests long after the client has disconnected. Database queries continue running after request timeout.

**Cause:** `context.Background()` is used instead of `r.Context()`, or a derived context's cancel function is never called (resource leak).

**Fix:**
```go
// BAD: ignores client cancellation
results, err := s.db.QueryContext(context.Background(), query)

// GOOD: respects client cancellation and server timeout
ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
defer cancel()
results, err := s.db.QueryContext(ctx, query)
```
Always `defer cancel()` immediately after creating a derived context.

## Response Written After Handler Returns

**Symptom:** `http: superfluous response.WriteHeader call` log messages. Data races detected by `-race`. Corrupted responses sent to clients.

**Cause:** A goroutine spawned in a handler writes to `http.ResponseWriter` after the handler function has returned. By that point, the server may have recycled the response writer.

**Fix:** Never pass `http.ResponseWriter` to a goroutine. Collect results in the goroutine via channels or `errgroup`, then write the response in the handler function:
```go
func (s *Server) handler(w http.ResponseWriter, r *http.Request) {
    ch := make(chan result, 1)
    go func() { ch <- doWork(r.Context()) }()

    select {
    case res := <-ch:
        respondJSON(w, http.StatusOK, res)
    case <-r.Context().Done():
        respondError(w, http.StatusGatewayTimeout, "request timed out")
    }
}
```

## Nil Pointer on Missing Context Value

**Symptom:** `panic: interface conversion: interface is nil, not string` when extracting a value from context.

**Cause:** `ctx.Value(key)` returns `nil` if the key was never set (e.g., middleware that sets the value was not in the chain). A direct type assertion `ctx.Value(key).(string)` panics on nil.

**Fix:** Always use the comma-ok pattern:
```go
userID, ok := ctx.Value(userIDKey).(string)
if !ok {
    respondError(w, http.StatusUnauthorized, "missing user context")
    return
}
```

## Connection Pool Exhaustion

**Symptom:** Requests hang waiting for a database connection. Logs show `context deadline exceeded` or `too many connections`.

**Cause:** `MaxOpenConns` is too low for the request volume, or connections are leaked (rows not closed, transactions not committed/rolled back).

**Fix:**
- Set pool sizes based on expected concurrency: `db.SetMaxOpenConns(25)`.
- Always close rows: `defer rows.Close()` immediately after `Query`.
- Always finalize transactions: `defer tx.Rollback()` (no-op if already committed).
- Monitor with `db.Stats()` to track open connections, wait count, and wait duration.
```go
rows, err := s.db.QueryContext(ctx, query)
if err != nil {
    return fmt.Errorf("query users: %w", err)
}
defer rows.Close()  // MUST be deferred before processing rows
```

## Panic in Handler Without Recovery Middleware

**Symptom:** Client sees a connection reset. Server log may show the panic stack trace, or nothing at all if stderr is not captured.

**Cause:** A nil pointer dereference, index out of range, or other panic in a handler crashes the goroutine serving that request.

**Fix:** Add recovery middleware as the outermost layer:
```go
func (s *Server) recoverPanic(next http.Handler) http.Handler {
    return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
        defer func() {
            if err := recover(); err != nil {
                s.logger.Error("panic", "error", err, "stack", string(debug.Stack()))
                w.Header().Set("Connection", "close")
                http.Error(w, "Internal Server Error", http.StatusInternalServerError)
            }
        }()
        next.ServeHTTP(w, r)
    })
}
```

## JSON Decode into Wrong Type

**Symptom:** Fields are zero-valued after decoding. No error returned from `json.Decode`.

**Cause:** The target struct has unexported fields, wrong field names, or missing `json` tags. JSON numbers decoded into `interface{}` become `float64`, not `int`.

**Fix:**
- Use `json` struct tags: `json:"field_name"`.
- Export all fields that should be decoded.
- Use `DisallowUnknownFields()` to catch unexpected input fields.
- For strict numeric types, decode into a concrete struct (not `map[string]interface{}`).
```go
type CreateUserRequest struct {
    Name  string `json:"name" validate:"required"`
    Email string `json:"email" validate:"required,email"`
    Age   int    `json:"age" validate:"gte=0"`
}
```

## Server Timeout Misconfiguration

**Symptom:** Slow clients cause goroutine accumulation. Server becomes unresponsive under slow-loris attacks.

**Cause:** `ReadTimeout` and `WriteTimeout` are not set on `http.Server`, defaulting to 0 (no timeout).

**Fix:**
```go
httpServer := &http.Server{
    Addr:         ":8080",
    Handler:      handler,
    ReadTimeout:  5 * time.Second,
    WriteTimeout: 10 * time.Second,
    IdleTimeout:  120 * time.Second,
}
```

## Import Cycle

**Symptom:** `import cycle not allowed` compilation error.

**Cause:** Package A imports package B which imports package A. Common when handler and service packages reference each other's types.

**Fix:** Define shared types (models, interfaces) in a separate package that both can import. Use interfaces at package boundaries so packages depend on abstractions, not concrete implementations.

Related skills: `api-contracts`, `postgresql`, `observability-logging`.
