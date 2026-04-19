# Sub Task B — Consolidation Plan

## Evidence sources

1. **Repo skills**: Direct examination of all 24 SKILL.md files
2. **Official sources**: Agent Skills spec (agentskills.io), best practices research
3. **Repo reports**: Task 2 Condense Report, Task 3 Condense Review, consensus memo, independent review, rewrite plan
4. **Own reasoning**: Based on functional overlap analysis and routing disambiguation

## Consolidation decisions

### Merge 1: skill-description-optimizer INTO skill-trigger-optimization

**Evidence:**
- Task 2 Report: "The second skill is broader and already subsumes the first"
- skill-description-optimizer procedure (steps 1-6) is a strict subset of skill-trigger-optimization (steps 1-6)
- Both rewrite the description field to fix routing
- skill-trigger-optimization additionally covers "When to use" / "Do NOT use" boundaries
- Independent review does not raise objections to this merge
- Consensus memo: "the overlap problem is real"

**Action:** Delete `skill-description-optimizer/`. The skill-trigger-optimization already covers all its functionality.

### Merge 2: skill-eval-runner INTO skill-evaluation

**Evidence:**
- Task 2 Report: "mission duplication... The difference is only entry mode: ad hoc vs suite-driven"
- skill-eval-runner expects YAML files (triggers.yaml, outputs.yaml, baselines.yaml)
- skill-evaluation constructs its own test cases
- The eval-runner's procedure is a mechanical subset of evaluation's broader procedure
- Rewrite plan: "Evaluate one skill rigorously rather than loosely"
- Consensus memo noted this as an active review question but did not object

**Action:** Merge eval-runner's suite-execution capability into skill-evaluation as a "suite mode" entry point. Delete `skill-eval-runner/`.

### Merge 3: provenance-audit INTO skill-provenance (dual-mode)

**Evidence:**
- Task 2 Report: "These are close enough that users are likely to pick the wrong one"
- provenance-audit: read-heavy assessment (audit, verify, assign trust)
- skill-provenance: writes provenance records (frontmatter patch + PROVENANCE.md)
- Both inspect the same material: origin, authorship, license, trust
- The natural workflow is audit → record, which should be one skill with two modes

**Action:** Merge provenance-audit's audit procedure into skill-provenance as an "audit" mode. Rename to `skill-provenance/` (keep existing directory). Delete `provenance-audit/`.

### Merge 4: overlay-generator INTO skill-packaging

**Evidence:**
- Task 2 Report: "overlay-generator sits inside both packaging flows already"
- skill-packaging Step 3 already includes "Generate client overlays (only when needed)"
- overlay-generator's procedure is a subset of the packaging workflow
- Standalone overlay generation is a rare use case — usually done during packaging
- Rewrite plan groups these in the same sprint

**Action:** Absorb overlay-generator's overlay format specifications into skill-packaging's Step 3. Delete `overlay-generator/`.

### Keep separate: skill-packager

**Evidence:**
- Task 2 Report: "The real difference is scope: one skill vs many"
- skill-packager orchestrates multi-skill releases
- skill-packaging handles single-skill bundling
- These serve different triggers: "package this skill" vs "release the whole library"

**Action:** Keep both. Rewrite skill-packager to explicitly delegate per-skill work to skill-packaging.

### No changes needed for these skills:
- skill-adaptation, skill-anti-patterns, skill-benchmarking, skill-catalog-curation
- skill-deprecation-manager, skill-improver, skill-installer, skill-lifecycle-management
- skill-reference-extraction, skill-registry-manager, skill-safety-review
- skill-testing-harness, skill-variant-splitting, community-skill-harvester, skill-creator

## Execution order

1. Merge skill-description-optimizer → skill-trigger-optimization
2. Merge skill-eval-runner → skill-evaluation
3. Merge provenance-audit → skill-provenance
4. Merge overlay-generator → skill-packaging
5. Update README inventory
6. Remove fluff/useless wording from all affected skills
