---
name: database-persistence
description: Covers schema design, migrations, query patterns, and persistence technology choices for the data layer. Trigger on 'schema design', 'database migration', 'choose database', 'add table', 'expand-contract migration'. DO NOT USE for query performance tuning (use performance-baseline), database infrastructure/ops (use deployment-pipeline), or full ORM framework migration (use migration-refactor).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: database-persistence
  maturity: draft
  risk: low
  tags: [database, persistence]
---

# Purpose
Choose the right persistence layer and design schema evolution strategy. Following evolutionary database design principles: all schema changes are migrations, migrations are version-controlled, and changes are small and frequent rather than large and rare.

# When to use this skill
Use when:
- Choosing database technology for new project
- Designing schema for new feature
- Planning database migration strategy
- Schema change required for existing system

Do NOT use when:
- Query optimization (use performance-profiling)
- Database operations/infrastructure (use cloud/ops skills)
- Simple CRUD with existing schema

# Operating procedure
1. **Choose persistence type by access pattern**:
   ```
   Access Pattern                    → Best Fit
   ──────────────────────────────────────────────────
   Complex queries, joins, ACID      → PostgreSQL, MySQL
   Document-oriented, flexible       → MongoDB, Firestore
   Key-value, high throughput        → Redis, DynamoDB
   Time-series, metrics              → TimescaleDB, InfluxDB
   Graph relationships               → Neo4j, Dgraph
   Full-text search                  → Elasticsearch, Typesense
   Embedded, zero-config             → SQLite
   ```

2. **Design migrations as version-controlled code**:
   ```sql
   -- migrations/001_create_users.sql
   -- Each migration has up and down
   
   -- +migrate Up
   CREATE TABLE users (
       id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
       email VARCHAR(255) NOT NULL UNIQUE,
       created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
   );
   CREATE INDEX idx_users_email ON users(email);
   
   -- +migrate Down
   DROP TABLE users;
   ```

3. **Apply expand-contract pattern for breaking changes**:
   ```sql
   -- Phase 1: Expand - add new column, keep old
   ALTER TABLE users ADD COLUMN full_name VARCHAR(255);
   
   -- Phase 2: Migrate - backfill data
   UPDATE users SET full_name = first_name || ' ' || last_name;
   
   -- Phase 3: Code change - update application to use new column
   -- (deploy application changes)
   
   -- Phase 4: Contract - remove old columns
   ALTER TABLE users DROP COLUMN first_name;
   ALTER TABLE users DROP COLUMN last_name;
   ```

4. **Never modify released migrations**:
   ```
   ❌ Edit existing migration file after it's been run in any environment
   ✅ Create new migration to alter/fix the schema
   
   migrations/
   ├── 001_create_users.sql      # Never edit after merge
   ├── 002_add_user_email.sql    # Never edit after merge
   └── 003_fix_user_email.sql    # New migration to fix
   ```

5. **Handle data migrations separately from schema migrations**:
   ```python
   # data_migrations/001_backfill_user_status.py
   # Separate from schema migrations because:
   # - May need to run in batches for large tables
   # - May need different rollback strategy
   # - May need to run with application running
   
   def migrate(batch_size=1000):
       while True:
           rows = db.execute("""
               UPDATE users SET status = 'active'
               WHERE status IS NULL
               LIMIT %s
               RETURNING id
           """, [batch_size])
           if not rows:
               break
           time.sleep(0.1)  # Rate limit to avoid lock contention
   ```

6. **Design for query patterns**:
   ```sql
   -- If you'll query by email frequently, index it
   CREATE INDEX idx_users_email ON users(email);
   
   -- If you'll query by created_at ranges, consider partial index
   CREATE INDEX idx_users_recent ON users(created_at) 
       WHERE created_at > NOW() - INTERVAL '30 days';
   
   -- If you'll join users with orders frequently, ensure FK indexed
   CREATE INDEX idx_orders_user_id ON orders(user_id);
   ```

# Output defaults
```markdown
## Database Design: [Feature/Table Name]

### Technology Choice
- Database: [PostgreSQL/MongoDB/etc]
- Rationale: [why this fits the access pattern]

### Schema
```sql
CREATE TABLE [name] (
    -- columns with types and constraints
);
```

### Migrations
1. `XXX_create_[table].sql` - Initial schema
2. `XXX_add_[column].sql` - [description]

### Indexes
- [index name]: [columns] - [query pattern it supports]

### Query Patterns
| Operation | Query | Expected Performance |
|-----------|-------|---------------------|
| Get by ID | SELECT * FROM x WHERE id = ? | O(1) |
| List recent | SELECT * FROM x ORDER BY created_at DESC LIMIT 100 | Index scan |
```

# References
- Evolutionary Database Design: https://martinfowler.com/articles/evodb.html
- PostgreSQL Documentation: https://www.postgresql.org/docs/current/

# Failure handling
- **Migration fails halfway**: Wrap in transaction where possible; have rollback ready; never leave partial state
- **Production data won't fit new constraint**: Add constraint as NOT VALID first, validate separately: `ALTER TABLE ADD CONSTRAINT ... NOT VALID; ALTER TABLE VALIDATE CONSTRAINT ...`
- **Large table migration too slow**: Use batched updates with `LIMIT` and loop; consider `pt-online-schema-change` for MySQL
- **Need to reorder migrations**: Don't; create new migration that achieves desired state from current state
- **ORM generates inefficient queries**: Log queries in development; write raw SQL for complex queries
