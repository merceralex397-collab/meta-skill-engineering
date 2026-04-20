---
name: delegation-boundaries
description: Decide what should stay local versus what can be delegated, with special attention to blocking work, tightly coupled tasks, read-only dead ends, and overlapping write scopes. Use when an agent is considering delegation, when the workflow risks bouncing work between roles, or when you need to explain why a subtask should not be handed off. Do not use when delegation is not under consideration.
---

# Purpose

Use this skill to prevent bad delegation decisions before they happen.

# When to use this skill

Use this skill when:

- the next step might be delegated
- the workflow is at risk of over-delegating or under-delegating
- a role split is unclear
- a task may be too coupled or too blocking to hand off

# Do not use this skill when

- there is no live decision about delegation
- the workflow is already fixed and the only task is to execute it

# Operating procedure

1. Ask whether the next local step is blocked by the proposed delegate.
   If yes, prefer local execution.

2. Check coupling.
   If the task shape depends on ongoing local reasoning, avoid delegation.

3. Check scope quality.
   A good delegate task is concrete, bounded, independently useful, and materially advances the main task.

4. Check write scope.
   Never split overlapping edits across separate implementers without a clear merge owner.

5. Make the boundary explicit.
   State why the work stays local or why it is safe to delegate.

# Decision rules

- Keep urgent blockers local.
- Delegate independent information gathering freely if it will still be useful even when the answer is "nothing found".
- Do not delegate tasks whose output would be "an opinion on what to do next" unless that opinion is itself the requested artifact.
- Avoid read-only delegation for work that clearly requires code change to be useful.

# Output requirements

Return:

1. `Keep Local`
2. `Safe to Delegate`
3. `Unsafe Delegation Patterns`

# References

- Read `references/bad-delegation-patterns.md` for common failure cases.
- Read `references/local-vs-delegate-tests.md` for a quick boundary check.
- Read `references/write-scope-rules.md` for edit-surface safety.

# Failure handling

- If the task sits on the fence, bias toward local execution and explain why.
- If the delegate proposal is vague, rewrite it into a bounded contract before approving it.
