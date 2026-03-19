# Meta Skill Engineering

A meta-skill engineering workspace containing 20 skills that create, refine, test, package, and govern agent skills.

## Repository Layout

- `./<skill-name>/` — repo-owned skill packages at the repository root. Each package has a `SKILL.md` baseline contract and may include `references/`, `scripts/`, `evals/`, `assets/`, or `agents/`.
- `skill creator/` — archived source material from the pre-consolidation state.
- `tasks/` — task notes, worklogs, reviews, and maintenance instructions.

## Skill Lifecycle Pipeline

The standard workflow for creating and managing a skill:

```
skill-creator → skill-testing-harness → skill-evaluation → skill-benchmarking
                                                                ↓
skill-lifecycle-management ← skill-packaging ← skill-safety-review ← skill-trigger-optimization
```

1. **Create** a skill → `skill-creator`
2. **Build tests** → `skill-testing-harness`
3. **Evaluate** routing and output → `skill-evaluation`
4. **Benchmark** variants (if needed) → `skill-benchmarking`
5. **Optimize** triggers → `skill-trigger-optimization`
6. **Safety review** → `skill-safety-review`
7. **Record provenance** → `skill-provenance`
8. **Package** for distribution → `skill-packaging`
9. **Install** → `skill-installer`
10. **Manage lifecycle** → `skill-lifecycle-management`

## Skill Inventory

| Folder | Purpose |
| --- | --- |
| `community-skill-harvester` | Find external skills from public registries and evaluate them for adoption. |
| `skill-adaptation` | Rewrite a skill's context-dependent references for a new environment. |
| `skill-anti-patterns` | Scan SKILL.md for concrete anti-patterns and report fixes. |
| `skill-benchmarking` | Compare skill variants on the same test cases. |
| `skill-catalog-curation` | Detect duplicates, enforce category consistency, and verify discoverability. |
| `skill-creator` | Create new agent skills from scratch and iterate through test-review-improve cycles. |
| `skill-deprecation-manager` | Safely deprecate, retire, or merge obsolete skills. |
| `skill-evaluation` | Evaluate a single skill's routing accuracy, output quality, and baseline value. |
| `skill-improver` | Improve an existing skill package. |
| `skill-installer` | Install a skill package into a local agent client skill directory. |
| `skill-lifecycle-management` | Manage skills through draft, beta, stable, deprecated, and archived states. |
| `skill-packager` | Build distributable bundles for one or more skills in a release. |
| `skill-packaging` | Bundle a finished skill into a versioned archive with manifest, checksums, and overlays. |
| `skill-provenance` | Audit and record origin, authorship, license, and trust level for a skill. |
| `skill-reference-extraction` | Split large reference material out of a SKILL.md. |
| `skill-registry-manager` | Maintain the skill library catalog and generate the index. |
| `skill-safety-review` | Audit a skill for safety hazards before publication or import. |
| `skill-testing-harness` | Build test infrastructure for a skill. |
| `skill-trigger-optimization` | Fix skill routing by rewriting description and boundary text. |
| `skill-variant-splitting` | Split a broad skill into focused variants. |

## Skill Categories

**Creation & Improvement**
- `skill-creator` — create new skills
- `skill-improver` — improve existing skills
- `community-skill-harvester` — find and evaluate external skills

**Quality & Testing**
- `skill-testing-harness` — build test infrastructure
- `skill-evaluation` — evaluate routing and output quality
- `skill-benchmarking` — compare skill variants
- `skill-anti-patterns` — audit for structural anti-patterns
- `skill-trigger-optimization` — fix routing descriptions

**Safety & Provenance**
- `skill-safety-review` — audit for safety hazards
- `skill-provenance` — audit and record origin and trust

**Packaging & Distribution**
- `skill-packaging` — bundle a skill with manifest and overlays
- `skill-packager` — orchestrate multi-skill releases
- `skill-installer` — install skill packages

**Library Management**
- `skill-catalog-curation` — audit library for duplicates and gaps
- `skill-registry-manager` — maintain catalog and index
- `skill-lifecycle-management` — manage skill maturity states
- `skill-deprecation-manager` — execute skill deprecation

**Transformation**
- `skill-adaptation` — port skills to new environments
- `skill-variant-splitting` — split broad skills into focused variants
- `skill-reference-extraction` — extract reference material from SKILL.md
