---
name: tradeoff-analysis
description: Compare explicit options across criteria such as cost, complexity, speed, risk, reversibility, lock-in, and operational burden. Use when the task says "compare these options", "what are the tradeoffs", "which approach should we pick", "decision matrix", or when the user needs a reasoned recommendation between concrete alternatives. Do not use when there is only one plan to critique or when success criteria are still undefined.
---

# Purpose

Use this skill to turn "it depends" into a defensible decision.

The output should not be a vague pros/cons list. It should identify the decision question, filter out options that fail non-negotiables, compare the remaining options on criteria that matter, and recommend one with explicit regrets.

# When to use this skill

Use this skill when:

- the user has two or more concrete options
- the decision involves tradeoffs across speed, correctness, cost, risk, or maintainability
- a team needs a recommendation that can survive review
- the task calls for a decision matrix or structured comparison

Strong trigger phrases:

- "tradeoff analysis"
- "compare these options"
- "which approach should we pick"
- "decision matrix"
- "pros and cons are not enough"

# Do not use this skill when

- there is no real option set yet; help generate options first
- the job is to critique the safety of a chosen execution plan; use plan review
- the job is to imagine failure scenarios after a choice is already made; use premortem

# Inputs expected

Prefer:

- a decision question
- 2 or more named options
- explicit constraints or non-negotiables
- criteria that matter to the decision

If criteria are missing, infer a draft set and label the inference.

# Operating procedure

1. State the decision question.
   Example: "Should we use expand-contract migration, dual writes, or a hard cutover?"

2. Identify non-negotiables.
   These are pass/fail gates, not weighted preferences. Examples: regulatory requirement, downtime limit, team skill constraint, deadline ceiling.

3. Remove invalid options.
   If an option fails a non-negotiable, say so early instead of letting weighted scoring hide it.

4. Compare the surviving options.
   Use criteria such as:
   - delivery speed
   - correctness and safety
   - operational burden
   - reversibility
   - team familiarity
   - cost
   - vendor or architectural lock-in

5. Examine second-order effects.
   A useful tradeoff analysis names not only what is expensive now, but what becomes expensive later: maintenance, migration difficulty, observability burden, support complexity, or future product constraints.

6. Recommend one option.
   Include:
   - why it wins
   - what regret remains
   - what would change the decision

# Decision rules

- Separate hard constraints from weighted preferences.
- Do not reward an option for being fast if it fails a safety or compliance constraint.
- Prefer reversible options when uncertainty is high and the cost delta is acceptable.
- Prefer boring options when the "clever" option only wins on elegance.
- If two options are close, say what evidence or experiment would break the tie.
- If the decision depends mostly on an unknown assumption, say that the next step is validation, not selection theater.

# Output requirements

Return:

1. `Decision Question`
2. `Constraints`
3. `Option Comparison`
4. `Recommendation`
5. `Decision Reversal Triggers`

If useful, include a small table.

# Scripts

Use `scripts/decision_matrix.py` when the options and criteria are already structured and a weighted score helps.

Example input:

```json
{
  "criteria": [
    {"name": "speed", "weight": 2},
    {"name": "safety", "weight": 4},
    {"name": "operational_burden", "weight": 3}
  ],
  "options": [
    {"name": "hard-cutover", "scores": {"speed": 5, "safety": 1, "operational_burden": 2}},
    {"name": "expand-contract", "scores": {"speed": 3, "safety": 5, "operational_burden": 4}}
  ]
}
```

The script is for structured scoring only. It does not choose the criteria for you.

# References

- Read `references/criteria-library.md` when the criteria list is weak or unbalanced.
- Read `references/bias-checks.md` to avoid choosing an option for the wrong reason.
- Read `references/output-template.md` for a compact recommendation shape.
- RFC 7282 (https://www.rfc-editor.org/rfc/rfc7282.html) — ensuring objections are addressed before consensus, not just outvoted

# Failure handling

- If there is only one real option, say this is not a tradeoff analysis and redirect to plan review or assumptions audit.
- If the criteria are obviously partisan or incomplete, say so and repair the matrix before recommending.
- If the decision depends on unknown facts, return the cheapest experiment or validation step needed before committing.

# Named failure modes of this method

- **Criteria rigging**: Choosing criteria that favor the preferred option. Fix: let the decision question drive the criteria, not the answer.
- **Score washing**: Assigning weighted scores that obscure the real tradeoff (e.g., giving "team familiarity" weight 5 to justify the boring option). Fix: justify each weight before scoring options.
- **Omitting the regret**: Recommending an option without stating what is sacrificed. Fix: every recommendation must name the regret—what downside you are accepting.
- **False precision**: Scoring options 3.7 vs 3.8 when the real uncertainty is ±2. Fix: if scores are close, say the options are close and name the tiebreaker experiment.
- **Missing reversal triggers**: Recommending an option without stating what would change the decision. Fix: always include decision reversal triggers (step 6 in output).
