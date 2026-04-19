---
name: skill-benchmarking
description: >-
  Compare skill variants head-to-head using pass rate, token usage, and win rate
  to pick a winner. Use when choosing between two skill versions ("which is
  better?", "did the refinement help?", "benchmark these variants"), measuring
  whether a change improved quality, or deciding whether to keep or deprecate a
  variant. Do not use for evaluating a single skill in isolation (use
  skill-evaluation) or for building test infrastructure (use
  skill-testing-harness).
---

# Purpose

Compare two or more skill variants (A vs B, before vs after, skill vs no-skill) on the same test cases. Produce a summary table with pass rate, token usage, and win rate, then recommend which variant to keep.

# When to use

- Two or more skill variants exist and one must be chosen
- A skill was refined and the change needs measured impact
- User says "which is better?", "did this help?", "benchmark these"
- Periodic audit to cull underperforming variants
- Justifying whether skill maintenance investment is paying off

# When NOT to use

- Only one skill, no variant to compare → `skill-evaluation`
- Need to build test cases or harness → `skill-testing-harness`
- Skill is broken and needs fixing → `skill-improver`
- Quick spot-check, not systematic comparison

# Procedure

1. **Define the comparison**
   - Identify variants: A vs B, before vs after, skill vs no-skill baseline.
   - Choose metrics: pass rate, token usage, win rate. Drop metrics irrelevant to the decision.
   - Set minimum sample size (N ≥ 10 per variant).

2. **Select benchmark cases**
   - Reuse existing eval cases if available.
   - Cover typical, edge, and adversarial inputs.
   - Use identical cases across all variants — never mix.

3. **Collect metrics**
   - **Pass rate**: % of cases meeting acceptance criteria.
   - **Token usage**: Average input + output tokens per case.
   - **Routing accuracy**: Precision and recall if routing is in scope.
   - **Win rate**: For each case, judge which variant produced better output (A / B / Tie). Calculate overall win percentage.

   **Win-rate judging method:**
   - **Blind comparison**: Strip skill-name indicators and variant labels from outputs before comparing. Present as "Output A" and "Output B" so judgment isn't biased by which variant is "new".
   - **Scoring rubric** (apply to each case independently):
     - *Correctness*: Is the output factually right and free of hallucination?
     - *Completeness*: Are all required sections and elements present?
     - *Conciseness*: Is there unnecessary padding, repetition, or filler?
     - *Actionability*: Could someone act on this output without further clarification?
   - **Ties**: If the quality difference is marginal across all rubric dimensions, favor the shorter or cheaper output (token efficiency as tiebreaker). Do not force a winner when there isn't one.
   - **>2 variants**: Use round-robin pairwise comparison (A vs B, A vs C, B vs C), not free-for-all ranking. Tally wins per variant across all pairs. This avoids the "middle option bias" that free-form ranking introduces.

4. **Assess significance**
   - Pass rate: Is the difference > 5 percentage points?
   - Token usage: Is the difference > 10%?
   - Win rate: Is it meaningfully above 50%?
   - If all metrics are within noise, declare tie.

5. **Produce benchmark report**
   - Fill in the output contract below.

# Output contract

Produce exactly this structure:

```
## Benchmark: [Variant A] vs [Variant B]

### Summary
| Metric         | A     | B     | Winner |
|----------------|-------|-------|--------|
| Pass Rate      | 85%   | 92%   | B      |
| Avg Tokens     | 1200  | 980   | B      |
| Win Rate       | 35%   | 65%   | B      |
| Cases Tested   | 15    | 15    | —      |

### Breakdown
[Results by category if cases span multiple categories. Omit if single category.]

### Significance
- Pass rate delta: [X]pp — [meaningful | within noise]
- Token delta: [X]% — [meaningful | within noise]
- Win rate: [X]% — [above | at | below] 50%

### Recommendation
**Keep [winner]**. [Deprecate | archive] [loser].
Rationale: [one sentence explaining the deciding factor]
```

# Failure handling

- **Fewer than 10 cases per variant**: Run anyway but mark results as preliminary. State the sample size and warn that conclusions may not hold.
- **Metrics too close to call**: Recommend keeping the simpler or smaller variant as tiebreaker. Never force a winner when differences are within noise.
- **Variants serve different purposes**: Do not force a single winner. Document which contexts favor each variant and recommend keeping both with routing guidance.
- **Missing acceptance criteria**: Ask the user to define pass/fail before running. Do not invent criteria.

## Next steps

After benchmarking:
- Improve the weaker variant → `skill-improver`
- Deprecate the losing variant → `skill-lifecycle-management`
- Package the winner → `skill-packaging`
