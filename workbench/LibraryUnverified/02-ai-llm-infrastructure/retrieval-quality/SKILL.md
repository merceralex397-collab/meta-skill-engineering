---
name: retrieval-quality
description: Evaluate and improve retrieval system quality using NDCG, MRR, recall@k, precision@k, and hit-rate metrics with relevance-annotated query sets. Use when measuring retrieval pipeline accuracy, comparing retrieval configurations, building evaluation datasets, or diagnosing degraded RAG output quality. Do not use for end-to-end LLM output evaluation or generative quality assessment beyond the retrieval stage.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: retrieval-quality
  maturity: draft
  risk: low
  tags: [retrieval-quality, ndcg, mrr, recall, evaluation]
---

# Purpose

Use this skill to measure, benchmark, and improve the quality of retrieval systems — build annotated evaluation sets, compute standard IR metrics, compare retrieval configurations in controlled A/B experiments, and diagnose root causes of degraded retrieval accuracy.

# When to use this skill

Use this skill when:

- measuring retrieval accuracy with NDCG@k, MRR, recall@k, precision@k, or hit-rate
- building or expanding a relevance-annotated query-document evaluation dataset
- comparing two retrieval configurations (different embeddings, chunk sizes, rerankers, hybrid weights)
- diagnosing why a RAG pipeline produces incorrect or poorly grounded answers
- setting up automated retrieval regression tests in CI/CD
- analyzing per-query retrieval failures to identify systematic gaps in coverage or ranking

# Do not use this skill when

- the task is evaluating LLM-generated text quality (fluency, factuality, hallucination) rather than retrieval ranking
- the task is building the retrieval pipeline itself (use `rag-retrieval` instead)
- the task is measuring end-to-end RAG system quality including generation (use `llm-integration` or `eval-harness`)
- a narrower active skill already owns the problem

# Operating procedure

1. Define the evaluation scope.
   Identify the retrieval system under test, the query distribution (user queries, synthetic queries, or production logs), and the document corpus version. Pin the corpus snapshot to avoid evaluation drift.

2. Build or load the relevance annotation set.
   Create a JSONL file with `{"query": "...", "relevant_doc_ids": ["id1", "id2"], "relevance_grades": [3, 2]}`. Use graded relevance (0–3) for NDCG or binary (0/1) for precision/recall. Target at least 50 queries with 3–5 relevant documents each for meaningful results.

3. Collect relevance judgments.
   For new datasets, use pooled judging: run the query against 2–3 retrieval variants, pool the top-20 results, and annotate each. Use domain experts for annotation; avoid LLM-as-judge for ground-truth labels unless calibrated against human judgments.

4. Run the retrieval system and capture results.
   For each query, capture the ranked list of document IDs and scores at depth k (typically k=5, 10, 20). Store results as `{"query_id": "...", "results": [{"doc_id": "...", "rank": 1, "score": 0.87}, ...]}`.

5. Compute metrics.
   - **NDCG@k**: measures ranking quality with graded relevance; use k=10 as the primary metric.
   - **MRR**: mean reciprocal rank of the first relevant result; use for navigational queries.
   - **Recall@k**: fraction of relevant documents retrieved in top-k; use k=20 to measure coverage.
   - **Precision@k**: fraction of top-k results that are relevant; use k=5 for user-facing quality.
   - **Hit-rate@k**: fraction of queries with at least one relevant result in top-k.

6. Compare configurations with statistical rigor.
   Run both configurations on the same query set. Compute paired differences per query. Use a paired t-test or bootstrap confidence interval to determine if the difference is statistically significant (p < 0.05). Report mean metric, standard error, and per-query breakdown.

7. Diagnose failure patterns.
   Sort queries by metric score ascending to identify the worst-performing queries. Categorize failures: missing documents in corpus, poor embedding similarity for domain terms, keyword mismatch in hybrid search, or reranker disagreement with retrieval order.

8. Document results and recommendations.
   Record the evaluation dataset version, metric values for each configuration, statistically significant differences, identified failure patterns, and recommended next steps.

# Decision rules

- Use NDCG@10 as the primary metric for general-purpose retrieval; use MRR when users expect a single correct answer.
- Require at least 50 annotated queries before drawing conclusions — smaller sets have high variance.
- A retrieval configuration change is only an improvement if the metric gain is statistically significant at p < 0.05.
- When NDCG is high but downstream RAG quality is low, the problem is in generation or context packing, not retrieval.
- Re-evaluate after every chunking, embedding, or reranking change — never assume retrieval quality is preserved.

# Output requirements

1. `Evaluation Dataset` — JSONL file with queries, relevant doc IDs, and relevance grades
2. `Metric Report` — NDCG@k, MRR, recall@k, precision@k, hit-rate@k with confidence intervals
3. `Configuration Comparison` — side-by-side metrics with statistical significance test results
4. `Failure Analysis` — categorized list of worst-performing queries with root cause annotations
5. `Recommendations` — ranked list of changes expected to improve retrieval quality

# References

Read these only when relevant:

- `references/ir-metrics-formulas.md`
- `references/annotation-guidelines.md`
- `references/ab-testing-retrieval.md`

# Related skills

- `rag-retrieval`
- `llm-integration`
- `eval-harness`

# Anti-patterns

- Evaluating on fewer than 20 queries and reporting metrics as reliable — small sample sizes produce misleading results.
- Using LLM-generated relevance labels as ground truth without human calibration — introduces systematic bias.
- Comparing retrieval configurations on different query sets — makes results incomparable.
- Reporting only mean metrics without per-query variance — hides bimodal failure patterns where most queries work but a subset completely fails.
- Optimizing recall@k without checking precision — retrieving more documents does not help if they are irrelevant.

# Failure handling

- If annotation disagreement is high (Cohen's kappa < 0.4), revise the annotation guidelines and re-train annotators before proceeding.
- If metrics are unstable across runs, check for non-determinism in the retrieval system (random tie-breaking, approximate nearest neighbor variability) and fix or average across multiple runs.
- If a configuration change improves NDCG but degrades MRR, analyze whether the change helps multi-document queries at the expense of single-answer queries and decide based on the product's primary use case.
- If evaluation reveals a corpus coverage gap (relevant documents missing from the index), fix the ingestion pipeline before re-tuning retrieval parameters.
