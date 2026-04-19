---
name: eval-dataset-design
description: Designs evaluation datasets for LLMs including task taxonomy, difficulty calibration, contamination prevention, scoring rubrics, and benchmark integration with lm-evaluation-harness and HELM. Use when creating, auditing, or improving eval sets for model assessment.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: eval-dataset-design
  maturity: draft
  risk: low
  tags: [eval, dataset, benchmark, lm-eval-harness, HELM]
---

# Purpose

Design rigorous evaluation datasets that measure real model capabilities across task types (classification, generation, reasoning, code, math) with calibrated difficulty, contamination controls, and reproducible scoring.

# When to use this skill

Use this skill when:

- designing a new eval benchmark or task suite for an LLM
- auditing an existing eval set for contamination, difficulty calibration, or coverage gaps
- creating tasks for `lm-evaluation-harness` or HELM-style evaluation pipelines
- building scoring rubrics for open-ended generation tasks
- selecting few-shot examples or designing prompt templates for consistent evaluation

# Do not use this skill when

- the task is fine-tuning or training a model (use `fine-tuning` or `instruction-tuning`)
- the goal is inference speed optimization (use `inference-kernel-optimization`)
- you need to run existing benchmarks without modification (just use lm-eval-harness directly)

# Operating procedure

1. **Define task taxonomy.** Classify eval targets: extractive QA, multi-choice classification, free-form generation, code synthesis, mathematical reasoning, or multi-turn dialogue. Each type requires different scoring.
2. **Calibrate difficulty.** Apply item response theory (IRT) concepts: estimate item difficulty and discrimination from pilot annotations. Establish human baselines — recruit 3–5 annotators and measure inter-annotator agreement (Krippendorff's α ≥ 0.7).
3. **Prevent contamination.** Insert canary strings (unique UUIDs) in held-out splits. Enforce temporal cutoffs (training data before date X, eval data after). Deduplicate against known pretraining corpora using MinHash or exact n-gram matching.
4. **Design prompt templates.** Use fixed prompt templates per task type. For few-shot, select examples via diversity sampling (maximize coverage of answer types). Keep `k` between 0–5 shots; always include a balanced label distribution.
5. **Build scoring rubrics.** For open-ended generation: define 3–5 rubric dimensions (accuracy, relevance, fluency, completeness). Use Likert scales (1–5). For code: use pass@k with unit test suites. For math: exact-match after normalization.
6. **Implement in lm-evaluation-harness.** Create a task YAML: define `dataset_path`, `doc_to_text`, `doc_to_target`, and `metric_list`. Register under `lm_eval/tasks/`. For HELM: define scenario + adapter + metric triples.
7. **Validate the eval set.** Run against 2+ models of known quality to verify score separation. Check ceiling (human) and floor (random baseline) performance.

# Decision rules

- Minimum 200 examples per eval task for statistical power; 500+ preferred for fine-grained analysis.
- Always include a random-baseline and human-ceiling measurement.
- If inter-annotator agreement < 0.6, revise task definition or annotation guidelines before proceeding.
- Prefer exact-match and pass@k over BLEU/ROUGE for tasks with deterministic answers.
- Use bootstrap confidence intervals (n=1000) when reporting aggregate scores.

# Output requirements

1. `Task specification` — taxonomy, prompt template, and example format
2. `Dataset splits` — train (optional few-shot source), validation, test with contamination controls
3. `Scoring rubric` — metric definitions, human annotation guidelines, automated scorer code
4. `Baseline results` — random baseline, human ceiling, and 1–2 reference model scores
5. `lm-eval-harness task YAML or HELM scenario config`

# References

- EleutherAI lm-evaluation-harness: https://github.com/EleutherAI/lm-evaluation-harness
- HELM benchmark: https://crfm.stanford.edu/helm/
- Item Response Theory for NLP: Lalor et al., 2019
- Contamination detection: Dodge et al., "Documenting Large Webtext Corpora"
- Benchmark saturation: Kiela et al., "Dynabench" (NeurIPS 2021)

# Related skills

- `benchmark-design`
- `safety-alignment`
- `fine-tuning`
- `instruction-tuning`

# Failure handling

- If contamination is detected in the eval set, quarantine affected examples and re-sample from the held-out pool.
- If human agreement is too low, run a calibration round with revised guidelines before collecting final annotations.
- If score separation between known-good and known-bad models is < 5%, the eval task lacks discriminative power — redesign the task or increase difficulty.
