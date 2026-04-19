---
name: orm-patterns
description: >-
  Write correct SQLAlchemy 2.0 queries, avoid N+1 problems, and use eager
  loading strategies. Use when writing SQLAlchemy models or queries, fixing
  N+1 query bugs, choosing between joinedload/selectinload/subqueryload,
  or migrating from SQLAlchemy 1.x to 2.0 style. Do not use for BigQuery
  analytics (prefer bigquery) or raw SQL without an ORM.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: orm-patterns
  maturity: draft
  risk: low
  tags: [orm, sqlalchemy, database]
---

# Purpose

Write correct SQLAlchemy 2.0 queries with proper relationship loading, N+1 prevention, and session management.

# When to use this skill

- writing SQLAlchemy 2.0 models with `mapped_column` and `Mapped` types
- fixing N+1 query problems with eager loading strategies
- choosing between `joinedload`, `selectinload`, and `subqueryload`
- migrating from SQLAlchemy 1.x to 2.0 style

# Do not use this skill when

- working with BigQuery — prefer `bigquery`
- writing raw SQL without an ORM
- using a non-Python ORM (Prisma, TypeORM, GORM)

# Procedure

1. **Define models with 2.0 syntax** — use `Mapped[type]`, `mapped_column()`, and `relationship()` with type annotations.
2. **Detect N+1 queries** — enable `echo=True` on engine or use SQLAlchemy event listeners to count queries per request.
3. **Choose loading strategy** — `joinedload` for one-to-one/many-to-one; `selectinload` for one-to-many; `subqueryload` for deep nesting.
4. **Use `select()` over `query()`** — 2.0 style: `session.execute(select(User).where(...))` not `session.query(User).filter(...)`.
5. **Manage sessions** — use `async_sessionmaker` or `sessionmaker` with context manager; never share sessions across threads.
6. **Add indexes** — annotate columns with `index=True`; create composite indexes via `__table_args__`.
7. **Write migrations** — use Alembic `--autogenerate` to detect model changes; always review generated SQL.
8. **Test queries** — assert query count in tests using `connection.execute(text("SELECT ..."))` event counters.

# Model example (2.0 style)

```python
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship
from sqlalchemy import ForeignKey, String
from datetime import datetime

class Base(DeclarativeBase):
    pass

class User(Base):
    __tablename__ = "users"
    id: Mapped[int] = mapped_column(primary_key=True)
    email: Mapped[str] = mapped_column(String(255), unique=True, index=True)
    created_at: Mapped[datetime] = mapped_column(default=datetime.utcnow)
    posts: Mapped[list["Post"]] = relationship(back_populates="author")

class Post(Base):
    __tablename__ = "posts"
    id: Mapped[int] = mapped_column(primary_key=True)
    title: Mapped[str] = mapped_column(String(500))
    author_id: Mapped[int] = mapped_column(ForeignKey("users.id"), index=True)
    author: Mapped["User"] = relationship(back_populates="posts")
```

# Loading strategies

```python
from sqlalchemy.orm import joinedload, selectinload
from sqlalchemy import select

# One-to-many: use selectinload (2 queries, no cartesian product)
stmt = select(User).options(selectinload(User.posts)).where(User.id == 1)

# Many-to-one: use joinedload (single JOIN)
stmt = select(Post).options(joinedload(Post.author)).limit(20)

# Nested: chain strategies
stmt = select(User).options(
    selectinload(User.posts).joinedload(Post.tags)
)
```

# Decision rules

- Default to `selectinload` for collections — avoids cartesian product explosion.
- Use `joinedload` only for single-object relationships (many-to-one, one-to-one).
- Never access lazy-loaded relationships outside an active session — use eager loading or `expire_on_commit=False`.
- Use `Mapped[type]` for all columns — it provides type checking and IDE support.
- Run `alembic check` in CI to ensure models and migrations stay in sync.

# References

- https://docs.sqlalchemy.org/en/20/orm/quickstart.html
- https://docs.sqlalchemy.org/en/20/orm/queryguide/relationships.html
- https://alembic.sqlalchemy.org/

# Related skills

- `bigquery` — analytics SQL patterns
