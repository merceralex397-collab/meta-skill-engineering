---
name: express-node
description: "Guides Express.js API development: middleware chain ordering, Router composition, error-handling middleware, request validation (zod/joi/express-validator), async error propagation, security hardening with helmet/cors, TypeScript integration, graceful shutdown, and PM2 clustering."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: express-node
  maturity: draft
  risk: low
  tags: [express, node, typescript, api]
---

# Purpose

Guides the design and implementation of Express.js HTTP services—from middleware stack composition through route validation, error handling, TypeScript integration, security header configuration, and production deployment. Ensures Express-specific patterns like middleware ordering, 4-argument error handlers, and async error propagation are applied correctly.

# When to use this skill

Use this skill when:

- building or modifying an Express.js API (JavaScript or TypeScript)
- composing middleware chains (body parsing, auth, validation, error handling)
- adding request validation with zod, joi, or express-validator
- configuring security headers with helmet, CORS policies, or rate limiting
- setting up async error handling to prevent unhandled promise rejections
- implementing graceful shutdown for containerized or clustered deployments
- writing Express route tests with supertest

# Do not use this skill when

- the project uses a different Node.js framework (Fastify, Hapi, Koa)—adapt or skip
- the task is frontend React/Vue/Angular with no Express backend—prefer frontend skills
- the task is Python web (Flask/FastAPI/Django)—prefer `flask`, `fastapi`, or `python` skills
- the task is purely database schema design—prefer `orm-patterns` or `postgresql`

# Operating procedure

1. **Audit the middleware stack.** Verify ordering: body parsers first (`express.json()`, `express.urlencoded()`), then security middleware (`helmet()`, `cors()`), then auth middleware, then routes, then the error-handling middleware last.
2. **Organize routes with Router.** Each resource or domain gets its own `Router` instance in a separate file. Mount routers with `app.use("/api/v1/users", userRouter)`.
3. **Add input validation.** Every route that accepts user input must validate before processing. Use zod schemas, joi, or express-validator. Create reusable validation middleware.
4. **Handle async errors.** Wrap async route handlers to catch rejected promises. Use `express-async-errors` (require at top) or a manual wrapper: `const asyncHandler = (fn) => (req, res, next) => Promise.resolve(fn(req, res, next)).catch(next)`.
5. **Implement the error-handling middleware.** Define a 4-argument function `(err, req, res, next)` as the LAST middleware. Log the error, set the status code, and return a consistent error response shape.
6. **Configure security.** Enable `helmet()` for security headers. Configure CORS with an explicit origin allowlist. Add `express-rate-limit` to protect against abuse.
7. **Set up graceful shutdown.** Listen for `SIGTERM` and `SIGINT`. Call `server.close()` to stop accepting new connections, then wait for in-flight requests to complete before `process.exit(0)`.
8. **Write tests.** Use `supertest` to test routes without starting a real server. Test happy paths, validation failures, auth rejection, and error handler responses.
9. **Verify production readiness.** Confirm `NODE_ENV=production`, error stack traces are not leaked to clients, health check endpoint exists, and the process manager (PM2/Docker) is configured for restarts.

# Decision rules

- Middleware order matters: parsers → security → auth → routes → error handler. Violating this causes silent failures.
- Always use the 4-argument `(err, req, res, next)` signature for error-handling middleware—Express uses the arity to distinguish it.
- Never use `app.use(express.json())` after route definitions—body will be unparsed.
- Prefer `Router` for route grouping over defining all routes on `app` directly.
- Return consistent error shapes: `{ error: string, message: string, statusCode: number }`.
- Use `http-errors` or `createError(status, message)` for operational errors in routes.
- Set `trust proxy` when running behind a reverse proxy (nginx, ALB) so `req.ip` is correct.
- Use `express-rate-limit` on auth endpoints at minimum; apply globally for public APIs.
- For TypeScript, augment `Request` interface to type custom properties (e.g., `req.user`).
- Never call `res.send()` or `res.json()` after `next(err)`—the error handler owns the response.

# Output requirements

1. `Middleware Stack` — ordered list of middleware, mount points
2. `Route Implementation` — router file, method, path, validation, handler
3. `Error Handling` — error middleware, error shapes, status codes
4. `Security` — helmet config, CORS policy, rate limits
5. `Verification` — test commands, supertest assertions, health check

# References

Read these only when relevant:

- `references/implementation-patterns.md` — middleware, router, error, async, TypeScript patterns
- `references/validation-checklist.md` — pre-deploy and pre-merge verification items
- `references/failure-modes.md` — common Express runtime errors and their fixes

# Related skills

- `api-contracts` — OpenAPI/Swagger specification and contract testing
- `postgresql` — database integration and connection pooling for Node
- `rate-limits-retries` — advanced rate limiting and retry strategies
- `observability-logging` — structured logging for Node.js services
- `realtime-websocket` — WebSocket integration with Express

# Anti-patterns

- **Middleware soup.** Defining all middleware, routes, and error handlers in a single `app.js` file. Extract routers and middleware into separate modules.
- **Forgotten `next(err)`.** Catching an error in an async handler and sending a response manually instead of calling `next(err)`, bypassing centralized error handling and logging.
- **Error handler in the wrong position.** Registering the `(err, req, res, next)` middleware before routes, so it never receives errors from route handlers.
- **Blocking the event loop.** Running CPU-intensive synchronous operations (crypto, JSON parsing of huge payloads, image processing) in a route handler. Offload to a worker thread or external service.
- **`res.send()` after `res.send()`.** Attempting to send a response twice (e.g., forgetting `return` after `res.send()` in an if-block), causing `ERR_HTTP_HEADERS_SENT`.
- **Swallowing async errors.** Using `async` route handlers without a wrapper, causing unhandled promise rejections that crash the process in Node 15+.
- **Leaking stack traces.** Sending `err.stack` in production error responses, exposing internal file paths and code structure.
- **Global mutable state.** Storing request-scoped data in module-level variables, causing data leakage between concurrent requests.

# Failure handling

- If a route returns 404 unexpectedly, verify the router is mounted with `app.use()` and the HTTP method matches (`app.get` vs `app.post`).
- If the error handler is never invoked, check that it has exactly 4 parameters and is registered after all routes.
- If `req.body` is `undefined`, ensure `express.json()` middleware is mounted before the route.
- If CORS preflight requests fail, verify `cors()` is mounted before routes and the `OPTIONS` method is handled.
- If the process crashes on unhandled rejection, add `express-async-errors` or wrap async handlers with the `asyncHandler` pattern.
- If PM2 shows repeated restarts, check for synchronous throws or unhandled rejections in startup code.
