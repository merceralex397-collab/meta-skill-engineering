---
name: cloudflare-worker-patterns
description: "Write and optimize Cloudflare Workers — implement fetch handlers, Durable Objects for stateful logic, service bindings, KV/R2 data access, scheduled/cron triggers, and middleware patterns with wrangler dev/deploy. Use when writing Worker code, debugging Worker runtime behavior, or designing Durable Object state machines. Do not use for Cloudflare DNS/WAF/Pages configuration (prefer cloudflare skill) or non-Worker serverless platforms."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cloudflare-worker-patterns
  maturity: draft
  risk: low
  tags: [cloudflare-workers, durable-objects, service-bindings, wrangler]
---

# Purpose

Write, structure, and optimize Cloudflare Worker application code — fetch handlers, Durable Objects for stateful coordination, service bindings for Worker-to-Worker calls, KV/R2/D1 data access patterns, scheduled/cron handlers, and middleware/router patterns using the Workers runtime.

# When to use this skill

- Implementing a `fetch` handler that routes requests, transforms responses, or proxies origins.
- Creating Durable Objects for stateful logic (counters, rate limiters, WebSocket rooms, coordination).
- Setting up service bindings to call one Worker from another without network hops.
- Reading/writing KV, R2, or D1 from within Worker code using environment bindings.
- Implementing `scheduled` event handlers for cron-triggered background tasks.
- Building middleware chains (auth, logging, CORS) in a Worker router framework (Hono, itty-router).
- Debugging Worker runtime errors, CPU time limits, or memory issues with `wrangler dev` and `wrangler tail`.
- Writing Worker unit tests using Miniflare or `wrangler test` (Vitest integration).

# Do not use this skill when

- The task is about Cloudflare DNS records, WAF rules, cache page rules, or Pages build config — prefer `cloudflare`.
- The target is AWS Lambda, GCP Cloud Functions, or another non-Cloudflare serverless platform — prefer `serverless-patterns`.
- The task involves only `wrangler.toml` bindings without any Worker code changes — prefer `cloudflare`.
- The focus is generic serverless architecture design without Cloudflare-specific constraints.

# Operating procedure

1. **Identify the Worker entry point.** Locate the `src/index.ts` (or `.js`) file exported as the Worker module. Confirm it exports a `fetch` handler and optionally `scheduled`, `queue`, or `email` handlers.
2. **Set up the local dev environment.** Run `wrangler dev` to start the local development server. Confirm bindings (KV, R2, D1, Durable Objects) are available via `--local` or `--remote` flags.
3. **Implement the fetch handler.** Parse the incoming `Request` URL and method. Route to handler functions using a router (Hono: `app.get('/path', handler)`, itty-router: `router.get('/path', handler)`). Return a `new Response()` with appropriate status, headers, and body.
4. **Add middleware.** Insert middleware functions for cross-cutting concerns: CORS headers (`Access-Control-Allow-Origin`), authentication (verify JWT or API key from `Authorization` header), request logging (timestamp, method, path, status), and error wrapping (try/catch returning 500 with error ID).
5. **Implement Durable Object classes.** Export a class extending `DurableObject`. Implement `fetch()` for HTTP-based state access. Use `this.ctx.storage.get/put/delete` for persistent state. Use `this.ctx.storage.transaction()` for atomic multi-key operations. Bind the DO in `wrangler.toml` under `[durable_objects]`.
6. **Wire data access patterns.** For KV: use `env.MY_KV.get(key)` / `.put(key, value, {expirationTtl})`. For R2: use `env.MY_BUCKET.get(key)` / `.put(key, body)`. For D1: use `env.MY_DB.prepare('SELECT ...').bind(params).all()`. Handle null returns (key not found) explicitly.
7. **Implement scheduled handlers.** Export a `scheduled(event, env, ctx)` function. Use `event.cron` to distinguish between multiple cron triggers. Use `ctx.waitUntil()` for async work that must complete after the handler returns.
8. **Set up service bindings.** In `wrangler.toml`, add `[[services]]` with `binding`, `service`, and `environment`. Call the bound service from Worker code via `env.MY_SERVICE.fetch(request)`.
9. **Write tests.** Use Miniflare for integration tests that exercise bindings. Use Vitest with `wrangler test` (unstable_dev) for unit tests. Mock external fetches with `fetchMock`. Test Durable Objects by creating stubs via `env.MY_DO.get(id)`.
10. **Debug runtime issues.** Check CPU time limits (10ms free, 30ms paid for fetch; 30s for cron). Use `wrangler tail` to stream live logs. Check for unhandled promise rejections that silently fail. Verify `ctx.waitUntil()` is used for background work.
11. **Deploy and verify.** Run `wrangler deploy`. Hit the production URL and verify responses. Check `wrangler tail --format=json` for errors in production traffic.

# Decision rules

- Use Durable Objects when you need strongly consistent state or coordination between requests — KV is eventually consistent.
- Use KV for read-heavy, write-infrequent data (config, feature flags, cached API responses).
- Use R2 for binary data >25MB or when S3-compatible API access is needed.
- Use D1 for relational queries — but be aware of row limits and SQLite constraints.
- Use `ctx.waitUntil()` for fire-and-forget work (analytics, logging) — do not `await` it in the response path.
- Use service bindings over `fetch('https://other-worker.example.com')` to avoid network overhead and enforce internal-only access.
- Keep Worker code under the 1MB compressed size limit. Use dynamic imports or split into multiple Workers if approaching the limit.

# Output requirements

1. **Worker code** — the `fetch`/`scheduled` handler implementation with proper typing.
2. **Durable Object class** — if stateful logic is needed, the class with storage operations.
3. **wrangler.toml changes** — any new bindings, DO declarations, or service binding configs.
4. **Test file** — at least one test covering the primary handler path.
5. **Deployment verification** — confirmed the Worker responds correctly at its production route.

# References

- Workers runtime API: https://developers.cloudflare.com/workers/runtime-apis/
- Durable Objects: https://developers.cloudflare.com/durable-objects/
- Hono framework for Workers: https://hono.dev/docs/getting-started/cloudflare-workers
- Miniflare testing: https://miniflare.dev/
- Workers size and CPU limits: https://developers.cloudflare.com/workers/platform/limits/
- `references/preflight-checklist.md`

# Related skills

- `cloudflare` — DNS, WAF, Pages, R2/KV/D1 provisioning and `wrangler.toml` configuration.
- `serverless-patterns` — generic serverless architecture patterns.
- `vercel` — alternative edge runtime platform.

# Anti-patterns

- Blocking the fetch handler with long-running synchronous work — use `ctx.waitUntil()` for background tasks.
- Using `fetch()` to call another Worker in the same account instead of service bindings.
- Storing large objects (>25MB) in KV — use R2 instead.
- Relying on global variables for request-scoped state — Workers may share isolates across requests.
- Not handling the `null` return from `KV.get()` or `R2.get()` — always check for missing keys.
- Writing Durable Object state without transactions when multiple keys must be atomically consistent.

# Failure handling

- If `wrangler dev` fails to start, check that `wrangler.toml` bindings have valid IDs and that `node_modules` is installed.
- If a Worker exceeds CPU time limits, profile the handler to find expensive operations. Move heavy computation to a queued Worker or Durable Object alarm.
- If Durable Object storage operations throw, wrap in try/catch and return a 503 with a retry-after header.
- If `wrangler deploy` succeeds but the Worker returns errors, check `wrangler tail` for unhandled exceptions and verify all environment bindings are provisioned in the target environment.
- If the task is about platform configuration (DNS, WAF, caching) rather than Worker code, redirect to the `cloudflare` skill.
