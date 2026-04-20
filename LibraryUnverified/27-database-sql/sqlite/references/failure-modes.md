# Failure Modes

SQLite-specific failure scenarios with detection, impact, and resolution.

## SQLITE_BUSY errors (error code 5)

**Symptoms**: Application receives "database is locked" errors, writes fail intermittently,
concurrent operations timeout.

**Detection**:
```python
try:
    conn.execute("INSERT INTO ...")
except sqlite3.OperationalError as e:
    if "database is locked" in str(e):
        # SQLITE_BUSY — another connection holds a lock
```

**Root causes**:
- `busy_timeout` not set (default is 0 — fail immediately)
- Multiple processes writing simultaneously (SQLite is single-writer)
- Long-running write transaction blocking other writers
- WAL checkpoint blocked by a long-running reader

**Resolution**:
1. Set `PRAGMA busy_timeout = 5000` on every connection
2. Enforce single-writer pattern in application architecture
3. Keep write transactions as short as possible
4. Implement application-level retry with backoff for transient BUSY errors

## Database locked (extended)

**Symptoms**: Even reads fail with "database is locked". All connections blocked.

**Detection**:
```bash
# Check for processes holding locks:
fuser mydb.db mydb.db-wal mydb.db-shm

# Check if WAL checkpoint is stuck:
sqlite3 mydb.db "PRAGMA wal_checkpoint;"
```

**Root causes**:
- A process crashed while holding an exclusive lock
- `VACUUM` or `PRAGMA wal_checkpoint(RESTART)` running (takes exclusive lock)
- File-level lock stuck due to NFS or network filesystem issues
- Stale `-wal` or `-shm` file from crashed process

**Resolution**:
1. Identify and terminate the blocking process
2. If no process holds the lock, delete `-shm` and `-wal` files (data is safe if WAL was checkpointed)
3. Do not use SQLite on NFS or network filesystems (WAL requires shared memory)
4. Run `PRAGMA integrity_check` after recovery

## Corrupt journal file

**Symptoms**: "database disk image is malformed" errors, integrity_check reports errors,
queries return wrong data.

**Detection**:
```sql
PRAGMA integrity_check;
-- Returns specific errors if corruption exists
-- "ok" if database is clean
```

**Root causes**:
- Power loss during write without `synchronous = FULL`
- Disk corruption or bad sectors
- File copied while database was in use (without backup API)
- NFS or network filesystem with broken locking

**Resolution**:
1. If using WAL mode, try opening the database — SQLite will attempt WAL recovery automatically
2. If recovery fails, restore from last known good backup
3. Try `.recover` command in sqlite3 CLI to salvage data from corrupt database:
```bash
sqlite3 corrupt.db ".recover" | sqlite3 recovered.db
```
4. Investigate root cause — check disk health, filesystem type, backup procedures

## WAL file growth

**Symptoms**: `-wal` file grows to hundreds of MB or GB, disk space consumed,
performance degradation.

**Detection**:
```bash
ls -lh mydb.db mydb.db-wal
# WAL file should normally be < 10-50MB
```

```sql
PRAGMA wal_checkpoint;
-- Check if log_pages is growing without being checkpointed
```

**Root causes**:
- Long-running read transaction preventing WAL checkpoint (readers block truncation)
- `wal_autocheckpoint` disabled or set too high
- Application never closes read connections/transactions
- Checkpoint running but WAL still has active readers

**Resolution**:
1. Close long-running read transactions (use short-lived connections for reads)
2. Set `PRAGMA wal_autocheckpoint = 1000` (default — checkpoint every 1000 pages)
3. Run manual checkpoint after bulk writes: `PRAGMA wal_checkpoint(TRUNCATE)`
4. Monitor WAL file size and alert if it exceeds threshold

## Concurrent write conflicts

**Symptoms**: Multiple processes or threads competing for write access, frequent
SQLITE_BUSY errors even with busy_timeout set.

**Detection**: High rate of SQLITE_BUSY errors in application logs. Multiple processes
with open write connections visible via `fuser` or `lsof`.

**Root causes**:
- Architecture mistake: SQLite used where a server database is needed
- Multiple worker processes each trying to write independently
- No write serialization in application layer

**Resolution**:
1. Enforce single-writer at application level (write queue, mutex, or dedicated writer process)
2. If multiple processes must write, evaluate migrating to PostgreSQL
3. Use WAL mode to at least allow reads concurrent with the single writer
4. Consider WAL2 mode (experimental) if available in your SQLite version

## Disk full handling

**Symptoms**: `SQLITE_FULL` error (code 13), writes fail, transactions roll back.

**Detection**:
```python
try:
    conn.execute("INSERT INTO ...")
except sqlite3.OperationalError as e:
    if "database or disk is full" in str(e):
        # SQLITE_FULL — disk space exhausted
```

**Root causes**:
- Disk partition full
- WAL file consuming unexpected disk space
- `VACUUM` requires temporary space equal to the database size
- Temporary files in `/tmp` filling up (for large sorts/joins)

**Resolution**:
1. Free disk space immediately
2. Check WAL file size — run `PRAGMA wal_checkpoint(TRUNCATE)` to reclaim
3. Set `PRAGMA temp_store_directory` to a partition with more space (deprecated but functional)
4. Run `VACUUM` to reclaim space from deleted rows (requires 2× database size in free space)
5. Monitor disk usage with alerts at 80% capacity

## Data loss from missing foreign_keys pragma

**Symptoms**: Orphaned rows accumulate, referential integrity silently violated,
application logic breaks due to missing parent records.

**Detection**:
```sql
PRAGMA foreign_keys;
-- If returns 0: foreign keys are NOT enforced

PRAGMA foreign_key_check;
-- Lists all FK violations: (table, rowid, parent, fkid)
```

**Root causes**:
- `PRAGMA foreign_keys = ON` not set on every connection
- Foreign keys off by default in SQLite — unlike PostgreSQL
- ORM or connection library not running initialization PRAGMAs

**Resolution**:
1. Add `PRAGMA foreign_keys = ON` to connection initialization code
2. Run `PRAGMA foreign_key_check` to find existing violations
3. Fix orphaned rows: delete orphans or recreate missing parents
4. Add integration tests that verify FK enforcement is active
