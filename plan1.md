# Plan 1: Hub Anti-Pattern Remediation

## Problem

The 12 skills form a 55%-dense dependency graph (73/132 possible edges). `skill-improver` is referenced by all 11 other skills; `skill-evaluation` by 10/11. Descriptions spend 34% of their words on "Do not use" routing. 213 total cross-references create maintenance fragility and routing confusion.

---

## Phase 1: ~~Strip Negative Routing from Descriptions~~ → REVISED: Compress Description Routing

### Why the original Phase 1 was wrong

Skills use **progressive disclosure** with three loading levels (per `skill-creator/SKILL.md:138-142`):

1. **Metadata** (name + description) — **always in context** for ALL skills (~100 words)
2. **SKILL.md body** — loaded **only after** the skill triggers
3. **Bundled resources** — loaded on demand by the agent

The `# When NOT to use` section in the body is **never seen during routing**. It's only available after the skill has already been selected. The "Do not use for..." text in the description is the **only negative routing signal** the host has when choosing between skills.

The repo's own `skill-creator` explicitly instructs (line 87): *"End with 'Do not use for…' naming adjacent skills"*.

**Removing "Do not use" from descriptions would break routing by eliminating pre-selection negative signals.**

### Revised approach: Compress, don't remove

The hub anti-pattern in descriptions is real — 34% of description words are routing overhead. But the fix is compression, not removal. Keep negative routing in descriptions, but make it concise.

**Current verbose pattern** (skill-improver, 41 words of negative routing):
```
Do not use for creating a new skill from scratch (use skill-creator),
trigger-only fixes when the body is fine (use skill-trigger-optimization),
porting a skill to a different stack or context (use skill-adaptation),
or quick structural audits with no rewrite (use skill-anti-patterns).
```

**Compressed pattern** (19 words):
```
Not for: new skills (skill-creator), trigger-only fixes
(skill-trigger-optimization), stack porting (skill-adaptation),
audits without rewrite (skill-anti-patterns).
```

This preserves every skill name (routing signal) and every scenario (discrimination signal) while cutting words by ~50%.

### Exact changes per skill:

#### skill-adaptation
**Current negative routing** (31 words):
```
Do not use for writing a new skill from scratch (use skill-creator),
improving an existing project-specific skill without changing context
(use skill-improver), or splitting one skill into stack-specific variants
(use skill-variant-splitting).
```
**Compressed** (18 words):
```
Not for: new skills (skill-creator), in-context improvement
(skill-improver), variant splitting (skill-variant-splitting).
```

#### skill-anti-patterns
**Current negative routing** (26 words):
```
Do not use for full skill rewrites (use skill-creator), trigger-only
fixes (use skill-trigger-optimization), surgical fixes to known problems
(use skill-improver), or measuring routing precision/recall
(use skill-evaluation).
```
**Compressed** (18 words):
```
Not for: full rewrites (skill-creator), trigger-only fixes
(skill-trigger-optimization), surgical fixes (skill-improver),
routing measurement (skill-evaluation).
```

#### skill-benchmarking
**Current negative routing** (19 words):
```
Do not use for evaluating a single skill in isolation (use
skill-evaluation) or for building test infrastructure (use
skill-testing-harness).
```
**Compressed** (11 words):
```
Not for: single-skill evaluation (skill-evaluation), building tests
(skill-testing-harness).
```

#### skill-catalog-curation
**Current negative routing** (23 words):
```
Do not use for: improving a single skill (skill-improver), creating a
new skill (skill-creator), promoting or deprecating individual skills
through lifecycle states (skill-lifecycle-management).
```
**Compressed** (13 words):
```
Not for: single-skill improvement (skill-improver), creation
(skill-creator), lifecycle state changes (skill-lifecycle-management).
```

#### skill-creator
**Current negative routing** (35 words):
```
Do not use for splitting a broad skill into variants
(skill-variant-splitting), adapting a skill to a different environment
(skill-adaptation), improving an existing skill without full iteration
(skill-improver), or running standalone evaluations without creation
intent (skill-evaluation).
```
**Compressed** (17 words):
```
Not for: variant splitting (skill-variant-splitting), context porting
(skill-adaptation), improving existing (skill-improver), standalone
evaluation (skill-evaluation).
```

#### skill-evaluation
**Current negative routing** (22 words):
```
Do not use for comparing multiple variants head-to-head
(skill-benchmarking), building test infrastructure or eval suites
(skill-testing-harness), or fixing a broken skill (skill-improver).
```
**Compressed** (13 words):
```
Not for: variant comparison (skill-benchmarking), building tests
(skill-testing-harness), fixing skills (skill-improver).
```

#### skill-improver
**Current negative routing** (41 words):
```
Do not use for creating a new skill from scratch (use skill-creator),
trigger-only fixes when the body is fine (use skill-trigger-optimization),
porting a skill to a different stack or context (use skill-adaptation),
or quick structural audits with no rewrite (use skill-anti-patterns).
```
**Compressed** (19 words):
```
Not for: new skills (skill-creator), trigger-only fixes
(skill-trigger-optimization), stack porting (skill-adaptation),
audits without rewrite (skill-anti-patterns).
```

#### skill-lifecycle-management
**Current negative routing** (22 words):
```
Do not use for creating new skills (use skill-creator), improving
individual skill quality (use skill-improver), or reorganizing the
library catalog (use skill-catalog-curation).
```
**Compressed** (13 words):
```
Not for: new skills (skill-creator), quality improvement
(skill-improver), catalog reorganization (skill-catalog-curation).
```

#### skill-safety-review
**Current negative routing** (25 words):
```
Do not use for routing or output-quality evaluation (use
skill-evaluation), structural anti-pattern detection (use
skill-anti-patterns), or skills that are purely informational with
no side effects.
```
**Compressed** (15 words):
```
Not for: routing/quality evaluation (skill-evaluation), anti-pattern
detection (skill-anti-patterns), purely informational skills.
```

#### skill-testing-harness
**Current negative routing** (22 words):
```
Do not use for running existing tests (use skill-evaluation),
comparing skill variants (use skill-benchmarking), or updating tests
that already exist (edit directly).
```
**Compressed** (13 words):
```
Not for: running existing tests (skill-evaluation), variant comparison
(skill-benchmarking), updating existing tests.
```

#### skill-trigger-optimization
**Current negative routing** (19 words):
```
Do not use for fixing output quality when routing is correct
(use skill-improver) or structural anti-pattern audits
(use skill-anti-patterns).
```
**Compressed** (12 words):
```
Not for: output quality fixes (skill-improver), anti-pattern audits
(skill-anti-patterns).
```

#### skill-variant-splitting
**Current negative routing** (19 words):
```
Do not use for porting a skill to a different context
(skill-adaptation), trigger-only fixes (skill-trigger-optimization),
or catalog-level reorganization (skill-catalog-curation).
```
**Compressed** (12 words):
```
Not for: context porting (skill-adaptation), trigger-only fixes
(skill-trigger-optimization), catalog reorganization
(skill-catalog-curation).
```

### Phase 1 Revised Summary

| Skill | Negative words before | Negative words after | Saved |
|-------|----------------------|---------------------|-------|
| skill-adaptation | 31 | 18 | 13 |
| skill-anti-patterns | 26 | 18 | 8 |
| skill-benchmarking | 19 | 11 | 8 |
| skill-catalog-curation | 23 | 13 | 10 |
| skill-creator | 35 | 17 | 18 |
| skill-evaluation | 22 | 13 | 9 |
| skill-improver | 41 | 19 | 22 |
| skill-lifecycle-management | 22 | 13 | 9 |
| skill-safety-review | 25 | 15 | 10 |
| skill-testing-harness | 22 | 13 | 9 |
| skill-trigger-optimization | 19 | 12 | 7 |
| skill-variant-splitting | 19 | 12 | 7 |
| **Total** | **304** | **174** | **130 (43% reduction)** |

All skill names preserved. All scenarios preserved. Just verbose prose compressed to terse format.
Total description size: 910 → 780 words (14% reduction overall, vs 33% in the flawed original plan).

---

## Phase 2: Reduce In-Body Cross-References

**Rationale**: Procedure, Failure handling, and Next steps sections contain references to other skills. Some are genuine pipeline handoffs (keep). Others are redundant routing that duplicates "When NOT to use" or adds no procedural value (remove).

**Classification**:
- **KEEP**: References where one skill's output is the next skill's required input (pipeline edges)
- **KEEP**: References in Next steps that define the actual pipeline
- **REMOVE**: References in Procedure that say "for X, use skill-Y" (that's the host's routing job)
- **REWRITE**: References in Failure handling that say "route to skill-X" without explaining the recovery action

### Exact changes per skill:

#### skill-adaptation
- **Failure handling**: KEEP `skill-creator` ref (genuine: "skill may not be portable, recommend skill-creator to build from scratch instead" — this is a real fallback)
- **Next steps**: KEEP all 3 refs (evaluation, trigger-opt, safety-review — these are the post-adaptation pipeline)
- **Changes: None** — all refs are genuine pipeline edges

#### skill-anti-patterns
- **Procedure**: KEEP `skill-variant-splitting` ref in AP-5 definition — it's part of the anti-pattern's fix instruction, not routing
- **Failure handling**: KEEP `skill-creator` ref — genuine fallback for malformed skills
- **Next steps**: KEEP all 3 refs — these define the post-audit pipeline
- **Changes: None** — all refs are genuine

#### skill-benchmarking
- **Procedure**: REMOVE the inline ref to `skill-evaluation` in "For skill vs no-skill baseline evaluation, use skill-evaluation"
  - **Why remove**: This is routing advice, not a procedural step. The host handles this.
  - **Replace with**: "For skill vs no-skill baseline evaluation (single-skill, not variant comparison), use a different workflow."
- **Next steps**: KEEP both refs (improver, lifecycle-management — genuine pipeline)
- **Total refs removed: 1**

#### skill-catalog-curation
- **Next steps**: KEEP both refs (trigger-opt, lifecycle-management — genuine pipeline)
- **Changes: None**

#### skill-creator (heaviest — 11 outbound refs in procedure alone)
- **Procedure**: 
  - REMOVE "For details on field schemas, delegate to `skill-testing-harness`" → Replace with: "For details on field schemas, refer to AGENTS.md."
  - REMOVE "For comprehensive test suites (8+ cases, adversarial scenarios, edge coverage), route to `skill-testing-harness`" → Replace with: "For comprehensive test suites (8+ cases), build them separately after creation."
  - KEEP "Run `skill-testing-harness`" in Phase 5 iteration loop — it's a procedural instruction
  - KEEP "Run `skill-evaluation`" in Phase 5 — procedural instruction
  - KEEP "Run `skill-trigger-optimization`" in Phase 5 — procedural instruction
  - KEEP "Run `skill-safety-review`" in Phase 5 — procedural instruction
- **Failure handling**: KEEP `skill-variant-splitting` ref — genuine fallback for over-broad scope
- **Next steps**: KEEP all 6 refs — this is the creation pipeline definition
- **Total refs removed: 2** (the "delegate to" inline routing refs)

#### skill-evaluation
- **Procedure**: KEEP `skill-benchmarking` ref — it's part of a test-writing instruction ("add trigger phrases from skill-benchmarking"), not routing
- **Failure handling**: 
  - REWRITE: "Route to `skill-improver` with the eval report" → "Stop evaluation. The eval report at `eval-results/<skill>-eval.md` documents the specific failures for use by whoever fixes the skill."
  - REMOVE second `skill-improver` ref: "This enables `skill-improver` to use eval-driven diagnosis" → Remove entire sentence (it's explaining internal mechanics, not a procedure step)
- **Next steps**: KEEP all 5 refs — these define the post-evaluation pipeline
- **Total refs removed: 2** (failure handling rewrites)

#### skill-improver
- **Procedure**: KEEP `skill-anti-patterns` ref — it's a genuine decision gate ("if 3+ anti-patterns detected, use Mode 2")
- **Failure handling**:
  - KEEP `skill-anti-patterns` ref — genuine reference to the anti-pattern catalog
  - REWRITE: "Recommend the appropriate next action: `skill-variant-splitting` for splits, `skill-catalog-curation` for merges, `skill-lifecycle-management` for retirement" → "Recommend the appropriate next action: split the skill, merge it in the catalog, or retire it."
  - **Why**: The action (split/merge/retire) is what matters, not the skill names. The host will route.
- **Next steps**: KEEP all 3 refs — post-improvement pipeline
- **Total refs removed: 3** (variant-splitting, catalog-curation, lifecycle-management from failure handling)

#### skill-lifecycle-management
- **Procedure**: KEEP both `skill-evaluation` refs — they're promotion criteria ("formal evaluation via `skill-evaluation` returned a Pass verdict"), which is a genuine dependency
- **Failure handling**: KEEP `skill-evaluation` ref — genuine: "Block promotion; recommend running skill-evaluation"
- **Next steps**: KEEP both refs (safety-review, evaluation)
- **Changes: None** — all refs are genuine data dependencies

#### skill-safety-review
- **Next steps**: KEEP both refs (improver, lifecycle-management)
- **Changes: None**

#### skill-testing-harness
- **Procedure**: KEEP 3 refs (trigger-optimization, evaluation, benchmarking) — these are inside trigger-negative test examples showing `better_skill` fields, which is test data, not routing
- **Failure handling**:
  - REWRITE: "flag for `skill-trigger-optimization`" → "Flag the skill as not ready for test generation — its description needs trigger-level rewording first."
  - REWRITE: "flag for `skill-improver` to add output contract" → "Flag the skill as needing an output contract before tests can be written."
  - KEEP `skill-catalog-curation` ref — it's a genuine merge recommendation for narrow skills
- **Next steps**: KEEP both refs (evaluation, benchmarking)
- **Total refs removed: 2** (trigger-optimization, improver from failure handling)

#### skill-trigger-optimization
- **Procedure**: REMOVE inline ref to `skill-evaluation` and `skill-benchmarking`: "tests (skill-evaluation) or comparing variants (skill-benchmarking)" → "running tests or comparing variants"
  - **Why**: This is parenthetical routing info, not a procedure step
- **Failure handling**: 
  - REWRITE: "recommend `skill-variant-splitting`" → "recommend splitting the skill into narrower variants"
  - KEEP `skill-catalog-curation` ref — genuine escalation for boundary conflicts
- **Next steps**: KEEP all 3 refs (evaluation, testing-harness, catalog-curation)
- **Total refs removed: 3** (2 from procedure, 1 from failure handling)

#### skill-variant-splitting
- **Failure handling**: REWRITE: "Report that no beneficial split axis exists — improve the skill in place (via `skill-improver`)" → "Report that no beneficial split axis exists — improve the skill in place instead."
- **Next steps**: KEEP all 3 refs (catalog-curation, evaluation, lifecycle-management)
- **Total refs removed: 1** (improver from failure handling)

### Phase 2 Summary

| Skill | Refs before | Refs removed | Refs after |
|-------|------------|-------------|-----------|
| skill-adaptation | 4 | 0 | 4 |
| skill-anti-patterns | 5 | 0 | 5 |
| skill-benchmarking | 3 | 1 | 2 |
| skill-catalog-curation | 2 | 0 | 2 |
| skill-creator | 11 | 2 | 9 |
| skill-evaluation | 7 | 2 | 5 |
| skill-improver | 7 | 3 | 4 |
| skill-lifecycle-management | 5 | 0 | 5 |
| skill-safety-review | 2 | 0 | 2 |
| skill-testing-harness | 7 | 2 | 5 |
| skill-trigger-optimization | 7 | 3 | 4 |
| skill-variant-splitting | 4 | 1 | 3 |
| **Total** | **64** | **14** | **50** |

Note: Phase 2 only targets Procedure/Failure/Next-steps sections. Description refs are handled in Phase 1.

---

## Phase 3: Break Circular Routing Chains

**Current circular chains** (bidirectional edges where A→B and B→A both exist):

| Chain | A→B location | B→A location | Resolution |
|-------|-------------|-------------|------------|
| skill-improver ↔ skill-anti-patterns | improver procedure: "3+ from skill-anti-patterns scan" | anti-patterns next-steps: "Fix found issues → skill-improver" | **Keep both** — this is a genuine diagnostic→fix pipeline. Anti-patterns diagnoses, improver fixes. Not circular in purpose. |
| skill-evaluation ↔ skill-improver | evaluation failure: "Route to skill-improver" | improver next-steps: "Verify → skill-evaluation" | **Remove evaluation→improver** (already done in Phase 2 failure rewrite). The remaining direction is: evaluation feeds data, improver acts, then routes back to evaluation for verification. This is a legitimate re-test loop, not a circular routing trap. |
| skill-evaluation ↔ skill-benchmarking | evaluation procedure: "add trigger phrases from skill-benchmarking" | benchmarking procedure: "For baseline, use skill-evaluation" | **Remove benchmarking→evaluation** (already done in Phase 2). The remaining ref in evaluation's procedure is test-writing guidance, not routing. |
| skill-testing-harness ↔ skill-trigger-optimization | harness failure: "flag for skill-trigger-optimization" | trigger-opt next-steps: "Build trigger tests → skill-testing-harness" | **Remove harness→trigger-opt** (already done in Phase 2 failure rewrite). Remaining direction: trigger-opt routes to harness for test creation. |

**Result**: After Phase 2 rewrites, all circular chains are already broken. No additional Phase 3 changes needed — the Phase 2 rewrites were designed to resolve these.

---

## Phase 4: Add AP-17 to Anti-Patterns Checklist

**File**: `skill-anti-patterns/SKILL.md`
**Location**: After AP-16 (line ~139), before `# Output contract`

**Exact text to add**:

```markdown
**AP-17: Hub coupling** · `MEDIUM` — maintenance burden and routing confusion
- Pattern: Skill references 5+ other skills by name in its body, or description contains "Do not use" routing to 3+ alternatives
- Example before: `Do not use for creating (skill-creator), improving (skill-improver), evaluating (skill-evaluation), or testing (skill-testing-harness).`
- Example after: Description contains only positive routing. Negative routing lives in "When NOT to use" section. Body references only skills whose output is a direct input.
- Fix: Move "Do not use" out of description into "When NOT to use". In procedure, reference only skills whose output this skill consumes. In next-steps, reference only the immediate next pipeline step.
```

**Also update**:
- Output contract table: add row `| AP-17 | Hub coupling | ... |`
- Any reference to "AP-1 through AP-16" → "AP-1 through AP-17"
- Purpose line: "AP-1 through AP-16" → "AP-1 through AP-17"

---

## Verification

1. **Before**: Run `scripts/run-evals.sh --all` — record baseline trigger rates
2. **Execute**: Phases 1→4
3. **After**: Run `scripts/run-evals.sh --all` — compare trigger rates
4. **Structural**: Run `scripts/validate-skills.sh` — confirm 12/12 pass
5. **Graph metrics**: Re-run graph density analysis — target <40% density
6. **Description metrics**: Re-measure avg word count — target <55 words avg

## Risks

| Risk | Mitigation |
|------|-----------|
| Removing "Do not use" from descriptions hurts routing accuracy | Run trigger-negative evals before and after. The "When NOT to use" section body is still read by the host. |
| Removing procedure refs breaks handoff chains | Only removing routing refs, not data-dependency refs. Next-steps pipeline refs are all kept. |
| AP-17 is too strict for this repo (meta-skills are inherently coupled) | AP-17 is a detection pattern, not a hard rule. The audit output already uses PRESENT/ABSENT, not pass/fail. |
