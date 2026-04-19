---
name: approval-gates
description: Insert explicit approval gates before risky, destructive, expensive, or irreversible transitions in an autonomous workflow. Use when the task crosses a trust boundary such as production rollout, deletion, mass refactor, network spend, external publication, or policy-sensitive change, and when the workflow needs a clear stop-until-approved checkpoint. Do not use for low-risk local edits with no meaningful blast radius.
---

# Purpose

Use this skill to stop automation at the right moments instead of after damage.

# When to use this skill

Use this skill when:

- a workflow includes destructive or irreversible operations
- the next stage spends meaningful money, time, or trust
- a publish, merge, rollout, or external-facing action is about to happen
- the process needs a human checkpoint before advancing

# Do not use this skill when

- the action is low-risk and reversible
- the approval adds ceremony without reducing risk

# Operating procedure

1. Identify the transition boundary.
   Name the exact action that should not happen automatically.

2. State the approval reason.
   Tie the gate to risk: destructive change, blast radius, user impact, spend, or compliance.

3. Define the pre-gate evidence.
   List what must be present before asking for approval.

4. Freeze the workflow at the gate.
   Do not speculate that approval will be granted. Stop cleanly and summarize what is ready.

5. State the post-approval action.
   Make it obvious what happens only after approval arrives.

# Decision rules

- Add a gate for irreversible deletion, production-impacting rollout, external publication, or sensitive permission change.
- Do not add a gate just because a task is large; tie it to actual risk.
- If evidence is missing, do not ask for approval yet. First gather the missing receipts.

# Output requirements

Return:

1. `Approval Gate`
2. `Why Approval Is Required`
3. `Evidence Ready`
4. `Action After Approval`

# References

- Read `references/gate-triggers.md` for common approval boundaries.
- Read `references/pre-approval-evidence.md` for receipt expectations.
- Read `references/approval-message-template.md` for a compact gate summary.

# Failure handling

- If the action is risky but no gate exists in the plan, add one and explain why.
- If the user explicitly waives a low-value gate, note that and proceed.
- If the workflow already crossed the boundary without approval, flag that as a process failure.
