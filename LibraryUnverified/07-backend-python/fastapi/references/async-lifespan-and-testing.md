# Async, Lifespan, And Testing

Async decisions:

- use `async def` for handlers that await async database or network I/O
- use plain `def` for CPU-bound or synchronous library work
- do not wrap blocking calls in async handlers without moving them off the event loop

Lifespan guidance:

- initialize shared clients, pools, or caches in `lifespan`
- clean them up there too
- keep startup side effects deterministic and testable

Testing guidance:

- prefer route or integration tests that prove request validation, dependency behavior, and response shape together
- when debugging route registration, use `scripts/route_inventory.py`
- test at least one negative path for auth, validation, or not-found behavior when touching route logic

Common failures:

- startup code hidden at import time
- async handlers calling blocking SDKs directly
- tests asserting only status code while ignoring response model drift
