---
name: agent-memory
description: >-
  Design memory stores and retrieval for AI agents — short-term buffers,
  long-term vector stores, and memory policies. Use when adding conversation
  memory to an agent, implementing RAG-based recall, designing memory
  compaction or summarization, or choosing between memory backends.
  Do not use for prompt engineering without memory concerns or for
  vector DB infrastructure setup (prefer embeddings-indexing).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: agent-memory
  maturity: draft
  risk: low
  tags: [agent, memory, llm]
---

# Purpose

Design and implement memory systems for AI agents: working memory buffers, episodic recall, and long-term knowledge stores.

# When to use this skill

- adding conversation memory to a chat agent or assistant
- implementing RAG-based recall for long-running agent sessions
- designing memory compaction, summarization, or eviction policies
- choosing between in-context memory, vector store, or structured memory

# Do not use this skill when

- setting up vector DB infrastructure — prefer `embeddings-indexing`
- doing prompt engineering without memory needs — prefer prompt skills
- building inference infrastructure — prefer `inference-serving`

# Procedure

1. **Classify memory needs** — working memory (current conversation), episodic (past interactions), semantic (facts/knowledge), procedural (learned skills).
2. **Choose memory backend** — in-context window for short conversations, vector store for episodic recall, structured DB for facts.
3. **Implement working memory** — maintain a sliding window of recent messages. Truncate or summarize when approaching context limit.
4. **Add episodic recall** — embed and store conversation turns in a vector DB. Retrieve top-k relevant memories on each turn.
5. **Implement summarization** — when conversation exceeds threshold, compress older turns into a summary. Store summary as a memory entry.
6. **Set eviction policy** — time-based decay (older = lower score), relevance-based (low retrieval count = evict), or fixed capacity with LRU.
7. **Add memory metadata** — timestamp, source, confidence, access count. Use metadata filters during retrieval.
8. **Test retrieval quality** — verify the agent recalls relevant past context; measure recall@k on known memory queries.

# Memory architecture

```
Working Memory (in-context)
  system prompt + recent N messages + retrieved memories
  |
  v -- overflow -->
Episodic Store (vector DB)
  embedded conversation turns, searchable by similarity
  |
  v -- compaction -->
Summary Store (structured)
  compressed summaries of past sessions, key facts extracted
```

# Key patterns

```python
class AgentMemory:
    def __init__(self, vector_store, max_working=20):
        self.working = []          # recent messages
        self.max_working = max_working
        self.vector_store = vector_store

    def add(self, role, content):
        self.working.append({"role": role, "content": content})
        self.vector_store.upsert(content, metadata={"role": role, "ts": time.time()})
        if len(self.working) > self.max_working:
            self._compact()

    def recall(self, query, k=5):
        return self.vector_store.query(query, top_k=k)

    def build_context(self, query):
        recalled = self.recall(query)
        return recalled + self.working[-self.max_working:]

    def _compact(self):
        old = self.working[:len(self.working)//2]
        summary = llm_summarize(old)
        self.working = [{"role": "system", "content": summary}] + self.working[len(old):]
```

# Decision rules

- Keep working memory under 50% of context window — leave room for generation and tool results.
- Embed conversation turns at the turn level, not token level — retrieval is more coherent.
- Always include timestamps in memory metadata — enables recency-weighted retrieval.
- Summarize, do not delete — compressed memories are better than lost memories.
- Test with adversarial queries — "what did we discuss about X last week?" should retrieve correctly.

# References

- https://python.langchain.com/docs/modules/memory/
- https://docs.llamaindex.ai/en/stable/module_guides/storing/

# Related skills

- `context-management-memory` — managing context window budgets
- `embeddings-indexing` — vector store setup and indexing
- `model-routing` — choosing models based on memory needs
