---
name: manager-hierarchy-design
description: >
  Designs manager/worker agent hierarchies with explicit span-of-control limits, delegation depth,
  and communication topology for multi-agent systems.
  Trigger — "how many agents should report to one manager", "design agent hierarchy",
  "span of control", "delegation depth", "agent team structure", "manager vs worker roles",
  "fan-out pattern", "orchestration tiers".
  Skip — single-agent tasks, tasks with fewer than 3 workers, already-defined hierarchies needing only execution.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: manager-hierarchy-design
  maturity: draft
  risk: low
  tags: [manager, hierarchy, design, delegation, span-of-control]
---

# Purpose

Design the organizational structure of a multi-agent system: how many manager layers exist,
which agents are managers vs workers, what each manager controls, and how results flow back
up the hierarchy — ensuring no manager is overloaded and no worker is unsupervised.

# When to use

- A task requires 4+ agents working in parallel or in sequence.
- An existing flat agent pool is dropping tasks or producing conflicting outputs.
- The user asks to "structure the team", "add a manager layer", or "split up delegation".
- A workflow audit reveals that a single orchestrator is bottlenecked on too many direct reports.
- Planning a new multi-agent project and need to decide on hierarchy before execution begins.

# Do NOT use when

- Only 1-3 agents are involved (flat coordination is fine).
- The hierarchy already exists and the task is to execute within it (use the delegation skill).
- The problem is debugging agent failures, not organizational design (use `multi-agent-debugging`).
- The task is about runtime safety of parallel work (use `parallel-lane-safety`).

# Operating procedure

1. **Inventory the workload.** List every distinct task, subtask, or responsibility that needs
   an agent. Output a table: `| Task ID | Description | Estimated Complexity | Dependencies |`.
2. **Identify natural clusters.** Group tasks by shared domain, shared files, or sequential
   dependency. Run a dependency analysis: draw edges between tasks that share inputs/outputs.
3. **Apply span-of-control limits.** Set the maximum direct reports per manager to 5 (default).
   If a cluster has more than 5 tasks, split it into sub-clusters with a section lead.
4. **Determine hierarchy depth.** Count the layers needed. Enforce a maximum depth of 3
   (orchestrator → manager → worker). If the workload requires more, flatten by combining
   similar tasks rather than adding a 4th layer.
5. **Assign roles.** For each node in the hierarchy, write a role card:
   `| Agent Role | Responsibilities | Direct Reports | Reports To | Write Scope |`.
6. **Define communication topology.** Specify for each manager-worker pair:
   - How the worker reports completion (artifact handoff vs status message).
   - What triggers the manager to intervene (timeout, error, or quality gate failure).
   - Whether siblings can communicate directly or must route through the manager.
7. **Define result aggregation.** For each manager, specify how it combines worker outputs:
   merge files, concatenate reports, run a synthesis prompt, or pick-best.
8. **Validate the design.** Check: no orphan tasks, no worker with 2 managers, no manager
   with 0 reports, every task reachable from the root orchestrator.
9. **Output the hierarchy diagram.** Render as an indented text tree or Mermaid graph block.

# Decision rules

- Prefer flat over deep: add a manager layer only when span-of-control would exceed 5.
- Every manager must have at least 2 direct reports; a manager with 1 report should be collapsed.
- Workers never delegate to other workers; only managers delegate.
- Sibling-to-sibling communication is allowed only for read-only data sharing, never write coordination.
- If two clusters share more than 50% of their file write scopes, merge them under one manager.

# Output requirements

1. **Hierarchy Diagram** — text tree or Mermaid graph showing all agents and reporting lines.
2. **Role Cards Table** — one row per agent with role, responsibilities, reports-to, write scope.
3. **Communication Rules** — how each pair interacts (async artifact, sync handoff, broadcast).
4. **Aggregation Strategy** — per manager, how worker outputs are combined.
5. **Validation Checklist** — confirmation that all constraints pass.

# References

- `references/delegate-contracts.md` — contract templates for manager-worker delegation.
- `references/checkpoint-rules.md` — checkpoint requirements at each hierarchy level.
- `references/failure-escalation.md` — escalation paths when a manager detects worker failure.

# Related skills

- `goal-decomposition` — for breaking a goal into the task list that feeds hierarchy design.
- `parallel-lane-safety` — for ensuring workers in different lanes don't collide.
- `panel-of-experts` — for when "multiple perspectives" replaces "multiple workers".
- `long-run-watchdog` — for monitoring the health of managers and workers during execution.

# Failure handling

- If the workload inventory is incomplete, pause and request the missing task definitions before designing.
- If span-of-control cannot be maintained without exceeding depth 3, document the trade-off
  explicitly and choose the lesser violation with justification.
- If role assignments create circular dependencies, break the cycle by elevating one task to
  a higher manager level.
- If the user overrides a constraint (e.g., "I want 10 direct reports"), comply but log a
  warning about expected coordination overhead.
