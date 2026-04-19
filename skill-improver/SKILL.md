---
name: skill-improver
description: >-
  Improve an existing skill package — tighten routing, sharpen procedure, add or
  prune support layers, upgrade packaging. Use when the user says "improve this
  skill", "this skill is weak/vague/bloated", "harden this SKILL.md", or "add
  evals/references to this skill package". Do not use for creating a new skill
  from scratch (use skill-creator), trigger-only fixes when the body is fine
  (use skill-trigger-optimization), porting a skill to a different stack or
  context (use skill-adaptation), or quick structural audits with no rewrite
  (use skill-anti-patterns).
---

# Purpose

Improve an existing skill package. A skill is a reusable operating manual for an agent. Improving one means improving four things together:

1. **Routing** — whether the right tasks activate it.
2. **Execution** — whether the body gives concrete, repeatable procedure.
3. **Support layers** — whether references, scripts, and evals exist where they help.
4. **Maintainability** — whether the skill stays narrow and worth activating.

Preserve the skill's core purpose unless the user explicitly asks to reposition it.

# When to use

Use when:

- the user provides a SKILL.md and wants it improved,
- the user says a skill feels weak, vague, generic, bloated, or under-specified,
- the user wants better triggering, structure, examples, or supporting files,
- the user wants to know whether a skill needs references/, scripts/, or evals/,
- the user wants a thin prompt upgraded into a durable skill package.

# When NOT to use

- Creating a brand-new skill from scratch — use **skill-creator**
- The problem is only the description/trigger and the body is fine — use **skill-trigger-optimization**
- Porting or adapting a skill to a different stack or context — use **skill-adaptation**
- Running a quick structural audit with no rewrite planned — use **skill-anti-patterns**
- The task is a repo review, architecture review, or product planning exercise

# Procedure

## Improvement modes

Choose the lightest mode that solves the real problem.

### Mode selection guide

Choose Mode 1 (Surgical edit) when:
- Specific failure report with reproduction steps
- Single section identified as the problem
- Skill works correctly for most cases
- Fix is isolated (changing one step, tightening one description, adding one failure case)

Choose Mode 2 (Structural refactor) when:
- Multiple anti-patterns detected (3+ from skill-anti-patterns scan)
- Section ordering contradicts logical flow
- Content is scattered across wrong sections (procedure content in Purpose, routing content in Procedure)
- Skill grew organically and needs reorganization

Choose Mode 3 (Package upgrade) when:
- Skill needs new resources (references, scripts, evals) that don't exist yet
- Skill exists as SKILL.md only but complexity justifies support files
- Existing support files are outdated or broken

When in doubt: Start with Mode 1. If Mode 1 changes touch >30% of the file, switch to Mode 2.

### Mode 1 — Surgical edit

Use when the skill is broadly sound and mainly needs better trigger wording, clearer steps, stronger output contract, or removal of fluff.

Output: updated SKILL.md, concise change summary, optional eval refresh.

Change summary template:

```
**Skill**: [name]
**Failure mode**: [category from Phase 2 table]
**Evidence**: [what went wrong]

### Change
**Section**: [which section changed]
**Before**: [original text]
**After**: [new text]
**Rationale**: [why this fixes the failure mode]

### Verification
- [ ] Existing evals still pass
- [ ] Known good cases still work
- [ ] Failure case now handled
```

### Mode 2 — Structural refactor

Use when the skill has the right goal but poor execution: vague description, no phases, no decision rules, no failure handling, too much in one file.

Output: rewritten SKILL.md, new references/ if justified, eval stubs or updated evals.

### Mode 3 — Package upgrade

Use when the skill should become a first-class reusable package: shared across projects, needs baseline comparisons, needs scripts for repeated mechanics.

Output: improved SKILL.md, evals, references, scripts only where deterministic.

Follow these phases in order unless the user clearly wants a lighter pass.

## Phase 1 — Understand the current skill

Identify:

- the skill's current purpose and intended user problem,
- the current activation boundary,
- the main failure mode,
- whether the skill is draft, project-local, or intended for reuse.

Read the existing skill package before editing. If the conversation provides enough context, extract it instead of re-asking.

**Check for eval results.** Look for `eval-results/<skill-name>-eval.md` (symlink to latest run). If it exists, read it and extract:

- Gate pass/fail status for each gate (precision, recall, behavior, structural, usefulness)
- Trigger precision and recall percentages
- Behavior pass rate and any failing cases
- Usefulness scores and judge rationale (if present)
- List of specific prompts that failed, with the reason (misrouted, wrong output, low usefulness)

These quantitative signals become the primary input for Phase 2. If no eval results exist, proceed with the heuristic diagnosis below.

## Phase 2 — Diagnose the weakness

Name the primary failure mode before rewriting.

### Eval-driven diagnosis (preferred)

When eval results from Phase 1 are available, use them as primary evidence:

| Eval signal | → Failure mode | Primary fix target |
|---|---|---|
| Trigger precision < 80% | overtriggering | tighten description, add "do not use" boundaries |
| Trigger recall < 80% | undertriggering | rewrite description with concrete trigger phrases |
| Behavior pass rate < 80% | wrong output format or missing edge case | fix output contract, add missing procedure steps |
| Usefulness score < 3/5 | prompt-blob syndrome or missing branching | replace prose with concrete steps, add decision rules |
| Structural score < 8/10 | package rot | fix section ordering, add missing sections |
| Multiple gates failing | compound failure | prioritize routing fixes first, then output quality |

Cross-reference eval failures with the specific failing prompts to identify the root cause. A skill that fails on edge-case prompts needs different fixes than one that fails on core cases.

### Heuristic diagnosis (fallback)

When no eval results are available, map the reported or observed failure to a fix target:

| Failure mode | Primary fix target |
|---|---|
| undertriggering | rewrite description with concrete trigger phrases |
| overtriggering | tighten description, add "do not use" boundaries |
| prompt-blob syndrome | replace prose with concrete procedural steps |
| missing branching | add decision rules for common variants |
| wrong output format | fix output contract or output defaults |
| missing edge case | add case to workflow or operating procedure |
| scope creep | add constraints, narrow boundaries |
| bloat | remove unnecessary steps or content |
| resource mismatch | add or remove references/scripts as evidence warrants |
| no proof | add eval stubs or test prompts |
| package rot | update docs, add evals, review support layers |

Score informally against: routing quality, procedural clarity, decision support, support-layer quality, evaluation readiness, maintainability. See `references/skill-quality-rubric.md` for the detailed rubric.

## Phase 3 — Decide what to improve

Improve in this priority order:

1. Clarify purpose.
2. Tighten routing description.
3. Add or improve workflow phases.
4. Add decision rules for common branches.
5. Add output contract and failure handling.
6. Decide whether references, scripts, or evals are warranted.

For support-layer decisions, see `references/resource-decision-guide.md`.

For skill-type-specific improvement strategies, see `references/skill-type-playbooks.md`.

## Phase 4 — Rewrite with restraint

Prefer concrete decisions over generic advice. Explain *why* where that helps the agent adapt. Do not inflate the skill with policy text that belongs elsewhere.

If the skill spans multiple use cases, add explicit phases, a decision table, or separate mode sections — not one flat list.

## Phase 5 — Add support layers only if justified

Add references when optional depth would clutter SKILL.md or the skill covers multiple variants. When extracting large reference blocks from a bloated SKILL.md, follow the classification heuristic and procedure in `references/extraction-guide.md`.

Add scripts when a deterministic task recurs and a script reduces context waste.

Add evals when routing quality matters or the user wants evidence the revision helped.

Do not add scripts or references merely to make the package feel complete.

## Phase 6 — Self-review

Before presenting the result, verify:

- Description clearly states what the skill does and when it triggers.
- "Do not use" boundaries exist where confusion with adjacent skills is likely.
- Workflow is ordered and branch-aware.
- At least one stop condition, quality gate, or failure path exists.
- Support files are justified rather than ornamental.
- Original purpose is preserved.

If any check fails, revise.

## Phase 7 — Package the result

Before packaging, compare the improved skill against the original to verify quality gates pass:

```bash
./scripts/run-baseline-comparison.sh <original-skill.md> <modified-skill.md>
```

This checks structural score delta, section preservation, name preservation, line limits, and eval regression. If gates fail, revise before delivering.

Return:

- the improved files,
- a short rationale for each major change,
- 2–5 recommended eval prompts for the improved skill,
- risks or open questions, only if something could not be settled cleanly.

# Output contract

Every improvement produces:

1. **Updated skill files** — the modified SKILL.md and any new/changed support files
2. **Change summary** — for each change: section affected, before text, after text, rationale
3. **Eval prompts** — 2–5 recommended test prompts to verify the improvement
4. **Next steps** — specific follow-up actions (e.g., "run skill-evaluation to verify routing")

```
## Improvement Report: [skill-name]

### Mode: [Surgical edit | Structural refactor | Package upgrade]
### Primary failure mode: [from Phase 2 table]

### Changes
| Section | Before | After | Rationale |
|---------|--------|-------|-----------|
| ... | ... | ... | ... |

### Files modified
- [list of files changed/added/removed]

### Recommended eval prompts
1. [prompt that should trigger the skill]
2. [prompt that should NOT trigger]
3. [edge case prompt]

### Next steps
- Run `skill-evaluation` to verify routing accuracy
- Run `skill-testing-harness` if no evals/ directory exists
```

# Failure handling

## Anti-patterns

Avoid these when improving a skill:

1. **Placeholder upgrades** — replacing one vague paragraph with a longer vague paragraph.
2. **Monolith inflation** — stuffing every idea into SKILL.md instead of using references/.
3. **Tool cosplay** — adding scripts that don't solve a repeated mechanical problem.
4. **Silent repurposing** — changing what the skill is for without telling the user.
5. **Unverifiable improvement** — claiming the skill is better with no eval prompts or review path.

For a full structural anti-pattern catalog, use **skill-anti-patterns**.

## Incomplete skill

If the skill is too incomplete to improve cleanly (missing description, no procedure, stub-only):

1. Name what is missing.
2. Preserve what can be salvaged.
3. Produce the lightest viable improved draft.
4. Mark assumptions explicitly.

## Quick pass requested

If the user wants a quick pass rather than a full package upgrade, do that. The point is to improve the skill, not force ceremony. Skip support-layer generation and focus on the SKILL.md body only.

## Scope change mid-improvement

If analysis reveals the skill's purpose should fundamentally change (e.g., it should be split, merged, or retired):

1. Stop the improvement.
2. Document the finding: what the skill currently does vs what it should do.
3. Recommend the appropriate next action: `skill-variant-splitting` for splits, `skill-catalog-curation` for merges, `skill-lifecycle-management` for retirement.
4. Do not rewrite a skill into something it was never meant to be.

## Contradictory requirements

If the SKILL.md contains instructions that contradict each other (e.g., description says "never modify files" but procedure step 3 says "write the output file"):

1. List each contradiction with line references.
2. Ask the user which intent is correct before rewriting.
3. If no user is available, favor the description (it is the contract) and flag the procedure steps for review.

## Missing support files

If the skill references files that do not exist (phantom references):

1. List each missing reference with the line that references it.
2. Decide per reference: create the file with reasonable content, or remove the reference.
3. Default to removing the reference unless the content is essential to the procedure.

## Circular or conflicting cross-references

If the skill's cross-references create loops (A→B→A) or point to skills that contradict this skill's purpose:

1. Map the reference chain.
2. Remove circular links — keep only the outbound reference that adds value.
3. Flag conflicting cross-references for manual review.

# Next steps

After improving a skill:
- Verify the improvement → `skill-evaluation`
- Build test infrastructure if none exists → `skill-testing-harness`
- Optimize triggers if routing changed → `skill-trigger-optimization`
