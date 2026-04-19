---
name: circular-references-example
description: >
  Analyzes inter-service message contracts to detect breaking changes
  before deployment, generating compatibility reports and migration guides.
---

# Message Contract Analyzer

## Purpose

Detect breaking changes in inter-service message schemas (Protobuf, Avro,
JSON Schema) before they reach production. Compares the proposed schema
version against the currently deployed version, classifies each difference
as compatible or breaking, and produces a migration guide for any breaking
changes that have been approved via the exception process.

## When to use

- A PR modifies a `.proto`, `.avsc`, or `.schema.json` file in the
  shared contracts repository.
- Before releasing a new version of a service that produces or consumes
  shared messages.
- During quarterly contract audits to verify no undocumented drift.

## When NOT to use

- For REST API contract testing — use `api-contract-validator` instead.
- For generating client code from schemas — use `schema-codegen` instead.
- For monitoring runtime deserialization errors — use `runtime-monitor`.

## Procedure

1. Load the current (deployed) schema from `contracts/deployed/<service>/`
   and the proposed schema from the PR's changed files.

2. Parse both schemas into a normalized intermediate representation (IR)
   that captures fields, types, nesting, and metadata independent of the
   schema format.

3. Compute a field-level diff between the deployed IR and the proposed IR.
   Classify each change:
   - **Compatible**: adding an optional field, widening a type (int32 →
     int64), adding a new enum value at the end.
   - **Breaking**: removing a field, renaming a field, narrowing a type,
     changing a field number (Protobuf), reordering enum values (Avro).

4. For each breaking change, check the exception register at
   `contracts/exceptions.yaml` to see if it has been pre-approved. If
   approved, include it in the migration guide. If not approved, flag it
   as a blocking issue.

5. Generate the compatibility report with a per-field breakdown and an
   overall verdict (COMPATIBLE, BREAKING_APPROVED, BREAKING_BLOCKED).

6. For approved breaking changes, generate a migration guide that includes:
   - Consumer-side code changes needed.
   - Recommended deployment order (producer first vs. consumer first).
   - Rollback procedure if the migration fails.

7. Post the report as a PR comment and set the check status accordingly.

## Output contract

- `compatibility-report.md` — Field-level diff with classifications.
- `migration-guide.md` — Generated only when approved breaking changes
  exist.
- PR check status: `success` if COMPATIBLE or BREAKING_APPROVED;
  `failure` if BREAKING_BLOCKED.

## Failure handling

- If schema parsing fails, report the parse error with file path and
  line number. Do not attempt partial analysis.
- If `contracts/deployed/` has no baseline for a service, treat all
  fields as new (compatible) and note that this is a first-version
  analysis.
- If the exception register is malformed, block the PR and request a
  fix to `exceptions.yaml` before re-running.

## Next steps

- Breaking changes flagged here should be resolved using
  `schema-evolution-planner`, which designs backward-compatible
  migration paths.
- After contract changes are deployed, run `runtime-compatibility-checker`
  to verify that all consumers handle the new schema correctly.
- `runtime-compatibility-checker` feeds results back into
  `message-contract-analyzer` (this skill) to update the deployed
  baseline and close the verification loop.
