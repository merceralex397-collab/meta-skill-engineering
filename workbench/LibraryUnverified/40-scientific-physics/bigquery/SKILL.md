---
name: bigquery
description: >-
  Write efficient BigQuery SQL with partition pruning, clustering, cost
  estimation, and slot management. Use when writing or optimizing BigQuery
  queries, designing partitioned/clustered tables, estimating query cost
  with dry-run, or debugging slot contention. Do not use for PostgreSQL/MySQL
  queries (prefer orm-patterns) or real-time OLTP workloads.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: bigquery
  maturity: draft
  risk: low
  tags: [bigquery, sql, data]
---

# Purpose

Write efficient BigQuery SQL with proper partitioning, clustering, cost estimation, and query optimization.

# When to use this skill

- writing or optimizing BigQuery SQL queries
- designing tables with time-based partitioning and clustering
- estimating query cost before running with `--dry_run`
- debugging slow queries or slot contention in BigQuery

# Do not use this skill when

- working with PostgreSQL/MySQL — prefer `orm-patterns`
- building real-time OLTP systems — BigQuery is for analytics
- managing GCP infrastructure beyond BigQuery (Terraform, etc.)

# Procedure

1. **Estimate cost first** — run `bq query --dry_run --use_legacy_sql=false 'SELECT ...'` to see bytes scanned.
2. **Prune partitions** — always filter on the partition column (usually `_PARTITIONTIME` or a `DATE`/`TIMESTAMP` column) in `WHERE`.
3. **Use clustering** — cluster by high-cardinality filter columns (e.g., `user_id`, `event_name`) after partitioning.
4. **Select only needed columns** — BigQuery is columnar; `SELECT *` scans all columns and inflates cost.
5. **Avoid cross joins** — use `JOIN` with explicit keys. Check `INFORMATION_SCHEMA.JOBS` for bytes billed.
6. **Use `APPROX_COUNT_DISTINCT`** — for cardinality estimates on large tables, ~2% error but 10x faster.
7. **Materialize CTEs** — BigQuery evaluates CTEs multiple times; use temp tables for repeated subqueries.
8. **Monitor slots** — check `INFORMATION_SCHEMA.JOBS_BY_PROJECT` for `total_slot_ms` to find expensive queries.

# Table design

```sql
CREATE TABLE project.dataset.events (
  event_date    DATE NOT NULL,
  event_name    STRING NOT NULL,
  user_id       STRING,
  properties    JSON,
  created_at    TIMESTAMP NOT NULL
)
PARTITION BY event_date
CLUSTER BY event_name, user_id
OPTIONS (
  partition_expiration_days = 365,
  require_partition_filter = true
);
```

# Cost estimation

```bash
# Dry run to check bytes scanned (cost = bytes * $6.25/TB on-demand)
bq query --dry_run --use_legacy_sql=false \
  'SELECT user_id, COUNT(*) FROM project.dataset.events
   WHERE event_date BETWEEN "2024-01-01" AND "2024-01-31"
   GROUP BY 1'

# Check actual cost of recent queries
SELECT
  job_id,
  total_bytes_billed / POW(1024, 4) AS tb_billed,
  total_slot_ms / 1000 AS slot_seconds
FROM `region-us`.INFORMATION_SCHEMA.JOBS_BY_PROJECT
WHERE creation_time > TIMESTAMP_SUB(CURRENT_TIMESTAMP(), INTERVAL 1 DAY)
ORDER BY total_bytes_billed DESC
LIMIT 10;
```

# Decision rules

- Set `require_partition_filter = true` on large tables — prevents full-table scans.
- Partition by date for time-series data; by integer range for non-temporal data.
- Cluster by up to 4 columns in order of filter frequency.
- Use `APPROX_` functions for dashboards; exact aggregates for financial data.
- Prefer `MERGE` over `DELETE + INSERT` for upserts — single-pass atomic operation.

# References

- https://cloud.google.com/bigquery/docs/best-practices-performance-overview
- https://cloud.google.com/bigquery/pricing

# Related skills

- `orm-patterns` — OLTP database patterns
