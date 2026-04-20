---
name: serving-architecture
description: Designs and deploys LLM inference infrastructure using vLLM, TGI, or TensorRT-LLM with continuous batching, PagedAttention KV cache management, and speculative decoding. Use when configuring serving frameworks, optimizing throughput/latency, setting up streaming APIs, or scaling GPU inference horizontally. Do not use for model training or quantization research.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: serving-architecture
  maturity: draft
  risk: low
  tags: [serving, inference, vllm, tgi, deployment]
---

# Purpose

Deploy LLMs for production inference with high throughput, low latency, and efficient GPU utilization. Covers serving framework selection (vLLM, TGI, TensorRT-LLM), memory management via PagedAttention, continuous batching, speculative decoding for latency reduction, and horizontal scaling strategies.

# When to use this skill

Use this skill when:

- deploying a model behind an OpenAI-compatible API using vLLM or TGI
- configuring KV cache management, PagedAttention, or prefix caching
- implementing continuous batching (in-flight batching) for throughput optimization
- setting up speculative decoding with a draft model for latency reduction
- configuring streaming token output via server-sent events (SSE)
- scaling inference across multiple GPUs or nodes with load balancing

# Do not use this skill when

- the task is quantizing a model — prefer `quantization-research`
- the task is training or fine-tuning — prefer `training-infrastructure`
- the task is building custom CUDA kernels — prefer `inference-kernel-optimization`

# Operating procedure

1. **Select the serving framework.**
   - *vLLM*: Best throughput via PagedAttention. Supports continuous batching, prefix caching, and quantized models. Start with:
     ```bash
     python -m vllm.entrypoints.openai.api_server \
       --model meta-llama/Llama-3.1-8B-Instruct \
       --tensor-parallel-size 2 \
       --max-model-len 8192 \
       --gpu-memory-utilization 0.90
     ```
   - *TGI (Text Generation Inference)*: HuggingFace's solution, Docker-native. Good for quick deployment:
     ```bash
     docker run --gpus all -p 8080:80 \
       ghcr.io/huggingface/text-generation-inference:latest \
       --model-id meta-llama/Llama-3.1-8B-Instruct \
       --max-batch-prefill-tokens 4096 \
       --max-input-tokens 2048
     ```
   - *TensorRT-LLM*: NVIDIA-optimized, best single-request latency. Requires model compilation step.
2. **Configure KV cache and memory.** PagedAttention (vLLM) manages KV cache as virtual memory pages, eliminating fragmentation. Key settings:
   - `gpu-memory-utilization`: fraction of GPU memory for KV cache (0.85–0.95)
   - `max-model-len`: maximum sequence length (determines max KV cache per request)
   - Enable prefix caching (`--enable-prefix-caching`) for workloads with shared system prompts
3. **Enable continuous batching.** Unlike static batching, continuous batching inserts new requests into the batch as existing ones complete, maximizing GPU utilization. vLLM and TGI enable this by default. Monitor `batch_size` and `queue_depth` metrics.
4. **Set up speculative decoding** for latency-sensitive applications. A small draft model generates candidate tokens verified in parallel by the target model:
   ```bash
   python -m vllm.entrypoints.openai.api_server \
     --model meta-llama/Llama-3.1-70B-Instruct \
     --speculative-model meta-llama/Llama-3.1-8B-Instruct \
     --num-speculative-tokens 5
   ```
   Expect 1.5–2× latency reduction for greedy decoding when acceptance rate is >70%.
5. **Configure streaming output.** Expose SSE endpoints for token-by-token delivery. Both vLLM and TGI support `stream=True` in the OpenAI-compatible API. Set appropriate timeouts (30–120s) and handle client disconnections gracefully.
6. **Serve quantized models.** Load GPTQ/AWQ models directly:
   ```bash
   python -m vllm.entrypoints.openai.api_server \
     --model TheBloke/Llama-2-13B-GPTQ \
     --quantization gptq --dtype float16
   ```
7. **Scale horizontally.** Use tensor parallelism across GPUs on a single node (`--tensor-parallel-size N`). For multi-node, use pipeline parallelism or deploy multiple replicas behind a load balancer (round-robin or least-connections). Monitor GPU utilization, TTFT (time to first token), and TPS (tokens per second).

# Decision rules

- Prefer vLLM for throughput-critical workloads (batch inference, high-concurrency APIs).
- Prefer TGI for quick HuggingFace model deployment with minimal configuration.
- Prefer TensorRT-LLM when single-request latency is the primary constraint and NVIDIA GPUs are available.
- Enable prefix caching when >50% of requests share a common system prompt.
- Use speculative decoding only when the draft model acceptance rate exceeds 60%; otherwise the overhead negates latency gains.
- Set `gpu-memory-utilization` to 0.90 as default; reduce to 0.85 if OOM errors occur under load spikes.
- Always monitor TTFT separately from total generation time — they have different optimization levers.

# Output requirements

1. `Serving Configuration` — framework, model, parallelism settings, memory config, and launch command
2. `Performance Baseline` — TTFT (p50/p95), TPS, max concurrent requests, and GPU utilization under load
3. `Scaling Plan` — horizontal scaling strategy, load balancer config, and auto-scaling triggers
4. `API Specification` — endpoint URLs, streaming support, rate limits, and error response format

# References

Read these only when relevant:

- vLLM documentation: https://docs.vllm.ai/
- Kwon et al., "Efficient Memory Management for Large Language Model Serving with PagedAttention"
- HuggingFace TGI: https://huggingface.co/docs/text-generation-inference
- Leviathan et al., "Fast Inference from Transformers via Speculative Decoding"
- NVIDIA TensorRT-LLM: https://github.com/NVIDIA/TensorRT-LLM

# Related skills

- `quantization-research` — preparing quantized models for serving
- `inference-kernel-optimization` — custom kernels for attention and sampling
- `training-infrastructure` — GPU cluster management (shared concerns)
- `benchmark-design` — designing latency and throughput benchmarks

# Failure handling

- If OOM errors occur during serving, reduce `gpu-memory-utilization`, decrease `max-model-len`, or enable quantized serving (GPTQ/AWQ).
- If TTFT is too high under load, check batch queue depth — continuous batching can delay new requests when the batch is full. Increase replicas or reduce `max-batch-size`.
- If speculative decoding shows no latency improvement, profile the acceptance rate — if below 60%, disable it or use a better draft model.
- If streaming connections time out, verify keep-alive settings on the reverse proxy (nginx/envoy) and increase SSE timeout to match max generation time.
