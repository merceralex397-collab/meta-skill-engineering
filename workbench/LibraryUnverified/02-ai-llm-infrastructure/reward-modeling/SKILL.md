---
name: reward-modeling
description: Trains and evaluates reward models that score LLM outputs for RLHF pipelines using Bradley-Terry preference modeling and TRL's RewardTrainer. Use when building a scalar reward signal from pairwise human preferences, diagnosing reward hacking, or choosing between process and outcome reward models. Do not use for the RL policy optimization step itself.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: reward-modeling
  maturity: draft
  risk: low
  tags: [reward, modeling, rlhf, preferences]
---

# Purpose

Train a scalar reward model from pairwise human preference data to provide the training signal for RLHF or best-of-N sampling. The reward model takes a prompt + completion and outputs a single scalar score. It is trained so that preferred completions score higher than rejected ones under the Bradley-Terry model.

# When to use this skill

Use this skill when:

- training a reward model from pairwise preference data (chosen vs rejected completions)
- implementing the Bradley-Terry loss: `-log(sigmoid(r_chosen - r_rejected))`
- configuring TRL's `RewardTrainer` or building a custom reward training loop
- diagnosing reward hacking or overoptimization in an RLHF pipeline
- choosing between process reward models (PRM) and outcome reward models (ORM)
- evaluating reward model agreement with held-out human preferences

# Do not use this skill when

- the task is the PPO/DPO policy optimization step — prefer `preference-optimization`
- the task is building safety-specific reward signals — prefer `safety-alignment`
- the task is designing the preference annotation interface or guidelines — prefer `eval-dataset-design`

# Operating procedure

1. **Prepare preference data.** Each example must have: `prompt`, `chosen` completion, `rejected` completion. Format as HuggingFace Dataset with columns matching TRL expectations. Ensure annotator agreement ≥70% on a validation split.
2. **Initialize the reward model.** Start from the same base model (or SFT checkpoint) that the policy will use. Add a value head that projects the final hidden state to a scalar:
   ```python
   from trl import AutoModelForCausalLMWithValueHead
   reward_model = AutoModelForCausalLMWithValueHead.from_pretrained("meta-llama/Llama-3.1-8B-Instruct")
   ```
3. **Configure training.** Use `RewardConfig` to set learning rate (1e-5 to 5e-6), max sequence length, and gradient accumulation:
   ```python
   from trl import RewardTrainer, RewardConfig
   config = RewardConfig(
       output_dir="./reward_model",
       per_device_train_batch_size=4,
       num_train_epochs=1,
       learning_rate=1.5e-5,
       max_length=2048,
   )
   trainer = RewardTrainer(
       model=reward_model,
       args=config,
       train_dataset=train_dataset,
       processing_class=tokenizer,
   )
   trainer.train()
   ```
4. **Apply the Bradley-Terry loss.** The default TRL loss is: `loss = -log(sigmoid(r_chosen - r_rejected))`. This maximizes the margin between chosen and rejected scores. Monitor the mean margin during training — it should increase steadily.
5. **Evaluate the reward model.**
   - *Agreement*: Measure accuracy on held-out preference pairs (target: >70%).
   - *Reward distribution*: Plot score histograms for chosen vs rejected — distributions should separate clearly.
   - *Calibration*: Check that reward differences correlate with annotator confidence levels.
6. **Guard against reward hacking.** When used in RLHF, the policy will exploit reward model weaknesses. Mitigations:
   - Apply a KL penalty: `reward_total = reward - β * KL(policy || reference)` with β ∈ [0.01, 0.1].
   - Use reward model ensembles and take the conservative (minimum) score.
   - Monitor output length — reward hacking often manifests as length gaming.
7. **Choose PRM vs ORM.** Use outcome reward models (ORM) for general preference alignment. Use process reward models (PRM) for step-by-step reasoning tasks (math, code) where intermediate steps can be scored individually.

# Decision rules

- Train for only 1 epoch on preference data to avoid overfitting — reward models overfit quickly.
- If agreement on held-out data is <65%, the preference data quality is insufficient; collect more annotations before proceeding.
- Use a base model size ≥ policy model size for the reward model when possible (larger RMs are more robust).
- Prefer margin-based evaluation (mean reward gap) over raw accuracy for tracking training progress.
- If the reward model assigns systematically higher scores to longer outputs, add a length penalty or normalize by length.

# Output requirements

1. `Reward Model Checkpoint` — saved model with value head, tokenizer, and training config
2. `Evaluation Report` — held-out accuracy, mean chosen/rejected margin, reward distribution plots
3. `Data Summary` — preference dataset statistics: size, source, annotator agreement, domain coverage
4. `Integration Notes` — how to load the reward model for PPO training or best-of-N sampling

# References

Read these only when relevant:

- TRL RewardTrainer docs: https://huggingface.co/docs/trl/reward_trainer
- Ouyang et al., "Training language models to follow instructions with human feedback" (InstructGPT)
- Stiennon et al., "Learning to summarize from human feedback"
- Lightman et al., "Let's Verify Step by Step" (process reward models)

# Related skills

- `preference-optimization` — PPO/DPO policy training that consumes the reward model
- `safety-alignment` — safety-specific reward signals and refusal training
- `eval-dataset-design` — designing preference annotation tasks
- `synthetic-data-generation` — generating synthetic preference pairs

# Failure handling

- If reward model accuracy plateaus below 65%, audit the preference data for label noise, contradictory pairs, or insufficient prompt diversity.
- If reward scores collapse to a narrow range, reduce learning rate and check for gradient issues in the value head.
- If the policy exploits the reward model during RLHF, increase the KL penalty β or retrain the RM with adversarial examples from the exploiting policy.
- If PRM training fails to improve step-level accuracy, verify that step-level annotations are consistent and that the granularity matches the model's generation pattern.
