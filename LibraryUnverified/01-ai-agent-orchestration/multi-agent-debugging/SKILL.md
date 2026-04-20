---
name: multi-agent-debugging
description: >
  Diagnoses failures in multi-agent systems by tracing errors across agent boundaries,
  replaying message sequences, and constructing blame paths to the root cause.
  Trigger — "agent produced wrong output", "which agent failed", "trace the error",
  "debug multi-agent", "cross-agent failure", "replay agent messages", "blame path",
  "agents disagree on result".
  Skip — single-agent bugs, application-level debugging unrelated to agent orchestration, performance tuning.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: multi-agent-debugging
  maturity: draft
  risk: low
  tags: [multi, agent, debugging, trace, replay, blame]
---

# Purpose

When a multi-agent workflow produces incorrect results, silently drops work, or fails partway
through, systematically trace the failure across agent boundaries to identify which agent,
message, or state transition caused the breakdown — then recommend a targeted fix.

# When to use

- A multi-agent run completed but the output is wrong or incomplete.
- An agent in the middle of a chain failed and it is unclear whether the fault is upstream or local.
- Two agents produced contradictory outputs that were merged incorrectly.
- The orchestrator reports success but artifact inspection reveals missing or corrupt deliverables.
- A previously working multi-agent workflow started failing after a change to one agent prompt or tools.

# Do NOT use when

- A single agent failed in isolation with no cross-agent interaction (use standard debugging).
- The system has not run yet and you are designing, not debugging (use `manager-hierarchy-design`).
- The failure is a stall or hang, not a wrong result (use `long-run-watchdog`).
- The issue is write conflicts between parallel agents (use `parallel-lane-safety`).

# Operating procedure

1. **Collect the failure evidence.** Gather: the final incorrect output, the expected output,
   all agent logs, and the orchestrator run summary. List the agents involved in a table:
   `| Agent | Role | Status | Output Summary |`.
2. **Build the message timeline.** Extract every inter-agent message (delegations, handoffs,
   results) and sort by timestamp. Output as: `| T | From | To | Message Type | Payload Hash |`.
3. **Identify the first divergence point.** Compare actual outputs vs expected at each agent
   boundary. Find the earliest point where actual does not equal expected. Mark this as DIVERGENCE_POINT.
4. **Snapshot state at divergence.** Capture the input the divergent agent received, its prompt,
   its tool calls, and its output. Diff the input against what the upstream agent claims to
   have sent — check for serialization errors, truncation, or schema mismatches.
5. **Replay the failing agent.** Re-run the divergent agent with the exact same input in isolation.
   If it produces the same wrong output, the bug is local. If it produces correct output,
   the bug is environmental (context pollution, shared state, or race condition).
6. **Construct the blame path.** Walk backward from the divergence point:
   - Was the input wrong? Blame the upstream agent; recurse to step 4 with that agent.
   - Was the input correct but the output wrong? Blame this agent prompt, tools, or model.
   - Was the output correct but downstream misinterpreted it? Blame the handoff schema.
7. **Classify the failure pattern.** Assign one of:
   PROMPT_DRIFT | SCHEMA_MISMATCH | STATE_POLLUTION | RACE_CONDITION |
   SILENT_DROP | TRUNCATION | TOOL_ERROR | MODEL_HALLUCINATION.
8. **Write the root-cause report.** Include: blame path, failure pattern, evidence chain,
   and a concrete fix recommendation (e.g., add output schema validation to the handoff).
9. **Verify the fix.** If possible, apply the fix and replay the failing segment to confirm
   the divergence is resolved.

# Decision rules

- Always start from the output and trace backward; never guess the root cause from symptoms alone.
- A replay that produces different results than the original run indicates non-determinism —
  investigate shared mutable state or race conditions first.
- If the blame path crosses 3+ agents, check for systemic issues (schema drift, prompt template
  changes) rather than blaming individual agents.
- Never blame the model as hallucinating without first ruling out bad input, bad prompt,
  and bad tool configuration.

# Output requirements

1. **Agent Inventory** — table of all agents, roles, and statuses.
2. **Message Timeline** — chronological list of all inter-agent communications.
3. **Blame Path** — ordered chain from symptom to root cause with evidence at each step.
4. **Failure Classification** — one of the defined failure patterns.
5. **Fix Recommendation** — specific, actionable change to prevent recurrence.

# References

- `references/delegate-contracts.md` — expected message schemas for inter-agent communication.
- `references/checkpoint-rules.md` — where state snapshots should exist for replay.
- `references/failure-escalation.md` — when to escalate vs continue debugging.

# Related skills

- `long-run-watchdog` — for detecting stalls that may precede failures.
- `process-versioning` — for identifying if a recent workflow version change caused the break.
- `parallel-lane-safety` — for failures rooted in concurrent write conflicts.
- `panel-of-experts` — for getting multiple perspectives on ambiguous root causes.

# Failure handling

- If agent logs are missing or incomplete, reconstruct the timeline from available artifacts
  (file timestamps, git commits, output files) and flag gaps explicitly.
- If replay produces non-deterministic results, run 3 replays and report the distribution
  of outcomes rather than relying on a single run.
- If the blame path is ambiguous between two agents, report both candidates with confidence
  levels and suggest adding instrumentation to disambiguate on the next run.
- If the root cause is outside the agent system (e.g., external API failure), document it
  clearly and recommend retry logic or circuit-breaker patterns.
