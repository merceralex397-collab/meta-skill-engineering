---
name: skill-lifecycle-management
description: >-
  Manage skill lifecycle states (draft → beta → stable → deprecated → archived)
  including promotion criteria, deprecation procedures, and maturity audits. Use
  when a user says "deprecate this skill", "promote this to stable", "retire
  this", "this is replaced by X", "which skills are production-ready", or
  "audit maturity across the library". Do not use for creating new skills
  (use skill-creator), improving individual skill quality (use skill-improver),
  or reorganizing the library catalog (use skill-catalog-curation).
---

# Purpose

Manage skills through lifecycle states: draft → beta → stable → deprecated → archived. Ensure maturity labels reflect reality, retired skills don't silently break dependents, and promotion/deprecation criteria are applied consistently.

# When to use

- Auditing which skills are production-ready vs still draft
- Promoting a skill after evaluation and safety review confirm it is ready
- Promoting a skill after evaluation confirms it works
- Deprecating a skill that has been superseded or consistently fails
- Tracking lifecycle transitions across a skill library
- User says "deprecate this skill", "retire this", or "this is replaced by X"
- A catalog audit identifies a skill for retirement
- A skill is causing harm and needs immediate pull

# When NOT to use

- Creating a new skill from scratch → `skill-creator`
- Improving an existing skill's quality or output → `skill-improver`
- Reorganizing the library catalog, deduplicating, or enforcing naming → `skill-catalog-curation`

# Procedure

## Lifecycle states

| State | Meaning | Guidance |
|-------|---------|----------|
| `draft` | Being written, not validated | Do not use in production |
| `beta` | Basic validation passed, accepting feedback | Use with monitoring |
| `stable` | Fully validated and promoted | Default choice |
| `deprecated` | Superseded or not recommended | Functional but avoid for new work |
| `archived` | Removed from active use | Reference only, do not install |

## Operating procedure

1. **Assess current lifecycle stage for each skill** using these signals:
   - **draft**: Has `evals/` directory but fewer than 3 test cases total across all JSONL files, OR passes `validate-skills.sh` with score < 8.
   - **beta**: Has `evals/` with ≥3 cases. Passes structural validation (score ≥ 8). No formal evaluation Pass verdict yet (no eval report in `eval-results/` with gate PASS).
   - **stable**: Formal evaluation (via `skill-evaluation`) returned a Pass verdict. At least 10 test cases with ≥90% pass rate. Referenced by other skills or AGENTS.md as a recommended tool.
   - **deprecated**: SKILL.md body contains a `⚠️ DEPRECATED` notice. Still in its root directory (not yet archived).
   - **archived**: Directory exists under `archive/` rather than at the repository root.
   Flag anomalies — skills whose signals contradict their apparent state (e.g., no evals but referenced as stable in AGENTS.md).
2. **Apply promotion criteria**:
   - draft → beta: Tested with ≥3 **diverse** prompts — one core use case, one edge case, one negative case (should NOT trigger). All three produce expected output or correct non-trigger. Diversity matters more than count; three paraphrases of the same query do not qualify.
   - beta → stable: Formal evaluation (via `skill-evaluation`) returned a Pass verdict. At least 10 test cases with ≥90% pass rate. Used in at least 2 real projects or sessions without reported failure.
   - stable → deprecated: Before transitioning, document the replacement skill by name, verify the replacement is itself stable, and update all cross-references (AGENTS.md, other skills' "Do not use" sections, command definitions) to point to the replacement. The state change does not take effect until these updates are committed.
3. **Apply deprecation criteria** (any one sufficient):
   - A strictly better replacement exists and is stable
   - Unused for ≥3 cycles
   - Consistently fails evaluation
4. **Execute transitions**: Add a deprecation notice to the skill's SKILL.md body (see procedure below), update cross-references in AGENTS.md and other skills that mention it, and commit all changes together.
5. **Check dependents**: If a deprecated skill is referenced in AGENTS.md, commands, or other skills, flag each reference for update.
6. **Update cross-references**: In AGENTS.md, README.md, and any other skills' "Next steps" or "When NOT to use" sections, replace references to the transitioned skill with its replacement (or note removal).

## Deprecation procedure

When deprecating or retiring a skill, follow this detailed procedure:

### 1. Confirm deprecation decision

Determine the reason:
- **Superseded**: Replaced by a better skill
- **Merged**: Functionality absorbed into another skill
- **Unused**: No invocations over N cycles
- **Failing**: Consistently poor evaluation results
- **Harmful**: Misfiring or producing wrong outputs — pull immediately

### 2. Find all references

```bash
grep -r "<skill-name>" AGENTS.md **/SKILL.md docs/ 2>/dev/null
grep -r "Do NOT use" **/SKILL.md | grep -i "<skill-name>"
```

### 3. Update each reference

Replace with replacement skill pointer, or note "no replacement available".

### 4. Add deprecation notice

Add a notice at the top of the SKILL.md body (after frontmatter):
```markdown
> ⚠️ DEPRECATED as of [date]. Use [replacement] instead.
> Reason: [one sentence]. Kept for reference only.
```

Do NOT add lifecycle metadata to YAML frontmatter — the repo contract limits frontmatter to `name` and `description` only.

### 5. Move to archive (if applicable)

Confirm with user before moving:
```
About to move skill-name/ to archive/skill-name/. Proceed? [y/N]
```

```bash
mkdir -p archive
mv skill-name/ archive/skill-name/
```
Do NOT delete — preserve for reference and provenance.

### 6. Update cross-references

- Update AGENTS.md to remove or redirect references to the deprecated skill
- Update README.md if the skill is listed in an active inventory
- Update other skills' "Next steps" or "When NOT to use" sections that reference it
- Commit all reference updates in the same commit as the deprecation notice or archive move

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

When executing a deprecation specifically, also produce:

```
## Deprecation: [skill-name]

**Reason**: [superseded | merged | unused | failing | harmful]
**Replacement**: [skill] or "none"
**Date**: [YYYY-MM-DD]

### References Updated
| File | Change |
|------|--------|
| [file] | [old → new] |

### Verification
- [x] All references updated
- [x] Deprecation notice added
- [x] Archived (if applicable)
- [x] Cross-references updated (AGENTS.md, README.md, other skills)
```

If no transitions are warranted, state that explicitly — do not invent changes.

# Failure handling

| Problem | Response |
|---------|----------|
| Skills with unknown lifecycle state | Infer from evidence: has eval results in eval-results/ with gate PASS → stable; has evals/ but no results yet → beta; has evals/ but fewer than 3 cases → draft; has deprecation notice in SKILL.md → deprecated; lives in `archive/` → archived |
| Deprecated skill has active dependents | Identify or create replacement first; do not deprecate until dependents have a migration path |
| Disputed maturity (e.g. "stable" but failing evals) | Default to the more conservative state and note the discrepancy |
| No evaluation data available for promotion | Block promotion; recommend running `skill-evaluation` first |
| Replacement skill incomplete | Create migration path with limitations noted |
| Urgent harm from a skill | Deprecate immediately, create follow-up ticket for replacement |

# Next steps

Before promoting a skill to stable:
- Run safety review → `skill-safety-review`
- Run evaluation → `skill-evaluation`
