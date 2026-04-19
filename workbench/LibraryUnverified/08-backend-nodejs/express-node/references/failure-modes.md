# Failure Modes — Express / Node.js

Common Express runtime errors, their root causes, and fixes.

## Unhandled Promise Rejection Crashing the Process

**Symptom:** Process exits with `UnhandledPromiseRejectionWarning` (Node 14) or crashes immediately (Node 15+).

**Cause:** An `async` route handler throws, but Express doesn't natively catch async rejections. The rejection propagates to the Node.js process level.

**Fix:** Wrap all async handlers:
```javascript
// Option A: manual wrapper
const asyncHandler = (fn) => (req, res, next) =>
  Promise.resolve(fn(req, res, next)).catch(next);

router.get("/users", asyncHandler(async (req, res) => { /* ... */ }));

// Option B: require express-async-errors at entry point
require("express-async-errors");
```
Also add a global safety net:
```javascript
process.on("unhandledRejection", (reason) => {
  logger.error("Unhandled rejection", { reason });
  process.exit(1);
});
```

## Middleware Order Bug: Error Handler Before Routes

**Symptom:** Errors in route handlers are not caught; clients receive default Express HTML error pages.

**Cause:** The `(err, req, res, next)` middleware was registered before routes, so Express doesn't route errors to it.

**Fix:** Move the error handler to the very last `app.use()` call, after all routers and the 404 handler.

## Missing Body Parser

**Symptom:** `req.body` is `undefined` in POST/PUT handlers.

**Cause:** `express.json()` is not mounted, or is mounted after the route.

**Fix:**
```javascript
// Must be before any route that reads req.body
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
```

## CORS Preflight Failures

**Symptom:** Browser shows CORS error on non-simple requests (PUT, DELETE, custom headers). GET works fine.

**Cause:** The browser sends an OPTIONS preflight request. If `cors()` middleware is not mounted or is mounted after a catch-all route, the preflight gets a 404 or missing headers.

**Fix:**
```javascript
const cors = require("cors");
app.use(cors({
  origin: ["https://app.example.com"],
  methods: ["GET", "POST", "PUT", "DELETE"],
  credentials: true,
}));
// Must be before routes
```

## Memory Leaks from Unclosed Connections

**Symptom:** Memory usage grows over time. Heap dumps show accumulating socket or database connection objects.

**Cause:** Database connections, Redis clients, or HTTP agents are created per-request but never closed. Or event listeners are added per-request without removal.

**Fix:**
- Use connection pools (pg `Pool`, mongoose connection pool) instead of per-request connections.
- Close connections in graceful shutdown handler.
- Audit for event listener leaks: `emitter.on()` in request handlers without corresponding `removeListener()`.
- Monitor with `process.memoryUsage()` in health check.

## Event Loop Blocking

**Symptom:** All requests hang or time out. Server becomes unresponsive under load.

**Cause:** A synchronous CPU-intensive operation (large JSON parse, crypto, image processing, tight loop) blocks the event loop.

**Fix:**
- Move CPU-intensive work to `worker_threads` or a separate microservice.
- Use streaming JSON parsers for large payloads.
- Set `express.json({ limit: "1mb" })` to reject oversized payloads.
- Monitor event loop lag with `monitorEventLoopDelay()` (Node 12+).

## ERR_HTTP_HEADERS_SENT

**Symptom:** `Error [ERR_HTTP_HEADERS_SENT]: Cannot set headers after they are sent to the client`

**Cause:** `res.send()` or `res.json()` is called twice in the same request—typically because the handler doesn't `return` after sending an early response.

**Fix:**
```javascript
// BAD
if (!user) {
  res.status(404).json({ error: "Not found" });
  // Missing return! Execution continues...
}
res.json({ data: user });

// GOOD
if (!user) {
  return res.status(404).json({ error: "Not found" });
}
res.json({ data: user });
```

## `trust proxy` Not Set Behind Reverse Proxy

**Symptom:** `req.ip` returns `127.0.0.1` or the proxy's IP instead of the client's IP. Rate limiting doesn't work correctly.

**Cause:** Express doesn't trust `X-Forwarded-For` headers by default.

**Fix:**
```javascript
app.set("trust proxy", 1); // trust first proxy
// Or for specific proxy:
app.set("trust proxy", "loopback, 10.0.0.0/8");
```

## Payload Too Large (413)

**Symptom:** POST requests with larger bodies fail with 413.

**Cause:** `express.json({ limit })` defaults to 100kb.

**Fix:** Increase the limit deliberately:
```javascript
app.use(express.json({ limit: "1mb" }));
```
But set a reasonable upper bound—don't remove the limit entirely.

## Graceful Shutdown Not Working

**Symptom:** Containerized service kills in-flight requests on deploy. Clients see connection reset errors.

**Cause:** No SIGTERM handler, or `process.exit()` is called immediately without waiting for `server.close()`.

**Fix:** See the graceful shutdown pattern in `implementation-patterns.md`. The key is: stop accepting new connections → wait for in-flight to drain → close DB pools → exit.

## Environment Config Not Loaded

**Symptom:** `process.env.DATABASE_URL` is `undefined`.

**Cause:** `dotenv` is not loaded early enough, or `.env` file is missing.

**Fix:**
```javascript
// Must be the very first line in entry point
require("dotenv").config();
```
Verify `.env` exists and is not committed to git (listed in `.gitignore`).

Related skills: `api-contracts`, `rate-limits-retries`, `observability-logging`.
