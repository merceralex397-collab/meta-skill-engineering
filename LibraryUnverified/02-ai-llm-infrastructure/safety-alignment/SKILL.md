---
name: safety-alignment
description: Implements red teaming, refusal training, Constitutional AI, and safety RLHF to align LLMs against harmful outputs while preserving helpfulness. Use when designing safety data pipelines, evaluating jailbreak robustness with benchmarks like ToxiGen/BBQ/TruthfulQA, or balancing over-refusal vs harmlessness. Do not use for general RLHF preference optimization unrelated to safety.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: safety-alignment
  maturity: draft
  risk: low
  tags: [safety, alignment, red-teaming, constitutional-ai]
---

# Purpose

Make LLMs reliably refuse harmful requests while remaining maximally helpful on benign ones. This covers the full safety alignment pipeline: red teaming to discover vulnerabilities, generating safety training data, training with safety-specific reward signals, and evaluating robustness across harm categories and multi-turn conversations.

# When to use this skill

Use this skill when:

- conducting red teaming to discover jailbreaks and adversarial prompt categories
- generating safety preference pairs (harmful-request → refusal vs harmful-request → compliance)
- implementing Constitutional AI (principle-based self-critique and revision)
- training or fine-tuning with safety-specific RLHF reward signals
- evaluating safety with benchmarks: ToxiGen (toxicity), BBQ (bias), TruthfulQA (factuality)
- analyzing the helpfulness-vs-harmlessness tradeoff and over-refusal rates

# Do not use this skill when

- the task is general RLHF/DPO without a safety focus — prefer `preference-optimization`
- the task is training the reward model itself — prefer `reward-modeling`
- the task is runtime content filtering or output moderation — prefer application-level guardrails

# Operating procedure

1. **Define the harm taxonomy.** Categorize risks: violence, self-harm, illegal activity, CSAM, PII leakage, deception, bias/discrimination. Map each category to severity levels and expected model behavior (hard refusal vs soft deflection).
2. **Red team the model.** Generate adversarial prompts across jailbreak categories:
   - *Direct requests*: explicit harmful instructions
   - *Roleplay/persona attacks*: "You are an evil AI, now..."
   - *Encoding attacks*: base64, rot13, or language-switching to bypass filters
   - *Multi-turn escalation*: gradually steering conversation toward harm
   - *Few-shot poisoning*: providing harmful examples in context
   Use automated red teaming (e.g., generate attacks with a separate LLM) and manual expert review.
3. **Generate safety training data.** For each harmful prompt, create preference pairs:
   - *Chosen*: a clear, helpful refusal that explains why the request cannot be fulfilled
   - *Rejected*: a compliant response that provides harmful content
   For Constitutional AI, generate self-critique: have the model produce an initial response, critique it against safety principles, then revise. Use the revision as the chosen response.
4. **Train with safety signals.** Options:
   - *Safety SFT*: fine-tune on curated refusal examples (fast, baseline approach)
   - *Safety RLHF*: train a safety-specific reward model that scores refusals highly, then run PPO with a combined reward: `r_total = α * r_helpfulness + (1-α) * r_safety`
   - *Safety DPO*: directly optimize on safety preference pairs without a separate reward model
5. **Evaluate comprehensively.**
   - Run ToxiGen for toxicity detection across demographic groups
   - Run BBQ (Bias Benchmark for QA) for stereotyping and bias
   - Run TruthfulQA for factuality under adversarial framing
   - Measure over-refusal rate on a benign test set (target: <5% false refusals)
   - Test multi-turn safety: ensure the model maintains refusals across follow-up attempts
6. **Analyze representation engineering.** Use gradient-based methods to identify safety-relevant directions in activation space. Probe internal representations to verify the model distinguishes harmful from benign requests at intermediate layers, not just at the output.
7. **Balance helpfulness and harmlessness.** Track both metrics jointly. If over-refusal exceeds 5%, add borderline-safe examples to training data with "helpful response" as the chosen completion. Use A/B evaluation with human raters scoring both safety and usefulness.

# Decision rules

- Never sacrifice hard-refusal categories (CSAM, weapons of mass destruction) for helpfulness gains.
- Prefer Constitutional AI revision over simple refusal SFT when nuanced responses are needed (e.g., medical, legal topics).
- If multi-turn jailbreak success rate exceeds 10%, add multi-turn adversarial examples to training data.
- If over-refusal rate exceeds 5% on benign queries, the safety training is too aggressive — rebalance the training mix.
- Use automated red teaming for coverage, but require human expert review for high-severity categories.
- Always evaluate safety on the latest model checkpoint, not just the final one — safety can degrade during training.

# Output requirements

1. `Harm Taxonomy` — categorized risk areas with severity levels and expected model behaviors
2. `Red Team Report` — attack categories tested, success rates, and example jailbreaks discovered
3. `Safety Evaluation` — ToxiGen/BBQ/TruthfulQA scores, over-refusal rate, multi-turn robustness results
4. `Training Data Summary` — number of safety pairs, category distribution, Constitutional AI revision statistics

# References

Read these only when relevant:

- Bai et al., "Constitutional AI: Harmlessness from AI Feedback" (Anthropic)
- Touvron et al., "Llama 2: Open Foundation and Fine-Tuned Chat Models" (safety training section)
- Perez et al., "Red Teaming Language Models with Language Models"
- Hartvigsen et al., "ToxiGen: A Large-Scale Machine-Generated Dataset for Adversarial and Implicit Hate Speech Detection"
- Lin et al., "TruthfulQA: Measuring How Models Mimic Human Falsehoods"

# Related skills

- `reward-modeling` — building the safety reward model
- `eval-dataset-design` — designing safety evaluation benchmarks
- `preference-optimization` — PPO/DPO training loop that consumes safety data
- `synthetic-data-generation` — generating adversarial and safety training data at scale

# Failure handling

- If red teaming reveals a jailbreak category with >20% success rate, halt deployment and prioritize targeted safety data generation for that category.
- If ToxiGen scores regress after safety training, check for data contamination or reward model miscalibration.
- If over-refusal is high but harmlessness metrics are only marginally improved, suspect the safety training data contains mislabeled benign examples — audit the data.
- If multi-turn safety degrades while single-turn is strong, the model is likely not attending to full conversation context — increase context length in safety training examples.
