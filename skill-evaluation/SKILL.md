---
name: skill-evaluation
description: >-
  Evaluate whether a single skill routes correctly and produces better output
  than a no-skill baseline — measuring positive trigger rate, negative rejection
  rate, and output win rate. Use when someone says "is this skill working?", "validate before
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
- `evals/behavior.jsonl`

Run the eval suite using `scripts/run-evals.sh`:

```bash
./scripts/run-evals.sh <skill-name>                  # Standard eval
./scripts/run-evals.sh --usefulness <skill-name>     # With LLM-as-Judge usefulness scoring
./scripts/run-evals.sh --runs 3 <skill-name>         # Multi-run with majority voting
```

This runs trigger tests (positive trigger rate and negative rejection rate), behavior tests (output format compliance), and optionally usefulness tests (LLM-judged output quality). Results are saved to `eval-results/`.

Calculate positive trigger rate, negative rejection rate, output pass rate, and baseline win rate.
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
- **Negative cases**: Read the skill's "When NOT to use" section. Each bullet becomes at least one test case. Then add near-miss cases drawn from adjacent skills' trigger phrases — these test whether the boundary is sharp. For example, if evaluating `skill-evaluation`, add trigger phrases from `skill-benchmarking` as negative cases.
- **Quality cases**: Use realistic, complete task prompts that exercise the full procedure — not just routing. Include at least one edge case where the skill must make a judgment call (e.g., ambiguous input, missing data, conflicting requirements).
- **Anti-pattern to avoid**: Do not write trigger tests that contain the skill name (e.g., "use skill-evaluation to assess this"). Real users rarely name the skill explicitly; tests that do will inflate precision and miss real routing failures.

## 3. **Evaluate routing accuracy**

- Run each positive case — did the skill trigger? (target: 100%)
- Run each negative case — did the skill stay silent? (target: 100%)
- Positive trigger rate = TP / (TP + FN) — how often the skill fires on positive cases
- Negative rejection rate = TN / (TN + FP) — how often the skill stays silent on negative cases

## 4. **Evaluate output quality**

- Run each quality case with the skill active
- Score against rubric: correct? complete? well-formatted? no hallucination?

## 5. **Run baseline comparison** (optional, manual)

This step is manual and requires the `copilot` CLI. It measures whether the skill is actually better than no skill:

- Temporarily remove or rename the skill's SKILL.md so it cannot be loaded
- Re-run the same quality cases without the skill active
- Restore the skill's SKILL.md after baseline runs complete
- Blind-compare outputs (judge without knowing which is skill vs baseline)
- Win rate = skill-wins / total-cases

Note: `run-baseline-comparison.sh` compares two versions of a SKILL.md (before/after modification), not skill-vs-no-skill. Use it for modification quality checks, not for this baseline step.

## 6. **Synthesize and verdict**

- Routing target: positive trigger rate ≥ 95% and negative rejection rate ≥ 90%
- Minimum gate (automated): positive trigger rate ≥ 80% and negative rejection rate ≥ 80%. Skills below this threshold fail.
- Quality target: ≥ 80% of outputs meet the rubric
- Baseline target: win rate ≥ 60%
- The 95%/90% targets are aspirational quality bars. The 80%/80% gates are the automated pass/fail thresholds in `run-evals.sh`. Skills between 80–95% pass the gate but should still be improved.
- Verdict: **Pass** / **Fail** / **Needs Work** with the specific failing metrics

# Output contract

```
## Skill Evaluation: [skill-name]

### Routing Accuracy
| Metric                 | Value | Gate (≥80%) | Target | Pass? |
|------------------------|-------|-------------|--------|-------|
| Positive trigger rate  | X%    | ≥ 80%       | ≥ 95%  | ✓/✗   |
| Negative rejection rate| X%    | ≥ 80%       | ≥ 90%  | ✓/✗   |

Misrouted cases: [list or "None"]

### Output Quality (N cases)
Score: X/N pass (Y%)

### Baseline Comparison (if run separately via run-baseline-comparison.sh)
Win rate: X/N (Y%)

### Verdict: [Pass | Fail | Needs Work]
Failing metrics: [list or "None"]
Next action: [specific remediation or "Ready for promotion"]

### Handoff (for downstream skills)
- **Eval report**: eval-results/[skill-name]-eval.md
- **Primary failure**: [routing | output-quality | usefulness | none]
- **Failing cases**:
  - [prompt text] — [reason: misrouted / wrong output / low usefulness score]
  - ...
- **Recommended next skill**: [skill-trigger-optimization | skill-improver | skill-benchmarking | skill-safety-review | none]
```

The Handoff section is derived from the eval results above. When routing to downstream skills (especially `skill-improver`), include the eval report path and the specific failing prompts. The `run-evals.sh` script writes reports to `eval-results/` with a `-eval.md` symlink to the latest.

# Failure handling

| Situation | Action |
|-----------|--------|
| No eval cases exist | Create minimum set: 3 positive triggers, 3 negative triggers, 2 quality cases. Mark them as ad-hoc in the report. |
| Cannot determine whether skill triggered | Inspect client routing logs. If unavailable, compare output structure with and without the skill description present. |
| Baseline comparison inconclusive (win rate 45–55%) | Double the sample size. If still inconclusive, report as "neutral — skill neither helps nor hurts." |
| Routing passes but output quality fails | Stop evaluation. Route to `skill-improver` with the eval report path (`eval-results/<skill>-eval.md`) and the specific failing prompts. |
| Skill passes eval but fails in real usage | Eval set has coverage gaps. Add the failing real-world case and re-run. |

**Routing to downstream skills:** When handing off to another skill (trigger-optimization, improver, etc.), always include:
1. The eval report path: `eval-results/<skill-name>-eval.md`
2. The primary failure type and specific failing prompts from the evaluation
3. The recommended next skill based on the failure type

This enables `skill-improver` to use eval-driven diagnosis (reading the report) rather than relying on heuristic guesswork.

# Next steps

After evaluation:
- If routing fails → `skill-trigger-optimization`
- Diagnose anti-patterns before improving → `skill-anti-patterns`
- If output quality fails → `skill-improver`
- If comparing variants → `skill-benchmarking`
- Before promotion to stable → `skill-safety-review`
