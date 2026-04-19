---
name: ollama
description: Configure, serve, and manage local LLMs with Ollama — write Modelfiles, pull/push models, set GPU layers and context windows, call chat/generate/embeddings API endpoints, and troubleshoot serving issues. Use when a task involves ollama serve, ollama run, Modelfile customization, or local inference API integration. Do not use for cloud-hosted LLM APIs, vLLM/TGI serving, or GGUF quantization without Ollama.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ollama
  maturity: draft
  risk: low
  tags: [ollama, modelfile, local-llm, inference-api]
---

# Purpose

Use this skill to manage the full lifecycle of local LLM serving with Ollama — from authoring Modelfiles and pulling models through configuring GPU offload and context windows to integrating with the HTTP API for chat, completion, and embedding workloads.

# When to use this skill

Use this skill when:

- writing or editing a `Modelfile` (FROM, PARAMETER, SYSTEM, TEMPLATE directives)
- pulling, pushing, copying, or listing models via `ollama pull`, `ollama push`, `ollama cp`, `ollama list`
- configuring `ollama serve` startup flags, environment variables (`OLLAMA_HOST`, `OLLAMA_NUM_PARALLEL`, `OLLAMA_MAX_LOADED_MODELS`)
- tuning GPU layer offload (`num_gpu`), context window (`num_ctx`), or batch size (`num_batch`)
- calling the Ollama REST API (`/api/chat`, `/api/generate`, `/api/embeddings`, `/api/tags`)
- integrating Ollama with application code via the official Python or JS client libraries
- debugging model load failures, OOM crashes, or slow first-token latency

# Do not use this skill when

- the task targets cloud-hosted LLM APIs (OpenAI, Anthropic, Bedrock, Vertex AI)
- the task involves vLLM, TGI, or llama.cpp serving without Ollama wrapping
- the work is pure GGUF quantization or model conversion without Ollama deployment
- a narrower active skill already owns the specific problem

# Operating procedure

1. Identify the target model and hardware.
   Confirm the model name/tag (e.g., `llama3:8b-instruct-q4_K_M`), available VRAM, and RAM. Run `ollama list` to check currently loaded models.

2. Author or update the Modelfile.
   Set `FROM` to the base model or GGUF path. Add `PARAMETER num_gpu <layers>` for GPU offload. Set `PARAMETER num_ctx <tokens>` for context window. Define `SYSTEM` prompt and `TEMPLATE` using Go template syntax with `{{ .System }}`, `{{ .Prompt }}`, and `{{ .Response }}` placeholders.

3. Build and test the custom model.
   Run `ollama create <name> -f Modelfile`. Verify with `ollama run <name>` using a representative prompt. Check `ollama show <name> --modelfile` to confirm parameters persisted.

4. Configure the server for production use.
   Set `OLLAMA_HOST=0.0.0.0:11434` for network access. Set `OLLAMA_NUM_PARALLEL` for concurrent request slots. Set `OLLAMA_MAX_LOADED_MODELS` based on available memory. Start with `ollama serve` or the systemd unit.

5. Wire API integration.
   Use `POST /api/chat` with `{"model": "<name>", "messages": [...]}` for multi-turn chat. Use `POST /api/generate` for single-shot completions. Use `POST /api/embeddings` with `{"model": "<name>", "prompt": "<text>"}` for vector embeddings. Set `"stream": false` when you need the full response in one JSON object.

6. Validate performance and resource usage.
   Measure time-to-first-token and tokens-per-second. Monitor VRAM usage with `nvidia-smi` or `ollama ps`. If OOM occurs, reduce `num_gpu` layers or switch to a smaller quantization.

7. Document the configuration.
   Record the final Modelfile, environment variables, hardware specs, and measured throughput in the project README or deployment doc.

# Decision rules

- Default to `num_ctx 4096` unless the use case requires longer context; doubling context quadruples KV-cache memory.
- Prefer `q4_K_M` quantization for balanced quality/speed; use `q5_K_M` when quality is critical and VRAM allows.
- Always pin model tags with explicit quantization suffixes — never rely on `latest`.
- Use `stream: true` for user-facing chat; use `stream: false` for programmatic extraction pipelines.
- If the model must fit in CPU-only RAM, set `num_gpu 0` and expect 5-10x slower inference.

# Output requirements

1. `Modelfile` — complete file with FROM, PARAMETER, SYSTEM, and TEMPLATE directives
2. `Server Configuration` — environment variables and startup command
3. `API Integration Code` — client code with endpoint, model name, and key parameters
4. `Performance Baseline` — measured tokens/sec, VRAM usage, and context window tested

# References

Read these only when relevant:

- `references/modelfile-syntax.md`
- `references/ollama-api-endpoints.md`
- `references/gpu-memory-estimation.md`

# Related skills

- `llm-integration`
- `local-llm`
- `llama-cpp`
- `vllm-serving`

# Anti-patterns

- Using `ollama run` in production scripts instead of the HTTP API — breaks on concurrent requests.
- Setting `num_ctx` to the model's maximum without checking available VRAM — causes silent OOM and fallback to CPU.
- Omitting the SYSTEM directive in a Modelfile and relying on per-request system prompts — leads to inconsistent behavior across clients.
- Pulling models by bare name (`ollama pull llama3`) without specifying a quantization tag — creates non-reproducible deployments.

# Failure handling

- If `ollama serve` fails to start, check port conflicts on 11434 and verify the `OLLAMA_MODELS` directory has write permissions.
- If a model fails to load with OOM, reduce `num_gpu` by half and retry; if still failing, switch to a smaller quantization variant.
- If `/api/chat` returns empty or truncated responses, verify `num_ctx` is large enough for the prompt and check `num_predict` is not set to zero.
- If embedding requests return 404, confirm the model supports embeddings — not all chat models expose the embedding endpoint.
