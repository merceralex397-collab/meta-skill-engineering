# Worklog: Repository Lean & Flows

## Date
2025-03-19

## Summary
Deep analysis of all 20 skills followed by 4-phase implementation to lean the repo, clarify flows, and build testing infrastructure.

## Analysis Findings

### Built-in Flows Identified
1. **Creation Pipeline** (10 skills in sequence): community-skill-harvester → skill-creator → skill-testing-harness → skill-evaluation → skill-trigger-optimization → skill-safety-review → skill-provenance → skill-packaging → skill-installer → skill-lifecycle-management
2. **Improvement Pipeline** (4-skill loop): skill-anti-patterns → skill-improver → skill-evaluation → skill-trigger-optimization
3. **Library Management Pipeline**: skill-catalog-curation → skill-lifecycle-management

### Overlaps Found and Resolved
4 pairs of overlapping skills merged:
- skill-packager → skill-packaging (packager was "run packaging in a loop")
- skill-deprecation-manager → skill-lifecycle-management (deprecation is one lifecycle transition)
- skill-registry-manager → skill-catalog-curation (both manage library metadata)
- skill-reference-extraction → skill-improver (extraction is one improver sub-task, 0 inbound refs)

### Entry Points Identified
- **Create**: skill-creator (333 lines, most comprehensive)
- **Improve**: skill-anti-patterns (diagnose) → skill-improver (fix)
- **Library**: skill-catalog-curation (audit + manage)
- **External**: community-skill-harvester

### Inbound Reference Analysis (most-referenced skills)
- skill-improver: 15 inbound refs (most connected)
- skill-evaluation: 13 inbound refs
- skill-creator: 11 inbound refs
- skill-reference-extraction: 0 inbound refs (isolated → merged)
- skill-packager: 1 inbound ref (near-isolated → merged)

## Implementation

### Phase 1: Merge overlapping skills (20 → 16)
- **skill-packager → skill-packaging**: Added "Batch mode" section covering multi-skill release bundling, combined index generation, and release notes template. Updated description to cover both single and batch packaging.
- **skill-deprecation-manager → skill-lifecycle-management**: Added full "Deprecation procedure" section with 7-step workflow (confirm decision, find refs, update refs, update frontmatter, add notice, archive, update catalog). Added safety confirmation gate for archive move. Added deprecation-specific output contract and failure cases.
- **skill-registry-manager → skill-catalog-curation**: Added "Registry operations" section covering skill registration, metadata updates, library index generation (skills-lock.json + CATALOG.md templates), and naming convention enforcement.
- **skill-reference-extraction → skill-improver**: Created `skill-improver/references/extraction-guide.md` with full extraction procedure, classification heuristics, size rules, and output template. Updated Phase 5 to reference it.

### Phase 2: Fix cross-references and trim
- Updated skill-variant-splitting (deprecation-manager → lifecycle-management)
- Updated skill-benchmarking (deprecation-manager → lifecycle-management)
- Updated community-skill-harvester workflow-notes (skill-packager → skill-packaging)
- Removed boilerplate `## References` sections from 13 skills (identical agentskills.io URL only)
- Kept References only in community-skill-harvester (has SPDX) and skill-creator (has "What are skills")

### Phase 3: Updated root docs
- **README.md**: Rewrote for 16-skill inventory. Added 3 pipeline diagrams, entry points table, updated categories.
- **AGENTS.md**: Updated inventory count (20→16), added pipeline diagrams and entry point table, noted References are optional.

### Phase 4: Testing infrastructure
- Created `scripts/run-evals.sh`: Reads JSONL test cases, invokes copilot CLI, checks skill activation, reports precision/recall. Supports --dry-run, --all, --model, --timeout.
- Seeded eval suites for 5 critical skills (skill-creator, skill-anti-patterns, skill-improver, skill-evaluation, skill-trigger-optimization) with 8-10 positive and 8 negative cases each.
- Standardized existing evals (skill-creator, skill-improver) from old format to skill-testing-harness JSONL standard.
- Added .gitignore for eval-results/ directory.

## Key Decisions
1. **Keep skill-evaluation vs skill-benchmarking separate**: Despite overlap, they have distinct triggers (single vs comparison) and the distinction is useful for routing.
2. **No orchestrator skill in repo**: The meta-skill-orchestrator lives in ~/.copilot/skills/ — it's a tool, not a meta-skill about skills.
3. **References section policy**: Only include when skill-specific references exist. Generic spec URL was noise.
4. **Eval runner uses sonnet by default**: Cheaper than opus for trigger testing, fast enough for CI.
5. **run-first-eval deferred**: Live eval runs are expensive and slow. Infrastructure is ready; runs can happen on demand.

## Diff Stats
- Phase 1-3: 61 files changed, 395 insertions, 993 deletions (net -598 lines)
- Phase 4: 12 files changed, 301 insertions, 18 deletions
- **Total: 73 files changed, net reduction of ~300 lines while adding substantial new content**
