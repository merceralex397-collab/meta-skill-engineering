---
name: data-cleaning-labeling
description: Cleans raw text data (Unicode normalization, deduplication via MinHash/SimHash, perplexity filtering, language ID) and manages annotation pipelines (inter-annotator agreement, gold sets, calibration). Use when preparing training or evaluation data that needs dedup, quality filtering, PII removal, or human labeling workflows. Do not use for dataset mixing/curation strategy or synthetic data generation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: data-cleaning-labeling
  maturity: draft
  risk: low
  tags: [data, cleaning, labeling, deduplication, annotation]
---

# Purpose

Cleans and normalizes raw text corpora for ML training, implements deduplication pipelines, applies quality filters, removes PII, and designs annotation workflows with measurable inter-annotator reliability. Covers the full path from raw scraped data to labeled, training-ready datasets.

# When to use this skill

Use this skill when:

- normalizing text data: fixing Unicode (NFKC), collapsing whitespace, stripping HTML/boilerplate, fixing encoding issues
- deduplicating a corpus using MinHash (datasketch), SimHash, exact hash, or n-gram overlap methods
- building quality filters using perplexity scoring (KenLM), heuristic rules, or classifier-based filtering
- performing language identification with fasttext `lid.176.bin` or similar models
- designing annotation guidelines, measuring inter-annotator agreement (Cohen's κ, Fleiss' κ, Krippendorff's α)
- removing PII (emails, phone numbers, SSNs, names) using regex patterns or NER-based detection

# Do not use this skill when

- the task is about dataset mixing ratios or domain balancing (use `dataset-curation`)
- the task is generating synthetic training data (use `synthetic-data-generation`)
- you need to design evaluation benchmarks (use `benchmark-design`)
- the work is standard ETL with no ML training or annotation component

# Operating procedure

1. **Profile the raw data.** Sample 10K documents and compute: character encoding distribution, language distribution (via `fasttext`), average document length, duplicate rate (exact MD5), and Unicode category breakdown. Use `datasets.load_dataset(..., streaming=True)` for large corpora.
2. **Normalize text.** Apply in order: (a) Unicode NFKC normalization, (b) fix mojibake via `ftfy.fix_text()`, (c) strip HTML with `trafilatura` or `beautifulsoup4`, (d) collapse whitespace and normalize line endings, (e) remove zero-width characters and control chars except `\n\t`.
3. **Deduplicate.** Choose method by corpus size:
   - <1M docs: exact dedup via SHA-256 hash of normalized text
   - 1M–100M docs: MinHash LSH with `datasketch.MinHashLSH(threshold=0.8, num_perm=128)`
   - Substring-level: use suffix arrays or SimHash for near-duplicate paragraph detection
   - Log dedup rate; typical web crawls lose 30–60% of documents.
4. **Filter for quality.** Apply cascading filters:
   - Language ID: keep only target languages (fasttext confidence >0.5)
   - Perplexity: score with KenLM model trained on known-good data; remove top 5% highest perplexity
   - Heuristic filters: remove docs with >80% punctuation, <50 chars, >30% uppercase, excessive repetition (line repeated >3 times)
   - Classifier: optionally train a fasttext quality classifier on curated positive/negative examples
5. **Remove PII.** Apply regex for emails, phone numbers, SSNs, IP addresses. Use a NER model (spaCy `en_core_web_lg` or Presidio) for names/addresses. Replace detected PII with type-specific tokens: `[EMAIL]`, `[PHONE]`, `[NAME]`.
6. **Design annotation pipeline.** Write annotation guidelines with ≥5 worked examples per label. Compute inter-annotator agreement on a pilot batch (≥200 samples, ≥3 annotators). Target Cohen's κ ≥ 0.7 for binary tasks, Fleiss' κ ≥ 0.6 for multi-class.
7. **Implement label QA.** Embed 5–10% gold-standard items (pre-labeled by experts) in each annotation batch. Flag annotators whose gold accuracy drops below 85%. Run calibration sessions when κ drops below threshold.

# Decision rules

- Always deduplicate before quality filtering to avoid wasting compute on duplicates.
- Use MinHash with ≥128 permutations for corpora >1M documents; exact hash is insufficient for near-duplicates.
- Require Cohen's κ ≥ 0.7 before using human labels for training; retrain annotators if below 0.6.
- PII removal must run before any data leaves the secure processing environment.
- Log every filtering step with sample counts: raw → deduped → filtered → labeled, to track data loss.

# Output requirements

1. `Data profile report` — encoding stats, language distribution, length distribution, duplicate rate
2. `Cleaning pipeline config` — ordered list of normalization and filter steps with thresholds
3. `Dedup report` — method used, threshold, number of clusters, documents removed
4. `Annotation guidelines` — label definitions, examples, edge cases, and decision tree
5. `Quality metrics` — inter-annotator κ scores, gold-set accuracy per annotator, final label distribution

# References

- `datasketch` library for MinHash/LSH: `github.com/ekzhu/datasketch`
- `fasttext` language identification: `fasttext.cc/docs/en/language-identification.html`
- `ftfy` for Unicode fixes: `github.com/rspeer/python-ftfy`
- `trafilatura` for web text extraction: `github.com/adbar/trafilatura`
- Microsoft Presidio for PII detection: `github.com/microsoft/presidio`
- HuggingFace `datasets` library for data loading and processing

# Related skills

- `dataset-curation` — mixing and balancing cleaned data into training sets
- `synthetic-data-generation` — generating additional labeled data
- `benchmark-design` — designing evaluation sets from cleaned data
- `llm-creation` — end-to-end pretraining that consumes cleaned data

# Failure handling

- If dedup rate exceeds 70%, investigate the data source for crawler traps or template pages before proceeding.
- If inter-annotator κ is below 0.5, halt labeling and revise guidelines; do not train on unreliable labels.
- If PII detection recall is uncertain, run a manual audit on 500 random samples before releasing the dataset.
- If quality filtering removes >50% of data, lower thresholds incrementally and inspect borderline samples rather than accepting a severely reduced corpus.
