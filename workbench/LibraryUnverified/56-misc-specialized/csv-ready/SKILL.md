---
name: csv-ready
description: "Generate well-formed CSV files with correct encoding, delimiter selection, field escaping, consistent headers, and streaming for large datasets. Use when producing CSV output, converting data to CSV format, fixing CSV encoding issues, or validating CSV structure. Do not use for Excel-specific features (prefer xlsx-generation) or parsing CSV input from external sources."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: csv-ready
  maturity: draft
  risk: low
  tags: [csv, encoding, delimiter, escaping, data-export]
---

# Purpose

Generate well-formed CSV files with correct encoding, proper delimiter selection, robust field escaping, consistent header naming conventions, and streaming support for large datasets. This skill ensures CSV output is portable across Excel, Google Sheets, database import tools, and downstream data pipelines.

# When to use this skill

- The task requires producing a `.csv` file from structured or semi-structured data
- Data must be exported in a tabular format consumable by spreadsheets or ETL pipelines
- CSV encoding issues need diagnosis or repair (BOM, UTF-8 vs Latin-1, mojibake)
- Header normalization is needed (snake_case, deduplication, type suffixes)
- A large dataset (>100K rows) needs streamed output instead of in-memory generation
- Existing CSV output has escaping bugs (unquoted commas, embedded newlines, bare quotes)

# Do not use this skill when

- The output requires Excel-specific features like formulas, cell formatting, or multiple sheets — prefer `xlsx-generation`
- The task is parsing or reading CSV input from external sources rather than generating output
- The data target is JSON, Parquet, or another non-CSV structured format
- Image or binary data is involved in the output

# Operating procedure

1. **Identify the target consumer** — determine whether the CSV will be consumed by Excel, a database COPY command, a Python pandas pipeline, or a web download. This drives encoding and delimiter choices.
2. **Select encoding** — default to UTF-8 with BOM (`utf-8-sig`) for Excel compatibility. Use plain UTF-8 for programmatic consumers. Use Latin-1 only when explicitly required by a legacy system.
3. **Choose the delimiter** — use comma (`,`) as default. Switch to tab (`\t`) for fields that frequently contain commas (addresses, descriptions). Switch to pipe (`|`) only for legacy mainframe integrations. Document the choice in the file header or metadata.
4. **Normalize headers** — convert all column names to `snake_case`, strip leading/trailing whitespace, replace spaces and special characters with underscores, and deduplicate by appending `_2`, `_3` suffixes.
5. **Apply field escaping** — quote any field containing the delimiter character, double-quote characters, or newlines. Use RFC 4180 double-quote escaping (embed `""` for literal quotes). Never use backslash escaping.
6. **Handle null and empty values** — represent SQL NULL as an empty unquoted field. Represent an actual empty string as `""`. Document the convention in output metadata.
7. **Validate row consistency** — confirm every row has exactly the same number of fields as the header row. Reject or quarantine rows with field-count mismatches and log the line numbers.
8. **Stream large datasets** — for datasets exceeding 50K rows, write rows incrementally using `csv.writer` (Python), a streaming writer, or chunked output. Never build the entire file in memory.
9. **Add line-ending consistency** — use CRLF (`\r\n`) for Windows/Excel targets, LF (`\n`) for Unix pipelines. Do not mix line endings within a single file.
10. **Validate the output** — open the generated file in a secondary parser (e.g., `csv.reader` round-trip) to confirm field counts, encoding, and escaping are correct. Spot-check the first 5 and last 5 rows.
11. **Document the file** — include a sidecar `.meta` or inline comment row (if supported by the consumer) specifying: encoding, delimiter, null convention, row count, and generation timestamp.

# Decision rules

- When in doubt about delimiter, default to comma with proper quoting over switching delimiters.
- Prefer RFC 4180 compliance over custom escaping schemes.
- If the consumer is unknown, produce UTF-8-BOM + comma + CRLF as the safest combination.
- If any field contains binary data or multi-line HTML, flag it for a different format (JSON or XLSX) rather than forcing it into CSV.
- For datasets with >50 columns, consider whether CSV is the right format — suggest Parquet or XLSX if column count makes CSV unwieldy.

# Output requirements

1. **CSV file** — well-formed file passing RFC 4180 validation
2. **Header row** — normalized snake_case column names as the first row
3. **Encoding declaration** — explicit encoding stated in filename convention or sidecar metadata
4. **Row count summary** — total rows written (excluding header), logged to stdout or metadata
5. **Validation report** — confirmation that round-trip parsing succeeded with zero field-count errors

# References

- RFC 4180: Common Format and MIME Type for CSV Files — https://tools.ietf.org/html/rfc4180
- Python `csv` module documentation — https://docs.python.org/3/library/csv.html
- W3C CSV on the Web recommendations — https://www.w3.org/TR/tabular-data-primer/

# Related skills

- `xlsx-generation` — when Excel-specific features (formulas, formatting, sheets) are needed
- `table-extraction` — when source data comes from HTML or PDF tables
- `document-to-structured-data` — when upstream input is unstructured text or documents

# Anti-patterns

- **Skipping quoting because "the data looks clean"** — always apply RFC 4180 quoting rules regardless of apparent data content.
- **Using `str.join(",")` instead of a CSV library** — this silently breaks on fields containing commas, quotes, or newlines.
- **Mixing encodings** — concatenating UTF-8 and Latin-1 sources without re-encoding produces mojibake.
- **Hardcoding LF line endings for Excel consumers** — Excel on Windows requires CRLF for reliable column detection.
- **Omitting BOM for UTF-8 Excel files** — without BOM, Excel may misinterpret UTF-8 as ANSI encoding.

# Failure handling

- If encoding detection fails, default to UTF-8 and log a warning listing the byte sequences that triggered the failure.
- If a row has a field-count mismatch, quarantine the row to a separate `.errors.csv` file with the original line number and write valid rows to the primary output.
- If the dataset exceeds available memory during generation, switch to chunked streaming and retry.
- If the target consumer rejects the file, inspect delimiter detection by opening in a hex editor and checking for mixed line endings or BOM issues.
- If the task requires features beyond CSV capabilities (merged cells, formulas), redirect to `xlsx-generation`.
