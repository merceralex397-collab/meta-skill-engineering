# Sequencing Anti-Patterns

## Common failure patterns

### Deploy before proving the change in a safe environment

Symptoms:

- tests appear only after deploy
- rollout depends on "monitor and see"

Repair:

- add pre-release verification
- define production canary checks separately

### Migrate schema after code requires it

Symptoms:

- application step assumes columns, tables, or indexes that are created later

Repair:

- split migration and consumer rollout
- use expand-contract where possible

### Parallel lanes touching the same boundary with no merge protocol

Symptoms:

- two teams edit the same service, API, schema, or deployment unit
- no lock, branch strategy, or handoff point is defined

Repair:

- define integration order
- add a single integration owner

### Rollback is "git revert"

Symptoms:

- infrastructure, data, or contract changes claim source control alone is enough

Repair:

- specify state rollback, feature flags, backups, or forward-fix procedure

### Proof is activity-shaped instead of evidence-shaped

Bad:

- "finish implementation"
- "validate manually"
- "ensure everything works"

Good:

- "run migration dry-run on staging and record query output"
- "verify latency, error rate, and auth success rate on canary"
