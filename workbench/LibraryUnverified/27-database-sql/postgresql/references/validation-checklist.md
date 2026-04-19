# Validation Checklist

Pre-deploy and ongoing validation checks for PostgreSQL changes.

## Pre-deploy migration checklist

- [ ] Migration file has a unique sequential version number
- [ ] `CREATE INDEX` uses `CONCURRENTLY` on any table with existing data
- [ ] Column additions are `NULL` initially (backfill + `SET NOT NULL` as separate step)
- [ ] No `ALTER TABLE ... ADD COLUMN ... DEFAULT expr` on PG < 11 (rewrites entire table)
- [ ] Down/rollback migration exists and has been tested
- [ ] Migration runs cleanly on a copy of production schema (`pg_dump --schema-only` + apply)
- [ ] `DROP` operations are preceded by a deprecation period in a prior release
- [ ] Foreign key additions use `NOT VALID` + `VALIDATE CONSTRAINT` to avoid full-table lock
- [ ] Estimated lock duration for each DDL statement documented

```sql
-- Adding a FK without blocking reads/writes:
ALTER TABLE orders ADD CONSTRAINT fk_orders_user
    FOREIGN KEY (user_id) REFERENCES users(id) NOT VALID;
-- In a later migration, validate (takes ShareUpdateExclusiveLock, not AccessExclusive):
ALTER TABLE orders VALIDATE CONSTRAINT fk_orders_user;
```

## Index coverage verification

- [ ] Every foreign key column has a corresponding index
- [ ] Query patterns from `pg_stat_statements` (top 20 by `total_exec_time`) have supporting indexes
- [ ] No duplicate indexes (same columns in same order):

```sql
SELECT indrelid::regclass AS table_name,
       array_agg(indexrelid::regclass) AS indexes,
       indkey
FROM pg_index
GROUP BY indrelid, indkey
HAVING count(*) > 1;
```

- [ ] Unused indexes identified and scheduled for removal:

```sql
SELECT schemaname, relname AS table, indexrelname AS index,
       idx_scan, pg_size_pretty(pg_relation_size(indexrelid)) AS size
FROM pg_stat_user_indexes
WHERE idx_scan = 0
ORDER BY pg_relation_size(indexrelid) DESC;
```

- [ ] Partial indexes considered for columns with skewed value distributions
- [ ] Index-only scans verified where applicable (check `Heap Fetches` in EXPLAIN)

## Connection pool sizing

- [ ] pgbouncer `default_pool_size` does not exceed PostgreSQL `max_connections`
- [ ] Application pool `max_size` does not exceed pgbouncer `max_client_conn`
- [ ] `pool_mode = transaction` for web/API workloads (not `session` mode)
- [ ] `server_idle_timeout` set to reclaim unused server connections
- [ ] No prepared statements used when pgbouncer is in transaction mode (incompatible)
- [ ] Connection count verified under load:

```sql
SELECT state, count(*)
FROM pg_stat_activity
WHERE datname = 'myapp'
GROUP BY state;
```

- [ ] `idle in transaction` connections have timeout set:

```sql
ALTER DATABASE myapp SET idle_in_transaction_session_timeout = '30s';
```

## Lock contention checks

- [ ] No `LOCK TABLE` in application code (use row-level locks instead)
- [ ] `SELECT ... FOR UPDATE` uses `SKIP LOCKED` or `NOWAIT` where appropriate
- [ ] Advisory locks have matching unlock calls or use session-scoped locks
- [ ] Migration DDL lock durations estimated:

```sql
-- Check current locks before deploying:
SELECT pid, mode, granted, relation::regclass, query
FROM pg_locks l
JOIN pg_stat_activity a ON l.pid = a.pid
WHERE NOT granted;
```

- [ ] Long-running transactions monitored:

```sql
SELECT pid, now() - xact_start AS duration, state, query
FROM pg_stat_activity
WHERE state != 'idle'
ORDER BY duration DESC
LIMIT 10;
```

## EXPLAIN plan review

- [ ] No sequential scans on tables with > 10k rows (unless intended for full-table operations)
- [ ] Join strategies appropriate: hash join for large sets, nested loop for small indexed lookups
- [ ] `Buffers: shared read` count is low relative to `shared hit` (cache hit ratio > 99%)
- [ ] No `Sort Method: external merge` (indicates `work_mem` too low for the sort)
- [ ] Estimated rows close to actual rows (stale statistics if 10× off → run `ANALYZE`)
- [ ] No implicit casts causing index bypass (e.g., comparing `int` column to `text` parameter)

```sql
-- Check overall cache hit ratio:
SELECT
    sum(heap_blks_hit) / (sum(heap_blks_hit) + sum(heap_blks_read)) AS cache_hit_ratio
FROM pg_statio_user_tables;
-- Target: > 0.99
```

## Autovacuum health checks

- [ ] No tables with `n_dead_tup` > 10% of `n_live_tup`:

```sql
SELECT relname, n_live_tup, n_dead_tup,
       round(n_dead_tup::numeric / greatest(n_live_tup, 1) * 100, 1) AS dead_pct,
       last_autovacuum, last_autoanalyze
FROM pg_stat_user_tables
WHERE n_dead_tup > 1000
ORDER BY n_dead_tup DESC;
```

- [ ] `age(datfrozenxid)` < 500 million for all databases (wraparound threshold is 2 billion)
- [ ] High-write tables have per-table autovacuum tuning:

```sql
ALTER TABLE high_write_table SET (
    autovacuum_vacuum_scale_factor = 0.02,
    autovacuum_analyze_scale_factor = 0.01
);
```

- [ ] `autovacuum_max_workers` appropriate for workload (default 3; increase for many tables)
