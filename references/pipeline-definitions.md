# Pipeline Definitions

This document defines the standardized pipelines used throughout the Meta Skill Engineering repository for skill creation, improvement, and library management.

## Overview

Pipelines orchestrate multiple skills in sequence, with state persistence for resume capability and conditional branching based on evaluation results.

---

## 1. Creation Pipeline

**Purpose:** Create a new skill from scratch through full validation and packaging.

**Phases:** 9 phases

### Flow

```
skill-creator 
    → skill-testing-harness 
    → skill-evaluation 
    → skill-trigger-optimization 
    → skill-safety-review 
    → skill-provenance 
    → skill-packaging 
    → skill-installer 
    → skill-lifecycle-management
```

### Phase Details

| Phase | Skill | Purpose | Exit Conditions |
|-------|-------|---------|-----------------|
| 1 | skill-creator | Generate initial SKILL.md from brief | Success → Phase 2; Failure → Halt |
| 2 | skill-testing-harness | Create eval infrastructure (JSONL tests) | Success → Phase 3; Failure → Retry or Halt |
| 3 | skill-evaluation | Run tests and evaluate quality | Score ≥ 60 → Phase 4; Score < 60 → Branch to Improver |
| 4 | skill-trigger-optimization | Fix routing description and boundaries | Success → Phase 5; Failure → Branch to Improver |
| 5 | skill-safety-review | Audit for safety hazards | Pass → Phase 6; Fail → Halt |
| 6 | skill-provenance | Record origin and trust metadata | Success → Phase 7 |
| 7 | skill-packaging | Bundle with manifest and overlays | Success → Phase 8 |
| 8 | skill-installer | Install to local agent | Success → Phase 9 |
| 9 | skill-lifecycle-management | Set lifecycle state to "active" | Complete |

### Conditional Branching

```
If eval_score < 60:
    Insert skill-improver before Phase 4
    Set goal = "Address quality score {eval_score}"
    
If trigger_precision < 0.80:
    Insert skill-trigger-optimization
    Set focus = "description and trigger boundaries"
```

### Output Artifacts

- `skills/<skill-name>/SKILL.md`
- `skills/<skill-name>/evals/trigger-positive.jsonl`
- `skills/<skill-name>/evals/output-tests.jsonl`
- `skills/<skill-name>/references/` (if applicable)
- `skills/<skill-name>/scripts/` (if applicable)
- Packaged archive in `dist/`

---

## 2. Improvement Pipeline

**Purpose:** Improve an existing skill based on identified issues or goals.

**Phases:** 4 phases

### Flow

```
skill-anti-patterns 
    → skill-improver 
    → skill-evaluation 
    → skill-trigger-optimization
```

### Phase Details

| Phase | Skill | Purpose | Exit Conditions |
|-------|-------|---------|-----------------|
| 1 | skill-anti-patterns | Scan for structural anti-patterns | Issues found → Phase 2; No issues → Skip to Phase 3 |
| 2 | skill-improver | Fix identified issues | Success → Phase 3 |
| 3 | skill-evaluation | Validate improvements | Score ≥ 70 → Phase 4; Score < 70 → Branch to Improver |
| 4 | skill-trigger-optimization | Verify routing accuracy | Complete |

### Entry Points

The Improvement Pipeline can be entered from:
- Manual improvement request
- Anti-pattern scan results
- Failed evaluation in Creation Pipeline
- Periodic quality review

### Conditional Branching

```
If anti_patterns_found == 0 AND no_improvement_goal:
    Skip Phase 2, go directly to Phase 3 (re-evaluation)
    
If eval_score < previous_score:
    Rollback changes
    Halt with failure status
```

---

## 3. Library Management Pipeline

**Purpose:** Audit, organize, and maintain the skill library.

**Phases:** 2 phases

### Flow

```
skill-catalog-curation → skill-lifecycle-management
```

### Phase Details

| Phase | Skill | Purpose | Exit Conditions |
|-------|-------|---------|-----------------|
| 1 | skill-catalog-curation | Audit for duplicates, gaps, coverage | Success → Phase 2 |
| 2 | skill-lifecycle-management | Apply lifecycle transitions | Complete |

### Operations

**Catalog Curation:**
- Detect duplicate skills
- Identify gaps in coverage
- Update catalog index
- Maintain registry

**Lifecycle Management:**
- Promote skills (unverified → workbench → production)
- Deprecate outdated skills
- Archive retired skills
- Update maturity states

---

## Pipeline State Format

Pipeline state is persisted as JSON:

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
      "decision_branch": null,
      "start_time": null,
      "end_time": null
    }
  ],
  "current_phase": 0,
  "resume_from": null,
  "quality_score": null,
  "artifacts": []
}
```

---

## Resume Capability

To resume a halted pipeline:

1. Read state file at `tasks/pipelines/{pipeline_id}-state.json`
2. Identify last completed phase
3. Set `current_phase = last_completed + 1`
4. Continue execution from that point

### Resume Conditions

- Resume possible: Phase failed but state saved
- Resume not possible: Corrupted state or Phase 1 (skill-creation) failed

---

## Decision Rules

### Quality Score Branching

```
If eval_score < 60:
    Insert skill-improver before next phase
    Set improvement_goal = "Address quality score {eval_score}"
    
If trigger_precision < 0.80:
    Insert skill-trigger-optimization
    Set focus = "description and trigger boundaries"
    
If safety_review_failed:
    Halt pipeline
    Require manual intervention
```

### Failure Handling

```
If phase fails (exit_code != 0):
    Option 1: Retry with same input (max 2 retries)
    Option 2: Halt pipeline, mark failed
    Option 3: Skip and continue (if non-critical)
```

---

## Pipeline Execution

### Configuration

Pipelines are configured via JSON:

```json
{
  "pipeline_type": "creation",
  "target_skill": "my-new-skill",
  "input": {
    "brief": "Create a skill for...",
    "target_library": "LibraryUnverified"
  },
  "options": {
    "auto_branching": true,
    "max_retries": 2,
    "halt_on_failure": true
  }
}
```

### Execution

```bash
# Run pipeline
python scripts/run_pipeline.py --config pipeline-config.json

# Resume pipeline
python scripts/resume_pipeline.py --pipeline-id <uuid>

# Check status
python scripts/pipeline_status.py --pipeline-id <uuid>
```

---

## References

- `skill-orchestrator/SKILL.md` - Orchestration skill documentation
- `references/eval-artifact-schema.md` - Eval artifact formats
- `references/conditional-branching-rules.md` - Detailed decision trees

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-04-14 | Initial pipeline definitions |
