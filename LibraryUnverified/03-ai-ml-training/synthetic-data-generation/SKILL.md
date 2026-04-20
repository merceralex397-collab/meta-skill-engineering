---
name: synthetic-data-generation
description: Generates synthetic instruction-response training data using Self-Instruct, Evol-Instruct, and distillation from strong models via OpenAI/Anthropic APIs or vLLM batch inference. Use when creating instruction-tuning datasets, evolving seed tasks for complexity, filtering for quality and diversity, or generating domain-specific training data (code, math, reasoning). Do not use for human annotation pipeline design.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: synthetic-data-generation
  maturity: draft
  risk: low
  tags: [synthetic, data, generation, self-instruct, distillation]
---

# Purpose

Generate high-quality synthetic instruction-response pairs at scale for fine-tuning LLMs. Covers Self-Instruct (bootstrapping from seed tasks), Evol-Instruct (iteratively evolving complexity), and distillation-based generation (using strong model outputs as training data). Includes quality filtering, deduplication, and formatting for training pipelines.

# When to use this skill

Use this skill when:

- bootstrapping an instruction-tuning dataset from a small set of seed examples (Self-Instruct)
- evolving instructions for increasing complexity and diversity (Evol-Instruct / WizardLM approach)
- distilling outputs from a strong model (GPT-4, Claude) into training data for a smaller model
- generating domain-specific synthetic data: code, math, reasoning chains, or multi-turn conversations
- filtering and deduplicating generated data for training quality
- running batch inference with vLLM or API calls for large-scale generation

# Do not use this skill when

- the task is fine-tuning on the generated data — prefer `instruction-tuning`
- the task is creating preference pairs for RLHF — prefer `reward-modeling` or `safety-alignment`
- the task is cleaning and labeling human-collected data — prefer `data-cleaning-labeling`

# Operating procedure

1. **Design seed tasks.** Create 50–200 diverse, high-quality seed examples with `instruction`, `input` (optional), and `output`. Cover task types (QA, summarization, code, math), complexity levels (factual → multi-step reasoning), and target domains.
2. **Generate via Self-Instruct.** Prompt the teacher model with 3–5 seeds to generate new instruction-response pairs. Generate 5–10× the seed count per iteration. Filter and add high-quality outputs back as seeds for the next round.
3. **Apply Evol-Instruct for complexity.** Evolve existing instructions by adding constraints, deepening reasoning, increasing specificity, or adding edge cases. Run 2–4 evolution rounds. Discard incoherent outputs (filter by perplexity or LLM judge).
4. **Distill from strong models.** For highest quality, generate responses using a frontier model:
   ```python
   from openai import OpenAI
   client = OpenAI()
   response = client.chat.completions.create(
       model="gpt-4o",
       messages=[{"role": "user", "content": instruction}],
       temperature=0.7,
       max_tokens=2048,
   )
   ```
   For batch generation at scale, use vLLM offline inference:
   ```python
   from vllm import LLM, SamplingParams
   llm = LLM(model="meta-llama/Llama-3.1-70B-Instruct")
   params = SamplingParams(temperature=0.7, max_tokens=2048)
   outputs = llm.generate(prompts, params)
   ```
5. **Filter for quality.** Apply multi-stage filtering:
   - *Deduplication*: MinHash or embedding-based near-duplicate removal (>0.85 cosine similarity)
   - *Coherence*: Discard responses that don't address the instruction (use a classifier or LLM judge)
   - *Length filtering*: Remove very short (<20 tokens) or excessively long responses
   - *Safety filtering*: Remove toxic, biased, or harmful content with a safety classifier
   - *Consistency*: Generate 2–3 responses per instruction, keep only those with consistent information
6. **Format for training.** Convert to the target chat template (ChatML, Llama, etc.):
   ```json
   {"messages": [
     {"role": "system", "content": "You are a helpful assistant."},
     {"role": "user", "content": "<instruction>"},
     {"role": "assistant", "content": "<response>"}
   ]}
   ```
   Save as JSONL. Include metadata: generation model, temperature, timestamp, and quality scores.
7. **Manage volume and diversity.** Track category distribution with a diversity budget. If any task type exceeds 30% of the dataset, down-sample or generate more from underrepresented categories. Target 10K–100K examples for instruction tuning, 1K–10K for domain-specific fine-tuning.

# Decision rules

- Use Self-Instruct when starting from scratch with limited seed data (<200 examples).
- Use Evol-Instruct when the base dataset exists but lacks complexity or diversity.
- Use distillation when quality matters more than cost — frontier model outputs produce the best training signal.
- Prefer vLLM batch inference over API calls when generating >10K examples (10–100× cheaper).
- Always deduplicate before training — duplicate examples cause memorization and reduce generalization.
- If generating code data, validate syntax by attempting to parse (AST parsing for Python, compilation for compiled languages).
- For math data, verify answers with a symbolic solver or separate model before including.

# Output requirements

1. `Dataset Manifest` — total examples, category distribution, generation method per subset, and quality filter pass rates
2. `Seed Task Documentation` — the seed examples used, diversity analysis, and coverage gaps
3. `Quality Report` — deduplication rate, coherence filter rejection rate, sample quality audit results
4. `Training-Ready Files` — JSONL formatted data with chat template applied, split into train/validation

# References

Read these only when relevant:

- Wang et al., "Self-Instruct: Aligning Language Models with Self-Generated Instructions"
- Xu et al., "WizardLM: Empowering Large Language Models to Follow Complex Instructions" (Evol-Instruct)
- Taori et al., "Stanford Alpaca: An Instruction-following LLaMA Model"
- vLLM offline inference: https://docs.vllm.ai/en/latest/getting_started/examples/offline_inference.html
- OpenAI Batch API: https://platform.openai.com/docs/guides/batch

# Related skills

- `instruction-tuning` — fine-tuning on the generated data
- `data-cleaning-labeling` — cleaning and annotating raw human data
- `eval-dataset-design` — creating evaluation sets (not training data)
- `reward-modeling` — generating preference pairs for RLHF

# Failure handling

- If generated instructions are repetitive despite diverse seeds, increase temperature (0.8–1.0) and add explicit diversity prompts ("Generate a task different from all previous ones").
- If quality filter rejects >50% of generated data, the teacher model or prompt template is poor — revise the generation prompt before scaling up.
- If Evol-Instruct produces incoherent outputs after 2+ rounds, cap evolution depth and use the last coherent version.
- If API rate limits block large-scale generation, switch to vLLM batch inference with an open-weight model or use the OpenAI Batch API (50% cost reduction, 24-hour turnaround).
