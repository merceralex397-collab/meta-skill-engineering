---
name: quantization-strategy
description: Select and apply model quantization formats (GGUF, GPTQ, AWQ, bitsandbytes) with appropriate bit widths, calibration data, and quality-latency tradeoffs. Use when choosing quantization format for deployment, converting models between formats, tuning bitsandbytes config, or evaluating quantized model quality. Do not use for training, fine-tuning, or inference serving configuration unrelated to quantization.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: quantization-strategy
  maturity: draft
  risk: low
  tags: [quantization, gguf, gptq, awq, bitsandbytes]
---

# Purpose

Use this skill to select the right quantization format and bit width for a given model, hardware target, and quality requirement — then execute the conversion, validate output quality, and document the tradeoff decision.

# When to use this skill

Use this skill when:

- choosing between GGUF, GPTQ, AWQ, or bitsandbytes for a deployment target
- converting a model from FP16/BF16 to a quantized format using `llama.cpp/quantize`, `auto-gptq`, `autoawq`, or `transformers` with bitsandbytes
- selecting a bit width (Q2_K through Q8_0 for GGUF, 4-bit/3-bit for GPTQ/AWQ, nf4/fp4 for bitsandbytes)
- preparing or selecting a calibration dataset for GPTQ or AWQ quantization
- benchmarking quantized model quality against the FP16 baseline (perplexity, task accuracy, output diff)
- estimating VRAM/RAM requirements for a quantized model at a given context length
- debugging quality regressions after quantization (incoherent output, degraded accuracy on specific tasks)

# Do not use this skill when

- the task is model training, fine-tuning, or LoRA adapter creation
- the task is inference serving configuration (vLLM, TGI, Ollama settings) unrelated to quantization choices
- the model is already quantized and the task is prompt engineering or application integration
- a narrower active skill already owns the problem

# Operating procedure

1. Profile the deployment constraints.
   Record target hardware (GPU model and VRAM, CPU cores, RAM), maximum acceptable latency (tokens/sec), maximum memory budget, and whether the model must run fully on GPU, CPU, or split.

2. Estimate model memory at each bit width.
   Use the formula: `memory_gb ≈ (params_billions × bits_per_weight) / 8 + kv_cache_overhead`. For a 7B model at Q4: ~4 GB weights + KV cache. Compare against available VRAM.

3. Select the quantization format based on serving runtime.
   - **GGUF** → llama.cpp, Ollama, LM Studio (CPU or GPU, flexible layer splitting)
   - **GPTQ** → vLLM, TGI, transformers with `auto-gptq` (GPU-only, fast inference with Marlin/ExLlama kernels)
   - **AWQ** → vLLM, TGI, transformers with `autoawq` (GPU-only, slightly better quality than GPTQ at same bits)
   - **bitsandbytes** → transformers in-process loading (simplest setup, nf4/fp4, no separate conversion step)

4. Prepare the calibration dataset (GPTQ/AWQ only).
   Select 128–512 representative samples from the target domain. Use `c4` or `wikitext` as fallback if domain data is unavailable. Ensure samples cover the range of expected input lengths.

5. Execute the quantization.
   - GGUF: `./quantize input.gguf output.gguf Q4_K_M`
   - GPTQ: `auto_gptq.AutoGPTQForCausalLM.from_pretrained(...).quantize(calibration_data, bits=4, group_size=128)`
   - AWQ: `awq.AutoAWQForCausalLM.from_pretrained(...).quantize(calibration_data, quant_config={"w_bit": 4, "q_group_size": 128})`
   - bitsandbytes: load with `BitsAndBytesConfig(load_in_4bit=True, bnb_4bit_quant_type="nf4", bnb_4bit_compute_dtype=torch.bfloat16)`

6. Evaluate quality against the FP16 baseline.
   Run perplexity on a held-out set. Run 3–5 representative task prompts and compare output quality. Measure accuracy on a relevant benchmark (e.g., MMLU subset, domain-specific QA). Accept if degradation is <2% on the primary metric.

7. Document the decision.
   Record: source model, quantization format, bit width, calibration data, quality delta vs FP16, memory footprint, and throughput measured on target hardware.

# Decision rules

- Default to `Q4_K_M` (GGUF) or 4-bit with group_size=128 (GPTQ/AWQ) as the starting point.
- Use `Q5_K_M` or `Q5_K_S` when Q4 shows >2% quality degradation on the primary evaluation metric.
- Prefer AWQ over GPTQ when both are supported by the serving runtime — AWQ tends to preserve quality slightly better.
- Use bitsandbytes only for development/prototyping or when the serving runtime is `transformers` directly.
- Never quantize below Q3 for models under 13B parameters — quality degradation is typically unacceptable.
- Always re-evaluate quality when switching calibration datasets — domain mismatch causes silent quality loss.

# Output requirements

1. `Hardware Profile` — GPU/CPU specs, VRAM, RAM, and target throughput
2. `Format Selection Rationale` — chosen format, bit width, and why alternatives were rejected
3. `Quantization Command or Config` — exact command or code to reproduce the conversion
4. `Quality Evaluation Results` — perplexity delta, task accuracy comparison, and sample output diffs
5. `Deployment Artifact` — path to the quantized model file and its measured memory footprint

# References

Read these only when relevant:

- `references/gguf-quant-types.md`
- `references/gptq-awq-comparison.md`
- `references/bitsandbytes-config.md`

# Related skills

- `local-llm`
- `ollama`
- `vllm-serving`
- `llama-cpp`

# Anti-patterns

- Quantizing without measuring quality — assuming lower bits are "good enough" without running a perplexity or task eval.
- Using a generic calibration dataset (e.g., `c4`) for a domain-specific model when domain data is available.
- Choosing GPTQ/AWQ for a CPU-only deployment — these formats require GPU kernels for efficient inference.
- Applying bitsandbytes quantization and then saving/loading the model as if it were a static quantized checkpoint — bitsandbytes quantizes at load time.

# Failure handling

- If quantized model produces incoherent output, re-quantize with a higher bit width (Q4→Q5→Q6) and re-evaluate.
- If GPTQ quantization crashes with OOM during calibration, reduce calibration sample count or max sequence length.
- If the quantized model loads but inference is slower than expected, verify the correct compute kernel is active (ExLlama v2 for GPTQ, Marlin for AWQ).
- If perplexity degrades >5% compared to FP16, try a different quantization method before accepting the quality loss.
