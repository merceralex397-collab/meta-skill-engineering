---
name: dense-to-moe-experiments
description: Converts dense transformer models to Mixture-of-Experts (MoE) architectures via FFN upcycling, expert initialization (copy/split-perturb/random), router training (top-k gating, auxiliary load-balancing loss), and continued pretraining. Use when experimenting with dense-to-MoE conversion, expert routing strategies, or MoE scaling. Do not use for training dense models or serving optimization.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: dense-to-moe-experiments
  maturity: draft
  risk: medium
  tags: [moe, mixture-of-experts, upcycling, routing, sparsity]
---

# Purpose

Guides the experimental conversion of dense transformer models into Mixture-of-Experts (MoE) architectures. Covers FFN layer splitting into multiple experts, initialization strategies, router/gating mechanism design, load balancing, continued pretraining schedules, and evaluation methodology for comparing MoE vs. dense baselines at equivalent active parameter counts.

# When to use this skill

Use this skill when:

- upcycling a dense model's FFN layers into N experts per transformer block (Mixtral-style)
- choosing expert initialization: copying dense weights, split+perturb, or random initialization
- implementing or tuning router/gating networks (top-1, top-2, expert choice, hash-based routing)
- adding auxiliary load-balancing losses to prevent expert collapse (GShard, Switch Transformer patterns)
- designing continued pretraining schedules after MoE upcycling
- comparing dense baselines vs. MoE variants with matched active parameters and FLOPs

# Do not use this skill when

- training a dense model from scratch (use `pretraining-pipeline`)
- optimizing MoE inference serving (use `serving-architecture`)
- the task is model distillation or compression (use `distillation-compression`)
- the task is infrastructure setup (use `training-infrastructure`)

# Operating procedure

1. **Select the source dense model and target MoE configuration.** Define: number of experts per layer (typically 8, 16, or 64), experts activated per token (top-k, typically k=2), and which layers to convert (usually all FFN layers, sometimes alternating). Document total params vs. active params — e.g., 8 experts with top-2 routing means 4x total params but same active FLOPs per token.
2. **Initialize experts from dense FFN weights.** Choose a strategy:
   - **Copy**: duplicate the dense FFN weights to all N experts identically. Simplest; relies on router training to break symmetry. Risk: slow differentiation.
   - **Split + perturb**: copy weights then add small Gaussian noise (σ=0.01–0.02) to each expert. Faster differentiation; standard practice.
   - **Random subset**: initialize each expert with a random subset of the dense FFN neurons. Better diversity but larger initial quality drop.
   - **Cluster-based**: cluster hidden representations, assign neurons to experts by cluster. Most principled but highest complexity.
3. **Implement the router/gating mechanism.** The router is a learned linear projection `W_gate ∈ R^{d_model × num_experts}` producing logits per token. Apply top-k selection:
   ```python
   gate_logits = x @ W_gate  # (batch*seq, num_experts)
   topk_vals, topk_idx = torch.topk(gate_logits, k=2)
   weights = F.softmax(topk_vals, dim=-1)
   ```
   Consider alternatives: expert-choice routing (experts select tokens instead of tokens selecting experts), hash-based routing (deterministic, no learned parameters), or soft-MoE (continuous mixing).
4. **Add load-balancing loss.** Without balancing, routers collapse to using 1–2 experts. Add auxiliary loss:
   - **GShard-style**: `L_aux = α * N * Σ(f_i * P_i)` where f_i is fraction of tokens routed to expert i, P_i is mean gate probability for expert i. Typical α=0.01.
   - **Switch Transformer**: simplified version with differentiable load balance. Set expert capacity factor C=1.25 (25% overflow buffer).
   - **Z-loss**: penalize large logits to stabilize training: `L_z = β * mean(logsumexp(gate_logits)^2)`, β=0.001.
   - Monitor expert utilization: all experts should receive 0.8/N to 1.2/N fraction of tokens.
5. **Continue pretraining.** After upcycling, train on 50B–200B tokens (5–20% of original pretraining budget). Use lower learning rate (0.1–0.3x original peak LR) with warmup. The router needs 1B–5B tokens to stabilize routing patterns. Monitor per-expert utilization and downstream eval metrics every 1B tokens.
6. **Evaluate against dense baseline.** Compare on matched active parameters (not total params). Report: eval loss, downstream benchmarks (MMLU, HumanEval, GSM8K), expert utilization entropy, routing stability (% of tokens that change expert assignment between consecutive checkpoints), and wall-clock training/inference time.

# Decision rules

- Use top-2 routing as the default; top-1 is prone to expert collapse, top-4+ adds overhead with diminishing returns.
- Set auxiliary loss coefficient α=0.01 initially; increase to 0.1 only if expert utilization entropy is <50% of uniform.
- Always use split+perturb initialization over plain copy; the noise cost is negligible but differentiation is measurably faster.
- If any expert receives <5% of its fair share of tokens after 5B tokens of training, increase α or investigate dead experts.
- MoE models should match or exceed dense baseline quality at the same active-param count within 100B continued-pretraining tokens; if not, revisit initialization and routing.

# Output requirements

1. `Architecture spec` — num experts, top-k, layers converted, total vs. active params, FLOP comparison
2. `Initialization config` — strategy used, perturbation scale, weight mapping
3. `Router config` — gating type, auxiliary loss formulation, capacity factor, loss coefficients
4. `Training log` — loss curves, expert utilization histograms per checkpoint, routing stability metrics
5. `Comparison report` — dense vs. MoE results on matched benchmarks with confidence intervals

# References

- Mixtral of Experts: Jiang et al. "Mixtral of Experts" (Mixtral-8x7B architecture)
- Switch Transformer: Fedus et al. "Switch Transformers: Scaling to Trillion Parameter Models"
- GShard: Lepikhin et al. "GShard: Scaling Giant Models with Conditional Computation"
- ST-MoE: Zoph et al. "ST-MoE: Designing Stable and Transferable Sparse Expert Models"
- MegaBlocks: efficient MoE training library `github.com/databricks/megablocks`
- Fairseq MoE: `github.com/facebookresearch/fairseq/tree/main/examples/moe`

# Related skills

- `serving-architecture` — deploying MoE models with expert parallelism
- `training-infrastructure` — distributed training setup for MoE (expert parallelism, all-to-all communication)
- `distillation-compression` — distilling MoE back to dense for inference efficiency
- `model-architecture` — general transformer architecture decisions

# Failure handling

- If expert utilization collapses (>80% of tokens to ≤2 experts), increase auxiliary loss α by 5x and restart from last balanced checkpoint.
- If loss spikes during continued pretraining, reduce learning rate by 50% and add gradient clipping at 1.0.
- If MoE underperforms dense baseline after 100B tokens, verify initialization correctness and check that router gradients are non-zero.
- If all-to-all communication dominates training time, consider expert-choice routing or reducing expert count per layer.
