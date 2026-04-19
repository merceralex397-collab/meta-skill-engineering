---
name: inference-kernel-optimization
description: Optimizes LLM inference kernels using FlashAttention, KV cache paging, Triton/CUDA kernel development, operator fusion, quantized GEMM, speculative decoding, and TensorRT-LLM. Use when reducing latency, memory, or throughput bottlenecks in model serving.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: inference-kernel-optimization
  maturity: draft
  risk: low
  tags: [inference, kernels, FlashAttention, Triton, CUDA, vLLM, TensorRT]
---

# Purpose

Optimize low-level inference kernels for LLM serving — FlashAttention, KV cache management, operator fusion, custom CUDA/Triton kernels, quantized GEMM, and speculative decoding — to reduce latency, memory footprint, and cost per token.

# When to use this skill

Use this skill when:

- implementing or tuning FlashAttention or paged attention for a serving stack
- writing custom CUDA or Triton kernels for transformer operations
- optimizing KV cache memory (paged allocation, prefix caching, multi-query attention)
- fusing operators (QKV projection fusion, fused MLP, fused softmax)
- deploying with TensorRT-LLM, vLLM, or ONNX Runtime and tuning their kernel configs
- implementing speculative decoding or quantized inference (INT8/INT4 GEMM)

# Do not use this skill when

- the task is model training optimization (use `fine-tuning` or `llm-creation`)
- the goal is weight quantization research without kernel changes (use `quantization-research`)
- the task is model distillation or pruning (use `distillation-compression`)

# Operating procedure

1. **Profile the bottleneck.** Use `torch.profiler`, `nsys profile`, or `py-spy` to identify whether the workload is memory-bound (attention, KV cache) or compute-bound (GEMM, MLP). Measure time-to-first-token (TTFT) and inter-token latency separately.
2. **Optimize attention.** Replace standard attention with FlashAttention-2: `from flash_attn import flash_attn_func`. Key idea: tile Q/K/V to SRAM, compute softmax in a single pass without materializing the N×N attention matrix. Reduces memory from O(N²) to O(N). For serving, implement paged attention (vLLM-style): allocate KV cache in fixed-size blocks, map logical positions to physical blocks via a block table.
3. **Fuse operators.** Combine QKV linear projections into a single GEMM. Fuse LayerNorm + residual addition. Fuse GeLU/SiLU into the MLP GEMM epilogue. Use `torch.compile(mode="max-autotune")` to auto-fuse where possible.
4. **Write custom Triton kernels.** For operations without good fused implementations:
   ```python
   import triton
   import triton.language as tl
   @triton.jit
   def fused_softmax_kernel(output_ptr, input_ptr, n_cols, BLOCK_SIZE: tl.constexpr):
       row_idx = tl.program_id(0)
       col_offsets = tl.arange(0, BLOCK_SIZE)
       mask = col_offsets < n_cols
       row = tl.load(input_ptr + row_idx * n_cols + col_offsets, mask=mask, other=-float('inf'))
       row_max = tl.max(row, axis=0)
       numerator = tl.exp(row - row_max)
       denominator = tl.sum(numerator, axis=0)
       tl.store(output_ptr + row_idx * n_cols + col_offsets, numerator / denominator, mask=mask)
   ```
5. **Quantized inference.** Use INT8 weight-only or W4A16 quantization with CUTLASS/CUDA GEMM kernels. For AWQ/GPTQ models, use the fused dequant-GEMM pattern. Benchmark with `torch.cuda.Event` timers.
6. **Speculative decoding.** Use a small draft model to generate k candidate tokens, verify in parallel with the full model. Achieves 2–3× speedup on autoregressive generation when draft acceptance rate > 70%.
7. **Benchmark end-to-end.** Measure tokens/second, TTFT, peak GPU memory, and tail latency (p99). Compare against baseline (HuggingFace generate) on fixed prompts.

# Decision rules

- If attention is >40% of total time, prioritize FlashAttention-2 or paged attention.
- If GEMM is the bottleneck, try INT8 quantized kernels before writing custom CUDA.
- Use Triton for prototyping custom kernels; move to CUTLASS/CUDA only if Triton perf is insufficient.
- Prefer `torch.compile` fusion before manual kernel writing — it handles many common patterns.
- For batch serving, paged KV cache (vLLM) typically outperforms static pre-allocation by 2–4×.
- Speculative decoding helps most for long-output generation; skip for short (<50 token) outputs.

# Output requirements

1. `Profile report` — bottleneck identification with nsys/torch.profiler traces
2. `Kernel implementation` — Triton/CUDA source with correctness tests against reference
3. `Benchmark results` — tokens/sec, TTFT, memory, p99 latency before and after optimization
4. `Integration plan` — how the kernel plugs into the serving framework (vLLM, TensorRT-LLM, etc.)

# References

- FlashAttention-2: Dao, "FlashAttention-2: Faster Attention with Better Parallelism" (arXiv:2307.08691)
- PagedAttention / vLLM: Kwon et al., "Efficient Memory Management for LLM Serving" (arXiv:2309.06180)
- Triton language: https://triton-lang.org/
- TensorRT-LLM: https://github.com/NVIDIA/TensorRT-LLM
- CUTLASS: https://github.com/NVIDIA/cutlass
- Speculative decoding: Leviathan et al. (arXiv:2211.17192)

# Related skills

- `quantization-research`
- `distillation-compression`
- `benchmark-design`
- `llm-creation`

# Failure handling

- If a custom kernel produces incorrect results, add a correctness test comparing against `torch.nn.functional.scaled_dot_product_attention` on random inputs (atol=1e-2 for fp16).
- If Triton kernel is slower than PyTorch eager, check block sizes and memory access patterns — ensure coalesced global memory reads.
- If OOM during KV cache allocation, reduce max batch size or switch to paged allocation with smaller block sizes (e.g., 16 tokens per block).
