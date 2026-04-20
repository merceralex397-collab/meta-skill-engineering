---
name: llama-cpp
description: >-
  Compile and run llama.cpp for local inference — GGUF quantization, context
  sizing, and GPU offloading. Use when building llama.cpp from source,
  converting models to GGUF, configuring n_gpu_layers for partial offload,
  or tuning context size and batch parameters. Do not use for vLLM/TGI
  serving (prefer inference-serving) or model selection (prefer model-selection).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: llama-cpp
  maturity: draft
  risk: low
  tags: [llama-cpp, quantization, gguf, inference]
---

# Purpose

Compile, quantize, and run models with llama.cpp for efficient local inference on CPU, GPU, or hybrid setups.

# When to use this skill

- building llama.cpp from source with CUDA/Metal/OpenBLAS support
- converting HuggingFace models to GGUF format
- configuring GPU layer offloading (`n_gpu_layers`) for hybrid CPU/GPU
- tuning context size, batch size, and thread count for performance

# Do not use this skill when

- deploying production GPU inference — prefer `inference-serving` (vLLM)
- choosing which model to use — prefer `model-selection`
- building agent memory — prefer `agent-memory`

# Procedure

1. **Clone and build** — `git clone https://github.com/ggml-org/llama.cpp && cd llama.cpp`.
2. **Compile with backend** — `cmake -B build -DGGML_CUDA=ON` (NVIDIA), `-DGGML_METAL=ON` (Apple), `-DGGML_BLAS=ON` (CPU).
3. **Convert model** — `python convert_hf_to_gguf.py <model-dir> --outfile model.gguf --outtype f16`.
4. **Quantize** — `./llama-quantize model.gguf model-Q4_K_M.gguf Q4_K_M`. Choose quant level by quality/size tradeoff.
5. **Run inference** — `./llama-cli -m model-Q4_K_M.gguf -p "prompt" -n 256 -ngl 35 --ctx-size 4096`.
6. **Start server** — `./llama-server -m model.gguf --port 8080 -ngl 35 --ctx-size 8192` for OpenAI-compatible API.
7. **Tune performance** — adjust `-t` (threads), `-b` (batch size), `-ngl` (GPU layers), `--ctx-size`.
8. **Benchmark** — `./llama-bench -m model.gguf -ngl 35` to measure tokens/sec.

# Quantization levels

| Quant | Bits | Size (7B) | Quality | Use case |
|-------|------|-----------|---------|----------|
| Q2_K | 2-3 | ~2.7 GB | Low | Extreme compression |
| Q4_K_M | 4 | ~4.1 GB | Good | Best balance |
| Q5_K_M | 5 | ~4.8 GB | Very good | Quality priority |
| Q6_K | 6 | ~5.5 GB | Near-fp16 | Max local quality |
| Q8_0 | 8 | ~7.2 GB | Excellent | If VRAM allows |

# GPU offloading

```bash
# Full GPU offload (all layers on GPU)
./llama-cli -m model.gguf -ngl 99 --ctx-size 4096

# Partial offload (first 20 layers on GPU, rest on CPU)
./llama-cli -m model.gguf -ngl 20 --ctx-size 4096

# CPU only
./llama-cli -m model.gguf -ngl 0 -t 8 --ctx-size 2048
```

# Decision rules

- Use `Q4_K_M` as the default quantization — best quality/size tradeoff.
- Set `-ngl` to as many layers as GPU VRAM allows — even partial offload helps significantly.
- Larger context sizes consume more RAM — `--ctx-size 4096` needs ~2 GB extra for 7B model.
- Use `-t` equal to physical core count (not hyperthreads) for CPU inference.
- Always benchmark after changing parameters — small changes can have large throughput effects.

# References

- https://github.com/ggml-org/llama.cpp
- https://huggingface.co/docs/hub/gguf

# Related skills

- `inference-serving` — production GPU serving with vLLM
- `offline-cpu-inference` — CPU-only optimization strategies
- `model-selection` — choosing which model to quantize
