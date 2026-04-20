---
name: postgresql
description: >-
  Guides PostgreSQL schema design, index selection (B-tree/GIN/GiST), query optimization
  with EXPLAIN ANALYZE, connection pooling via pgbouncer, migration workflows (alembic/flyway/dbmate),
  transaction isolation levels, VACUUM tuning, table partitioning, and pg_stat_statements analysis.
  Use when writing schemas, optimizing queries, configuring connections, or planning migrations
  against PostgreSQL.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: postgresql
  maturity: draft
  risk: low
  tags: [postgresql, sql, database, indexing, migrations]
---

# Purpose

Provide concrete, actionable guidance for PostgreSQL schema design, query optimization,
connection management, migration workflows, and operational tuning. This skill encodes
production-tested patterns so agents produce correct DDL, efficient queries, and safe
deployment procedures instead of guessing at PostgreSQL internals.

# When to use this skill

Use this skill when:

- designing or modifying PostgreSQL schemas (tables, indexes, constraints, partitions)
- writing or optimizing SQL queries against PostgreSQL (CTEs, window functions, EXPLAIN plans)
- configuring connection pooling (pgbouncer, PgPool-II) or diagnosing connection exhaustion
- creating or reviewing database migrations (alembic, flyway, dbmate, or raw SQL migrations)
- tuning autovacuum, analyzing bloat, or investigating lock contention
- working with PostgreSQL-specific features: advisory locks, pg_stat_statements, LISTEN/NOTIFY, logical replication

# Do not use this skill when

- the database is SQLite, MySQL, or another RDBMS — use the appropriate skill instead
- the task is purely ORM-level (model definitions, relationship loading) — use `orm-patterns`
- the task is BigQuery or analytical warehouse work — use `bigquery`
- the work is application-layer API design with no direct SQL involvement

# Operating procedure

1. **Identify the PostgreSQL version and extensions in use.** Check `SELECT version()` and `\dx` output. Version determines available features (e.g., MERGE in v15+, partitioning improvements in v12+).
2. **Design schema with normalization first, denormalize with evidence.** Start at 3NF. Only denormalize when EXPLAIN ANALYZE shows join costs justify it. Document the trade-off.
3. **Select index types deliberately:**
   - B-tree (default): equality and range queries on scalar columns
   - GIN: array containment (`@>`), full-text search (`tsvector`), JSONB path queries
   - GiST: geometric data, range types, nearest-neighbor searches
   - BRIN: large append-only tables with naturally ordered data (timestamps, sequences)
   - Hash: equality-only lookups (rare; B-tree usually wins)
4. **Write migrations as versioned, idempotent SQL files.** Each migration file gets a sequential ID. Use `CREATE INDEX CONCURRENTLY` to avoid table locks. Always include a down/rollback migration.
5. **Run EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) on every non-trivial query before shipping.** Look for sequential scans on large tables, nested loop joins on big result sets, and high buffer hit ratios that mask I/O problems.
6. **Configure connection pooling externally.** Use pgbouncer in transaction mode for web workloads. Size the pool at `(2 * CPU cores) + effective_spindle_count` for the PostgreSQL server; keep pgbouncer's `default_pool_size` at 2-3× that.
7. **Set transaction isolation deliberately.** Use READ COMMITTED (default) for most OLTP. Use REPEATABLE READ for report queries that must see a consistent snapshot. Use SERIALIZABLE only when correctness requires it, and handle serialization failures with retry logic.
8. **Monitor and tune autovacuum.** Check `pg_stat_user_tables.n_dead_tup` and `last_autovacuum`. For high-write tables, lower `autovacuum_vacuum_scale_factor` to 0.01-0.05. Watch for transaction ID wraparound via `age(datfrozenxid)`.
9. **Use advisory locks for application-level coordination.** `pg_advisory_lock(key)` for exclusive, `pg_try_advisory_lock(key)` for non-blocking. Always use `pg_advisory_unlock` or session-level locks that release on disconnect.
10. **Validate with pg_stat_statements.** Enable the extension, then query `pg_stat_statements` for top queries by `total_exec_time`, `calls`, and `mean_exec_time`. Target the top 10 queries for optimization.

# Decision rules

- **Index creation**: Never add an index without first checking if an existing index covers the query. Use `pg_stat_user_indexes` to find unused indexes for removal.
- **CTEs vs subqueries**: In PostgreSQL 12+, CTEs are inlined by default. Use `MATERIALIZED` hint only when you need optimization fences. In pre-12, prefer subqueries for performance-critical paths.
- **Partitioning threshold**: Consider range or list partitioning when a table exceeds ~50M rows or when queries consistently filter on the partition key. Prefer declarative partitioning (v10+) over inheritance.
- **JSONB vs normalized columns**: Use JSONB for truly schemaless, rarely-queried metadata. Use typed columns for anything that appears in WHERE clauses, JOINs, or aggregations.
- **Sequence vs UUID for PKs**: Use `bigserial` for internal IDs (compact, ordered, index-friendly). Use UUIDv7 (time-sortable) for distributed systems or public-facing IDs.
- **Lock strategy**: Prefer `SELECT ... FOR UPDATE SKIP LOCKED` for queue-like patterns. Avoid `LOCK TABLE` in application code.
- **Bulk operations**: Use `COPY` for bulk inserts (10-100× faster than INSERT). Use `INSERT ... ON CONFLICT` for upserts. Batch updates with CTEs returning modified rows.

# Anti-patterns

- **Missing `WHERE` on `UPDATE`/`DELETE`**: Always require a WHERE clause in DML. Use `BEGIN` + verify row count before `COMMIT`.
- **`SELECT *` in production queries**: Enumerate columns explicitly. `SELECT *` breaks when columns are added and wastes I/O on unused columns.
- **Indexes on low-cardinality columns**: An index on a boolean column is almost never useful. PostgreSQL will prefer a sequential scan.
- **Using `OFFSET` for pagination**: O(n) cost. Use keyset pagination (`WHERE id > $last_seen_id ORDER BY id LIMIT $n`) instead.
- **Long-running transactions holding locks**: Transactions open for minutes block autovacuum and cause table bloat. Set `idle_in_transaction_session_timeout`.
- **Not using `CONCURRENTLY` for index creation**: `CREATE INDEX` takes an `ACCESS EXCLUSIVE` lock. On production tables, always use `CREATE INDEX CONCURRENTLY`.
- **Ignoring `pg_stat_statements`**: Flying blind on query performance. Enable it in `shared_preload_libraries` and review weekly.
- **Storing large blobs in PostgreSQL**: Use external object storage (S3) with a URL reference. Large bytea columns bloat tables and backups.

# Output requirements

1. `Schema Design` — DDL with explicit types, constraints, and index definitions
2. `Migration Plan` — Ordered migration files with up/down, noting lock implications
3. `Query Plan Analysis` — EXPLAIN output with interpretation and optimization notes
4. `Connection Config` — pgbouncer or application pool settings with rationale
5. `Validation` — Specific queries or commands to verify the change works correctly

# References

Read these when relevant to the specific task:

- `references/implementation-patterns.md` — Index selection, pooling config, migration workflows, query optimization, partitioning
- `references/validation-checklist.md` — Pre-deploy checks, index coverage, pool sizing, lock audits, EXPLAIN review
- `references/failure-modes.md` — Connection exhaustion, deadlocks, bloat, wraparound, replication lag

# Related skills

- `orm-patterns` — When the task involves ORM model definitions or query generation layered on PostgreSQL
- `sqlite` — When comparing embedded vs server database trade-offs
- `data-model` — When the task is primarily about entity relationships and data modeling
- `observability-logging` — When adding query logging, slow query tracking, or pgbadger analysis

# Failure handling

- **Connection exhaustion**: Check `pg_stat_activity` for idle connections. Verify pgbouncer pool limits. Kill idle-in-transaction sessions with `pg_terminate_backend()`.
- **Deadlocks detected**: Read the PostgreSQL log for deadlock details. Ensure consistent lock ordering across transactions. Consider advisory locks for complex workflows.
- **Autovacuum not keeping up**: Lower per-table `autovacuum_vacuum_scale_factor`. Check if long-running transactions are blocking vacuum. Monitor `n_dead_tup` growth rate.
- **Migration failed mid-apply**: Check if the migration was wrapped in a transaction. For DDL that cannot be rolled back (e.g., `DROP COLUMN`), use a two-phase approach: deprecate first, remove later.
- **Query performance regression**: Compare EXPLAIN plans before and after. Check for missing statistics (`ANALYZE table_name`). Verify index is being used (`enable_seqscan = off` as diagnostic, never in production).
- **Transaction ID wraparound warning**: This is critical. Run `VACUUM FREEZE` on affected tables immediately. Monitor `age(datfrozenxid)` — emergency at 2 billion.
