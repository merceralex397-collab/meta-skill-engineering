# AGENTS.md

This repository is a meta-skill engineering workspace. In the canonical Scafforge workspace it lives at `agent-tools/meta-skill-engineering/`, but it must remain usable as a standalone product. Treat each top-level skill directory as a first-class package with `SKILL.md` as the baseline contract.

## Working Rules

- Prefer direct, factual documentation and implementation notes.
- Keep the root skill inventory limited to the 17 repo-owned top-level skill packages.
- Update root docs when repo-owned skill packages are added, removed, renamed, or materially re-scoped.
- Do not conflate archived material in `skill creator/` with the active inventory.

## Runtime and Studio Integration

- Treat OpenCode as the canonical AI runtime for Meta Skill Studio surfaces and repo automation that needs an agent runtime.
- Repository-level OpenCode defaults live in `.opencode/opencode.json`; do not introduce parallel runtime-selection guidance for Codex, Gemini, Copilot, or other CLIs in active docs/UI.
- `.opencode/skills/` is an OpenCode mirror for selected repo-owned skills, not the authoritative root inventory.
- `LibraryUnverified/` and `LibraryWorkbench/` are corpus/library areas, not repo-owned root skill packages.

## Surface Authority

- The authoritative headless execution surface is `scripts/meta-skill-studio.py --mode cli`.
- `scripts/meta_skill_studio/app.py` (`StudioCore`) is the shared workflow backend that UI shells must align to.
- `docs/cli/action-contract.md` is the published CLI contract; `docs/cli/feature-inventory.md` is the audit baseline.
- TUI, tkinter GUI, and `windows-wpf/` are convenience shells layered on the same workflow truth, not competing contracts.
- `scripts/meta_skill_studio/opencode_sdk_bridge.mjs` is an assistant-chat helper, not the authoritative workflow surface.

## WPF Release Validation

- For `windows-wpf/` changes, `dotnet build` and `dotnet test` are not enough to claim completion.
- Before marking WPF work complete, run `windows-wpf\build-release.ps1` and confirm the publish smoke test passes.
- The smoke test must launch `windows-wpf\publish\MetaSkillStudio.exe`, verify it stays alive briefly, and fail on any matching `.NET Runtime`, `Application Error`, or `Windows Error Reporting` events.
- Treat startup XAML/resource failures as blocking release issues even when compile and unit tests pass.

## Skill Package Shape

- Every repo-owned skill package must contain `SKILL.md`.
- A richer package may also include `references/`, `scripts/`, `evals/`, `assets/`, or `agents/`.
- When a package has evals or scripts, treat them as support layers for the skill rather than as the skill itself.
- Skills are internal-only; do not add license, compatibility, or release metadata unless explicitly needed.

## SKILL.md Structure

All skills should follow this section order:
1. YAML frontmatter (name, description)
2. Purpose
3. When to use / When NOT to use (use these exact heading names)
4. Procedure
5. Output contract
6. Failure handling
7. Next steps (workflow pointers to related skills)
8. References (optional — only include when skill-specific references exist)

## Pipelines

### Creation Pipeline
```
community-skill-harvester → skill-creator → skill-testing-harness → skill-evaluation
    → skill-trigger-optimization → skill-safety-review → skill-provenance
    → skill-packaging → skill-installer → skill-lifecycle-management
```

### Discovery Pipeline
```
community-skill-harvester → skill-evaluation → skill-safety-review
    → skill-provenance → skill-packaging → skill-installer
    → skill-lifecycle-management
```

### Improvement Pipeline
```
skill-anti-patterns → skill-improver → skill-evaluation → skill-trigger-optimization
```

### Library Management Pipeline
```
skill-catalog-curation → skill-lifecycle-management
```

### Auxiliary Skills

- `skill-adaptation` — on-demand intervention tool for adapting skills to new contexts
- `skill-variant-splitting` — on-demand intervention tool for splitting skills into variants
- `skill-benchmarking` — used within the evaluation stage for quality measurement
- `skill-orchestrator` — the pipeline engine that coordinates end-to-end workflows

## Entry Points

| Goal | Start here |
|------|-----------|
| Create a new skill | `skill-creator` |
| Improve an existing skill | `skill-anti-patterns` → `skill-improver` |
| Audit the skill library | `skill-catalog-curation` |
| Find external skills | `community-skill-harvester` |

## Using External References

- Use the Agent Skills site as the external reference model:
  - [Agent Skills home](https://agentskills.io/)
  - [What are skills?](https://agentskills.io/what-are-skills)
  - [Specification](https://agentskills.io/specification)
- Prefer the repo's own patterns when documenting or extending skills.

## Inventory Boundaries

- Root inventory includes only the 17 skill packages at the repository root.
- `skill creator/` is archived source material from the pre-consolidation state.
- `tasks/` is documentation, worklogs, and reviews — not a skill package.
- `scripts/` contains automation scripts for running evals and orchestration.

## Available Root Scripts

| Script | Purpose |
|------|-----------|
| `scripts/meta-skill-studio.py` | Authoritative CLI/TUI/GUI entrypoint |
| `scripts/validate-skills.sh` | Structural validator for repo-owned root skills |
| `scripts/run-evals.sh` | JSONL eval runner |
| `scripts/pre-commit-check.sh` | Local pre-commit checks |
| `scripts/nightly-full-test.sh` | Nightly-oriented repository test wrapper |
| `scripts/regression-alert.sh` | Regression alert helper |
| `scripts/run-meta-skill-cycle.sh` | Experimental orchestration helper; not part of the authoritative CLI contract |
