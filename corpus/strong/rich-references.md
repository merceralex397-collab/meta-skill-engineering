---
name: rich-references-example
description: >
  Plans and executes zero-downtime data migrations between storage backends
  (e.g., MySQL to PostgreSQL, DynamoDB to Aurora) with validation and
  rollback checkpoints.
---

# Storage Backend Migration

## Purpose

Migrate application data from one storage backend to another without
downtime. Handles schema translation, continuous replication during the
cutover window, data validation, and automated rollback if validation
fails. Designed for migrations where the application must remain live
throughout.

## When to use

- The team has decided to move from one database engine to another (e.g.,
  MySQL → PostgreSQL, DynamoDB → Aurora).
- A service is consolidating from multiple data stores into a single
  backend.
- A legacy database must be retired and its data moved to a supported
  platform.

## When NOT to use

- For schema changes within the same database engine — use
  `database-migration-planning` instead.
- For one-time data exports or archival — use `data-export-runner` instead.
- For replication setup without migration intent — use `replica-setup`
  instead.

## Procedure

1. Generate a schema mapping document by comparing the source and target
   DDL. Use `references/migration-checklist.md` to verify that every
   source table, column, index, and constraint has a target equivalent.

2. Create the target schema in the destination database. Run the DDL and
   verify with `\dt` / `SHOW TABLES` that all objects exist.

3. Perform an initial bulk data copy using the appropriate tool for the
   backend pair (e.g., `pgloader` for MySQL → PostgreSQL, custom DMS
   task for DynamoDB → Aurora). Log row counts per table.

4. Enable change-data-capture (CDC) on the source database to stream
   ongoing writes to the target. Verify CDC lag is below 5 seconds
   before proceeding.

5. Run the validation suite described in `references/migration-checklist.md`
   section 3:
   - Row count comparison per table (tolerance: 0 for non-streaming
     tables, ≤ CDC lag window for streaming tables).
   - Checksum comparison on a random 1% sample of rows per table.
   - Foreign key integrity check on the target.

6. If validation passes, execute the cutover:
   a. Set the source to read-only.
   b. Wait for CDC lag to reach 0.
   c. Re-run row count validation.
   d. Switch the application's connection string to the target.
   e. Verify application health checks pass within 60 seconds.

7. If any step fails, execute the rollback procedure detailed in
   `references/rollback-guide.md`:
   a. Switch the application connection string back to the source.
   b. Disable CDC.
   c. Mark the migration attempt as FAILED in the migration log.

8. After successful cutover, keep the source in read-only mode for 48
   hours as a safety net, then decommission.

## Output contract

- `migration-report.md` — Full report including row counts, checksums,
  CDC lag timeline, cutover timestamp, and validation results.
- `migration-log.json` — Machine-readable event log of every step with
  timestamps and status (PASS / FAIL / SKIP).
- Exit code 0 on successful cutover; exit code 1 on rollback.

## Failure handling

- If bulk copy fails partway, resume from the last checkpoint rather
  than restarting. Checkpoints are logged per-table in `migration-log.json`.
- If CDC lag exceeds 30 seconds for more than 5 minutes, pause the
  migration and alert the operator. Do not proceed to cutover.
- If post-cutover health checks fail, trigger automatic rollback within
  60 seconds using `references/rollback-guide.md` procedure.

## Next steps

- Validate migrated data with `skill-testing-harness` integration tests.
- Feed `migration-report.md` into `skill-evaluation` for process scoring.
- Decommission the source backend via `infrastructure-teardown` after the
  48-hour observation window.

## References

- `references/migration-checklist.md` — Step-by-step validation checklist
  covering row counts, constraint verification, and data sampling.
- `references/rollback-guide.md` — Detailed rollback procedure with
  connection-string switchback, CDC teardown, and incident documentation.
