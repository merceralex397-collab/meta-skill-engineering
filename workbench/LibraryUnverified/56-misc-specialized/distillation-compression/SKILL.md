---
name: distillation-compression
description: Transfers knowledge from large teacher models to smaller students via soft-label distillation (KL divergence, temperature scaling), feature-based distillation (intermediate layer matching, attention transfer), and structured pruning (head pruning, layer dropping). Use when reducing model size while preserving quality. Do not use for quantization-only workflows or MoE conversion.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: distillation-compression
  maturity: draft
  risk: low
  tags: [distillation, compression, pruning, knowledge-transfer]
---

# Purpose

Transfers knowledge from large teacher models into smaller, faster student models using soft-label distillation, feature-based distillation, and structured pruning techniques. Covers temperature scaling, KL divergence losses, intermediate layer matching, attention transfer, head/layer pruning, distillation data selection, and quality-speed tradeoff evaluation.

# When to use this skill

Use this skill when:

- training a student model on soft labels from a teacher model (DistilBERT, TinyLlama patterns)
- implementing KL divergence loss with temperature scaling for logit-based distillation
- matching intermediate representations between teacher and student (feature-based distillation, attention transfer)
- performing structured pruning: removing attention heads, dropping layers, or reducing hidden dimensions
- choosing what data to distill on (task-specific vs. task-agnostic, data selection strategies)
- evaluating quality-speed tradeoffs: benchmarking student vs. teacher on accuracy, latency, and memory

# Do not use this skill when

- the task is quantization without distillation (use `quantization-research`)
- the task is dense-to-MoE conversion (use `dense-to-moe-experiments`)
- the task is full pretraining from scratch (use `pretraining-pipeline`)
- the task is inference kernel optimization without model changes (use `inference-kernel-optimization`)

# Operating procedure

1. **Define teacher and student architectures.** Choose student size: typical compression ratios are 2x–6x parameter reduction. For transformer LLMs, reduce by: (a) fewer layers (e.g., 12→6), (b) smaller hidden dim (e.g., 768→384), (c) fewer attention heads (e.g., 12→6), or combinations. Initialize student from teacher by copying a subset of layers (e.g., every other layer) when architectures are compatible.
2. **Implement soft-label distillation loss.** Combine hard-label cross-entropy with KL divergence on softened logits:
   ```python
   import torch.nn.functional as F
   T = 4.0  # temperature
   alpha = 0.5  # balance between hard and soft loss
   soft_teacher = F.log_softmax(teacher_logits / T, dim=-1)
   soft_student = F.log_softmax(student_logits / T, dim=-1)
   loss_kd = F.kl_div(soft_student, soft_teacher.exp(), reduction='batchmean') * (T ** 2)
   loss_hard = F.cross_entropy(student_logits, labels)
   loss = alpha * loss_kd + (1 - alpha) * loss_hard
   ```
   Temperature T=2–6 works well for most tasks. Higher T for larger teacher-student capacity gaps. The T² factor compensates for gradient scaling.
3. **Add feature-based distillation (optional).** Match intermediate layer representations:
   - **Hidden state matching**: add MSE loss between teacher and student hidden states at aligned layers. Use a linear projection if dimensions differ: `proj = nn.Linear(student_dim, teacher_dim)`.
   - **Attention transfer**: match attention weight matrices: `loss_attn = MSE(student_attn_weights, teacher_attn_weights)` across aligned heads.
   - Typically weight feature losses at 0.1–0.5x the main distillation loss.
4. **Select distillation data.** For task-specific distillation, use the target task's training data plus 2–5x augmented data labeled by the teacher. For task-agnostic distillation, use a diverse general corpus (Wikipedia + BookCorpus + code). Prioritize samples where teacher confidence is moderate (entropy in top 30%) — these carry the most information.
5. **Apply structured pruning (if combining with distillation).** Prune before or during distillation:
   - **Head pruning**: compute importance scores per head (gradient-based or Taylor expansion). Remove heads where importance < 10% of max. Typical: remove 30–50% of heads with <2% accuracy loss.
   - **Layer dropping**: remove bottom-ranked layers by probing accuracy. For LLMs, middle layers are often most droppable.
   - **Width reduction**: reduce FFN intermediate size by pruning neurons with lowest activation magnitude.
   - After pruning, fine-tune with distillation loss for 1–3 epochs to recover quality.
6. **Train the student.** Use learning rate 3–5x higher than teacher's fine-tuning LR. Train for 3–10 epochs on distillation data. Use cosine LR schedule with warmup. Monitor both distillation loss and downstream task metrics every epoch.
7. **Evaluate quality-speed tradeoff.** Report for both teacher and student: (a) task accuracy/F1 on held-out test set, (b) model size (params, disk), (c) inference latency (ms/token on target hardware), (d) memory footprint (peak GPU MB), (e) throughput (tokens/sec). Compute the Pareto efficiency: is the student on the accuracy-vs-speed frontier?

# Decision rules

- Use temperature T=4 as default; tune in {2, 3, 4, 6} if quality is below target.
- Set alpha=0.5 (equal hard/soft loss) as baseline; increase alpha toward 0.7 for tasks where teacher soft labels are high quality.
- Prefer layer-dropping initialization (copy every other layer) over random initialization for student models.
- If student achieves <90% of teacher accuracy, add feature-based distillation before increasing student size.
- Structured pruning combined with distillation outperforms either alone; always distill after pruning.
- Distillation data should be ≥10x the fine-tuning dataset size when using teacher-generated labels.

# Output requirements

1. `Architecture spec` — teacher and student configs, parameter counts, compression ratio
2. `Loss config` — distillation loss formulation, temperature, alpha, feature-matching layers and weights
3. `Training config` — data sources, LR schedule, epochs, batch size, hardware
4. `Evaluation report` — accuracy/F1 comparison, latency benchmarks, memory usage, Pareto analysis
5. `Pruning report` (if applicable) — heads/layers/neurons removed, importance scores, recovery fine-tuning results

# References

- Hinton et al. "Distilling the Knowledge in a Neural Network" — foundational KD paper
- DistilBERT: Sanh et al. "DistilBERT, a distilled version of BERT" — layer-dropping + distillation
- TinyBERT: Jiao et al. "TinyBERT: Distilling BERT for Natural Language Understanding" — feature-based
- MiniLM: Wang et al. "MiniLM: Deep Self-Attention Distillation" — attention transfer
- Michel et al. "Are Sixteen Heads Really Better than One?" — attention head pruning
- PyTorch: `torch.nn.KLDivLoss`, `torch.nn.functional.kl_div`

# Related skills

- `quantization-research` — further compression via INT8/INT4 after distillation
- `inference-kernel-optimization` — optimizing inference for the compressed model
- `pretraining-pipeline` — training the teacher model that distillation starts from
- `dense-to-moe-experiments` — alternative approach to scaling efficiency via sparsity

# Failure handling

- If student accuracy is <85% of teacher after standard distillation, try: increasing temperature, adding feature matching, or using a larger student architecture.
- If distillation loss plateaus but task metrics are poor, the student may lack capacity — increase hidden dim or layer count by one step.
- If pruning degrades quality >5%, reduce pruning aggressiveness (remove fewer heads/layers) and extend recovery fine-tuning.
- If latency improvement is <1.5x despite significant parameter reduction, the bottleneck may be memory-bandwidth — consider combining with quantization.
