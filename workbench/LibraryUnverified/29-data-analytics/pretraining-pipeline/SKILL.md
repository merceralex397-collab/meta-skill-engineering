---
name: pretraining-pipeline
description: Build and configure LLM pretraining pipelines covering data loading (streaming/sharded), distributed training (FSDP, DeepSpeed ZeRO), learning rate schedules (warmup + cosine decay), gradient accumulation, checkpointing, and monitoring (loss curves, MFU, gradient norms). Use when setting up accelerate/deepspeed configs or torch.distributed training loops. Do not use for fine-tuning, inference, or model architecture design.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: pretraining-pipeline
  maturity: draft
  risk: low
  tags: [pretraining, distributed, deepspeed, fsdp, accelerate]
---

# Purpose

Build end-to-end LLM pretraining pipelines covering tokenized data loading, distributed training configuration (FSDP/DeepSpeed), learning rate scheduling, gradient accumulation, checkpointing strategy, and training monitoring using accelerate, deepspeed, torch.distributed, and wandb.

# When to use this skill

Use this skill when:

- Configuring distributed training: `accelerate launch --config_file accelerate_config.yaml train.py`
- Setting up DeepSpeed ZeRO stages: `ds_config.json` with `"zero_optimization": {"stage": 2}`
- Designing LR schedule: warmup steps + cosine decay to `min_lr = peak_lr * 0.1`
- Computing effective batch size: `effective_batch = per_device_batch * num_gpus * gradient_accumulation_steps`
- Implementing checkpointing: frequency, async saving, training resumption from checkpoint
- Monitoring training: loss curves, gradient norms, learning rate, MFU (Model FLOPs Utilization) via wandb

# Do not use this skill when

- The task is about model architecture (layer types, attention variants) — use `model-architecture`
- The task is fine-tuning or LoRA on an existing model — use `fine-tuning`
- The task is about tokenizer training or vocabulary design — use `tokenizer-design`
- The task is about inference serving or deployment — use `serving-architecture`

# Operating procedure

1. **Prepare tokenized data**: Tokenize corpus offline into packed binary shards. Use `datasets.load_dataset("path", streaming=True)` for large datasets. Pack sequences to `max_seq_length` with `<eos>` separators to avoid padding waste. Store as memory-mapped files or webdataset `.tar` shards for efficient streaming.
2. **Configure distributed strategy**:
   - **FSDP** (`torch.distributed.fsdp`): Shards model parameters, gradients, and optimizer states across GPUs. Use `FullyShardedDataParallel(model, sharding_strategy=ShardingStrategy.FULL_SHARD)` for maximum memory savings.
   - **DeepSpeed ZeRO-1**: Shards optimizer states only. Minimal communication overhead.
   - **DeepSpeed ZeRO-2**: Shards optimizer states + gradients. Good balance of memory and speed.
   - **DeepSpeed ZeRO-3**: Shards everything including parameters. Required for models that don't fit on a single GPU even without optimizer states.
   - For multi-node: combine with tensor parallelism (Megatron-style) for models >70B.
3. **Set learning rate schedule**: Standard: linear warmup for ~2000 steps to peak LR, then cosine decay. Peak LR by model size: ~3e-4 for 1B, ~1.5e-4 for 7B, ~1e-4 for 13B+. Set `min_lr = peak_lr * 0.1`. Use AdamW with `betas=(0.9, 0.95)`, `weight_decay=0.1`.
4. **Configure gradient accumulation**: Set `gradient_accumulation_steps` so `effective_batch_size = per_gpu_batch * num_gpus * grad_accum_steps` reaches target (typically 2M-4M tokens). Example: 8 GPUs × 4 per-device × 32 accum steps × 2048 seq_len = ~2M tokens.
5. **Set checkpointing strategy**: Save every 500-1000 steps. Use async checkpoint saving (`torch.distributed.checkpoint` or DeepSpeed async checkpointing) to avoid training stalls. Keep last 3-5 checkpoints plus milestone saves. Always save optimizer state for resumption.
6. **Configure monitoring**: Log to wandb: `training_loss`, `gradient_norm`, `learning_rate`, `tokens_per_second`, `MFU`. Set gradient norm clipping: `max_grad_norm=1.0`. Alert on loss spikes (>2x rolling average).
7. **Validate pipeline**: Run 100-step smoke test before full training. Verify: loss decreases, gradient norms are stable (typically 0.1-10.0), LR schedule matches config, checkpoint save/resume round-trips correctly.

# Decision rules

- Use FSDP for PyTorch-native workflows; use DeepSpeed for maximum memory efficiency or when ZeRO-3 offloading to CPU/NVMe is needed
- ZeRO-2 is the default for models that fit in aggregate GPU memory; ZeRO-3 only when model params alone exceed total GPU VRAM
- Activation checkpointing (`gradient_checkpointing=True`) trades ~30% speed for ~50% memory reduction — enable for memory-constrained setups
- Batch size ramp: start at 1/4 target batch size for first 5% of training, then ramp to full. Stabilizes early training.
- If MFU < 40% on A100s, investigate data loading bottlenecks, communication overhead, or kernel inefficiency
- For runs >1 day, log to persistent storage (wandb, tensorboard on shared FS) — never rely only on stdout

# Output requirements

1. `Training Config` — Complete accelerate/deepspeed config with all parallelism, batch size, LR, and optimizer settings
2. `Data Pipeline Spec` — Tokenization format, shard layout, sequence packing strategy, and streaming config
3. `Compute Plan` — GPU count, expected training time, tokens/second target, MFU target, total token budget
4. `Monitoring Dashboard` — wandb project setup with tracked metrics: loss, grad norm, LR, throughput, MFU

# References

- DeepSpeed ZeRO: Rajbhandari et al., "ZeRO: Memory Optimizations Toward Training Trillion Parameter Models" (arxiv 1910.02054)
- PyTorch FSDP: https://pytorch.org/docs/stable/fsdp.html
- Hoffmann et al., "Training Compute-Optimal Large Language Models" — Chinchilla scaling laws (arxiv 2203.15556)
- HuggingFace Accelerate: https://huggingface.co/docs/accelerate
- Weights & Biases: https://docs.wandb.ai

# Related skills

- `model-architecture` — defines the model structure this pipeline trains
- `tokenizer-design` — produces the tokenizer and vocab used in data preparation
- `moe-architecture` — MoE models require expert parallelism in addition to data/tensor parallelism
- `training-infrastructure` — hardware provisioning and cluster setup for pretraining

# Failure handling

- If loss spikes >3x rolling average, reduce LR by 50% and resume from last stable checkpoint. If persistent, check data corruption in recent shards.
- If gradient norms explode (>100), verify `max_grad_norm` clipping is active. Reduce LR or batch size. Check for NaN in data.
- If OOM during training, enable activation checkpointing first, then reduce per-device batch size, then move to higher ZeRO stage.
- If checkpoint resume produces different loss than pre-save, verify optimizer state and RNG state are both restored. DeepSpeed: check `load_checkpoint` returns success.
- If MFU drops suddenly mid-training, check for thermal throttling, network degradation, or a slow data loader worker.
