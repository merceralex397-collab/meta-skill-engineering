---
name: acceptance-criteria-hardening
description: Turn vague success language in tickets, stories, specs, PRDs, and implementation notes into observable, testable acceptance criteria. Use when the user asks to "write acceptance criteria", "make this testable", "tighten definition of done", "turn this story into checks", or when a task looks executable but success is still fuzzy. Do not use for full plan critique, option comparison, or generic code review.
---

# Purpose

Use this skill to convert soft success language into hard completion checks.

Good acceptance criteria define observable outcomes, boundaries, and proof. They do not hide implementation detail as "done", and they do not rely on taste words like "smooth", "intuitive", or "works well".

# When to use this skill

Use this skill when:

- a ticket or story says what to build but not how completion will be proven
- the user explicitly asks for acceptance criteria or definition-of-done help
- criteria are written as implementation tasks instead of user-visible outcomes
- criteria exist but ignore failure paths, empty states, or permission boundaries

Strong trigger phrases:

- "write acceptance criteria"
- "make this story testable"
- "tighten definition of done"
- "turn this into measurable checks"
- "what should success look like"

# Do not use this skill when

- the main problem is whether the overall plan is safe or sequenced correctly; use plan review
- the main problem is whether hidden assumptions are credible; use assumptions audit
- the task is comparing approaches before any requirements are fixed; use tradeoff analysis

# Inputs expected

Prefer to start from:

- a story or ticket
- a PRD or spec section
- a list of draft criteria
- a "done means..." paragraph

If the input mixes requirements, implementation notes, and criteria, separate them first.

# Operating procedure

1. Extract the real outcome.
   Identify actor, context, system behavior, and observable result. If the criterion says only "implement X", rewrite it around the effect users or operators will see.

2. Split overloaded bullets.
   If one bullet contains several assertions joined by "and", "or", or "including", split it into smaller checks unless those parts are inseparable.

3. Replace vague language.
   Remove terms like "fast", "robust", "user-friendly", "complete", and "works correctly" unless they are defined by a metric or observable behavior.

4. Add boundary coverage.
   Check whether the criteria cover at least the happy path plus the relevant boundary cases:
   - empty or missing data
   - invalid input
   - permission or auth failures
   - retries, duplicates, or concurrency
   - partial outage or third-party failure

5. Attach proof.
   For each criterion, name the proof source when possible: unit test, integration test, end-to-end test, screenshot, query result, log line, dashboard signal, or manual verification step.

6. Separate requirement from implementation.
   Move framework choices, library names, and internal code tasks into notes unless they are themselves externally required.

7. Return a hardened set.
   Group the result into core outcomes, boundary conditions, and open questions if the source material is incomplete.

# Decision rules

- Prefer one criterion per observable outcome.
- Prefer actor-centric wording when the user experience matters.
- Prefer system-centric wording when the requirement is operational or API-facing.
- Do not invent target numbers for latency, uptime, or throughput unless the source material or domain context supports them.
- When a criterion depends on another team, external API, or future migration, call that out as an assumption or prerequisite instead of hiding it in the criterion.
- If proof cannot be specified yet, say what evidence class is still needed rather than pretending the criterion is complete.

# Output requirements

Return these sections:

1. `Hardened Criteria`
2. `Boundary Cases Added`
3. `Proof Sources`
4. `Open Questions`

Keep the final criteria concise enough to paste into a ticket.

# Scripts

Use `scripts/ac_lint.py` when you receive a long raw list of criteria and need a quick pass over weak phrasing.

Example:

```bash
python scripts/ac_lint.py --input story.md --format text
```

The script flags weak language and overloaded bullets. It does not rewrite the criteria for you.

# References

- Read `references/weak-language-catalog.md` when the source text feels hand-wavy.
- Read `references/criterion-patterns.md` to choose a pattern for UI, API, job, or migration criteria.
- Read `references/boundary-cases.md` when the draft criteria are happy-path only.
- BDD / Given-When-Then (https://dannorth.net/introducing-bdd/) — use this pattern when criteria describe user-facing behavior.

# Failure handling

- If the source text does not state the intended outcome, stop and ask for the missing behavior instead of fabricating it.
- If implementation notes are the only input, say that acceptance criteria cannot be derived confidently without user-visible or operator-visible outcomes.
- If the domain is safety-critical, regulated, or performance-sensitive, mark any missing measurable thresholds as blockers.

# Named failure modes of this method

- **Implementation criteria masquerading as acceptance**: Writing "use React" or "add an index" as acceptance criteria when these are implementation choices, not observable outcomes. Fix: every criterion must describe what a user, operator, or test can observe.
- **Taste-word survival**: Replacing one vague word with another ("fast" → "performant") instead of defining the actual threshold. Fix: if you cannot name a number or observable behavior, the criterion is still vague.
- **Happy-path-only hardening**: Tightening the success criteria while ignoring error paths, empty states, and permission boundaries. Fix: always run the boundary coverage check (step 4).
- **Over-specification**: Turning 3 reasonable criteria into 30 micro-criteria that nobody will read or test. Fix: each criterion should be independently verifiable and worth testing on its own.
- **Proof-free criteria**: Hardening the language but not attaching proof sources—the criterion reads well but nobody knows how to verify it. Fix: every criterion needs a named proof source (step 5).
