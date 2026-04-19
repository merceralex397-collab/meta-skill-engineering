# Implementation Patterns

Concrete SQLite patterns for configuration, connection management, migrations,
extensions, and backup strategies.

## WAL mode setup

### Essential connection initialization

Every new connection must set PRAGMAs before any other operations:

```python
import sqlite3

def get_connection(db_path: str) -> sqlite3.Connection:
    conn = sqlite3.connect(db_path)
    conn.execute("PRAGMA journal_mode = WAL")
    conn.execute("PRAGMA synchronous = NORMAL")
    conn.execute("PRAGMA foreign_keys = ON")
    conn.execute("PRAGMA busy_timeout = 5000")
    conn.execute("PRAGMA cache_size = -64000")  # 64MB
    conn.execute("PRAGMA wal_autocheckpoint = 1000")
    return conn
```

```javascript
// Node.js with better-sqlite3
const Database = require('better-sqlite3');
const db = new Database('app.db');
db.pragma('journal_mode = WAL');
db.pragma('synchronous = NORMAL');
db.pragma('foreign_keys = ON');
db.pragma('busy_timeout = 5000');
db.pragma('cache_size = -64000');
```

```rust
// Rust with rusqlite
use rusqlite::Connection;

fn open_db(path: &str) -> rusqlite::Result<Connection> {
    let conn = Connection::open(path)?;
    conn.execute_batch("
        PRAGMA journal_mode = WAL;
        PRAGMA synchronous = NORMAL;
        PRAGMA foreign_keys = ON;
        PRAGMA busy_timeout = 5000;
        PRAGMA cache_size = -64000;
    ")?;
    Ok(conn)
}
```

### WAL mode verification

```sql
-- Verify WAL is active (returns "wal"):
PRAGMA journal_mode;

-- Check WAL file status:
PRAGMA wal_checkpoint;
-- Returns: (busy, log_pages, checkpointed_pages)
```

## Connection management patterns

### Single-writer with multiple readers

```python
import threading
import sqlite3
from contextlib import contextmanager

class SQLitePool:
    """Single write connection, multiple read connections."""

    def __init__(self, db_path: str):
        self._db_path = db_path
        self._write_lock = threading.Lock()
        self._write_conn = self._make_conn()

    def _make_conn(self) -> sqlite3.Connection:
        conn = sqlite3.connect(self._db_path)
        conn.execute("PRAGMA journal_mode = WAL")
        conn.execute("PRAGMA synchronous = NORMAL")
        conn.execute("PRAGMA foreign_keys = ON")
        conn.execute("PRAGMA busy_timeout = 5000")
        return conn

    @contextmanager
    def reader(self):
        """Create a new read-only connection (safe for concurrent use)."""
        conn = self._make_conn()
        conn.execute("PRAGMA query_only = ON")
        try:
            yield conn
        finally:
            conn.close()

    @contextmanager
    def writer(self):
        """Serialize write access through a single connection."""
        with self._write_lock:
            try:
                yield self._write_conn
                self._write_conn.commit()
            except Exception:
                self._write_conn.rollback()
                raise
```

### Web framework integration (Flask example)

```python
import sqlite3
from flask import Flask, g

app = Flask(__name__)
DATABASE = '/path/to/app.db'

def get_db():
    if 'db' not in g:
        g.db = sqlite3.connect(DATABASE)
        g.db.execute("PRAGMA journal_mode = WAL")
        g.db.execute("PRAGMA foreign_keys = ON")
        g.db.execute("PRAGMA busy_timeout = 5000")
        g.db.row_factory = sqlite3.Row
    return g.db

@app.teardown_appcontext
def close_db(exception):
    db = g.pop('db', None)
    if db is not None:
        db.close()
```

## Migration strategies

### Numbered migration files

```
migrations/
  001_create_users.sql
  002_create_posts.sql
  003_add_users_email_index.sql
```

### Migration runner

```python
import os
import sqlite3

def run_migrations(conn: sqlite3.Connection, migrations_dir: str):
    conn.execute("""
        CREATE TABLE IF NOT EXISTS _migrations (
            id INTEGER PRIMARY KEY,
            name TEXT NOT NULL UNIQUE,
            applied_at TEXT NOT NULL DEFAULT (datetime('now'))
        )
    """)

    applied = {row[0] for row in conn.execute("SELECT name FROM _migrations")}

    for filename in sorted(os.listdir(migrations_dir)):
        if filename.endswith('.sql') and filename not in applied:
            path = os.path.join(migrations_dir, filename)
            with open(path) as f:
                sql = f.read()
            conn.executescript(sql)
            conn.execute("INSERT INTO _migrations (name) VALUES (?)", (filename,))
            conn.commit()
```

### The 12-step ALTER TABLE pattern

For schema changes SQLite cannot do directly (column type changes, adding constraints):

```sql
-- 1. Create new table with desired schema
CREATE TABLE users_new (
    id INTEGER PRIMARY KEY,
    email TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- 2. Copy data from old table
INSERT INTO users_new (id, email, name, created_at)
SELECT id, email, name, created_at FROM users;

-- 3. Drop old table
DROP TABLE users;

-- 4. Rename new table
ALTER TABLE users_new RENAME TO users;

-- 5. Recreate indexes
CREATE INDEX idx_users_email ON users (email);

-- 6. Recreate triggers (if any)
-- 7. Verify with PRAGMA integrity_check
```

## JSON1 extension usage

### Storing and querying JSON

```sql
-- Store JSON metadata
CREATE TABLE products (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    metadata TEXT  -- JSON stored as TEXT
);

INSERT INTO products (name, metadata) VALUES
    ('Widget', '{"color": "red", "weight": 1.5, "tags": ["sale", "new"]}');

-- Extract scalar values
SELECT name, json_extract(metadata, '$.color') AS color
FROM products
WHERE json_extract(metadata, '$.weight') < 2.0;

-- Query arrays with json_each
SELECT p.name, j.value AS tag
FROM products p, json_each(p.metadata, '$.tags') j
WHERE j.value = 'sale';

-- Update JSON fields
UPDATE products
SET metadata = json_set(metadata, '$.color', 'blue')
WHERE id = 1;

-- Check if JSON is valid
SELECT name FROM products WHERE json_valid(metadata);
```

## FTS5 full-text search

### Setup and usage

```sql
-- Create FTS5 virtual table
CREATE VIRTUAL TABLE docs_fts USING fts5(
    title,
    body,
    content='docs',        -- external content table
    content_rowid='id'
);

-- Populate from source table
INSERT INTO docs_fts(rowid, title, body)
SELECT id, title, body FROM docs;

-- Search with ranking
SELECT d.*, rank
FROM docs_fts f
JOIN docs d ON d.id = f.rowid
WHERE docs_fts MATCH 'sqlite AND (performance OR optimization)'
ORDER BY rank;

-- Keep FTS in sync with triggers
CREATE TRIGGER docs_ai AFTER INSERT ON docs BEGIN
    INSERT INTO docs_fts(rowid, title, body) VALUES (new.id, new.title, new.body);
END;

CREATE TRIGGER docs_ad AFTER DELETE ON docs BEGIN
    INSERT INTO docs_fts(docs_fts, rowid, title, body)
    VALUES ('delete', old.id, old.title, old.body);
END;

CREATE TRIGGER docs_au AFTER UPDATE ON docs BEGIN
    INSERT INTO docs_fts(docs_fts, rowid, title, body)
    VALUES ('delete', old.id, old.title, old.body);
    INSERT INTO docs_fts(rowid, title, body) VALUES (new.id, new.title, new.body);
END;
```

## Backup strategies

### Hot backup with .backup API

```python
import sqlite3

def backup_database(source_path: str, backup_path: str):
    source = sqlite3.connect(source_path)
    backup = sqlite3.connect(backup_path)
    source.backup(backup)
    backup.close()
    source.close()
```

### VACUUM INTO (SQLite 3.27+)

```sql
-- Creates a compacted backup without locking the source for the full duration
VACUUM INTO '/path/to/backup.db';
```

### Periodic checkpoint before backup

```sql
-- Force a full WAL checkpoint to ensure backup has all data
PRAGMA wal_checkpoint(TRUNCATE);
```

Related skills: `postgresql`, `orm-patterns`, `data-model`, `background-jobs-queues`.
