---
name: instruction-tuning
description: Performs supervised fine-tuning on instruction-following data using TRL SFTTrainer, chat templates, response-only loss masking, and sequence packing. Covers Alpaca/ShareGPT/OpenAI data formats, data mixing, and quality filtering. Use when training a model to follow instructions or hold conversations.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: instruction-tuning
  maturity: draft
  risk: low
  tags: [instruction-tuning, SFT, chat-template, TRL, SFTTrainer]
---

# Purpose

Train LLMs to follow instructions and hold multi-turn conversations via supervised fine-tuning (SFT) on curated instruction data, using HuggingFace TRL, chat templates, response-only loss masking, and efficient sequence packing.

# When to use this skill

Use this skill when:

- training a base model to follow instructions or hold multi-turn conversations
- formatting datasets into Alpaca, ShareGPT, or OpenAI chat format for SFT
- configuring `SFTTrainer` with chat templates and response-only loss masking
- designing data mixing strategies across conversation, QA, coding, and math tasks
- implementing sequence packing to maximize training throughput

# Do not use this skill when

- the task is domain-specific fine-tuning on non-instruction data (use `fine-tuning` for task adaptation with LoRA/QLoRA)
- the goal is RLHF, DPO, or preference-based alignment (use `preference-optimization`)
- you only need to evaluate a model's instruction-following (use `eval-dataset-design`)

# Operating procedure

1. **Choose data format.** Three standard formats:
   - **Alpaca**: `{"instruction": "...", "input": "...", "output": "..."}`
   - **ShareGPT**: `{"conversations": [{"from": "human", "value": "..."}, {"from": "gpt", "value": "..."}]}`
   - **OpenAI**: `{"messages": [{"role": "system", "content": "..."}, {"role": "user", "content": "..."}, {"role": "assistant", "content": "..."}]}`
2. **Apply chat template.** Use the tokenizer's built-in template:
   ```python
   from transformers import AutoTokenizer
   tokenizer = AutoTokenizer.from_pretrained("meta-llama/Llama-3-8B-Instruct")
   formatted = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=False)
   ```
   Ensure the template matches the model's expected format (ChatML, Llama, Zephyr, etc.).
3. **Configure response-only loss masking.** Train only on assistant turns — mask user/system tokens from the loss. In TRL, use `DataCollatorForCompletionOnlyLM` with the assistant response template token:
   ```python
   from trl import DataCollatorForCompletionOnlyLM
   collator = DataCollatorForCompletionOnlyLM(
       response_template="<|assistant|>", tokenizer=tokenizer
   )
   ```
4. **Mix data sources.** Combine diverse tasks: 40% general conversation, 20% QA, 20% code, 10% math, 10% creative writing. Adjust proportions based on target use case. Oversample underrepresented categories.
5. **Filter for quality.** Remove duplicates (exact + near-duplicate via MinHash). Filter by response length (drop < 10 tokens). Remove examples with toxic content via classifier. Optionally score with a reward model and keep top 80%.
6. **Configure SFTTrainer with packing.**
   ```python
   from trl import SFTTrainer, SFTConfig
   config = SFTConfig(
       output_dir="./sft-output", max_seq_length=2048,
       packing=True, num_train_epochs=2,
       per_device_train_batch_size=4, gradient_accumulation_steps=4,
       learning_rate=2e-5, lr_scheduler_type="cosine", warmup_ratio=0.03,
       bf16=True, logging_steps=10, save_strategy="epoch"
   )
   trainer = SFTTrainer(model=model, args=config, train_dataset=dataset,
                         data_collator=collator, tokenizer=tokenizer)
   ```
   Packing concatenates multiple short examples into one sequence to fill `max_seq_length`, improving GPU utilization.
7. **Evaluate.** Measure held-out loss, MT-Bench score, AlpacaEval win rate, or custom instruction-following eval. Compare against base model and prior SFT checkpoints.

# Decision rules

- Use response-only loss masking for all instruction tuning — training on user prompts wastes capacity and can cause parroting.
- Enable packing when average example length < 50% of `max_seq_length`.
- If the model struggles with multi-turn, increase the proportion of multi-turn conversation data.
- Learning rate 2e-5 for full SFT, 2e-4 if combining with LoRA (delegate to `fine-tuning` skill for adapter config).
- Minimum dataset: 10k high-quality instruction pairs; 50k+ for general-purpose chat models.
- If perplexity on a held-out general set degrades by >5%, reduce training data volume or add regularization.

# Output requirements

1. `Data pipeline` — format, cleaning steps, mixing ratios, final dataset size
2. `Training config` — SFTConfig params, chat template, loss masking setup, packing config
3. `Training logs` — loss curves, learning rate schedule, gradient norms
4. `Evaluation report` — held-out loss, MT-Bench/AlpacaEval scores, qualitative examples
5. `Model artifacts` — saved model/adapter, tokenizer, training config YAML

# References

- TRL SFTTrainer: https://huggingface.co/docs/trl/sft_trainer
- Alpaca dataset format: https://github.com/tatsu-lab/stanford_alpaca
- ShareGPT format: https://huggingface.co/datasets/anon8231489123/ShareGPT_Vicuna_unfiltered
- MT-Bench: Zheng et al., "Judging LLM-as-a-Judge" (arXiv:2306.05685)
- LIMA: Zhou et al., "LIMA: Less Is More for Alignment" (arXiv:2305.11206)

# Related skills

- `fine-tuning`
- `preference-optimization`
- `synthetic-data-generation`
- `eval-dataset-design`

# Failure handling

- If training loss plateaus immediately, verify chat template produces correctly tokenized sequences — print a decoded example to confirm.
- If the model outputs empty or repeated tokens, check that the loss mask is not accidentally masking all tokens (log the fraction of unmasked tokens per batch).
- If multi-turn performance is poor, ensure conversation history is included in the input, not just the last user turn.
