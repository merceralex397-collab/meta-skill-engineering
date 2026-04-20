---
name: sqlite
description: >-
  Guides SQLite configuration (WAL mode, PRAGMA tuning), concurrency patterns,
  file locking, application-level migrations, JSON1/FTS5 extensions, and backup strategies.
  Use when configuring SQLite for embedded apps, test harnesses, single-writer services,
  CLI tools, or mobile/desktop local storage.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: sqlite
  maturity: draft
  risk: low
  tags: [sqlite, embedded-database, sql, file-database]
---

# Purpose

Provide concrete guidance for configuring, querying, and operating SQLite databases
in application code. SQLite has fundamentally different concurrency, deployment, and
operational characteristics from server databases. This skill encodes those differences
so agents produce correct PRAGMA settings, safe migration strategies, and appropriate
concurrency patterns instead of treating SQLite like a lightweight PostgreSQL.

# When to use this skill

Use this skill when:

- configuring SQLite PRAGMA settings (journal_mode, synchronous, foreign_keys, busy_timeout)
- implementing concurrent access patterns for SQLite (WAL mode, reader/writer coordination)
- writing application-level migrations for SQLite (no transactional DDL for some operations)
- using SQLite extensions: JSON1 for JSON queries, FTS5 for full-text search
- building embedded databases, test fixtures, CLI tool storage, or mobile local storage
- deciding whether SQLite is appropriate vs a server database for a given workload

# Do not use this skill when

- the database is PostgreSQL, MySQL, or another server database — use `postgresql` or the appropriate skill
- the task involves connection pooling, replication, or multi-server architectures
- the workload requires concurrent writes from multiple processes (SQLite is single-writer)
- the task is about ORM-level patterns (model definitions, eager loading) — use `orm-patterns`

# Operating procedure

1. **Determine the deployment context.** Is this embedded (mobile, desktop, CLI), test harness, or single-server web app? This determines concurrency requirements and PRAGMA choices.
2. **Enable WAL mode immediately.** Unless there is a specific reason for rollback journal (e.g., read-only media), always use WAL:
   ```sql
   PRAGMA journal_mode = WAL;
   ```
3. **Set essential PRAGMAs at connection open.** These must be set per-connection, not once globally:
   ```sql
   PRAGMA journal_mode = WAL;          -- write-ahead logging
   PRAGMA synchronous = NORMAL;         -- safe with WAL, faster than FULL
   PRAGMA foreign_keys = ON;            -- off by default!
   PRAGMA busy_timeout = 5000;          -- wait 5s on lock instead of failing immediately
   PRAGMA cache_size = -64000;          -- 64MB page cache (negative = KB)
   PRAGMA wal_autocheckpoint = 1000;    -- checkpoint every 1000 pages
   ```
4. **Design for single-writer concurrency.** WAL allows concurrent reads with one writer. Structure application code so writes go through a single connection or serialized write queue. Multiple readers can use separate connections.
5. **Write migrations as numbered SQL files.** SQLite does not support transactional DDL for all operations (e.g., `ALTER TABLE` is limited). Some schema changes require the 12-step migration: create new table, copy data, drop old, rename new.
6. **Use appropriate column types.** SQLite uses dynamic typing but declare types for documentation and ORM compatibility. Use `INTEGER PRIMARY KEY` for rowid alias (auto-increment). Use `TEXT` for dates in ISO 8601 format.
7. **Leverage JSON1 for flexible metadata.** Store structured metadata as JSON in TEXT columns, query with `json_extract()`, `json_each()`, and `json_tree()`.
8. **Use FTS5 for full-text search.** Create virtual tables with `fts5` module for fast text search instead of `LIKE '%query%'` scans.
9. **Implement backup strategies.** Use `.backup` API for hot backups or `VACUUM INTO 'backup.db'` for compacted copies. Never copy the file directly while connections are open.
10. **Verify with integrity checks.** Run `PRAGMA integrity_check` after migrations and periodically in production.

# Decision rules

- **WAL vs rollback journal**: Use WAL unless the database is on read-only media, shared via NFS (WAL requires shared memory), or you need strict serialization of all access.
- **synchronous = NORMAL vs FULL**: NORMAL is safe with WAL mode (transactions survive process crash but not OS crash). Use FULL only if you cannot tolerate any data loss on power failure.
- **SQLite vs server database**: Use SQLite when the workload is single-writer, data fits on one machine, there is no need for concurrent write scaling, and deployment simplicity matters. Switch to PostgreSQL when you need concurrent writes from multiple processes, replication, or row-level locking.
- **JSON column vs separate table**: Use JSON1 for semi-structured metadata queried occasionally. Use normalized tables for data that appears in WHERE clauses, JOINs, or needs indexing.
- **FTS5 vs LIKE**: Use FTS5 for any text search on more than a few thousand rows. `LIKE '%term%'` forces full table scan and cannot use indexes.
- **In-memory vs file-backed**: Use `:memory:` or `file::memory:` for test fixtures and ephemeral data. Use file-backed for anything that must survive process restart.

# Anti-patterns

- **Not setting `foreign_keys = ON`**: SQLite disables foreign key enforcement by default. Every connection must enable it explicitly or FK constraints are silently ignored.
- **Opening multiple write connections**: SQLite allows only one writer at a time. Multiple write connections cause `SQLITE_BUSY` errors. Use a single write connection with a queue or mutex.
- **Using `PRAGMA journal_mode` in a transaction**: Journal mode changes must happen outside any transaction. Setting it inside `BEGIN...COMMIT` is silently ignored.
- **Copying the database file while in use**: Without using the backup API, this can produce a corrupt copy. Use `.backup`, `VACUUM INTO`, or `sqlite3_backup_init()`.
- **`ALTER TABLE` assumptions from PostgreSQL**: SQLite's `ALTER TABLE` only supports `RENAME TABLE`, `RENAME COLUMN`, `ADD COLUMN`, and `DROP COLUMN` (3.35+). No `ALTER COLUMN`, no `ADD CONSTRAINT`.
- **Not setting `busy_timeout`**: Without it, any lock contention immediately returns `SQLITE_BUSY`. Set at least 1000-5000ms.
- **Storing large blobs**: SQLite page size defaults to 4KB. Large blobs fragment across many pages and slow down reads. Keep blobs under 100KB or use external files with path references.
- **Using `AUTOINCREMENT` unnecessarily**: `INTEGER PRIMARY KEY` already auto-increments via rowid. Adding `AUTOINCREMENT` prevents reuse of deleted rowids but adds overhead and a separate tracking table.

# Output requirements

1. `PRAGMA Configuration` — Exact PRAGMA statements with rationale for each setting
2. `Schema DDL` — CREATE TABLE statements with appropriate types and constraints
3. `Migration Plan` — Numbered migration files, noting SQLite-specific limitations
4. `Concurrency Model` — How readers and writers are coordinated in the application
5. `Validation` — PRAGMA checks and integrity verification commands

# References

Read these when relevant to the specific task:

- `references/implementation-patterns.md` — WAL setup, connection management, migrations, JSON1/FTS5, backups
- `references/validation-checklist.md` — PRAGMA verification, FK enforcement, WAL checkpoints, integrity checks
- `references/failure-modes.md` — SQLITE_BUSY, locked database, corrupt journal, WAL growth, disk full

# Related skills

- `postgresql` — When the workload outgrows SQLite's single-writer model
- `orm-patterns` — When using SQLAlchemy, Django ORM, or Prisma with SQLite as backend
- `data-model` — When designing entity relationships that will be stored in SQLite
- `background-jobs-queues` — When using SQLite as a job queue backend

# Failure handling

- **SQLITE_BUSY (error 5)**: Another connection holds a write lock. Verify `busy_timeout` is set. Check for long-running write transactions. Ensure single-writer pattern is enforced.
- **Database locked**: A process has an exclusive lock (during checkpoint or VACUUM). Wait and retry. Check if WAL checkpoint is stuck.
- **Corrupt database**: Run `PRAGMA integrity_check`. If corruption is confirmed, restore from backup. Investigate cause: disk failure, incomplete write, NFS usage.
- **WAL file growing unbounded**: Checkpointing is not running. Check `wal_autocheckpoint` setting. Run `PRAGMA wal_checkpoint(TRUNCATE)` manually. Verify no long-running read transactions blocking checkpoint.
- **Migration failed**: SQLite has limited `ALTER TABLE`. If a migration requires column type change or constraint addition, use the 12-step migration pattern (create new table → copy data → drop old → rename).
- **Disk full during write**: SQLite will return `SQLITE_FULL`. The transaction is rolled back. Free disk space and retry. Check WAL file size — it may be consuming unexpected space.