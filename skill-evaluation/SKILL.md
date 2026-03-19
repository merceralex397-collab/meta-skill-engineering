---
name: skill-evaluation
description: >-
  Evaluate whether a single skill routes correctly and produces better output
  than a no-skill baseline — measuring trigger precision/recall and output win
  rate. Use when someone says "is this skill working?", "validate before
  promoting", "does this skill still add value?", "run the eval suite", or
  "regression test this skill". Supports both ad-hoc evaluation and running
  existing eval suites. Do not use for comparing multiple variants head-to-head
  (skill-benchmarking), building test infrastructure or eval suites
  (skill-testing-harness), or fixing a broken skill (skill-improver).
---

# Purpose

Produce quantitative evidence that a single skill adds value: it triggers on the right inputs, stays silent on wrong inputs, and improves output quality over the no-skill baseline.

# When to use

- "Is this skill working?" / "evaluate this skill" / "does this help?"
- "Run the eval suite" / "regression test this skill"
- New skill needs validation before promotion to stable
- Skill was refined and you need to verify the fix worked
- Skill has been modified and needs regression testing against its eval suite
- CI/pre-release validation requires documented eval results
- Periodic audit of whether an existing skill still adds value

# When NOT to use

- Comparing two or more skill variants head-to-head → `skill-benchmarking`
- Creating eval files, trigger tests, or test infrastructure → `skill-testing-harness`
- Skill is obviously broken or producing bad output → `skill-improver`

# Procedure

## Entry mode selection

Check whether the skill has an existing eval suite:
- If `evals/` directory exists with test files → use **Suite Mode** (Step 0)
- If no eval suite exists → use **Ad-hoc Mode** (start at Step 1)

## Step 0 — Suite mode: run existing eval suite

Locate test files in the skill directory. Supported formats:
- `evals/trigger-positive.jsonl` and `evals/trigger-negative.jsonl`
- `evals/triggers.yaml` (combined positive/negative)
- `evals/behavior.jsonl` or `evals/outputs.yaml`
- `evals/baselines.yaml`

For each trigger test case, run the prompt and record whether the skill fired.
For each behavior test case, run the skill and check against expected patterns.
For each baseline test case, compare skill-active vs skill-inactive output.

Calculate precision, recall, output pass rate, and baseline win rate.
Then skip to Step 6 to synthesize the verdict.

If some eval files are missing, note incomplete coverage and fall through to
ad-hoc mode for the missing test types.

## 1. **Define success criteria**

- Routing: triggers on positive cases, stays silent on negative cases
- Quality: outputs are correct, complete, well-formatted, no hallucination
- Baseline: outputs are better than running without the skill

## 2. **Prepare evaluation inputs**

- 5–10 positive trigger cases (should activate the skill)
- 5–10 negative trigger cases (should NOT activate)
- 3–5 quality cases for output assessment

**How to construct effective test cases:**
- **Positive cases**: Read the skill's "When to use" section. Each bullet becomes at least one test case using realistic phrasing. Then add paraphrased versions — formal ("Please evaluate this skill's effectiveness"), casual ("is this skill any good?"), and indirect ("I'm not sure this skill helps"). This tests routing robustness, not just keyword matching.
- **Negative cases**: Read the skill's "Do NOT use when" section. Each bullet becomes at least one test case. Then add near-miss cases drawn from adjacent skills' trigger phrases — these test whether the boundary is sharp. For example, if evaluating `skill-evaluation`, add trigger phrases from `skill-benchmarking` as negative cases.
- **Quality cases**: Use realistic, complete task prompts that exercise the full procedure — not just routing. Include at least one edge case where the skill must make a judgment call (e.g., ambiguous input, missing data, conflicting requirements).
- **Anti-pattern to avoid**: Do not write trigger tests that contain the skill name (e.g., "use skill-evaluation to assess this"). Real users rarely name the skill explicitly; tests that do will inflate precision and miss real routing failures.

## 3. **Evaluate routing accuracy**

- Run each positive case — did the skill trigger? (target: 100%)
- Run each negative case — did the skill stay silent? (target: 100%)
- Precision = TP / (TP + FP), Recall = TP / (TP + FN)

## 4. **Evaluate output quality**

- Run each quality case with the skill active
- Score against rubric: correct? complete? well-formatted? no hallucination?

## 5. **Run baseline comparison**

- To create a baseline: temporarily remove or rename the skill's SKILL.md from the agent client's skill directory so it cannot be loaded
- Run the same quality cases without the skill active
- Restore the skill's SKILL.md after baseline runs complete
- Blind-compare outputs where possible (judge without knowing which is skill vs baseline)
- Win rate = skill-wins / total-cases

## 6. **Synthesize and verdict**

- Routing target: precision ≥ 95% and recall ≥ 90%
- Quality target: ≥ 80% of outputs meet the rubric
- Baseline target: win rate ≥ 60%
- These are targets, not bright lines. Use judgment when results are near the boundary (e.g., 93% precision on 15 cases is one misrouted case — investigate whether it's a genuine routing failure or an ambiguous edge case).
- Verdict: **Pass** / **Fail** / **Needs Work** with the specific failing metrics

# Output contract

```
## Skill Evaluation: [skill-name]

### Routing Accuracy
| Metric    | Value | Target | Pass? |
|-----------|-------|--------|-------|
| Precision | X%    | ≥ 95%  | ✓/✗   |
| Recall    | X%    | ≥ 90%  | ✓/✗   |

Misrouted cases: [list or "None"]

### Output Quality (N cases)
Score: X/N pass (Y%)

### Baseline Comparison
Win rate: X/N (Y%)

### Verdict: [Pass | Fail | Needs Work]
Failing metrics: [list or "None"]
Next action: [specific remediation or "Ready for promotion"]
```

# Failure handling

| Situation | Action |
|-----------|--------|
| No eval cases exist | Create minimum set: 3 positive triggers, 3 negative triggers, 2 quality cases. Mark them as ad-hoc in the report. |
| Cannot determine whether skill triggered | Inspect client routing logs. If unavailable, compare output structure with and without the skill description present. |
| Baseline comparison inconclusive (win rate 45–55%) | Double the sample size. If still inconclusive, report as "neutral — skill neither helps nor hurts." |
| Routing passes but output quality fails | Stop evaluation. Route to `skill-improver` with the failing cases attached. |
| Skill passes eval but fails in real usage | Eval set has coverage gaps. Add the failing real-world case and re-run. |

## Next steps

After evaluation:
- If routing fails → `skill-trigger-optimization`
- If output quality fails → `skill-improver`
- If comparing variants → `skill-benchmarking`
- If ready for release → `skill-packaging`
- Before promotion to stable → `skill-provenance`, `skill-safety-review`
