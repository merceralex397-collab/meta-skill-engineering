# Failure Modes — Python

Common Python development pitfalls, their root causes, and fixes.

## Circular Import Dependency

**Symptom:** `ImportError: cannot import name 'X' from partially initialized module 'Y'` or `AttributeError` on a module.

**Cause:** Module A imports from module B at the top level, and module B imports from module A. Python's import system loads modules partially, so the second import finds an incomplete module.

**Fix:**
- Move shared types to a third module that both can import.
- Use late imports (import inside the function that needs it).
- For type annotations only, use `TYPE_CHECKING` conditional import:
```python
from __future__ import annotations
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from mypackage.models import User  # only evaluated by type checkers

def process_user(user: User) -> None:  # string annotation at runtime
    ...
```

## Missing `__init__.py`

**Symptom:** `ModuleNotFoundError: No module named 'mypackage.submodule'` even though the file exists.

**Cause:** The directory is not recognized as a Python package because `__init__.py` is missing. (Namespace packages exist but are a deliberate advanced pattern.)

**Fix:** Add an empty `__init__.py` to every package directory:
```bash
touch src/mypackage/__init__.py
touch src/mypackage/services/__init__.py
```

## Virtual Environment Not Activated

**Symptom:** `ModuleNotFoundError` for installed packages, or wrong Python version used. `pip install` installs into system Python.

**Cause:** The virtual environment was created but not activated, or a new terminal session was opened without activation.

**Fix:**
```bash
# Activate (must be done in every new shell session)
source .venv/bin/activate     # Linux/macOS
# Verify
which python                  # should point to .venv/bin/python
pip list                      # should show project dependencies
```
Configure your shell or IDE to auto-activate. Add `.venv/` to `.gitignore`.

## Dependency Conflicts

**Symptom:** `pip install` fails with `ResolutionImpossible` or packages fail at runtime with version mismatch.

**Cause:** Two packages require incompatible versions of a shared dependency. Often caused by overly strict pinning or very old pins.

**Fix:**
- Use version ranges, not exact pins, for direct dependencies: `requests>=2.28,<3.0`.
- Use a lock file for reproducible builds: `poetry lock` or `uv pip compile`.
- Check conflicts: `pip check` or `pipdeptree`.
- If conflict is irreconcilable, consider replacing one of the conflicting packages.

## `type: ignore` Overuse

**Symptom:** mypy passes, but runtime `TypeError` or `AttributeError` occurs because incorrect types were silently ignored.

**Cause:** `# type: ignore` was added to silence type errors instead of fixing them. Over time, real bugs accumulate behind the ignored lines.

**Fix:**
- Use specific ignore codes: `# type: ignore[assignment]` instead of blanket `# type: ignore`.
- Require a comment explaining why: `# type: ignore[override]  # base class has different signature`.
- Audit existing ignores periodically: `grep -r "type: ignore" src/ | wc -l`.
- Configure mypy to warn on unused ignores: `warn_unused_ignores = true`.

## Mutable Default Arguments

**Symptom:** Function accumulates state across calls. List or dict arguments "remember" values from previous invocations.

**Cause:** Default argument values are evaluated once at function definition time, not at each call. Mutable defaults are shared across all calls.

**Fix:**
```python
# BAD: list is created once and shared
def add_item(item: str, items: list[str] = []) -> list[str]:
    items.append(item)
    return items

# GOOD: use None sentinel and create fresh list
def add_item(item: str, items: list[str] | None = None) -> list[str]:
    if items is None:
        items = []
    items.append(item)
    return items

# ALSO GOOD: use dataclass with field(default_factory=list)
```

## Exception Swallowing

**Symptom:** Operations fail silently. Bugs are invisible until they cause cascading failures downstream.

**Cause:** Broad `except` clauses catch and discard exceptions:
```python
# BAD
try:
    result = risky_operation()
except Exception:
    pass  # silently swallowed
```

**Fix:**
- Catch specific exceptions: `except ValueError as e:`.
- At minimum, log the exception: `logger.exception("operation failed")`.
- Re-raise if you can't handle it: `raise` or `raise NewError() from e`.
- Never use bare `except:` (catches `SystemExit` and `KeyboardInterrupt`).

## GIL Bottleneck in CPU-Bound Code

**Symptom:** Multi-threaded Python code doesn't get speedup for CPU-bound work. All threads effectively run on one core.

**Cause:** Python's Global Interpreter Lock (GIL) prevents true parallel execution of Python bytecode in threads. Threading only helps for I/O-bound work.

**Fix:**
- For CPU-bound parallelism, use `multiprocessing` or `concurrent.futures.ProcessPoolExecutor`.
- For I/O-bound concurrency, use `asyncio` or `threading`.
- For numerical computation, use NumPy/Polars (release GIL in C extensions).
```python
from concurrent.futures import ProcessPoolExecutor

with ProcessPoolExecutor(max_workers=4) as pool:
    results = list(pool.map(cpu_heavy_function, data_chunks))
```

## `asyncio.run()` Called Inside Running Event Loop

**Symptom:** `RuntimeError: This event loop is already running` in Jupyter notebooks or nested async contexts.

**Cause:** `asyncio.run()` creates a new event loop, but one is already running (e.g., in Jupyter or inside an existing async framework).

**Fix:**
- In Jupyter: use `await coroutine()` directly (the notebook runs its own event loop).
- In production: ensure `asyncio.run()` is only at the top-level entry point.
- If you must run async from sync context inside an async app, use `asyncio.create_task()` or restructure to be fully async.

## Missing py.typed Marker

**Symptom:** Type checkers ignore your package's type annotations when it's installed as a dependency. Consumers see `module is missing library stubs`.

**Cause:** PEP 561 requires a `py.typed` marker file in the package root for type checkers to recognize inline type annotations.

**Fix:**
```bash
touch src/mypackage/py.typed
```
And include it in package data in `pyproject.toml`:
```toml
[tool.setuptools.package-data]
mypackage = ["py.typed"]
```

## Import Order Lint Failures

**Symptom:** CI fails on import sorting even though code works fine.

**Cause:** Imports are not sorted according to the configured style (isort/ruff). Different tools may disagree on order.

**Fix:** Standardize on one tool. Ruff includes isort-compatible sorting:
```bash
ruff check --select I --fix .  # fix import order
ruff format .                   # fix formatting
```
Configure in `pyproject.toml`:
```toml
[tool.ruff.lint.isort]
known-first-party = ["mypackage"]
```

Related skills: `flask`, `fastapi`, `orm-patterns`, `observability-logging`.
