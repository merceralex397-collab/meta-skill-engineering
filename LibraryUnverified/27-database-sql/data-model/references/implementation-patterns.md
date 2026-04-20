# Implementation Patterns

Concrete patterns for data model design. Reference these when building or reviewing schema.

---

## Normalization examples

**1NF — eliminate repeating groups:**

```sql
-- BAD: repeating group in a single column
CREATE TABLE orders (
  id SERIAL PRIMARY KEY,
  customer_id INT NOT NULL,
  item_names TEXT  -- "widget,gadget,sprocket"
);

-- GOOD: separate table for order items
CREATE TABLE order_items (
  id SERIAL PRIMARY KEY,
  order_id INT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
  product_id INT NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  quantity INT NOT NULL CHECK (quantity > 0),
  unit_price NUMERIC(10,2) NOT NULL
);
```

**2NF — remove partial dependencies (relevant for composite keys):**

```sql
-- BAD: product_name depends only on product_id, not on (order_id, product_id)
CREATE TABLE order_items (
  order_id INT,
  product_id INT,
  product_name TEXT,  -- partial dependency
  quantity INT,
  PRIMARY KEY (order_id, product_id)
);

-- GOOD: product_name lives in the products table
CREATE TABLE products (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  sku TEXT NOT NULL UNIQUE
);
```

**3NF — remove transitive dependencies:**

```sql
-- BAD: city depends on zip_code, not directly on customer
-- customer_id → zip_code → city
CREATE TABLE customers (
  id SERIAL PRIMARY KEY,
  zip_code TEXT,
  city TEXT  -- transitive dependency
);

-- GOOD: separate location table or derive city from zip at query time
CREATE TABLE zip_codes (
  code TEXT PRIMARY KEY,
  city TEXT NOT NULL,
  state TEXT NOT NULL
);
```

---

## Junction table patterns (many-to-many)

```sql
-- Standard junction table with surrogate PK
CREATE TABLE project_members (
  id SERIAL PRIMARY KEY,
  project_id INT NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
  user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  role TEXT NOT NULL DEFAULT 'member' CHECK (role IN ('owner', 'admin', 'member', 'viewer')),
  joined_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  UNIQUE (project_id, user_id)
);

-- Junction table with composite PK (no surrogate needed if no extra attributes)
CREATE TABLE article_tags (
  article_id INT NOT NULL REFERENCES articles(id) ON DELETE CASCADE,
  tag_id INT NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
  PRIMARY KEY (article_id, tag_id)
);

-- Self-referencing many-to-many (followers)
CREATE TABLE user_follows (
  follower_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  following_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (follower_id, following_id),
  CHECK (follower_id != following_id)
);
```

---

## Soft delete implementation

```sql
-- Add soft delete column
ALTER TABLE users ADD COLUMN deleted_at TIMESTAMPTZ;

-- Create a view for active records (preferred access path)
CREATE VIEW active_users AS
  SELECT * FROM users WHERE deleted_at IS NULL;

-- Partial index for active records — speeds up the common query
CREATE INDEX idx_users_active_email ON users (email) WHERE deleted_at IS NULL;

-- Soft delete operation
UPDATE users SET deleted_at = now() WHERE id = 42;

-- Restore
UPDATE users SET deleted_at = NULL WHERE id = 42;

-- Unique constraint that only applies to active records
CREATE UNIQUE INDEX idx_users_unique_email_active ON users (email) WHERE deleted_at IS NULL;
```

---

## Temporal patterns

**SCD Type 1 — overwrite (no history):**

```sql
-- Simple updated_at tracking
CREATE TABLE products (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  price NUMERIC(10,2) NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Trigger to auto-update updated_at (Postgres)
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_products_updated_at
  BEFORE UPDATE ON products
  FOR EACH ROW EXECUTE FUNCTION update_updated_at();
```

**SCD Type 2 — full history with effective dating:**

```sql
CREATE TABLE pricing_history (
  id SERIAL PRIMARY KEY,
  product_id INT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
  price NUMERIC(10,2) NOT NULL,
  effective_from TIMESTAMPTZ NOT NULL DEFAULT now(),
  effective_to TIMESTAMPTZ,  -- NULL means current
  created_by INT REFERENCES users(id)
);

-- Current price query
SELECT price FROM pricing_history
WHERE product_id = 7 AND effective_to IS NULL;

-- Price at a specific point in time
SELECT price FROM pricing_history
WHERE product_id = 7
  AND effective_from <= '2024-06-15T00:00:00Z'
  AND (effective_to IS NULL OR effective_to > '2024-06-15T00:00:00Z');
```

---

## Polymorphic association approaches

**Single Table Inheritance (STI):**

```sql
CREATE TABLE notifications (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  type TEXT NOT NULL CHECK (type IN ('email', 'sms', 'push')),
  recipient TEXT NOT NULL,
  subject TEXT,          -- email only
  body TEXT NOT NULL,
  phone_number TEXT,     -- sms only
  device_token TEXT,     -- push only
  sent_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- Pro: single table, simple queries. Con: many nullable columns.
```

**Class Table Inheritance:**

```sql
CREATE TABLE comments (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  body TEXT NOT NULL,
  author_id INT NOT NULL REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE issue_comments (
  comment_id UUID PRIMARY KEY REFERENCES comments(id) ON DELETE CASCADE,
  issue_id INT NOT NULL REFERENCES issues(id) ON DELETE CASCADE
);

CREATE TABLE pr_comments (
  comment_id UUID PRIMARY KEY REFERENCES comments(id) ON DELETE CASCADE,
  pull_request_id INT NOT NULL REFERENCES pull_requests(id) ON DELETE CASCADE,
  line_number INT
);
-- Pro: no nullable columns. Con: requires JOIN to get full record.
```

**Shared Primary Key pattern (concrete table with FK to base):**

```sql
-- Base entity
CREATE TABLE vehicles (
  id SERIAL PRIMARY KEY,
  type TEXT NOT NULL CHECK (type IN ('car', 'truck', 'motorcycle')),
  make TEXT NOT NULL,
  model TEXT NOT NULL,
  year INT NOT NULL
);

-- Subtype tables share the PK
CREATE TABLE cars (
  vehicle_id INT PRIMARY KEY REFERENCES vehicles(id) ON DELETE CASCADE,
  num_doors INT NOT NULL,
  trunk_capacity_liters INT
);

CREATE TABLE trucks (
  vehicle_id INT PRIMARY KEY REFERENCES vehicles(id) ON DELETE CASCADE,
  payload_capacity_kg NUMERIC(8,2) NOT NULL,
  num_axles INT NOT NULL DEFAULT 2
);
```

---

## JSONB column patterns

```sql
-- Good use: truly variable metadata that differs per row
CREATE TABLE integrations (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  provider TEXT NOT NULL,
  config JSONB NOT NULL DEFAULT '{}',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- GIN index for containment queries
CREATE INDEX idx_integrations_config ON integrations USING GIN (config);

-- Query: find integrations with a specific setting
SELECT * FROM integrations WHERE config @> '{"webhook_enabled": true}';

-- Query: extract a specific key
SELECT id, config->>'api_key' AS api_key FROM integrations WHERE provider = 'stripe';
```

When NOT to use JSONB — if you regularly filter, join, or sort by a field, extract it:

```sql
-- BAD: filtering inside JSONB on every request
SELECT * FROM orders WHERE metadata->>'priority' = 'high';

-- GOOD: promote to a real column
ALTER TABLE orders ADD COLUMN priority TEXT CHECK (priority IN ('low', 'normal', 'high', 'urgent'));
```

---

## Audit columns

```sql
-- Standard audit column set
CREATE TABLE invoices (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  -- domain columns
  customer_id INT NOT NULL REFERENCES customers(id),
  total NUMERIC(12,2) NOT NULL,
  status TEXT NOT NULL DEFAULT 'draft' CHECK (status IN ('draft', 'sent', 'paid', 'void')),
  -- audit columns
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  created_by INT NOT NULL REFERENCES users(id),
  updated_by INT NOT NULL REFERENCES users(id)
);

-- For full audit log (append-only history table)
CREATE TABLE invoice_audit_log (
  id BIGSERIAL PRIMARY KEY,
  invoice_id UUID NOT NULL REFERENCES invoices(id),
  action TEXT NOT NULL CHECK (action IN ('create', 'update', 'delete')),
  changed_fields JSONB,
  old_values JSONB,
  new_values JSONB,
  performed_by INT NOT NULL REFERENCES users(id),
  performed_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

---

Related skills: `orm-patterns`, `postgresql`, `sqlite`, `api-contracts`.
