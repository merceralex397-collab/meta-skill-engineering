---
name: context-management-memory
description: >-
  Manage LLM context windows — token budgeting, message pruning, and context
  compression. Use when hitting context length limits, implementing token
  counting, designing message truncation strategies, or optimizing prompt
  templates for token efficiency. Do not use for vector store setup (prefer
  embeddings-indexing) or agent memory architecture (prefer agent-memory).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: context-management-memory
  maturity: draft
  risk: low
  tags: [context, memory, tokens, llm]
---

# Purpose

Manage LLM context windows: token counting, budget allocation, message pruning, and context compression techniques.

# When to use this skill

- hitting context length limits and need to fit more content
- implementing token counting for prompt budget management
- designing message truncation or sliding window strategies
- compressing context with summarization or extraction

# Do not use this skill when

- designing agent memory stores — prefer `agent-memory`
- setting up vector databases — prefer `embeddings-indexing`
- choosing which model to use — prefer `model-selection`

# Procedure

1. **Count tokens accurately** — use `tiktoken` for OpenAI models, model-specific tokenizers for others. Never estimate by word count.
2. **Set budget allocation** — divide context: system prompt (fixed), retrieved context (variable), conversation history (sliding), generation headroom (reserved).
3. **Implement sliding window** — keep last N messages. When over budget, remove oldest user/assistant pairs (keep system prompt).
4. **Summarize overflow** — when pruning messages, summarize removed content into a single system message. Preserve key facts.
5. **Compress retrieved context** — extract relevant sentences from documents instead of including full text. Use LLM extraction if needed.
6. **Use structured prompts** — JSON/YAML-structured prompts are more token-efficient than verbose natural language for data-heavy contexts.
7. **Monitor usage** — log `prompt_tokens` and `completion_tokens` from API responses. Alert when consistently above 80% of limit.
8. **Test boundary cases** — verify behavior at exactly max context length. Ensure graceful degradation, not crashes.

# Token budget template

```
Model: gpt-4o (128k context)
  System prompt:         ~2,000 tokens (fixed)
  Retrieved documents:  ~10,000 tokens (variable)
  Conversation history: ~10,000 tokens (sliding window)
  User message:          ~1,000 tokens (current turn)
  Generation headroom:   ~4,000 tokens (reserved for response)
  Safety margin:         ~1,000 tokens (buffer)
  ---
  Total budget:         ~28,000 tokens used of 128k
```

# Key patterns

```python
import tiktoken

def count_tokens(messages, model="gpt-4o"):
    enc = tiktoken.encoding_for_model(model)
    total = 0
    for msg in messages:
        total += 4  # message overhead
        total += len(enc.encode(msg["content"]))
    return total + 2  # reply priming

def trim_to_budget(messages, max_tokens, model="gpt-4o"):
    while count_tokens(messages, model) > max_tokens and len(messages) > 2:
        messages.pop(1)  # remove oldest non-system message
    return messages
```

# Decision rules

- Reserve at least 10% of context window for generation — starved output causes truncation.
- Count tokens with the model's actual tokenizer — GPT-4 and Claude tokenize differently.
- Prune user/assistant pairs together — orphaned messages confuse the model.
- Summarize before discarding — "Earlier, we discussed X, Y, Z" preserves continuity.
- Prefer shorter system prompts — they consume budget on every request.

# References

- https://github.com/openai/tiktoken
- https://platform.openai.com/docs/guides/text-generation

# Related skills

- `agent-memory` — agent memory architecture
- `embeddings-indexing` — retrieving relevant context from vector stores
- `model-selection` — choosing models with appropriate context windows
