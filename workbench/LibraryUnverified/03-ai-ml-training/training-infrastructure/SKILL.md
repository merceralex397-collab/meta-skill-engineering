---
name: training-infrastructure
description: Configure distributed LLM training infrastructure—DDP, FSDP, DeepSpeed ZeRO, multi-node orchestration, checkpointing, fault tolerance, and mixed precision. Use when setting up torchrun/accelerate/deepspeed jobs, writing SLURM scripts, tuning NCCL, or debugging GPU memory and communication bottlenecks.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: training-infrastructure
  maturity: draft
  risk: low
  tags: [distributed-training, deepspeed, fsdp, gpu, infrastructure]
---

# Purpose

Set up, configure, and debug distributed training infrastructure for large language models—covering parallelism strategies (DDP, FSDP, DeepSpeed ZeRO), multi-node orchestration, checkpointing, fault tolerance, mixed precision, and GPU profiling.

# When to use this skill

- Configuring distributed training with `torchrun`, `accelerate launch`, or `deepspeed` launcher
- Choosing between DDP, FSDP (`ShardingStrategy.FULL_SHARD`), or DeepSpeed ZeRO stages 1/2/3
- Writing SLURM job scripts for multi-node GPU clusters
- Setting up checkpointing (async, distributed, `safetensors` format)
- Debugging OOM errors, NCCL timeouts, or GPU memory fragmentation
- Configuring mixed precision (BF16/FP16) with `torch.autocast` or DeepSpeed config
- Profiling training with `torch.profiler` or NVIDIA Nsight Systems

# Do not use this skill when

- The task is model architecture design (layers, attention)—use `model-architecture`
- The task is inference serving or deployment—use `serving-architecture`
- The task is hyperparameter tuning or training recipe design—use `pretraining-pipeline`

# Operating procedure

1. **Select parallelism strategy.** For models that fit in one GPU with gradients: use DDP (`torchrun --nproc_per_node=8`). For models requiring sharding: use FSDP or DeepSpeed ZeRO.
   - FSDP: `FullyShardedDataParallel(model, sharding_strategy=ShardingStrategy.FULL_SHARD, mixed_precision=MixedPrecision(param_dtype=torch.bfloat16))`
   - DeepSpeed ZeRO-2: `{"zero_optimization": {"stage": 2, "offload_optimizer": {"device": "cpu"}, "allgather_bucket_size": 5e8, "reduce_bucket_size": 5e8}}`
   - For 70B+ models, combine tensor parallelism (Megatron-style column/row splits) with ZeRO-3 or FSDP for the remaining dimensions.
2. **Configure multi-node launch.** Use NCCL backend with `torchrun --nproc_per_node=8 --nnodes=2 --node_rank=$RANK --master_addr=$MASTER --master_port=29500 train.py`. Set `NCCL_IB_DISABLE=0` for InfiniBand clusters; set `NCCL_SOCKET_IFNAME=eth0` for TCP.
3. **Set up mixed precision.** Prefer BF16 on Ampere+ GPUs (no loss scaling needed). For FP16, enable dynamic loss scaling: `torch.cuda.amp.GradScaler(init_scale=2**16)` with `torch.autocast("cuda", dtype=torch.float16)`. In DeepSpeed config: `{"bf16": {"enabled": true}}`.
4. **Configure checkpointing.** Save every N steps using `torch.distributed.checkpoint.save` for sharded models or `safetensors.torch.save_model()` for single-file. Enable async checkpointing to overlap save I/O with forward pass. Keep last K checkpoints; delete older ones.
5. **Enable fault tolerance.** Use `torchrun` elastic launch (`--max_restarts=3`). Implement heartbeat monitoring between nodes. Log to wandb with `WANDB_RESUME=allow` so interrupted runs resume automatically.
6. **Monitor GPU utilization.** Run `nvidia-smi dmon -s u -d 5` during training. Target >80% GPU compute utilization; if lower, profile for communication bottlenecks. Watch for memory fragmentation via `torch.cuda.memory_stats()["allocated_bytes.all.peak"]`.
7. **Profile bottlenecks.** Use `torch.profiler.profile(activities=[ProfilerActivity.CPU, ProfilerActivity.CUDA], schedule=torch.profiler.schedule(wait=1, warmup=1, active=3))`. Export to Chrome trace or TensorBoard. Identify whether bottleneck is compute-bound (increase batch size) or communication-bound (overlap allreduce, use gradient compression).
8. **Write SLURM job script.** Include `#SBATCH --gpus-per-node=8`, `#SBATCH --ntasks-per-node=1`, `srun torchrun ...`. Set `--time` conservatively with checkpoint-resume for long jobs.

# Decision rules

- If model fits in GPU memory with optimizer states: use DDP (simplest, fastest).
- If model fits but optimizer states don't: use ZeRO-2 or FSDP `SHARD_GRAD_OP`.
- If model parameters don't fit on one GPU: use ZeRO-3 or FSDP `FULL_SHARD`.
- If model exceeds 70B parameters: combine tensor parallelism with data parallelism.
- Always use BF16 over FP16 when hardware supports it—eliminates loss scaling complexity.
- Prefer `accelerate` for single-codebase portability across DDP/FSDP/DeepSpeed.

# Output requirements

1. `Parallelism config` — strategy chosen, DeepSpeed JSON or FSDP wrapper code, with justification
2. `Launch command` — exact `torchrun` / `accelerate launch` / `deepspeed` command with all flags
3. `Checkpoint plan` — format (sharded vs safetensors), frequency, retention policy, resume procedure
4. `Resource budget` — GPU count, memory per GPU, estimated time, SLURM resource request

# References

- DeepSpeed docs: https://www.deepspeed.ai/docs/config-json/
- PyTorch FSDP tutorial: https://pytorch.org/tutorials/intermediate/FSDP_tutorial.html
- HuggingFace Accelerate: https://huggingface.co/docs/accelerate
- NVIDIA NCCL docs: https://docs.nvidia.com/deeplearning/nccl/user-guide/
- PyTorch distributed checkpoint: https://pytorch.org/docs/stable/distributed.checkpoint.html

# Related skills

- `model-architecture` — parallelism strategy depends on model size and layer structure
- `pretraining-pipeline` — training recipe runs on top of the infrastructure configured here
- `serving-architecture` — checkpoint format affects serving load path
- `model-merging` — merging sharded checkpoints requires compatible save format

# Failure handling

- If NCCL timeout occurs, verify network connectivity between nodes, increase `NCCL_TIMEOUT` (default 1800s), and check for straggler GPUs with `nvidia-smi`.
- If OOM during forward pass with FSDP, enable `activation_checkpointing` or reduce micro-batch size before switching to CPU offload.
- If loss diverges after switching to FP16, switch to BF16 or increase `GradScaler` `init_scale`; check for overflow in gradient norms via `torch.nn.utils.clip_grad_norm_`.
- If checkpointing is too slow, enable async save, switch to `safetensors` format, or write to local NVMe then async-copy to shared storage.
