---
name: model-routing
description: >-
  Route LLM requests to different models based on complexity, cost, and
  latency requirements. Use when implementing a model router that dispatches
  to small/large models, designing fallback chains, or optimizing cost by
  routing simple queries to cheaper models. Do not use for choosing a single
  model (prefer model-selection) or inference infrastructure (prefer
  inference-serving).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: model-routing
  maturity: draft
  risk: low
  tags: [model, routing, llm, cost]
---

# Purpose

Route LLM requests to appropriate models based on task complexity, cost constraints, and latency requirements.

# When to use this skill

- building a router that dispatches to small vs. large models based on query complexity
- implementing fallback chains (try cheap model first, escalate on failure)
- optimizing LLM costs by routing simple queries to smaller/cheaper models
- adding model routing to an existing agent or API gateway

# Do not use this skill when

- choosing a single model for a project — prefer `model-selection`
- deploying inference infrastructure — prefer `inference-serving`
- managing context windows — prefer `context-management-memory`

# Procedure

1. **Define routing tiers** — cheap/fast (Haiku, GPT-4o-mini), standard (Sonnet, GPT-4o), premium (Opus, o1).
2. **Classify request complexity** — use heuristics: token count, keyword signals (code, math, creative), or a small classifier model.
3. **Implement router** — check complexity score against thresholds. Route to cheapest tier that can handle the task.
4. **Add fallback chain** — if cheap model fails (low confidence, refusal, malformed output), retry with the next tier up.
5. **Set cost budgets** — track per-request cost. Alert when daily/monthly spend approaches limits.
6. **Cache responses** — hash (model + prompt) for deterministic requests. Serve from cache before routing.
7. **Monitor quality** — log model used, latency, and output quality score per request. Detect tier-mismatch patterns.
8. **Tune thresholds** — adjust complexity thresholds weekly based on quality and cost data.

# Routing architecture

```
Request --> Classifier --> Complexity Score
                             |
                  Low (<0.3) | Med (0.3-0.7) | High (>0.7)
                     |             |               |
                  Haiku/Mini    Sonnet/4o       Opus/o1
                     |             |               |
                  Response    Response          Response
                     |
              Confidence < threshold?
                     |
                  Escalate to next tier
```

# Key patterns

```python
class ModelRouter:
    TIERS = {
        "fast":    {"model": "claude-haiku", "max_complexity": 0.3, "cost_per_1k": 0.0003},
        "standard":{"model": "claude-sonnet", "max_complexity": 0.7, "cost_per_1k": 0.003},
        "premium": {"model": "claude-opus", "max_complexity": 1.0, "cost_per_1k": 0.015},
    }

    def route(self, request):
        score = self.classify_complexity(request)
        for tier in ["fast", "standard", "premium"]:
            if score <= self.TIERS[tier]["max_complexity"]:
                return self.TIERS[tier]["model"]
        return self.TIERS["premium"]["model"]

    def classify_complexity(self, request):
        # Heuristics: length, code presence, reasoning keywords
        text = request["content"]
        score = min(len(text) / 2000, 1.0)  # length signal
        if any(kw in text for kw in ["explain", "analyze", "compare"]):
            score += 0.3
        return min(score, 1.0)
```

# Decision rules

- Default to the cheapest model that can handle the task — escalate on failure, not preemptively.
- Use output validation to detect when a cheap model fails — JSON schema check, confidence score, refusal detection.
- Cache identical requests — many applications send repeated or near-identical prompts.
- Log every routing decision with model, latency, cost, and quality — you cannot optimize without data.
- Re-evaluate thresholds monthly — model capabilities and pricing change frequently.

# References

- https://docs.anthropic.com/en/docs/about-claude/models
- https://platform.openai.com/docs/models

# Related skills

- `model-selection` — choosing models for a project
- `inference-serving` — hosting the models being routed to
- `context-management-memory` — managing context per model tier
