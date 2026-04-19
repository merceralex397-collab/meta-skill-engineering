# Implementation Patterns — Go API Service

## Handler Structure with Service Struct

Attach handlers to a struct that holds all dependencies, avoiding package-level globals:

```go
// internal/server/server.go
type Server struct {
    db     *sql.DB
    logger *slog.Logger
    cfg    Config
}

func New(db *sql.DB, logger *slog.Logger, cfg Config) *Server {
    return &Server{db: db, logger: logger, cfg: cfg}
}

func (s *Server) Routes() http.Handler {
    mux := http.NewServeMux()

    // Go 1.22+ pattern-based routing
    mux.HandleFunc("GET /api/v1/users", s.listUsers)
    mux.HandleFunc("GET /api/v1/users/{id}", s.getUser)
    mux.HandleFunc("POST /api/v1/users", s.createUser)
    mux.HandleFunc("DELETE /api/v1/users/{id}", s.deleteUser)

    mux.HandleFunc("GET /health", s.healthCheck)
    mux.HandleFunc("GET /ready", s.readyCheck)

    // Wrap with middleware
    var handler http.Handler = mux
    handler = s.recoverPanic(handler)
    handler = s.logRequest(handler)
    handler = s.addRequestID(handler)

    return handler
}
```

## Middleware Pattern

Go middleware follows the `func(http.Handler) http.Handler` signature:

```go
// internal/middleware/logging.go
func (s *Server) logRequest(next http.Handler) http.Handler {
    return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
        start := time.Now()
        wrapped := &responseWriter{ResponseWriter: w, statusCode: http.StatusOK}

        next.ServeHTTP(wrapped, r)

        s.logger.Info("request completed",
            "method", r.Method,
            "path", r.URL.Path,
            "status", wrapped.statusCode,
            "duration", time.Since(start),
            "request_id", r.Context().Value(requestIDKey),
        )
    })
}

func (s *Server) recoverPanic(next http.Handler) http.Handler {
    return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
        defer func() {
            if err := recover(); err != nil {
                s.logger.Error("panic recovered",
                    "error", err,
                    "stack", string(debug.Stack()),
                )
                http.Error(w, "Internal Server Error", http.StatusInternalServerError)
            }
        }()
        next.ServeHTTP(wrapped, r)
    })
}

// responseWriter wraps http.ResponseWriter to capture the status code
type responseWriter struct {
    http.ResponseWriter
    statusCode int
}

func (rw *responseWriter) WriteHeader(code int) {
    rw.statusCode = code
    rw.ResponseWriter.WriteHeader(code)
}
```

## Context Value Propagation

Use typed keys for context values to avoid collisions:

```go
// internal/middleware/requestid.go
type contextKey string

const requestIDKey contextKey = "request_id"

func (s *Server) addRequestID(next http.Handler) http.Handler {
    return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
        id := r.Header.Get("X-Request-ID")
        if id == "" {
            id = uuid.NewString()
        }
        ctx := context.WithValue(r.Context(), requestIDKey, id)
        w.Header().Set("X-Request-ID", id)
        next.ServeHTTP(w, r.WithContext(ctx))
    })
}

func RequestIDFromContext(ctx context.Context) string {
    if id, ok := ctx.Value(requestIDKey).(string); ok {
        return id
    }
    return ""
}
```

## Graceful Shutdown Implementation

```go
// cmd/api/main.go
func main() {
    cfg := loadConfig()
    logger := slog.New(slog.NewJSONHandler(os.Stdout, nil))

    db, err := openDB(cfg)
    if err != nil {
        logger.Error("failed to open database", "error", err)
        os.Exit(1)
    }
    defer db.Close()

    srv := server.New(db, logger, cfg)

    httpServer := &http.Server{
        Addr:         ":" + cfg.Port,
        Handler:      srv.Routes(),
        ReadTimeout:  5 * time.Second,
        WriteTimeout: 10 * time.Second,
        IdleTimeout:  120 * time.Second,
    }

    // Start server in a goroutine
    go func() {
        logger.Info("server starting", "port", cfg.Port)
        if err := httpServer.ListenAndServe(); err != nil && err != http.ErrServerClosed {
            logger.Error("server failed", "error", err)
            os.Exit(1)
        }
    }()

    // Wait for interrupt signal
    ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGTERM, syscall.SIGINT)
    defer stop()
    <-ctx.Done()

    // Graceful shutdown with timeout
    shutdownCtx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
    defer cancel()

    logger.Info("shutting down server")
    if err := httpServer.Shutdown(shutdownCtx); err != nil {
        logger.Error("server shutdown failed", "error", err)
        os.Exit(1)
    }
    logger.Info("server stopped")
}
```

## JSON Response Helpers

```go
// internal/server/response.go
func respondJSON(w http.ResponseWriter, status int, data any) {
    w.Header().Set("Content-Type", "application/json")
    w.WriteHeader(status)
    if data != nil {
        if err := json.NewEncoder(w).Encode(data); err != nil {
            // Log but don't try to write another response
            slog.Error("failed to encode response", "error", err)
        }
    }
}

func respondError(w http.ResponseWriter, status int, message string) {
    respondJSON(w, status, map[string]string{
        "error":   http.StatusText(status),
        "message": message,
    })
}

func decodeJSON(r *http.Request, dst any) error {
    dec := json.NewDecoder(r.Body)
    dec.DisallowUnknownFields()
    if err := dec.Decode(dst); err != nil {
        return fmt.Errorf("decode json: %w", err)
    }
    // Reject requests with multiple JSON objects
    if dec.More() {
        return fmt.Errorf("body must contain a single JSON object")
    }
    return nil
}
```

## Health and Ready Endpoints

```go
func (s *Server) healthCheck(w http.ResponseWriter, r *http.Request) {
    respondJSON(w, http.StatusOK, map[string]string{
        "status": "ok",
    })
}

func (s *Server) readyCheck(w http.ResponseWriter, r *http.Request) {
    ctx, cancel := context.WithTimeout(r.Context(), 2*time.Second)
    defer cancel()

    if err := s.db.PingContext(ctx); err != nil {
        respondError(w, http.StatusServiceUnavailable, "database not reachable")
        return
    }
    respondJSON(w, http.StatusOK, map[string]string{
        "status": "ready",
    })
}
```

## Go Module Layout for Services

```
myservice/
├── cmd/
│   └── api/
│       └── main.go             # Entry point, wiring, graceful shutdown
├── internal/
│   ├── server/
│   │   ├── server.go           # Server struct, Routes()
│   │   ├── users.go            # User handlers
│   │   ├── response.go         # JSON helpers
│   │   └── middleware.go       # Middleware functions
│   ├── service/
│   │   └── user.go             # Business logic
│   ├── repository/
│   │   └── user.go             # Database queries
│   └── model/
│       └── user.go             # Domain types
├── migrations/
│   ├── 001_create_users.up.sql
│   └── 001_create_users.down.sql
├── go.mod
├── go.sum
├── Makefile
└── Dockerfile
```

## Database Connection Setup

```go
func openDB(cfg Config) (*sql.DB, error) {
    db, err := sql.Open("pgx", cfg.DatabaseURL)
    if err != nil {
        return nil, fmt.Errorf("open db: %w", err)
    }

    db.SetMaxOpenConns(25)
    db.SetMaxIdleConns(10)
    db.SetConnMaxLifetime(5 * time.Minute)
    db.SetConnMaxIdleTime(1 * time.Minute)

    ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
    defer cancel()

    if err := db.PingContext(ctx); err != nil {
        db.Close()
        return nil, fmt.Errorf("ping db: %w", err)
    }
    return db, nil
}
```

Related skills: `api-contracts`, `postgresql`, `observability-logging`, `data-model`.
