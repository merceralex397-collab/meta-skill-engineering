# Validation Checklist — Express / Node.js

An Express change is not ready for merge until every applicable item is verified.

## Middleware Order

- [ ] `express.json()` and `express.urlencoded()` are mounted BEFORE any route that reads `req.body`
- [ ] `helmet()` is mounted BEFORE any route sends a response
- [ ] `cors()` is mounted BEFORE routes (handles OPTIONS preflight)
- [ ] Authentication middleware is mounted BEFORE protected routes
- [ ] Error-handling middleware `(err, req, res, next)` is the LAST `app.use()` call
- [ ] 404 catch-all is registered AFTER all route handlers

## Security

- [ ] `helmet()` is enabled (sets `X-Content-Type-Options`, `X-Frame-Options`, CSP, etc.)
- [ ] CORS origin is an explicit allowlist, not `*` in production
- [ ] Rate limiting is configured on at minimum auth endpoints (`express-rate-limit`)
- [ ] Request body size is limited (`express.json({ limit: "10kb" })`)
- [ ] `trust proxy` is set when behind a reverse proxy (needed for correct `req.ip`)
- [ ] Error responses in production do NOT include stack traces
- [ ] Sensitive headers are not logged

## Input Validation

- [ ] Every route accepting user input validates `req.body`, `req.params`, and `req.query`
- [ ] Validation uses a schema library (zod, joi, express-validator), not manual checks
- [ ] Validated/parsed data replaces raw `req.body` (type safety)
- [ ] Validation errors return 400 with field-level detail
- [ ] File uploads validate type, size, and count

## Async Error Handling

- [ ] All `async` route handlers are wrapped with `asyncHandler` or `express-async-errors` is loaded
- [ ] No `async` function is passed to `app.use()` or `router.get()` without error catching
- [ ] `process.on("unhandledRejection")` logs and exits (fail-fast)
- [ ] `process.on("uncaughtException")` logs and exits (non-recoverable)
- [ ] Promises in middleware chains are not fire-and-forget

## Error Handler

- [ ] Centralized error handler has exactly 4 arguments: `(err, req, res, next)`
- [ ] Operational errors (4xx) return user-friendly messages
- [ ] Programming errors (5xx) return generic message in production
- [ ] All errors are logged with context (method, path, request ID)
- [ ] Error response shape is consistent: `{ error, message, statusCode }`

## Graceful Shutdown

- [ ] `SIGTERM` and `SIGINT` handlers call `server.close()`
- [ ] In-flight requests are allowed to complete before exit
- [ ] Database connections are closed during shutdown
- [ ] Forced exit timeout exists (e.g., 30s) to prevent zombie processes

## Health and Observability

- [ ] `GET /health` endpoint returns 200 with uptime/timestamp
- [ ] `GET /ready` endpoint checks downstream dependencies (DB, cache)
- [ ] Request logging middleware is in place (morgan, pino-http, or custom)
- [ ] Request ID is generated or propagated (`X-Request-Id` header)
- [ ] Structured logging is used (pino, winston), not `console.log`

## Testing

- [ ] Route tests use `supertest` (no real server needed)
- [ ] Tests cover happy path, validation errors, auth rejection, and 500 scenarios
- [ ] Test database is isolated (in-memory SQLite, test container, or mocked)
- [ ] Tests run with: `npm test` or `jest --coverage`
- [ ] No test depends on global state from another test

## Deployment

- [ ] `NODE_ENV=production` is set in production
- [ ] App starts with a process manager (PM2, Docker, systemd), not `node app.js`
- [ ] PM2 cluster mode or Docker replicas for multi-core usage
- [ ] Static assets served by nginx/CDN, not Express in production
- [ ] `.env` file is NOT committed; `dotenv` loads from environment

## Pre-merge Smoke Test

```bash
npm run lint
npm test -- --coverage
NODE_ENV=test npm start &  # verify startup
curl -s http://localhost:3000/health | jq .
kill %1
```
