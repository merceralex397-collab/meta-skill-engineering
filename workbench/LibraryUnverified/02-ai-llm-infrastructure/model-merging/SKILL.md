---
name: model-merging
description: Merge multiple fine-tuned LLM checkpoints using mergekit with methods like linear interpolation, SLERP, TIES, DARE, task arithmetic, and frankenmerging. Use when combining specialized model capabilities without retraining — e.g., merging a code model with a chat model. Do not use for training, LoRA adapter composition, or inference serving.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: model-merging
  maturity: draft
  risk: low
  tags: [model, merging, mergekit, slerp, ties, dare]
---

# Purpose

Combine multiple fine-tuned model checkpoints into a single model using weight-space merging techniques via mergekit, selecting the right merge strategy and validating results with benchmark evaluation.

# When to use this skill

Use this skill when:

- Combining two or more fine-tuned models (e.g., code + chat + math specialists) into one checkpoint
- Writing a mergekit YAML config for linear, SLERP, TIES, DARE, or passthrough (frankenmerge) methods
- Performing task arithmetic: computing task vectors (`fine_tuned - base`) and adding/subtracting them
- Deciding merge strategy based on model similarity, task overlap, and parameter conflict density
- Evaluating whether a merged model retains capabilities from each source

# Do not use this skill when

- The task involves training or fine-tuning a model from scratch — use `fine-tuning` or `pretraining-pipeline`
- The task is about combining LoRA adapters at inference time (adapter stacking) — not weight merging
- The task is about model architecture design — use `model-architecture`
- Models have incompatible architectures (different hidden sizes, vocab sizes, or tokenizers)

# Operating procedure

1. **Verify compatibility**: All source models must share identical architecture (hidden_size, num_layers, vocab_size). Tokenizer must match or be explicitly handled. Check with `model.config.to_dict()`.
2. **Select merge method**:
   - **Linear** (`merge_method: linear`): Weighted average of parameters. Simple, works well for similar models. Config: `weight: 0.6` per model.
   - **SLERP** (`merge_method: slerp`): Spherical interpolation between exactly 2 models. Smoother blending, better for dissimilar fine-tunes. Set `t: 0.5` for equal blend.
   - **TIES** (`merge_method: ties`): Trim small deltas, elect sign by majority, merge. Best when models have conflicting parameter updates. Set `density: 0.5` to keep top 50% of delta magnitudes.
   - **DARE** (`merge_method: dare_ties`): Randomly drop delta elements and rescale survivors. Effective for merging many models. Set `density: 0.3` for aggressive sparsification.
   - **Task arithmetic**: Compute `task_vector = fine_tuned_weights - base_weights`, then `merged = base + α * task_vector_A + β * task_vector_B`.
   - **Frankenmerge** (`merge_method: passthrough`): Interleave layers from different models to create a deeper model. E.g., take layers 0-15 from model A and layers 8-23 from model B to make a 32-layer hybrid.
3. **Write mergekit config**: Create a YAML file specifying `merge_method`, `slices` (layer ranges), `models` with paths and weights, and `parameters` (density, t).
4. **Execute merge**: Run `mergekit-yaml config.yaml ./output_dir --cuda --trust-remote-code`.
5. **Validate output**: Run perplexity eval on a held-out set. Compare benchmark scores (MMLU, HumanEval, etc.) against source models. Check for catastrophic capability loss.

# Decision rules

- Use SLERP for 2-model merges where models are fine-tuned from the same base — it preserves geometry better than linear
- Use TIES or DARE when merging 3+ models or when source models show conflicting parameter updates
- Use linear merge as a baseline; if it works, prefer it for simplicity
- Frankenmerging changes model depth — only use when you want a larger model and accept potential layer-boundary artifacts
- Task arithmetic requires access to the shared base model; without it, fall back to TIES
- If merged model perplexity degrades >10% vs best source, try adjusting weights or switching methods

# Output requirements

1. `Merge Config` — Complete mergekit YAML with method, models, weights, slice ranges, and parameters
2. `Source Model Inventory` — List of source models with their base, fine-tune domain, and architecture hash
3. `Evaluation Comparison` — Table of benchmark scores: each source model vs merged model
4. `Method Rationale` — Why the chosen merge method suits these specific models

# References

- mergekit library: https://github.com/arcee-ai/mergekit
- Yadav et al., "TIES-Merging: Resolving Interference When Merging Models" (arxiv 2306.01708)
- Yu et al., "Language Models are Super Mario: Absorbing Abilities from Homologous Models as a Free Lunch" — DARE (arxiv 2311.03099)
- Ilharco et al., "Editing Models with Task Arithmetic" (arxiv 2212.04089)
- Goddard et al., "Arcee's MergeKit: A Toolkit for Merging LLMs" (arxiv 2403.13257)

# Related skills

- `model-architecture` — to verify source models share compatible architecture
- `fine-tuning` — the upstream process that produces models to be merged
- `safety-alignment` — merged models may lose alignment; re-evaluate safety post-merge
- `serving-architecture` — for deploying the merged checkpoint

# Failure handling

- If architecture mismatch is detected (different `hidden_size`, `num_layers`, or `vocab_size`), abort and report which fields differ.
- If SLERP is requested with more than 2 models, switch to TIES or DARE with a warning.
- If merged model perplexity exceeds 2x the best source model, flag as failed merge and recommend retraining or different method.
- If mergekit OOMs, retry with `--lazy-unpickle` and `--low-cpu-memory` flags, or merge on CPU with `--no-cuda`.
