---
name: data-model
description: "Designs database entities, relationships, constraints, and migration-safe schema evolution. Use when creating tables, defining foreign keys, choosing primary key strategies (UUID/ULID/auto-increment), adding indexes, implementing soft deletes, designing audit trails, writing schema migrations, or deciding between normalized columns and JSON/JSONB storage."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: data-model
  maturity: draft
  risk: medium
  tags: [data, model, schema, migration, normalization, constraints]
---

# Purpose

Designs database entities, relationships, constraints, and migration-safe schema evolution. Covers normalization (1NF–3NF and when to denormalize), relationship mapping, primary key strategies, constraint design, soft delete patterns, temporal data, schema migrations, JSON/JSONB columns, polymorphic associations, audit trails, and index design.

# When to use this skill

Use this skill when:

- Creating or altering database tables, columns, or constraints
- Designing entity relationships (one-to-one, one-to-many, many-to-many with junction tables)
- Choosing a primary key strategy (auto-increment, UUID v4/v7, ULID, composite keys)
- Adding or modifying indexes to support query patterns
- Implementing soft delete (`deleted_at` column) or temporal patterns (SCD Type 1/2, effective dating)
- Writing schema migrations that must be safe against concurrent production traffic
- Deciding whether to normalize data or use JSON/JSONB columns
- Designing audit columns (`created_at`, `updated_at`, `created_by`, `updated_by`)
- Implementing polymorphic associations (single table inheritance, class table inheritance)

# Do not use this skill when

- The task is about ORM query optimization or application-level data access patterns — prefer `orm-patterns`
- The task is about API request/response contracts — prefer `api-contracts`
- The task is BigQuery-specific analytical schema design — prefer `bigquery`
- The task is purely about database performance tuning (query plans, connection pooling) — prefer `postgresql` or `sqlite`

# Operating procedure

1. **Inventory existing schema.** Run `\dt` (Postgres), `.tables` (SQLite), or `SHOW TABLES` (MySQL) to list current tables. Read the active migration files to understand schema history. Identify the ORM in use (SQLAlchemy, Prisma, ActiveRecord, TypeORM, Drizzle, etc.).
2. **Define entities and their normal form.** For each entity, list all attributes. Apply normalization: eliminate repeating groups (1NF), remove partial dependencies (2NF), remove transitive dependencies (3NF). Document any intentional denormalization with a written justification (read performance, reporting, etc.).
3. **Map relationships explicitly.** For each relationship, determine cardinality. One-to-many: FK on the many side. Many-to-many: create a junction table with composite PK or surrogate PK + unique constraint on the FK pair. One-to-one: FK with UNIQUE constraint.
4. **Choose primary key strategy.** Auto-increment for internal-only single-database tables. UUID v4 for distributed systems needing collision-free IDs (accept index fragmentation cost). UUID v7 or ULID for distributed systems needing time-sortable IDs with better index locality. Composite keys only when the combination is the natural domain identifier.
5. **Define constraints.** Every required field gets NOT NULL. Natural keys get UNIQUE constraints. Every FK specifies ON DELETE behavior (CASCADE, RESTRICT, SET NULL). Add CHECK constraints for enum-like columns (`CHECK (status IN ('active', 'suspended', 'deleted'))`). Add exclusion constraints for non-overlapping ranges where supported.
6. **Design indexes.** Add indexes on all FK columns. Add indexes for columns used in WHERE, ORDER BY, and JOIN clauses in hot queries. Use partial indexes for soft-deleted records (`WHERE deleted_at IS NULL`). Use composite indexes with the most selective column first.
7. **Add temporal and audit columns.** Add `created_at TIMESTAMPTZ NOT NULL DEFAULT now()` and `updated_at TIMESTAMPTZ NOT NULL DEFAULT now()` with a trigger or ORM hook. For audit trails, add `created_by` and `updated_by` FK columns to the users table. For versioned data, choose SCD Type 2 (history rows with effective_from/effective_to).
8. **Write the migration.** Use the project's migration tool (`alembic revision --autogenerate`, `prisma migrate dev`, `rails generate migration`, `knex migrate:make`). Ensure the migration is reversible — provide both `up` and `down`. For column renames, use a multi-step deploy: add new column → backfill → update code → drop old column.
9. **Validate.** Run the migration against a test database. Check that rollback works. Run the full test suite. Verify FK cascades behave correctly with test data. Check query plans for new indexes with `EXPLAIN ANALYZE`.

# Decision rules

- **Normalize by default; denormalize with evidence.** Do not denormalize until you have a measured query performance problem. Document the denormalization and the compensating consistency mechanism.
- **Every FK must specify ON DELETE.** Never rely on the database default. CASCADE for child lifecycle tied to parent. RESTRICT for children that must not be orphaned. SET NULL for optional associations.
- **Prefer UUID v7/ULID over UUID v4** when you need distributed IDs. The time-sortable prefix preserves B-tree insert performance.
- **Soft deletes need global discipline.** If you add `deleted_at`, every query on that table must filter `WHERE deleted_at IS NULL` unless explicitly querying archived data. Use a view or default scope to enforce this.
- **JSON/JSONB is for truly schemaless or rarely-queried data.** If you filter, join, or aggregate on a field inside JSON more than occasionally, extract it to a column.
- **Migrations must not lock tables for extended periods.** On large tables, use `ALTER TABLE ... ADD COLUMN` (non-blocking in Postgres for nullable/default columns), avoid rewriting the table, and use `CREATE INDEX CONCURRENTLY`.
- **One migration per concern.** Do not bundle unrelated schema changes. This makes rollback granular.

# Output requirements

1. `Entity List` — all entities with their attributes, types, and constraints
2. `Relationship Map` — cardinality and FK direction for every relationship
3. `Key Strategy` — PK type chosen per table with rationale
4. `Migration Plan` — ordered migration steps with reversibility notes
5. `Index Plan` — indexes added with the query pattern they support

# References

Read these when the task involves the relevant pattern:

- `references/implementation-patterns.md` — normalization examples, junction tables, soft delete, temporal patterns, polymorphic associations, JSONB patterns, audit columns
- `references/validation-checklist.md` — constraint verification, migration safety, index coverage
- `references/failure-modes.md` — orphan records, UUID fragmentation, migration deadlocks, soft delete query leaks

# Anti-patterns

- **The God Table.** One table with 40+ columns, nullable everything, used by every feature. Split into proper entities.
- **Stringly-typed enums.** Storing status as unconstrained VARCHAR with no CHECK constraint. Use CHECK or a lookup table.
- **Missing FK constraints.** "We enforce it in the app layer." The app layer has bugs; the database does not forget.
- **`SELECT *` driven schema.** Adding columns "because the ORM maps them" without considering query cost. Design columns for the domain, not the ORM.
- **Migration-by-yolo.** Running `ALTER TABLE` directly in production without a migration file. Every schema change must be versioned.
- **Premature JSONB.** Storing structured, queryable data in JSONB because "it's flexible." It's flexible until you need a JOIN.
- **Shared sequence across tables.** Using a single auto-increment sequence for multiple tables to get "globally unique" IDs. Use UUID instead.

# Related skills

- `orm-patterns` — query building, lazy/eager loading, N+1 prevention
- `postgresql` — Postgres-specific types, extensions, performance tuning
- `sqlite` — SQLite-specific constraints and limitations
- `api-contracts` — how schema maps to API response shapes
- `bigquery` — analytical denormalized schema design

# Failure handling

- **If the existing schema is undocumented:** Run schema introspection (`pg_dump --schema-only`, `prisma db pull`, `.schema` in SQLite) to generate a baseline before proposing changes.
- **If a migration fails mid-apply:** Do not manually fix the database. Roll back the migration, fix the migration file, and re-run. If the migration tool's state is corrupted, check its version table (`alembic_version`, `_prisma_migrations`, `schema_migrations`).
- **If you cannot determine cardinality:** Ask for concrete examples. "Can one Order have many Payments, or exactly one?" Do not guess relationship cardinality.
- **If performance requirements are unclear:** Default to normalized design with appropriate indexes. Denormalization is always available later; un-denormalizing is painful.
- **If the task spans multiple services with separate databases:** Identify which service owns each entity. Cross-service references use logical IDs (not FKs). Document the eventual consistency boundary.
