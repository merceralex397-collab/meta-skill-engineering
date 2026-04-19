---
name: inference-serving
description: >-
  Deploy LLM inference with vLLM, TGI, or Ollama — batching, quantization,
  and GPU memory management. Use when deploying a model server with vLLM or
  TGI, configuring continuous batching, managing GPU memory with quantization,
  or benchmarking inference throughput. Do not use for model selection
  (prefer model-selection) or CPU-only inference (prefer offline-cpu-inference).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: inference-serving
  maturity: draft
  risk: low
  tags: [inference, vllm, tgi, serving]
---

# Purpose

Deploy and optimize LLM inference servers using vLLM, TGI, or Ollama with proper batching, quantization, and resource management.

# When to use this skill

- deploying a model with vLLM or TGI for production inference
- configuring continuous batching and max concurrent requests
- fitting models into GPU memory with quantization (AWQ, GPTQ, GGUF)
- benchmarking throughput (tokens/sec) and latency (TTFT, TPS)

# Do not use this skill when

- choosing which model to use — prefer `model-selection`
- running on CPU only — prefer `offline-cpu-inference`
- building agent memory or RAG — prefer `agent-memory` or `embeddings-indexing`

# Procedure

1. **Choose serving framework** — vLLM for high-throughput GPU serving, TGI for HuggingFace models, Ollama for local dev.
2. **Select quantization** — AWQ/GPTQ for GPU (4-bit, minimal quality loss), GGUF for llama.cpp/CPU. Match quant to VRAM budget.
3. **Launch server** — `vllm serve <model> --tensor-parallel-size <gpus> --max-model-len <ctx> --quantization awq`.
4. **Configure batching** — vLLM uses continuous batching by default. Set `--max-num-seqs` to control concurrency.
5. **Set memory limits** — `--gpu-memory-utilization 0.9` for vLLM. Reserve 10% for KV cache overhead.
6. **Add OpenAI-compatible API** — vLLM and TGI both serve `/v1/chat/completions`. Point clients at `http://localhost:8000`.
7. **Benchmark** — `python -m vllm.entrypoints.openai.api_server --benchmark` or use `locust`/`hey` for load testing.
8. **Monitor** — track GPU utilization (`nvidia-smi`), request queue depth, TTFT (time to first token), and throughput.

# vLLM deployment

```bash
# Basic serving
pip install vllm
vllm serve meta-llama/Llama-3.1-8B-Instruct \
  --tensor-parallel-size 1 \
  --max-model-len 8192 \
  --gpu-memory-utilization 0.9

# With quantization (fits larger models in less VRAM)
vllm serve TheBloke/Llama-3.1-70B-AWQ \
  --quantization awq \
  --tensor-parallel-size 2 \
  --max-model-len 4096
```

# TGI deployment

```bash
docker run --gpus all -p 8080:80 \
  -v /data:/data \
  ghcr.io/huggingface/text-generation-inference:latest \
  --model-id meta-llama/Llama-3.1-8B-Instruct \
  --max-batch-total-tokens 32768 \
  --max-input-tokens 4096
```

# GPU memory estimation

```
Model params * bytes per param = VRAM needed
  7B  * 2 bytes (fp16)  = ~14 GB
  7B  * 0.5 bytes (4bit) = ~3.5 GB + KV cache
  70B * 2 bytes (fp16)  = ~140 GB (2x A100 80GB)
  70B * 0.5 bytes (4bit) = ~35 GB + KV cache
```

# Decision rules

- Use vLLM for production GPU serving — best throughput via PagedAttention.
- Use Ollama for local development and testing — simplest setup.
- AWQ quantization for GPU, GGUF for CPU — do not mix formats.
- Set `max-model-len` to actual max needed, not model max — saves KV cache memory.
- Always benchmark with realistic request patterns before deploying.

# References

- https://docs.vllm.ai/
- https://huggingface.co/docs/text-generation-inference/
- https://ollama.com/

# Related skills

- `model-selection` — choosing the right model
- `llama-cpp` — CPU/hybrid inference with llama.cpp
- `offline-cpu-inference` — CPU-only optimization
