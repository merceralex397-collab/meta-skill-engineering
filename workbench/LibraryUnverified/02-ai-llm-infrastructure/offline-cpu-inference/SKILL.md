---
name: offline-cpu-inference
description: >-
  Optimize LLM inference for CPU-only environments — quantization, threading,
  and memory mapping. Use when running models without GPU, optimizing
  llama.cpp for CPU, choosing quantization for RAM-constrained systems, or
  deploying inference on commodity hardware. Do not use for GPU inference
  (prefer inference-serving) or model selection (prefer model-selection).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: offline-cpu-inference
  maturity: draft
  risk: low
  tags: [cpu, inference, offline, quantization]
---

# Purpose

Optimize LLM inference for CPU-only environments using quantization, memory mapping, thread tuning, and efficient model selection.

# When to use this skill

- running LLM inference on machines without GPUs
- optimizing llama.cpp CPU performance (threads, batch size, memory mapping)
- choosing quantization levels for RAM-constrained systems
- deploying inference on edge devices or commodity servers

# Do not use this skill when

- GPU hardware is available — prefer `inference-serving`
- choosing which model to use — prefer `model-selection`
- building vector search — prefer `embeddings-indexing`

# Procedure

1. **Assess hardware** — check available RAM (`free -h`), CPU cores (`nproc`), and instruction set support (AVX2, AVX-512).
2. **Choose model size** — RAM budget: model file + ~2GB overhead + context KV cache. 8GB RAM = 7B Q4, 16GB = 13B Q4 or 7B Q6, 32GB = 30B Q4.
3. **Select quantization** — `Q4_K_M` for best quality/size balance. `Q3_K_S` for extreme compression. `Q5_K_M` if RAM allows.
4. **Enable memory mapping** — llama.cpp uses `mmap` by default. Ensure sufficient virtual memory. Use `--mlock` to pin in RAM for consistent performance.
5. **Tune threads** — set `-t` to physical core count (not hyperthreads). On NUMA systems, use `numactl --cpunodebind=0`.
6. **Set batch size** — larger batches improve throughput: `-b 512` for prompt processing. Reduce for interactive use: `-b 128`.
7. **Use prompt caching** — enable `--prompt-cache` to avoid re-processing repeated system prompts.
8. **Benchmark** — `./llama-bench -m model.gguf -t <threads>` to measure prompt eval and token generation speed.

# Hardware sizing guide

| RAM | Model size | Quantization | Context | Speed (est.) |
|-----|-----------|-------------|---------|-------------|
| 8 GB | 7B | Q4_K_M (4.1GB) | 2048 | ~10 tok/s |
| 16 GB | 7B | Q6_K (5.5GB) | 8192 | ~15 tok/s |
| 16 GB | 13B | Q4_K_M (7.4GB) | 4096 | ~6 tok/s |
| 32 GB | 30B | Q4_K_M (17GB) | 4096 | ~3 tok/s |
| 64 GB | 70B | Q4_K_M (38GB) | 4096 | ~2 tok/s |

# Performance tuning

```bash
# Optimal CPU inference (adjust -t to your physical core count)
./llama-cli -m model-Q4_K_M.gguf \
  -t 8 \               # physical cores
  -b 512 \             # batch size for prompt processing
  --ctx-size 4096 \
  --mlock \            # pin model in RAM
  -p "Your prompt here"

# Server mode with prompt caching
./llama-server -m model-Q4_K_M.gguf \
  -t 8 \
  --ctx-size 4096 \
  --port 8080 \
  --prompt-cache prompt-cache.bin
```

# Decision rules

- Physical cores, not hyperthreads — HT adds ~10% throughput but increases latency variance.
- `Q4_K_M` is the sweet spot — measurably better than Q4_0/Q4_1 with negligible size increase.
- Memory-map large models — keeps RSS low, lets OS manage paging, but performance is worse than full RAM.
- Use `--mlock` only if the entire model fits in RAM — partial mlock causes OOM kills.
- Prompt caching saves 50-90% of prompt eval time for repeated system prompts.

# References

- https://github.com/ggml-org/llama.cpp
- https://huggingface.co/docs/hub/gguf

# Related skills

- `llama-cpp` — building and running llama.cpp
- `inference-serving` — GPU-based serving when hardware is available
- `model-selection` — choosing appropriately sized models
