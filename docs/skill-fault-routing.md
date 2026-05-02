# Skill Fault Routing

Meta Skill Engineering owns the active follow-up path for Archive `skill-fault` evidence. Archive remains the historical evidence store; Meta converts qualifying packets into tracked improvement or creation work.

## Packet Contract

Archive handoff packets use `docs/schemas/skill-fault-ingest-packet.schema.json`.

Required fields:

- `schema_version`: `1`
- `candidate_id`: stable Archive candidate or finding id
- `failure_family`: `skill-fault`, `skill-faults`, or `skill_fault`
- `summary`: actionable evidence summary
- `evidence`: one or more evidence objects with `kind` and at least one of `path`, `url`, or `excerpt`

Optional routing fields:

- `target_skill`: repo-owned skill suspected to need improvement
- `suggested_skill_name`: candidate skill to create if no existing root skill matches
- `source_archive_path`: relative Archive evidence path
- `routing_hints`: trigger/safety/provenance hints from Archive triage

## CLI

```powershell
python scripts/meta-skill-studio.py --mode cli --action ingest-skill-fault --packet path\to\packet.json --format json
```

The action writes a Studio run artifact under `.meta-skill-studio/runs/` and pipeline state, report, and log artifacts under `tasks/pipelines/`.

## Dispositions

- `improve_existing_skill`: `target_skill` matches a repo-owned root skill.
- `create_new_skill`: no existing root skill matches, but `target_skill` or `suggested_skill_name` names a plausible candidate.
- `no_action`: packet is valid but does not identify a skill target or candidate.
- `reject_evidence`: packet validation failed.

Ingestion never promotes a skill. Packaging remains blocked until evaluation, safety review, and provenance review have passed.
