---
name: steelman
description: Rebuild a proposal, argument, or design in its strongest plausible form before evaluating it. Use when the user says "steelman this", "make the best case for this", "what's the strongest version of this argument", "assume the author is right—what would that look like", or when a proposal has been dismissed too quickly and deserves charitable reconstruction. Do not use for adversarial attack (use red-team-challenge), evidence-quality review (use skeptic-pass), or risk enumeration (use premortem).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: steelman
  maturity: draft
  risk: low
  tags: [steelman, argumentation, critique, reasoning]
---

# Purpose

Take a proposal, argument, or design—especially one that seems weak, unpopular, or poorly presented—and reconstruct it in the strongest form the author could plausibly have intended. Then evaluate that strongest version. If even the steelmanned version fails, the idea truly fails. If the steelmanned version succeeds, the original was dismissed unfairly.

This is the opposite of a straw man. Instead of weakening the argument to defeat it easily, you strengthen it to test it fairly.

This differs from related skills:
- **red-team-challenge** tries to break the proposal. Steelman tries to make it as strong as possible first.
- **skeptic-pass** demands evidence for claims. Steelman fills in the evidence the author might have cited.
- **plan-review** judges execution readiness. Steelman judges whether the core idea has merit.

# When to use this skill

Use when:
- the user says "steelman this", "make the best case", "strongest version of this argument"
- a proposal was rejected quickly and the user wants to check whether the rejection was fair
- a team debate has polarized and one side's argument needs charitable reconstruction
- an alternative that lost a tradeoff analysis deserves a second look
- the user's own idea needs strengthening before presenting it to stakeholders
- during consensus-building, to ensure dissenting views are heard in their strongest form (RFC 7282)

Do NOT use when:
- the goal is to attack or exploit the proposal (use `red-team-challenge`)
- the goal is to check evidence quality (use `skeptic-pass`)
- the goal is to judge execution feasibility (use `plan-review`)
- the proposal is already well-articulated and doesn't need reconstruction

# Operating procedure

1. **State the original argument as received.**
   Quote or summarize the proposal exactly as presented, including any weaknesses in presentation. Do not editorialize yet.

2. **Identify the core thesis.**
   Strip away presentation problems—poor phrasing, missing context, emotional language, tangents—and isolate the central claim or proposal in one sentence:
   "The core argument is: [X] because [Y], which would achieve [Z]."

3. **List what the author got right.**
   Before improving anything, acknowledge the valid observations, correct premises, or genuine insights in the original. This prevents the steelman from being a complete replacement rather than an enhancement.

4. **Fill in the missing support.**
   For each weak or unsupported point in the original, ask: "What evidence, reasoning, or context could the author have provided that would make this point strong?" Add:
   - Data or benchmarks that would support the claims
   - Prior art or precedent that validates the approach
   - Constraints or context that explain seemingly odd choices
   - Edge cases the author likely considered but didn't mention
   - Counterarguments the author would have anticipated

5. **Reconstruct the strongest version.**
   Rewrite the proposal as if the most competent advocate were presenting it. The steelmanned version must:
   - Be plausible—don't invent support that probably doesn't exist
   - Preserve the author's intent—don't change the proposal into something different
   - Address the known objections—incorporate the strongest responses to common criticisms
   - Be specific—replace vague claims with concrete ones where possible

6. **Now evaluate the steelmanned version.**
   Apply honest judgment to the strongest version:
   - Does the core thesis hold when presented at its best?
   - What are the remaining weaknesses that even the best version cannot fix?
   - What conditions would need to be true for this to succeed?
   - Is the steelmanned version worth pursuing, modifying, or setting aside?

7. **Deliver the verdict.**
   One of:
   - **Strong after steelman**: The idea has genuine merit that was obscured by poor presentation. Recommend proceeding with the steelmanned version.
   - **Conditional after steelman**: The idea has merit under specific conditions. Name the conditions.
   - **Weak even after steelman**: Even the strongest version has fundamental problems. Name the specific reasons it fails.

# Output contract

Return a **Steelman Report** with:

1. `Original Argument` — the proposal as received (brief summary or quote)
2. `Core Thesis` — one-sentence distillation
3. `What the Author Got Right` — valid points in the original
4. `Steelmanned Version` — the proposal rebuilt in its strongest form
5. `Evaluation of Strongest Version` — honest assessment of the steelman
6. `Remaining Weaknesses` — problems that survive even the strongest reconstruction
7. `Verdict` — Strong / Conditional / Weak, with justification

# Named failure modes of this method

- **Substitution**: Replacing the author's actual argument with a different (better) argument. Fix: the steelman must preserve the author's core thesis, not invent a new one.
- **Overclaiming**: Inventing support that probably doesn't exist to make the argument look stronger than it could plausibly be. Fix: every added support must pass a plausibility check—would a well-informed advocate actually have this evidence?
- **Skipping the evaluation**: Doing all the reconstruction work but then failing to honestly evaluate the result. Fix: the steelman is a means to fair evaluation, not an end in itself.
- **False charity**: Steelmanning a genuinely bad idea to avoid conflict. Fix: if the strongest version still fails, say so clearly.
- **Losing the author**: Reconstructing so aggressively that the author wouldn't recognize their own argument. Fix: check that the core thesis is preserved.

# References

- RFC 7282 (https://www.rfc-editor.org/rfc/rfc7282.html) — ensuring dissenting positions are heard in their strongest form before consensus
- Rapoport's Rules — Dennett, D. (2013). Intuition Pumps: how to compose a successful critical commentary (re-express, list agreements, mention what you learned, only then criticize)
- Mill, J.S. (1859). On Liberty — "He who knows only his own side of the case knows little of that"
- Principle of charity in argumentation theory

# Failure handling

- If the original argument is too vague to have a core thesis, ask for clarification rather than inventing one.
- If the argument is already strong, say so—do not artificially weaken it just to have something to steelman.
- If the argument is on a topic where you lack domain knowledge to fill in plausible support, state the limitation and provide the structural steelman without fabricated domain evidence.
