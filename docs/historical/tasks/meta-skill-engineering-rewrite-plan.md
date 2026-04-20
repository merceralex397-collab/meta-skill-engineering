# Concrete Rewrite Plan — Meta Skill Engineering Pack

## Objective

Bring the meta-skill-engineering pack up to a consistent package-grade standard so that every reusable skill has:

1. a sharp routing description,
2. a compact but explicit procedural body,
3. support layers only where justified,
4. machine-readable metadata,
5. repeatable evaluations,
6. changelog/provenance/security discipline.

## Target standard

Every promoted skill should converge toward this package shape:

```text
skill-name/
├── SKILL.md
├── manifest.yaml              # required for reusable/public skills
├── evals/
│   ├── trigger-positive.jsonl
│   ├── trigger-negative.jsonl
│   └── behavior.jsonl
├── references/               # optional, only if justified
│   └── README.md
├── scripts/                  # optional, only if deterministic helpers exist
├── README.md                 # required once package grows beyond SKILL-only
└── CHANGELOG.md              # required once versioning starts
```

## Global rewrite rules

### 1. Normalize all SKILL.md files to one structure

Use this section order everywhere:

1. Purpose
2. When to use
3. When NOT to use
4. Procedure / Workflow
5. Decision rules
6. Output contract
7. Failure handling
8. Script usage (only if scripts exist)
9. Reference usage (only if references exist)
10. Maintenance notes (optional)

### 2. Rewrite every description as routing logic

Every description should:
- start with an action verb,
- state the task shape,
- include 2–4 realistic trigger phrasings,
- name at least 2 confused-neighbor boundaries where applicable,
- avoid branding/marketing tone.

### 3. Add minimum package metadata

Add `manifest.yaml` to every skill that is intended for reuse.

Minimum fields:
- `schema_version`
- `skill_id`
- `canonical_name`
- `version`
- `owner`
- `status`
- `summary`
- `risk_level`
- `categories`
- `compatibility.clients`
- `requires.tools`
- `artifacts`
- `security`
- `provenance`
- `metrics`

### 4. Add minimum eval coverage

Every reusable skill gets:
- `trigger-positive.jsonl`
- `trigger-negative.jsonl`
- `behavior.jsonl`

Optional later:
- `evals/baselines/no_skill.json`
- `evals/reports/latest.md`

### 5. Add changelog discipline

Every package-grade skill gets `CHANGELOG.md`.

Track:
- routing changes,
- workflow changes,
- new/removed references,
- new/removed scripts,
- eval-result impact,
- compatibility changes.

### 6. Separate package levels

Define three maturity bands:

- **Level 1 — SKILL-only**: simple, low-risk, narrow skill.
- **Level 2 — Managed package**: SKILL + manifest + evals + changelog.
- **Level 3 — Operational package**: Level 2 + references/scripts/README + validation hooks.

The rewrite should move most skills to **Level 2** and only a few to **Level 3**.

## Priority order

### Wave 1 — Foundation and control plane
1. `skill-authoring`
2. `skill-improver`
3. `skill-testing-harness`
4. `skill-trigger-optimization`

### Wave 2 — Governance and release quality
5. `skill-evaluation`
6. `skill-benchmarking`
7. `skill-packaging`
8. `skill-provenance`
9. `skill-safety-review`
10. `skill-lifecycle-management`

### Wave 3 — Library hygiene and transformation
11. `skill-anti-patterns`
12. `skill-adaptation`
13. `skill-variant-splitting`
14. `skill-reference-extraction`
15. `skill-catalog-curation`
16. `skill-installer`

## File-by-file rewrite plan

### 1) skill-authoring

**Goal:** Make this the canonical creation skill for the whole library.

**Keep:**
- one-sentence scope test,
- routing emphasis,
- basic SKILL body scaffold.

**Rewrite:**
- add explicit output modes:
  - SKILL-only,
  - package scaffold,
  - portable-core scaffold,
- add “decision rules” section,
- add “failure handling” section if not already explicit,
- make it emit `manifest.yaml` when the requested skill is reusable.

**Add files:**
- `manifest.yaml`
- `evals/trigger-positive.jsonl`
- `evals/trigger-negative.jsonl`
- `evals/behavior.jsonl`
- `CHANGELOG.md`
- `README.md`
- `references/authoring-checklist.md`
- `references/description-patterns.md`
- optionally `scripts/init_skill_package.py`

**Acceptance criteria:**
- can create a minimal skill,
- can create a package-grade skill,
- outputs realistic trigger examples,
- emits neighbor boundaries,
- creates eval seed files automatically.

### 2) skill-improver

**Goal:** Turn it into the canonical reference implementation for the pack.

**Keep:**
- 4-part improvement model,
- mode system,
- restraint principle,
- current manifest/references/scripts/evals.

**Rewrite:**
- add explicit before/after diff contract,
- add package-upgrade decision tree,
- add regression gate language,
- add “do not add files unless justified” scoring rubric.

**Add/adjust files:**
- expand `manifest.yaml` with provenance/security fields,
- add `evals/baselines/` support,
- add `references/change-types.md`,
- add `references/package-upgrade-thresholds.md`,
- update changelog with release-style entries,
- optionally add `scripts/compare_skill_versions.py`.

**Acceptance criteria:**
- can decide surgical vs structural vs package upgrade,
- can explain exactly why each new file was or was not added,
- can produce a rewrite plan and a diff summary,
- can seed regression evals.

### 3) skill-testing-harness

**Goal:** Become the standard eval seeding skill.

**Rewrite:**
- require three eval classes explicitly: positive, negative, behavior,
- add baseline file generation,
- add held-out cases guidance,
- add confused-neighbor generation rules,
- distinguish routing evals from behavior evals more sharply.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/eval-design-patterns.md`
- `references/assertion-writing-guide.md`
- `scripts/init_evals.py`
- `scripts/check_eval_shape.py`

**Acceptance criteria:**
- can seed evals for any skill from SKILL.md,
- produces non-cheating negative cases,
- produces behavior assertions that are actually testable,
- supports baseline comparison scaffolding.

### 4) skill-trigger-optimization

**Goal:** Become the routing specialist.

**Rewrite:**
- force confusion-pair analysis,
- require undertrigger/overtrigger examples,
- require rewritten description + rewritten positive/negative boundaries,
- output trigger test suggestions alongside the rewrite.

**Add files:**
- `manifest.yaml`
- `evals/trigger-positive.jsonl`
- `evals/trigger-negative.jsonl`
- `README.md`
- `CHANGELOG.md`
- `references/routing-failure-patterns.md`
- optionally `scripts/description_lint.py`

**Acceptance criteria:**
- can sharpen a description without changing core task,
- can identify nearest confused skill,
- can generate follow-up trigger tests.

### 5) skill-evaluation

**Goal:** Evaluate one skill rigorously rather than loosely.

**Rewrite:**
- require use of existing eval files when present,
- add fixed scorecard schema,
- add blind-comparison guidance,
- add pass/fail/needs-work thresholds to manifest targets,
- add regression language against previous versions.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/scoring-rubrics.md`
- `references/blind-review-guide.md`
- optionally `scripts/render_eval_report.py`

**Acceptance criteria:**
- produces a consistent scorecard,
- reports routing metrics separately from usefulness,
- reports baseline delta clearly,
- names exact failure reasons.

### 6) skill-benchmarking

**Goal:** Compare variants with real methodology.

**Rewrite:**
- add benchmark protocol section,
- require same-case comparison,
- define tie handling and minimum sample size,
- add “when not to trust the result” section.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/benchmark-design.md`
- `references/metric-selection.md`
- optionally `scripts/benchmark_summary.py`

**Acceptance criteria:**
- supports A/B and before/after,
- produces decision-ready winner/keep/deprecate output,
- warns on underpowered sample size.

### 7) skill-packaging

**Goal:** Package skills as actual release artifacts, not just copy folders.

**Rewrite:**
- align to portable-core + overlays model,
- add validation-before-package gate,
- add outputs for bundle/archive/checksum/metadata,
- add target-client packaging notes,
- add dry-run mode expectations.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/package-layouts.md`
- `references/client-targets.md`
- `scripts/package_skill.py`
- `scripts/compute_checksums.py`

**Acceptance criteria:**
- package command has predictable outputs,
- checksum generation included,
- package refuses malformed skill trees.

### 8) skill-provenance

**Goal:** Move provenance from prose report into enforceable metadata.

**Rewrite:**
- require provenance extraction into manifest fields,
- distinguish source, maintainer, derivative basis, and assumptions,
- add confidence levels for missing provenance,
- add “trust impact” conclusion.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/provenance-checklist.md`
- optionally `scripts/extract_provenance.py`

**Acceptance criteria:**
- writes or updates manifest provenance,
- flags unverifiable origin cleanly,
- identifies hidden assumptions and undeclared dependencies.

### 9) skill-safety-review

**Goal:** Make safety review operational and machine-aided.

**Rewrite:**
- map findings to manifest security fields,
- require script and reference review separately,
- classify risk levels consistently,
- add remediation templates,
- add trust/publish recommendation output.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/safety-checklist.md`
- `references/destructive-action-patterns.md`
- optionally `scripts/scan_skill_risks.py`

**Acceptance criteria:**
- emits structured findings with severity,
- distinguishes procedural risk from script risk,
- updates security metadata where applicable.

### 10) skill-lifecycle-management

**Goal:** Make lifecycle state changes evidence-backed.

**Rewrite:**
- stop relying only on prose maturity claims,
- require evidence inputs: eval state, changelog freshness, provenance completeness, safety review status,
- define state transition gates,
- define deprecation notice format.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/lifecycle-gates.md`
- optionally `scripts/check_promotion_readiness.py`

**Acceptance criteria:**
- can justify draft → beta → stable,
- can justify deprecated/archived,
- can point to missing gate evidence.

### 11) skill-anti-patterns

**Goal:** Become the lint-style audit skill.

**Rewrite:**
- turn AP list into a machine-readable rule catalog,
- add fix priority ordering,
- distinguish routing-critical vs maintainability-only defects,
- output patch recommendations in a uniform structure.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/anti-pattern-catalog.md`
- `scripts/lint_skill_structure.py`

**Acceptance criteria:**
- emits consistent issue IDs,
- prioritizes fixes correctly,
- can feed into skill-improver mode selection.

### 12) skill-adaptation

**Goal:** Port skills cleanly without silent scope drift.

**Rewrite:**
- separate invariant core from context overlay,
- require adaptation map table,
- require unchanged-core declaration,
- add provenance note for derivative version.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/adaptation-patterns.md`
- `references/context-mapping-template.md`

**Acceptance criteria:**
- lists every changed tool/path/term,
- explains what stayed invariant,
- avoids accidental repositioning.

### 13) skill-variant-splitting

**Goal:** Split broad skills into coherent sibling skills.

**Rewrite:**
- add split-threshold rules,
- require sibling-boundary descriptions,
- require post-split routing tests across siblings,
- add migration/output plan.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/split-axis-guide.md`
- optionally `scripts/propose_skill_split.py`

**Acceptance criteria:**
- generates clear variant names,
- avoids overlap between siblings,
- includes negative tests for sibling confusion.

### 14) skill-reference-extraction

**Goal:** Make progressive disclosure precise rather than just “move long stuff out.”

**Rewrite:**
- tighten extraction criteria,
- require inline references to externalized files,
- add shared-vs-local reference rule,
- add anti-fragmentation rule.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/extraction-thresholds.md`
- `references/reference-index-template.md`
- optionally `scripts/find_extraction_candidates.py`

**Acceptance criteria:**
- extracted references remain discoverable,
- core procedure stays intact,
- no orphaned reference files.

### 15) skill-catalog-curation

**Goal:** Manage the library as a system, not as a loose folder of skills.

**Rewrite:**
- define category schema,
- add registry-oriented outputs,
- sharpen duplicate-vs-neighbor distinction,
- add action-signature methodology and confidence scoring.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/catalog-taxonomy.md`
- `references/dedup-decision-rules.md`
- optionally `scripts/build_catalog_index.py`

**Acceptance criteria:**
- emits merge/differentiate/keep decisions,
- identifies category drift,
- can generate catalog action items.

### 16) skill-installer

**Goal:** Harden installation and align it with trust policy.

**Keep:**
- existing scripts,
- GitHub/local/archive support,
- current client path table.

**Rewrite:**
- add scope model (project/user/admin),
- add trust prompts/policies,
- add checksum verification where available,
- add overwrite/dry-run semantics,
- add uninstall/update guidance,
- add shadowing/conflict detection.

**Add files:**
- `manifest.yaml`
- `README.md`
- `CHANGELOG.md`
- `references/install-scopes.md`
- `references/trust-policy.md`
- optionally `scripts/verify_package.py`

**Acceptance criteria:**
- install path is validated,
- overwrite is never silent,
- trust status is surfaced,
- dry-run supported,
- remote package verification supported when metadata exists.

## Standard manifest template to apply

```yaml
schema_version: 1
skill_id: skill-name
canonical_name: skill-name
version: 0.1.0
owner: project-local
status: draft
summary: One-sentence operational summary.
risk_level: low
categories:
  - meta
compatibility:
  clients:
    - opencode
    - copilot
    - codex
    - gemini-cli
    - claude-code
requires:
  tools: []
optional_tools: []
artifacts: []
security:
  network_required: false
  executes_local_code: false
  writes_files: false
  destructive_actions: false
  secrets_required: []
  privileged_paths: []
provenance:
  author: unknown
  maintainer: project-local
  source_repo: local
  derived_from: []
  signed: false
metrics:
  trigger_precision_target: 0.90
  trigger_recall_target: 0.85
  usefulness_target: 0.80
```

## Standard README template to apply

Each package-grade skill should get a short `README.md` with:

1. what the skill does,
2. when to use it,
3. package contents,
4. client compatibility,
5. safety/risk notes,
6. how to evaluate it,
7. version/changelog pointer.

## Standard changelog template to apply

```md
# Changelog

## 0.1.0
- initial package scaffold
- added manifest
- added eval seeds

## 0.2.0
- rewrote description for routing precision
- clarified boundaries against neighboring skills
- added behavior evals
```

## Execution plan by sprint

### Sprint 1 — Common infrastructure
- define shared manifest template
- define shared README template
- define shared changelog template
- define shared eval templates
- define naming/status/risk vocabulary

### Sprint 2 — Upgrade the 4 core skills
- authoring
- improver
- testing-harness
- trigger-optimization

### Sprint 3 — Governance/release skills
- evaluation
- benchmarking
- packaging
- provenance
- safety-review
- lifecycle-management

### Sprint 4 — Transformation/hygiene skills
- anti-patterns
- adaptation
- variant-splitting
- reference-extraction
- catalog-curation
- installer

### Sprint 5 — Validation pass
- run anti-pattern audit across all rewritten skills
- seed missing eval files
- verify manifest consistency
- check cross-skill neighbor boundaries
- verify package contents match artifacts list

## Definition of done

The pack is “rewritten” only when all of the following are true:

1. Every reusable skill has a `manifest.yaml`.
2. Every reusable skill has trigger-positive, trigger-negative, and behavior eval files.
3. Every description is routing-oriented and names realistic triggers.
4. Every skill has explicit negative boundaries.
5. Every Level 2+ skill has `README.md` and `CHANGELOG.md`.
6. Scripts exist only where deterministic helper logic is justified.
7. References exist only where progressive disclosure is justified.
8. Safety/provenance metadata is present for any skill with scripts or consequential behavior.
9. Cross-skill boundaries are consistent.
10. The pack has a clear promotion path from draft to stable.

## Recommended immediate next move

Do not rewrite all 16 simultaneously.

Start by rewriting these four to define the standard:
- `skill-authoring`
- `skill-improver`
- `skill-testing-harness`
- `skill-trigger-optimization`

Then use those rewritten skills to help rewrite the rest.
