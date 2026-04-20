---
name: benchmark-design
description: Designs reproducible ML/LLM evaluation benchmarks including metric selection (BLEU, ROUGE, pass@k), evaluation protocols (few-shot, zero-shot, chain-of-thought), dataset splits with contamination prevention, and statistical significance testing. Use when building or auditing evaluation suites, leaderboards, or model comparison frameworks. Do not use for training pipelines or data curation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: benchmark-design
  maturity: draft
  risk: low
  tags: [benchmark, evaluation, metrics, leaderboard]
---

# Purpose

Designs fair, discriminating, reproducible benchmarks for LLM and ML model evaluation. Covers evaluation protocol design, metric selection, dataset split integrity, statistical significance testing, and leaderboard construction using standard frameworks like lm-evaluation-harness.

# When to use this skill

Use this skill when:

- designing a new evaluation suite or benchmark for model comparison
- selecting metrics (accuracy, F1, BLEU, ROUGE-L, pass@k, exact match) for a specific task type
- defining evaluation protocols: few-shot prompting (k=0,1,5), chain-of-thought, or tool-use evals
- auditing an existing benchmark for data contamination or statistical validity
- building leaderboard infrastructure with reproducibility guarantees

# Do not use this skill when

- the task is about curating training data (use `dataset-curation`)
- the task is about cleaning or labeling raw data (use `data-cleaning-labeling`)
- the work is model training or architecture changes, not evaluation design
- you need to optimize inference speed rather than measure model quality

# Operating procedure

1. **Define the capability being measured.** State what the benchmark tests (e.g., reasoning, code generation, factual recall). Map to existing benchmark patterns: MMLU for knowledge, HumanEval/MBPP for code, GSM8K for math, TruthfulQA for factuality.
2. **Select evaluation protocol.** Choose few-shot count and prompt format. For reasoning tasks, decide between direct answering vs. chain-of-thought. Document the exact prompt template including system message and separator tokens.
3. **Choose metrics.** Match metrics to task type:
   - Classification/QA: accuracy, macro-F1, exact match
   - Generation: BLEU, ROUGE-L, BERTScore
   - Code: pass@k (k=1,10,100) with temperature sampling per the Chen et al. unbiased estimator
   - Open-ended: win-rate via LLM-as-judge with position debiasing
4. **Design dataset splits.** Create held-out test sets with contamination prevention: compute 13-gram overlap against Common Crawl and known training corpora. Insert canary strings (`BENCHMARK_CANARY_GUID`) to detect leakage. Ensure test set has ≥500 samples for stable estimates.
5. **Implement with lm-evaluation-harness.** Define tasks as YAML configs in `lm_eval/tasks/`. Use `EleutherAI/lm-evaluation-harness` for standardized few-shot evaluation. For custom tasks, subclass `Task` and implement `doc_to_text`, `doc_to_target`, and `process_results`.
6. **Compute statistical significance.** Report bootstrap confidence intervals (n=1000 resamples) for all metrics. Use paired bootstrap tests or McNemar's test when comparing two models. Report effect sizes, not just p-values.
7. **Document reproducibility.** Record: model revision/commit, decoding params (temperature, top_p, max_tokens), prompt hash, library versions, and hardware. Publish a benchmark card with dataset size, license, and known limitations.

# Decision rules

- Use pass@k with unbiased estimator for code benchmarks; never report pass@1 from greedy decoding as pass@k.
- Require ≥1000 bootstrap resamples for confidence intervals on benchmarks with <2000 samples.
- Flag any benchmark where 13-gram overlap with known pretraining data exceeds 5% of test samples.
- For LLM-as-judge evaluations, always run both orderings (A/B and B/A) and average to remove position bias.
- Prefer normalized metrics (accuracy, macro-F1) over raw counts for cross-dataset comparison.

# Output requirements

1. `Benchmark specification` — task definition, prompt template, metric formulas, and dataset split sizes
2. `Evaluation config` — lm-evaluation-harness YAML or equivalent runner configuration
3. `Contamination report` — n-gram overlap analysis between test set and known training corpora
4. `Results table` — metric scores with 95% confidence intervals and sample counts
5. `Reproducibility card` — model versions, decoding params, library versions, hardware

# References

- EleutherAI lm-evaluation-harness: `github.com/EleutherAI/lm-evaluation-harness`
- OpenAI Evals framework: `github.com/openai/evals`
- HELM (Holistic Evaluation of Language Models): `crfm.stanford.edu/helm`
- Chen et al. "Evaluating Large Language Models Trained on Code" — pass@k estimator
- MMLU, HumanEval, GSM8K, TruthfulQA benchmark papers

# Related skills

- `eval-dataset-design` — designing the data that benchmarks consume
- `dataset-curation` — sourcing and filtering data for training or evaluation
- `safety-alignment` — evaluating safety properties specifically
- `reward-modeling` — using human preference data for evaluation signals

# Failure handling

- If the test set is too small for stable metrics (<200 samples), flag this and recommend increasing size or using bootstrapped CIs.
- If contamination analysis reveals >5% overlap, quarantine affected samples and report cleaned vs. full results.
- If no existing benchmark covers the target capability, design a minimal viable eval with ≥500 samples before committing to a full suite.
- If inter-annotator agreement (Cohen's κ) on human-labeled gold data is <0.6, revise annotation guidelines before using as ground truth.
