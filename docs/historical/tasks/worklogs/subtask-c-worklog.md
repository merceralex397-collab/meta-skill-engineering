# Sub Task C Worklog — Merged Review

## Date
2026-03-19

## Objective
Review both meta-skill-engineering reviews, merge into one actionable review reflecting the new repo state after Sub Task B consolidation.

## Process

1. Read the full independent review (499 lines) — covers all 16 original skills individually plus ecosystem-level findings
2. Read the full rewrite plan (675 lines) — covers package-grade standards and file-by-file rewrite targets
3. Filtered both through current state: 4 merges already completed, license/compatibility removed
4. Identified which findings are resolved and which remain
5. Merged remaining findings into priority-ordered actionable list

## Key decisions

### Resolved by consolidation (removed from merged review)
- Provenance pair confusion → now dual-mode skill-provenance
- Eval-runner duplication → now suite mode in skill-evaluation
- Description optimizer subsumption → removed entirely
- Overlay generator subsumption → merged into skill-packaging

### Kept as critical
- Self-consistency failures in skill-trigger-optimization and skill-improver — both teach rules they don't follow
- Broken description promise in skill-testing-harness (promises baseline comparisons, doesn't deliver)

### Kept as high priority
- Pipeline/entry-point workflow — no skill describes the create→test→evaluate→benchmark→package sequence
- Underspecified procedures in safety-review, evaluation, variant-splitting
- Missing anti-patterns in skill-anti-patterns catalog
- SKILL.md structure normalization needed across all skills

### Deprioritized from rewrite plan
- manifest.yaml requirement — internal-only skills don't need machine-readable metadata
- CHANGELOG.md requirement — not needed for internal use
- README.md per skill — SKILL.md is sufficient for internal use

## Output
Created `tasks/merged-actionable-review.md` — single actionable document with 11 prioritized action items, evidence sources, and affected skills listed.
