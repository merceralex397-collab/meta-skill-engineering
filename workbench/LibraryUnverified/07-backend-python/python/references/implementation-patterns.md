# Implementation Patterns — Python

## Project Layout: src/ Layout (Recommended)

The `src/` layout prevents accidentally importing uninstalled code during development:

```
myproject/
├── src/
│   └── mypackage/
│       ├── __init__.py
│       ├── main.py
│       ├── models.py
│       ├── services/
│       │   ├── __init__.py
│       │   └── user_service.py
│       └── utils/
│           ├── __init__.py
│           └── logging.py
├── tests/
│   ├── __init__.py
│   ├── conftest.py
│   ├── test_models.py
│   └── test_user_service.py
├── pyproject.toml
├── README.md
└── .python-version
```

## Project Layout: Flat Layout (Simple Projects)

For single-package projects or scripts:

```
myproject/
├── mypackage/
│   ├── __init__.py
│   └── main.py
├── tests/
│   └── test_main.py
├── pyproject.toml
└── README.md
```

## pyproject.toml Configuration

```toml
[build-system]
requires = ["hatchling"]
build-backend = "hatchling.build"

[project]
name = "mypackage"
version = "0.1.0"
description = "A Python project"
requires-python = ">=3.11"
dependencies = [
    "pydantic>=2.0,<3.0",
    "structlog>=23.0",
]

[project.optional-dependencies]
dev = [
    "pytest>=7.0",
    "pytest-cov>=4.0",
    "mypy>=1.5",
    "ruff>=0.1",
]

[project.scripts]
mypackage = "mypackage.main:cli"

[tool.pytest.ini_options]
testpaths = ["tests"]
addopts = "-v --tb=short"

[tool.mypy]
python_version = "3.11"
strict = true
warn_return_any = true
warn_unused_configs = true

[tool.ruff]
target-version = "py311"
line-length = 88

[tool.ruff.lint]
select = ["E", "F", "I", "N", "UP", "B", "SIM", "TCH"]

[tool.ruff.lint.isort]
known-first-party = ["mypackage"]
```

## Virtual Environment Workflow

```bash
# Option A: stdlib venv
python -m venv .venv
source .venv/bin/activate  # Linux/macOS
pip install -e ".[dev]"

# Option B: poetry
poetry install
poetry shell

# Option C: uv (fast, Rust-based)
uv venv
uv pip install -e ".[dev]"
source .venv/bin/activate
```

## Type Annotation Patterns

```python
from __future__ import annotations

from dataclasses import dataclass, field
from typing import Protocol, TypeAlias

# Type aliases for clarity
UserID: TypeAlias = str
Email: TypeAlias = str

# Protocol for dependency injection
class UserRepository(Protocol):
    def get(self, user_id: UserID) -> User | None: ...
    def save(self, user: User) -> None: ...
    def list_all(self) -> list[User]: ...

# Dataclass for simple data containers
@dataclass
class User:
    id: UserID
    name: str
    email: Email
    roles: list[str] = field(default_factory=list)

# Pydantic for validation + serialization
from pydantic import BaseModel, EmailStr, field_validator

class CreateUserRequest(BaseModel):
    name: str
    email: EmailStr
    age: int

    @field_validator("age")
    @classmethod
    def age_must_be_positive(cls, v: int) -> int:
        if v < 0:
            raise ValueError("age must be non-negative")
        return v
```

## Logging Configuration

```python
# Using stdlib logging
import logging
import logging.config

LOGGING_CONFIG = {
    "version": 1,
    "disable_existing_loggers": False,
    "formatters": {
        "standard": {
            "format": "%(asctime)s [%(levelname)s] %(name)s: %(message)s"
        },
        "json": {
            "()": "pythonjsonlogger.jsonlogger.JsonFormatter",
            "format": "%(asctime)s %(levelname)s %(name)s %(message)s",
        },
    },
    "handlers": {
        "console": {
            "class": "logging.StreamHandler",
            "formatter": "standard",
            "level": "DEBUG",
        },
    },
    "loggers": {
        "mypackage": {"level": "INFO", "handlers": ["console"]},
    },
}

logging.config.dictConfig(LOGGING_CONFIG)
logger = logging.getLogger(__name__)

# Using structlog (recommended for structured logging)
import structlog

structlog.configure(
    processors=[
        structlog.contextvars.merge_contextvars,
        structlog.processors.add_log_level,
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.dev.ConsoleRenderer(),  # dev; use JSONRenderer in prod
    ],
    logger_factory=structlog.PrintLoggerFactory(),
)
log = structlog.get_logger()
log.info("user_created", user_id="abc123", email="user@example.com")
```

## Async Patterns

```python
import asyncio
from typing import AsyncIterator

async def fetch_user(client: HttpClient, user_id: str) -> User:
    response = await client.get(f"/users/{user_id}")
    return User(**response.json())

async def fetch_users_concurrent(
    client: HttpClient, user_ids: list[str]
) -> list[User]:
    """Fetch multiple users concurrently with bounded concurrency."""
    semaphore = asyncio.Semaphore(10)

    async def bounded_fetch(uid: str) -> User:
        async with semaphore:
            return await fetch_user(client, uid)

    return await asyncio.gather(*(bounded_fetch(uid) for uid in user_ids))

# Entry point
def main() -> None:
    asyncio.run(async_main())

async def async_main() -> None:
    async with HttpClient() as client:
        users = await fetch_users_concurrent(client, ["1", "2", "3"])
```

## CLI Entry Points

```python
# src/mypackage/main.py
import argparse
import sys

def cli() -> None:
    parser = argparse.ArgumentParser(description="My Package CLI")
    parser.add_argument("--verbose", "-v", action="store_true")
    subparsers = parser.add_subparsers(dest="command", required=True)

    run_parser = subparsers.add_parser("run", help="Run the service")
    run_parser.add_argument("--port", type=int, default=8000)

    args = parser.parse_args()
    if args.command == "run":
        run_server(port=args.port, verbose=args.verbose)

if __name__ == "__main__":
    cli()
```

## Pytest Fixtures and Parametrize

```python
# tests/conftest.py
import pytest
from mypackage.models import User

@pytest.fixture
def sample_user() -> User:
    return User(id="u1", name="Alice", email="alice@example.com")

@pytest.fixture
def mock_repo(sample_user: User) -> FakeUserRepository:
    repo = FakeUserRepository()
    repo.save(sample_user)
    return repo

# tests/test_user_service.py
import pytest
from mypackage.services.user_service import UserService

class TestUserService:
    def test_get_existing_user(self, mock_repo, sample_user):
        svc = UserService(repo=mock_repo)
        user = svc.get_user(sample_user.id)
        assert user is not None
        assert user.name == "Alice"

    def test_get_missing_user(self, mock_repo):
        svc = UserService(repo=mock_repo)
        assert svc.get_user("nonexistent") is None

    @pytest.mark.parametrize("email,valid", [
        ("user@example.com", True),
        ("not-an-email", False),
        ("", False),
        ("a@b.c", True),
    ])
    def test_email_validation(self, email: str, valid: bool):
        if valid:
            CreateUserRequest(name="Test", email=email, age=25)
        else:
            with pytest.raises(ValueError):
                CreateUserRequest(name="Test", email=email, age=25)
```

Related skills: `flask`, `fastapi`, `orm-patterns`, `observability-logging`.
