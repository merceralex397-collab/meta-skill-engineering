---
name: fastapi
description: Build or review FastAPI endpoints, dependencies, Pydantic models, and lifespan wiring with clear boundary contracts and testable error handling. Use this when editing FastAPI apps, routers, request or response models, dependency injection, or async API behavior. Do not use for generic Python scripts or framework-neutral API design.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: fastapi
  maturity: draft
  risk: low
  tags: [fastapi, api, pydantic, async]
---

# Purpose

Use this skill to keep FastAPI work explicit at the HTTP boundary instead of letting framework convenience leak through the whole service.

# When to use this skill

Use this skill when:

- adding or refactoring `FastAPI`, `APIRouter`, `Depends`, `response_model`, or `lifespan` wiring
- reviewing request validation, response shaping, auth dependencies, or background task usage in a FastAPI service
- debugging why a route, dependency chain, or model contract is behaving differently from the intended API surface

# Do not use this skill when

- the task is a general Python script, CLI, or batch job with no FastAPI boundary
- the main work is framework-neutral contract design rather than FastAPI implementation details
- a narrower active skill already owns the problem, such as pure database modeling or queue delivery

# Operating procedure

1. Inventory the app surface first.
   Locate the app entrypoint, router layout, dependency providers, exception handlers, and model modules before editing route code.

2. Keep transport models at the edge.
   Use Pydantic request and response models to define HTTP contracts. Do not pass raw ORM objects or ad hoc dicts through the handler boundary unless the repo already standardizes that pattern.

3. Make dependency flow obvious.
   Prefer small composable dependencies for auth, DB sessions, and request-scoped context. Avoid hidden globals or dependency functions that perform unrelated work.

4. Choose sync versus async deliberately.
   Use `async def` only when the handler actually awaits async I/O. Keep CPU-heavy work or blocking libraries out of the event loop unless the repo already wraps them safely. Example: `def` for sync DB calls via SQLAlchemy; `async def` only when using async drivers like `asyncpg` or `httpx`.

5. Normalize failure behavior.
   Decide where errors should become `HTTPException`, where domain errors should be translated centrally, and what shape the client should receive.

6. Verify the real boundary.
   Run the narrowest route or integration test that proves the request body, dependency chain, status code, and response model all match the intended contract.

# Decision rules

- Prefer `response_model` and explicit status codes over relying on implicit serialization.
- Keep DB session creation, auth lookup, and request context dependencies separate so failures are easier to reason about.
- Use `lifespan` or equivalent startup and shutdown wiring for shared resources; avoid scattered initialization side effects.
- If a route starts accumulating branching business logic, push that logic down into a service layer and keep the handler focused on transport concerns.

# Output requirements

1. `Service Surface`
2. `Router and Dependency Plan`
3. `Model and Error Shape`
4. `Validation`

# Scripts

- `scripts/route_inventory.py`: scan a FastAPI codebase for route decorators, methods, paths, and sync versus async handlers.

# References

Read these only when relevant:

- `references/router-and-dependency-patterns.md`
- `references/validation-and-response-shapes.md`
- `references/async-lifespan-and-testing.md`

# Related skills

- `python`
- `api-contracts`
- `auth-patterns`

# Anti-patterns

- Returning ORM models directly as responses, leaking internal schema to clients.
- Using `async def` for CPU-bound or blocking I/O operations that starve the event loop.
- Giant dependency chains that hide business logic behind layers of `Depends`.
- Catching exceptions in handlers instead of using centralized exception handlers.
- Missing `response_model` on routes, causing unvalidated response shapes.

# Failure handling

- If the repo does not separate handlers, services, and persistence yet, describe the existing pattern before forcing a refactor.
- If request and response shapes are unclear, stop and inventory the live contract before rewriting models.
- If the real bug is framework-neutral API design, hand off to `api-contracts` instead of stretching this skill.
