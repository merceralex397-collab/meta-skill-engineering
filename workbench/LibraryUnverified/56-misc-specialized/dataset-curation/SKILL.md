---
name: dataset-curation
description: Designs training dataset composition including domain mixing ratios (code, text, math, conversation), quality scoring (perplexity, classifier, reward-model), decontamination against eval sets, token budgeting, versioning, and dataset card documentation. Use when assembling or rebalancing a pretraining or fine-tuning corpus. Do not use for raw text cleaning or annotation workflows.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: dataset-curation
  maturity: draft
  risk: low
  tags: [dataset, curation, mixing, decontamination, versioning]
---

# Purpose

Assembles, balances, decontaminates, versions, and documents training datasets for LLM pretraining and fine-tuning. Covers domain mixing strategy, quality scoring pipelines, eval-set decontamination, token budget estimation, and HuggingFace dataset card authoring.

# When to use this skill

Use this skill when:

- deciding domain mixing ratios for a pretraining corpus (e.g., 50% web, 20% code, 15% books, 10% math, 5% conversation)
- scoring document quality using perplexity models, classifier-based scoring, or reward-model-based filtering
- decontaminating training data against evaluation benchmarks (MMLU, HumanEval, GSM8K) via n-gram overlap
- estimating total token counts and compute budgets for a training run
- versioning datasets and writing dataset cards for reproducibility
- building data pipelines with HuggingFace `datasets`, `datatrove`, or RedPajama-style tooling

# Do not use this skill when

- the task is raw text normalization, dedup, or PII removal (use `data-cleaning-labeling`)
- the task is generating synthetic data (use `synthetic-data-generation`)
- the task is designing evaluation benchmarks (use `benchmark-design`)
- the task is preference data collection for RLHF (use `preference-optimization`)

# Operating procedure

1. **Inventory available sources.** Catalog each data source with: name, domain (web/code/books/math/conversation/scientific), estimated token count, license, and quality tier. Use `datasets.load_dataset_builder(name).info` to inspect metadata. Track in a manifest file or database table.
2. **Define domain mixing ratios.** Set proportions based on target capabilities. Common starting points (Llama-style): web text 50%, code 20%, academic/books 15%, math 10%, conversation 5%. Use the Doremi or data mixing law approach: train small proxy models with different ratios and measure downstream performance to optimize.
3. **Score document quality.** Apply tiered scoring:
   - Tier 1 (fast): heuristic filters — remove short docs (<50 tokens), high-repetition, non-target-language
   - Tier 2 (medium): perplexity scoring with a KenLM model or small GPT-2; keep bottom 80% by perplexity
   - Tier 3 (expensive): classifier trained on curated high-quality examples, or reward model scores
   - Assign each document a quality score; use score for weighted sampling during training.
4. **Decontaminate against eval sets.** For every benchmark in the eval suite: (a) extract all test-set items, (b) compute 13-gram overlap between training documents and test items, (c) remove or flag any training document with ≥1 matching 13-gram from a test item. Insert canary strings in eval sets for ongoing detection. Use `datatrove`'s decontamination module or custom suffix-array implementation.
5. **Estimate token budget.** Count tokens per source using the target tokenizer (`AutoTokenizer.from_pretrained`). Compute total tokens needed using Chinchilla scaling: ~20 tokens per parameter for compute-optimal training. Track epoch count per domain — avoid >2 epochs on any single source.
6. **Version and document.** Tag each dataset version with a unique ID (git SHA or timestamp). Write a HuggingFace dataset card including: source descriptions, processing steps applied, mixing ratios, total tokens per domain, license information, known limitations, and decontamination report. Store the manifest as `dataset_card.yaml` alongside the data.
7. **Build the pipeline.** Use `datatrove` for large-scale processing: readers → filters → dedup → decontam → writers. For smaller corpora, use HuggingFace `datasets.interleave_datasets()` with probability weights matching target ratios. Output in tokenized format (Arrow or binary) for efficient DataLoader consumption.

# Decision rules

- Never exceed 2 epochs on any single data source; diminishing returns and memorization risk increase sharply.
- Always decontaminate against every eval benchmark before training; post-hoc contamination analysis is insufficient.
- Prefer quality over quantity: a 500B-token high-quality corpus outperforms a 1T-token unfiltered corpus.
- When mixing ratios are uncertain, run ablations on a small model (1B params) for 10B tokens to compare.
- Code data should preserve file structure and imports; avoid random chunking that breaks syntax.

# Output requirements

1. `Data manifest` — table of sources with domain, token count, license, quality tier, and mixing weight
2. `Mixing config` — domain ratios and sampling probabilities, with justification
3. `Decontamination report` — overlap counts per eval benchmark, documents removed, method used
4. `Token budget` — total tokens, tokens per domain, estimated epochs, compute estimate
5. `Dataset card` — HuggingFace-format card with provenance, processing steps, and limitations

# References

- HuggingFace `datasets` library: `huggingface.co/docs/datasets`
- `datatrove` pipeline: `github.com/huggingface/datatrove`
- RedPajama-Data: `github.com/togethercomputer/RedPajama-Data`
- Dolma toolkit: `github.com/allenai/dolma`
- Chinchilla scaling laws: Hoffmann et al. "Training Compute-Optimal Large Language Models"
- Doremi data mixing: Xie et al. "DoReMi: Optimizing Data Mixtures Speeds Up Language Model Pretraining"

# Related skills

- `data-cleaning-labeling` — raw text cleaning and annotation before curation
- `synthetic-data-generation` — generating additional data for underrepresented domains
- `benchmark-design` — designing the eval sets that curation must decontaminate against
- `preference-optimization` — curating preference pairs for alignment training

# Failure handling

- If a data source has unclear licensing, quarantine it and flag for legal review; do not include in the default mix.
- If decontamination removes >10% of a domain's data, investigate whether the source was derived from eval benchmarks.
- If token counts diverge >20% from estimates after tokenization, re-audit the manifest for counting errors.
- If ablation experiments show a domain is hurting performance, reduce its weight to ≤2% before removing entirely.
