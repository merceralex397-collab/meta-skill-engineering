---
name: workflow-state-memory
description: >
  Trigger — "save workflow state", "persist progress between sessions",
  "checkpoint the current run", "resume where we left off", "state file",
  "serialize workflow progress".
  Skip — task completes in a single session with no resumption needed;
  state is already managed by an external system (CI, database); the
  workflow has no intermediate results worth saving.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: workflow-state-memory
  maturity: draft
  risk: low
  tags: [workflow, state, persistence, checkpoint, resume]
---

# Purpose

Defines how to serialize, store, and reload workflow state so that agent
sessions can be interrupted and resumed without losing progress — replacing
reliance on conversational memory with explicit, file-backed checkpoints.

# When to use

- A workflow spans multiple sessions or may be interrupted mid-run.
- The user says "save progress", "checkpoint", or "I'll continue later."
- A long-running task (>10 steps) risks losing intermediate results if the
  session drops.
- Multiple agents need to share state through a common file rather than
  passing context through prompts.

# Do NOT use when

- The entire task fits in one session and produces a single atomic output.
- An external system (CI pipeline, database, issue tracker) already tracks
  the workflow state.
- The workflow is stateless by design (each run is independent).
- The user explicitly says state persistence is not needed.

# Operating procedure

1. **Identify state-worthy data.** List every piece of intermediate data
   that would be lost if the session ended now. Typical candidates:
   - Current stage index and stage name.
   - Todo/task list with per-item status (pending, in_progress, done, failed).
   - File paths that have been modified.
   - Gate check results from `verification-before-advance`.
   - Research findings from `subagent-research-patterns`.
2. **Choose a storage location.** Default to `.workflow-state/` in the repo
   root. If the repo has a `.gitignore`, check whether the directory is
   excluded; if not, add it. For non-repo contexts, use `/tmp/workflow-state/`.
3. **Define the checkpoint schema.** Create a JSON file named
   `<workflow-name>-state.json` with this structure:
   ```json
   {
     "workflow": "<name>",
     "version": 1,
     "created_at": "<ISO-8601>",
     "updated_at": "<ISO-8601>",
     "current_stage": "<stage-id>",
     "stages": [ { "id": "…", "status": "…", "evidence": "…" } ],
     "artifacts": [ "<path1>", "<path2>" ],
     "notes": "<free-text for context>"
   }
   ```
4. **Write the initial checkpoint.** Run `bash` to create the directory and
   write the JSON file with current values. Validate the JSON with
   `python3 -m json.tool <file>` before proceeding.
5. **Update the checkpoint after each stage.** After completing a stage (and
   passing its gate if `verification-before-advance` is active), update the
   `current_stage`, the stage's `status`, and `updated_at`. Write the file
   atomically: write to a `.tmp` file first, then `mv` it over the original.
6. **Resume from checkpoint.** At session start, check for an existing state
   file: `ls .workflow-state/*-state.json`. If found, read it with `view`,
   parse the `current_stage`, and skip all stages marked `done`. Report to
   the user: "Resuming from stage N: <name>. Stages 1–(N-1) already done."
7. **Handle version migration.** If the checkpoint schema `version` does not
   match the expected version, run a migration function: read old fields,
   map them to new fields, bump the version, and write a new file. Keep the
   old file as `<name>-state.v<old>.json.bak`.
8. **Clean up on completion.** When all stages are done, move the state file
   to `.workflow-state/archive/` with a timestamp suffix. Do not delete it —
   it serves as an audit trail.

# Decision rules

- Always write state to a file, never rely on SQL session tables alone
  (those are ephemeral to the CLI session, not the workflow).
- Use JSON for state files — not YAML, not plain text — to enable
  programmatic parsing on resume.
- If two sessions attempt to resume the same workflow, compare `updated_at`
  timestamps; the newer one wins. Archive the older state file with a
  `conflict-` prefix.
- Checkpoint frequency: after every stage completion by default. The user
  may lower this to every N stages for fast workflows.
- Never store secrets, tokens, or credentials in the state file.

# Output requirements

1. **State File** — a valid JSON checkpoint at the defined storage location.
2. **Resume Report** — on session start, a brief summary of what was
   recovered: stages done, current stage, any conflicts resolved.
3. **Archive Entry** — on workflow completion, the final state file moved to
   the archive directory.

# References

- `references/checkpoint-rules.md` — canonical checkpoint format and rules.
- `references/delegate-contracts.md` — how sub-agents reference shared state.

# Related skills

- `verification-before-advance` — gate checks that update stage status.
- `subagent-research-patterns` — research results stored as state artifacts.
- `swarm-patterns` — shared blackboard state for multi-agent coordination.
- `session-resume-rehydration` — higher-level session resume orchestration.

# Failure handling

- **State file is corrupted (invalid JSON):** Attempt to recover by parsing
  with `python3 -c "import json; …"` in lenient mode. If unrecoverable,
  rename the file with a `.corrupt` suffix, start a fresh state, and warn
  the user about lost progress.
- **Storage location is read-only:** Fall back to `/tmp/workflow-state/` and
  warn the user that state will not survive a machine restart.
- **Conflicting concurrent writes:** Use the `updated_at` comparison rule.
  If timestamps are identical (rare), prompt the user to choose which state
  to keep.
- **Missing state file on resume:** Treat as a fresh start. Do not error —
  log "No prior state found, beginning from stage 1."
