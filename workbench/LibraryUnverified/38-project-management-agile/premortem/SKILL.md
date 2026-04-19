---
name: premortem
description: Run a premortem on a proposed project, rollout, architecture, or major change by assuming it failed and working backward to the most plausible causes. Use when the task says "premortem", "what could go wrong before we start", "assume this fails in six months", "give me the likely failure modes", or when a plan needs proactive risk discovery rather than post-hoc analysis. Do not use for code review or for root-cause analysis after a real incident.
---

# Purpose

Use this skill to surface plausible future failure before the work begins.

Unlike an assumptions audit, this skill starts from a fictional future failure and forces the review to explain how the failure happened, what signals would have appeared early, and what can still be done now.

# When to use this skill

Use this skill when:

- a project or rollout is important enough that rework or outage would be expensive
- the team wants likely failure modes before committing
- the plan seems optimistic and needs an adversarial risk pass
- the user explicitly asks what could go wrong

Strong trigger phrases:

- "premortem"
- "what could go wrong"
- "assume this fails"
- "how does this project die"
- "likely failure modes"

# Do not use this skill when

- the failure already happened and you need actual incident analysis; use root-cause analysis instead
- the problem is comparing options rather than discovering risks inside a chosen path; use tradeoff analysis
- the problem is extracting hidden dependencies without framing failure scenarios; use assumptions audit

# Inputs expected

Best inputs:

- a plan or roadmap
- a migration or rollout doc
- an architecture decision
- a product proposal

If the scope is huge, pick one concrete failure horizon and one target outcome first.

# Operating procedure

1. Lock the failure statement.
   Write a single sentence such as: "Three months after launch, this initiative is judged a failure because ..."

2. Pick the horizon.
   Use a concrete review window such as:
   - first deploy
   - first month after launch
   - first quarter of operation

3. Generate failure modes.
   Cover at least these buckets when relevant:
   - product or user adoption
   - engineering implementation
   - integration or dependency behavior
   - operations and observability
   - data correctness
   - security, compliance, or approval
   - team/process execution

4. Force plausibility.
   For each failure mode, explain why it could plausibly happen given the current plan rather than inventing random disasters.

5. Identify leading indicators.
   Name the early warning signs that would show up before the full failure lands.

6. Convert into controls.
   For the highest-risk failure modes, define:
   - preventive action
   - detection method
   - containment or rollback
   - owner or decision point

7. Return the risk register.
   Keep the output ranked. The goal is to identify the failures most worth defending against now.

# Decision rules

- Prefer probable, boring failure over cinematic failure.
- Treat late-discovered failures as more dangerous than early-discovered ones at the same impact level.
- Do not spend the whole review on edge-case catastrophes if the plan is more likely to fail on ownership, rollout discipline, data quality, or integration assumptions.
- If the failure mode is already addressed by the plan, note the existing control instead of double-counting the risk.
- Escalate risks that are both high-impact and hard to observe early.

# Output requirements

Return:

1. `Failure Statement`
2. `Top Failure Modes`
3. `Leading Indicators`
4. `Preventive Controls`
5. `Open Risks`

Prefer a table for the main failure modes.

# Scripts

This skill is reasoning-first. Do not use or add automation unless a structured risk register already exists and only needs deterministic rescoring.

# References

- Read `references/failure-catalog.md` when you need a starter set of plausible failure classes.
- Read `references/leading-indicators.md` to sharpen detection before failure becomes visible to users.
- Read `references/report-template.md` when formatting a concise premortem register.
- Google SRE Postmortem Culture (https://sre.google/workbook/postmortem-culture/) — blameless analysis and learning from failure

# Failure handling

- If the scope is too broad, reduce it to one initiative, one rollout, or one milestone instead of producing generic fear.
- If the plan is too early for detailed risk analysis, return only the highest-level failure modes and the missing information needed for a deeper pass.
- If the user asks for absolute certainty, state clearly that premortems rank plausibility; they do not predict the future.

# Named failure modes of this method

- **Cinematic failure bias**: Generating dramatic, unlikely failure scenarios (datacenter fire, zero-day exploit) while missing boring, probable failures (missed deadline, data quality issue, team turnover). Fix: prefer probable, boring failure over cinematic failure.
- **Risk listing without controls**: Producing a fear inventory but not converting top risks into preventive actions, detection methods, or containment plans. Fix: always convert (step 6).
- **Unfocused horizon**: Trying to premortem everything from first deploy to 5-year operation. Fix: lock a single failure horizon (step 2) before generating failure modes.
- **Existing control blindness**: Flagging risks that the plan already addresses, producing a pessimistic report. Fix: check the plan's existing controls before adding a risk to the register.
- **Equal-weight risks**: Listing all failure modes without ranking, making it impossible to prioritize. Fix: always rank by plausibility and lateness of discovery.
