---
name: long-run-watchdog
description: >
  Monitors autonomous agent tasks for stalls, infinite loops, resource exhaustion, and progress stagnation,
  then applies bounded recovery actions.
  Trigger — "agent is stuck", "task seems hung", "detect stall", "watchdog timer", "loop detection",
  "agent not making progress", "heartbeat check", "run is taking too long".
  Skip — single-shot queries, tasks completing in under 60 seconds, manual debugging sessions.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: long-run-watchdog
  maturity: draft
  risk: low
  tags: [long, run, watchdog, stall, heartbeat, monitoring]
---

# Purpose

Detect when a long-running agent task has stalled, entered an infinite loop, exhausted resources,
or stopped making meaningful progress — then intervene with a graduated recovery policy before
the task wastes further compute or produces corrupt output.

# When to use

- An autonomous agent run has exceeded its expected wall-clock time.
- Output logs show repeated identical actions (loop signature).
- A heartbeat or progress metric has flatlined for N intervals.
- Resource consumption (tokens, API calls, disk writes) is spiking without corresponding output.
- The orchestrator needs pre-defined escalation rules for unattended runs.

# Do NOT use when

- The task is a single prompt/response with no iteration.
- The agent completed normally but the user dislikes the result (use review skills instead).
- You are actively debugging a failure that already terminated (use `multi-agent-debugging`).
- The run is interactive with a human in the loop providing guidance each step.

# Operating procedure

1. **Collect run metadata.** Read the agent's task description, expected duration, and any
   configured timeout values. List them in a summary table: `| Field | Value |`.
2. **Define heartbeat contract.** Set a heartbeat interval (default: 60 s for short tasks,
   300 s for long tasks). Record the metric that constitutes "progress" — e.g., new file written,
   test passed, commit created, lines of output produced.
3. **Scan recent output for loop signatures.** Run `grep` or string-match on the last 200 lines
   of agent output. Flag if any 5-line block repeats 3+ times consecutively.
4. **Check progress metric delta.** Compare the progress metric at T-now vs T-minus-one-interval.
   If delta is zero for 2 consecutive intervals, mark the run as STALLED.
5. **Check resource burn rate.** Count total tokens or API calls consumed in the last interval.
   If burn rate exceeds 3× the average of prior intervals with zero output delta, mark as RUNAWAY.
6. **Apply recovery action based on severity:**
   - STALLED → Send a nudge prompt: restate the goal and last successful checkpoint.
   - RUNAWAY → Pause the agent, snapshot current state, and escalate to the orchestrator.
   - LOOP_DETECTED → Inject a "break the pattern" prompt with an alternative approach suggestion.
   - TIMEOUT → Terminate the run, preserve artifacts, and write a summary of progress so far.
7. **Log the intervention.** Write a structured record: `{ timestamp, run_id, condition, action_taken, outcome }`.
8. **Verify recovery.** After a nudge or pattern-break, wait one full interval and re-run steps 3-5.
   If the condition persists after 2 recovery attempts, escalate to TIMEOUT.
9. **Produce a watchdog report.** Summarize: total run time, intervals monitored, conditions detected,
   actions taken, final status (recovered / terminated / escalated).

# Decision rules

- Never terminate a run on the first stall detection; always attempt at least one nudge first.
- If the agent has produced partial valuable output, snapshot it before any destructive action.
- A loop is only confirmed when the same action sequence repeats 3+ times — not 2.
- Resource burn rate thresholds are relative to the run's own history, not absolute numbers.
- Escalation to a human or parent orchestrator is mandatory after 2 failed recovery attempts.

# Output requirements

1. **Watchdog Status** — one of: HEALTHY, STALLED, RUNAWAY, LOOP_DETECTED, TIMEOUT, RECOVERED.
2. **Heartbeat Log** — table of intervals with progress metric values.
3. **Intervention Record** — each action taken with timestamp and outcome.
4. **Recovery Recommendation** — if terminated, a concrete suggestion for what to change on retry.

# References

- `references/checkpoint-rules.md` — how to snapshot agent state.
- `references/failure-escalation.md` — escalation chain definitions.
- `references/delegate-contracts.md` — expected heartbeat clauses in delegation.

# Related skills

- `multi-agent-debugging` — for post-mortem analysis after a watchdog termination.
- `verification-before-advance` — for quality gates that prevent runaway progress.
- `human-interrupt-handling` — for escalations that reach a human operator.
- `autonomous-run-control` — for broader run lifecycle management.

# Failure handling

- If agent output is inaccessible (no logs), treat as STALLED immediately and escalate.
- If the heartbeat metric is undefined, fall back to wall-clock time with a 2× expected-duration threshold.
- If recovery actions themselves fail (e.g., prompt injection rejected), terminate and preserve all state.
- If multiple agents share a run, isolate the stalled agent before intervening to avoid cascade disruption.
