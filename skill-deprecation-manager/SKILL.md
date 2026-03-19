---
name: skill-deprecation-manager
description: "Safely deprecate, retire, or merge obsolete skills while preserving backward references and library clarity. Use when a user says 'deprecate this skill', 'retire this', or 'this is replaced by X', when a catalog audit identifies a skill for retirement, or when a skill is causing harm and needs immediate pull. Do not use when the skill needs improvement (use skill-improver) or when the repo doesn't support deprecation (just delete)."
---

# Purpose

Safely deprecate a skill: update lifecycle state, add deprecation notices, redirect references to the replacement, and preserve content for history.

# When to use

- User says "deprecate this skill", "retire this", or "this is replaced by X"
- A catalog audit identifies a skill for retirement
- A skill is causing harm and needs immediate pull
- A skill has been superseded or consistently fails evaluation

# When NOT to use

- Skill needs improvement, not retirement → `skill-improver`
- Repo doesn't support deprecation workflows (just delete the skill)
- Skill needs lifecycle state tracking without deprecation → `skill-lifecycle-management`
- Reorganizing the catalog → `skill-catalog-curation`

# Procedure

### 1. Confirm deprecation decision

Determine the reason:
- **Superseded**: Replaced by a better skill
- **Merged**: Functionality absorbed into another skill
- **Unused**: No invocations over N cycles
- **Failing**: Consistently poor evaluation results
- **Harmful**: Misfiring or producing wrong outputs — pull immediately

### 2. Find all references

```bash
# Search across repo
grep -r "<skill-name>" AGENTS.md **/SKILL.md skills-lock.json docs/ 2>/dev/null
# Check "Do NOT use when" sections in sibling skills
grep -r "Do NOT use" **/SKILL.md | grep -i "<skill-name>"
```

### 3. Update each reference

Replace with replacement skill pointer, or note "no replacement available".

### 4. Update frontmatter

```yaml
metadata:
  maturity: deprecated
  deprecated_by: replacement-skill  # or "none"
  deprecated_reason: "Superseded by newer version"
```

### 5. Add deprecation notice

At top of SKILL.md body:
```markdown
> ⚠️ DEPRECATED as of [date]. Use [replacement] instead.
> Reason: [one sentence]. Kept for reference only.
```

### 6. Move to archive (if exists)

Confirm with user before moving:
```
About to move skill-name/ to ARCHIVE/skill-name/. Proceed? [y/N]
```

```bash
mkdir -p ARCHIVE
mv skill-name/ ARCHIVE/skill-name/
```
Do NOT delete — preserve for reference and provenance.

### 7. Update registry

- Mark `deprecated: true` in `skills-lock.json`
- Remove from active catalog
- Add to "Deprecated" section of index

# Output contract

```markdown
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
- [x] Archived
- [x] Registry updated
```

# Failure handling

- **Active dependencies, no replacement**: Don't deprecate — document gap, build replacement first
- **Replacement incomplete**: Create migration path with limitations noted
- **Urgent harm**: Deprecate immediately, create follow-up ticket for replacement

## Next steps

After deprecating a skill:
- Update the catalog → `skill-catalog-curation`
- Update the registry index → `skill-registry-manager`
- Track the lifecycle transition → `skill-lifecycle-management`

## References

- Agent Skills specification: https://agentskills.io/specification
