---
name: artifact-contracts
description: >
  Trigger — "define artifact contract", "schema for agent handoff", "what should stage X produce",
  "input/output contract", "validate agent output shape", "contract between agents".
  Skip — single-agent tasks with no handoff, pure code generation with no multi-stage pipeline,
  or when the artifact format is already fully specified in an existing contract file.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: artifact-contracts
  maturity: draft
  risk: low
  tags: [artifact, contracts, schema, multi-agent, handoff]
---

# Purpose

Define explicit input/output schemas for artifacts passed between agents or pipeline stages.
Every producer knows exactly what shape to emit; every consumer knows exactly what shape to expect.
Eliminates silent data-loss and schema drift across multi-agent workflows.

# When to use

- A multi-agent workflow hands data from one stage to another and the schema is undocumented.
- An agent's output is consumed by a downstream agent and validation is missing.
- Contract versioning is needed because the artifact shape is evolving across iterations.
- A new pipeline stage is being added and its expected inputs/outputs must be specified.

# Do NOT use when

- The task is a single-agent, single-step operation with no handoff.
- An existing contract file already fully covers the artifact shape (update it instead).
- The work is pure prose generation where structured schemas add no value.

# Operating procedure

1. Run `grep -r "contract" --include="*.md" --include="*.json" --include="*.yaml"` in the repo to find existing contract definitions.
2. List every pipeline stage involved in the workflow in a numbered table: `| # | Stage | Producer Agent | Consumer Agent |`.
3. For each stage boundary, write a JSON Schema or TypeScript interface defining the artifact with required fields, types, and descriptions.
4. Add a `version` field (semver string) to every contract schema — start at `1.0.0` for new contracts.
5. Define validation rules: list which fields are required vs. optional, acceptable value ranges, and enum constraints.
6. Write a `validate_artifact(data, schema)` pseudocode block showing how the consumer should reject malformed input.
7. Create a producer/consumer pairing table: `| Contract Name | Producer | Consumer | Version | Breaking Change Policy |`.
8. Add a `_contract_meta` envelope to each artifact specifying `schema_version`, `produced_by`, `produced_at` (ISO-8601), and `checksum` (SHA-256 of payload).
9. Run a dry-run validation: pick one real or sample artifact, apply the schema, and list any violations found.
10. Write the final contract files to `contracts/` (or the repo's equivalent directory), one file per stage boundary.

# Decision rules

- If a field is consumed by any downstream agent, it is `required` — never silently optional.
- If two producers emit the same artifact type, unify into one schema and version it; do not fork.
- Breaking changes (removing fields, narrowing types) require a major version bump and a migration note.
- Non-breaking changes (adding optional fields) require a minor version bump.
- If a consumer receives an artifact that fails validation, it must halt and return a structured error — never silently drop fields.

# Output requirements

1. **Contract Schema** — one JSON Schema or TypeScript interface per stage boundary.
2. **Pairing Table** — maps every producer to every consumer with version and change policy.
3. **Validation Report** — dry-run results showing pass/fail per field for a sample artifact.
4. **Migration Notes** — for any version bump, a list of what changed and how consumers should adapt.

# References

- `references/delegate-contracts.md` — patterns for structuring delegation handoff.
- `references/checkpoint-rules.md` — when to checkpoint before/after artifact production.
- JSON Schema specification (https://json-schema.org/) for schema authoring.

# Related skills

- `autonomous-run-control` — contracts govern what a run must produce before it can terminate.
- `collaboration-checkpoints` — checkpoints often trigger contract validation.
- `goal-decomposition` — decomposed tasks need contracts at each task boundary.
- `verification-before-advance` — verification uses contracts as the acceptance criteria.

# Failure handling

- **Missing contract**: If no contract exists for a stage boundary, create one before allowing data to flow — never infer schema from a single sample.
- **Schema mismatch**: If a produced artifact fails validation, block the pipeline and emit a diff showing expected vs. actual shape.
- **Version conflict**: If producer and consumer expect different versions, halt and surface both versions for reconciliation.
- **Ambiguous ownership**: If no single agent owns a contract, assign ownership explicitly before proceeding.
