---
name: skill-orchestrator
description: >-
  Orchestrate multi-skill pipelines through the CLI. Use when "run the creation pipeline", 
  "execute the improvement workflow", "resume my pipeline", or when chaining multiple 
  skills with decision points. Supports Creation, Improvement, and Library Management 
  pipelines with state persistence and conditional branching. Do not use for single 
  skill operations (use the skill directly) or exploratory tasks without defined workflow.
---

## Purpose
Execute documented skill pipelines (Creation, Improvement, Library Management) automatically, with state persistence for resume capability and conditional branching based on evaluation results.

## When to use
- Run a full Creation pipeline on a new skill
- Execute Improvement pipeline on existing skills
- Automate Library Management workflows
- Resume interrupted pipelines
- Chain multiple skills with decision points
- User says "run the pipeline", "execute workflow", "resume my pipeline"

## When NOT to use
- Single skill operation needed (use that skill directly)
- Task is exploratory or undefined
- Real-time user input is required at every step

## Procedure

### 1. Pipeline Selection
Identify the target pipeline. See `references/pipeline-definitions.md` for detailed specifications of all 3 pipelines.

**Available Pipelines:**
- **Creation Pipeline** (9 phases)
- **Improvement Pipeline** (4 phases)
- **Library Management Pipeline** (2 phases)

### 2. CLI Interface

**Create a new pipeline:**
```bash
./scripts/meta-skill-studio.py --mode cli --action create --brief "Create a skill for..."
```

**Run a pipeline:**
```bash
./scripts/meta-skill-studio.py --mode cli --action run --pipeline creation --target <skill-name>
```

**Resume a halted pipeline:**
```bash
./scripts/meta-skill-studio.py --mode cli --action resume --run-id <uuid>
```

### 3. Configuration Phase
Load or create pipeline configuration:

```json
{
  "pipeline_id": "uuid-v4-string",
  "pipeline_type": "creation|improvement|library-management",
  "target_skill": "skill-name",
  "start_time": "2026-04-14T10:00:00Z",
  "phases": [
    {
      "phase_id": 1,
      "skill": "skill-creator",
      "status": "pending|running|completed|failed",
      "input": {},
      "output": {},
      "exit_code": null,
      "decision_branch": null
    }
  ],
  "current_phase": 0,
  "resume_from": null
}
```

### 4. Execution Phase
For each phase in the pipeline:

1. **Load State**: Check for existing state file at `.meta-skill-studio/runs/{run_id}/state.json`
2. **Execute Skill**: Run the target skill via CLI action
3. **Capture Output**: Store stdout, stderr, exit code, artifacts
4. **Evaluate Results**:
   - If exit code != 0 → Mark failed, optionally retry or halt
   - If exit code == 0 → Mark completed, check for decision branch
5. **Conditional Branching**: See Decision Rules below
6. **Save State**: Persist progress after each phase
7. **Log Progress**: Write to output panel

### 5. Decision Rules

**Quality Score Branching:**
```
If eval_score < 60:
  Insert skill-improver before next phase
  Set improvement_goal = "Address quality score {eval_score}"
  
If trigger_precision < 0.80:
  Insert skill-trigger-optimization
  Set focus = "description and trigger boundaries"
```

**Failure Handling:**
```
If phase fails (exit_code != 0):
  Option 1: Retry with same input (max 2 retries)
  Option 2: Halt pipeline, mark failed
  Option 3: Skip and continue (if non-critical)
```

### 6. State Persistence
State files stored at `.meta-skill-studio/runs/{run_id}/`:
- `state.json` - Current execution state
- `log.md` - Human-readable log
- `final-report.json` - Completion summary

### 7. Resume Capability
To resume a halted pipeline:
1. Read state file
2. Identify last completed phase
3. Set `current_phase = last_completed + 1`
4. Continue execution from that point

### 8. Completion
When all phases complete:
1. Generate final report with:
   - Total phases executed
   - Success/failure counts
   - Final quality scores
   - Generated artifacts list
2. Update skill lifecycle state if applicable
3. Clean up or archive state files (configurable)

## Output Contract

### Success Output
```json
{
  "pipeline_id": "uuid",
  "status": "completed",
  "phases_executed": 9,
  "phases_successful": 9,
  "phases_failed": 0,
  "final_quality_score": 85,
  "artifacts": [
    "skills/skill-name/SKILL.md",
    "skills/skill-name/evals/trigger-positive.jsonl",
    ...
  ],
  "report_path": ".meta-skill-studio/runs/uuid/final-report.json"
}
```

### Failure Output
```json
{
  "pipeline_id": "uuid",
  "status": "failed",
  "failed_at_phase": 3,
  "failed_skill": "skill-evaluation",
  "error": "Exit code 1: validation failed",
  "resume_possible": true,
  "state_file": ".meta-skill-studio/runs/uuid/state.json"
}
```

## Failure handling

| Problem | Response |
|---------|----------|
| Pipeline config invalid | Report specific error, halt before execution |
| Phase fails with exit code != 0 | Retry up to 2 times, then halt or skip based on config |
| State file corrupted | Report error, cannot resume — start fresh |
| Cannot resume (skill changed) | Warn user, suggest fresh pipeline run |
| Missing required output from previous phase | Halt, report missing artifact |

## References
- `references/pipeline-definitions.md` - Detailed pipeline specifications
- `references/eval-artifact-schema.md` - Eval artifact formats
- `references/conditional-branching-rules.md` - Decision tree documentation

## Next steps

After pipeline completion:
- Review created skill → manual review of SKILL.md
- Install to local library → `skill-installer`
- Evaluate routing quality → `skill-evaluation`
- For failed phases → `skill-improver` or `skill-trigger-optimization`