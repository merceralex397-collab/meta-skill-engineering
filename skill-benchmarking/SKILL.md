---
name: skill-benchmarking
description: >-
  Compare skill variants head-to-head using pass rate, routing accuracy, and
  usefulness score to pick a winner. Use when choosing between two skill versions
  ("which is better?", "did the refinement help?", "benchmark these variants"),
  measuring whether a change improved quality, or deciding whether to keep or
  deprecate a variant. Do not use for evaluating a single skill in isolation (use
  skill-evaluation) or for building test infrastructure (use
  skill-testing-harness).
---

# Purpose

Compare two or more skill variants (A vs B, before vs after) on the same test cases. Produce a summary table with pass rate, routing accuracy, and usefulness score, then recommend which variant to keep.

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
- Quick spot-check, not systematic comparison → `skill-evaluation` (single-skill mode)

# Procedure

1. **Define the comparison**
   - Identify variants: A vs B, or before vs after. For skill vs no-skill baseline evaluation, use `skill-evaluation` instead.
   - Choose metrics: pass rate, routing accuracy, usefulness score. Drop metrics irrelevant to the decision.
   - Set minimum sample size (N ≥ 10 per variant).

2. **Select benchmark cases**
   - Reuse existing eval cases if available.
   - Cover typical, edge, and adversarial inputs.
   - Use identical cases across all variants — never mix.

3. **Collect metrics**

   Use automated tooling to collect metrics consistently:

   ```bash
   ./scripts/run-evals.sh <skill-name>                                     # Trigger + behavior tests
   ./scripts/run-evals.sh --usefulness <skill-name>                        # Add LLM-judged quality
   ./scripts/run-baseline-comparison.sh <variant-a.md> <variant-b.md>     # Before/after comparison
   ```

   - **Pass rate**: % of trigger-positive and behavior cases meeting acceptance criteria.
   - **Routing accuracy**: Precision and recall from trigger-positive and trigger-negative eval sets.
   - **Usefulness score**: Average LLM-judged quality score (1–5) from `--usefulness` runs. Compare scores between variants to determine which produces higher-quality output.

   **Usefulness comparison method:**
   - Run `--usefulness` evals for each variant under identical conditions.
   - Compare the average usefulness scores directly. A variant with a meaningfully higher score (≥ 0.5 points) is the winner on quality.
   - **Scoring rubric** (applied by LLM judge per case):
     - *Correctness*: Is the output factually right and free of hallucination?
     - *Completeness*: Are all required sections and elements present?
     - *Conciseness*: Is there unnecessary padding, repetition, or filler?
     - *Actionability*: Could someone act on this output without further clarification?
   - **Ties**: If the usefulness score difference is < 0.5 points and pass rates are within noise, favor the simpler variant. Do not force a winner when there isn't one.
   - **>2 variants**: Compare usefulness scores pairwise (A vs B, A vs C, B vs C). Tally wins per variant across all pairs.

4. **Assess significance**
   - Pass rate: Is the difference > 5 percentage points?
   - Usefulness score: Is the difference ≥ 0.5 points?
   - If all metrics are within noise, declare tie.

5. **Produce benchmark report**
   - Fill in the output contract below.

# Output contract

Produce exactly this structure:

```
## Benchmark: [Variant A] vs [Variant B]

### Summary
| Metric           | A     | B     | Winner |
|------------------|-------|-------|--------|
| Pos Trigger %    | 85%   | 92%   | B      |
| Neg Reject %     | 95%   | 90%   | A      |
| Behavior Pass %  | 80%   | 88%   | B      |
| Usefulness Score  | 3.2   | 4.1   | B      |
| Cases Tested     | 15    | 15    | —      |

### Breakdown
[Results by category if cases span multiple categories. Omit if single category.]

### Significance
- Pass rate delta: [X]pp — [meaningful | within noise]
- Usefulness delta: [X] points — [meaningful | within noise]

### Recommendation
**Keep [winner]**. [Deprecate | archive] [loser].
Rationale: [one sentence explaining the deciding factor]
```

# Failure handling

- **Fewer than 10 cases per variant**: Run anyway but mark results as preliminary. State the sample size and warn that conclusions may not hold.
- **Metrics too close to call**: Recommend keeping the simpler or smaller variant as tiebreaker. Never force a winner when differences are within noise.
- **Variants serve different purposes**: Do not force a single winner. Document which contexts favor each variant and recommend keeping both with routing guidance.
- **Missing acceptance criteria**: Ask the user to define pass/fail before running. Do not invent criteria.

# Next steps

After benchmarking:
- Improve the weaker variant → `skill-improver`
- Deprecate the losing variant → `skill-lifecycle-management`
