# Failure Modes

PostgreSQL-specific failure scenarios with detection, impact, and resolution.

## Connection exhaustion

**Symptoms**: Application errors "too many connections", new connections rejected,
pgbouncer queue growing.

**Detection**:
```sql
SELECT count(*), state FROM pg_stat_activity GROUP BY state;
SELECT count(*) FROM pg_stat_activity;  -- compare to max_connections
SHOW max_connections;
```

**Root causes**:
- Application not closing connections (connection leak)
- Missing or misconfigured connection pooler
- Long-running transactions holding connections
- `idle in transaction` sessions accumulating

**Resolution**:
1. Immediately terminate idle-in-transaction sessions older than threshold:
```sql
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE state = 'idle in transaction'
  AND xact_start < now() - interval '5 minutes';
```
2. Deploy pgbouncer if not present; switch to transaction-mode pooling
3. Set `idle_in_transaction_session_timeout` at the database level
4. Audit application code for connection leaks (missing `close()` or context manager)

## Deadlocks

**Symptoms**: `ERROR: deadlock detected` in logs, occasional query failures,
intermittent 500 errors.

**Detection**:
```sql
-- Check deadlock count
SELECT deadlocks FROM pg_stat_database WHERE datname = current_database();

-- PostgreSQL log shows full deadlock detail including queries and lock types
```

**Root causes**:
- Transactions acquiring locks in inconsistent order
- `SELECT FOR UPDATE` on overlapping row sets from concurrent transactions
- Foreign key checks triggering implicit locks on parent tables

**Resolution**:
1. Establish consistent lock ordering (e.g., always lock parent before child)
2. Keep transactions short — batch processing in smaller chunks
3. Use `SKIP LOCKED` for queue-worker patterns
4. Consider advisory locks for complex multi-table updates

## Long-running transactions

**Symptoms**: Table bloat increasing, autovacuum unable to clean dead tuples,
`n_dead_tup` growing, `xact_start` showing transactions open for hours.

**Detection**:
```sql
SELECT pid, now() - xact_start AS age, state, query
FROM pg_stat_activity
WHERE xact_start IS NOT NULL
ORDER BY age DESC;
```

**Impact**: Prevents autovacuum from freezing old tuples, blocks DDL operations,
increases table and index bloat, eventually causes performance degradation.

**Resolution**:
1. Set `idle_in_transaction_session_timeout = '60s'` for web application databases
2. Set `statement_timeout = '30s'` as a safety net for runaway queries
3. Monitor with alerting on transactions open longer than 5 minutes
4. For analytical queries, use a read replica with `hot_standby_feedback = on`

## Table bloat from missed VACUUM

**Symptoms**: Table size growing disproportionately to row count, slow sequential
scans, high disk usage.

**Detection**:
```sql
-- Estimate bloat using pgstattuple extension
CREATE EXTENSION IF NOT EXISTS pgstattuple;
SELECT * FROM pgstattuple('my_table');

-- Quick check: compare relation size to expected size
SELECT pg_size_pretty(pg_total_relation_size('my_table')) AS total_size,
       n_live_tup,
       pg_size_pretty(pg_total_relation_size('my_table')::numeric / greatest(n_live_tup, 1)) AS bytes_per_row
FROM pg_stat_user_tables
WHERE relname = 'my_table';
```

**Resolution**:
1. Run `VACUUM FULL my_table` (takes `ACCESS EXCLUSIVE` lock — schedule during maintenance)
2. For zero-downtime: use `pg_repack` extension to rebuild table without exclusive lock
3. Fix the root cause: autovacuum tuning, long transaction prevention
4. Monitor `n_dead_tup` / `n_live_tup` ratio — alert when > 20%

## Transaction ID (XID) wraparound

**Symptoms**: PostgreSQL log warnings about approaching XID wraparound,
database refuses writes with "database is not accepting commands to avoid wraparound".

**Detection**:
```sql
SELECT datname, age(datfrozenxid) AS xid_age,
       current_setting('autovacuum_freeze_max_age')::bigint AS freeze_max
FROM pg_database
ORDER BY age(datfrozenxid) DESC;
-- CRITICAL when xid_age > 1.2 billion
```

**Impact**: If `age(datfrozenxid)` reaches 2 billion, PostgreSQL shuts down writes
to prevent data corruption. This is a database emergency.

**Resolution**:
1. Run `VACUUM FREEZE` on the affected database immediately
2. If autovacuum is stuck, check for long-running transactions and terminate them
3. Increase `autovacuum_freeze_max_age` if freezing is happening too late
4. Monitor this metric with alerting — threshold at 500 million

## Sequence exhaustion

**Symptoms**: `ERROR: nextval: reached maximum value of sequence`, inserts fail.

**Detection**:
```sql
SELECT sequencename, last_value, max_value,
       (max_value - last_value) AS remaining
FROM pg_sequences
WHERE max_value - last_value < 1000000;
```

**Resolution**:
1. `ALTER SEQUENCE my_seq MAXVALUE 9223372036854775807` (upgrade to bigint range)
2. For tables using `serial` (int4), migrate to `bigserial`:
   - Create new bigint sequence
   - Alter column type: `ALTER TABLE t ALTER COLUMN id TYPE bigint`
   - Reassign default to new sequence
3. Prevent: always use `bigserial` or `bigint` with `GENERATED ALWAYS AS IDENTITY`

## Replication lag

**Symptoms**: Read replicas returning stale data, `pg_stat_replication` showing
growing `replay_lag`.

**Detection**:
```sql
-- On primary:
SELECT client_addr, state, sent_lsn, write_lsn, flush_lsn, replay_lsn,
       now() - replay_lag AS replay_lag
FROM pg_stat_replication;

-- On replica:
SELECT now() - pg_last_xact_replay_timestamp() AS replication_delay;
```

**Root causes**:
- Replica under-provisioned (CPU, I/O)
- Network latency between primary and replica
- Long-running queries on replica blocking replay
- WAL generation rate exceeding replay capacity

**Resolution**:
1. Check replica resource utilization (CPU, disk I/O, network)
2. Set `max_standby_streaming_delay` to cancel conflicting queries sooner
3. Use `hot_standby_feedback = on` if replica queries are being cancelled
4. For high-throughput: increase `wal_buffers`, use async replication if lag tolerance permits
5. Consider multiple replicas to distribute read load
