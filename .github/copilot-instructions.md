# Meta-Skill-Engineering — Copilot Instructions

This is an internal meta-skill engineering workspace. The 17 repo-owned skill packages at the repository root create, refine, test, package, install, orchestrate, and govern agent skills. This is not a distribution package or public library.

## Active Skill Inventory (17 packages)

community-skill-harvester, skill-adaptation, skill-anti-patterns, skill-benchmarking, skill-catalog-curation, skill-creator, skill-evaluation, skill-improver, skill-installer, skill-lifecycle-management, skill-orchestrator, skill-packaging, skill-provenance, skill-safety-review, skill-testing-harness, skill-trigger-optimization, skill-variant-splitting

## Canonical SKILL.md Format

Every SKILL.md must follow this structure exactly:

1. **YAML frontmatter** — `name` and `description` fields only. No license, maturity, compatibility, or other metadata.
2. `# Purpose`
3. `# When to use`
4. `# When NOT to use` — use this exact heading
5. `# Procedure` — all procedural content here, with `##` subheadings
6. `# Output contract`
7. `# Failure handling`
8. `# Next steps`
9. `# References` — optional, only when skill-specific references exist

## Eval Contract

Each skill has an `evals/` directory with exactly these JSONL files:

- `trigger-positive.jsonl` — `{"prompt": "...", "expected": "trigger", "category": "core|indirect|paraphrase|edge", "notes": "..."}`
- `trigger-negative.jsonl` — `{"prompt": "...", "expected": "no_trigger", "better_skill": "skill-name-or-null", "notes": "..."}`
- `behavior.jsonl` — `{"prompt": "...", "expected_sections": [...], "required_patterns": [...], "forbidden_patterns": [...], "min_output_lines": 15, "notes": "..."}`

Optional usefulness fields for `--usefulness` mode: `"usefulness_criteria": "...", "usefulness_dimensions": [...], "usefulness_threshold": N`

No other eval formats are active. Do not use `evals.json`, `output-tests.jsonl`, `triggers.yaml`, `outputs.yaml`, or `baselines.yaml`.

## Surface Authority

- The authoritative headless execution surface is `scripts/meta-skill-studio.py --mode cli`.
- `scripts/meta_skill_studio/app.py` (`StudioCore`) is the shared workflow backend that TUI, tkinter GUI, and WPF should map onto.
- `docs/cli/action-contract.md` is the published CLI verb contract, and `docs/cli/feature-inventory.md` is the audit baseline.
- `windows-wpf/` is a Windows convenience shell and packaging surface, not the only real product path.
- `scripts/meta_skill_studio/opencode_sdk_bridge.mjs` is an assistant-chat helper, not the authoritative create/improve/evaluate/library-management surface.

## Available Scripts

| Script | Purpose |
|--------|---------|
| `scripts/meta-skill-studio.py` | Authoritative CLI/TUI/GUI entrypoint for Meta Skill Studio |
| `scripts/validate-skills.sh` | Validate root skill package structure |
| `scripts/run-evals.sh` | Run JSONL trigger/behavior evals |
| `scripts/pre-commit-check.sh` | Local pre-commit checks |
| `scripts/nightly-full-test.sh` | Nightly-oriented repository test wrapper |
| `scripts/regression-alert.sh` | Regression alert helper |
| `scripts/run-meta-skill-cycle.sh` | **Optional/experimental** — orchestrate a non-authoritative meta-skill cycle |

After editing any SKILL.md, run `scripts/validate-skills.sh` to confirm compliance.

## Key Rules

- Frontmatter must contain only `name` and `description`. No other fields.
- **Keep root docs current with every commit.** When a commit changes scripts, eval capabilities, skill contracts, or repo structure, update `AGENTS.md`, `README.md`, and `.github/copilot-instructions.md` in the same commit so they never drift from the implemented system.
- Do not create `manifest.yaml` in skill packages — it is a stale distribution artifact.
- Do not add license, compatibility, or release metadata to skills.
- `archive/` is read-only historical storage. Do not modify archived skills.
- `corpus/` contains test skills for meta-skill evaluation (5 weak, 5 strong, 5 adversarial, 3 regression). Treat as test fixtures.
- `eval-results/` contains timestamped eval reports with `<skill>-eval.md` symlinks to latest. Handoff between `skill-evaluation` and `skill-improver`.
- `docs/` contains operational documentation (evaluation cadence, workflows).
- `tasks/` is documentation and worklogs, not a skill package.

## Implementation Integrity Rules

These rules exist because an agent previously substituted documentation edits for planned code changes, wrote TODO comments and marked them as completed, claimed files were clean without reading them, and hid incomplete work inside large batched commits. Every rule below addresses a specific observed failure.

### No doc-for-code substitution
When a plan specifies code changes (new CLI flags, functions, scripts, logic), the implementation must modify the code files specified. A documentation-only edit does not satisfy a code change requirement. If you cannot implement the code change, explicitly report it as deferred with a reason — never silently reframe the plan as a documentation task.

### No TODO-as-done
Writing `# TODO: implement X` is not implementing X. A TODO comment is an admission of incomplete work. Never mark an item as done if the implementation contains TODO/FIXME/HACK comments for the core functionality that item requires.

### Verify before claiming resolved
Before marking any finding as "already done," "already clean," or "not applicable," read the relevant file and quote the specific content that proves it. State the file path and line numbers. If you cannot produce a citation, the finding is not resolved.

### No silent scope downgrades
If you choose a simpler approach than what the plan specifies, flag the deviation explicitly and get approval before marking it done. Do not reframe a shortcut as the intended approach.

### Verify sub-agent output against the plan
After a sub-agent completes work, verify its changes against the original plan requirements before committing. For code changes: confirm the planned files were modified and the planned functionality exists (grep for new flags, function names, etc.). Do not commit sub-agent output without this check.

### Small commits per finding
When implementing a plan with numbered findings, commit each finding individually or in small thematic groups (≤3 findings). Each commit message must name the finding IDs it addresses. Do not batch more than 3 findings per commit.

### Doc drift check on every commit
After every commit that changes scripts, skill contracts, or repo structure, diff the script tables in README.md, AGENTS.md, and copilot-instructions.md against `ls scripts/` and flag any mismatch before pushing.

## Pipelines

- **Creation**: skill-creator → skill-testing-harness → skill-evaluation → skill-trigger-optimization → skill-safety-review → skill-lifecycle-management
- **Improvement**: skill-evaluation → skill-anti-patterns → skill-improver → skill-trigger-optimization. Eval results in `eval-results/` are the handoff: skill-evaluation writes structured reports, skill-improver reads them to drive diagnosis.
- **Library Management**: skill-catalog-curation → skill-lifecycle-management

## Entry Points

| Goal | Start here |
|------|-----------|
| Create a new skill | `skill-creator` |
| Improve a skill | `skill-evaluation` → `skill-anti-patterns` → `skill-improver` |
| Evaluate a skill | `skill-evaluation` |
| Audit the library | `skill-catalog-curation` |

## Workflow References

- CLI contract: `docs/cli/action-contract.md`
- Feature inventory: `docs/cli/feature-inventory.md`
- Surface authority: `docs/architecture/surface-authority.md`
- Evaluation disposition: `docs/evaluation/plugin-eval-disposition.md`
