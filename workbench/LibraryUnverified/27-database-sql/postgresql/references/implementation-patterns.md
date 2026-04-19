# Implementation Patterns

Concrete PostgreSQL patterns for schema design, indexing, connection management,
migrations, and query optimization.

## Index type selection guide

| Index Type | Use When | Example |
|------------|----------|---------|
| B-tree | Equality, range, sorting, `LIKE 'prefix%'` | `CREATE INDEX idx_users_email ON users (email)` |
| GIN | Array ops (`@>`), JSONB queries, full-text (`tsvector`) | `CREATE INDEX idx_docs_tags ON docs USING gin (tags)` |
| GiST | Geometry, range types, nearest-neighbor | `CREATE INDEX idx_locations_geo ON locations USING gist (coordinates)` |
| BRIN | Large, naturally-ordered tables (time-series) | `CREATE INDEX idx_events_ts ON events USING brin (created_at)` |
| Hash | Equality-only (rare — B-tree usually better) | `CREATE INDEX idx_sessions_token ON sessions USING hash (token)` |

### Composite index ordering

Place equality columns first, then range columns, then sort columns:

```sql
-- Query: WHERE tenant_id = $1 AND created_at > $2 ORDER BY priority DESC
CREATE INDEX idx_tasks_lookup ON tasks (tenant_id, created_at, priority DESC);
```

### Partial indexes

Reduce index size by filtering on common query predicates:

```sql
-- Only index active users (80% of queries filter on active = true)
CREATE INDEX idx_users_active_email ON users (email) WHERE active = true;
```

### Expression indexes

Index computed values for function-based lookups:

```sql
CREATE INDEX idx_users_lower_email ON users (lower(email));
-- Query: WHERE lower(email) = lower($1)
```

## Connection pooling configuration

### pgbouncer transaction-mode setup

```ini
; pgbouncer.ini
[databases]
myapp = host=127.0.0.1 port=5432 dbname=myapp

[pgbouncer]
listen_addr = 0.0.0.0
listen_port = 6432
auth_type = md5
auth_file = /etc/pgbouncer/userlist.txt

; Transaction mode — connection returned to pool after each transaction
pool_mode = transaction
default_pool_size = 25          ; per user/database pair
max_client_conn = 200           ; total client connections
reserve_pool_size = 5           ; extra connections for burst
reserve_pool_timeout = 3        ; seconds before using reserve pool

; Timeouts
server_idle_timeout = 300       ; close idle server connections after 5 min
client_idle_timeout = 0         ; 0 = no client idle timeout
query_timeout = 30              ; kill queries running > 30s (optional)

; Logging
log_connections = 0
log_disconnections = 0
stats_period = 60
```

### Pool sizing formula

PostgreSQL max connections: `(2 × CPU cores) + effective_spindle_count`
- 4-core server with SSD: ~10-20 `max_connections`
- pgbouncer `default_pool_size`: match or slightly exceed PostgreSQL max connections
- Application connection pool: match pgbouncer `max_client_conn`

## Migration workflow

### File naming convention

```
migrations/
  V001__create_users_table.sql
  V002__add_users_email_index.sql
  V003__create_orders_table.sql
  V003__create_orders_table_down.sql
```

### Safe migration patterns

```sql
-- Always use IF NOT EXISTS / IF EXISTS for idempotency
CREATE TABLE IF NOT EXISTS users (
    id bigserial PRIMARY KEY,
    email text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

-- Add columns as nullable first, backfill, then add constraint
ALTER TABLE users ADD COLUMN IF NOT EXISTS phone text;
-- (backfill in separate step)
-- ALTER TABLE users ALTER COLUMN phone SET NOT NULL;  -- later migration

-- Create indexes concurrently to avoid locking
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_users_email ON users (email);

-- Rename with a view bridge for zero-downtime
ALTER TABLE users RENAME COLUMN name TO display_name;
CREATE VIEW users_compat AS SELECT *, display_name AS name FROM users;
```

### Migration with alembic (Python)

```python
# alembic/versions/001_create_users.py
def upgrade():
    op.create_table('users',
        sa.Column('id', sa.BigInteger, primary_key=True, autoincrement=True),
        sa.Column('email', sa.Text, nullable=False),
        sa.Column('created_at', sa.DateTime(timezone=True), server_default=sa.func.now()),
    )
    op.create_index('idx_users_email', 'users', ['email'], unique=True,
                     postgresql_concurrently=True)

def downgrade():
    op.drop_index('idx_users_email', 'users', postgresql_concurrently=True)
    op.drop_table('users')
```

## Query optimization patterns

### Using EXPLAIN ANALYZE effectively

```sql
EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT u.id, u.email, count(o.id) AS order_count
FROM users u
JOIN orders o ON o.user_id = u.id
WHERE u.created_at > now() - interval '30 days'
GROUP BY u.id;
```

Key metrics to check:
- **Seq Scan on large tables**: Likely missing index
- **Nested Loop with high row estimates**: Consider hash or merge join hints
- **Buffers shared read** (not hit): I/O-bound; check `shared_buffers` sizing
- **Planning time** vs **Execution time**: High planning = too many partitions or complex views

### Keyset pagination

```sql
-- Instead of: SELECT * FROM events ORDER BY id LIMIT 50 OFFSET 10000
-- Use:
SELECT * FROM events
WHERE id > $last_seen_id
ORDER BY id
LIMIT 50;
```

### Bulk upsert with ON CONFLICT

```sql
INSERT INTO inventory (sku, quantity, updated_at)
VALUES ($1, $2, now()), ($3, $4, now()), ...
ON CONFLICT (sku) DO UPDATE SET
    quantity = EXCLUDED.quantity,
    updated_at = EXCLUDED.updated_at;
```

### Efficient counting

```sql
-- Exact count (slow on large tables):
SELECT count(*) FROM events WHERE type = 'click';

-- Approximate count (fast, from statistics):
SELECT reltuples::bigint FROM pg_class WHERE relname = 'events';

-- Windowed approximate with HyperLogLog (requires extension):
SELECT hll_cardinality(hll_union_agg(hll_hash_text(user_id))) FROM events;
```

## Partitioning strategies

### Range partitioning by time

```sql
CREATE TABLE events (
    id bigserial,
    created_at timestamptz NOT NULL,
    event_type text NOT NULL,
    payload jsonb
) PARTITION BY RANGE (created_at);

CREATE TABLE events_2024_q1 PARTITION OF events
    FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');
CREATE TABLE events_2024_q2 PARTITION OF events
    FOR VALUES FROM ('2024-04-01') TO ('2024-07-01');

-- Create indexes on the parent (propagated to partitions)
CREATE INDEX idx_events_type ON events (event_type);
```

### List partitioning by tenant

```sql
CREATE TABLE tenant_data (
    id bigserial,
    tenant_id int NOT NULL,
    data jsonb
) PARTITION BY LIST (tenant_id);

CREATE TABLE tenant_data_1 PARTITION OF tenant_data FOR VALUES IN (1);
CREATE TABLE tenant_data_2 PARTITION OF tenant_data FOR VALUES IN (2);
-- Add a default partition for new tenants
CREATE TABLE tenant_data_default PARTITION OF tenant_data DEFAULT;
```

### Partition maintenance

```sql
-- Detach old partitions for archival (non-blocking in v14+)
ALTER TABLE events DETACH PARTITION events_2023_q1 CONCURRENTLY;

-- Drop detached partition or move to archive schema
ALTER TABLE events_2023_q1 SET SCHEMA archive;
```

Related skills: `orm-patterns`, `data-model`, `sqlite`, `observability-logging`.
