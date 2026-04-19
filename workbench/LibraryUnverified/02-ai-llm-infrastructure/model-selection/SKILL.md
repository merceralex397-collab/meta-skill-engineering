---
name: model-selection
description: >-
  Choose the right LLM for a task based on capability, cost, latency, and
  context window. Use when evaluating models for a new project, comparing
  model benchmarks, deciding between API and self-hosted, or sizing context
  window requirements. Do not use for inference infrastructure (prefer
  inference-serving) or model routing logic (prefer model-routing).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: model-selection
  maturity: draft
  risk: low
  tags: [model, selection, llm]
---

# Purpose

Choose the right LLM for a task based on capability benchmarks, cost, latency, context window, and deployment constraints.

# When to use this skill

- evaluating which model to use for a new AI feature
- comparing model capabilities across providers (OpenAI, Anthropic, Google, open-source)
- deciding between API-hosted and self-hosted models
- sizing context window and throughput requirements

# Do not use this skill when

- deploying inference servers — prefer `inference-serving`
- building model routing logic — prefer `model-routing`
- quantizing or compiling models — prefer `llama-cpp`

# Procedure

1. **Define requirements** — list task type (chat, code, reasoning, classification), required context window, latency target, cost budget.
2. **Shortlist candidates** — filter by context window and capability. Include at least one from each category: API-hosted, open-source.
3. **Check benchmarks** — compare on relevant benchmarks (MMLU, HumanEval, MATH, MT-Bench). Use LMSYS Chatbot Arena for subjective quality.
4. **Estimate cost** — calculate monthly cost: `(requests/day * avg_tokens * price_per_token * 30)`. Include both input and output tokens.
5. **Test on your data** — run 50-100 representative examples through each candidate. Score by task-specific metrics (accuracy, format compliance).
6. **Evaluate latency** — measure TTFT and tokens/sec. API models: test from your deployment region. Self-hosted: benchmark on your hardware.
7. **Check constraints** — data residency, content policies, rate limits, SLAs. Some use cases require self-hosted for compliance.
8. **Select and document** — choose model, document rationale, set fallback model, plan re-evaluation schedule.

# Model landscape (2024-2025)

| Category | Models | Best for |
|----------|--------|----------|
| Premium reasoning | Claude Opus, o1/o3, Gemini Pro | Complex analysis, math, coding |
| Standard | Claude Sonnet, GPT-4o, Gemini Flash | General tasks, good cost/quality |
| Fast/cheap | Claude Haiku, GPT-4o-mini | Classification, extraction, routing |
| Open-source large | Llama 3.1 70B, Mixtral 8x22B | Self-hosted, data-sensitive |
| Open-source small | Llama 3.1 8B, Phi-3, Qwen2 | Edge, CPU, fine-tuning |
| Code | Claude Sonnet, GPT-4o, DeepSeek | Code generation, review |
| Embedding | text-embedding-3, Cohere embed-v3 | Search, RAG |

# Cost estimation

```
Example: Customer support chatbot
  - 10,000 conversations/day
  - Avg 2,000 input tokens + 500 output tokens per conversation

GPT-4o-mini: (10k * 2k * $0.15/1M) + (10k * 500 * $0.60/1M) = $6/day = $180/month
GPT-4o:      (10k * 2k * $2.50/1M) + (10k * 500 * $10/1M)   = $100/day = $3,000/month
Sonnet:      (10k * 2k * $3/1M) + (10k * 500 * $15/1M)       = $135/day = $4,050/month
Self-hosted 8B: ~$500/month (1x A10G) — fixed cost, unlimited requests
```

# Decision rules

- Start with the cheapest model that meets quality requirements — upgrade only with evidence.
- Test on YOUR data, not just benchmarks — benchmark performance does not always transfer.
- Factor in total cost: API fees + engineering time + infrastructure for self-hosted.
- Plan for model deprecation — always have a fallback model tested and ready.
- Re-evaluate quarterly — new models release frequently and shift the cost/quality frontier.

# References

- https://artificialanalysis.ai/
- https://chat.lmsys.org/?leaderboard
- https://platform.openai.com/docs/models

# Related skills

- `model-routing` — routing requests across multiple models
- `inference-serving` — deploying chosen models
- `llama-cpp` — running open-source models locally
