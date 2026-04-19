---
name: embeddings-indexing
description: >-
  Build vector search with embeddings, FAISS, pgvector, or Pinecone. Use when
  creating embedding pipelines, setting up vector indexes, implementing
  similarity search, or choosing embedding models and distance metrics.
  Do not use for full-text keyword search (use Elasticsearch/Typesense) or
  agent memory design (prefer agent-memory).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: embeddings-indexing
  maturity: draft
  risk: low
  tags: [embeddings, vector, search, faiss]
---

# Purpose

Build vector search systems: generate embeddings, index them in FAISS/pgvector/Pinecone, and implement similarity retrieval.

# When to use this skill

- generating embeddings from text for semantic search or RAG
- setting up FAISS, pgvector, Pinecone, or Chroma as a vector store
- choosing embedding models (OpenAI, Cohere, sentence-transformers)
- tuning similarity search with distance metrics and index parameters

# Do not use this skill when

- doing full-text keyword search — use Elasticsearch or Typesense
- designing agent memory policies — prefer `agent-memory`
- serving LLM inference — prefer `inference-serving`

# Procedure

1. **Choose embedding model** — OpenAI `text-embedding-3-small` (1536d, cheap), Cohere `embed-v3`, or local `sentence-transformers/all-MiniLM-L6-v2` (384d, free).
2. **Preprocess text** — chunk documents into 256-512 token segments with 50-token overlap. Preserve paragraph boundaries.
3. **Generate embeddings** — batch API calls (max 2048 texts per OpenAI call). Normalize vectors to unit length for cosine similarity.
4. **Choose vector store** — FAISS for local/prototyping, pgvector for Postgres-native, Pinecone/Weaviate for managed cloud.
5. **Create index** — FAISS: `IndexFlatIP` for exact search, `IndexIVFFlat` for approximate. pgvector: `CREATE INDEX USING ivfflat ... WITH (lists = 100)`.
6. **Insert vectors** — batch upserts with metadata (source, chunk_id, timestamp). Store raw text alongside vectors for retrieval.
7. **Query** — embed query text, search top-k nearest neighbors, apply metadata filters, rerank results if needed.
8. **Evaluate** — measure recall@k on a test set. Tune chunk size, overlap, and index params based on results.

# FAISS example

```python
import faiss
import numpy as np

dimension = 1536
index = faiss.IndexFlatIP(dimension)  # inner product (cosine on normalized vecs)

# Add vectors
vectors = np.array(embeddings, dtype="float32")
faiss.normalize_L2(vectors)
index.add(vectors)

# Search
query_vec = np.array([query_embedding], dtype="float32")
faiss.normalize_L2(query_vec)
distances, indices = index.search(query_vec, k=10)
```

# pgvector setup

```sql
CREATE EXTENSION vector;

CREATE TABLE documents (
  id SERIAL PRIMARY KEY,
  content TEXT NOT NULL,
  embedding VECTOR(1536),
  metadata JSONB
);

CREATE INDEX ON documents USING ivfflat (embedding vector_cosine_ops) WITH (lists = 100);

-- Query
SELECT id, content, 1 - (embedding <=> $1::vector) AS similarity
FROM documents
ORDER BY embedding <=> $1::vector
LIMIT 10;
```

# Decision rules

- Normalize embeddings to unit length — makes cosine and inner product equivalent.
- Chunk at 256-512 tokens with overlap — too small loses context, too large dilutes relevance.
- Use `IndexIVFFlat` or HNSW for datasets > 100k vectors — exact search is too slow.
- Store raw text with vectors — you need it for the LLM prompt, not just the vector.
- Batch embedding API calls — single-text calls are 10-50x slower due to overhead.

# References

- https://github.com/facebookresearch/faiss
- https://github.com/pgvector/pgvector
- https://platform.openai.com/docs/guides/embeddings

# Related skills

- `agent-memory` — using embeddings for agent recall
- `context-management-memory` — fitting retrieved chunks into context
- `inference-serving` — hosting embedding models locally
