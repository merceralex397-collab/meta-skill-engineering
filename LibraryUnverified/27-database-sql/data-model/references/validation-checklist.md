# Validation Checklist

A data model change is not ready to merge until every applicable item below is verified.

---

## Constraint completeness

- [ ] Every required field has `NOT NULL`.
- [ ] Natural keys (email, username, SKU, slug) have `UNIQUE` constraints.
- [ ] Enum-like columns have `CHECK` constraints listing valid values.
- [ ] Boolean columns default to a sensible value (`DEFAULT false` or `DEFAULT true`) — not nullable.
- [ ] Numeric columns that must be positive have `CHECK (col > 0)` or `CHECK (col >= 0)`.
- [ ] Text columns that must not be empty have `CHECK (length(col) > 0)` where the database supports it.

## Foreign key discipline

- [ ] Every FK specifies `ON DELETE` behavior explicitly — never rely on the default.
- [ ] `ON DELETE CASCADE` is used only when the child has no independent meaning without the parent.
- [ ] `ON DELETE RESTRICT` is used when child records must block parent deletion (e.g., orders block customer deletion).
- [ ] `ON DELETE SET NULL` is used for optional associations (the FK column must be nullable).
- [ ] No FK references a soft-deleted parent without filtering — if the parent uses soft delete, queries must respect `deleted_at IS NULL`.

## Index coverage

- [ ] Every FK column has an index (the database does not auto-index FKs in Postgres or SQLite).
- [ ] Columns used in `WHERE`, `ORDER BY`, or `JOIN` in hot queries are indexed.
- [ ] Composite indexes list the most selective column first.
- [ ] Partial indexes are used for soft-delete tables: `CREATE INDEX ... WHERE deleted_at IS NULL`.
- [ ] No duplicate/redundant indexes exist (e.g., index on `(a)` is redundant if `(a, b)` already exists).
- [ ] Unique constraints create implicit indexes — do not add a separate index on the same column.

## Migration safety

- [ ] The migration has both `up` and `down` (reversible).
- [ ] The migration has been tested: apply, rollback, re-apply all succeed on a clean database.
- [ ] No column renames or type changes without a multi-step migration plan (add new → backfill → deploy code → drop old).
- [ ] `NOT NULL` is not added to an existing column without a default or backfill — this rewrites the table and blocks writes.
- [ ] New indexes on large tables use `CREATE INDEX CONCURRENTLY` (Postgres) to avoid write locks.
- [ ] The migration does not drop a column still referenced by running application code (coordinate with deploy).
- [ ] Migration files are named/numbered to avoid ordering conflicts with concurrent branches.

## Soft delete consistency

- [ ] If the table uses soft delete, a view or default scope filters `WHERE deleted_at IS NULL`.
- [ ] Unique constraints account for soft delete — use partial unique index `WHERE deleted_at IS NULL` so that a deleted record does not block re-creation.
- [ ] Cascading behavior from a soft-deleted parent to children is documented and handled in application logic (since DB cascades only fire on real DELETE).
- [ ] Reports and aggregation queries explicitly state whether they include or exclude soft-deleted records.

## Temporal and audit columns

- [ ] `created_at` is `TIMESTAMPTZ NOT NULL DEFAULT now()` — never nullable, never set by application code.
- [ ] `updated_at` is maintained by a database trigger or ORM hook, not manual application updates.
- [ ] `created_by` / `updated_by` reference the users table with a valid FK.
- [ ] Audit log tables are append-only — no UPDATE or DELETE on audit rows.
- [ ] SCD Type 2 history tables have non-overlapping `effective_from` / `effective_to` ranges (enforce with exclusion constraint or application logic).

## JSON/JSONB discipline

- [ ] JSONB columns have a GIN index if they are queried with containment operators (`@>`, `?`, `?|`).
- [ ] Fields queried in `WHERE` or `ORDER BY` more than occasionally are extracted to real columns.
- [ ] JSONB columns have a documented schema (even if not enforced by the DB) — e.g., a comment or a JSON Schema in the codebase.
- [ ] Application code validates JSONB structure before writing, not just on read.

## General data integrity

- [ ] No orphan records can result from the change — trace all parent-child relationships.
- [ ] Default values are sensible — `DEFAULT ''` is almost always wrong; prefer `NOT NULL` without a default and let the application supply the value.
- [ ] Columns storing money use `NUMERIC(p,s)` — never `FLOAT` or `DOUBLE PRECISION`.
- [ ] Timestamps use `TIMESTAMPTZ` — never `TIMESTAMP` without time zone.
- [ ] The schema change is backward-compatible with the currently deployed application version (or a deploy-sequence is documented).

---

Related skills: `orm-patterns`, `postgresql`, `sqlite`.
