# AGENTS.md

This repository is a meta-skill engineering workspace. Treat each top-level skill directory as a first-class package with `SKILL.md` as the baseline contract.

## Working Rules

- Prefer direct, factual documentation and implementation notes.
- Keep the root skill inventory limited to the 26 repo-owned top-level skill packages.
- Update root docs when repo-owned skill packages are added, removed, renamed, or materially re-scoped.
- Treat `foundskills/` as reference-only corpus material. It is useful for studying imported skill techniques, especially for `skill-creator`, but it is not part of the root inventory.
- Do not conflate imported samples in `foundskills/` with repo-authored skill packages.

## Skill Package Shape

- Every repo-owned skill package must contain `SKILL.md`.
- A richer package may also include `manifest.yaml`, `references/`, `scripts/`, `evals/`, `assets/`, `agents/`, `overlays/`, `README.md`, or `CHANGELOG.md`.
- When a package has a manifest, keep its metadata aligned with `SKILL.md` and the package's supporting files.
- When a package has evals or scripts, treat them as support layers for the skill rather than as the skill itself.

## Using External References

- Use the Agent Skills site as the external reference model:
  - [Agent Skills home](https://agentskills.io/)
  - [What are skills?](https://agentskills.io/what-are-skills)
  - [Specification](https://agentskills.io/specification)
- Prefer the repo's own patterns when documenting or extending skills.
- If a change depends on imported examples in `foundskills/`, say so explicitly and keep the dependency limited to that corpus.

## Inventory Boundaries

- Root inventory includes only the skill packages at the repository root.
- `tasks/` is documentation and worklog material, not a skill package.
- `foundskills/` is a separate corpus root and should be documented separately from the active inventory.

