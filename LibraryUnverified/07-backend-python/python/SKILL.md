---
name: python
description: "Guides Python project structure, tooling, and conventions: src layout vs flat, virtual environments (venv/poetry/uv), type hints with mypy/pyright, dependency management (pyproject.toml, poetry, uv), pytest testing, ruff/black linting, structured logging, async patterns, dataclasses/Pydantic, and packaging."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: python
  maturity: draft
  risk: low
  tags: [python, typing, testing, packaging]
---

# Purpose

Guides the structure, tooling, and conventions of Python projects—from virtual environment setup and dependency management through type annotations, testing with pytest, linting with ruff/black, structured logging, async patterns, and packaging for distribution. This skill covers the language-level foundations that framework-specific skills (Flask, FastAPI) build upon.

# When to use this skill

Use this skill when:

- setting up a new Python project (directory layout, pyproject.toml, virtual environment)
- configuring type checking with mypy or pyright
- writing or improving pytest tests (fixtures, parametrize, coverage)
- setting up linting/formatting with ruff, black, or isort
- managing dependencies with poetry, uv, pip, or pip-tools
- working with dataclasses or Pydantic models for data structures
- implementing async code with asyncio
- configuring structured logging (stdlib logging or structlog)
- packaging a Python library or application for distribution
- managing Python versions with pyenv

# Do not use this skill when

- the task is Flask-specific (routing, blueprints, extensions)—prefer `flask` skill
- the task is FastAPI-specific (path operations, dependency injection)—prefer `fastapi` skill
- the task is purely about database queries or ORM patterns—prefer `orm-patterns`
- the task involves no Python code at all

# Operating procedure

1. **Assess project layout.** Determine if the project uses `src/` layout or flat layout. For new projects, prefer `src/` layout for proper import isolation. Check for `pyproject.toml`, `setup.py`, or `setup.cfg`.
2. **Set up virtual environment.** Use `python -m venv .venv` for simple projects, `poetry install` for Poetry-managed projects, or `uv venv && uv pip install -e .` for uv-managed projects. Verify the venv is activated.
3. **Configure dependencies.** Define dependencies in `pyproject.toml` (preferred) or `requirements.txt`. Pin direct dependencies with version constraints. Use lock files (`poetry.lock`, `uv.lock`) for reproducible builds.
4. **Add type annotations.** Annotate function signatures, class attributes, and return types. Configure mypy or pyright in `pyproject.toml`. Run type checking: `mypy src/` or `pyright`.
5. **Write tests.** Use pytest with fixtures for test setup, `@pytest.mark.parametrize` for data-driven tests, and `conftest.py` for shared fixtures. Run with `pytest tests/ -v --cov=src`.
6. **Configure linting.** Set up ruff for linting and formatting (replaces flake8 + isort + black). Configure in `pyproject.toml` under `[tool.ruff]`. Run with `ruff check .` and `ruff format .`.
7. **Set up logging.** Use `logging.config.dictConfig()` or structlog for structured logging. Never use `print()` for operational output. Configure log levels per environment.
8. **Verify the full pipeline.** Run type check → lint → format → test → build in sequence: `mypy src/ && ruff check . && pytest tests/ -v`.

# Decision rules

- Use `pyproject.toml` as the single source of project metadata and tool configuration—avoid `setup.py`, `setup.cfg`, `tox.ini`, and scattered config files where possible.
- Prefer `src/` layout for libraries and packages (prevents accidental import of uninstalled code). Use flat layout only for simple scripts or single-module projects.
- Always use virtual environments. Never install project dependencies into the system Python.
- Type-annotate all public function signatures. Use `from __future__ import annotations` for forward references (Python 3.7–3.9).
- Use `dataclasses.dataclass` for simple data containers. Use Pydantic `BaseModel` when you need validation, serialization, or schema generation.
- Prefer `ruff` over separate flake8 + isort + black tools—it's faster and combines all three.
- Use `pytest` over `unittest`—it has less boilerplate, better fixtures, and richer plugin ecosystem.
- For async code, use `asyncio.run()` at the entry point. Use `async def` / `await` consistently through the call chain—don't mix sync and async without `asyncio.to_thread()`.
- Pin direct dependencies with version ranges (`>=1.0,<2.0`). Let the lock file pin transitives.
- Use `logging.getLogger(__name__)` for per-module loggers. Never use the root logger directly.

# Output requirements

1. `Project Structure` — layout, pyproject.toml, virtual environment setup
2. `Implementation` — code with type annotations, data structures
3. `Testing` — pytest tests, fixtures, parametrize, coverage command
4. `Quality` — mypy/ruff configuration, linting results
5. `Verification` — full pipeline command sequence

# References

Read these only when relevant:

- `references/implementation-patterns.md` — project layout, config, typing, logging, async patterns
- `references/validation-checklist.md` — pre-merge quality gates and checks
- `references/failure-modes.md` — common Python development pitfalls and their fixes

# Related skills

- `flask` — Flask web application development
- `fastapi` — FastAPI web application development
- `orm-patterns` — SQLAlchemy and database patterns
- `observability-logging` — advanced logging and observability
- `background-jobs-queues` — Celery, RQ, and async task processing

# Anti-patterns

- **No virtual environment.** Installing packages into the system Python, causing dependency conflicts across projects and non-reproducible builds.
- **`type: ignore` carpet-bombing.** Adding `# type: ignore` to silence all type errors instead of fixing them. Undermines the value of type checking entirely.
- **Mutable default arguments.** Using `def f(items=[]):`—the list is shared across all calls. Use `def f(items=None): items = items or []`.
- **Bare `except`.** Writing `except:` or `except Exception:` that catches and swallows all errors, including `KeyboardInterrupt` and `SystemExit`.
- **Circular imports.** Module A imports from module B at the top level, and module B imports from module A. Restructure or use late imports.
- **Star imports.** Using `from module import *` which pollutes the namespace and makes it impossible to trace where names come from.
- **Print debugging in production.** Using `print()` instead of `logging`. Print goes to stdout with no level, timestamp, or structured fields.
- **Pinning transitive dependencies.** Manually pinning versions of transitive dependencies in `requirements.txt` instead of using a lock file, causing maintenance burden and conflicts.
- **Missing `__init__.py`.** Forgetting `__init__.py` in package directories (required for regular packages; namespace packages are a deliberate advanced pattern).
- **Exception swallowing.** `try: ... except SomeError: pass`—catching an exception and doing nothing, hiding bugs.

# Failure handling

- If imports fail with `ModuleNotFoundError`, verify the virtual environment is activated and the package is installed (`pip list | grep package`).
- If circular imports occur, move shared types to a separate module or use `TYPE_CHECKING` conditional imports for type-only references.
- If mypy reports errors on third-party libraries, check for `py.typed` marker or install type stubs (`pip install types-requests`).
- If pytest can't find tests, verify test files are named `test_*.py` and the `tests/` directory has an `__init__.py` (or configure `pythonpath` in pytest config).
- If `ruff` and `black` conflict on formatting, switch to `ruff format` which is black-compatible and eliminates the conflict.
- If async code deadlocks, check for `asyncio.run()` called inside an already-running event loop, or blocking sync calls inside `async def` functions.
