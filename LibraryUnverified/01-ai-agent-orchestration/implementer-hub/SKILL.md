---
name: implementer-hub
description: >
  Trigger — "implement across multiple files", "coordinate implementation", "split work across modules",
  "implement this plan", "orchestrate the changes", "multi-file implementation".
  Skip — if only a single file needs changes (use `implementer-node-agent`), if context is not
  yet loaded (use `implementer-context` first), or if the plan does not exist yet (use `planner`).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: implementer-hub
  maturity: draft
  risk: low
  tags: [implementer, hub]
---

# Purpose

Coordinates multi-file and multi-module implementation by partitioning work, sequencing
changes to avoid merge conflicts, managing cross-cutting concerns, and routing individual
file changes to the appropriate implementer agents.

# When to use

- An approved plan requires changes across 3+ files or 2+ modules.
- Multiple implementer agents need to work on the same codebase without conflicts.
- Cross-cutting concerns (e.g., error handling, logging, type changes) affect multiple modules.
- A ticket's implementation order matters (e.g., types first, then services, then routes).

# Do NOT use when

- Only a single file needs modification — route directly to `implementer-node-agent`.
- No implementation plan exists yet — use `planner` first.
- Context has not been gathered — use `implementer-context` first.
- The task is testing or QA — use `qa-validation`.

# Operating procedure

1. Read the implementation plan and extract the full list of files to be created or modified.
2. Run `git --no-pager diff --name-only HEAD~5` to check for in-flight changes that might conflict with the plan.
3. Group the file list into modules by directory (e.g., `src/auth/`, `src/api/`, `src/db/`). Create a module partition table: | Module | Files | Dependencies | Priority |.
4. Identify cross-cutting concerns by searching for shared types or interfaces: run `grep -rn 'export type\|export interface' <target-dirs> | head -30` to find shared contracts.
5. Build a dependency-ordered implementation sequence: types/interfaces → data layer → service layer → API/route layer → tests. Record this as a numbered step list.
6. For each step in the sequence, identify which files change and whether they can be parallelized (no shared imports between them) or must be serial (one imports from another).
7. Dispatch each work unit to the appropriate implementer agent with: file path, planned changes, relevant context from the context bundle, and the conventions sheet.
8. After each implementer agent completes, run `git --no-pager diff --stat` to verify only the expected files were modified.
9. Run the project's existing lint command (extract from `package.json` scripts or `Makefile`) to catch convention violations across all changed files.
10. Run the project's test suite with `npm test`, `pytest`, `cargo test`, or equivalent. Record pass/fail counts.
11. If any tests fail, identify the failing test, map it back to the responsible implementer change, and re-dispatch a fix to that specific agent.
12. Compile a completion report with the full list of changes, test results, and any remaining issues.

# Decision rules

- Never dispatch two agents to modify the same file simultaneously — serialize those changes.
- If a type definition changes, all downstream consumers must be updated in the same batch before tests run.
- If a new dependency is needed, add it to the manifest first (step it before any code that imports it).
- If the plan has >15 file changes, split into 2–3 sub-batches and run the test suite between batches.
- If an agent reports a conflict with existing code not in the plan, pause and escalate to the user.

# Output requirements

1. **Module Partition Table** — columns: Module, Files, Dependency Count, Execution Priority.
2. **Implementation Sequence** — numbered list showing the order of file changes with parallelization notes.
3. **Dispatch Log** — for each dispatched unit: agent, file(s), status (pending/done/failed), and duration.
4. **Conflict Check** — list of any merge conflicts or unexpected file modifications detected.
5. **Test Results** — pass/fail count, failing test names, and which change caused each failure.
6. **Completion Summary** — total files changed, lines added/removed, and outstanding issues.

# References

- `references/handoff-contract.md` — inter-agent handoff format
- `references/anti-patterns.md` — common multi-agent coordination failures
- `references/success-criteria.md` — completion verification standards

# Related skills

- `implementer-context` — provides the context bundles consumed by this skill
- `implementer-node-agent` — executes individual file-level changes
- `planner` — produces the plans this skill orchestrates
- `qa-validation` — validates the combined output of all implementer agents

# Failure handling

- If an implementer agent fails on a file, retry once with additional context. If it fails again, mark that file as blocked and continue with independent files.
- If the test suite fails after a batch, run `git stash` on the batch changes, re-run tests to confirm the baseline passes, then `git stash pop` and bisect the failure.
- If circular dependencies between modules prevent clean ordering, identify the cycle and break it by extracting shared types into a common module first.
- If the plan references files that do not exist and are not marked as "create", flag the discrepancy and ask for plan clarification before proceeding.
