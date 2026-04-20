---
name: session-resume-rehydration
description: Rehydrate interrupted work by reconstructing the active objective, current stage, changed files, open blockers, and next safe step from repo state and recent artifacts. Use when resuming after context loss, model restart, handoff, or user interruption, and when the next step must be rediscovered from evidence rather than memory. Do not use for fresh greenfield work with no prior session state.
---

# Purpose

Use this skill to restart from evidence instead of guesswork.

# When to use this skill

Use this skill when:

- the session was interrupted or compacted
- there is prior unfinished work in the repo
- a handoff note, branch, or ticket exists but context is stale
- the user says "pick this back up" or "continue where you left off"

# Do not use this skill when

- the task is brand new
- there is no prior work state to recover

# Operating procedure

1. Find the active task surface.
   Look for tickets, recent edits, handoff docs, branch state, or explicit work notes.

2. Rebuild the current stage.
   Determine whether the work is in understand, plan, implement, validate, or handoff.

3. Compare artifacts to claimed status.
   Do not trust old status text without checking changed files and validation results.

4. List blockers and uncertainty.
   State what is still unknown, what was attempted, and what needs reconfirmation.

5. Propose the next safe action.
   Resume with the smallest step that reduces uncertainty or continues the current stage cleanly.

# Decision rules

- Prefer repo evidence over memory.
- If the last known state and the current repo diverge, trust the current repo and note the mismatch.
- If validation status is unknown, assume it must be rerun or reconfirmed.
- If there is no clear active task, do not invent one; ask the user to point at the right objective.

# Output requirements

Return:

1. `Recovered Objective`
2. `Current Stage`
3. `Evidence Found`
4. `Unresolved State`
5. `Next Safe Step`

# References

- Read `references/recovery-signals.md` for common resume clues.
- Read `references/status-vs-evidence.md` to avoid trusting stale notes.
- Read `references/restart-checklist.md` for the minimum rehydration pass.

# Failure handling

- If the repo contains several unfinished strands, name them and ask which one to resume.
- If prior notes contradict current files, highlight the divergence instead of merging them silently.
- If no durable evidence exists, state that the restart surface is weak and reconstruction is partial.
