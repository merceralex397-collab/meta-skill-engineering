---
name: backlog-verifier
description: Verify that tickets, backlog items, and task packs are coherent, actionable, non-duplicative, and sequenced well enough for execution. Use when a user wants a backlog checked, when tickets may overlap, or when a task board needs a quality pass before agents start consuming it. Do not use for reviewing code or for writing a new plan from scratch.
---

# Purpose

Use this skill to make a backlog executable.

# Operating procedure

1. Read the backlog surface.
2. Check each item for scope, owner/actor, acceptance surface, and dependency clarity.
3. Find duplicates, gaps, and sequencing conflicts.
4. Return the minimum set of fixes needed before execution.

# Decision rules

- A ticket without an observable done-state is not ready.
- Two tickets that edit the same thing for the same reason are probably duplicates or need a split.
- If prerequisite work is missing, raise that before discussing polish.

# Output requirements

Return:

1. `Ready Items`
2. `Blocked or Weak Items`
3. `Duplicates or Overlap`
4. `Backlog Repair`

# References

- Read `references/readiness-checks.md`.
- Read `references/overlap-signals.md`.
- Read `references/repair-patterns.md`.
