---
name: llm-creation
description: Plans and executes end-to-end LLM creation from architecture design through pretraining, instruction tuning, and alignment. Covers scaling laws, compute budgets, tokenizer training, distributed training infrastructure, and evaluation checkpoints. Use when building a new language model from scratch.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: llm-creation
  maturity: draft
  risk: low
  tags: [llm, pretraining, scaling-laws, transformer, distributed-training]
---

# Purpose

Plan and execute the full pipeline for creating an LLM from scratch — architecture design, scaling law calculations, tokenizer training, distributed pretraining, instruction tuning, alignment, evaluation checkpoints, and release documentation.

# When to use this skill

Use this skill when:

- designing a new LLM architecture (choosing model size, layers, heads, context length)
- estimating compute budgets and data requirements using scaling laws
- planning the full training pipeline: tokenizer → pretraining → SFT → alignment
- setting up distributed training infrastructure (FSDP, DeepSpeed, Megatron-LM)
- defining evaluation checkpoint schedules and release criteria

# Do not use this skill when

- the task is fine-tuning an existing model (use `fine-tuning` or `instruction-tuning`)
- the task is only about inference optimization (use `inference-kernel-optimization`)
- the goal is eval dataset creation without training (use `eval-dataset-design`)

# Operating procedure

1. **Define architecture.** Choose a decoder-only transformer configuration:
   ```
   Model size guide:
   - 1B:  num_layers=24,  hidden_dim=2048,  num_heads=16,  context=2048
   - 7B:  num_layers=32,  hidden_dim=4096,  num_heads=32,  context=4096
   - 13B: num_layers=40,  hidden_dim=5120,  num_heads=40,  context=4096
   - 70B: num_layers=80,  hidden_dim=8192,  num_heads=64,  context=8192
   ```
   Use RoPE positional embeddings, SwiGLU activation, RMSNorm, GQA (grouped-query attention) for ≥13B models.
2. **Apply scaling laws.** Chinchilla-optimal: train on ~20 tokens per parameter (a 7B model needs ~140B tokens). Estimate total FLOPs: `C ≈ 6 × N × D` where N = parameters, D = tokens. Budget GPU-hours: `C / (GPU_TFLOPS × utilization × 3600)`, target 40–50% MFU.
3. **Train tokenizer.** Train a BPE tokenizer (SentencePiece or HuggingFace tokenizers) on a representative corpus sample. Vocab size: 32k–64k. Ensure coverage of code, multilingual text, and special tokens (`<|begin_of_text|>`, `<|end_of_text|>`, chat role markers).
4. **Configure distributed pretraining.** Choose framework:
   - **FSDP** (PyTorch native): `FullyShardedDataParallel` with mixed precision (bf16), activation checkpointing.
   - **DeepSpeed ZeRO Stage 3**: partitions optimizer states, gradients, and parameters.
   - **Megatron-LM**: tensor + pipeline parallelism for >70B models.
   Learning rate: peak 3e-4, cosine decay to 3e-5, warmup over first 2000 steps. Batch size: ramp from 256 to 4M tokens over warmup.
5. **Monitor training.** Log training loss, gradient norm, and learning rate every 10 steps. Evaluate perplexity on held-out validation set every 1000 steps. Run downstream benchmarks (MMLU, HellaSwag, HumanEval) at 25%, 50%, 75%, and 100% of training.
6. **Post-training pipeline.** After pretraining: instruction-tune with SFTTrainer (delegate to `instruction-tuning` skill), then align with DPO or RLHF (delegate to `preference-optimization` skill).
7. **Document and release.** Produce a model card: architecture, training data composition, compute used, benchmark results, known limitations, intended use, and license.

# Decision rules

- Follow Chinchilla scaling: if compute is fixed, allocate ~50% to model size and ~50% to data tokens.
- For context length > 8k, use RoPE with NTK-aware scaling or YaRN for length extrapolation.
- Use GQA (num_kv_heads = num_heads / 8) for models ≥ 13B to reduce KV cache memory.
- If training loss spikes, reduce learning rate by 50% and resume from last stable checkpoint.
- Checkpoint every 1000 steps minimum; keep at least last 5 checkpoints for rollback.
- Do not release without running safety evaluations (ToxiGen, BBQ bias benchmark, red-teaming).

# Output requirements

1. `Architecture spec` — model config (layers, dims, heads, context, vocab), parameter count
2. `Compute budget` — FLOPs estimate, GPU type/count, estimated wall-clock time, cost
3. `Data plan` — corpus composition, token counts, deduplication and filtering pipeline
4. `Training config` — optimizer, LR schedule, batch size ramp, parallelism strategy
5. `Evaluation schedule` — checkpoints, benchmarks, and pass/fail criteria at each stage
6. `Model card` — standard model card with architecture, data, benchmarks, limitations

# References

- Chinchilla scaling laws: Hoffmann et al., "Training Compute-Optimal LLMs" (arXiv:2203.15556)
- LLaMA architecture: Touvron et al. (arXiv:2302.13971)
- DeepSpeed ZeRO: Rajbhandari et al. (arXiv:1910.02054)
- Megatron-LM: Shoeybi et al. (arXiv:1909.08053)
- RoPE: Su et al., "RoFormer" (arXiv:2104.09864)
- Model cards: Mitchell et al., "Model Cards for Model Reporting" (FAT* 2019)

# Related skills

- `model-architecture`
- `data-cleaning-labeling`
- `instruction-tuning`
- `fine-tuning`
- `preference-optimization`

# Failure handling

- If training loss diverges, roll back to the last stable checkpoint, halve the learning rate, and resume.
- If downstream benchmarks regress at a checkpoint, investigate data quality for the recent training window — check for duplicates or corrupted shards.
- If GPU utilization drops below 30% MFU, profile for communication bottlenecks and adjust parallelism strategy (increase tensor parallelism, reduce pipeline stages).
