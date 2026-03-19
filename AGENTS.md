# AGENTS.md

This repository is a meta-skill engineering workspace. Treat each top-level skill directory as a first-class package with `SKILL.md` as the baseline contract.

## Working Rules

- Prefer direct, factual documentation and implementation notes.
- Keep the root skill inventory limited to the 16 repo-owned top-level skill packages.
- Update root docs when repo-owned skill packages are added, removed, renamed, or materially re-scoped.
- Do not conflate archived material in `skill creator/` with the active inventory.

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

### Improvement Pipeline
```
skill-anti-patterns → skill-improver → skill-evaluation → skill-trigger-optimization
```

### Library Management Pipeline
```
skill-catalog-curation → skill-lifecycle-management
```

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

- Root inventory includes only the 16 skill packages at the repository root.
- `skill creator/` is archived source material from the pre-consolidation state.
- `tasks/` is documentation, worklogs, and reviews — not a skill package.
- `scripts/` contains automation scripts for running evals and orchestration.
