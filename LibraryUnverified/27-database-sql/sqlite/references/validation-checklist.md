# Validation Checklist

SQLite-specific validation checks for configuration, migrations, and operations.

## PRAGMA verification

Run these checks after opening any connection to verify configuration:

- [ ] Journal mode is WAL:
```sql
PRAGMA journal_mode;
-- Expected: "wal"
```

- [ ] Foreign keys are enforced:
```sql
PRAGMA foreign_keys;
-- Expected: 1
```

- [ ] Synchronous level is appropriate:
```sql
PRAGMA synchronous;
-- Expected: 1 (NORMAL) for WAL mode, 2 (FULL) for rollback journal
```

- [ ] Busy timeout is set:
```sql
PRAGMA busy_timeout;
-- Expected: > 0 (typically 1000-5000)
```

- [ ] Cache size is configured:
```sql
PRAGMA cache_size;
-- Expected: negative value in KB (e.g., -64000 for 64MB)
```

## Foreign key enforcement check

- [ ] Foreign keys enabled on EVERY connection (PRAGMAs are per-connection, not per-database)
- [ ] Existing data passes FK validation:
```sql
PRAGMA foreign_key_check;
-- Should return empty result set
-- If rows returned: (table, rowid, parent_table, fkid) = violations
```

- [ ] FK constraints verified after migrations that alter table structure
- [ ] ON DELETE / ON UPDATE actions tested for all foreign keys
- [ ] `PRAGMA defer_foreign_keys = ON` used only inside transactions that temporarily violate FK order

## WAL checkpoint monitoring

- [ ] Checkpoint status checked periodically:
```sql
PRAGMA wal_checkpoint;
-- Returns: (busy, log_pages, checkpointed_pages)
-- busy = 1 means checkpoint could not complete (reader blocking)
-- log_pages should decrease after checkpoint
```

- [ ] WAL file size monitored (should not grow unbounded):
```bash
ls -la mydb.db-wal
# If WAL file > 100MB, checkpointing may be blocked
```

- [ ] Auto-checkpoint threshold configured:
```sql
PRAGMA wal_autocheckpoint;
-- Default: 1000 pages. Lower for less WAL growth, higher for better write throughput
```

- [ ] Manual checkpoint after bulk operations:
```sql
PRAGMA wal_checkpoint(TRUNCATE);
-- TRUNCATE mode resets WAL file to zero size
```

## busy_timeout configuration

- [ ] busy_timeout set to at least 1000ms for applications with any concurrency
- [ ] Timeout value appropriate for workload:
  - CLI tools: 1000-3000ms
  - Web apps: 3000-5000ms
  - Background jobs: 5000-30000ms
- [ ] Error handling for SQLITE_BUSY after timeout expiry
- [ ] No code paths that open connections without setting busy_timeout

## integrity_check

- [ ] Run after every migration:
```sql
PRAGMA integrity_check;
-- Expected: "ok"
-- Any other output indicates corruption
```

- [ ] Periodic integrity check in production (weekly or after unexpected shutdown):
```sql
PRAGMA quick_check;
-- Faster than full integrity_check, catches most corruption
```

- [ ] Foreign key integrity verified separately:
```sql
PRAGMA foreign_key_check;
```

## Schema validation

- [ ] All tables have explicit column types (even though SQLite uses dynamic typing)
- [ ] `INTEGER PRIMARY KEY` used for auto-increment rowid aliases (not `INT PRIMARY KEY`)
- [ ] `TEXT` used for dates in ISO 8601 format (SQLite has no native date type)
- [ ] `NOT NULL` constraints on columns that must not be null
- [ ] Indexes exist for all columns used in WHERE, JOIN, and ORDER BY clauses:
```sql
-- List all indexes:
SELECT name, tbl_name, sql FROM sqlite_master WHERE type = 'index';
```

## Migration validation

- [ ] Migration tracking table exists and is populated
- [ ] Each migration file is idempotent or has guard checks
- [ ] 12-step ALTER TABLE pattern used for unsupported operations
- [ ] `PRAGMA integrity_check` passes after migration
- [ ] `PRAGMA foreign_key_check` passes after migration
- [ ] Data counts verified before and after data-moving migrations
- [ ] Indexes recreated after table rebuild migrations
