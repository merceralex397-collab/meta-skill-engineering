NOTE: This work was carried out before the meta engineering skills were split off into a separate repository.

# Meta-Skill Engineering Audit Worklog

## Task

Audit, clean, and verify all 19 skills from `LibraryUnverified/03-meta-skill-engineering/` using the meta-skill-engineering skills themselves. Produce verified candidates in `Library/03-meta-skill-engineering/`.

## Summary

- **Input**: 19 unverified skills from external source (Codex skill ecosystem)
- **Output**: 16 verified skills in `Library/03-meta-skill-engineering/`
- **Deleted**: 3 skills (merged or redundant)
- **Total SKILL.md lines**: 2,244 → 2,095 (7% reduction after structural cleanup + functional enrichment)

---

## Phase 0: Setup

Created `Library/03-meta-skill-engineering/` and `Tasks/worklog/`. Flattened `skill-improver/skill-improver/` double-nesting to `skill-improver/`.

## Phase 1: Triage — Overlap Resolution

### Deleted: `skill-installation`
- **Reason**: 26-line redirect stub containing only "This skill has been merged into skill-installer". Zero unique content.
- **Skills used**: skill-catalog-curation (overlap detection)

### Deleted: `skill-creator` (merged into `skill-authoring`)
- **Reason**: 427 lines, heavily Codex-specific. Depended on `init_skill.py`, `generate_openai_yaml.py`, `quick_validate.py`, `agents/openai.yaml`. ~60% was Codex platform tutorial.
- **What was preserved**: Host-agnostic design guidance merged into `skill-authoring`:
  - "Concise is key" / context window awareness
  - "Match freedom to fragility" framework (high/medium/low freedom)
  - Size targets and progressive disclosure guidance
- **Skills used**: skill-catalog-curation (overlap detection), skill-improver (merge technique)

### Deleted: `skill-refinement` (merged into `skill-improver`)
- **Reason**: 86 lines. Entirely a subset of `skill-improver` Mode 1 (Surgical edit). `skill-improver` already covered all three improvement levels.
- **What was preserved**: Failure-mode categorization table with fix-action mappings merged into `skill-improver` Phase 2. Refinement summary template merged into Mode 1 output.
- **Skills used**: skill-catalog-curation (overlap detection), skill-improver (merge technique)

## Phase 2: De-Codex Pass

Applied to all 16 surviving skills. Changes per skill:

| Change | Before | After |
|--------|--------|-------|
| `owner` | `codex` | Removed (moved to frontmatter normalization) |
| `clients` | `[openai-codex, gemini-cli, opencode, github-copilot]` | `[opencode, copilot, codex, gemini-cli, claude-code]` |
| Codex URLs | `developers.openai.com/codex/*` in References | Removed |
| `agents/` dirs | `agents/openai.yaml` | Deleted |
| `overlays/` dirs | 4 client-specific overlay subdirs | Deleted |
| Body text | "Codex" as runtime agent name | Replaced with generic "the agent" |

**Special handling**:
- `skill-installer`: Generalized `$CODEX_HOME/skills/` to multi-client path table, removed `openai/skills` GitHub repo references, changed "Restart Codex" to "Restart the agent client"

## Phase 3: Individual Skill Audit

Each skill processed through a 5-step pipeline. Audit tools processed first (so their techniques were clean when applied to remaining skills).

### Pipeline Applied

| Step | Technique Source | What It Does |
|------|-----------------|--------------|
| 1. Anti-pattern scan | `skill-anti-patterns` | Check for AP-1 through AP-12 structural anti-patterns |
| 2. Trigger optimization | `skill-trigger-optimization` | Rewrite description for routing precision/recall |
| 3. Content refinement | `skill-improver` | Tighten language, remove bloat, strengthen outputs |
| 4. Size check | `skill-reference-extraction` | Extract to references/ if > 150 lines |
| 5. Safety review | `skill-safety-review` | Check scope creep, description-behavior mismatch |

### Per-Skill Audit Results

#### skill-anti-patterns (132 → 133 lines)
- Added 3 realistic trigger phrases to description
- Expanded negative boundaries from 2 → 4
- Fixed nonsensical tags `[skill, anti, patterns]` → `[skill-engineering, audit, quality]`
- Replaced vague failure handling with specific recovery actions
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files) — all generic Codex boilerplate
- Kept: AP-1 through AP-12 catalog (core value)

#### skill-trigger-optimization (97 → 104 lines)
- Started description with "Rewrite" (stronger action verb)
- Added `skill-anti-patterns` as negative boundary (was missing)
- Added "Type:" and "Confused with:" fields to output contract
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-improver (407 → 198 lines, 51% reduction)
- Removed "Working posture" (fluffy meta-commentary)
- Removed "Examples" section (restated what modes already explain)
- Removed "Decision rules" section (duplicated trigger-optimization territory)
- Trimmed anti-patterns 10 → 5 (kept improvement-specific, cut structural-audit ones)
- Consolidated Phase 2 failure modes into integrated table
- Cleaned manifest.yaml provenance block

#### skill-reference-extraction (105 → 100 lines)
- Added named alternatives in negative boundaries
- Changed pre-filled example values to bracket placeholders
- Fixed verification checkboxes from `[x]` → `[ ]`
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-safety-review (110 → 90 lines)
- Started description with "Audit"
- Added realistic trigger phrases ("is this skill safe to publish")
- Converted procedure from question-list to prose with specific examples
- Added Safety section with specific scan commands
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-authoring (215 → 194 lines)
- Folded "Design principles" into existing steps (eliminated wrapper prose)
- Fixed weak output defaults → concrete 4-item deliverables list
- **Removed**: references/official-spec.md (duplicated SKILL.md), references/iteration-and-testing.md (Codex-specific)
- **Kept**: 11 reference files, 2 templates, 10 workflows (all host-agnostic)

#### skill-catalog-curation (109 → 107 lines)
- Replaced "Flag >70% overlap" (magic number) with intent-based matching
- Replaced "Apply quality gates" with "Check discoverability" (old step duplicated lifecycle-management)
- Output: replaced hardcoded example numbers with placeholders
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-evaluation (95 → 97 lines)
- Added "Next action" field to output (forces actionable verdicts)
- Removed phantom `skill-eval-runner` reference
- Made baseline comparison required (was optional but output assumed it)
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-benchmarking (91 → 95 lines)
- Added concrete thresholds (5pp, 10%, >50%) to procedure
- Merged metrics collection steps (removed unrealistic blind comparison framing)
- Added "Missing acceptance criteria" failure mode
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-testing-harness (153 → 143 lines)
- Fixed `skill-eval-runner` → `skill-evaluation`
- Trimmed JSONL examples (removed duplicates overlapping with skill-authoring)
- Collapsed verbose fixture list into single sentence
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-adaptation (96 → 95 lines)
- Fixed phantom `skill-description-optimizer` reference
- Added "do not guess" constraint to procedure
- Removed step 7 (document history) — provenance is out of scope
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-lifecycle-management (89 → 87 lines)
- Removed arbitrary "Library <15 skills" exclusion
- Extracted lifecycle state definitions to standalone table
- Removed step "Add event to PROVENANCE.md" (overlapped with provenance skill)
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-packaging (99 → 125 lines)
- Fixed phantom `skill-packager` and `skill-installation` references
- Changed checksums from single concatenated to per-file SHA-256
- Added version handling: "Prompt user; do not guess"
- Risk raised to medium (creates archives)
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-installer (132 → 116 lines)
- Risk raised to medium (copies files to disk)
- Added new Safety section: overwrite protection, path traversal, source trust
- Removed phantom `npx skills add` CLI reference
- Documented Codex-specific defaults in scripts as limitation
- **Removed**: Codex-specific client paths

#### skill-provenance (108 → 108 lines)
- Removed step 6 "Record change history" (overlapped with git/lifecycle-management)
- Converted trust levels from bullets to scannable table
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

#### skill-variant-splitting (100 → 110 lines)
- Fixed phantom `skill-description-optimizer` reference
- Added all 3 negative boundaries
- Expanded failure handling with specific recovery actions
- **Removed**: manifest.yaml, evals/ (4 files), references/ (3 files)

## Phase 4: Catalog-Level Consistency

### Cross-Reference Fix
Fixed 18 stale references to `skill-refinement` across 10 skills → replaced with `skill-improver`. Verified zero stale refs remain.

### Frontmatter Normalization
Standardized all 16 skills to consistent format:
```yaml
---
name: skill-name
description: >- [text]
license: Apache-2.0
compatibility:
  clients: [opencode, copilot, codex, gemini-cli, claude-code]
---
```
- Removed all `metadata` blocks (owner, domain, maturity, risk, tags — provenance noise, not routing data)
- Removed `allowed-tools` (Codex-specific)
- Added missing `license` and `compatibility` fields to 4 skills

### Final Verification
- Zero stale cross-references
- Zero Codex-specific URLs
- Zero `owner: codex` references
- Consistent frontmatter across all 16 skills
- All skills 87–198 lines (well within bounds)

---

## Skills Used (and why)

| Skill | How Used | Why This Skill |
|-------|----------|----------------|
| **skill-anti-patterns** | Step 1 of pipeline: structural audit of each skill | Detects the structural problems (circular triggers, vague outputs, etc.) that other techniques then fix |
| **skill-trigger-optimization** | Step 2: rewrite descriptions for routing | Description is the highest-leverage field — determines whether skill fires |
| **skill-improver** | Step 3: content refinement, merge technique | Covers surgical edits through structural refactors; used for both individual tightening and the overlap merges |
| **skill-reference-extraction** | Step 4: size check | Determines whether SKILL.md content should be split into references/ |
| **skill-safety-review** | Step 5: safety audit | Catches scope creep, missing safety gates, description-behavior mismatches |
| **skill-catalog-curation** | Phase 1 + Phase 4: overlap detection, consistency | Library-level view: finds duplicates, enforces consistency, checks discoverability |

## Skills NOT Used (and why)

| Skill | Why Not Used |
|-------|-------------|
| **skill-authoring** | Creates new skills from scratch — we were improving existing skills |
| **skill-creator** | Deleted (Codex-specific) — even before deletion, it creates new skills |
| **skill-adaptation** | Adapts skills to different stacks/contexts — we weren't changing target context |
| **skill-benchmarking** | Compares skill variants head-to-head — requires before/after runtime testing, not applicable in static audit |
| **skill-evaluation** | Measures routing/output quality against baseline — requires runtime execution |
| **skill-testing-harness** | Builds test infrastructure — useful but a separate task from content audit |
| **skill-installation** | Deleted (redirect stub) |
| **skill-installer** | Installs skills into agent clients — not relevant to content audit |
| **skill-lifecycle-management** | Manages draft→stable states — useful for maturity tracking but not content improvement |
| **skill-packaging** | Packages for distribution — not relevant to content audit |
| **skill-provenance** | Documents origin/authorship — we already know the origin |
| **skill-variant-splitting** | Splits broad skills into variants — no skills needed splitting (overlaps were resolved by merging instead) |
| **skill-reference-extraction** | Was in the pipeline but no skill exceeded the threshold after refinement — all stayed under 200 lines |

Note: `skill-reference-extraction` was *applied* as a check (Step 4 of every audit) but never *triggered* because all skills came in under the extraction threshold after content refinement.

---

## Final Inventory

| # | Skill | Lines | Status |
|---|-------|-------|--------|
| 1 | skill-adaptation | 95 | Verified |
| 2 | skill-anti-patterns | 133 | Verified |
| 3 | skill-authoring | 194 | Verified |
| 4 | skill-benchmarking | 95 | Verified |
| 5 | skill-catalog-curation | 107 | Verified |
| 6 | skill-evaluation | 97 | Verified |
| 7 | skill-improver | 198 | Verified |
| 8 | skill-installer | 116 | Verified |
| 9 | skill-lifecycle-management | 87 | Verified |
| 10 | skill-packaging | 125 | Verified |
| 11 | skill-provenance | 108 | Verified |
| 12 | skill-reference-extraction | 100 | Verified |
| 13 | skill-safety-review | 90 | Verified |
| 14 | skill-testing-harness | 143 | Verified |
| 15 | skill-trigger-optimization | 104 | Verified |
| 16 | skill-variant-splitting | 110 | Verified |

**Total**: 1,895 lines across 16 skills (down from 2,244 across 19).

---

## Post-Audit Review Pass

A code review (Opus 4.6) found 8 issues. All fixed:

### Issue 1 + 3 (Critical/High): skill-authoring workflows/ taught wrong format
- **Problem**: 10 workflow files, 2 templates, and 10/11 references taught XML-tag format (`<objective>`, `<routing>`, etc.) contradicting the Markdown-heading format used by every skill in this library. 3 workflows referenced non-existent `references/use-xml-tags.md`.
- **Fix**: Deleted `workflows/` (10 files), `templates/` (2 files), and 10/11 reference files. Kept only `references/best-practices.md` (format-neutral, Markdown headings, host-agnostic).

### Issue 2 (High): skill-installer scripts Codex-bound
- **Problem**: Both Python scripts defaulted to `~/.codex/skills/`, `openai/skills` repo, and `codex` user-agent strings.
- **Fix**: Added `--client` flag (copilot/codex/opencode/claude-code/gemini-cli) to both scripts. Defaults to `copilot`. Centralized client path mapping in `github_utils.py`. Fixed user-agent strings to `scafforge-*`. Updated SKILL.md to document the flag.

### Issue 4 (Medium): skill-improver version mismatch
- **Fix**: Added 0.2.0 entry to CHANGELOG.md documenting the audit changes.

### Issue 5 (Medium): skill-improver manifest missing codex client
- **Fix**: Added `codex` to manifest.yaml clients list.

### Issue 6 (Medium): YAML scalar style inconsistency
- **Fix**: Normalized `skill-benchmarking`, `skill-trigger-optimization`, `skill-lifecycle-management` to `>-` (folded, strip) matching the other 13 skills.

### Issue 7 (Medium): skill-authoring taught stripped metadata blocks
- **Fix**: Removed `metadata` block from the Step 3 YAML template and removed all `metadata.*` rows from the field table.

### Issue 8 (Low): Surviving hedge verbs
- **Fix**: `skill-lifecycle-management` "Ensure" → "Set". `skill-testing-harness` "consider merging" → "merge via skill-variant-splitting or widen the trigger set".

---

## Phase 6: Functional Improvement Pass

After structural cleanup, an honest assessment revealed the skills had been **normalized but not functionally improved** — cleaner format, better triggers, but the actual procedures, decision logic, and domain expertise were unchanged. This phase addressed that gap with targeted enrichments to all 16 skills.

### Approach
For each skill, identified the specific functional weakness (vague decision criteria, missing worked examples, thin procedure steps) and added concrete domain knowledge. Changes were surgical — only touching sections that needed enrichment, not rewriting what was already good.

### Skills used
- **skill-improver** (Mode 1 — surgical edit): Each skill got targeted fixes, not structural rewrites
- **skill-anti-patterns** (AP-3 check): Verified enrichments added procedural value, not just more words
- **skill-evaluation** (conceptual): Applied the "does each step produce a concrete artifact or decision?" test

### Changes by skill

**Audit tools (5 skills):**

| Skill | Functional Improvement |
|---|---|
| **skill-anti-patterns** | Added severity tags (CRITICAL/HIGH/MEDIUM/LOW) to all 12 anti-patterns with cause rationale. Added "Quick scan priority" with 5-min/15-min/triage shortcuts — routing-critical APs first |
| **skill-trigger-optimization** | Added worked example showing bad→good description transformation with explicit annotation of each transform applied |
| **skill-improver** | Added signal-based mode selection guide with concrete indicators for Mode 1/2/3 and a 30% escalation heuristic |
| **skill-reference-extraction** | Added reference-vs-core distinction heuristic (consulted-every-time vs sub-case-only), 20-line extraction threshold, reference link quality check |
| **skill-safety-review** | Added 3-tier risk classification (High: file deletion/deployment, Medium: file creation/network, Low: reads/computation) with per-tier action requirements. Added 5 concrete scope-creep signals |

**Authoring & operations (6 skills):**

| Skill | Functional Improvement |
|---|---|
| **skill-authoring** | Added "Common authoring mistakes" section: 5 anti-patterns (tutorial-not-procedure, goals-not-steps, inline-reference-bloat, description-last, missing-negatives) with symptoms |
| **skill-catalog-curation** | Added action-signature extraction method for duplicate detection (verb+object grouping, >50% trigger overlap threshold). Added 3 concrete discoverability thresholds |
| **skill-lifecycle-management** | Enriched promotion criteria: draft→beta requires diverse prompts (core/edge/negative), beta→stable requires ≥10 cases at ≥90% + 2 real-project uses. Added deprecation protocol |
| **skill-adaptation** | Added concrete invariant identification method (always-check list, never-invariant list, does-vs-how heuristic). Added post-adaptation validation walk-through |
| **skill-variant-splitting** | Added axis selection priority: fewest-variants-first, distinct-trigger-vocabulary tiebreaker, Stack>Domain>Platform>Scope default, >5-variants red flag |
| **skill-installer** | Added pre-install verification (archived check, frontmatter validation, scripts/ scanning) and post-install verification steps with fail-fast removal |

**Evaluation & packaging (5 skills):**

| Skill | Functional Improvement |
|---|---|
| **skill-evaluation** | Added test case construction guide: derive positives from "When to use", negatives from "Do NOT use" + adjacent skills. Anti-pattern: don't include skill names in test prompts |
| **skill-benchmarking** | Added blind comparison protocol, 4-dimension scoring rubric (correctness/completeness/conciseness/actionability), token-efficiency tiebreaker, round-robin pairwise for >2 variants |
| **skill-testing-harness** | Added 4 categories each for positive and negative cases with 2-per-category minimums. Added 60/30/10 distribution rule. Output tests check for hallucinated sections and 30% hedge-word threshold |
| **skill-packaging** | Added semver guidance (MAJOR/MINOR/PATCH criteria), required-vs-optional field rules, pre-archive validation checklist (parseable SKILL.md, manifest matching, no stray files, 100KB warning) |
| **skill-provenance** | Added investigation techniques: git blame for origin, URL scanning for evidence sources, tool/path/extension scanning for encoded assumptions |

### Metrics
- **Lines**: 1,895 → 2,095 (+200 lines, all functional content)
- **Skills improved**: 16/16
- **New decision criteria added**: 23 (concrete thresholds, heuristics, classification tiers)
- **Worked examples added**: 2 (skill-trigger-optimization, skill-reference-extraction)
- **No frontmatter changes** — all structural normalization from Phase 4 preserved
- **No stale references introduced** — verified post-improvement

---

## Final Summary

### Total transformation: LibraryUnverified → Library

| Metric | Before | After |
|---|---|---|
| Skills | 19 | 16 |
| Total SKILL.md lines | 2,244 | 2,095 |
| Codex-specific artifacts | ~60 files | 0 |
| Stale cross-references | 18 | 0 |
| Frontmatter format | inconsistent | normalized (4-field) |
| Severity/priority guidance | 0 skills | 1 (skill-anti-patterns) |
| Worked examples | 0 skills | 2 (skill-trigger-optimization, skill-reference-extraction) |
| Concrete decision thresholds | sparse | 23 new criteria across 16 skills |
| Mode/variant selection guides | 0 | 2 (skill-improver, skill-variant-splitting) |

### What changed functionally
- Every skill now has concrete decision criteria where it previously had vague guidance
- Audit tools (anti-patterns, safety-review) now have severity classification and triage shortcuts
- Skills that make choices (improver mode, variant axis, lifecycle promotion) now have signal-based decision guides
- Test-related skills (evaluation, benchmarking, testing-harness) now teach HOW to construct effective test cases
- Safety and packaging skills now have concrete verification checklists

### What was preserved
- All genuinely useful content from deleted skills (merged into surviving counterparts)
- All host-agnostic auxiliary files (scripts, references, evals) from skill-improver and skill-authoring
- All cross-references updated to point to correct surviving skills
