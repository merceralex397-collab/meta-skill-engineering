---
name: assumptions-audit
description: Extract and audit hidden assumptions in a plan, brief, RFC, ticket, architecture note, or product requirement. Use when the task says "what assumptions are we making", "what has to be true", "audit this proposal for hidden assumptions", "what are we taking for granted", or when a plan looks too clean because its dependencies, users, resourcing, or environment constraints are implied rather than stated. Do not use for generic code review or for comparing explicit options.
---

# Purpose

Use this skill to expose what a document silently depends on.

Most plans do not fail only because of bad steps. They fail because the author assumes permissions, data quality, team capacity, vendor behavior, migration windows, user behavior, or policy approval without writing any of that down.

# When to use this skill

Use this skill when:

- a plan or design looks plausible but feels under-specified
- the user explicitly asks what assumptions need validation
- a ticket depends on external systems, teams, approvals, or unknown inputs
- implementation risk is driven by uncertainty rather than by obvious sequencing

Strong trigger phrases:

- "assumptions audit"
- "what are we assuming"
- "what has to be true"
- "what are we taking for granted"
- "find the hidden assumptions"

# Do not use this skill when

- the task is deciding between explicit alternatives; use tradeoff analysis
- the task is reviewing the order and gates of a plan; use plan review
- the task is imagining future failure scenarios after the assumptions are already known; use premortem

# Inputs expected

Useful source material includes:

- an RFC or architecture note
- a plan or rollout document
- a ticket or epic description
- a product brief
- a migration proposal

If the input is only a single sentence, the output should be a short assumption inventory, not a fake full audit.

# Operating procedure

1. Read for claims, not just tasks.
   Highlight statements that imply "this will work because..." even when the because-clause is unwritten.

2. Classify each assumption.
   Use the categories in `references/assumption-taxonomy.md`:
   - user or product behavior
   - dependency or integration behavior
   - operational environment
   - data quality or measurement
   - resourcing or ownership
   - policy, legal, or security approval

3. Mark support level.
   For each assumption, decide whether it is:
   - `explicitly-supported`
   - `plausible-but-unproven`
   - `contradicted`
   - `unknown`

4. Rank danger.
   Prioritize using three questions:
   - how wrong could this be
   - how much damage would it cause
   - how late would we discover it

5. Convert dangerous assumptions.
   For high-risk assumptions, turn them into one of:
   - a validation question
   - an approval gate
   - a prerequisite task
   - a monitoring requirement
   - an explicit note in the plan

6. Return an assumption register.
   Keep it small and sharp. The goal is not to catalog every background fact. The goal is to isolate the assumptions that can break execution.

# Decision rules

- Prefer assumptions with high blast radius over assumptions that are merely interesting.
- Treat external vendor behavior, data cleanliness, and cross-team coordination as higher-risk than local code details unless the task context proves otherwise.
- If an assumption is already supported by a cited document, keep it in the register but lower its risk.
- If a statement is actually a decision, do not call it an assumption; capture only what that decision still relies on.
- If the plan says "we can just..." or "we only need to...", look for the hidden dependency being compressed by that phrase.

# Output requirements

Return:

1. `Top Assumptions`
2. `Why They Are Risky`
3. `Evidence Status`
4. `Required Conversions`

Use a table when there are more than three assumptions.

# Scripts

This skill is judgment-heavy. Do not add or use a script unless the input already exists in a structured register that needs deterministic reshaping.

# References

- Read `references/assumption-taxonomy.md` to classify assumptions cleanly.
- Read `references/danger-signals.md` when the source document feels overconfident or compressed.
- Read `references/conversion-patterns.md` when turning assumptions into gates, questions, or tasks.

# Failure handling

- If the source material is too thin, return only the highest-probability assumptions and say the audit is shallow because the source is shallow.
- If you cannot tell whether something is an assumption or a known fact, mark it as `unknown` and identify the evidence needed.
- If the document is already explicit and well-supported, say that the audit found low hidden-assumption risk instead of forcing drama into the output.

# Named failure modes of this method

- **Assumption inflation**: Listing obvious background facts ("we assume users have internet access") alongside genuinely hidden dependencies, making the real risks hard to find. Fix: only include assumptions with non-trivial blast radius.
- **Taxonomy without ranking**: Classifying assumptions by category but not by danger, producing a flat list that doesn't guide action. Fix: always rank by the three danger questions (step 4).
- **Conversion failure**: Identifying dangerous assumptions but not converting them into gates, tasks, or monitoring—the audit becomes an academic exercise. Fix: every high-risk assumption must produce a conversion (step 5).
- **Assumption-decision confusion**: Calling a deliberate choice an "assumption" when it was already decided. Fix: if it was an explicit decision, capture what that decision still depends on, not the decision itself.
