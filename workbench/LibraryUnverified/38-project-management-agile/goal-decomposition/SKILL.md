---
name: goal-decomposition
description: >
  Trigger — "break this down into tasks", "decompose this goal", "create subtasks for",
  "plan the work breakdown", "what are the steps to achieve", "split this into agent-sized tasks".
  Skip — when the goal is already a single concrete action (e.g., "fix typo in README"),
  when a full task breakdown already exists and is current, or when the user explicitly
  wants to execute rather than plan.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: goal-decomposition
  maturity: draft
  risk: low
  tags: [goal, decomposition, planning, task-breakdown, MECE]
---

# Purpose

Transform high-level goals into a tree of agent-executable tasks, each with clear scope,
acceptance criteria, and dependency ordering. Ensures every leaf task is small enough for
a single agent to complete in one run and that the full tree covers the goal with no gaps
or overlaps (MECE).

# When to use

- A goal or epic is too large for a single agent to execute in one run.
- The user provides a broad objective ("refactor auth system") that needs structured decomposition.
- A project kickoff needs a work breakdown before agents can be assigned.
- An existing decomposition has gaps, overlaps, or tasks that are still too large.
- Dependencies between tasks are unclear and need explicit ordering.

# Do NOT use when

- The goal is already a single concrete action that one agent can complete directly.
- A complete, current task breakdown already exists (update it rather than recreating).
- The user wants execution, not planning — hand off to the appropriate execution skill.

# Operating procedure

1. State the top-level goal in one sentence. Write it as a measurable outcome (e.g., "All API endpoints return valid JSON Schema-compliant responses" not "improve the API").
2. Identify the goal's **scope boundaries**: list what is in-scope and what is explicitly out-of-scope.
3. Decompose the goal into 2–5 **sub-goals** using MECE principles: each sub-goal is mutually exclusive (no overlap) and collectively exhaustive (full coverage).
4. For each sub-goal, repeat decomposition until every leaf task meets the **single-agent rule**: completable by one agent in one run (≤30 tool calls, ≤50,000 tokens, ≤300 seconds).
5. Write each leaf task with these fields: `| Task ID | Title | Description (≤50 words) | Acceptance Criteria (≤3 items) | Estimated Tool Calls | Dependencies |`.
6. Build a **dependency graph**: for each task, list which other task IDs must complete first. Verify there are no circular dependencies by performing a topological sort.
7. Run the topological sort: list tasks in execution order, grouping tasks that can run in parallel on the same line.
8. Validate MECE coverage: for each sub-goal, confirm that its child tasks fully cover the sub-goal's scope and do not overlap with siblings.
9. Assign **priority tiers**: Tier 1 = tasks with no dependencies (can start immediately), Tier 2 = tasks depending only on Tier 1, etc.
10. Produce the final **Goal Tree** as a markdown outline with indentation showing parent-child relationships, plus the execution-order table.
11. Write leaf tasks to the repo's ticket system (e.g., `tickets/` directory) if one exists, or output them as a structured list.

# Decision rules

- If a leaf task requires more than 30 tool calls, it is too large — split it further.
- If two tasks modify the same file, add an explicit dependency or assign them to the same agent.
- Acceptance criteria must be verifiable by an agent (e.g., "tests pass", "file exists") — never subjective ("code is clean").
- If the goal has ambiguous scope, define the narrowest reasonable interpretation and note assumptions.
- Maximum tree depth is 4 levels. If deeper decomposition is needed, the goal itself is too broad — split the goal first.

# Output requirements

1. **Goal Statement** — one-sentence measurable outcome.
2. **Scope Boundaries** — in-scope and out-of-scope lists.
3. **Goal Tree** — markdown outline showing hierarchical decomposition.
4. **Leaf Task Table** — all leaf tasks with ID, title, description, acceptance criteria, estimated effort, and dependencies.
5. **Execution Order** — topologically sorted task list with parallel groupings.
6. **Dependency Graph** — text-based DAG or mermaid diagram showing task relationships.

# References

- `references/delegate-contracts.md` — each leaf task becomes a delegation contract.
- `references/checkpoint-rules.md` — checkpoints align with sub-goal completion boundaries.
- MECE framework (McKinsey) for ensuring complete, non-overlapping decomposition.

# Related skills

- `artifact-contracts` — each leaf task should have an artifact contract defining its output.
- `autonomous-run-control` — task sizing must respect agent run budgets.
- `human-interrupt-handling` — ambiguous goals may require human clarification before decomposition.
- `collaboration-checkpoints` — parallel task groups need sync points.

# Failure handling

- **Ambiguous goal**: If the goal cannot be stated as a measurable outcome, pause and request clarification — do not decompose a vague goal.
- **MECE violation detected**: If tasks overlap, merge the overlapping portions into a single task. If gaps exist, create a new task to cover the gap.
- **Circular dependency**: If topological sort fails, identify the cycle and break it by splitting one task into a dependency-free setup step and a dependent execution step.
- **Over-decomposition**: If the tree exceeds 30 leaf tasks, re-examine sub-goals for opportunities to merge related tasks.
