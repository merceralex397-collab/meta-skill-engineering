# AGENTS.md

This repository is a meta-skill engineering workspace. Treat each top-level skill directory as a first-class package with `SKILL.md` as the baseline contract.

## Working Rules

- Prefer direct, factual documentation and implementation notes.
- Keep the root skill inventory limited to the 12 repo-owned top-level skill packages.
- **Keep root docs current with every commit.** When a commit changes scripts, eval capabilities, skill contracts, or repo structure, update `AGENTS.md`, `README.md`, and `.github/copilot-instructions.md` in the same commit so they never drift from the implemented system.
- Update root docs when repo-owned skill packages are added, removed, renamed, or materially re-scoped.

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

## Skill Package Shape

- Every repo-owned skill package must contain `SKILL.md`.
- A richer package may also include `references/`, `scripts/`, `evals/`, or `assets/`.
- When a package has evals or scripts, treat them as support layers for the skill rather than as the skill itself.
- Skills are internal-only; do not add license, compatibility, or release metadata unless explicitly needed.

## Script Distribution Model

Root `scripts/` contains the **source-of-truth** copies of all automation scripts. Per-skill `scripts/` directories contain **deployed copies** — identical files, same names. Each skill gets only the scripts its procedure actually references.

**Workflow**: Edit a script in root `scripts/` → run `scripts/sync-to-skills.sh` → copies propagate to all skills that use them.

**Sync modes**:
- `./scripts/sync-to-skills.sh` — copy all scripts per manifest
- `./scripts/sync-to-skills.sh --dry-run` — show what would be copied
- `./scripts/sync-to-skills.sh --check` — verify per-skill copies match root (CI-friendly)

The manifest mapping (which scripts go to which skills) is defined at the top of `sync-to-skills.sh`. When adding a new script or changing which skills use it, update the manifest and re-run sync.

## Eval Suite Structure

Every skill package should include an `evals/` directory with these files:

```
evals/
├── trigger-positive.jsonl   # Prompts that SHOULD activate the skill
├── trigger-negative.jsonl   # Prompts that should NOT activate the skill
├── behavior.jsonl           # Output quality checks
└── README.md                # Optional: how to run and extend tests
```

**trigger-positive.jsonl** — one JSON object per line:
```json
{"prompt": "...", "expected": "trigger", "category": "core|indirect|paraphrase|edge", "notes": "..."}
```

**trigger-negative.jsonl** — one JSON object per line:
```json
{"prompt": "...", "expected": "no_trigger", "better_skill": "skill-name-or-null", "notes": "..."}
```

**behavior.jsonl** — one JSON object per line:
```json
{"prompt": "...", "expected_sections": ["..."], "required_patterns": ["..."], "forbidden_patterns": ["..."], "min_output_lines": 15, "notes": "..."}
```

Optional usefulness evaluation fields (for `--usefulness` mode):
```json
{"prompt": "...", "expected_sections": ["..."], "required_patterns": ["..."], "forbidden_patterns": ["..."], "min_output_lines": 15, "notes": "...", "usefulness_criteria": "What 'good' looks like for this case", "usefulness_dimensions": ["correctness", "completeness", "actionability", "conciseness"], "usefulness_threshold": 3}
```

No other eval formats are active. Do not use `evals.json`, `output-tests.jsonl`, `triggers.yaml`, `outputs.yaml`, or `baselines.yaml`.

## SKILL.md Structure

All skills should follow this section order:
1. YAML frontmatter (name, description — these two fields only, no license/metadata/compatibility)
2. Purpose
3. When to use
4. When NOT to use (use this exact heading, not "Do NOT use when:")
5. Procedure (all procedural content goes under this heading, using ## subheadings)
6. Output contract
7. Failure handling
8. Next steps
9. References (optional)

## Pipelines

### Creation Pipeline
```
skill-creator → skill-testing-harness → skill-evaluation
    → skill-trigger-optimization → skill-safety-review → skill-lifecycle-management
```

### Improvement Pipeline
```
skill-evaluation → skill-anti-patterns → skill-improver → skill-trigger-optimization
```

**Eval-results handoff**: `skill-evaluation` produces reports in `eval-results/<skill>-eval.md` with a structured Handoff section (primary failure, failing cases, recommended next skill). `skill-improver` reads these reports in Phase 1 and uses eval signals to drive diagnosis in Phase 2 (eval-driven diagnosis table). When no eval results exist, skill-improver falls back to heuristic diagnosis.

### Library Management Pipeline
```
skill-catalog-curation → skill-lifecycle-management
```

## Entry Points

| Goal | Start here |
|------|-----------|
| Create a new skill | `skill-creator` |
| Improve an existing skill | `skill-evaluation` → `skill-anti-patterns` → `skill-improver` |
| Evaluate a skill | `skill-evaluation` |
| Audit the skill library | `skill-catalog-curation` |

## Inventory Boundaries

- Root inventory includes only the 12 skill packages at the repository root.
- `archive/` contains skills removed from the active inventory (distribution-oriented skills).
- `corpus/` contains test skills for evaluating meta-skills (5 weak, 5 strong, 5 adversarial, 3 regression).
- `scripts/` contains automation scripts for running evals, validation, and optimization.
- `eval-results/` contains timestamped eval reports (markdown) with a `<skill>-eval.md` symlink to the latest. These are the handoff mechanism between `skill-evaluation` and `skill-improver`.
- `docs/` contains operational documentation (evaluation cadence, workflows).
- `VerifiedSkills/` contains benchmarked candidates from workbench/LibraryUnverified/ that have passed rigorous evaluation.
- `.opencode/agents/` contains project-specific sub-agents for specialized tasks.

## Sub-Agents

Project-specific sub-agents are defined in `.opencode/agents/`:

| Agent | Purpose |
|-------|---------|
| `manager` | Orchestrates the repository autonomously, delegates to other agents |
| `categorizer` | Audits LibraryUnverified/ and reorganizes skills into correct categories |
| `evaluator` | Runs evaluation tests against skills using the eval pipeline |
| `performance-monitor` | Permanently monitors performance, auto-applies improvements |

Invoke via OpenCode's `@agent` syntax.

## Evaluation Tooling

The eval system uses OpenCode SDK with structured JSON output for routing detection.

| Script | Purpose |
|--------|---------|
| `scripts/opencode-eval.sh` | Trigger and behavior tests with pass/fail gates (`--observe`/`--strict` routing, `--runs N` majority voting) |
| `scripts/opencode-trigger-opt.sh` | Automated trigger optimization with 60/40 train/test split |
| `scripts/opencode-corpus-eval.sh` | Two-layer meta-skill evaluation against corpus |
| `scripts/opencode-full-cycle.sh` | Full 5-step evaluation cadence |
| `scripts/opencode-meta-cycle.sh` | Meta-skill improvement cycle orchestration |
| `scripts/validate-skills.sh` | Structural compliance check for all 12 skills |
| `scripts/check_skill_structure.py` | 10-point structural scoring for a skill |
| `scripts/check_preservation.py` | Jaccard similarity for content preservation |
| `scripts/skill_lint.py` | Lint a SKILL.md for format issues |
| `scripts/harvest_failures.py` | Convert failures into regression cases |
| `scripts/sync-to-skills.sh` | Sync root scripts to per-skill `scripts/` directories per manifest |

**Default model:** `minimax-coding-plan/Minimax-M2.7`. Override with `EVAL_MODEL` env var.

**OpenCode server:** `http://127.0.0.1:4096` (default). Override with `OPENCODE_SERVER` env var.

**Routing modes:** `--observe` (default) uses OpenCode session observation for SKILL.md file reads.

See `docs/evaluation-cadence.md` for the full evaluation workflow.

## OpenCode SDK Integration

- `.github/copilot-instructions.md` provides project-level instructions.
- `.github/extensions/meta-skill-tools/` provides validation tools (`mse_validate_skill`, `mse_validate_all`, `mse_lint_skill`, `mse_check_preservation`) and an auto-validation hook.
- `orchestrator.mjs` uses `@opencode-ai/sdk` for mass parallel evaluation via multiple OpenCode sessions.
- `.opencode/opencode.json` configures plugins including `opencode-scheduler` for recurring tasks.
