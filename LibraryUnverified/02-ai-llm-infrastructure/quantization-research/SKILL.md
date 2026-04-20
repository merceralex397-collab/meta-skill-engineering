---
name: quantization-research
description: Applies post-training quantization (GPTQ, AWQ, GGUF) and quantization-aware training to reduce LLM memory footprint and inference cost. Use when the task involves bit-width selection, calibration, weight quantization, or evaluating perplexity degradation from quantized models. Do not use for general model compression that is not quantization-specific.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: quantization-research
  maturity: draft
  risk: low
  tags: [quantization, research, inference, compression]
---

# Purpose

Reduce model memory footprint and inference latency via weight and activation quantization while controlling quality degradation. Covers post-training quantization (PTQ) methods like GPTQ, AWQ, and GGUF conversion, as well as quantization-aware training (QAT) where forward passes simulate low-precision arithmetic.

# When to use this skill

Use this skill when:

- quantizing a pretrained model for deployment (GPTQ, AWQ, or GGUF export)
- selecting bit-width (FP16, BF16, INT8, INT4, NF4) for a serving target
- running calibration passes to collect activation statistics for quantization ranges
- evaluating perplexity or task accuracy before and after quantization
- configuring `BitsAndBytesConfig` for 4-bit or 8-bit inference in HuggingFace

# Do not use this skill when

- the task is knowledge distillation or model pruning without quantization — prefer `distillation-compression`
- the task is writing custom CUDA kernels for inference — prefer `inference-kernel-optimization`
- the task is deploying an already-quantized model behind an API — prefer `serving-architecture`

# Operating procedure

1. **Identify the target precision.** Choose FP16/BF16 for training-compatible half-precision, INT8 for balanced size/quality, or INT4/NF4 for aggressive compression. BF16 has wider dynamic range than FP16 and avoids overflow in large models.
2. **Select the quantization method.**
   - *GPTQ*: Layer-wise PTQ using approximate Hessian inverse to minimize squared error per layer. Use `auto_gptq` with a calibration dataset of 128–256 samples. Produces GPU-optimized models.
   - *AWQ*: Activation-aware weight quantization that protects salient weight channels (those multiplied by large activations). Use `autoawq`. Better generalization than GPTQ on some benchmarks.
   - *GGUF/GGML*: CPU-friendly quantization via `llama.cpp`. Convert with `python convert_hf_to_gguf.py` then quantize with `./quantize model.gguf model-Q4_K_M.gguf Q4_K_M`.
   - *BitsAndBytes*: On-the-fly NF4/INT8 quantization at load time:
     ```python
     from transformers import BitsAndBytesConfig
     bnb_config = BitsAndBytesConfig(
         load_in_4bit=True,
         bnb_4bit_quant_type="nf4",
         bnb_4bit_compute_dtype=torch.bfloat16,
         bnb_4bit_use_double_quant=True,
     )
     model = AutoModelForCausalLM.from_pretrained(model_id, quantization_config=bnb_config)
     ```
3. **Run calibration.** Provide a representative calibration dataset (e.g., 128 samples from C4 or domain data). GPTQ uses this to compute layer-wise Hessians; AWQ uses it to identify salient channels.
4. **Evaluate quality.** Measure perplexity on a held-out set (WikiText-2 or domain corpus). Run downstream task benchmarks (MMLU, HumanEval) and compare against the FP16 baseline. Accept ≤0.5 perplexity increase for INT4.
5. **For QAT**, insert `torch.quantization.FakeQuantize` modules into the model, train for 1–5% of original steps with a reduced learning rate, then export to INT8 via `torch.quantization.convert`.
6. **Package the artifact.** Save quantized weights with metadata: original model, method, bit-width, calibration dataset, and perplexity delta.

# Decision rules

- Prefer AWQ over GPTQ when the model will be used across diverse tasks (AWQ generalizes better).
- Prefer GPTQ when targeting a single benchmark or domain and maximum compression is needed.
- Use GGUF when the deployment target is CPU or Apple Silicon (llama.cpp ecosystem).
- Use BitsAndBytes NF4 for quick experimentation; switch to GPTQ/AWQ for production.
- Do not skip calibration — uncalibrated quantization causes severe accuracy loss especially at INT4.
- Always compare quantized model against FP16 baseline on at least two evaluation axes (perplexity + one task metric).

# Output requirements

1. `Quantization Config` — method, bit-width, calibration details, and BitsAndBytes/GPTQ/AWQ parameters used
2. `Quality Report` — perplexity before/after, task accuracy delta, and failure cases identified
3. `Artifact Metadata` — model card with quantization provenance for reproducibility
4. `Deployment Notes` — compatible serving frameworks and expected memory/latency profile

# References

Read these only when relevant:

- GPTQ paper: Frantar et al., "GPTQ: Accurate Post-Training Quantization for Generative Pre-trained Transformers"
- AWQ paper: Lin et al., "AWQ: Activation-aware Weight Quantization for LLM Compression"
- `auto-gptq` library: https://github.com/AutoGPTQ/AutoGPTQ
- `autoawq` library: https://github.com/casper-hansen/AutoAWQ
- `bitsandbytes` library: https://github.com/TimDettmers/bitsandbytes
- `llama.cpp` GGUF tooling: https://github.com/ggerganov/llama.cpp

# Related skills

- `distillation-compression` — non-quantization model compression
- `inference-kernel-optimization` — custom kernels for quantized ops
- `serving-architecture` — deploying quantized models at scale
- `eval-dataset-design` — building calibration and evaluation datasets

# Failure handling

- If perplexity degrades >1.0 points at INT4, fall back to INT8 or use mixed-precision (sensitive layers at higher precision).
- If calibration data is unavailable, use a generic corpus (C4 or RedPajama sample) but flag reduced domain accuracy confidence.
- If GPTQ produces outlier layers with high reconstruction error, try increasing `group_size` (e.g., 128 → 64) or switch to AWQ.
- If the quantized model shows task-specific regressions not visible in perplexity, add task-specific eval samples to the calibration set and re-quantize.
