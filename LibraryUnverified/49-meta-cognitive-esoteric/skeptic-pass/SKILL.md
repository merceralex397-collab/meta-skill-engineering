---
name: skeptic-pass
description: Run a deliberately doubtful review that challenges every claim in a proposal, demands evidence for assertions, and flags optimism bias. Use when the user says "skeptic pass", "challenge these claims", "what's hand-wavy here", "where's the proof", "poke holes in the reasoning", or when a proposal reads too confidently for the evidence behind it. Do not use for adversarial exploitation (use red-team-challenge), hidden dependency discovery (use assumptions-audit), or structured option comparison (use tradeoff-analysis).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: skeptic-pass
  maturity: draft
  risk: low
  tags: [skeptic, critique, evidence, review]
---

# Purpose

Read a proposal as a demanding peer reviewer who needs convincing. Every claim is guilty until proven innocent. The goal is not to attack the plan (that is red-teaming) or to find hidden dependencies (that is assumptions audit)—the goal is to grade the evidence quality behind each assertion and flag where confidence outpaces proof.

This differs from related skills:
- **red-team-challenge** adopts an adversary's mindset and tries to exploit weaknesses. Skeptic-pass stays in reviewer mode and demands evidence.
- **assumptions-audit** extracts hidden dependencies. Skeptic-pass evaluates whether stated claims are actually supported.
- **steelman** rebuilds arguments in their strongest form. Skeptic-pass tests whether arguments hold up under scrutiny.

# When to use this skill

Use when:
- the user says "skeptic pass", "challenge these claims", "what's hand-wavy", "where's the proof"
- a proposal reads confidently but cites no evidence, benchmarks, or prior art
- a plan uses quantitative claims (latency targets, cost savings, adoption rates) without sourcing them
- a design document needs intellectual honesty review before stakeholder presentation
- after a steelman pass, to test whether even the strongest version holds up

Do NOT use when:
- the task is adversarial exploitation of a system (use `red-team-challenge`)
- the task is finding unstated dependencies (use `assumptions-audit`)
- the task is judging execution order and safety of a plan (use `plan-review`)
- the evidence is already known to be absent and the goal is just to list what's missing (use `gap-analysis`)

# Operating procedure

1. **Extract every claim.**
   Read the document and list every assertion that could be true or false. Include:
   - Factual claims ("Our API handles 10k req/s")
   - Causal claims ("This will reduce churn by 20%")
   - Feasibility claims ("This can be done in two sprints")
   - Comparative claims ("Option A is faster than Option B")
   - Consensus claims ("The team agrees that…", "Industry best practice is…")
   - Absence claims ("There is no risk of…", "This won't affect…")

2. **Grade the evidence behind each claim.**
   For each claim, assign one of:
   - **Proven**: Supported by cited data, benchmark, test result, or authoritative source
   - **Plausible**: Reasonable but unsupported—no evidence cited, just sounds right
   - **Asserted**: Stated as fact with no supporting argument at all
   - **Contradicted**: Available evidence suggests the opposite
   - **Unfalsifiable**: Stated in a way that cannot be tested or disproven ("this will be robust")

3. **Identify optimism bias patterns.**
   Flag these common patterns when present:
   - **Planning fallacy**: Timeline assumes everything goes right, no buffer for unknowns
   - **Anchoring**: A specific number is used without justification, and all reasoning anchors to it
   - **Survivorship bias**: "Company X did this successfully" without noting failures
   - **Appeal to novelty**: Preferring the new approach because it's new, not because evidence supports it
   - **Sunk cost**: Continuing a direction because of past investment, not future value
   - **Consensus illusion**: "Everyone agrees" when no explicit poll or decision was recorded
   - **Precision theater**: False precision (e.g., "this will take 14.5 days") masking deep uncertainty

4. **Stress-test the causal claims.**
   For every "X will cause Y" or "doing X will result in Y":
   - Is the mechanism explained or just asserted?
   - Are there confounding factors?
   - Has this causal chain been observed before, or is it hoped for?
   - What would disprove this claim?

5. **Challenge the numbers.**
   For every quantitative claim:
   - Where does this number come from?
   - Is it a measurement, estimate, or guess?
   - What's the confidence interval?
   - Is it cited or derived?
   - Does it survive a sanity check against known baselines?

6. **Produce a credibility assessment.**
   Grade the overall document:
   - **Well-supported**: Most claims are proven or plausible with clear reasoning
   - **Mixed**: Some claims are supported, others are pure assertion
   - **Weakly-supported**: Most claims are asserted without evidence
   - **Overconfident**: Confidence level far exceeds evidence quality

# Output contract

Return a **Skeptic Report** with:

1. `Claims Inventory` — numbered list of extracted claims
2. `Evidence Grades` — table: Claim | Grade (Proven/Plausible/Asserted/Contradicted/Unfalsifiable) | Evidence Cited | Evidence Needed
3. `Optimism Bias Flags` — specific patterns identified with the triggering text
4. `Weakest Claims` — the 3-5 claims with the largest gap between confidence and evidence
5. `Overall Credibility` — one of the four grades above, with a one-paragraph justification
6. `Evidence Collection Plan` — for the weakest claims, what specific evidence would resolve the doubt

# Named failure modes of this method

- **Cynicism spiral**: Doubting everything equally instead of prioritizing claims that matter. Fix: focus skepticism on claims that drive decisions, not background context.
- **Demanding impossible proof**: Requiring RCT-level evidence for reasonable engineering judgment. Fix: calibrate evidence expectations to the claim's impact—high-stakes claims need strong evidence, low-stakes claims need plausibility.
- **Missing the forest**: Grading individual claims without assessing whether the overall argument holds even if some claims are weak. Fix: always include the overall credibility assessment.
- **Style policing**: Flagging vague language when the underlying claim is actually well-supported elsewhere. Fix: check whether evidence exists before flagging weak phrasing.
- **Confirmation bias in review**: Only challenging claims you disagree with while accepting others at face value. Fix: grade every claim, including ones that feel obviously true.

# References

- Kahneman, D. (2011). Thinking, Fast and Slow — cognitive biases in judgment
- RFC 7282 (https://www.rfc-editor.org/rfc/rfc7282.html) — on consensus and objection handling in technical review
- Tetlock, P. (2015). Superforecasting — calibrating confidence to evidence
- Sagan, C. "Extraordinary claims require extraordinary evidence" — evidence proportionality principle

# Failure handling

- If the document is too short to contain meaningful claims, say so rather than manufacturing doubt.
- If all claims are well-supported, report that the skeptic pass found high credibility—do not force criticism.
- If the document is a brainstorm or early draft, adjust evidence expectations downward and note that a full skeptic pass requires a more developed proposal.
