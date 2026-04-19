---
name: parallel-lane-safety
description: >
  Enforces safety guardrails for parallel agent work — prevents write conflicts, ensures
  file-level isolation, and detects interference between concurrent agent lanes.
  Trigger — "agents writing to same file", "parallel conflict", "write collision",
  "lane isolation", "merge safety", "concurrent agent work", "file lock scope",
  "can these agents run in parallel".
  Skip — sequential single-agent work, read-only analysis tasks, tasks where only one agent writes.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: parallel-lane-safety
  maturity: draft
  risk: low
  tags: [parallel, lane, safety, isolation, conflict, merge]
---

# Purpose

Before launching agents to work in parallel, verify that their write scopes do not overlap,
define explicit lane boundaries, and establish conflict detection rules — so that concurrent
agent work can be merged cleanly without data loss, silent overwrites, or corrupted artifacts.

# When to use

- Two or more agents are about to run concurrently and at least one writes files.
- A merge or integration step is approaching after parallel work branches.
- A previous parallel run resulted in conflicting edits, lost changes, or broken builds.
- The orchestrator needs to decide which tasks can safely parallelize and which must serialize.
- Splitting a ticket or task into parallel lanes and need to define boundaries.

# Do NOT use when

- All concurrent agents are read-only (no write conflicts possible).
- Only one agent is running at a time (sequential execution).
- The task is debugging a failure that already happened (use `multi-agent-debugging`).
- The concern is agent performance, not safety (use watchdog or monitoring skills).

# Operating procedure

1. **Enumerate all active agents.** List every agent that will run concurrently. Output as:
   `| Agent | Task Description | Mode (read/write) | Status |`.
2. **Declare write scopes.** For each writing agent, list every file path, directory, database
   table, or API endpoint it may modify. Use glob patterns where appropriate.
   Output as: `| Agent | Write Scope (glob) | Scope Type (file/db/api) |`.
3. **Build the conflict matrix.** Cross-reference write scopes pairwise. For each pair of
   writing agents, check if any glob patterns overlap. Output as:
   `| Agent A | Agent B | Overlapping Scope | Conflict? |`.
4. **Classify each conflict.** For every overlap found, classify as:
   - HARD_CONFLICT — both agents write to the exact same file/line range. Must serialize.
   - SOFT_CONFLICT — agents write to the same directory but different files. Can parallelize
     with a post-merge verification step.
   - NO_CONFLICT — scopes are fully disjoint.
5. **Assign lane boundaries.** For each agent, write a lane specification:
   `| Agent | Allowed Write Paths | Forbidden Write Paths | Shared Read Paths |`.
   Every file must appear in exactly one agent allowed-write lane or be read-only for all.
6. **Define merge-safety checks.** For SOFT_CONFLICT lanes, specify a post-merge verification:
   - Run `git diff --name-only` between the two branches to confirm no unexpected overlaps.
   - Run the project build and test suite after merge.
   - Check file checksums for any shared config files.
7. **Establish runtime conflict detection.** Define what happens if an agent violates its lane:
   - Log the violation with: agent ID, file path, timestamp, operation attempted.
   - Halt the violating agent immediately (do not allow the write to proceed).
   - Notify the orchestrator to decide: retry with corrected scope, serialize, or abort.
8. **Emit the lane assignment plan.** Produce the final document with all lane boundaries,
   conflict classifications, and merge-safety rules.
9. **Validate isolation post-run.** After all agents complete, run a final check:
   - Verify no file was modified by more than one agent (check git blame or file timestamps).
   - Confirm build and tests pass on the merged result.

# Decision rules

- If any HARD_CONFLICT exists, those agents MUST serialize — no exceptions.
- SOFT_CONFLICTs are acceptable only if post-merge verification is defined and will be run.
- Shared configuration files (package.json, Cargo.toml, etc.) require explicit lock assignment
  to exactly one agent; others must treat them as read-only.
- When in doubt, serialize. The cost of a lost write is always higher than the cost of waiting.
- Lane boundaries are immutable once agents launch — no mid-run scope expansion.

# Output requirements

1. **Agent Inventory** — table of all concurrent agents and their modes.
2. **Write Scope Declarations** — per-agent list of all paths they will modify.
3. **Conflict Matrix** — pairwise comparison with conflict classification.
4. **Lane Assignment Plan** — per-agent allowed/forbidden write paths.
5. **Merge Safety Rules** — post-merge verification steps for each soft conflict.
6. **Violation Protocol** — what happens if an agent writes outside its lane.

# References

- `references/delegate-contracts.md` — write-scope clauses in delegation contracts.
- `references/checkpoint-rules.md` — checkpoints before and after parallel execution.
- `references/failure-escalation.md` — escalation when a lane violation is detected.

# Related skills

- `manager-hierarchy-design` — for structuring who assigns and enforces lanes.
- `multi-agent-debugging` — for post-mortem when parallel work produced conflicts.
- `panel-of-experts` — for parallel read-only analysis that does not need lane safety.
- `long-run-watchdog` — for monitoring agents that may exceed their lane due to stalls.

# Failure handling

- If an agent cannot declare its write scope upfront, refuse to parallelize it — serialize
  until the scope is known.
- If a lane violation is detected mid-run, halt only the violating agent, not the entire system.
- If post-merge verification fails, identify the conflicting files, roll back the later write,
  and re-run that agent with a corrected scope.
- If two agents both legitimately need to write the same file, escalate to the orchestrator
  to redesign the task split rather than allowing concurrent writes.
