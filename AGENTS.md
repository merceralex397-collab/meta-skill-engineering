# AGENTS.md

This repository is a meta-skill engineering workspace. Treat each top-level skill directory as a first-class package with `SKILL.md` as the baseline contract.

## Working Rules

- Prefer direct, factual documentation and implementation notes.
- Keep the root skill inventory limited to the 20 repo-owned top-level skill packages.
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
3. When to use / When NOT to use
4. Procedure
5. Output contract
6. Failure handling
7. Next steps (workflow pointers to related skills)
8. References (optional)

## Skill Workflow

The standard skill lifecycle follows this pipeline:
1. **Create** → `skill-creator`
2. **Test** → `skill-testing-harness`
3. **Evaluate** → `skill-evaluation`
4. **Benchmark** (if variants) → `skill-benchmarking`
5. **Optimize triggers** → `skill-trigger-optimization`
6. **Review safety** → `skill-safety-review`
7. **Record provenance** → `skill-provenance`
8. **Package** → `skill-packaging`
9. **Install** → `skill-installer`
10. **Manage lifecycle** → `skill-lifecycle-management`

## Using External References

- Use the Agent Skills site as the external reference model:
  - [Agent Skills home](https://agentskills.io/)
  - [What are skills?](https://agentskills.io/what-are-skills)
  - [Specification](https://agentskills.io/specification)
- Prefer the repo's own patterns when documenting or extending skills.

## Inventory Boundaries

- Root inventory includes only the 20 skill packages at the repository root.
- `skill creator/` is archived source material from the pre-consolidation state.
- `tasks/` is documentation, worklogs, and reviews — not a skill package.
