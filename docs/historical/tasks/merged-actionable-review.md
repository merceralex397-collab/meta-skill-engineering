# Merged Actionable Review — Post-Consolidation State

## Basis

This review merges findings from two sources:
1. **Independent review** (`meta-skill-engineering-review-independent.md`) — evaluation using external LLM behavior knowledge and the skill-creator reference
2. **Rewrite plan** (`meta-skill-engineering-rewrite-plan.md`) — package-grade standard and file-by-file rewrite targets

Both are filtered through the current repo state after Sub Task B consolidation (20 skills, 4 merges completed).

## Current inventory (20 skills)

community-skill-harvester, skill-adaptation, skill-anti-patterns, skill-benchmarking, skill-catalog-curation, skill-creator, skill-deprecation-manager, skill-evaluation, skill-improver, skill-installer, skill-lifecycle-management, skill-packager, skill-packaging, skill-provenance, skill-reference-extraction, skill-registry-manager, skill-safety-review, skill-testing-harness, skill-trigger-optimization, skill-variant-splitting

## Findings resolved by consolidation

These issues from the original reviews are now addressed:
- ✅ Provenance duplicate pair → merged into skill-provenance (dual-mode)
- ✅ Eval-runner/evaluation duplicate → merged into skill-evaluation (suite mode)
- ✅ Description-optimizer subsumed → removed, skill-trigger-optimization covers all
- ✅ Overlay-generator subsumed → merged into skill-packaging
- ✅ License/compatibility metadata removed (internal-only)

## Remaining critical actions

### 1. Fix self-consistency failures [Critical]
**Source**: Independent review, ecosystem findings

These skills fail to demonstrate the qualities they teach:

- **skill-trigger-optimization**: Description opens with technical language ("Rewrite a skill's description field and 'When to use' triggers to fix routing precision and recall") but its own procedure says descriptions should start with user-spoken trigger phrases. Rewrite using its own Step 4 rules.
- **skill-improver**: Teaches that skills need dedicated output contract sections but does not have one. Extract output contract from Phase 7 into a dedicated section.

### 2. Fix broken description promises [Critical]
**Source**: Independent review

- **skill-testing-harness**: Description promises "baseline comparisons" as an output but the output contract only produces trigger-positive.jsonl, trigger-negative.jsonl, output-tests.jsonl, and README.md. Either add baseline-cases.jsonl or remove "baseline comparisons" from description.

### 3. Add pipeline workflow / entry points [High]
**Source**: Independent review ecosystem findings, rewrite plan

No skill describes the natural workflow sequence. A new user doesn't know the order. Create entry-point guidance connecting:

1. **Create**: skill-creator
2. **Test**: skill-testing-harness
3. **Evaluate**: skill-evaluation
4. **Benchmark** (if variants): skill-benchmarking
5. **Optimize triggers**: skill-trigger-optimization
6. **Review safety**: skill-safety-review
7. **Record provenance**: skill-provenance
8. **Package**: skill-packaging
9. **Manage lifecycle**: skill-lifecycle-management

Each skill should have a "Next step" pointer at the end.

### 4. Fix underspecified procedures [High]
**Source**: Independent review

- **skill-safety-review Step 4** (injection): Says "identify paths where untrusted external content flows into instruction context" but gives no concrete detection procedure. Needs scan instructions matching the depth of other steps.
- **skill-evaluation Step 5** (baseline): "Run without the skill" is underspecified. Add: "Remove/disable the skill's SKILL.md from the agent client skill directory, run the same prompts, then re-enable."
- **skill-variant-splitting Step 4**: Offers two options (base skill with extensions vs fully independent variants) without selection criteria. Add threshold: "If shared content >30% of variant length, use base skill; otherwise independent."

### 5. Expand anti-pattern catalog [High]
**Source**: Independent review

skill-anti-patterns is missing four important anti-patterns:
- **Instruction overload**: Skills >400 lines degrade instruction following (middle content gets less attention)
- **Capability assumption**: Steps assume tools the agent may not have (subagents, browsers, specific CLIs)
- **Few-shot starvation**: Output format described in prose but never exemplified with a filled example
- **Hallucination surface**: Vague success criteria ("ensure quality") invite the agent to invent definitions

### 6. Normalize SKILL.md structure [High]
**Source**: Rewrite plan

All skills should use this section order:
1. Purpose
2. When to use / When NOT to use
3. Procedure
4. Output contract
5. Failure handling
6. References (optional)

Some skills currently deviate. Normalize during execution.

### 7. Fix cross-skill reference errors [Medium]
**Source**: Independent review, own analysis

- **skill-testing-harness**: Failure handling says "merge via skill-variant-splitting" — splitting is the opposite of merging. Correct to skill-catalog-curation.
- **skill-trigger-optimization**: "Host configuration" is undefined jargon. Change to "Escalate to skill-catalog-curation."
- **skill-variant-splitting**: No next-step pointer to skill-catalog-curation for updating the library index after a split.

### 8. Add missing user-spoken trigger phrases [Medium]
**Source**: Independent review

- **skill-variant-splitting**: Description uses structural observation signals, not user phrases. Add: "this skill does too much", "split this skill", "create variants"
- **skill-lifecycle-management**: Description opens with three verbs. Simplify.

### 9. Missing merge capability [Medium]
**Source**: Independent review

Three skills recommend merging but no skill describes how to merge. Options:
- Add merge procedure to skill-catalog-curation (it already detects duplicates)
- Or add merge guidance to skill-improver

### 10. Add provenance trigger points [Medium]
**Source**: Independent review

skill-provenance is isolated — no other skill references it. Add trigger points:
- skill-installer: "after importing from external source, run skill-provenance"
- skill-lifecycle-management: "before promoting to stable, verify provenance exists"

### 11. Improve trust-level criteria [Medium]
**Source**: Independent review

skill-provenance trust criteria are subjective ("known author", "peer-reviewed"). Need concrete thresholds: "High: git log shows named committer with ≥5 commits across ≥2 months, and passing eval exists."

## Items from rewrite plan that remain valid

The rewrite plan's package-shape standard (SKILL.md + evals/ + manifest.yaml for reusable skills) is sound but should be applied incrementally. Focus on:
- Adding evals/ seed files to skills that lack them
- Normalizing SKILL.md structure across all skills
- Adding "Next step" pointers to create workflow chains

The manifest.yaml and CHANGELOG.md requirements are deprioritized since these are internal-only skills.

## Execution priority

| Priority | Action | Skills affected |
|----------|--------|----------------|
| Critical | Fix self-consistency failures | skill-trigger-optimization, skill-improver |
| Critical | Fix broken description promise | skill-testing-harness |
| High | Add pipeline/entry-point workflow | All skills (next-step pointers) |
| High | Fix underspecified procedures | skill-safety-review, skill-evaluation, skill-variant-splitting |
| High | Expand anti-pattern catalog | skill-anti-patterns |
| High | Normalize SKILL.md structure | All skills |
| Medium | Fix cross-skill reference errors | skill-testing-harness, skill-trigger-optimization, skill-variant-splitting |
| Medium | Add user-spoken trigger phrases | skill-variant-splitting, skill-lifecycle-management |
| Medium | Add merge capability | skill-catalog-curation or skill-improver |
| Medium | Add provenance trigger points | skill-installer, skill-lifecycle-management |
| Medium | Improve trust-level criteria | skill-provenance |
