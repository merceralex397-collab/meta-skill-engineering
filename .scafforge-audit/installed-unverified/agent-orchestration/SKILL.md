---
name: agent-orchestration
description: Coordinate multi-agent or multi-stage work so each delegate has a bounded scope, clear artifact contract, and explicit handoff point. Use when the task is large enough for parallel or staged delegation, when one agent must gather evidence while another implements, or when an orchestrator needs to choose the next delegate instead of doing everything locally. Do not use when the task is simple enough to execute directly.
---

# Purpose

Use this skill to keep delegation useful instead of theatrical.

# When to use this skill

Use this skill when:

- the task spans distinct subproblems with different skills
- you need sidecar research while implementation continues
- a lead agent must choose sequence, handoff, or merge points
- multiple delegates could help but only if their outputs are bounded

# Do not use this skill when

- one agent can finish the job directly with less overhead
- the next local step depends on a result that should be done immediately, not delegated
- the only reason to delegate is to look busy

# Operating procedure

1. Hold the critical path locally.
   Identify the next blocking action and keep it with the lead agent unless there is a strong reason not to.

2. Split by responsibility, not by hope.
   Delegates should have disjoint outputs: exploration, implementation slice, verification pass, or research question.

3. Define the artifact contract.
   For each delegate, specify:
   - exact question or change scope
   - files or surfaces in scope
   - expected output shape
   - stop conditions

4. Choose sequence or parallelism deliberately.
   Parallelize only independent work. If task B depends on task A’s answer, do not pretend they are parallel.

5. Reintegrate before launching more delegates.
   Synthesize results, resolve conflicts, then decide the next stage.

# Decision rules

- Delegate sidecar work, not the immediate blocker.
- Prefer one strong delegate over a noisy swarm when the task is tightly coupled.
- Do not issue overlapping write scopes to separate implementers.
- If a delegate returns weak evidence, fix the contract before delegating again.

# Output requirements

Return:

1. `Critical Path`
2. `Delegates`
3. `Artifact Contracts`
4. `Rejoin Point`

# References

- Read `references/delegate-shapes.md` for common delegate roles.
- Read `references/parallelism-rules.md` before running work in parallel.
- Read `references/handoff-checklist.md` before integrating results.

# Failure handling

- If delegation overhead is greater than local execution, say so and do the work directly.
- If two delegates would touch the same write surface, redesign the split or keep the work local.
- If a delegate comes back with status but not evidence, do not advance the workflow.
