---
name: skill-lifecycle-management
description: >-
  Promote, deprecate, and track skills through lifecycle states
  (draft → beta → stable → deprecated → archived). Use when auditing maturity
  across a skill library, promoting a tested skill to stable, retiring a
  superseded skill, or checking which skills are production-ready. Do not use for
  creating new skills (use skill-creator), improving individual skill quality
  (use skill-improver), or reorganizing the library catalog
  (use skill-catalog-curation).
---

# Purpose

Manage skills through lifecycle states: draft → beta → stable → deprecated → archived. Ensure maturity labels reflect reality, retired skills don't silently break dependents, and promotion/deprecation criteria are applied consistently.

# When to use

- Auditing which skills are production-ready vs still draft
- Promoting a skill after evaluation confirms it works
- Deprecating a skill that has been superseded or consistently fails
- Tracking lifecycle transitions across a skill library

# When NOT to use

- Creating a new skill from scratch → `skill-creator`
- Improving an existing skill's quality or output → `skill-improver`
- Reorganizing the library catalog, deduplicating, or enforcing naming → `skill-catalog-curation`

# Lifecycle states

| State | Meaning | Guidance |
|-------|---------|----------|
| `draft` | Being written, not validated | Do not use in production |
| `beta` | Basic validation passed, accepting feedback | Use with monitoring |
| `stable` | Fully validated and promoted | Default choice |
| `deprecated` | Superseded or not recommended | Functional but avoid for new work |
| `archived` | Removed from active use | Reference only, do not install |

# Procedure

1. **Inventory current states**: List all skills with their `metadata.maturity` field. Flag anomalies — draft skills older than 2 cycles, stable skills with known bugs.
2. **Apply promotion criteria**:
   - draft → beta: Tested with ≥3 **diverse** prompts — one core use case, one edge case, one negative case (should NOT trigger). All three produce expected output or correct non-trigger. Diversity matters more than count; three paraphrases of the same query do not qualify.
   - beta → stable: Formal evaluation (via `skill-evaluation`) returned a Pass verdict. At least 10 test cases with ≥90% pass rate. Used in at least 2 real projects or sessions without reported failure.
   - stable → deprecated: Before transitioning, document the replacement skill by name, verify the replacement is itself stable, and update all cross-references (AGENTS.md, other skills' "Do not use" sections, command definitions) to point to the replacement. The state change does not take effect until these updates are committed.
3. **Apply deprecation criteria** (any one sufficient):
   - A strictly better replacement exists and is stable
   - Unused for ≥3 cycles
   - Consistently fails evaluation
4. **Execute transitions**: Update `metadata.maturity` in the skill's frontmatter.
5. **Check dependents**: If a deprecated skill is referenced in AGENTS.md, commands, or other skills, flag each reference for update.
6. **Update library index**: Set each skill's catalog entry to its new state.

# Output contract

Produce a markdown report with these sections:

```
## Lifecycle Audit

### State Summary
| State | Count | Skills |
|-------|-------|--------|
| draft | 5 | skill-a, skill-b, ... |
| stable | 20 | ... |

### Recommended Transitions
| Skill | Current | Recommended | Reason |
|-------|---------|-------------|--------|
| skill-x | draft | beta | 3 prompts tested, all passed |
| skill-y | stable | deprecated | Superseded by skill-z |

### Dependency Impact
| Deprecated Skill | Referenced By | Required Action |
|------------------|---------------|-----------------|
| skill-y | AGENTS.md L45 | Update reference to skill-z |

### Actions (ordered)
1. Promote skill-x to beta
2. Deprecate skill-y, update AGENTS.md reference
```

If no transitions are warranted, state that explicitly — do not invent changes.

# Failure handling

| Problem | Response |
|---------|----------|
| Skills missing `metadata.maturity` field | Infer and add the field (has evals → beta; no evals → draft) before proceeding |
| Deprecated skill has active dependents | Identify or create replacement first; do not deprecate until dependents have a migration path |
| Disputed maturity (e.g. "stable" but failing evals) | Default to the more conservative state and note the discrepancy |
| No evaluation data available for promotion | Block promotion; recommend running `skill-evaluation` first |

## Next steps

Before promoting a skill to stable:
- Verify provenance is documented → `skill-provenance`
- Run safety review → `skill-safety-review`
- Run evaluation → `skill-evaluation`

## References

- Agent Skills specification: https://agentskills.io/specification
