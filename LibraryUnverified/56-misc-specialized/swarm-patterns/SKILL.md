---
name: swarm-patterns
description: >
  Trigger — "run multiple agents in parallel on this", "scatter-gather across
  files", "map-reduce the codebase", "fan out agents", "coordinate a swarm",
  "blackboard pattern".
  Skip — only one agent is needed; task is sequential with hard data
  dependencies between each step; total work items < 3 (use simple
  delegation instead).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: swarm-patterns
  maturity: draft
  risk: low
  tags: [swarm, parallel, map-reduce, scatter-gather, coordination]
---

# Purpose

Provides coordination patterns for running many agents against a shared
goal with bounded overlap — including map/reduce, scatter-gather, and
blackboard architectures — so results converge without conflicts or
redundant work.

# When to use

- A task can be partitioned into 3+ independent work items of similar size.
- The user asks for parallel processing, fan-out, or batch agent work.
- A codebase-wide analysis (lint, migrate, audit) benefits from splitting
  by file, module, or directory.
- Results from many agents must be merged into one deliverable.

# Do NOT use when

- Work items have strict sequential dependencies (A must finish before B).
- Total items are fewer than 3 — simple delegation is cheaper.
- Agents would need write access to the same files (use lane-safety instead).
- A single agent can complete the task within its context window.

# Operating procedure

1. **Inventory the work items.** Run `glob` or `grep` to produce a concrete
   list of targets (files, modules, endpoints, tickets). Record the count.
2. **Choose a coordination pattern.**
   - *Map/Reduce* — each agent processes one partition and returns a typed
     result; a reducer merges all results. Best for homogeneous tasks.
   - *Scatter-Gather* — agents explore freely within a scope; gatherer
     deduplicates and ranks findings. Best for search or audit.
   - *Blackboard* — agents read/write to a shared state file; a monitor
     checks convergence. Best for iterative refinement.
   Record the chosen pattern and rationale.
3. **Define partition boundaries.** Assign each work item to exactly one
   agent. List assignments in a table: `| Agent ID | Scope (files/dirs) |
   Expected output shape |`. No two agents may share a write target.
4. **Set the overlap budget.** Specify the maximum acceptable duplication
   percentage (default 10%). If two agents' scopes overlap by more than
   this, merge the scopes into one agent.
5. **Set convergence criteria.** Define what "done" means: e.g., "all N
   agents returned a result" or "blackboard has no OPEN items remaining."
6. **Launch agents.** Use `task` tool calls in parallel. Each prompt must
   include: (a) the agent's partition, (b) the output schema, (c) a hard
   instruction to NOT touch files outside its partition.
7. **Collect results.** As agents complete, validate each result against the
   expected output shape. Log pass/fail per agent in a status table.
8. **Run the reduce / gather / convergence step.** Merge individual results:
   - *Map/Reduce*: concatenate or aggregate typed outputs.
   - *Scatter-Gather*: deduplicate findings by key, keep highest-confidence.
   - *Blackboard*: verify all items are RESOLVED; re-run stragglers once.
9. **Emit the merged deliverable.** Produce the final output with a provenance
   section showing which agent contributed which piece.

# Decision rules

- Never launch more agents than work items — 1:1 mapping maximum.
- If an agent fails, retry it once with the same partition. On second
  failure, absorb its partition into an adjacent agent.
- If overlap between two results exceeds the budget, discard the
  lower-confidence duplicate and note the discard in provenance.
- Prefer map/reduce when output is structured data; prefer scatter-gather
  when output is natural-language findings.
- Cap swarm size at 10 agents per round unless the user explicitly raises it.

# Output requirements

1. **Partition Table** — markdown table of agent-to-scope assignments.
2. **Status Table** — agent ID, status (pass/fail/retry), result summary.
3. **Merged Deliverable** — the combined output in the format the user
   requested (report, code changes, data table, etc.).
4. **Provenance Log** — which agent produced which section of the deliverable.

# References

- `references/delegate-contracts.md` — output shape contracts for agents.
- `references/checkpoint-rules.md` — when to checkpoint swarm state.
- `references/failure-escalation.md` — retry and absorb protocols.

# Related skills

- `subagent-research-patterns` — single-agent research delegation.
- `verification-before-advance` — gating the merge step on evidence.
- `workflow-state-memory` — persisting swarm state across sessions.
- `goal-decomposition` — breaking goals into work items pre-swarm.

# Failure handling

- **Agent returns wrong output shape:** Reject the result, log the schema
  mismatch, and re-launch the agent with an explicit schema reminder.
- **Swarm stalls (>50% agents timed out):** Cancel remaining agents, merge
  whatever results exist, and report partial coverage to the user.
- **Merge conflicts in blackboard:** Freeze the blackboard, list conflicting
  entries with their authors, and ask the user for resolution order.
- **Overlap budget exceeded after merge:** Flag the duplicated sections,
  keep the version with more citations, and note the discard in provenance.
