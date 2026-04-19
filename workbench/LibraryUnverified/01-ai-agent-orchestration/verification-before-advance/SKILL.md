---
name: verification-before-advance
description: >
  Trigger — "verify before moving on", "add a gate check", "require proof
  before next step", "evidence checklist", "quality gate", "don't proceed
  until tests pass".
  Skip — task is exploratory with no defined stages; user explicitly says
  "skip checks"; the workflow has only one step with no downstream
  dependencies.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: verification-before-advance
  maturity: draft
  risk: low
  tags: [verification, gate, checklist, quality, evidence]
---

# Purpose

Enforces explicit gate checks between workflow stages so that no agent
advances to the next step until concrete evidence proves the current step
is complete — replacing optimistic "looks good" status with verified
pass/fail criteria.

# When to use

- A multi-step workflow exists where later steps depend on earlier outputs.
- The user requests quality gates, checkpoints, or "don't proceed until…"
  conditions.
- Prior runs have failed because an agent skipped verification and built on
  incomplete work.
- A CI-like approve/reject flow is needed inside an agent workflow.

# Do NOT use when

- The task is a single atomic action with no downstream stages.
- The user explicitly opts out of verification ("just do it, skip checks").
- Verification would require external human approval that cannot be
  simulated (use `approval-gates` for human-in-the-loop instead).
- The workflow is purely exploratory with no concrete deliverable.

# Operating procedure

1. **List all workflow stages.** Run through the current plan, ticket, or
   task description and extract each distinct stage. Number them sequentially
   in a markdown list: `Stage 1: …`, `Stage 2: …`, etc.
2. **Define gate criteria per stage.** For each stage, write 1–4 verifiable
   criteria. Each criterion must be a concrete check, not a subjective
   judgment. Examples:
   - "All unit tests in `src/auth/` pass (`npm test -- --grep auth`)."
   - "File `docs/API.md` contains an entry for every route in `src/routes/`."
   - "No `TODO` or `FIXME` comments remain in changed files."
3. **Assign evidence type to each criterion.** Tag every criterion with one
   of: `command_output` (run a command, check exit code or output),
   `file_content` (grep/view a file for expected content), `artifact_exists`
   (glob for a required file), `metric_threshold` (numeric value ≥ or ≤ N).
4. **Execute the current stage's work.** Perform the actual task for the
   stage — write code, generate docs, run migrations, etc.
5. **Run the gate check.** For each criterion of the completed stage:
   (a) Execute the check (run the command, grep the file, glob for the
   artifact). (b) Record the result: PASS with evidence snippet, or FAIL
   with the actual vs. expected output.
6. **Evaluate the gate.** Apply the pass/fail threshold (default: ALL
   criteria must PASS). If the gate passes, record a `✅ Gate N passed`
   line and proceed to the next stage. If it fails, go to step 7.
7. **Handle gate failure.** List every FAIL criterion with its evidence.
   Attempt a fix for each (re-run the command after correction, edit the
   file, regenerate the artifact). Re-run the gate check (step 5). If the
   gate still fails after 2 retry cycles, halt the workflow and report the
   blockers to the user.
8. **Log the gate trace.** Append a `## Gate Log` section to the output
   with: stage name, criteria, evidence, pass/fail, retry count.

# Decision rules

- Never advance past a failed gate — no exceptions without explicit user
  override.
- If a criterion is ambiguous (cannot be checked mechanically), replace it
  with a concrete alternative or remove it and note the gap.
- If the stage itself produced no observable output (e.g., a "think about
  architecture" step), require at least one written artifact as evidence
  (a decision record, a diagram description, a pros/cons list).
- Treat flaky checks (pass on retry without code change) as a WARN —
  advance but record the flakiness in the gate log.
- Default threshold is ALL-pass. The user may lower it to N-of-M; record
  the custom threshold in the gate definition.

# Output requirements

1. **Gate Definition Table** — markdown table: `| Stage | Criterion |
   Evidence Type | Threshold |`.
2. **Gate Results** — per stage: `| Criterion | Result | Evidence Snippet |`.
3. **Gate Log** — append-only log of all gate evaluations, retries, and
   overrides across the entire workflow run.

# References

- `references/checkpoint-rules.md` — checkpoint file format for gate state.
- `references/failure-escalation.md` — escalation paths when gates fail.

# Related skills

- `workflow-state-memory` — persisting gate state across sessions.
- `subagent-research-patterns` — gathering evidence to satisfy gate criteria.
- `swarm-patterns` — running gate checks across parallel agent outputs.
- `approval-gates` — human-in-the-loop approval (complementary to this).

# Failure handling

- **Gate criterion is untestable:** Replace it with the closest testable
  proxy, or remove it and add an explicit "unverified assumption" note.
- **Retry budget exhausted:** Stop the workflow, emit all FAIL evidence,
  and ask the user for guidance — never silently skip.
- **Stage produces no output:** Treat missing output as automatic gate
  failure; do not invent evidence.
- **User requests override:** Record the override in the gate log with
  timestamp and reason, then advance with a `⚠️ OVERRIDDEN` tag.
