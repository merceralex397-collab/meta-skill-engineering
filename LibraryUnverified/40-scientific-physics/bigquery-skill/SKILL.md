---
name: bigquery-skill
description: "Google BigQuery specialist for query authoring, schema design, cost optimization, and data pipeline configuration. Triggers: 'BigQuery query', 'BQ cost estimate', 'partition strategy', 'optimize this BQ query', 'BigQuery schema design', 'scheduled query', 'data loading into BigQuery', 'UNNEST', 'wildcard tables'. Do NOT use for general SQL unrelated to BigQuery, Postgres/MySQL tuning, or data warehouse platforms other than BigQuery (Snowflake, Redshift, Databricks)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: bigquery-skill
  maturity: draft
  risk: low
  tags: [bigquery, google-cloud, sql, analytics, data-warehouse, cost-optimization]
---

# Purpose

Domain skill for Google BigQuery — covering query authoring, schema design, cost management, performance optimization, data loading strategies, and operational configuration. Produces production-ready SQL, schema recommendations, and cost-aware optimization guidance grounded in BigQuery's specific architecture (Dremel execution engine, Colossus storage, slot-based processing).

# When to use this skill

Use this skill when:

- writing or reviewing BigQuery Standard SQL queries
- designing or evolving BigQuery table schemas (partitioning, clustering, nested/repeated fields)
- estimating or reducing query costs (on-demand bytes billed or slot utilization)
- configuring data loading (batch load, streaming inserts, Storage Write API)
- setting up scheduled queries, data transfers, authorized views, or materialized views
- troubleshooting BigQuery performance (slot contention, shuffle limits, query plan analysis)
- migrating to BigQuery from other warehouses or from BigQuery Legacy SQL to Standard SQL

# Do not use this skill when

- the task involves a **different data warehouse** (Snowflake, Redshift, Databricks, Synapse) — use the appropriate platform skill
- the SQL is for **PostgreSQL, MySQL, or SQLite** with no BigQuery involvement
- the task is purely about **GCP IAM or networking** without BigQuery-specific configuration
- a quick format conversion or one-liner would suffice — use `misc-helper` instead

# Operating procedure

## 1. Understand the data context

- Identify the dataset, table(s), and their schemas. Check for partitioning (`_PARTITIONTIME`, `_PARTITIONDATE`, or column-based), clustering columns, and nested/repeated fields (RECORD/ARRAY types).
- Determine the pricing model in use: **on-demand** (per-TB scanned) or **flat-rate/editions** (slot-based).
- Note the data volume: row count, total storage size, and typical query scan size.

## 2. Author or optimize the query

**Query patterns to apply:**
- Always filter on the **partition column** first to enable partition pruning. Use `WHERE _PARTITIONTIME BETWEEN ...` or the partitioning column directly.
- Use `UNNEST()` to flatten ARRAY/STRUCT fields before filtering or aggregating.
- For wildcard tables (`project.dataset.events_*`), add `_TABLE_SUFFIX` filters to limit tables scanned.
- Prefer `APPROX_COUNT_DISTINCT()` over `COUNT(DISTINCT ...)` for large cardinality columns (typically 1-2% error, up to 50x faster).
- Select only needed columns — never `SELECT *` on wide or large tables.
- Place the **largest table first** in JOINs so the BigQuery optimizer can broadcast smaller tables.
- Use **CTEs** (WITH clauses) for readability; BigQuery optimizes them into the execution plan.
- Use `SAFE_DIVIDE()`, `SAFE_CAST()`, and `IFNULL()`/`COALESCE()` to handle nulls and division-by-zero without query failure.

**Cost estimation:**
- Run a **dry run** before execution: `bq query --dry_run --use_legacy_sql=false 'SELECT ...'` to get bytes processed estimate.
- Convert bytes to cost: on-demand pricing is $6.25/TB (first 1 TB/month free).
- For flat-rate, cost is fixed per slot-hour; focus on slot utilization and concurrency instead.

## 3. Design or review schema

- **Partitioning strategy**: use time-based partitioning (DAY, HOUR, MONTH, YEAR) for event/log data; integer-range partitioning for ID-sharded data. Max 4,000 partitions per table.
- **Clustering**: choose up to 4 columns, ordered by filter frequency (most filtered first). Clustering is free and improves both performance and cost.
- **Nested/repeated fields**: prefer denormalized schemas with STRUCT and ARRAY over normalized multi-table joins. BigQuery is optimized for wide, denormalized tables.
- **Materialized views**: use for frequently repeated aggregations. BigQuery auto-refreshes them and the optimizer can transparently rewrite queries to use them.

## 4. Configure data loading

- **Batch load** (recommended for most cases): use `bq load` or the Storage Write API in batch mode. Prefer **Avro** format (fastest load, self-describing schema), then **Parquet** (good compression, columnar), then **CSV** (slowest, requires schema definition).
- **Streaming inserts** (`tabledata.insertAll`): use only for real-time ingestion with <1 second latency requirements. Costs $0.05/GB. Data is available immediately but not in the streaming buffer for export for up to 90 minutes.
- **Storage Write API** (recommended over streaming): lower cost, exactly-once semantics, higher throughput. Use the default stream for low-latency or committed streams for transactional guarantees.

## 5. Validate and deliver

- Verify partition pruning in the query plan (INFORMATION_SCHEMA.JOBS or the BQ console execution details).
- Confirm bytes billed matches the dry run estimate (±10%).
- Check for slot contention in `INFORMATION_SCHEMA.JOBS_BY_PROJECT` if using flat-rate.

# Decision rules

- **Always partition**: no production table over 1 GB should be unpartitioned. Default to daily time partitioning unless the access pattern clearly favors another strategy.
- **Always cluster**: if a table has common filter/group-by columns, add clustering. There is no cost, only benefit.
- **Dry run first**: never execute an ad-hoc query on a table >10 GB without a dry run cost estimate.
- **Denormalize over join**: prefer nested/repeated fields over multi-table joins when the data relationship is 1:N and the nested data is always accessed with the parent.
- **Approximate over exact**: for exploratory analytics on >1 billion rows, prefer `APPROX_` functions unless exact counts are a business requirement.
- **Storage Write API over streaming inserts**: for new pipelines, always recommend the Storage Write API unless the team has an existing streaming inserts integration that works.
- **Avoid cross-region queries**: if data is in `US` and compute is in `EU`, flag the cost and latency implications.

# Output requirements

Every response must include the applicable sections:

1. **`Query`** — the complete, runnable BigQuery Standard SQL with comments explaining partition pruning, clustering usage, and any cost-relevant choices.
2. **`Cost Estimate`** — estimated bytes scanned and dollar cost (on-demand) or slot-seconds (flat-rate). Include the dry run command to verify.
3. **`Schema Recommendation`** (if schema changes are involved) — DDL statement with partitioning, clustering, and field type choices annotated.
4. **`Optimization Notes`** — bullet list of what was optimized and why (e.g., "Added `_PARTITIONDATE` filter to reduce scan from 2.1 TB to 45 GB").
5. **`Warnings`** (if any) — cost risks, missing partition filters, or deprecated patterns detected.

# Anti-patterns

- **`SELECT *` on large tables**: scans every column; always specify needed columns explicitly. On a 10-column, 5 TB table, selecting 2 columns reduces cost by ~80%.
- **Unpartitioned date-filtered queries**: filtering on a DATE column without partitioning forces a full table scan. Partition the table or use a partitioned view.
- **Streaming inserts for batch workloads**: using `tabledata.insertAll` for nightly ETL jobs wastes money ($0.05/GB vs free for batch loads) and lacks exactly-once guarantees.
- **JOINing before filtering**: apply WHERE clauses before JOINs to reduce shuffle volume. BigQuery's optimizer helps, but explicit pre-filtering is more reliable.
- **Ignoring clustering order**: clustering columns are hierarchical (like a compound index). Putting a high-cardinality column first when the low-cardinality column is filtered more often wastes the clustering benefit.
- **Overusing `ORDER BY` without `LIMIT`**: sorting the full result set of a multi-TB query is expensive and usually unnecessary. Always pair `ORDER BY` with `LIMIT` unless the consumer truly needs sorted output.
- **Legacy SQL**: any query starting with `#legacySQL` or using bracket syntax (`[project:dataset.table]`) should be migrated to Standard SQL.

# Related skills

- `fastapi-patterns` — for building API layers that query BigQuery
- `misc-helper` — for quick data format conversions before/after BigQuery operations
- `tauri-solidjs` — for desktop dashboards consuming BigQuery data

# Failure handling

- If the table schema is unknown, query `INFORMATION_SCHEMA.COLUMNS` or `INFORMATION_SCHEMA.TABLE_OPTIONS` to discover partitioning, clustering, and field types before proceeding.
- If a dry run returns an unexpectedly high bytes estimate, investigate: check for missing partition filters, unnecessary columns, or wildcard table suffix filters.
- If a query fails with "Resources exceeded", simplify the query (break into stages with temp tables), reduce JOIN fan-out, or add `LIMIT` for iterative development.
- If the pricing model is unknown, assume on-demand and flag that slot-based pricing should be verified with the team.
