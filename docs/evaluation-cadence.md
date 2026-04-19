# Evaluation Cadence

How to test whether the meta-skill system is working correctly.

## Quick Check (2 minutes)

Run structural validation only:

```bash
./scripts/validate-skills.sh
```

This checks every skill package for:
- Valid YAML frontmatter (name + description)
- All six required section headings (Purpose, When to use, When NOT to use, Procedure, Output contract, Failure handling)
- Cross-reference integrity (skill references point to existing directories)
- No phantom file references (files mentioned in SKILL.md actually exist)
- Line count within the 500-line limit
- Valid JSONL format in eval files

## Standard Cycle (5–10 minutes)

Run the full evaluation cadence:

```bash
./scripts/run-full-cycle.sh
```

This runs all five steps in sequence: structural validation, trigger/behavior evals, corpus evaluation, regression suite, and produces an aggregate report in `eval-results/summary-<timestamp>.md`. After evals and regression checks, failures are automatically harvested into `corpus/regression/` for future regression testing.

## Dry Run (instant)

Preview what would be tested without executing LLM calls:

```bash
./scripts/run-full-cycle.sh --dry-run
```

This runs structural validation (no LLM needed), lists all trigger and behavior test cases without executing them, and skips corpus evaluation and regression suite (which don't support dry-run mode).

## Individual Steps

### 1. Structural Validation

```bash
./scripts/validate-skills.sh
```

Run this after editing any SKILL.md to verify you haven't broken the required structure. Fast (no LLM calls), and the first thing the full cycle runs.

### 2. Trigger & Behavior Evals

```bash
./scripts/run-evals.sh --all              # All skills with evals/
./scripts/run-evals.sh skill-improver     # Single skill
./scripts/run-evals.sh --dry-run --all    # List cases without running
./scripts/run-evals.sh --strict --all     # Differential testing (2x slower)
./scripts/run-evals.sh --runs 3 --all     # 3 runs per prompt, majority vote
```

Runs JSONL test cases from each skill's `evals/` directory:
- **Trigger-positive tests** (`trigger-positive.jsonl`): Prompts that *should* activate the skill. Measures pos_trigger_rate.
- **Trigger-negative tests** (`trigger-negative.jsonl`): Prompts that should *not* activate the skill. Measures neg_reject_rate.
- **Behavior tests** (`behavior.jsonl`): Prompts that test output format compliance — required patterns, forbidden patterns, minimum output length.
- **Usefulness evaluation** (opt-in, `--usefulness`): LLM-as-Judge scoring of behavior test outputs across four dimensions (correctness, completeness, actionability, conciseness). Requires `usefulness_criteria` field in behavior.jsonl entries.

**Routing detection modes** control how trigger tests determine whether a skill was activated:

| Mode | Flag | Method | Speed |
|------|------|--------|-------|
| Observe | `--observe` (default) | Parse JSON output for actual SKILL.md file reads | 1x |
| Strict | `--strict` | Run with and without `--no-custom-instructions`, compare | 2x |

The default **observe** mode detects whether the model actually opened the skill's SKILL.md via the view tool.

**Multi-run variance reduction:** Use `--runs N` to run each prompt N times and decide pass/fail by majority vote. Recommended: `--runs 3` for reliable results. Multiplies API calls by N.

**Environment variables:** `EVAL_MODEL` (model, default gpt-4.1), `EVAL_TIMEOUT` (seconds), `EVAL_ROUTING` (observe|strict), `EVAL_RUNS` (runs per prompt), `EVAL_REASONING_EFFORT` (low|medium|high, omit for model default).

**Usefulness evaluation:** Use `--usefulness` to enable LLM-as-Judge scoring on behavior tests that have a `usefulness_criteria` field. A second LLM call rates the output on correctness, completeness, actionability, and conciseness (1–5 scale). Configure with: `USEFULNESS_MODEL` (judge model, defaults to EVAL_MODEL — use a different model to avoid self-evaluation bias), `USEFULNESS_THRESHOLD` (minimum score, default 3), `USEFULNESS_TIMEOUT` (seconds, default 45), `USEFULNESS_RUNS` (judge runs per case for median voting, default 1).

```bash
./scripts/run-evals.sh --usefulness --all                              # With usefulness scoring
USEFULNESS_MODEL=claude-sonnet-4-20250514 ./scripts/run-evals.sh --usefulness skill-creator  # Different judge model
```

After running all tests for a skill, the script evaluates pass/fail gates and appends a verdict to the report.

### 2b. Trigger Optimization

```bash
./scripts/run-trigger-optimization.sh skill-creator         # Optimize one skill
./scripts/run-trigger-optimization.sh --dry-run skill-creator  # Preview the train/test split
```

Automated trigger optimization with proper ML evaluation methodology:
1. **Split** eval cases 60/40 into train and test sets
2. **Baseline** the current description on the train set (3 runs per prompt)
3. **Analyze** failures and **propose** an improved description via LLM
4. **Re-evaluate** improved description on the train set
5. **Validate** on the held-out test set to catch overfitting
6. **Report** before/after comparison with ACCEPT/REJECT verdict

Does NOT auto-apply changes — outputs a proposed description for human review.

### 3. Corpus Evaluation

```bash
./scripts/run-corpus-eval.sh skill-improver --all               # Layer 1 only (structural)
./scripts/run-corpus-eval.sh --layer2 skill-improver --all      # Layer 1 + Layer 2 (meta-skill + judge)
./scripts/run-corpus-eval.sh --layer2 skill-anti-patterns adversarial  # Single tier with Layer 2
```

Tests meta-skills against the target skill corpus (`corpus/weak/`, `corpus/strong/`, `corpus/adversarial/`).

**Layer 1** (always runs): Structural scoring of each corpus fixture using `check_skill_structure.py`. Records pre-scores per skill.

**Layer 2** (opt-in via `--layer2`, requires `copilot` CLI): For each corpus fixture:
1. Invokes the meta-skill on the fixture via `copilot -p` to produce an improved version
2. Runs structural scoring on the improved version (post-score, delta)
3. Uses an LLM judge to A/B compare the original vs improved version
4. Computes an aggregate win rate (improved wins / total judged)

A win rate ≥ 60% indicates the meta-skill is effective. Below 40% indicates regression.

**Environment variables:** `EVAL_MODEL` (invocation model, default gpt-4.1), `EVAL_TIMEOUT` (seconds, default 120), `LAYER2_JUDGE_MODEL` (judge model, defaults to EVAL_MODEL — use a different model to avoid self-evaluation bias).

When `run-full-cycle.sh` detects the `copilot` CLI, it automatically enables `--layer2` for corpus evaluation.

### 4. Regression Suite

```bash
./scripts/run-regression-suite.sh
```

Runs regression test cases from `corpus/regression/`. Each `.json` file represents a previously-fixed failure. Verifies that structural checks and preservation checks remain passing.

## Reading Results

All results are saved to `eval-results/`. Key files:

| File pattern | Contents |
|---|---|
| `eval-results/summary-<timestamp>.md` | Aggregate report from the full cycle |
| `eval-results/summary-latest.md` | Symlink to the most recent aggregate report |
| `eval-results/<skill>-<timestamp>.md` | Per-skill eval results with gate verdicts |
| `eval-results/<skill>-eval.md` | Symlink to the latest per-skill report |
| `eval-results/corpus-<meta-skill>-<tier>-<timestamp>.md` | Corpus eval results per tier |

## Pass/Fail Gates

Each per-skill evaluation report includes a gates section with four checks:

| Gate | Threshold | What it measures |
|------|-----------|------------------|
| Positive trigger rate (pos_trigger_rate) | ≥ 80% | Positive trigger test pass rate — does the skill activate when it should? |
| Negative rejection rate (neg_reject_rate) | ≥ 80% | Negative trigger test pass rate — does the skill stay quiet when it shouldn't activate? |
| Behavior pass rate | ≥ 80% | Behavior test pass rate — does the output match expected patterns and length? |
| Structural validity | valid = true | `check_skill_structure.py` result — does SKILL.md have frontmatter and all required sections? |
| Usefulness (opt-in) | ≥ 3/5 | LLM-as-Judge average score across scored behavior cases. Only active with `--usefulness`. |

A skill passes only if **all four gates** pass (five gates when `--usefulness` is enabled). The overall cycle passes only if every skill passes its gates and every step exits cleanly.

## After a Failure

1. Read the failure details in the report (`eval-results/summary-latest.md` or the per-skill report)
2. Fix the underlying issue (SKILL.md structure, trigger wording, behavior output, etc.)
3. Re-run the full cycle to verify the fix: `./scripts/run-full-cycle.sh`
4. Failures from trigger and structural checks are automatically harvested into `corpus/regression/` by `scripts/harvest_failures.py` for future regression prevention
