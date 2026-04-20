---
name: api-contracts
description: Review or change HTTP API contracts with explicit compatibility rules, error envelopes, and breaking-change analysis. Use this when editing request or response schemas, status codes, OpenAPI surfaces, or versioned client behavior. Do not use for framework-specific implementation details that are better handled by a runtime skill like FastAPI or Express.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: api-contracts
  maturity: draft
  risk: low
  tags: [api, contracts, schema, compatibility]
---

# Purpose

Use this skill to make contract changes reviewable before they ship as accidental client breakage.

# When to use this skill

Use this skill when:

- changing request fields, response fields, status codes, pagination shape, or error bodies
- reviewing whether an API diff is additive, breaking, or only internal
- auditing a service that has undocumented contract drift between handlers, schema docs, and actual responses

# Do not use this skill when

- the work is mostly about one framework's handler wiring rather than the public contract
- the task is internal data modeling with no client-visible API boundary
- a narrower active skill already owns the problem, such as realtime event streaming or webhook signature verification

# Operating procedure

1. Inventory the boundary surface.
   List affected endpoints, methods, request models, response shapes, status codes, and any published schema or SDK surface tied to them.

2. Classify the proposed change.
   Separate additive changes from breaking ones. Removed fields, newly required fields, semantic meaning changes, and status-code swaps are all compatibility events. Example: adding an optional response field is additive; making a previously optional request field required is breaking.

3. Normalize the error contract.
   Make error bodies and failure codes predictable enough that clients can branch on them without scraping ad hoc strings.

4. Check migration impact.
   Identify which clients, jobs, webhooks, or generated SDKs rely on the changed surface. Decide whether the change needs versioning, deprecation, dual-write, or compatibility shims.

5. Verify the contract mechanically.
   Diff the schema or route inventory where possible, then confirm with a narrow integration test or golden response fixture.

# Decision rules

- Treat a field becoming required as a breaking change unless the old path is still accepted.
- Prefer additive evolution over silent semantic reinterpretation of existing fields.
- Keep pagination, filtering, and sort semantics explicit; hidden defaults become accidental contract.
- If the code and published schema disagree, the shipped behavior wins until the mismatch is corrected and communicated.

# Output requirements

1. `Boundary Inventory`
2. `Compatibility Risks`
3. `Contract Changes`
4. `Verification Plan`

# Scripts

- `scripts/contract_diff.py`: compare two OpenAPI or JSON schema files and flag likely breaking changes.

# References

Read these only when relevant:

- `references/breaking-change-checklist.md`
- `references/error-envelope-patterns.md`
- `references/versioning-and-compatibility.md`

# Related skills

- `fastapi`
- `api-debugging`
- `data-model`

# Anti-patterns

- Versioning via URL path without a migration plan for existing clients.
- Undocumented breaking changes shipped in patch releases.
- Error codes returned as free-form string messages instead of machine-readable codes.
- Pagination without total count or cursor stability guarantees.
- Inconsistent field naming across endpoints (camelCase vs snake_case mix).

# Failure handling

- If there is no trustworthy schema file, derive the boundary from tests, handlers, and captured examples before proposing compatibility claims.
- If the team intentionally ships undocumented behavior, name that explicitly instead of pretending the contract is cleaner than it is.
- If the real issue is runtime implementation rather than contract shape, hand off to the relevant framework skill.
