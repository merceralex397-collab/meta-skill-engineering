---
name: fine-tuning
description: Guides full and parameter-efficient fine-tuning (LoRA, QLoRA) of LLMs using PEFT, TRL SFTTrainer, and BitsAndBytes. Covers adapter config, hyperparameter selection, data formatting, and evaluation. Use when adapting a pretrained model to a specific task or domain.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: fine-tuning
  maturity: draft
  risk: low
  tags: [fine-tuning, LoRA, QLoRA, PEFT, SFTTrainer, TRL]
---

# Purpose

Guide the selection and execution of fine-tuning strategies for LLMs — from full fine-tuning to parameter-efficient methods (LoRA, QLoRA, prefix tuning) — using HuggingFace PEFT, TRL, and Transformers. Includes adapter configuration, quantization setup, data formatting, and evaluation.

# When to use this skill

Use this skill when:

- adapting a pretrained LLM to a specific downstream task or domain
- choosing between full fine-tuning, LoRA, QLoRA, or prefix tuning
- configuring PEFT adapters, quantization, or training hyperparameters
- preparing instruction/input/output datasets for supervised fine-tuning
- evaluating whether fine-tuning improved performance over the base model

# Do not use this skill when

- the task is prompt engineering or in-context learning without weight updates
- the goal is instruction-tuning on chat data (use `instruction-tuning` for SFT on conversations)
- the task is RLHF or DPO alignment (use `preference-optimization`)
- you have fewer than ~100 labeled examples (try few-shot prompting first)

# Operating procedure

1. **Decide tuning strategy.** Full fine-tuning: updates all parameters, requires multi-GPU, best for large high-quality datasets (>50k examples). LoRA: trains low-rank adapters on attention projections, 10–100× fewer trainable params. QLoRA: loads base model in 4-bit, trains LoRA adapters in fp16, fits 65B models on a single 48GB GPU.
2. **Configure LoRA adapter.** Use PEFT:
   ```python
   from peft import LoraConfig, get_peft_model
   lora_config = LoraConfig(
       r=16, lora_alpha=32,
       target_modules=["q_proj", "v_proj"],
       lora_dropout=0.05, bias="none", task_type="CAUSAL_LM"
   )
   model = get_peft_model(base_model, lora_config)
   ```
   For QLoRA, first quantize: `BitsAndBytesConfig(load_in_4bit=True, bnb_4bit_quant_type="nf4", bnb_4bit_compute_dtype=torch.bfloat16)`.
3. **Prepare data.** Format as `{"instruction": "...", "input": "...", "output": "..."}` or chat-template JSON. Ensure a held-out validation split (10–20%). Clean duplicates and filter low-quality examples.
4. **Configure training.** Use TRL's SFTTrainer:
   ```python
   from trl import SFTTrainer, SFTConfig
   training_args = SFTConfig(
       output_dir="./output", num_train_epochs=3,
       per_device_train_batch_size=4, gradient_accumulation_steps=4,
       learning_rate=2e-4, lr_scheduler_type="cosine", warmup_ratio=0.03,
       bf16=True, logging_steps=10, save_strategy="epoch",
       max_seq_length=2048
   )
   trainer = SFTTrainer(model=model, args=training_args, train_dataset=dataset)
   ```
5. **Evaluate.** Track validation loss each epoch. Run task-specific metrics (accuracy, F1, ROUGE, pass@k). Compare against base model zero-shot and few-shot baselines. Check for catastrophic forgetting on a held-out general benchmark.
6. **Merge and export.** For LoRA: `model.merge_and_unload()` to produce a standalone model, or ship adapter weights separately for modularity.

# Decision rules

- If prompt engineering achieves ≥90% of target quality, do not fine-tune — save compute.
- Use LoRA (r=8–64) as the default starting point; only go to full fine-tuning if LoRA plateaus.
- Use QLoRA when GPU VRAM is limited (≤24GB) and model is ≥13B parameters.
- Set learning rate to 2e-4 for LoRA, 1e-5 for full fine-tuning. Use cosine schedule with 3% warmup.
- Minimum dataset: ~500 examples for LoRA on focused tasks; ~5k for robust generalization.
- If validation loss increases for 2+ consecutive evaluations, stop training (early stopping).

# Output requirements

1. `Adapter config` — LoRA rank, alpha, target modules, dropout, quantization config
2. `Training config` — learning rate, batch size, epochs, scheduler, gradient accumulation
3. `Data spec` — format, size, split ratios, preprocessing steps
4. `Evaluation report` — base model vs. fine-tuned on held-out set, with task-specific metrics
5. `Exported artifacts` — adapter weights or merged model, tokenizer, training logs

# References

- LoRA: Hu et al., "Low-Rank Adaptation of Large Language Models" (arXiv:2106.09685)
- QLoRA: Dettmers et al., "QLoRA: Efficient Finetuning of Quantized LLMs" (arXiv:2305.14314)
- PEFT library: https://github.com/huggingface/peft
- TRL library: https://github.com/huggingface/trl
- BitsAndBytes: https://github.com/TimDettmers/bitsandbytes

# Related skills

- `instruction-tuning`
- `preference-optimization`
- `eval-dataset-design`
- `llm-creation`

# Failure handling

- If training loss does not decrease after 1 epoch, verify data formatting and check that labels are not masked entirely.
- If OOM occurs, reduce batch size, enable gradient checkpointing (`model.gradient_checkpointing_enable()`), or switch to QLoRA.
- If fine-tuned model regresses on general tasks, reduce epochs, lower learning rate, or add general-domain data to the mix.
