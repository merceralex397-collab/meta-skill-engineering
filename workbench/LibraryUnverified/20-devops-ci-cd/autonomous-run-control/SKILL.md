---
name: autonomous-run-control
description: >
  Trigger — "set run budget", "agent keeps running too long", "add timeout to agent",
  "stop runaway agent", "checkpoint the run", "pause execution", "rollback last run".
  Skip — when the task is a single short command (under 30 seconds), when the user is manually
  controlling execution step-by-step, or when the agent has no autonomous execution capability.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: autonomous-run-control
  maturity: draft
  risk: low
  tags: [autonomous, run-control, timeout, checkpoint, rollback]
---

# Purpose

Govern autonomous agent execution by defining start/stop policies, resource budgets, timeout
thresholds, checkpoint frequency, and rollback triggers. Prevents runaway agents from consuming
unbounded resources or producing unrecoverable state.

# When to use

- An agent will execute autonomously for more than 60 seconds or multiple tool calls.
- Token or cost budgets must be enforced for an agent run.
- A long-running workflow needs periodic checkpoints so partial progress is recoverable.
- An agent has previously run away (exceeded time or token limits) and guardrails are needed.
- Rollback capability is required in case an autonomous run produces bad output.

# Do NOT use when

- The task is a single synchronous command completing in under 30 seconds.
- The user is manually stepping through execution and controlling each action.
- The agent framework already provides built-in run control that is correctly configured.

# Operating procedure

1. Read the agent configuration file (e.g., `agents.yaml`, `AGENTS.md`, or equivalent) and extract current timeout, token limit, and checkpoint settings.
2. Define a **run budget** table: `| Resource | Limit | Current Setting | Recommended |` covering: wall-clock time, token count, tool-call count, and cost (USD).
3. Set wall-clock timeout: default 300 seconds for standard tasks, 900 seconds for multi-file refactors, 1800 seconds for full-repo operations.
4. Set token budget: default 50,000 tokens for standard tasks, 150,000 for complex tasks. Include both input and output tokens.
5. Set tool-call limit: default 30 calls for standard tasks, 100 for complex multi-step workflows.
6. Define **checkpoint frequency**: emit a checkpoint every N tool calls (default N=10) or every M seconds (default M=120), whichever comes first.
7. Write checkpoint format: `{ "run_id": "<uuid>", "step": <n>, "timestamp": "<ISO-8601>", "state_snapshot": "<serialized>", "files_modified": ["<paths>"] }`.
8. Define **runaway detection** rules: trigger alert if any of these occur — (a) 3 consecutive identical tool calls, (b) token usage exceeds 80% of budget with <20% of task complete, (c) no new file modifications in last 10 tool calls despite active execution.
9. Define **graceful shutdown** sequence: (a) complete current tool call, (b) write final checkpoint, (c) emit summary of progress and remaining work, (d) revert any uncommitted partial changes, (e) set run status to `paused`.
10. Define **rollback trigger** conditions: (a) test suite failures exceed pre-run baseline, (b) linter errors increase, (c) agent explicitly reports unrecoverable error. On trigger: revert to last checkpoint via `git checkout` or state restore.
11. Write all policies to a `run-control-policy.md` or update the agent config with the new limits.

# Decision rules

- Never allow an agent to run without at least a wall-clock timeout — infinite runs are always prohibited.
- If token usage exceeds 80% of budget, switch to summary-only mode: stop generating new code and produce a progress report instead.
- Checkpoint data must include the list of modified files so rollback can be surgical, not full-repo.
- Runaway detection must halt the agent, not just warn — warnings are insufficient for autonomous runs.
- If the agent is within 10% of any resource limit, emit a "budget warning" before the next tool call.

# Output requirements

1. **Run Budget Table** — resource limits with current vs. recommended values.
2. **Checkpoint Policy** — frequency, format, and storage location for checkpoints.
3. **Runaway Detection Rules** — enumerated conditions that trigger automatic halt.
4. **Graceful Shutdown Sequence** — ordered steps for clean termination.
5. **Rollback Policy** — conditions and mechanism for reverting to last-known-good state.

# References

- `references/checkpoint-rules.md` — detailed checkpoint format and storage patterns.
- `references/failure-escalation.md` — escalation paths when run control detects anomalies.
- Agent framework documentation for configuring native timeout and budget settings.

# Related skills

- `human-interrupt-handling` — human interrupts interact with run-control pause/resume.
- `collaboration-checkpoints` — multi-agent checkpoints build on single-agent checkpoint format.
- `artifact-contracts` — run completion requires producing contracted artifacts.
- `workflow-state-memory` — state persistence across paused/resumed runs.

# Failure handling

- **Checkpoint write failure**: If checkpoint cannot be persisted, halt the run immediately — do not continue without recovery capability.
- **Budget exceeded silently**: If the framework does not enforce budgets natively, wrap each tool call in a budget-check decorator and abort if exceeded.
- **Rollback failure**: If `git checkout` or state restore fails, preserve the current state as-is, log all modified file paths, and escalate to human.
- **Runaway false positive**: If the agent is halted but was making legitimate progress (e.g., repetitive but correct bulk operations), allow resume with a 2× budget increase and a note explaining the override.
