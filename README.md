# Meta Skill Engineering

Meta Skill Engineering is a **headless-first skill-engineering platform** for creating, evaluating, improving, packaging, installing, and governing agent skills.
Within the canonical Scafforge workspace this repo lives at `agent-tools/meta-skill-engineering/`, but it remains a standalone product when cloned and operated directly.

Inside the wider ecosystem it owns the `skill-faults` research path: when skills are weak, misleading, missing, or not being triggered appropriately, the retained evidence should route here for evaluation, improvement, and possible promotion into verified library skills.

The repository contains **17 repo-owned root skill packages** plus the automation surfaces used to operate them. The authoritative execution path is the **Python Studio CLI**; TUI, tkinter GUI, and WPF are convenience shells layered on top of the same workflow truth.

## Authoritative surfaces

- **Headless execution:** `python scripts/meta-skill-studio.py --mode cli`
- **CLI contract:** `docs/cli/action-contract.md`
- **Feature inventory:** `docs/cli/feature-inventory.md`
- **Surface authority:** `docs/architecture/surface-authority.md`
- **Evaluation disposition:** `docs/evaluation/plugin-eval-disposition.md`

## Quick agent/operator start
```bash
python scripts/meta-skill-studio.py --mode cli --action list-actions --format json
python scripts/meta-skill-studio.py --mode cli --action list-skills --format json
python scripts/meta-skill-studio.py --mode cli --action validate-skills --format json
```

Run artifacts are stored in `.meta-skill-studio/runs/`. Pipeline state and final reports are stored in `tasks/pipelines/`.

## Repository layout

- `./<skill-name>/` — the 17 repo-owned root skill packages
- `scripts/` — authoritative automation entrypoints and Studio backend package
- `docs/` — operational/reference documentation
- `LibraryUnverified/` — imported or raw skills awaiting validation
- `LibraryWorkbench/` — skills under active evaluation or benchmark work
- `Library/` — verified library tier
- `windows-wpf/` — Windows-native convenience shell and packaging path
- `skill creator/` — archived pre-consolidation material
- `tasks/` — worklogs, reviews, and orchestrator pipeline state/report artifacts

## Meta Skill Studio surfaces

### Python Studio CLI — authoritative

The Python CLI is the required contract for headless agents and automation. Canonical workflow families include:

- authoring: `create`, `improve`
- evaluation: `validate-skills`, `run-evals`, `evaluate-skill`, `benchmark-skill`, `compare-runs`, `improvement-brief`
- library/catalog: `find-skills`, `import-skill`, `promote-skill`, `demote-skill`, `move-skill`, `meta-manage`, `catalog-audit`
- governance/distribution: `safety-review`, `provenance-review`, `package-skill`, `install-skill`, `lifecycle-review`
- orchestration: `run-pipeline`, `resume-pipeline`
- introspection/runtime: `list-actions`, `list-skills`, `list-runs`, `show-run`, `list-models`, `list-providers`, `auth-provider`, `opencode-stats`

Prefer `--format json` for machine-readable output.

### TUI and tkinter GUI — convenience shells

- `python scripts/meta-skill-studio.py`
- `python scripts/meta-skill-studio.py --mode gui`

These are useful local shells, but they are **not** the source of workflow truth.

### Windows WPF — convenience shell and delivery path

The supported Windows app path lives in `windows-wpf/`. It provides:

- native Windows shell UX
- bundled workspace release builds
- MSI packaging
- inline assistant, analytics, provider/model, import, and library views

See `windows-wpf/README.md` for build and publish guidance. WPF remains layered on the same documented workflow contract; it is not the only real product path.

## Root skill inventory

| Folder | Purpose |
| --- | --- |
| `community-skill-harvester` | Find external skills from public registries and evaluate them for adoption. |
| `skill-adaptation` | Rewrite a skill's context-dependent references for a new environment. |
| `skill-anti-patterns` | Scan `SKILL.md` for concrete anti-patterns and report fixes. |
| `skill-benchmarking` | Compare skill variants on the same test cases. |
| `skill-catalog-curation` | Audit library organization, duplicates, and gaps. |
| `skill-creator` | Create new agent skills from scratch. |
| `skill-evaluation` | Evaluate routing accuracy and output quality. |
| `skill-improver` | Improve an existing skill package and its support layers. |
| `skill-installer` | Install a skill package into a local agent client skill directory. |
| `skill-lifecycle-management` | Manage lifecycle state, promotion, deprecation, and retirement. |
| `skill-orchestrator` | Coordinate documented multi-skill workflows. |
| `skill-packaging` | Bundle skills into archives or delivery artifacts. |
| `skill-provenance` | Audit origin, authorship, trust, and supporting evidence. |
| `skill-safety-review` | Audit a skill for safety hazards before adoption or release. |
| `skill-testing-harness` | Build JSONL eval suites and test infrastructure. |
| `skill-trigger-optimization` | Improve routing through description and boundary changes. |
| `skill-variant-splitting` | Split broad skills into focused variants. |

## Pipelines

### Creation

```text
community-skill-harvester → skill-creator → skill-testing-harness → skill-evaluation
    → skill-trigger-optimization → skill-safety-review → skill-provenance
    → skill-packaging → skill-installer → skill-lifecycle-management
```

### Discovery

```text
community-skill-harvester → skill-evaluation → skill-safety-review
    → skill-provenance → skill-packaging → skill-installer
    → skill-lifecycle-management
```

### Improvement

```text
skill-anti-patterns → skill-improver → skill-evaluation → skill-trigger-optimization
```

### Library management

```text
skill-catalog-curation → skill-lifecycle-management
```

## Available root scripts

| Script | Purpose |
| --- | --- |
| `scripts/meta-skill-studio.py` | Authoritative CLI/TUI/GUI entrypoint |
| `scripts/validate-skills.sh` | Structural validator for repo-owned root skills |
| `scripts/run-evals.sh` | JSONL eval runner |
| `scripts/pre-commit-check.sh` | Local pre-commit checks |
| `scripts/nightly-full-test.sh` | Nightly-oriented repository test wrapper |
| `scripts/regression-alert.sh` | Regression alert helper |
| `scripts/run-meta-skill-cycle.sh` | Experimental orchestration helper, not part of the authoritative contract |

## Evaluation posture

- JSONL fixture-driven evals remain the base testing model.
- `evaluate-skill` now emits a versioned run artifact with a measurement plan and improvement brief.
- `compare-runs` and `improvement-brief` turn evaluation output into reusable follow-up artifacts.
- Full plugin-eval cost/budget machinery is **not** applied blindly repo-wide; see `docs/evaluation/plugin-eval-disposition.md`.

## Contributing

Read `AGENTS.md` first. When changing CLI, scripts, or workflow contracts, keep `README.md`, `AGENTS.md`, and `.github/copilot-instructions.md` aligned with the implementation.
