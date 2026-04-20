# Validation Checklist — Python

A Python change is not ready for merge until every applicable item is verified.

## Type Checking

- [ ] `mypy --strict src/` or `pyright src/` passes with no errors
- [ ] All public function signatures have type annotations (parameters and return type)
- [ ] No `# type: ignore` without an accompanying explanation comment
- [ ] `from __future__ import annotations` is used in files needing forward references (3.7–3.9)
- [ ] Generic types use modern syntax (`list[str]` not `List[str]`) on Python 3.9+
- [ ] `py.typed` marker file exists at package root for typed packages

## Testing

- [ ] All tests pass: `pytest tests/ -v`
- [ ] Coverage meets project threshold: `pytest --cov=src --cov-fail-under=80`
- [ ] New functionality has corresponding tests
- [ ] Fixtures are in `conftest.py`, not duplicated across test files
- [ ] `@pytest.mark.parametrize` is used for data-driven test cases
- [ ] No tests depend on execution order or shared mutable state
- [ ] Async tests use `@pytest.mark.asyncio` and `pytest-asyncio`

## Linting and Formatting

- [ ] `ruff check .` passes (or `flake8` if ruff not adopted)
- [ ] `ruff format --check .` passes (or `black --check .`)
- [ ] Import order is consistent (ruff isort rules or `isort --check-only .`)
- [ ] No unused imports or variables
- [ ] No `print()` statements in production code (use `logging`)
- [ ] No `TODO` or `FIXME` without a linked issue/ticket

## Project Structure

- [ ] `pyproject.toml` is the single source of project metadata
- [ ] `__init__.py` files exist in all package directories
- [ ] `src/` layout is used (or flat layout is a deliberate choice)
- [ ] `tests/` directory mirrors the source structure
- [ ] No configuration in `setup.py` / `setup.cfg` / `tox.ini` (migrated to `pyproject.toml`)

## Dependencies

- [ ] Direct dependencies are declared with version constraints in `pyproject.toml`
- [ ] Lock file exists and is up to date (`poetry.lock`, `uv.lock`, or `requirements.txt` from pip-compile)
- [ ] No pinned transitive dependencies in manual `requirements.txt`
- [ ] Dev dependencies are separated (`[project.optional-dependencies] dev = [...]`)
- [ ] Vulnerable dependencies are checked: `pip-audit` or `safety check`

## Logging

- [ ] `logging.getLogger(__name__)` is used (not root logger)
- [ ] Log levels are appropriate (DEBUG for details, INFO for operations, ERROR for failures)
- [ ] No `print()` used for operational logging
- [ ] Structured logging configured for production (JSON format)
- [ ] Sensitive data (passwords, tokens) is not logged

## Async Code (if applicable)

- [ ] `asyncio.run()` is used at the entry point only
- [ ] No blocking sync calls inside `async def` functions
- [ ] `asyncio.to_thread()` wraps blocking I/O when called from async context
- [ ] `async with` is used for async context managers (HTTP clients, DB connections)
- [ ] Concurrency is bounded with `asyncio.Semaphore` for fan-out patterns

## Packaging (if applicable)

- [ ] `build-system` is specified in `pyproject.toml`
- [ ] Package builds cleanly: `python -m build`
- [ ] Entry points (`[project.scripts]`) are defined for CLI tools
- [ ] `LICENSE` file exists at project root
- [ ] Version is defined in `pyproject.toml` or `__version__` in `__init__.py`

## Pre-merge Smoke Test

```bash
# Full quality pipeline
source .venv/bin/activate
mypy src/                           # type check
ruff check .                        # lint
ruff format --check .               # format check
pytest tests/ -v --cov=src --tb=short  # test with coverage
python -m build                     # verify package builds (if applicable)
```
