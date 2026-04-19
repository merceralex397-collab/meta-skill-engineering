---
name: plan-review
description: Review a proposed execution plan, migration plan, rollout plan, implementation checklist, or design-to-build plan before implementation begins. Use when the task says things like "review this plan", "sanity check this rollout", "poke holes in my approach", "is this sequencing sound", or "what is missing before we start". Do not use for reviewing finished code, comparing multiple options before a plan exists, or turning vague goals into acceptance criteria.
---

# Purpose

Use this skill to judge whether a plan is executable, ordered correctly, and provable before work starts.

The job is not to admire the plan. The job is to find missing prerequisites, bad sequencing, fake verification, hidden irreversible steps, and undefined completion receipts.

# When to use this skill

Use this skill when:

- the input already contains a proposed approach, sequence, checklist, or rollout
- the user wants a readiness review before implementation starts
- a ticket, RFC, or migration note has steps but may be missing safety or verification
- a multi-step plan needs critique for dependencies, validation, rollback, or ownership

Strong trigger phrases:

- "review this plan"
- "sanity check this rollout"
- "is this sequencing sound"
- "what are the holes in this approach"
- "what did I miss before implementation"
- "can we execute this safely"

# Do not use this skill when

- the task is reviewing finished code or a merged change; use the review workflow instead
- the task is deciding between several candidate options; use tradeoff analysis first
- the task is turning vague success language into testable completion criteria; use acceptance-criteria hardening
- the task is extracting hidden assumptions without judging the full execution order; use assumptions audit

# Inputs expected

Prefer to work from one of these:

- a numbered plan
- a ticket with implementation steps
- an RFC or design doc with phases
- a release or migration runbook

If the input is messy, normalize it into:

1. goal and done-state
2. ordered steps
3. prerequisites and dependencies
4. proof or verification receipts
5. rollback or containment path
6. open questions

# Operating procedure

1. Normalize the plan.
   Extract the actual ordered steps, not the prose around them. If the input has no clear sequence, state that as a blocker.

2. Run the readiness sweep.
   Check for goal clarity, prerequisite coverage, sequencing, parallel-lane safety, verification, rollback, observability, and ownership.

3. Stress the order of operations.
   Look for prerequisites arriving after their consumers, irreversible actions before backup or feature flagging, deploy-before-test patterns, and "we'll verify later" gaps.

4. Inspect proof quality.
   Every major phase needs a concrete receipt. Accept receipts like tests, queries, dashboards, screenshots, logs, health checks, or explicit sign-off. Reject vague phrases like "confirm it works" without saying how.

5. Inspect operational safety.
   For migrations, auth changes, infra changes, or production rollouts, look for rollback, blast-radius control, monitoring, and idempotence. Missing rollback on an irreversible change is a major issue.

6. Produce a verdict.
   Use one of:
   - `approved`
   - `approved-with-conditions`
   - `rework-required`

7. Repair the plan surface.
   Do not stop at criticism. Provide the minimal missing steps, gates, or receipts needed to make the plan executable.

# Decision rules

- Mark the plan `rework-required` if the done-state is undefined.
- Mark the plan `rework-required` if destructive or irreversible steps lack rollback, backup, or containment.
- Mark the plan `rework-required` if critical dependencies are discovered after the step that needs them.
- Mark the plan `rework-required` if success is defined only by completion of activity instead of evidence.
- Mark the plan `approved-with-conditions` when the sequence is mostly sound but a few proof gates, owners, or safeguards are missing.
- Treat parallel work as risky when two lanes modify the same artifact, environment, schema, or release boundary without an explicit merge protocol.
- Treat "test after deploy" as insufficient unless there is also a pre-release verification gate and a safe rollback path.

# Output requirements

Return a short, decision-ready review with these sections:

1. `Verdict`
2. `Critical Findings`
3. `Conditional Gaps`
4. `Missing Receipts`
5. `Suggested Plan Repair`

Use file or document references when the source material provides them.

# Scripts

Use `scripts/plan_checklist.py` when the plan is long, messy, or spread across several sections.

Example:

```bash
python scripts/plan_checklist.py --input docs/rollout.md --format text
```

The script is a heuristic aid. It highlights obvious omissions but does not replace judgment.

# References

- Read `references/review-rubric.md` for the readiness criteria and severity thresholds.
- Read `references/sequencing-anti-patterns.md` when the plan includes migrations, deploys, or parallel work.
- Read `references/output-template.md` when you need a compact response shape.

# Failure handling

- If there is no real plan yet, say so and ask for a concrete sequence instead of pretending to review one.
- If the plan is only fragments, reconstruct the smallest plausible ordered outline and clearly mark any assumptions used to do that.
- If dependencies or constraints are unknown, turn them into explicit blockers or approval gates rather than silently accepting the gap.

# Named failure modes of this method

- **Approval without teeth**: Rubber-stamping a plan as "approved" because it's long and detailed, without checking proof gates. Fix: every major phase must have a named receipt (step 4); if it doesn't, the plan is not approved.
- **Critique without repair**: Listing 10 problems but not showing how to fix them, leaving the author worse off than before. Fix: always provide the repair (step 7).
- **Sequencing blindness**: Checking individual steps but not their order—missing that step 5 depends on step 8's output. Fix: always stress the order of operations (step 3).
- **Scope confusion**: Reviewing the plan's goals instead of its execution readiness. Fix: this skill judges whether the plan can be executed safely, not whether the goals are correct.
- **Risk theater**: Flagging low-probability risks while missing the obvious missing rollback on an irreversible step. Fix: prioritize irreversible steps and missing rollback over speculative risks.
