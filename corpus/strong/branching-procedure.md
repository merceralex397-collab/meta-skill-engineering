---
name: branching-procedure-example
description: >
  Decomposes a user-facing feature request into implementation tasks with
  dependency ordering, size estimates, and acceptance criteria — producing a
  ready-to-execute task breakdown for a single developer or small team.
---

# Feature Task Decomposition

## Purpose

Take a feature request (user story, product brief, or informal description)
and break it into concrete implementation tasks that a developer can pick
up without further planning. Each task has a clear scope, acceptance
criteria, dependency links, and a rough size estimate.

## When to use

- A product owner or tech lead hands over a feature and says "break this
  down into tasks."
- Sprint planning needs task-level granularity from a high-level story.
- A solo developer wants to plan their own work before starting.

## When NOT to use

- For architecture decisions or system design — use `architecture-advisor`.
- For breaking a large project into epics/milestones — that is higher-level
  scoping, not task decomposition.
- For bug triage — bugs are already scoped; decomposition adds overhead.

# Procedure

## 1. Parse the feature request

Extract:
- **Goal**: what the user/system should be able to do after this ships
- **Constraints**: deadline, tech stack, backward compatibility
- **Dependencies**: external APIs, other teams, prerequisite work
- **Acceptance criteria**: how do we know it's done?

If any of these are missing, ask the user before proceeding.

## 2. Identify task boundaries

Decompose by functional slice, not by layer:

| Approach | When to use |
|----------|-------------|
| Vertical slice | Default — each task delivers a thin end-to-end capability |
| Horizontal slice | When a shared foundation must exist before any feature work (e.g., schema migration) |
| Spike + implement | When uncertainty is high — first task is a timeboxed investigation |

Rules:
- Each task must be completable in ≤ 1 day of focused work.
- If a task is larger, split it further.
- If a task is < 1 hour, merge it into an adjacent task.

## 3. Order by dependency

Build a dependency graph:
- Tasks with no dependencies come first
- Tasks that unblock the most downstream work get higher priority
- If two tasks are independent, order by risk (riskier first)

Flag circular dependencies as a decomposition error — restructure.

## 4. Estimate size

Use t-shirt sizes mapped to rough hours:

| Size | Hours | Meaning |
|------|-------|---------|
| XS   | < 1   | Trivial — merge into another task |
| S    | 1–2   | Simple, well-understood |
| M    | 2–4   | Moderate complexity, clear approach |
| L    | 4–8   | Full day, may hit unknowns |
| XL   | > 8   | Too large — must be split further |

## 5. Write acceptance criteria per task

Each task gets 2–4 acceptance criteria in the format:
"Given [context], when [action], then [result]."

Avoid vague criteria like "works correctly" — specify observable outcomes.

# Output contract

```
## Task Breakdown: [feature name]

### Summary
- Tasks: N (XS: a, S: b, M: c, L: d)
- Estimated total: X–Y hours
- Critical path: [task-1 → task-3 → task-5]

### Tasks

#### T1: [title]
- **Size**: [S/M/L]
- **Depends on**: [none | T-id list]
- **Acceptance criteria**:
  - Given ..., when ..., then ...
  - Given ..., when ..., then ...
- **Notes**: [optional context]

[repeat for each task]

### Dependency Graph
T1 → T3 → T5
T2 → T4 ↗
```

# Failure handling

| Situation | Action |
|-----------|--------|
| Feature request is too vague to decompose | List what is missing (goal, scope, constraints) and ask the user to clarify before proceeding. |
| Cannot decompose below 1-day granularity | Mark the task as "needs spike" with a timebox. The spike task's output is a revised decomposition. |
| Circular dependency detected | Restructure: extract the shared concern into a new prerequisite task. |
| Estimated total exceeds available time | Flag the gap. Suggest scope reduction with specific tasks to defer. |
| Feature spans multiple teams | Identify the cross-team boundary. Split into team-scoped task sets with explicit handoff points. |

# Next steps

- After decomposition: create tickets from the task list
- If estimates are uncertain: run a spike task first, then re-estimate
- After implementation: compare actual vs estimated to calibrate future estimates
