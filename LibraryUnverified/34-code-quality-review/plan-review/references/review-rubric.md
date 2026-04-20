# Review Rubric

Use this rubric to decide whether the plan is ready.

## Core checks

| Dimension | Pass signal | Warn signal | Fail signal |
| --- | --- | --- | --- |
| Goal clarity | Done-state is observable | Goal is implied but fuzzy | No concrete done-state |
| Preconditions | Inputs, access, and dependencies are named | Some prerequisites are implied | Critical prerequisites are absent |
| Sequencing | Order respects dependency flow | A few ordering edges are unclear | Consumer appears before prerequisite |
| Verification | Each phase has a receipt | Some steps have weak receipts | Major phase has no evidence model |
| Rollback | Reversible path or containment exists | Rollback is partial | Irreversible step has no mitigation |
| Ownership | Someone or something owns each gate | Owners missing on minor steps | Critical gates have no owner |
| Operational safety | Monitoring and blast radius are addressed | Monitoring exists but is thin | Production-impacting step lacks controls |

## Severity guidance

- `critical`: execution should stop until fixed
- `major`: plan can proceed only with explicit conditions
- `minor`: quality gap that does not break the plan on its own

Escalate to `critical` when the gap makes failure likely and hard to detect early.
