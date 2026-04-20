---
name: team-leader
description: >
  Orchestrates a team of specialist agents to deliver a milestone through work breakdown, assignment, and progress tracking.
  Trigger — "coordinate the agents", "break down this milestone", "assign work to specialists",
  "lead the team on this feature", "orchestrate delivery", "track agent progress".
  Skip — single-agent tasks with no delegation needed, or tasks requiring direct code implementation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: team-leader
  maturity: draft
  risk: low
  tags: [team, leader]
---

# Purpose

Decompose a milestone or feature request into a work breakdown structure, assign tasks
to the most appropriate specialist agents, track progress across all work streams, escalate
blockers, enforce quality gates, and coordinate delivery. The team leader does not implement
— it delegates, monitors, and unblocks.

# When to use

- A milestone requires contributions from 3+ specialist agents (e.g., implementer, reviewer, tester).
- Work needs to be parallelised across multiple agents with dependency ordering.
- A complex feature spans multiple files, services, or skill domains.
- Prior attempts at a task failed due to scope creep or lack of coordination.
- The user asks to "orchestrate", "lead", or "coordinate" work across agents.

# Do NOT use when

- The task is small enough for a single agent to handle end-to-end.
- The user wants direct implementation, not coordination — use an implementer skill.
- The task is a pure review with no multi-agent workflow — use `code-review` or `security-review`.
- Planning only is needed with no execution — use `planner` instead.

# Operating procedure

1. Parse the milestone description and extract all deliverables by listing each concrete output (file, endpoint, test suite, doc) in a numbered list.
2. Build a work breakdown structure (WBS) table: `| Task ID | Description | Specialist | Dependencies | Estimated Effort | Status |`.
3. Identify the critical path by marking tasks that block others and sequencing them first.
4. Assign each task to a specialist agent by matching task requirements to agent capabilities — list the assignment as `Task T1 → agent-name (reason)`.
5. Define quality gates for each deliverable: specify the check (e.g., "tests pass", "review approved", "no lint errors") and the agent responsible for verification.
6. Dispatch the first batch of non-blocked tasks to their assigned agents with explicit prompts containing: task ID, scope boundary, input artifacts, expected output format, and deadline signal.
7. After each agent completes, verify the output against the quality gate — run the specified check command or review the artifact.
8. Update the WBS table status for completed tasks and identify newly unblocked tasks.
9. If a task is blocked for more than one cycle, escalate: document the blocker, identify which agent or resource can resolve it, and adjust the plan.
10. When all tasks show status DONE and all quality gates pass, compile the delivery summary.

# Decision rules

- Never implement directly — if no suitable specialist exists, create a ticket for manual follow-up.
- Assign tasks to the most specific agent available; prefer `security-review` over `code-review` for auth changes.
- If two tasks have a circular dependency, break the cycle by identifying a minimal interface contract both can code against.
- Re-plan after any task fails — do not simply retry with the same prompt.
- Limit WBS to 12 tasks maximum per milestone; split larger milestones into sub-milestones.

# Output requirements

1. **Milestone Summary** — one-paragraph description of what will be delivered.
2. **Work Breakdown Structure** — full WBS table with all columns filled.
3. **Agent Assignments** — mapping of tasks to specialists with rationale.
4. **Critical Path** — ordered list of blocking tasks.
5. **Quality Gates** — table of `| Deliverable | Gate Check | Verifier Agent |`.
6. **Progress Log** — timestamped entries for each status change.
7. **Delivery Report** — final summary with links to all outputs and any open items.

# References

- Project ticket system or backlog (if present in repo)
- `AGENTS.md` for available specialist descriptions
- Quality gate definitions from CI/CD configuration

# Related skills

- `planner` — creates the initial plan that team-leader executes
- `ticket-creator` — generates tickets for work items identified in the WBS
- `ticket-audit` — validates ticket quality before assignment
- `backlog-verifier` — confirms backlog state matches team-leader's tracking

# Failure handling

- If an assigned agent fails twice on the same task, reassign to an alternative agent or flag for human intervention.
- If the milestone scope changes mid-execution, pause all in-flight tasks, re-run steps 1–3, and re-dispatch.
- If a quality gate cannot be verified (e.g., no test runner configured), log the gap and mark the gate as MANUAL.
- If no specialist agent matches a task, document the capability gap and recommend creating or installing the needed skill.
