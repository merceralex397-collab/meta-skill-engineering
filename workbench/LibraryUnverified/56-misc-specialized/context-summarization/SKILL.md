---
name: context-summarization
description: Compress a large working context into a durable, restart-friendly summary that preserves active objective, decisions, changed surfaces, blockers, and next steps. Use when a session is long, handoff is needed, or the context window is getting noisy and a compact recovery surface would help. Do not use for user-facing narrative summaries where implementation state does not matter.
---

# Purpose

Use this skill to shrink context without losing the thread.

# Operating procedure

1. Extract the active objective.
2. Preserve decisions and constraints that still matter.
3. List changed files or surfaces if known.
4. Preserve blockers, open questions, and next step.
5. Omit fluff, repeated history, and resolved tangents.

# Decision rules

- Prefer restart utility over completeness.
- Preserve decisions, constraints, blockers, and next action before examples or rationale.
- If a fact is uncertain, mark it uncertain rather than smoothing it over.

# Output requirements

Return:

1. `Objective`
2. `State`
3. `Decisions`
4. `Blockers`
5. `Next Step`

# References

- Read `references/summary-order.md`.
- Read `references/what-to-drop.md`.
- Read `references/restart-utility.md`.
