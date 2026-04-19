---
name: rag-retrieval
description: Design and implement RAG retrieval pipelines — choose chunking strategies, select embedding models, configure vector stores, wire hybrid search with BM25+dense, add reranking stages, and pack retrieved context into LLM prompts. Use when building or debugging document ingestion, retrieval, or context assembly for LLM applications. Do not use for LLM prompt engineering without retrieval, or for search systems that do not feed into generative models.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: rag-retrieval
  maturity: draft
  risk: low
  tags: [rag, retrieval, chunking, embeddings, vector-search, reranking]
---

# Purpose

Use this skill to build and improve retrieval-augmented generation pipelines end-to-end — from document ingestion and chunking through embedding, indexing, retrieval, reranking, and context packing into the LLM prompt window.

# When to use this skill

Use this skill when:

- designing a document ingestion pipeline (PDF, HTML, Markdown → chunks → embeddings → vector store)
- choosing or changing a chunking strategy (fixed-size, recursive, semantic, sentence-window, parent-child)
- selecting an embedding model (OpenAI `text-embedding-3-small`, Cohere `embed-v3`, open-source `bge-large`, `e5-mistral`)
- configuring a vector store (Pinecone, Weaviate, Qdrant, Chroma, pgvector, FAISS)
- implementing hybrid search combining BM25 keyword search with dense vector retrieval
- adding a reranking stage (Cohere Rerank, cross-encoder models, ColBERT)
- assembling retrieved chunks into an LLM prompt with source attribution
- debugging low retrieval quality — irrelevant results, missed documents, or context window overflow

# Do not use this skill when

- the task is LLM prompt engineering or output formatting without a retrieval component
- the task is a traditional search/ranking system that does not feed into a generative model
- the task is purely about embedding model training or fine-tuning
- a narrower active skill (e.g., `retrieval-quality` for evaluation) already owns the problem

# Operating procedure

1. Analyze the source documents.
   Inventory document types (PDF, HTML, Markdown, code), average document length, total corpus size, and update frequency. Determine whether documents have natural structure (headings, sections) or are unstructured prose.

2. Select and implement the chunking strategy.
   - **Recursive character splitting** with 512-token chunks and 50-token overlap as default.
   - **Semantic chunking** (split on topic boundaries using embedding similarity) for long-form documents.
   - **Parent-child chunking** when retrieval needs small chunks but the LLM needs surrounding context.
   - Preserve metadata (source file, page number, section heading) on every chunk.

3. Choose and configure the embedding model.
   Match embedding dimension to vector store constraints. Benchmark embedding latency and cost at corpus scale. Use `text-embedding-3-small` (1536-dim) for cost efficiency or `bge-large-en-v1.5` (1024-dim) for open-source deployments. Normalize embeddings if the vector store uses cosine similarity.

4. Set up the vector store and index.
   Create the collection with the correct dimension and distance metric (cosine for normalized embeddings, dot product otherwise). Configure HNSW parameters: `ef_construction=200`, `M=16` as starting defaults. Add metadata filtering indexes for source, date, or document type.

5. Implement hybrid search (when applicable).
   Run BM25 over a text index in parallel with dense vector search. Merge results using Reciprocal Rank Fusion (RRF) with `k=60`. Weight keyword and vector scores — start with equal weights, tune based on evaluation.

6. Add a reranking stage.
   Take the top-50 results from retrieval and rerank with a cross-encoder or Cohere Rerank to produce the final top-5. This dramatically improves precision without changing the index.

7. Pack context into the LLM prompt.
   Order chunks by relevance score. Truncate to fit within the model's context window minus the system prompt, user query, and output token budget. Include source metadata for citation. Use a clear delimiter between chunks (e.g., `---`).

8. Test the full pipeline end-to-end.
   Run 10–20 representative queries. Verify that the correct source documents appear in the top-5 retrieved chunks. Measure retrieval latency (target: <500ms for the full retrieve+rerank path).

# Decision rules

- Default to 512-token chunks with 50-token overlap unless document structure suggests otherwise.
- Always add a reranking stage when top-k retrieval exceeds 10 items — the quality gain justifies the latency.
- Use hybrid search (BM25 + dense) for corpora with technical vocabulary, proper nouns, or acronyms that pure semantic search misses.
- Prefer smaller embedding models when corpus size exceeds 1M chunks — embedding cost and index size dominate.
- Never stuff the entire context window — reserve at least 20% for the LLM's reasoning and response generation.

# Output requirements

1. `Chunking Configuration` — strategy, chunk size, overlap, metadata fields preserved
2. `Embedding Pipeline` — model name, dimension, normalization, batch size, estimated cost
3. `Vector Store Schema` — collection name, distance metric, HNSW params, metadata indexes
4. `Retrieval Pipeline Code` — query → embed → search → (optional rerank) → context assembly
5. `Evaluation Baseline` — sample queries, retrieved chunks, and relevance assessment

# References

Read these only when relevant:

- `references/chunking-strategies.md`
- `references/embedding-model-comparison.md`
- `references/hybrid-search-rrf.md`

# Related skills

- `retrieval-quality`
- `llm-integration`
- `structured-output-pipelines`

# Anti-patterns

- Chunking without overlap — causes retrieval to miss information that spans chunk boundaries.
- Using the same embedding model for queries and documents without verifying asymmetric vs symmetric behavior.
- Indexing chunks without metadata — makes it impossible to filter by source, date, or section later.
- Skipping the reranking stage and sending raw top-k results to the LLM — retrieval precision drops significantly beyond k=5.
- Embedding entire documents as single vectors instead of chunking — buries fine-grained information in averaged embeddings.

# Failure handling

- If retrieval returns irrelevant results for keyword-heavy queries, add BM25 hybrid search or increase BM25 weight in the fusion formula.
- If the vector store query times out, reduce HNSW `ef_search` parameter or add pre-filtering to narrow the candidate set.
- If the LLM hallucinates despite retrieved context, verify chunks actually contain the answer — the issue may be chunking granularity, not retrieval ranking.
- If embedding latency is too high for real-time use, switch to a smaller model or pre-compute embeddings for the static corpus and only embed queries at runtime.
