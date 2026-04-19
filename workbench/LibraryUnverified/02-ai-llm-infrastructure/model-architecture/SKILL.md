---
name: model-architecture
description: Design and implement transformer model architectures including attention variants (MHA, GQA, MQA), positional encodings (RoPE, ALiBi), normalization strategies (Pre-LN, RMSNorm), and FFN activations (SwiGLU, GELU). Use when defining or modifying LlamaConfig, GPT-style, or Mistral-style model configs in PyTorch or HuggingFace transformers. Do not use for training loops, data pipelines, or inference serving.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: model-architecture
  maturity: draft
  risk: low
  tags: [model, architecture, transformer, attention, llama, gpt]
---

# Purpose

Guide the design and implementation of transformer-based language model architectures, covering component selection (attention, FFN, normalization, positional encoding) and scaling decisions using PyTorch and HuggingFace transformers.

# When to use this skill

Use this skill when:

- Defining a new model config: `LlamaConfig(hidden_size=4096, num_attention_heads=32, num_key_value_heads=8, intermediate_size=11008)`
- Choosing between attention variants: MHA (all heads unique), GQA (grouped key-value heads), MQA (single KV head shared)
- Selecting positional encoding: RoPE (`rotary_emb`), ALiBi (linear bias), or learned absolute embeddings
- Deciding normalization placement (Pre-LN vs Post-LN) or type (RMSNorm vs LayerNorm)
- Configuring FFN activation: SwiGLU (`LlamaMLP`), GELU (`GPT2MLP`), or ReLU
- Scaling a model (width vs depth tradeoffs, attention head sizing `d_head = hidden_size / num_heads`)

# Do not use this skill when

- The task is about training loops, optimizers, or learning rate schedules — use `pretraining-pipeline`
- The task is about MoE routing or expert design — use `moe-architecture`
- The task is about tokenizer vocabulary or encoding — use `tokenizer-design`
- The task is pure software engineering with no model structure concerns

# Operating procedure

1. **Identify target scale**: Determine parameter count target. Compute `params ≈ 12 * L * d²` for a standard transformer with L layers and hidden dim d.
2. **Select attention variant**: Use MHA for small models. Use GQA (`num_key_value_heads < num_attention_heads`) for 7B+ to reduce KV-cache memory. Use MQA only when inference latency is critical.
3. **Choose positional encoding**: Default to RoPE for most modern architectures (LLaMA, Mistral). Use ALiBi for length extrapolation without fine-tuning. Avoid learned absolute embeddings for new designs.
4. **Set normalization**: Use Pre-LN (norm before attention/FFN) with RMSNorm for training stability. Post-LN requires careful LR tuning. Example: `LlamaRMSNorm(hidden_size, eps=1e-6)`.
5. **Configure FFN block**: Use SwiGLU for LLaMA-style: `gate_proj * silu(up_proj)` with `intermediate_size ≈ 8/3 * hidden_size` rounded to multiple of 256. Use GELU for GPT-style.
6. **Validate dimensions**: Ensure `hidden_size % num_attention_heads == 0`. Typical `d_head` values: 64 (GPT-2), 128 (LLaMA). GQA requires `num_attention_heads % num_key_value_heads == 0`.
7. **Document config**: Output a complete HuggingFace config class with all architectural parameters specified explicitly.

# Decision rules

- GQA with `num_kv_heads = num_heads / 4` is the default for models ≥ 7B parameters (Mistral, LLaMA-2 70B pattern)
- RoPE `base=10000` is standard; increase base (e.g., 500000) or apply NTK-aware scaling for longer context
- SwiGLU + RMSNorm + Pre-LN + RoPE is the modern default stack (LLaMA-2/3, Mistral, Qwen2)
- Width scaling (larger `hidden_size`) is more compute-efficient than depth scaling (more layers) at fixed FLOP budget
- Attention head dim of 128 is preferred over 64 for models ≥ 7B (better per-head capacity)

# Output requirements

1. `Architecture Config` — Complete HuggingFace-compatible config with all dimensions, head counts, activation, norm type
2. `Component Justification` — Why each variant was chosen (attention type, positional encoding, activation)
3. `Parameter Count Estimate` — Breakdown: embedding, attention, FFN, norm, LM head
4. `Scaling Notes` — How the architecture changes at 2x or 10x parameter count

# References

- Vaswani et al., "Attention Is All You Need" (arxiv 1706.03762)
- Touvron et al., "LLaMA: Open and Efficient Foundation Language Models" (arxiv 2302.13971)
- Jiang et al., "Mistral 7B" (arxiv 2310.06825)
- Su et al., "RoFormer: Enhanced Transformer with Rotary Position Embedding" (arxiv 2104.09864)
- Shazeer, "GLU Variants Improve Transformer" (arxiv 2002.05202)
- HuggingFace `transformers.LlamaConfig`, `transformers.MistralConfig` source code

# Related skills

- `moe-architecture` — for Mixture-of-Experts extensions of these base architectures
- `pretraining-pipeline` — for training the defined architecture
- `tokenizer-design` — for vocabulary and embedding layer decisions
- `quantization-research` — for post-training compression of the architecture

# Failure handling

- If `hidden_size % num_attention_heads != 0`, reject the config and suggest valid dimension pairs.
- If GQA is requested but `num_attention_heads % num_key_value_heads != 0`, compute nearest valid divisor.
- If parameter count exceeds target by >20%, reduce `num_hidden_layers` first, then `hidden_size`.
- If unsure between architecture styles, default to LLaMA-3 pattern (GQA + RoPE + SwiGLU + RMSNorm).
