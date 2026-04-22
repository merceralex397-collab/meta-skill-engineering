# Evaluation cadence

This is the **current** evaluation cadence for the repository as hardened in plan 13.

## Core evaluation surfaces

| Surface | Use it for | Primary artifact |
| --- | --- | --- |
| `scripts/validate-skills.sh` | Fast structural validation of repo-owned root skills | terminal output, optional Studio run artifact via `validate-skills` |
| `scripts/run-evals.sh` | JSONL trigger/behavior eval execution | `eval-results/` |
| `python scripts/meta-skill-studio.py --mode cli --action evaluate-skill --format json` | Orchestrated validation + eval + judge summary + measurement plan + improvement brief | `.meta-skill-studio/runs/<timestamp>-evaluate-skill.json` |
| `python scripts/meta-skill-studio.py --mode cli --action compare-runs --before-run ... --after-run ... --format json` | Before/after comparison | `.meta-skill-studio/runs/<timestamp>-compare-runs.json` |
| `python scripts/meta-skill-studio.py --mode cli --action improvement-brief --run-file ... --format json` | Turn one prior run into a focused fix backlog | `.meta-skill-studio/runs/<timestamp>-improvement-brief.json` |

## Recommended cadence

### 1. Structural check

```bash
python scripts/meta-skill-studio.py --mode cli --action validate-skills --format json
```

Use this after changing any repo-owned `SKILL.md`.

### 2. Targeted eval run

```bash
python scripts/meta-skill-studio.py --mode cli --action evaluate-skill --skill skill-improver --format json
```

This wraps:

1. `scripts/validate-skills.sh`
2. `scripts/run-evals.sh`
3. a judge pass that produces a structured improvement brief

### 3. Compare before/after

```bash
python scripts/meta-skill-studio.py --mode cli --action compare-runs --before-run .meta-skill-studio/runs/<before>.json --after-run .meta-skill-studio/runs/<after>.json --format json
```

### 4. Rehydrate a fix backlog

```bash
python scripts/meta-skill-studio.py --mode cli --action improvement-brief --run-file .meta-skill-studio/runs/<run>.json --format json
```

## Artifact expectations

- `eval-results/` remains the direct output location for `scripts/run-evals.sh`.
- `.meta-skill-studio/runs/` is the stable machine-readable record for orchestrated Studio actions.
- `evaluate-skill` artifacts contain `summary`, `measurements`, `measurement_plan`, and `improvement_brief`.

## What is intentionally not claimed

- There is no repo-wide full-cycle runner in the current root `scripts/` inventory.
- Plugin-eval token-budget grading is not treated as a universal repo-wide gate.
- WPF is not required for any of the evaluation steps above.
