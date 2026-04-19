---
name: well-formed-example
description: >
  Plans and sequences database schema migrations for PostgreSQL services,
  producing versioned migration files, a rollback strategy, and a
  deployment-order manifest.
---

# Database Migration Planning

## Purpose

Translate a set of requested schema changes into a safe, ordered migration
plan for PostgreSQL databases. Produces numbered migration files that can
be applied with any standard migration runner (Flyway, golang-migrate,
Alembic), a tested rollback script for each step, and a deployment-order
manifest that respects foreign-key dependencies and data-backfill needs.

## When to use

- A feature branch requires schema changes (new tables, column additions,
  index changes, enum extensions).
- A data model refactor spans multiple tables and needs ordered execution.
- A large table needs an online migration (e.g., column rename via
  expand-contract pattern).

## When NOT to use

- For query performance tuning without schema changes — use
  `query-optimizer` instead.
- For data-only backfills that do not alter schema — use
  `data-backfill-runner` instead.
- For application-level ORM model changes that have not been approved
  for migration yet — complete design review first.

## Procedure

1. Collect the list of requested changes from the migration request
   document at `migrations/requests/<ticket-id>.md`. Each change must
   include: table name, operation (CREATE, ALTER, DROP), column
   definitions, and constraints.

2. Load the current schema snapshot from `migrations/schema.sql` (dumped
   by `pg_dump --schema-only`). Parse it into an in-memory representation
   of tables, columns, indexes, constraints, and enums.

3. For each requested change, compute the dependency graph:
   - Foreign keys imply ordering (referenced table migrated first).
   - Enum additions must precede columns that use the new value.
   - Index creation on large tables is scheduled as a separate
     `CONCURRENTLY` step after the column exists.

4. Topologically sort the changes. If a cycle is detected, report the
   cycle and abort — the request document must be revised.

5. For each sorted change, generate two files:
   - `V<NNN>__<description>.sql` — the forward migration.
   - `V<NNN>__<description>_rollback.sql` — the reverse operation.

6. For any ALTER on a table exceeding 1 million rows, apply the
   expand-contract pattern:
   - Step A: Add new column (nullable), deploy code that writes both.
   - Step B: Backfill existing rows in batches of 10,000.
   - Step C: Add NOT NULL constraint, drop old column.

7. Generate `migrations/manifest.yaml` listing every migration file in
   execution order with estimated lock duration and rollback safety flag.

8. Dry-run every forward migration against a disposable database created
   from `migrations/schema.sql`. Capture any errors.

9. Dry-run every rollback migration to verify reversibility.

10. Update `migrations/schema.sql` with the new post-migration schema.

## Output contract

- `migrations/versions/V<NNN>__*.sql` — Numbered forward migration files.
- `migrations/versions/V<NNN>__*_rollback.sql` — Corresponding rollback
  files.
- `migrations/manifest.yaml` — Ordered execution manifest with metadata.
- `migrations/schema.sql` — Updated full-schema snapshot.
- Dry-run log written to stdout. Exit code 0 if all migrations and
  rollbacks succeed; exit code 1 otherwise.

## Failure handling

- If the dependency graph contains a cycle, list the involved tables and
  exit with code 2. Do not generate partial migration files.
- If a dry-run migration fails, include the full PostgreSQL error in the
  output and mark the corresponding step as FAILED in the manifest.
- If `pg_dump` cannot connect to the reference database, fall back to
  parsing `migrations/schema.sql` directly with a warning that the
  snapshot may be stale.

## Next steps

- Submit generated migrations for review via the team's pull-request
  workflow.
- After merge, execute migrations using `deployment-orchestrator`.
- Track migration completion through `skill-lifecycle-management`.

## References

- `references/expand-contract-guide.md` — Detailed expand-contract
  pattern walkthrough with PostgreSQL-specific examples.
- `references/lock-duration-estimates.md` — Table of estimated lock
  durations by operation type and table size.
