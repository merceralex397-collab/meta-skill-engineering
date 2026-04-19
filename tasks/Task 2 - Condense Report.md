# Task 2 Condense Report

## Scope

I reviewed the 26 repo-owned skill packages described in [README.md](../README.md): 22 packages at the repository root and 4 packages under [`skill creator/`](../skill%20creator/). I read every `SKILL.md`, then read supporting manifests, references, eval docs, and scripts where they materially changed what the package actually does.

This report is about functional overlap, not name overlap. The question I used throughout was: if a user asked for a task in plain language, would two packages plausibly compete for the same job?

## Executive Summary

The repo has four clear duplication or near-duplication problems:

| Priority | Recommendation | Packages | Why |
| --- | --- | --- | --- |
| High | Merge into one routing-remediation skill | `skill-trigger-optimization`, `skill-description-optimizer` | The narrower skill is almost entirely a subset of the broader one. Both rewrite routing text, compare siblings, and validate with positive/negative prompts. |
| High | Merge into one provenance skill with modes | `provenance-audit`, `skill-provenance` | Both inspect origin, evidence, assumptions, and trust. The main difference is output mode: report vs recorded metadata. |
| High | Consolidate into one canonical evaluator | `skill-eval-runner`, `skill-evaluation` | Both evaluate one skill’s routing, output quality, and baseline value. They also conflict on thresholds and eval file formats. |
| Medium | Collapse or subordinate packaging sub-capabilities | `skill-packaging`, `skill-packager`, `overlay-generator` | These three all participate in bundling and overlay generation. The real distinction is single-skill packaging vs multi-skill release orchestration; overlay generation is thinner and likely better as a sub-capability. |

Two other areas need boundary cleanup, but not merges:

| Priority | Recommendation | Packages | Why |
| --- | --- | --- | --- |
| Medium | Clarify decision vs execution handoff | `skill-lifecycle-management`, `skill-deprecation-manager` | These are adjacent, but the split is defensible: lifecycle decides transitions; deprecation manager executes one deprecation. |
| Medium | Clarify audit vs mutation handoff | `skill-catalog-curation`, `skill-registry-manager` | These scan similar inventory, but one should analyze and recommend while the other mutates `skills-lock.json` / `CATALOG.md`. |

The creation cluster is mostly defensible once one package is reclassified:

| Recommendation | Packages | Why |
| --- | --- | --- |
| Keep separate, but sharpen roles | `skill-authoring`, `skill-improver`, `anthropiuc-skill-creator` | These cover greenfield authoring, existing-skill improvement, and full lifecycle orchestration respectively. |
| Reclassify, not merge | `community-skill-harvester` | It is an acquisition/adoption workflow, not a creation workflow. Its current placement makes it look more redundant than it is. |
| Keep separate | `microsoft-skill-creator` | This is a genuine specialization with Microsoft Learn research workflows, templates, and a hybrid local-plus-dynamic content model. |

## Detailed Findings

### 1. Creation And Acquisition

The root creation split is mostly sound.

- [`skill-authoring`](../skill%20creator/skill-authoring/SKILL.md) is a narrow greenfield skill writer. It focuses on one-sentence scope definition, frontmatter, required body sections, output defaults, and anti-pattern checks.
- [`skill-improver`](../skill-improver/SKILL.md) is a refactoring and package-upgrade skill for an existing package. Its three modes are surgical edit, structural refactor, and package upgrade, and it explicitly preserves the existing skill’s core purpose.
- [`anthropiuc-skill-creator`](../skill%20creator/anthropiuc-skill-creator/SKILL.md) is broader than both. It covers intent capture, drafting, eval design, baseline comparison, review generation, iterative improvement, description optimization, benchmarking, and packaging. The support scripts reinforce that breadth: [`run_eval.py`](../skill%20creator/anthropiuc-skill-creator/scripts/run_eval.py), [`run_loop.py`](../skill%20creator/anthropiuc-skill-creator/scripts/run_loop.py), [`aggregate_benchmark.py`](../skill%20creator/anthropiuc-skill-creator/scripts/aggregate_benchmark.py), and [`package_skill.py`](../skill%20creator/anthropiuc-skill-creator/scripts/package_skill.py).

Conclusion:

- Keep `skill-authoring` as the lightweight “create a new skill correctly” package.
- Keep `skill-improver` as the lightweight “repair or upgrade an existing skill” package.
- Keep `anthropiuc-skill-creator`, but position it explicitly as the umbrella “full lifecycle skill studio” so it stops reading like a duplicate of the narrower packages.

Two creation-adjacent packages are actually different:

- [`microsoft-skill-creator`](../skill%20creator/microsoft-skill-creator/SKILL.md) is specialized authoring for Microsoft ecosystems, backed by Learn MCP or `mslearn` and domain-specific templates in [`references/skill-templates.md`](../skill%20creator/microsoft-skill-creator/references/skill-templates.md).
- [`community-skill-harvester`](../skill%20creator/community-skill-harvester/SKILL.md) is a build-vs-buy workflow. It searches public sources, scores external skills, checks license compatibility, extracts reusable patterns, and produces import proposals. It should be grouped with adoption/import work, not with creation.

### 2. Evaluation And Testing

This is the messiest overlap cluster in the repo.

The intended roles are clear on paper:

- [`skill-testing-harness`](../skill-testing-harness/SKILL.md) builds eval artifacts.
- [`skill-evaluation`](../skill-evaluation/SKILL.md) evaluates one skill.
- [`skill-benchmarking`](../skill-benchmarking/SKILL.md) compares variants.
- [`skill-eval-runner`](../skill-eval-runner/SKILL.md) executes an existing eval suite.

The actual problem is that the evaluator and harness do not line up.

- `skill-testing-harness` writes `trigger-positive.jsonl`, `trigger-negative.jsonl`, and `output-tests.jsonl`.
- `skill-eval-runner` looks for `triggers.yaml`, `outputs.yaml`, and `baselines.yaml`.
- [`skill-eval-runner/manifest.yaml`](../skill-eval-runner/manifest.yaml) itself points back toward JSONL artifacts instead of the YAML files named in its own `SKILL.md`.
- [`skill-improver/scripts/init_eval_files.py`](../skill-improver/scripts/init_eval_files.py) also bootstraps JSONL-style eval files, which reinforces that the repo’s center of gravity is not the YAML format used by `skill-eval-runner`.

There is also mission duplication:

- `skill-evaluation` already knows how to build or assemble cases when none exist, then measure routing, output quality, and no-skill baseline value.
- `skill-eval-runner` measures the same three things and ends in the same kind of verdict.
- The difference is only entry mode: ad hoc vs suite-driven.

Recommendation:

1. Merge `skill-eval-runner` into `skill-evaluation`.
2. Keep `skill-testing-harness` separate, but standardize it on the same eval schema the evaluator expects.
3. Keep `skill-benchmarking` separate because its job is genuinely different: it chooses among variants and uses significance/tie-breaking logic rather than issuing a single-skill health verdict.
4. Make `skill-improver` delegate eval scaffolding to the canonical harness instead of maintaining a slightly different bootstrap pattern.

### 3. Packaging, Distribution, And Catalog

This cluster has real structure, but too many top-level entry points for work that often happens together.

The actual package roles are:

- [`skill-packaging`](../skill-packaging/SKILL.md): strict single-skill bundler with manifest, checksums, optional overlays, archive creation, and archive re-verification.
- [`skill-packager`](../skill-packager/SKILL.md): multi-skill release orchestrator that scans a library, validates many skills, generates overlays, emits per-skill manifests, produces per-skill and combined bundles, and writes release notes.
- [`overlay-generator`](../overlay-generator/SKILL.md): cross-client metadata conversion layer that derives per-client overlays from a canonical `SKILL.md`.
- [`skill-installer`](../skill-installer/SKILL.md): downstream installer into client-specific local directories. This is operationally distinct from packaging.
- [`skill-registry-manager`](../skill-registry-manager/SKILL.md): canonical catalog/state mutator for `skills-lock.json` and `CATALOG.md`.
- [`skill-catalog-curation`](../skill-catalog-curation/SKILL.md): library-wide audit/report that detects duplicates, discoverability gaps, and category drift.

The strongest overlap is `skill-packaging` vs `skill-packager`. Both create manifests, checksums, overlays, validation, and archives. The real difference is scope: one skill vs many.

`overlay-generator` also sits inside both packaging flows already:

- `skill-packager` explicitly includes overlay generation as a packaging step.
- `skill-packaging` includes optional overlay generation for a single skill.

That makes `overlay-generator` look more like a reusable packaging sub-capability than a strong standalone top-level skill unless overlay-only conversion is common enough to justify its own routing surface.

Recommendation:

1. Keep `skill-packaging` as the single-skill primitive.
2. Keep `skill-packager` only if it is rewritten as a release wrapper that explicitly delegates per-skill bundle creation to `skill-packaging`.
3. Absorb `overlay-generator` into the packaging family, or rewrite it to trigger only for overlay-only conversion/validation tasks.
4. Keep `skill-installer` separate.
5. Keep `skill-catalog-curation` and `skill-registry-manager` separate, but tighten the wording so curation owns analysis/recommendation and registry manager owns mutation.

One implementation detail is worth recording because it affects the condensation plan:

- The documented surface of [`skill-installer`](../skill-installer/SKILL.md) is broader than its scripts currently implement. The scripts primarily cover GitHub download/sparse checkout and install/list flows rather than the full local-folder/archive behavior described in the `SKILL.md`. See [`install-skill-from-github.py`](../skill-installer/scripts/install-skill-from-github.py), [`list-skills.py`](../skill-installer/scripts/list-skills.py), and [`github_utils.py`](../skill-installer/scripts/github_utils.py).

### 4. Provenance, Routing, And Governance

This cluster contains two very strong merge candidates and several defensible adjacencies.

#### Provenance

- [`provenance-audit`](../provenance-audit/SKILL.md) is a read-heavy provenance assessment workflow: reconstruct origin, verify source claims, review license, and assign trust.
- [`skill-provenance`](../skill-provenance/SKILL.md) investigates much of the same material, then writes a frontmatter patch plus `PROVENANCE.md`.

These are close enough that users are likely to pick the wrong one unless the split is extremely explicit.

Recommendation:

- Merge into one provenance package with two modes: `audit` and `record`.
- If both remain, rewrite the descriptions so `provenance-audit` is explicitly read-only and `skill-provenance` is explicitly “write the provenance record into the package.”

#### Routing

- [`skill-description-optimizer`](../skill-description-optimizer/SKILL.md) rewrites the `description` field to fix routing.
- [`skill-trigger-optimization`](../skill-trigger-optimization/SKILL.md) rewrites the `description` field and the “When to use” / “Do NOT use” boundaries to fix routing.

The second skill is broader and already subsumes the first.

Recommendation:

- Merge `skill-description-optimizer` into `skill-trigger-optimization`.
- Keep a “description-only mode” inside `skill-trigger-optimization` for constrained edits or batch work.

#### Governance

- [`skill-lifecycle-management`](../skill-lifecycle-management/SKILL.md) decides maturity transitions across the library.
- [`skill-deprecation-manager`](../skill-deprecation-manager/SKILL.md) executes one concrete deprecation: notice, reference updates, archive move, registry updates.

This is not a duplication problem. It is a handoff problem.

Recommendation:

- Keep both.
- Make lifecycle explicitly hand off approved deprecations to deprecation-manager.

### 5. Skills That Look Similar But Should Stay Separate

These pairs share vocabulary, but not enough actual function to justify merging:

| Packages | Why they are similar | Why they should stay separate |
| --- | --- | --- |
| `skill-safety-review`, `skill-anti-patterns` | Both audit a skill | Safety review is about harm, permissions, injection, destructive actions, and partial-failure risk. Anti-patterns is about routing quality, structural clarity, and maintainability. |
| `skill-adaptation`, `skill-variant-splitting` | Both change a skill’s shape to fit context | Adaptation ports one skill to a new environment while preserving identity. Variant splitting decomposes one over-broad identity into multiple narrower skills. |
| `skill-reference-extraction`, `skill-improver` | Both can slim or improve a package | Reference extraction has a narrow, threshold-based transformation contract. `skill-improver` is a broad rewrite/upgrade skill. |
| `skill-benchmarking`, `skill-evaluation` | Both use cases, metrics, and baselines | Benchmarking is comparative selection across variants; evaluation is a health verdict for one skill. |
| `skill-catalog-curation`, `skill-registry-manager` | Both scan the whole library | Curation should analyze and recommend; registry manager should update canonical catalog artifacts. |

## Recommended Action Plan

1. Merge `skill-trigger-optimization` and `skill-description-optimizer`.
2. Merge `provenance-audit` and `skill-provenance` into a dual-mode provenance package.
3. Merge `skill-eval-runner` into `skill-evaluation`.
4. Choose one canonical eval artifact schema and align `skill-testing-harness`, `skill-evaluation`, `skill-improver`, and any remaining suite-runner behavior to it.
5. Refactor the packaging family so `skill-packaging` is the single-skill primitive and `skill-packager` is the multi-skill release wrapper.
6. Decide whether `overlay-generator` is frequent enough as a standalone task; if not, absorb it into the packaging family.
7. Rewrite boundaries for `skill-catalog-curation` vs `skill-registry-manager` and for `skill-lifecycle-management` vs `skill-deprecation-manager`.
8. Reclassify `community-skill-harvester` as acquisition/adoption rather than creation.
9. Reposition `anthropiuc-skill-creator` as the umbrella full-lifecycle orchestrator rather than a second generic “creator” skill.

## Appendix: Per-Skill Disposition

| Skill | Closest overlap | Disposition |
| --- | --- | --- |
| `overlay-generator` | `skill-packaging`, `skill-packager` | Absorb into packaging family or sharply narrow to overlay-only tasks. |
| `provenance-audit` | `skill-provenance` | Merge into dual-mode provenance package. |
| `anthropiuc-skill-creator` | `skill-authoring`, `skill-improver` | Keep, but reposition as full lifecycle orchestrator. |
| `community-skill-harvester` | `skill-authoring` only at the “build vs buy” decision point | Keep, but reclassify as acquisition/import. |
| `microsoft-skill-creator` | `skill-authoring` | Keep separate as a real specialization. |
| `skill-authoring` | `anthropiuc-skill-creator` | Keep as lightweight greenfield authoring. |
| `skill-adaptation` | `skill-variant-splitting` | Keep separate. |
| `skill-anti-patterns` | `skill-safety-review`, `skill-trigger-optimization` | Keep separate. |
| `skill-benchmarking` | `skill-evaluation` | Keep separate. |
| `skill-catalog-curation` | `skill-registry-manager` | Keep, but sharpen boundary to analysis/reporting. |
| `skill-deprecation-manager` | `skill-lifecycle-management` | Keep, but make it the execution stage after lifecycle decisions. |
| `skill-description-optimizer` | `skill-trigger-optimization` | Merge into `skill-trigger-optimization`. |
| `skill-eval-runner` | `skill-evaluation` | Merge into `skill-evaluation`. |
| `skill-evaluation` | `skill-eval-runner` | Keep as canonical evaluator after merge. |
| `skill-improver` | `skill-authoring`, `skill-reference-extraction` | Keep separate, but delegate eval scaffolding to the canonical harness. |
| `skill-installer` | packaging family | Keep separate. |
| `skill-lifecycle-management` | `skill-deprecation-manager` | Keep separate as portfolio-level lifecycle governance. |
| `skill-packager` | `skill-packaging` | Keep only as multi-skill release wrapper, or merge into packaging family. |
| `skill-packaging` | `skill-packager` | Keep as the single-skill packaging primitive. |
| `skill-provenance` | `provenance-audit` | Merge into dual-mode provenance package. |
| `skill-reference-extraction` | `skill-improver` | Keep separate. |
| `skill-registry-manager` | `skill-catalog-curation` | Keep, but sharpen boundary to catalog mutation only. |
| `skill-safety-review` | `skill-anti-patterns` | Keep separate. |
| `skill-testing-harness` | `skill-evaluation`, `skill-improver` | Keep separate, but align to the canonical eval schema. |
| `skill-trigger-optimization` | `skill-description-optimizer` | Keep as the canonical routing-remediation skill after merge. |
| `skill-variant-splitting` | `skill-adaptation` | Keep separate. |

## Bottom Line

The repo does not have blanket duplication. It has a handful of concentrated overlaps:

- routing remediation,
- provenance,
- single-skill evaluation,
- packaging sub-capabilities.

If those are cleaned up, most of the remaining packages become easier to understand because their boundaries are already defensible.
