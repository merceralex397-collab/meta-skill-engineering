# Meta Skill Engineering

This repository is a working area for repo-owned meta-skills that create, refine, test, package, and govern agent skills.

Current state:

- 26 repo-owned skill packages total
- 22 packages live at the repository root
- 4 creation-focused packages live under `skill creator/`
- The imported `foundskills/` corpus has been removed from the active tree

## Repository Layout

- `./<skill-name>/` - repo-owned skill packages at the repository root. Each package has a `SKILL.md` baseline contract and may include supporting files such as `manifest.yaml`, `references/`, `scripts/`, `evals/`, `assets/`, `agents/`, or `overlays/`.
- `skill creator/` - grouped workspace for creation-focused skill packages. It is not itself a skill package.
- `tasks/` - task notes, worklogs, and repo maintenance instructions.

## Root Skill Inventory

The table below covers the 22 repo-owned skill packages at the repository root.

| Folder | Canonical name | Purpose | Support files |
| --- | --- | --- | --- |
| `overlay-generator` | `overlay-generator` | Generate client-specific overlays from a canonical `SKILL.md`. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `provenance-audit` | `provenance-audit` | Audit a skill or artifact origin chain and assign trust. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill-adaptation` | `skill-adaptation` | Rewrite a skill's context-dependent references for a new environment. | `SKILL.md` only |
| `skill-anti-patterns` | `skill-anti-patterns` | Scan `SKILL.md` for concrete anti-patterns and report fixes. | `SKILL.md` only |
| `skill-benchmarking` | `skill-benchmarking` | Compare skill variants on the same test cases. | `SKILL.md` only |
| `skill-catalog-curation` | `skill-catalog-curation` | Detect duplicates, enforce category consistency, and verify discoverability. | `SKILL.md` only |
| `skill-deprecation-manager` | `skill-deprecation-manager` | Safely deprecate, retire, or merge obsolete skills. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill-description-optimizer` | `skill-description-optimizer` | Rewrite a skill description to fix routing problems. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill-eval-runner` | `skill-eval-runner` | Run trigger tests, output tests, and baseline comparisons. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill-evaluation` | `skill-evaluation` | Produce quantitative evidence that a single skill adds value. | `SKILL.md` only |
| `skill-improver` | `skill-improver` | Improve an existing skill package. | `manifest.yaml`, `README.md`, `CHANGELOG.md`, `scripts/`, `references/`, `evals/` |
| `skill-installer` | `skill-installer` | Install a skill package into a local agent client skill directory. | `scripts/`, `assets/` |
| `skill-lifecycle-management` | `skill-lifecycle-management` | Manage skills through draft, beta, stable, deprecated, and archived states. | `SKILL.md` only |
| `skill-packager` | `skill-packager` | Build distributable bundles, manifests, and checksums. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill-packaging` | `skill-packaging` | Bundle a finished skill folder into a distributable archive. | `SKILL.md` only |
| `skill-provenance` | `skill-provenance` | Produce a provenance record for a skill. | `SKILL.md` only |
| `skill-reference-extraction` | `skill-reference-extraction` | Split large reference material out of a `SKILL.md`. | `SKILL.md` only |
| `skill-registry-manager` | `skill-registry-manager` | Maintain the skill library catalog and generate the index. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill-safety-review` | `skill-safety-review` | Audit a skill for safety hazards before publication or import. | `SKILL.md` only |
| `skill-testing-harness` | `skill-testing-harness` | Build test infrastructure for a skill. | `SKILL.md` only |
| `skill-trigger-optimization` | `skill-trigger-optimization` | Fix skill routing by rewriting description and boundary text. | `SKILL.md` only |
| `skill-variant-splitting` | `skill-variant-splitting` | Split a broad skill into focused variants. | `SKILL.md` only |

## Skill Creator Workspace

The table below covers the four creation-focused packages stored under `skill creator/`.

| Folder | Canonical name | Purpose | Support files |
| --- | --- | --- | --- |
| `skill creator/anthropiuc-skill-creator` | `skill-creator` | Create new skills, improve existing skills, and measure skill performance. | `LICENSE.txt`, `agents/`, `assets/`, `eval-viewer/`, `references/`, `scripts/` |
| `skill creator/community-skill-harvester` | `community-skill-harvester` | Find external skills and evaluate them for adoption. | `manifest.yaml`, `references/`, `evals/`, `agents/`, `overlays/` |
| `skill creator/microsoft-skill-creator` | `microsoft-skill-creator` | Create skills for Microsoft technologies using Learn MCP tools. | `references/` |
| `skill creator/skill-authoring` | `skill-authoring` | Create new agent skills in the `SKILL.md` format. | `references/` |
