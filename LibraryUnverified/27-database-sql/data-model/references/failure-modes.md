# Failure Modes

Specific data model failure modes to watch for. Each entry describes the symptom, root cause, and mitigation.

---

## 1. Orphan records from missing cascade behavior

**Symptom:** Child records exist with FK values that reference no parent. Queries return unexpected results or NULLs on JOIN.

**Root cause:** FK was created without `ON DELETE CASCADE` or `ON DELETE RESTRICT`. Parent was deleted, and the database silently set the FK to NULL or did nothing (depending on dialect defaults).

**Mitigation:** Always specify `ON DELETE` explicitly. Use `ON DELETE RESTRICT` to prevent accidental parent deletion. Use `ON DELETE CASCADE` only when children are meaningless without the parent. Audit existing FKs with:

```sql
-- Postgres: find FKs without explicit ON DELETE
SELECT conname, confdeltype FROM pg_constraint WHERE contype = 'f';
-- confdeltype: a=NO ACTION, r=RESTRICT, c=CASCADE, n=SET NULL, d=SET DEFAULT
```

---

## 2. Premature denormalization causing update anomalies

**Symptom:** Changing a value (e.g., a product name) requires updating it in 5+ tables. Some tables get updated, others don't. Users see stale data in some views.

**Root cause:** The same attribute was stored in multiple tables for "read performance" without a synchronization mechanism. No single source of truth.

**Mitigation:** Normalize first. When denormalization is justified by measured performance needs, use one of: materialized views (database manages sync), CDC/event-driven sync (application manages sync), or a single computed column. Document the denormalization in a schema comment.

---

## 3. UUID v4 B-tree index fragmentation

**Symptom:** Insert throughput degrades over time. Index size grows disproportionately to row count. `pg_stat_user_indexes` shows high `idx_scan` cost.

**Root cause:** UUID v4 is fully random. Every insert goes to a random leaf page in the B-tree, causing page splits and poor cache locality.

**Mitigation:** Switch to UUID v7 (RFC 9562) or ULID. Both have a time-sortable prefix that ensures sequential inserts cluster together. For existing UUID v4 columns, consider `REINDEX CONCURRENTLY` periodically, or accept the trade-off if the table is small.

---

## 4. Schema migration deadlocking concurrent traffic

**Symptom:** `ALTER TABLE` hangs, application queries time out, and workers report connection pool exhaustion.

**Root cause:** The migration acquired an `ACCESS EXCLUSIVE` lock on a high-traffic table. In Postgres, `ALTER TABLE ... ADD COLUMN ... DEFAULT` (prior to v11) or `ALTER TABLE ... ADD CONSTRAINT ... NOT VALID` without `VALIDATE CONSTRAINT` separately blocks reads.

**Mitigation:**
- Add nullable columns without defaults first (instant in Postgres 11+).
- Use `CREATE INDEX CONCURRENTLY` instead of `CREATE INDEX`.
- Set `lock_timeout` in the migration so it fails fast rather than blocking: `SET lock_timeout = '5s';`.
- For `NOT NULL` constraints on existing columns, add as `CHECK ... NOT VALID` then `VALIDATE CONSTRAINT` separately.

---

## 5. Soft delete queries forgetting `WHERE deleted_at IS NULL`

**Symptom:** Deleted users appear in search results, reports include archived data, unique constraint violations when re-creating a "deleted" entity.

**Root cause:** Queries were written without the soft-delete filter. There is no database-level enforcement that soft-deleted rows are excluded.

**Mitigation:**
- Create a view (`active_users`) and train the team to query the view, not the table.
- Use an ORM default scope (Rails: `default_scope { where(deleted_at: nil) }`, SQLAlchemy: custom query class).
- Use partial unique indexes so soft-deleted records don't block new records with the same natural key.
- In code reviews, grep for direct table access and verify the filter is present.

---

## 6. Polymorphic association without type constraint

**Symptom:** A `commentable_type` column contains values like `"Post"`, `"post"`, `"posts"`, and `"Blogpost"`. JOINs fail silently or return empty results.

**Root cause:** The polymorphic type column is unconstrained `TEXT`. Application code writes inconsistent values.

**Mitigation:** Add a `CHECK` constraint on the type column: `CHECK (commentable_type IN ('Post', 'Issue', 'PullRequest'))`. Alternatively, use class table inheritance (separate FK columns or separate join tables per type) which is enforceable by the database.

---

## 7. JSONB column becoming an unqueryable blob

**Symptom:** Feature requests like "find all users where settings.notifications.email is true" require full table scans. JSONB column has no consistent structure across rows.

**Root cause:** JSONB was used as a "dump everything here" column with no schema governance. Different application versions wrote different structures.

**Mitigation:**
- Define a JSON Schema for the column and validate in the application layer before writing.
- Add a GIN index for containment queries: `CREATE INDEX idx_settings ON users USING GIN (settings)`.
- When a field inside JSONB is queried frequently, extract it to a generated column:
  ```sql
  ALTER TABLE users ADD COLUMN email_notifications BOOLEAN
    GENERATED ALWAYS AS ((settings->'notifications'->>'email')::boolean) STORED;
  ```
- Set a policy: if a JSONB field is referenced in more than 2 queries, it becomes a column.

---

## 8. `FLOAT` for monetary values

**Symptom:** Invoice totals are off by fractions of a cent. `0.1 + 0.2 != 0.3` in the database.

**Root cause:** Money was stored as `FLOAT` or `DOUBLE PRECISION`, which are IEEE 754 floating-point types with inherent rounding errors.

**Mitigation:** Always use `NUMERIC(p,s)` or `DECIMAL(p,s)` for money. Common choice: `NUMERIC(12,2)` for amounts up to 9,999,999,999.99. Alternatively, store amounts as integer cents (`BIGINT`) and divide by 100 at the presentation layer.

---

## 9. Timestamp without time zone

**Symptom:** Records created in different time zones appear out of order. Scheduled events fire at the wrong time.

**Root cause:** `TIMESTAMP` (without time zone) was used. The database stores the literal value with no zone context. If the server's `timezone` setting changes, all stored times are silently reinterpreted.

**Mitigation:** Always use `TIMESTAMPTZ` (Postgres) or store timestamps in UTC explicitly. Set the database and application time zone to UTC. Never rely on the server's local time zone.

---

## 10. Adding NOT NULL to a populated column without backfill

**Symptom:** Migration fails with `column "x" contains null values`. Or on some databases, the entire table is rewritten and writes are blocked for minutes.

**Root cause:** `ALTER TABLE ... ALTER COLUMN x SET NOT NULL` when existing rows have NULL in that column. In Postgres, this also requires a full table scan to verify.

**Mitigation:** Three-step process: (1) Add a default or backfill existing NULLs: `UPDATE table SET x = 'default' WHERE x IS NULL`. (2) Add a `CHECK` constraint as `NOT VALID`: `ALTER TABLE ... ADD CONSTRAINT x_not_null CHECK (x IS NOT NULL) NOT VALID`. (3) Validate separately: `ALTER TABLE ... VALIDATE CONSTRAINT x_not_null`. Step 3 only takes a `SHARE UPDATE EXCLUSIVE` lock, not `ACCESS EXCLUSIVE`.

---

Related skills: `orm-patterns`, `postgresql`, `sqlite`, `api-contracts`.
