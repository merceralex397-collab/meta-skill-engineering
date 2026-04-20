# Router And Dependency Patterns

Use these patterns when editing a FastAPI service:

- Keep the app entrypoint thin. Mount routers, middleware, exception handlers, and startup wiring there instead of business logic.
- Group routes by domain or bounded context, not by HTTP verb.
- Use `Depends(...)` for request-scoped concerns such as auth context, DB sessions, tenant lookup, and pagination parsing.
- Avoid dependencies that both validate auth and perform unrelated side effects like audit writes or cache warming.
- Keep dependency return values small and explicit. Prefer returning a typed context object over a large mutable bag of values.

Suggested file split:

- `main.py` or `app.py`: app construction, middleware, router inclusion, lifespan
- `routers/`: route handlers and local request parsing
- `dependencies/`: auth, session, and request-context helpers
- `services/`: domain logic and orchestration
- `schemas/` or `models/api/`: request and response models

Warning signs:

- routes importing ORM internals directly from many modules
- auth, DB session, and feature-flag logic combined in one dependency
- global singletons with hidden startup order
- response shaping happening ad hoc inside every handler
