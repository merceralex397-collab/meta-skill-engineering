---
name: fastapi-patterns
description: "FastAPI application specialist for route design, dependency injection, Pydantic schemas, async patterns, database integration, authentication, and testing. Triggers: 'FastAPI route', 'Pydantic model', 'Depends()', 'FastAPI middleware', 'async endpoint', 'SQLAlchemy with FastAPI', 'FastAPI testing', 'OAuth2 JWT', 'FastAPI project structure', 'Alembic migration'. Do NOT use for Django, Flask, or other Python web frameworks; for general Python scripting unrelated to APIs; or for frontend-only work."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: fastapi-patterns
  maturity: draft
  risk: low
  tags: [fastapi, python, api, pydantic, sqlalchemy, async, rest-api, dependency-injection]
---

# Purpose

Domain skill for building production-grade FastAPI applications — covering project structure, route design, dependency injection, Pydantic schema patterns, async/sync execution, database integration (SQLAlchemy 2.0), authentication, middleware, and testing. Produces idiomatic FastAPI code following the framework's conventions and Python typing best practices.

# When to use this skill

Use this skill when:

- scaffolding a new FastAPI project or adding routes/endpoints to an existing one
- designing Pydantic request/response schemas with validation
- implementing dependency injection chains (`Depends()`) for auth, DB sessions, pagination, or shared logic
- integrating SQLAlchemy 2.0 (sync or async) with Alembic migrations
- implementing authentication (OAuth2 + JWT, API key, session-based)
- writing or reviewing FastAPI tests (TestClient, httpx.AsyncClient, dependency overrides)
- adding middleware (CORS, request timing, logging, exception handlers)
- deciding between `async def` and `def` for route handlers

# Do not use this skill when

- the framework is **Django**, **Flask**, **Starlette** (without FastAPI), or **Litestar** — different patterns and conventions
- the task is **general Python scripting** unrelated to API development
- the task is **frontend-only** (use `tauri-solidjs` or a frontend skill)
- a quick one-off data transformation or calculation — use `misc-helper`
- the task is specifically about **BigQuery SQL** — use `bigquery-skill`

# Operating procedure

## 1. Assess project state

- Check for existing project structure: does `app/main.py` exist? Is there a `pyproject.toml` or `requirements.txt` with `fastapi` listed?
- Identify FastAPI version: v0.100+ uses Pydantic v2 (`model_config` instead of inner `Config` class, `from_attributes` instead of `orm_mode`). Check `pyproject.toml` or `pip freeze`.
- Identify database layer: SQLAlchemy 1.4/2.0, Tortoise ORM, SQLModel, or none.
- Check for existing patterns: router organization, dependency injection style, schema naming conventions.

## 2. Project structure (scaffold or validate)

```
app/
├── main.py                    # FastAPI app instance, startup/shutdown, include_router
├── core/
│   ├── config.py              # Settings via pydantic-settings (BaseSettings)
│   ├── security.py            # JWT creation/verification, password hashing
│   └── database.py            # Engine, SessionLocal, get_db dependency
├── models/                    # SQLAlchemy ORM models
│   ├── __init__.py
│   ├── user.py                # class User(Base): ...
│   └── item.py
├── schemas/                   # Pydantic request/response schemas
│   ├── __init__.py
│   ├── user.py                # UserCreate, UserRead, UserUpdate
│   └── item.py
├── routers/                   # APIRouter modules grouped by domain
│   ├── __init__.py
│   ├── users.py               # router = APIRouter(prefix="/users", tags=["users"])
│   └── items.py
├── services/                  # Business logic (called by routers)
│   ├── __init__.py
│   ├── user_service.py
│   └── item_service.py
├── dependencies.py            # Shared Depends() functions (auth, pagination)
├── middleware.py               # Custom middleware (timing, logging)
└── exceptions.py              # Custom exception classes + handlers
alembic/
├── alembic.ini
├── env.py
└── versions/
tests/
├── conftest.py                # Fixtures: app, client, db_session, auth headers
├── test_users.py
└── test_items.py
```

## 3. Route design

- Group routes by domain in separate `APIRouter` instances with `prefix` and `tags`.
- Keep route handlers thin — extract business logic into service functions:
  ```python
  @router.post("/", response_model=UserRead, status_code=201)
  async def create_user(
      user_in: UserCreate,
      db: AsyncSession = Depends(get_db),
      current_user: User = Depends(get_current_admin),
  ):
      return await user_service.create(db, user_in)
  ```
- Use `response_model` to control serialization and strip internal fields from responses.
- Use appropriate status codes: `201` for creation, `204` for deletion, `200` for reads and updates.

## 4. Pydantic schema patterns

- **Separate request and response schemas**: `UserCreate` (input) vs `UserRead` (output). Never expose password hashes or internal IDs through response schemas.
- **Base → Create → Read → Update hierarchy**:
  ```python
  class UserBase(BaseModel):
      email: EmailStr
      name: str

  class UserCreate(UserBase):
      password: str

  class UserRead(UserBase):
      id: int
      created_at: datetime
      model_config = ConfigDict(from_attributes=True)  # Pydantic v2

  class UserUpdate(BaseModel):
      email: EmailStr | None = None
      name: str | None = None
  ```
- Use `Field()` for validation: `Field(min_length=1, max_length=255)`, `Field(ge=0)`, `Field(pattern=r"^[a-z]+$")`.
- Use custom validators with `@field_validator` (Pydantic v2) or `@validator` (v1) for complex validation logic.

## 5. Dependency injection

- **Database session**: use a generator dependency that yields a session and ensures cleanup:
  ```python
  async def get_db() -> AsyncGenerator[AsyncSession, None]:
      async with async_session_maker() as session:
          yield session
  ```
- **Authentication**: chain dependencies — `get_current_user` depends on `oauth2_scheme` which depends on the token header:
  ```python
  async def get_current_user(
      token: str = Depends(oauth2_scheme),
      db: AsyncSession = Depends(get_db),
  ) -> User:
      payload = verify_token(token)
      user = await db.get(User, payload["sub"])
      if not user:
          raise HTTPException(status_code=401, detail="User not found")
      return user
  ```
- **Pagination**: reusable dependency for list endpoints:
  ```python
  def get_pagination(skip: int = Query(0, ge=0), limit: int = Query(20, ge=1, le=100)):
      return {"skip": skip, "limit": limit}
  ```
- Nested dependencies are resolved automatically by FastAPI. Use `Depends()` at any depth.

## 6. Async vs sync decision

- Use `async def` when:
  - Making async I/O calls (async DB drivers like `asyncpg`, HTTP calls with `httpx`, file I/O with `aiofiles`)
  - The route is I/O bound and benefits from concurrency
- Use plain `def` when:
  - Using synchronous libraries (sync SQLAlchemy, `requests`, CPU-bound computation)
  - FastAPI automatically runs `def` handlers in a thread pool, so they won't block the event loop
- **NEVER** use sync blocking calls inside an `async def` handler — this blocks the entire event loop. Either use `def` (thread pool) or use `asyncio.to_thread()` / `run_in_executor()`.

## 7. Database integration (SQLAlchemy 2.0)

- **Async setup**:
  ```python
  from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker, AsyncSession

  engine = create_async_engine("postgresql+asyncpg://...", echo=False)
  async_session_maker = async_sessionmaker(engine, expire_on_commit=False)
  ```
- **Sync setup** (simpler, use when async is not needed):
  ```python
  from sqlalchemy import create_engine
  from sqlalchemy.orm import sessionmaker

  engine = create_engine("postgresql://...", pool_pre_ping=True)
  SessionLocal = sessionmaker(bind=engine, autoflush=False)
  ```
- **Alembic migrations**: `alembic init alembic`, configure `env.py` to import your `Base.metadata`, run `alembic revision --autogenerate -m "description"`, then `alembic upgrade head`.
- **Repository pattern**: encapsulate DB operations in service/repository functions. Routes should not contain raw SQL or ORM queries.

## 8. Authentication patterns

- **OAuth2 + JWT** (most common):
  ```python
  oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/auth/token")

  @router.post("/auth/token")
  async def login(form_data: OAuth2PasswordRequestForm = Depends(), db=Depends(get_db)):
      user = await authenticate_user(db, form_data.username, form_data.password)
      if not user:
          raise HTTPException(status_code=401, detail="Invalid credentials")
      token = create_access_token(data={"sub": str(user.id)})
      return {"access_token": token, "token_type": "bearer"}
  ```
- **API Key auth**: use `APIKeyHeader` or `APIKeyQuery` from `fastapi.security`.
- **Role-based access**: create dependency variants like `get_current_admin` that wrap `get_current_user` and check roles.

## 9. Middleware and exception handling

- **CORS**: always configure explicitly:
  ```python
  app.add_middleware(CORSMiddleware, allow_origins=["http://localhost:3000"],
                     allow_methods=["*"], allow_headers=["*"])
  ```
- **Request timing middleware**:
  ```python
  @app.middleware("http")
  async def add_timing_header(request: Request, call_next):
      start = time.perf_counter()
      response = await call_next(request)
      response.headers["X-Process-Time"] = str(time.perf_counter() - start)
      return response
  ```
- **Exception handlers**: register custom handlers for domain exceptions:
  ```python
  @app.exception_handler(NotFoundError)
  async def not_found_handler(request: Request, exc: NotFoundError):
      return JSONResponse(status_code=404, content={"detail": str(exc)})
  ```

## 10. Testing

- **Sync tests** with `TestClient`:
  ```python
  from fastapi.testclient import TestClient
  client = TestClient(app)
  response = client.post("/users/", json={"email": "test@example.com", "name": "Test", "password": "secret"})
  assert response.status_code == 201
  ```
- **Async tests** with `httpx.AsyncClient`:
  ```python
  async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
      response = await client.get("/users/1")
      assert response.status_code == 200
  ```
- **Dependency overrides** for isolating tests:
  ```python
  app.dependency_overrides[get_db] = lambda: test_db_session
  app.dependency_overrides[get_current_user] = lambda: mock_user
  ```
- Always clean up overrides: `app.dependency_overrides.clear()` in teardown.

# Decision rules

- **Thin routes, fat services**: route handlers should be 5–15 lines max. Extract validation logic, DB queries, and business rules into service functions.
- **Schema separation always**: never reuse the same Pydantic model for both input and output. Create/Read/Update schemas prevent accidental field exposure.
- **`async def` only with async I/O**: if you can't use an async driver, use `def`. Mixing sync blocking calls in `async def` is worse than using `def` everywhere.
- **Dependency injection over global state**: never import a DB session or config at module level for use in routes. Always use `Depends()`.
- **Pydantic v2 for new projects**: use `model_config = ConfigDict(...)` not inner `class Config`. Use `from_attributes=True` not `orm_mode=True`.
- **Repository pattern over inline queries**: routes should not contain `db.execute(select(...))` directly. Wrap in service/repository functions for testability and reuse.
- **Explicit response_model**: always set `response_model` on routes that return data. This ensures serialization strips internal fields and generates accurate OpenAPI docs.
- **Test with dependency overrides**: never mock FastAPI internals. Use `app.dependency_overrides` to swap dependencies for test doubles.

# Output requirements

Every response must include the applicable sections:

1. **`Route Definition`** — complete route handler with type annotations, `response_model`, status code, and dependencies. Include the router registration.
2. **`Schema Pair`** — Pydantic `Create` and `Read` (and `Update` if relevant) models with field validation and `model_config`.
3. **`Dependency Chain`** — the full chain of `Depends()` functions the route uses, from leaf (DB session) to root (auth guard).
4. **`Test Stub`** — a working test function using `TestClient` or `httpx.AsyncClient` with dependency overrides for the route.
5. **`Migration Note`** (if schema changes are involved) — the Alembic command to generate and apply the migration.

# Anti-patterns

- **Sync DB calls in `async def` routes**: calling synchronous SQLAlchemy `session.execute()` inside an `async def` handler blocks the event loop and kills concurrency. Use async SQLAlchemy or change the handler to `def`.
- **No schema validation**: accepting raw `dict` or `Any` instead of Pydantic models. This skips validation, loses OpenAPI docs, and invites injection bugs.
- **Fat route handlers**: putting DB queries, business logic, error handling, and response formatting all in one route function. Extract into services.
- **Missing dependency injection**: importing `SessionLocal()` directly in route files instead of using `Depends(get_db)`. This makes testing impossible without monkeypatching.
- **Shared request/response models**: using `UserCreate` as both the input and the response schema. This exposes password fields in responses.
- **Catching broad exceptions**: `except Exception: return 500` in route handlers. Use FastAPI's exception handler system with typed exceptions.
- **No CORS configuration**: forgetting to add `CORSMiddleware` and wondering why the frontend gets blocked. Always configure explicitly for known origins.
- **Hardcoded configuration**: putting database URLs, secret keys, or API keys directly in code. Use `pydantic-settings` with environment variables.
- **Testing without dependency overrides**: writing tests that hit a real database or external service because dependencies weren't overridden.

# Related skills

- `bigquery-skill` — for FastAPI routes that query BigQuery as a data source
- `tauri-solidjs` — for desktop frontends that consume FastAPI backends
- `misc-helper` — for quick utility tasks during API development

# Failure handling

- If the FastAPI version is ambiguous, check `pip freeze | grep fastapi` and `pip freeze | grep pydantic`. Pydantic v1 vs v2 changes schema syntax significantly.
- If async tests hang, verify the event loop policy and ensure `pytest-asyncio` is configured with `asyncio_mode = "auto"` in `pyproject.toml`.
- If dependency injection fails with circular imports, restructure: put dependencies in `dependencies.py`, models in `models/`, and schemas in `schemas/`. Import from these modules in routers.
- If Alembic autogenerate misses changes, verify that `env.py` imports all model modules so `Base.metadata` is fully populated before comparison.
- If `response_model` strips fields unexpectedly, check that the Pydantic model includes `from_attributes=True` (v2) or `orm_mode=True` (v1) and that the field names match the ORM model attributes.
