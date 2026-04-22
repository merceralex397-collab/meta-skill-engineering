# Meta Skill Studio CLI action contract

This document defines **complete CLI coverage** for Meta Skill Studio. If a workflow is not reachable through the actions below or a documented support script, the platform is incomplete.

## Authority rules

- **Authoritative headless surface:** `python scripts/meta-skill-studio.py --mode cli`
- **Preferred agent output mode:** add `--format json`
- **Stable run-artifact location:** `.meta-skill-studio/runs/`
- **Pipeline state/report location:** `tasks/pipelines/`
- **Eval report location:** `eval-results/`

Legacy aliases remain for compatibility:

| Alias | Canonical action |
| --- | --- |
| `test` | `evaluate-skill` |
| `benchmarks` | `benchmark-skill` |

## Common flags

| Flag | Meaning |
| --- | --- |
| `--format text|json` | `json` is the machine-readable contract; `text` preserves path-style output for existing shells |
| `--brief` | Creation brief or creation-pipeline brief |
| `--skill` | Target skill name |
| `--goal` | Concrete objective for improvement / governance / distribution actions |
| `--library` | Library tier selector for library-scoped actions |
| `--category`, `--to-category` | Source and target category paths |
| `--from-library`, `--to-library` | Source/target tier selectors |
| `--run-file`, `--before-run`, `--after-run` | Existing Studio run artifact inputs |
| `--pipeline`, `--run-id` | Orchestrator pipeline selection and resume id |

## Canonical actions

### Authoring

| Action | Required args | Output |
| --- | --- | --- |
| `create` | `--brief` | run artifact |
| `improve` | `--skill --goal` | run artifact |

### Evaluation

| Action | Required args | Output |
| --- | --- | --- |
| `validate-skills` | none | run artifact wrapping `scripts/validate-skills.sh` |
| `run-evals` | none or `--skill` | run artifact wrapping `scripts/run-evals.sh` |
| `evaluate-skill` | optional `--skill` | run artifact with validation, eval, judge result, measurement plan, and improvement brief |
| `benchmark-skill` | `--skill --goal` | run artifact plus benchmark JSONL file |
| `compare-runs` | `--before-run --after-run` | run artifact with comparison payload |
| `improvement-brief` | `--run-file` | run artifact with prioritized follow-up list |

### Library and catalog operations

| Action | Required args | Output |
| --- | --- | --- |
| `find-skills` | `--goal` | run artifact |
| `import-skill` | `--source` | run artifact |
| `promote-skill` | `--skill --category --from-library` | run artifact |
| `demote-skill` | `--skill --category --from-library` | run artifact |
| `move-skill` | `--skill --category --to-category --library` | run artifact |
| `meta-manage` | `--goal` | run artifact |
| `catalog-audit` | none or `--goal` | run artifact |

### Governance and distribution

| Action | Required args | Output |
| --- | --- | --- |
| `safety-review` | `--skill` | run artifact |
| `provenance-review` | `--skill` | run artifact |
| `package-skill` | `--skill` | run artifact |
| `install-skill` | `--skill` | run artifact |
| `lifecycle-review` | `--skill` | run artifact |

### Orchestration

| Action | Required args | Output |
| --- | --- | --- |
| `run-pipeline` | `--pipeline` and pipeline-specific inputs (`--brief` for creation, `--skill` for improvement/library-management) | run artifact plus `tasks/pipelines/*.json` |
| `resume-pipeline` | `--run-id` | run artifact plus `tasks/pipelines/*.json` |

### Runtime and introspection

| Action | Required args | Output |
| --- | --- | --- |
| `list-actions` | none | JSON query result |
| `list-skills` | none or `--library` | JSON query result |
| `list-runs` | none | JSON query result |
| `show-run` | `--run-file` | JSON query result |
| `list-models` | none | JSON query result |
| `list-providers` | none | JSON query result |
| `auth-provider` | `--provider` | JSON query result |
| `opencode-stats` | none | JSON query result |

## Stable run-artifact shape

Run-producing actions emit JSON with this top-level contract:

```json
{
  "kind": "meta-skill-studio-run",
  "schema_version": 1,
  "action": "evaluate-skill",
  "created_at": "2026-04-22T07:30:00Z",
  "repo_root": "C:/.../Meta-Skill-Engineering",
  "status": "succeeded",
  "workflow": {},
  "input": {},
  "summary": {},
  "artifacts": {},
  "measurements": {},
  "measurement_plan": null,
  "comparison": null,
  "improvement_brief": null,
  "notes": [],
  "result": {}
}
```

Field expectations:

- `measurement_plan` is populated for orchestrated evaluation actions such as `evaluate-skill`.
- `comparison` is populated by `compare-runs`.
- `improvement_brief` is populated by `evaluate-skill` and `improvement-brief`.
- `result` contains raw command/runtime result payloads for auditability.

## Validation story

The CLI contract is auditable without a GUI:

1. `python scripts/meta-skill-studio.py --help`
2. `python scripts/meta-skill-studio.py --mode cli --action list-actions --format json`
3. `python scripts/meta-skill-studio.py --mode cli --action list-skills --format json`
4. `python scripts/meta-skill-studio.py --mode cli --action validate-skills --format json`
5. Inspect `.meta-skill-studio/runs/` and `tasks/pipelines/` for the documented artifact families

If the action list and the tables above drift apart, the implementation is no longer in contract.
