---
name: moe-architecture
description: Design Mixture-of-Experts transformer architectures including router/gating design (top-k softmax), expert FFN configuration, load balancing loss, capacity factors, and expert parallelism. Use when implementing MixtralConfig, Switch Transformer, or custom MoE layers. Do not use for dense transformer design or inference optimization.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: moe-architecture
  maturity: draft
  risk: low
  tags: [moe, mixture-of-experts, routing, sparse, mixtral]
---

# Purpose

Design and implement Mixture-of-Experts (MoE) transformer architectures, covering router/gating mechanisms, expert FFN configuration, load balancing strategies, and distributed expert parallelism using HuggingFace transformers and PyTorch.

# When to use this skill

Use this skill when:

- Defining MoE config: `MixtralConfig(num_local_experts=8, num_experts_per_tok=2, router_aux_loss_coef=0.01)`
- Designing router/gating: top-k selection with softmax, noisy top-k, or expert-choice routing
- Tuning load balancing: `router_aux_loss_coef` (auxiliary loss), capacity factor, token dropping
- Choosing between sparse MoE patterns: Mixtral-style (top-2 of 8), Switch (top-1), or GShard
- Planning expert parallelism for distributed training across GPUs/nodes
- Analyzing total params vs active params tradeoffs (e.g., 47B total, 13B active per token)

# Do not use this skill when

- Designing a dense (non-MoE) transformer — use `model-architecture`
- Building training loops or data pipelines — use `pretraining-pipeline`
- Optimizing MoE inference serving (kernel fusion, expert offloading) — use `serving-architecture`
- The task has no model architecture concerns

# Operating procedure

1. **Define expert structure**: Each expert is a standard FFN (MLP) block. In Mixtral: `expert = MistralMLP(hidden_size=4096, intermediate_size=14336, act_fn=silu)`. All experts share the same architecture but have independent weights.
2. **Design the router**: The gating network is a linear layer `nn.Linear(hidden_size, num_experts, bias=False)` producing logits per token. Apply softmax, select top-k experts. Standard: `top_k=2` for Mixtral, `top_k=1` for Switch Transformer.
3. **Implement routing logic**: For each token, compute `router_logits = gate(hidden_states)`. Select top-k expert indices and weights. Combine expert outputs: `output = sum(weight_i * expert_i(input))` for selected experts.
4. **Add load balancing loss**: Without auxiliary loss, routers collapse to using 1-2 experts. Add `aux_loss = num_experts * sum(fraction_tokens_i * mean_routing_prob_i)` scaled by `router_aux_loss_coef` (typically 0.01-0.02). This is added to the main language modeling loss.
5. **Set capacity factor**: `capacity = (tokens_per_batch / num_experts) * capacity_factor`. Tokens exceeding expert capacity are dropped or routed to a shared fallback. Typical capacity_factor: 1.0-1.5 for training, 2.0+ for inference.
6. **Plan expert parallelism**: Distribute experts across GPUs. With 8 experts and 8 GPUs, place 1 expert per GPU. All-to-all communication moves tokens to their assigned expert's GPU and back. This is orthogonal to tensor/pipeline parallelism.
7. **Validate training dynamics**: Monitor per-expert token allocation (should be roughly uniform). Track router entropy (higher = more balanced). Watch for expert collapse (one expert receiving >50% of tokens).

# Decision rules

- Top-2 routing (Mixtral-style) gives better quality than top-1 at ~2x compute per token — use when quality matters more than speed
- Top-1 routing (Switch) maximizes throughput — use for very large models where compute budget is tight
- Set `num_experts` as a power of 2 (8, 16, 64) for clean distribution across GPUs
- More experts with top-2 routing (e.g., 64 experts, top-2) gives more total parameters with similar active compute
- If any expert consistently receives <1% of tokens, increase `router_aux_loss_coef` or add jitter noise to router logits
- MoE models need ~4x the total parameters of a dense model to match quality at the same active-parameter compute budget

# Output requirements

1. `MoE Config` — Complete `MixtralConfig` or equivalent with expert count, top-k, aux loss coefficient, capacity factor
2. `Router Design` — Gating mechanism, top-k strategy, noise injection, and load balancing approach
3. `Compute Analysis` — Total params, active params per token, FLOPs comparison vs equivalent dense model
4. `Distribution Plan` — Expert parallelism layout across GPUs, communication pattern (all-to-all)

# References

- Jiang et al., "Mixtral of Experts" (arxiv 2401.04088)
- Fedus et al., "Switch Transformers: Scaling to Trillion Parameter Models" (arxiv 2101.03961)
- Lepikhin et al., "GShard: Scaling Giant Models with Conditional Computation" (arxiv 2006.16668)
- Shazeer et al., "Outrageously Large Neural Networks: The Sparsely-Gated Mixture-of-Experts Layer" (arxiv 1701.06538)
- HuggingFace `transformers.MixtralConfig`, `transformers.MixtralForCausalLM` source code

# Related skills

- `model-architecture` — for the dense transformer base that MoE extends
- `pretraining-pipeline` — for training MoE models with expert parallelism
- `distillation-compression` — for distilling MoE into dense student models
- `training-infrastructure` — for multi-node expert parallelism setup

# Failure handling

- If expert utilization is severely imbalanced (>3x variance across experts), increase `router_aux_loss_coef` by 2-5x and add router z-loss.
- If all-to-all communication dominates training time (>30% of step), reduce number of experts or switch to expert-choice routing.
- If MoE model quality is worse than dense baseline at same active params, verify router is not collapsing — check expert assignment entropy.
- If OOM during training, reduce `capacity_factor` to drop overflow tokens rather than buffering them.
