# Meta Skill Engineering

A meta-skill engineering workspace containing 12 skills that create, refine, test, and govern agent skills.

## Repository Layout

- `./<skill-name>/` — repo-owned skill packages at the repository root. Each package has a `SKILL.md` baseline contract and may include `references/`, `scripts/`, `evals/`, or `assets/`.
- `archive/` — skills removed from the active inventory (distribution-oriented skills).
- `corpus/` — test skills for evaluating meta-skills: 5 weak, 5 strong, 5 adversarial, plus a regression directory for harvested failures.
- `scripts/` — automation scripts (eval runner, validation, optimization, corpus evaluation). Root scripts are source-of-truth copies; per-skill `scripts/` directories contain deployed copies via `sync-to-skills.sh`.
- `eval-results/` — timestamped eval reports; `<skill>-eval.md` symlinks to latest. Handoff mechanism between `skill-evaluation` and `skill-improver`.
- `docs/` — operational documentation including `evaluation-cadence.md`.
- `VerifiedSkills/` — benchmarked candidates from workbench/LibraryUnverified/ that have passed rigorous evaluation.
- `workbench/` — unverified skill candidates organized by domain. See LibraryUnverified/ for candidate skills awaiting benchmarking.

## Pipelines

Three built-in flows connect the skills:

### Creation Pipeline
```
skill-creator → skill-testing-harness → skill-evaluation
    → skill-trigger-optimization → skill-safety-review → skill-lifecycle-management
```

### Improvement Pipeline
```
skill-evaluation → skill-anti-patterns → skill-improver → skill-trigger-optimization
```
Evaluation first (establish baseline), anti-patterns second (diagnose), improver third (fix), trigger-optimization fourth (polish routing). Eval results in `eval-results/` serve as the data handoff between steps.

### Library Management Pipeline
```
skill-catalog-curation → skill-lifecycle-management
```

## Entry Points

| Goal | Start here |
|------|-----------|
| Create a new skill | `skill-creator` |
| Improve an existing skill | `skill-evaluation` (baseline) → `skill-anti-patterns` (diagnose) → `skill-improver` (fix) |
| Evaluate a skill | `skill-evaluation` |
| Audit the skill library | `skill-catalog-curation` |

## Skill Inventory

| Folder | Purpose |
| --- | --- |
| `skill-adaptation` | Rewrite a skill's context-dependent references for a new environment. |
| `skill-anti-patterns` | Scan SKILL.md for concrete anti-patterns and report fixes. |
| `skill-benchmarking` | Compare skill variants on the same test cases. |
| `skill-catalog-curation` | Audit library for duplicates and gaps; maintain catalog index. |
| `skill-creator` | Create new agent skills from scratch and iterate through test-review-improve cycles. |
| `skill-evaluation` | Evaluate a single skill's routing accuracy, output quality, and baseline value. |
| `skill-improver` | Improve an existing skill package — routing, procedure, support layers. |
| `skill-lifecycle-management` | Manage skills through lifecycle states; execute deprecation and retirement. |
| `skill-safety-review` | Audit a skill for safety hazards before publication or import. |
| `skill-testing-harness` | Build test infrastructure (JSONL eval suites) for a skill. |
| `skill-trigger-optimization` | Fix skill routing by rewriting description and boundary text. |
| `skill-variant-splitting` | Split a broad skill into focused variants. |

## Skill Categories

**Creation & Improvement**
- `skill-creator` — create new skills
- `skill-improver` — improve existing skills (includes reference extraction)

**Quality & Testing**
- `skill-testing-harness` — build test infrastructure
- `skill-evaluation` — evaluate routing and output quality
- `skill-benchmarking` — compare skill variants
- `skill-anti-patterns` — audit for structural anti-patterns
- `skill-trigger-optimization` — fix routing descriptions

**Safety**
- `skill-safety-review` — audit for safety hazards

**Library Management**
- `skill-catalog-curation` — audit library, maintain catalog index
- `skill-lifecycle-management` — manage lifecycle states, deprecation, and retirement

**Transformation**
- `skill-adaptation` — port skills to new environments
- `skill-variant-splitting` — split broad skills into focused variants

## Evaluation System

The eval system uses OpenCode SDK to test skills with real model responses via session-based evaluation.

| Script | Purpose |
|--------|---------|
| `scripts/opencode-eval.sh` | Trigger and behavior tests with pass/fail gates (`--observe` routing, `--runs N` majority voting) |
| `scripts/opencode-trigger-opt.sh` | Automated trigger optimization with train/test split |
| `scripts/opencode-corpus-eval.sh` | Two-layer meta-skill evaluation against corpus |
| `scripts/opencode-full-cycle.sh` | Full 5-step evaluation cadence |
| `scripts/opencode-meta-cycle.sh` | Meta-skill improvement cycle orchestration |
| `scripts/validate-skills.sh` | Structural compliance check for all 12 skills |
| `scripts/check_skill_structure.py` | 10-point structural scoring for a skill |
| `scripts/check_preservation.py` | Jaccard similarity for content preservation |
| `scripts/skill_lint.py` | Lint a SKILL.md for format issues |
| `scripts/harvest_failures.py` | Convert failures into regression cases |
| `scripts/sync-to-skills.sh` | Sync root scripts to per-skill `scripts/` directories |

**Key capabilities:**
- **Observe routing** (default): uses OpenCode session observation to detect actual SKILL.md file reads
- **Multi-run voting**: `--runs N` runs each prompt N times with majority-vote pass/fail
- **Default model**: `minimax-coding-plan/Minimax-M2.7` (override with `EVAL_MODEL`)
- **Trigger optimization**: 60/40 train/test split, LLM-proposed improvements, held-out validation

See `docs/evaluation-cadence.md` for the full workflow and environment variables.

## VerifiedSkills

Skills in `workbench/LibraryUnverified/` that have passed rigorous benchmarking live here. Categories are defined by the `categorizer` agent based on SKILL.md content analysis. See `VerifiedSkills/README.md` for the complete inventory.

## Sub-Agents

This project uses OpenCode sub-agents for specialized tasks:

| Agent | Purpose |
|-------|---------|
| `manager` | Orchestrates the repository autonomously, delegates to other agents |
| `categorizer` | Audits LibraryUnverified/ and reorganizes skills into correct categories |
| `evaluator` | Runs evaluation tests against skills using the eval pipeline |
| `performance-monitor` | Permanently monitors performance, auto-applies improvements |

Agents are defined in `.opencode/agents/` and activated via OpenCode's `@agent` syntax.

## SDK Orchestration

Mass parallel evaluation is orchestrated via `@opencode-ai/sdk`:

```bash
node orchestrator.mjs --skills=skill1,skill2 --workers=5
```

The orchestrator creates multiple sessions on a single OpenCode server and distributes eval jobs across workers. See `orchestrator.mjs` for details.
