---
name: process-versioning
description: >
  Versions agent workflows — tracks changes to prompts, procedures, tool configurations,
  and skill definitions across iterations with semantic versioning and migration paths.
  Trigger — "version this workflow", "track prompt changes", "workflow changelog",
  "compare workflow versions", "migrate to new process", "A/B test workflows",
  "rollback workflow", "what changed in the process".
  Skip — one-off ad hoc tasks with no reuse intent, versioning application code (use git directly),
  runtime debugging of agent failures.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: process-versioning
  maturity: draft
  risk: low
  tags: [process, versioning, changelog, migration, workflow]
---

# Purpose

Maintain a versioned history of agent workflows — including prompts, operating procedures,
tool configurations, and skill compositions — so that changes can be tracked, compared,
rolled back, and A/B tested across iterations without losing prior working configurations.

# When to use

- A workflow, prompt, or procedure is being modified and the prior version must be preserved.
- The team wants to compare the performance of two workflow versions side by side.
- A workflow regression occurred and you need to identify what changed between versions.
- Migrating an agent team from one process version to another with backward compatibility.
- Auditing the evolution of a skill or procedure over time for compliance or learning purposes.

# Do NOT use when

- Versioning application source code — use git directly.
- The workflow is brand new with no prior version to track (just create v1.0.0).
- Debugging a runtime failure in the current workflow (use `multi-agent-debugging`).
- The change is trivial whitespace or formatting with no semantic impact.

# Operating procedure

1. **Inventory the current workflow state.** List every artifact that constitutes the workflow:
   prompts, SKILL.md files, tool configs, agent definitions, delegation contracts.
   Output as: `| Artifact | Path | Current Version | Last Modified |`.
2. **Capture the current version snapshot.** For each artifact, record its content hash (SHA-256
   of file contents). Store the snapshot with a version label following semver:
   - MAJOR: breaking changes to inputs, outputs, or agent roles.
   - MINOR: new capabilities added, backward compatible.
   - PATCH: bug fixes, wording improvements, no behavior change.
3. **Diff the proposed changes.** For each artifact being modified, run a diff between the
   current version and the proposed version. Summarize changes in a table:
   `| Artifact | Change Type (major/minor/patch) | Description of Change |`.
4. **Determine the new version number.** Apply semver rules: the highest change type among
   all artifacts determines the version bump. If any artifact has a MAJOR change, the whole
   workflow gets a MAJOR bump.
5. **Write the changelog entry.** Create a structured entry:
   ```
   ## vX.Y.Z — YYYY-MM-DD
   ### Changed
   - [artifact]: description of what changed and why
   ### Migration
   - Steps required to move from vX.Y.(Z-1) to vX.Y.Z
   ### Compatibility
   - Backward compatible: yes/no
   - Agents affected: list
   ```
6. **Define the migration path.** If the change is MAJOR, write explicit migration steps:
   which prompts to update, which tool configs to modify, which agent roles change.
   If MINOR or PATCH, confirm no migration is needed.
7. **Tag the version.** Create a version tag in the version registry (a JSON or YAML manifest
   file). Include: version, date, author, content hashes, changelog reference.
8. **Set up A/B testing (if requested).** Define two parallel workflow configurations:
   - Control: current version. Variant: new version.
   - Specify the evaluation metric (success rate, output quality, latency, token cost).
   - Specify the sample size and decision threshold.
9. **Archive the previous version.** Ensure the prior version is fully recoverable — either
   via git tags, a versioned directory structure, or the manifest content hashes.

# Decision rules

- Every workflow change must produce a changelog entry — no undocumented modifications.
- MAJOR version changes require explicit migration documentation before deployment.
- Never delete a prior version; archive it so rollback is always possible.
- A/B tests must have predefined success criteria before starting, not after seeing results.
- If two versions have the same content hash, they are the same version regardless of label.

# Output requirements

1. **Version Manifest** — table of all artifacts with version numbers and content hashes.
2. **Changelog Entry** — structured record of what changed, why, and how to migrate.
3. **Migration Guide** — step-by-step instructions for MAJOR version transitions.
4. **Compatibility Matrix** — which agent versions work with which workflow versions.
5. **A/B Test Plan** (if applicable) — control vs variant, metrics, sample size, threshold.

# References

- `references/delegate-contracts.md` — versioning clauses in delegation contracts.
- `references/checkpoint-rules.md` — checkpoints aligned with version transitions.
- `references/failure-escalation.md` — rollback triggers when a new version degrades performance.

# Related skills

- `multi-agent-debugging` — for diagnosing regressions after a version change.
- `autonomous-backlog-maintenance` — for tracking version-related tasks in the backlog.
- `long-run-watchdog` — for monitoring A/B test runs across versions.
- `manager-hierarchy-design` — for versioning changes that alter the agent hierarchy.

# Failure handling

- If the version manifest is missing or corrupt, reconstruct it from git history by scanning
  file timestamps and commit messages for version-related changes.
- If a migration path is unclear, block the version deployment and request explicit migration
  steps from the author before proceeding.
- If an A/B test produces inconclusive results, extend the sample size by 50% once. If still
  inconclusive, default to the control version and document why.
- If rollback is requested but the prior version artifacts are missing, reconstruct from git
  history using the content hashes in the manifest.
