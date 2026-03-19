---
name: skill-catalog-curation
description: >-
  Audit a skill library for duplicates, category drift, and discoverability gaps;
  maintain the catalog index, metadata, tags, and naming conventions.
  Use when: "audit the skill library", "clean up overlapping skills",
  "organize the catalog before release", "update the registry", "add this to the catalog",
  "generate the skill index".
  Do not use for: improving a single skill (skill-improver), creating a new skill (skill-creator),
  promoting or deprecating individual skills through lifecycle states (skill-lifecycle-management).
---

# Purpose

Detect duplicates, enforce category consistency, flag deprecation candidates, verify discoverability, and maintain the catalog index across an entire skill library. Produces structured curation reports and keeps the library index (skills-lock.json, CATALOG.md) current.

# When to use

- Library has grown past ~20 skills and overlaps or inconsistencies have appeared
- Before a major release or after a bulk import
- Periodic maintenance pass (monthly for active libraries)
- User asks to "audit the library", "clean up skills", or "find duplicate skills"
- Adding a new skill to the registry
- Updating skill metadata after changes
- Generating a publishable skill index

# When NOT to use

- Improving or refining a single skill → `skill-improver`
- Creating a new skill from scratch → `skill-creator`
- Promoting, deprecating, or archiving individual skills through lifecycle gates → `skill-lifecycle-management`
- Installing or packaging skills → `skill-installer` / `skill-packaging`

# Procedure

## 1. Build inventory

- List every skill: name, category, maturity, last-modified date
- Count skills per category; flag uncategorized or miscategorized entries

## 2. Detect duplicates and overlaps

- **Extract action signatures**: For each skill, extract the first verb+object phrase from the description (e.g., "Audit a skill library" → `audit library`, "Compare skill variants" → `compare variants`). This is the skill's action signature.
- **Group by action signature**: Skills with the same or synonymous action signature (e.g., `audit library` ≈ `review catalog`) are potential duplicates. Compare trigger phrases within each group — if >50% of one skill's trigger phrases also appear in or paraphrase the other, flag as duplicate.
- **Check cross-references**: For skills not grouped by action signature, inspect "Do not use for" sections. Mutual cross-references (A says "not for X, use B" and B says "not for Y, use A") suggest related scopes that may overlap or have ambiguous boundaries.
- Flag identical names at different paths.
- For each flagged pair, recommend one of: **merge**, **differentiate** (rewrite boundaries), or **keep** (with rationale).

## 3. Audit categories

- Verify each skill's category matches its actual function (read the procedure, not just the name)
- Categories with ≤ 2 skills → propose merge into a neighbor
- Categories with > 15 skills → propose a split axis

## 4. Check discoverability

- Does each description start with an action verb?
- Are negative boundaries present and naming the correct neighbor skills?
- Would a user with a realistic task phrase find this skill via keyword match?

Flag concrete defects using these thresholds:
- **Too terse**: Description under 20 words → insufficient for reliable routing. Recommend expanding to at least one sentence with verb, scope, and context.
- **No trigger phrases**: Description lacks quoted example phrases (e.g., `"audit the library"`) → routing relies entirely on keyword overlap, which is fragile. Recommend adding 2–3 realistic trigger phrases.
- **Weak boundaries**: "Do not use" section names fewer than 2 alternative skills → the skill's scope edges are undefined. Check the catalog for the most likely confused neighbors and add them.

## 5. Flag deprecation candidates

- Superseded by a newer skill with the same coverage
- Targets a tool or framework no longer in the stack
- If usage metrics exist, flag skills with zero invocations over the review window

## 6. Compile report

Output the curation report using the structure in **Output contract** below.

# Output contract

The report MUST contain all six sections. Omit rows only when a section has zero findings; keep the heading with "None found."

```markdown
## Catalog Curation Report

### Inventory
- Total skills: <N>
- By maturity: <draft: N, stable: N, deprecated: N>
- Categories: <N>

### Duplicates / Overlaps
| Skill A | Skill B | Overlap evidence | Recommendation |
|---------|---------|-----------------|----------------|
| ...     | ...     | <shared trigger phrases or purpose text> | merge / differentiate / keep |

### Category Issues
| Issue | Affected skills | Recommended action |
|-------|----------------|--------------------|

### Discoverability Gaps
| Skill | Problem | Fix |
|-------|---------|-----|
| ...   | description starts with noun, no negative boundaries | rewrite description |

### Deprecation Candidates
| Skill | Reason | Replacement |
|-------|--------|-------------|

### Prioritized Actions
1. **[High]** <action> — <reason>
2. **[Medium]** <action> — <reason>
3. **[Low]** <action> — <reason>
```

# Failure handling

- **Cannot compare descriptions meaningfully** (e.g., descriptions are one-word stubs): report the skill as "unassessable — description too short to evaluate" and recommend a description rewrite before the next curation pass.
- **Category scheme is incoherent** (no consistent axis): propose a replacement taxonomy with explicit grouping criteria and flag it as a blocking action before other category fixes.
- **No usage metrics available**: fall back to last-modified date and whether the skill's target tool/framework still exists in the stack.
- **Findings exceed 30 action items**: split into phases — Phase 1: duplicates and broken boundaries (high), Phase 2: category restructuring (medium), Phase 3: discoverability polish (low). Do not emit an unprioritized list.

## Merge procedure

When the audit identifies true duplicates (recommendation: "merge"), execute:

1. **Choose the survivor** — the skill with broader coverage, better routing, or more support files
2. **Inventory the absorbed skill** — list all unique procedure steps, failure cases, and support files not present in the survivor
3. **Merge content** — add unique elements from the absorbed skill into the survivor. Do not duplicate content that already exists.
4. **Update routing** — rewrite the survivor's description to cover trigger phrases from both skills
5. **Update cross-references** — find all skills that reference the absorbed skill name and update to point to the survivor
6. **Remove the absorbed skill** — confirm with user before deleting:
   ```
   About to delete [absorbed-skill]/ directory. Proceed? [y/N]
   ```
   Do NOT delete without explicit user confirmation.
7. **Update the catalog** — update README, CATALOG.md, or skills-lock.json to reflect the merge

# Registry operations

## Register a new skill

When adding a skill to the catalog:

1. Verify SKILL.md has required frontmatter (`name`, `description`)
2. Assign maturity level (default: `draft`)
3. Verify no name collision with existing skills
4. Update library index files (see below)

## Update skill metadata

When a skill changes:

1. Read current catalog entry
2. Update changed fields (description, maturity, tags)
3. Record modification timestamp
4. Regenerate affected index entries

## Generate library index

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

## Enforce naming conventions

- Skill names: kebab-case, descriptive, 2-4 words
- Directory names match skill names exactly
- No abbreviations unless universally understood (e.g., `pr`, `qa`)

## Next steps

After curation:
- Execute merge recommendations → use the merge procedure above
- Fix discoverability issues → `skill-trigger-optimization`
- Deprecate identified candidates → `skill-lifecycle-management`
