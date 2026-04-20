---
name: preference-optimization
description: Implement preference-based alignment using DPO, IPO, KTO, or ORPO with TRL's DPOTrainer. Covers preference data formatting (prompt/chosen/rejected), beta tuning, loss variants, and reference model management. Use when aligning a fine-tuned LLM with human or AI preferences. Do not use for supervised fine-tuning, reward model training, or PPO-based RLHF.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: preference-optimization
  maturity: draft
  risk: low
  tags: [preference, dpo, alignment, trl, rlhf]
---

# Purpose

Align language models with human preferences using Direct Preference Optimization (DPO) and related methods via TRL's DPOTrainer, covering data preparation, loss configuration, beta tuning, and evaluation of alignment quality.

# When to use this skill

Use this skill when:

- Setting up DPO training: `from trl import DPOTrainer, DPOConfig`
- Configuring DPO: `DPOConfig(beta=0.1, loss_type="sigmoid", learning_rate=5e-7, max_length=1024)`
- Preparing preference datasets in `{"prompt": ..., "chosen": ..., "rejected": ...}` format
- Choosing between DPO variants: standard DPO, IPO (bounded loss), KTO (unpaired preferences), ORPO (odds-ratio)
- Managing reference model: frozen copy vs implicit reference via LoRA
- Deciding DPO vs full RLHF (PPO with reward model) for a given alignment task

# Do not use this skill when

- The task is supervised fine-tuning on instruction data — use `instruction-tuning` or `fine-tuning`
- The task is training a reward model for PPO-based RLHF — use `reward-modeling`
- The task is about generating synthetic preference data — use `synthetic-data-generation`
- The task has no alignment or preference learning component

# Operating procedure

1. **Prepare preference data**: Format dataset as `{"prompt": str, "chosen": str, "rejected": str}`. Each row is one comparison. Source from human annotations, AI feedback (Constitutional AI), or ranked model outputs. Ensure chosen/rejected share the same prompt. Load with `datasets.load_dataset()`.
2. **Initialize models**: Load the SFT model as the policy. For standard DPO, create a frozen reference copy: `ref_model = AutoModelForCausalLM.from_pretrained(model_path)`. With LoRA/QLoRA, omit `ref_model` — TRL uses the base LoRA-free model as implicit reference.
3. **Configure DPO training**:
   ```python
   from trl import DPOTrainer, DPOConfig
   training_args = DPOConfig(
       beta=0.1,                    # KL penalty strength
       loss_type="sigmoid",         # standard DPO loss
       learning_rate=5e-7,          # lower than SFT LR
       per_device_train_batch_size=4,
       gradient_accumulation_steps=4,
       max_length=1024,
       max_prompt_length=512,
       num_train_epochs=1,          # 1-3 epochs typical
   )
   ```
4. **Initialize trainer**: `trainer = DPOTrainer(model=model, ref_model=ref_model, args=training_args, train_dataset=dataset, tokenizer=tokenizer)`. Call `trainer.train()`.
5. **Monitor training**: Track `rewards/chosen` (should increase), `rewards/rejected` (should decrease), `rewards/margins` (should widen), and `rewards/accuracies` (fraction where chosen scores higher). Log with wandb.
6. **Evaluate alignment**: Test on held-out preference pairs. Run MT-Bench or AlpacaEval for general chat quality. Check for over-optimization: if `rewards/margins` grows but eval quality drops, reduce beta or stop early.

# Decision rules

- **DPO** (`loss_type="sigmoid"`): Default choice. Direct optimization of `L = -log(σ(β * (log π(y_w|x)/π_ref(y_w|x) - log π(y_l|x)/π_ref(y_l|x))))`. Simple, stable, well-understood.
- **IPO** (`loss_type="ipo"`): Adds regularization to prevent overfitting to preference noise. Use when data is noisy or small (<10k pairs).
- **KTO** (Kahneman-Tversky): Works with unpaired data (only "good" or "bad" examples, not paired comparisons). Use when paired preference data is expensive to collect.
- **ORPO**: Combines SFT and preference optimization in one step, no reference model needed. Use to reduce training pipeline complexity.
- `beta=0.1` is the standard starting point. Lower beta (0.01-0.05) for weaker alignment; higher (0.2-0.5) for stronger but riskier constraint.
- Use `learning_rate` 5-10x lower than SFT LR. DPO is sensitive to high LR — causes reward hacking.
- 1 epoch is usually sufficient. Multiple epochs risk overfitting to preference data.
- DPO is preferred over PPO-RLHF when: no reward model exists, compute is limited, or simplicity is valued. PPO-RLHF is preferred when: fine-grained reward shaping is needed or the reward signal is complex.

# Output requirements

1. `DPO Config` — Complete `DPOConfig` with beta, loss_type, LR, batch size, max lengths
2. `Data Spec` — Preference dataset format, size, source (human/AI), and quality checks applied
3. `Training Metrics` — Reward margins, chosen/rejected reward curves, training loss, accuracy on held-out pairs
4. `Alignment Evaluation` — MT-Bench scores, AlpacaEval win rates, or task-specific alignment benchmarks

# References

- Rafailov et al., "Direct Preference Optimization: Your Language Model is Secretly a Reward Model" (arxiv 2305.18290)
- Azar et al., "A General Theoretical Paradigm to Understand Learning from Human Feedback" — IPO (arxiv 2310.12036)
- Ethayarajh et al., "KTO: Model Alignment as Prospect Theoretic Optimization" (arxiv 2402.01306)
- Hong et al., "ORPO: Monolithic Preference Optimization without Reference Model" (arxiv 2403.07691)
- TRL documentation: https://huggingface.co/docs/trl/dpo_trainer
- `from trl import DPOTrainer, DPOConfig` — HuggingFace TRL library

# Related skills

- `fine-tuning` — SFT stage that precedes DPO in the standard alignment pipeline
- `reward-modeling` — alternative path using explicit reward models with PPO
- `safety-alignment` — broader alignment goals that DPO serves
- `dataset-curation` — quality of preference data directly impacts DPO outcomes

# Failure handling

- If `rewards/margins` plateau near zero after 100+ steps, the model is not learning preferences — check data quality, increase beta, or verify chosen/rejected are meaningfully different.
- If `rewards/accuracies` hits 1.0 early in training, the preference pairs are too easy — add harder near-boundary comparisons.
- If eval quality degrades while training metrics improve, this is reward over-optimization — reduce beta, stop training earlier, or use IPO.
- If OOM with full reference model, switch to LoRA-based DPO (implicit reference) or use `ref_model=None` with ORPO.
