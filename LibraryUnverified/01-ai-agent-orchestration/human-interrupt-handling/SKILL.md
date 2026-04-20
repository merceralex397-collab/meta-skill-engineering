---
name: human-interrupt-handling
description: >
  Trigger — "agent needs human input", "when should the agent ask the user", "pause for approval",
  "escalate to human", "resume after human response", "handle user interrupt mid-run".
  Skip — when the agent has all information needed to proceed, when the task is fully autonomous
  with pre-approved scope, or when interrupt handling is already configured in the agent framework.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: human-interrupt-handling
  maturity: draft
  risk: low
  tags: [human, interrupt, escalation, pause, resume, checkpoint]
---

# Purpose

Define when and how an autonomous agent should pause execution to request human input,
how to preserve context so the human has full situational awareness, and how to resume
execution cleanly after receiving a response. Prevents agents from making irreversible
decisions without appropriate human oversight.

# When to use

- An agent encounters a decision that exceeds its delegated authority (e.g., deleting files, modifying production config).
- Confidence in the next action is below a defined threshold and human judgment is needed.
- The agent detects ambiguity that cannot be resolved from available context.
- A destructive or irreversible operation requires explicit human approval.
- The agent has been running autonomously and a scheduled human review point is reached.

# Do NOT use when

- The agent has all information and authority needed to proceed without human input.
- The task scope was pre-approved and the agent is operating within those bounds.
- The agent framework already provides a built-in interrupt mechanism that is correctly configured.
- The question can be answered by reading existing documentation or code.

# Operating procedure

1. Define **interrupt severity levels** in a table: `| Level | Name | Trigger Condition | Max Wait Time | Fallback Action |`.
   - Level 1 (Info): non-blocking notification, agent continues. Wait: 0s. Fallback: log and proceed.
   - Level 2 (Query): agent pauses current branch but continues other work. Wait: 300s. Fallback: pick safest option.
   - Level 3 (Approval): agent fully pauses, no further actions. Wait: 600s. Fallback: abort and rollback.
   - Level 4 (Emergency): agent halts all work and alerts immediately. Wait: 1800s. Fallback: full rollback.
2. When an interrupt condition is detected, classify it into the appropriate severity level.
3. Create a **context snapshot** containing: `{ "run_id", "current_step", "files_modified", "pending_actions", "decision_needed", "options_considered", "confidence_per_option", "relevant_code_snippets" }`.
4. Format the human-facing interrupt message: state the decision needed in one sentence, list 2–4 options with pros/cons, highlight the recommended option and its confidence score, and include the context snapshot as a collapsible detail block.
5. Write the interrupt to the designated channel: inline prompt (for interactive sessions), GitHub issue comment (for async), or webhook (for external systems).
6. Start a **response timer** matching the severity level's max wait time.
7. While waiting: if severity ≤ 2, continue executing non-dependent work branches. If severity ≥ 3, write a checkpoint and halt all execution.
8. On human response: parse the response, validate it matches one of the presented options (or is a free-form override), and log the decision with timestamp and author.
9. **Resume protocol**: (a) reload the checkpoint or current state, (b) apply the human's decision, (c) verify the working tree is consistent (run `git status`, check for conflicts), (d) continue execution from the interrupted step.
10. On **timeout** (no human response within max wait): execute the fallback action defined for that severity level. Log that the fallback was used.
11. After the run completes, emit an **interrupt summary**: count of interrupts by level, average response time, timeouts triggered, decisions made.

# Decision rules

- Any operation that deletes files, modifies environment variables, or changes production configuration is minimum Level 3.
- If the agent's confidence in all available options is below 50%, escalate to Level 3 regardless of the operation type.
- Never execute a Level 3+ fallback that involves destructive actions — prefer abort over guess.
- Human responses override agent recommendations without exception, even if the agent disagrees.
- If the same interrupt type fires 3+ times in one run, batch remaining instances into a single interrupt with a summary table.

# Output requirements

1. **Interrupt Severity Table** — levels, triggers, wait times, and fallback actions.
2. **Context Snapshot** — structured JSON with all state needed for human to make an informed decision.
3. **Interrupt Message** — formatted question with options, pros/cons, and recommendation.
4. **Decision Log** — timestamped record of each interrupt, human response, and outcome.
5. **Interrupt Summary** — end-of-run statistics on interrupt count, response times, and timeouts.

# References

- `references/failure-escalation.md` — escalation paths and severity classification.
- `references/checkpoint-rules.md` — checkpoint format used for context preservation during interrupts.
- `references/delegate-contracts.md` — delegation boundaries that define when interrupts are needed.

# Related skills

- `autonomous-run-control` — run-control pause/resume interacts with interrupt handling.
- `collaboration-checkpoints` — multi-agent checkpoints may trigger synchronized interrupts.
- `goal-decomposition` — ambiguous goals may require human interrupt before decomposition begins.
- `verification-before-advance` — failed verification may escalate to human interrupt.

# Failure handling

- **Interrupt delivery failure**: If the interrupt message cannot be delivered (channel unavailable), fall back to the next available channel. If all channels fail, halt the run and write the interrupt to a local `interrupts.log` file.
- **Malformed human response**: If the response does not match any presented option and cannot be parsed as a free-form override, re-send the interrupt with a clarification request (once). On second failure, execute the safest fallback.
- **Context snapshot too large**: If the snapshot exceeds 5000 tokens, summarize to key facts and link to full state. Never truncate without a summary.
- **Resume state corruption**: If the working tree has changed between interrupt and resume (e.g., another agent modified files), run conflict detection before resuming. If conflicts exist, trigger a collaboration checkpoint.
