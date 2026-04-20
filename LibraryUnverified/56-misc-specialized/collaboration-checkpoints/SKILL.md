---
name: collaboration-checkpoints
description: >
  Trigger — "sync agents before merging", "add checkpoint between stages", "resolve merge conflict
  between agents", "fan-out fan-in pattern", "reconcile parallel agent work", "pause agents to align".
  Skip — when only one agent is running, when the task has no parallel branches, or when agents
  operate on completely independent file sets with no shared state.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: collaboration-checkpoints
  maturity: draft
  risk: low
  tags: [collaboration, checkpoints, sync, merge, parallel-agents]
---

# Purpose

Define synchronization points where parallel agents pause, share state, resolve conflicts,
and align before continuing. Prevents divergent work, merge conflicts, and duplicated effort
in multi-agent workflows operating on shared resources.

# When to use

- Two or more agents are working in parallel on a shared codebase or document set.
- A fan-out/fan-in pattern requires collecting results from multiple agents before proceeding.
- Agents have modified overlapping files and their changes must be reconciled.
- A workflow stage depends on the combined output of multiple predecessor stages.
- Agents need to share discoveries (e.g., "file X was moved") that affect each other's work.

# Do NOT use when

- Only one agent is executing — single-agent runs use `autonomous-run-control` checkpoints instead.
- Parallel agents operate on completely disjoint file sets with zero shared state.
- The workflow is strictly sequential with no concurrent execution.

# Operating procedure

1. List all active agents and their assigned scopes in a table: `| Agent ID | Scope (files/dirs) | Current Step | Status |`.
2. Identify **overlap zones**: run `comm <(sort agent_a_files.txt) <(sort agent_b_files.txt)` (or equivalent) to find files touched by multiple agents.
3. Define **checkpoint trigger conditions** — a checkpoint fires when any of: (a) an agent completes a defined stage, (b) wall-clock interval of 120 seconds elapses, (c) an agent modifies a file in another agent's scope, (d) an agent's output is needed as input by another agent.
4. At each checkpoint, every agent must emit a **checkpoint payload**: `{ "agent_id": "<id>", "step": <n>, "files_modified": [...], "files_read": [...], "state_hash": "<sha256>", "blockers": [...] }`.
5. Run **conflict detection**: compare `files_modified` across all agent payloads. If two agents modified the same file, flag it as a conflict.
6. For each conflict, apply **resolution protocol**: (a) diff both versions against the common ancestor, (b) if changes are in non-overlapping regions, auto-merge, (c) if changes overlap, halt both agents and present the conflict to the orchestrator or human.
7. Run **state reconciliation**: merge non-conflicting changes into a unified working tree. Verify the merged state by running `git diff --check` and any available linter.
8. Update each agent's context with discoveries from other agents: list new files, renamed files, deleted files, and changed interfaces.
9. Emit a **checkpoint summary**: `| Checkpoint # | Agents Synced | Conflicts Found | Conflicts Resolved | Merged Files | Remaining Blockers |`.
10. Release agents to resume work only after all conflicts are resolved and the merged state passes validation.

# Decision rules

- Never allow two agents to proceed past a checkpoint if they have unresolved file conflicts.
- Auto-merge is permitted only when changes are in non-overlapping line ranges of the same file.
- If an agent has been waiting at a checkpoint for >60 seconds for another agent, escalate to the orchestrator.
- Fan-in requires ALL branches to reach the checkpoint — do not proceed with partial results unless explicitly configured for best-effort mode.
- Checkpoint payloads must be immutable once emitted — agents cannot retroactively modify their reported state.

# Output requirements

1. **Agent Scope Table** — all agents, their scopes, and current status at checkpoint time.
2. **Conflict Report** — files with overlapping modifications, diff excerpts, and resolution outcome.
3. **Checkpoint Summary** — per-checkpoint metrics (agents synced, conflicts found/resolved, blockers).
4. **Merged State Validation** — result of linter/test run on the post-merge working tree.
5. **Resume Instructions** — per-agent list of updated context and next steps after checkpoint.

# References

- `references/checkpoint-rules.md` — checkpoint format and persistence requirements.
- `references/delegate-contracts.md` — contracts define what each agent should produce at checkpoints.
- `references/failure-escalation.md` — escalation when checkpoint conflicts cannot be auto-resolved.

# Related skills

- `autonomous-run-control` — single-agent checkpoints are a subset of collaboration checkpoints.
- `artifact-contracts` — contracts define the expected shape of checkpoint payloads.
- `human-interrupt-handling` — unresolvable conflicts may trigger a human interrupt.
- `parallel-lane-safety` — ensures parallel agents stay within their designated lanes.

# Failure handling

- **Agent timeout at checkpoint**: If an agent does not emit a checkpoint payload within the expected window, mark it `stalled` and proceed with available payloads after logging the gap.
- **Merge failure**: If auto-merge produces invalid code (linter/test failure), revert to pre-checkpoint state and re-assign the conflicting file to a single agent.
- **State hash mismatch**: If an agent's reported state hash does not match the actual file system, force a full re-sync from the merged working tree.
- **Deadlock**: If two agents are each waiting on the other's checkpoint, break the cycle by checkpointing the agent with less uncommitted work first.
