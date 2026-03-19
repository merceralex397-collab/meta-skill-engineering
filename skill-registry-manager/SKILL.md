---
name: skill-registry-manager
description: "Maintain the skill library catalog: add entries, update metadata, manage tags, track maturity/status, enforce naming conventions, and generate the library index. Use when adding a new skill to the registry, updating skill metadata after changes, auditing catalog consistency, or generating a publishable skill index. Do not use for editing skill content (edit SKILL.md directly) or for deprecating skills (use skill-deprecation-manager)."
---

# Purpose

Maintain the skill library catalog — the index, metadata, tags, maturity status, and consistency of all skills in the ecosystem. Ensures every skill is discoverable, correctly categorized, and consistently named.

# When to use

- Adding a new skill to the registry
- Updating skill metadata after changes
- Auditing catalog consistency (names, maturity, tags)
- Generating a publishable skill index
- User says "update the registry", "add this to the catalog", "generate the skill index"

# When NOT to use

- Editing skill content (edit SKILL.md directly)
- Deprecating skills → `skill-deprecation-manager`
- Auditing for duplicates and overlaps → `skill-catalog-curation`
- Creating new skills → `skill-creator`

# Procedure

## 1. Scan skill inventory

```bash
# Find all active skills
find . -name "SKILL.md" -not -path "*/ARCHIVE/*" -not -path "*/node_modules/*" | sort

# Extract metadata from each
for skill_md in $(find . -name "SKILL.md" -not -path "*/ARCHIVE/*"); do
  dir=$(dirname "$skill_md")
  name=$(grep "^name:" "$skill_md" | head -1 | cut -d: -f2- | tr -d ' "')
  maturity=$(grep "maturity:" "$skill_md" | head -1 | cut -d: -f2- | tr -d ' ')
  echo "$name | $maturity | $dir"
done
```

## 2. Validate catalog consistency

For each skill, check:

| Check | Criteria | Severity |
|-------|----------|----------|
| Frontmatter exists | YAML between `---` delimiters | Error |
| `name` field present | Non-empty string | Error |
| `description` field present | Non-empty, 20-100 words | Error |
| Name matches directory | `name` field = directory name | Warning |
| No duplicate names | Each name unique across catalog | Error |
| Tags are valid | Tags exist in the controlled vocabulary | Warning |
| Maturity is valid | One of: `draft`, `beta`, `stable`, `deprecated` | Warning |

## 3. Register a new skill

When adding a skill to the catalog:

1. Verify SKILL.md has required frontmatter (`name`, `description`)
2. Assign maturity level (default: `draft`)
3. Add to `skills-lock.json` with metadata snapshot
4. Verify no name collision with existing skills
5. Update library index

## 4. Update skill metadata

When a skill changes:

1. Read current `skills-lock.json` entry
2. Update changed fields (description, maturity, tags)
3. Record modification timestamp
4. Regenerate affected index entries

## 5. Generate library index

Produce a machine-readable index and a human-readable catalog:

**skills-lock.json** (machine-readable):
```json
{
  "version": "1.0.0",
  "generated": "<ISO timestamp>",
  "skills": {
    "<skill-name>": {
      "path": "<relative path>",
      "description": "<description>",
      "maturity": "<draft|beta|stable|deprecated>",
      "tags": ["<tag1>", "<tag2>"],
      "license": "<SPDX>",
      "last_updated": "<ISO date>"
    }
  }
}
```

**CATALOG.md** (human-readable):
```markdown
# Skill Catalog

## Active Skills
| Name | Maturity | Description |
|------|----------|-------------|
| [name] | [maturity] | [description] |

## Deprecated Skills
| Name | Replacement | Deprecated Date |
|------|-------------|----------------|
```

## 6. Enforce naming conventions

- Skill names: kebab-case, descriptive, 2-4 words
- Directory names match skill names exactly
- No abbreviations unless universally understood (e.g., `pr`, `qa`)

# Output contract

- Updated `skills-lock.json` with current catalog state
- Updated `CATALOG.md` with human-readable index
- Validation report listing any consistency issues found

# Failure handling

- **Duplicate skill name**: Reject registration, report collision with existing skill
- **Missing frontmatter**: Flag as invalid, do not register until fixed
- **Orphaned lock entries**: Remove entries for skills that no longer exist on disk
- **Name/directory mismatch**: Report as warning, recommend rename

## Next steps

After updating the registry:
- Audit catalog consistency → `skill-catalog-curation`
- Track lifecycle maturity → `skill-lifecycle-management`

## References

- Agent Skills specification: https://agentskills.io/specification
