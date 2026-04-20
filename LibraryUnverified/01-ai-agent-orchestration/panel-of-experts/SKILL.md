---
name: panel-of-experts
description: >
  Runs multiple expert agents in parallel on the same problem, normalizes their outputs,
  detects consensus or disagreement, and synthesizes a final recommendation.
  Trigger — "get multiple opinions", "panel of experts", "expert consensus", "compare approaches",
  "run experts in parallel", "multi-perspective review", "which approach is best",
  "synthesize expert outputs".
  Skip — trivial decisions with one obvious answer, tasks where a single specialist suffices,
  real-time latency-critical paths that cannot afford parallel fan-out.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: panel-of-experts
  maturity: draft
  risk: low
  tags: [panel, experts, consensus, synthesis, parallel]
---

# Purpose

When a problem benefits from multiple perspectives — architecture decisions, code review,
risk assessment, debugging — launch N expert agents in parallel with differentiated prompts,
collect their independent outputs, detect where they agree and disagree, and synthesize a
single actionable recommendation with confidence levels.

# When to use

- A decision has high stakes and benefits from independent expert review (architecture, security, design).
- The team wants to compare multiple implementation approaches before committing to one.
- A code review or audit should cover different concerns (correctness, performance, security) in parallel.
- The problem is ambiguous and you need to surface disagreement rather than follow one opinion.
- A previous single-agent attempt produced a low-confidence or controversial result.

# Do NOT use when

- The task has a single correct answer that one specialist can produce (e.g., fix a syntax error).
- Latency budget is too tight for parallel fan-out and synthesis (under 10 seconds total).
- Fewer than 2 meaningfully different expert perspectives exist for the problem.
- The task is execution, not analysis — building code rather than evaluating options.

# Operating procedure

1. **Define the problem statement.** Write a clear, self-contained problem description that
   each expert will receive. Include all relevant context, constraints, and evaluation criteria.
2. **Select the expert panel.** Choose 2-5 experts based on the problem domain. For each expert,
   write a one-line role description. Output as: `| Expert | Specialty | Perspective Focus |`.
   Examples: security-auditor, performance-engineer, maintainability-reviewer, domain-specialist.
3. **Differentiate the prompts.** For each expert, craft a prompt that shares the same problem
   statement but adds role-specific instructions. Each prompt must explicitly say:
   "Focus on [X]. Do not attempt to cover all aspects — other experts handle those."
4. **Launch experts in parallel.** Dispatch all expert prompts simultaneously. Set a uniform
   timeout (default: 120 seconds). Record launch timestamps.
5. **Collect and normalize outputs.** When each expert returns, extract:
   - Their recommendation (a single concrete action or choice).
   - Their confidence level (HIGH / MEDIUM / LOW).
   - Their top 3 supporting arguments.
   - Any risks or caveats they identified.
   Normalize into a uniform table: `| Expert | Recommendation | Confidence | Key Arguments | Risks |`.
6. **Detect consensus.** Count how many experts agree on the same recommendation. Apply rules:
   - All agree → STRONG_CONSENSUS.
   - Majority agrees → MAJORITY_CONSENSUS.
   - No majority → DISAGREEMENT.
7. **Resolve disagreements.** If DISAGREEMENT, identify the specific axes of conflict.
   List each contested point with the arguments for and against from each expert.
   If possible, run a brief tiebreaker prompt that presents only the contested points.
8. **Synthesize the final recommendation.** Write a single recommendation that:
   - States the chosen action and why.
   - Incorporates the strongest risk mitigations from dissenting experts.
   - Assigns a final confidence level based on consensus strength.
9. **Document the panel record.** Preserve the full expert outputs, the synthesis reasoning,
   and the final recommendation for audit purposes.

# Decision rules

- Never average opinions — pick a concrete recommendation and defend it with evidence.
- Dissenting expert views must be acknowledged, not silently discarded.
- If all experts are LOW confidence, escalate to a human rather than synthesizing a weak answer.
- Prefer the recommendation with the fewest unmitigated high-severity risks.
- Weight domain-specialist opinions higher than generalist opinions on domain-specific questions.

# Output requirements

1. **Panel Composition** — table of experts, their specialties, and perspective focus.
2. **Expert Outputs** — normalized table with recommendations, confidence, arguments, and risks.
3. **Consensus Assessment** — STRONG_CONSENSUS, MAJORITY_CONSENSUS, or DISAGREEMENT with details.
4. **Synthesis** — final recommendation with rationale and incorporated risk mitigations.
5. **Dissent Record** — any dissenting views and why they were not adopted.

# References

- `references/delegate-contracts.md` — prompt templates for expert agents.
- `references/checkpoint-rules.md` — when to checkpoint before acting on a panel recommendation.
- `references/failure-escalation.md` — escalation triggers when consensus is unachievable.

# Related skills

- `parallel-lane-safety` — for ensuring expert agents do not interfere with each other.
- `manager-hierarchy-design` — for structuring teams when experts become ongoing workers.
- `verification-before-advance` — for validating the synthesized recommendation before execution.
- `long-run-watchdog` — for monitoring expert agents that take too long.

# Failure handling

- If an expert times out, proceed with the remaining experts but note reduced panel size
  in the synthesis and lower the overall confidence by one level.
- If all experts produce identical boilerplate, the prompts lacked differentiation — rewrite
  the role-specific instructions with sharper focus areas and re-run.
- If the synthesis contradicts all expert recommendations, discard the synthesis and present
  the raw expert outputs to the user for manual decision.
- If fewer than 2 experts return usable output, abort the panel and fall back to a single
  best-qualified agent with an enriched prompt.
