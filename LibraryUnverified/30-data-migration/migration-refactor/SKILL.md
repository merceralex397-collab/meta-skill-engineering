---
name: migration-refactor
description: Plans and executes large refactors or migrations in bounded Strangler Fig stages with rollback awareness at each phase. Trigger on "migrate from X to Y", "strangler fig", "framework migration", "legacy replacement", "incremental migration". Do NOT use for dependency-upgrades (version bumps), database-persistence (schema-only migrations), or deployment-pipeline (deployment changes).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: migration-refactor
  maturity: draft
  risk: low
  tags: [migration, refactor]
---

# Purpose
Execute large-scale code migrations safely using the Strangler Fig pattern: incrementally replace old implementations while both old and new run in parallel, with clear rollback paths at each stage. This prevents big-bang rewrites that break everything at once.

# When to use this skill
Use when:
- Replacing a legacy system component with a new implementation
- Migrating from one framework/library to another (e.g., REST to GraphQL)
- Refactoring a monolith into services
- Database schema migration with data transformation

Do NOT use when:
- Small refactors that fit in one PR (just do them)
- Greenfield development with no legacy system
- Changes that don't need parallel running or rollback

# Operating procedure
1. **Identify the seam**: Find the boundary where old and new systems meet. This is typically an API surface, database access layer, or routing layer.

2. **Create the facade**:
   ```python
   # Example: Feature-flagged routing
   def get_user(user_id):
       if feature_flags.use_new_user_service(user_id):
           return new_user_service.get(user_id)
       return legacy_user_service.get(user_id)
   ```

3. **Implement new component**: Build the replacement behind the facade. New code must be production-ready but only receives traffic when the flag is enabled.

4. **Enable parallel running**: Route a percentage of traffic to new implementation while logging discrepancies:
   ```python
   def get_user_with_comparison(user_id):
       old_result = legacy_user_service.get(user_id)
       new_result = new_user_service.get(user_id)
       if old_result != new_result:
           log.warning(f"Mismatch for {user_id}", old=old_result, new=new_result)
       return old_result  # Still serving old until validated
   ```

5. **Gradual rollout**: Increase traffic to new system: 1% → 10% → 50% → 100%. At each stage, verify metrics match expectations. Define rollback criteria upfront (error rate > X%, latency > Y ms).

6. **Cut over and clean up**: Once at 100% with confidence, remove the facade, delete old code, and update documentation.

## Migration readiness checklist
Before starting any migration stage, verify:
- [ ] Both old and new implementations have the same test coverage
- [ ] Rollback procedure documented and tested (can revert in <5 min)
- [ ] Monitoring/alerting in place for error rate, latency, and data consistency
- [ ] Feature flag or traffic-split mechanism is working in staging
- [ ] Team agrees on rollback criteria (e.g., error rate >1%, p95 latency >2× baseline)
- [ ] Data migration strategy chosen (expand-contract, dual-write, or snapshot-and-replay)
- [ ] Communication plan for downstream consumers of the migrating component

## Handling database migrations within a refactor
Database changes during a migration require their own staging:
1. **Expand**: Add new columns/tables alongside old ones — no breaking changes
2. **Dual-write**: Application writes to both old and new schemas
3. **Backfill**: Migrate historical data from old to new schema
4. **Cut reads**: Switch reads to new schema, verify correctness
5. **Cut writes**: Stop writing to old schema
6. **Contract**: Drop old columns/tables after verification period

Never combine schema migration with application migration in the same deployment. If the schema change fails, the app rollback must still work against the old schema.

# Output defaults
```markdown
## Migration Plan: [Component Name]

### Seam Location
- File/module: [path]
- Interface: [function/class signatures]

### Stages
1. [ ] Create facade with feature flag
2. [ ] Implement new component behind facade  
3. [ ] Enable comparison logging (0% new traffic)
4. [ ] Rollout: 1% → 10% → 50% → 100%
5. [ ] Remove facade and legacy code

### Rollback Triggers
- Error rate increase > [X]%
- p95 latency increase > [Y]ms
- Data inconsistency detected

### Verification Checklist
- [ ] New component passes all existing tests
- [ ] Comparison logging shows <0.1% discrepancy rate
- [ ] Performance baseline matches or exceeds legacy
```

# References
- https://martinfowler.com/articles/strangler-fig.html

# Failure handling
- **Discrepancies in parallel running**: Log and investigate each case; do not proceed to higher traffic until explained
- **Performance regression in new system**: Roll back traffic, profile, and fix before retrying
- **Feature flag system unavailable**: Ensure flags have safe defaults (typically: serve old system)
- **Data migration conflicts**: Use expand-contract pattern—add new columns, backfill, migrate reads, then migrate writes, then drop old columns
- **Schema rollback needed after partial migration**: Keep old schema intact until the full verification period passes; never drop old columns before dual-write is fully disabled
- **Downstream consumers break**: Version your API surface; old consumers must keep working through the entire migration; add deprecation notices with concrete cutoff dates
