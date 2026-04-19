---
name: skill-catalog-curation
description: >-
  Audit a skill library for duplicates, category drift, and discoverability gaps;
  verify naming conventions, cross-references between SKILL.md files, and
  description quality across skill directories.
  Use when: "audit the skill library", "clean up overlapping skills",
  "organize the catalog", "find duplicate skills".
  Do not use for: improving a single skill (skill-improver), creating a new skill (skill-creator),
  promoting or deprecating individual skills through lifecycle states (skill-lifecycle-management).
---

# Purpose

Detect duplicates, enforce category consistency, flag deprecation candidates, verify discoverability, and maintain catalog consistency across an entire skill library. Produces structured curation reports with prioritized action items.

# When to use

- Library has grown past ~20 skills and overlaps or inconsistencies have appeared
- Before a major release or after a bulk import
- Periodic maintenance pass (monthly for active libraries)
- User asks to "audit the library", "clean up skills", or "find duplicate skills"

# When NOT to use

- Improving or refining a single skill → `skill-improver`
- Creating a new skill from scratch → `skill-creator`
- Splitting a single skill into focused variants → `skill-variant-splitting`
- Promoting, deprecating, or archiving individual skills through lifecycle gates → `skill-lifecycle-management`

# Procedure

## 1. Build inventory

- List every skill directory at the repo root that contains a `SKILL.md`
- For each skill record: name (directory name), category (inferred from pipeline position in `# Next steps` cross-references, e.g., creation-pipeline vs. improvement-pipeline), last-modified date (from `git log -1 --format=%cI -- <skill-dir>`)
- Infer status from location: root directories → **active**, `archive/` → **archived**, `corpus/` → **test fixture**
- Count skills per inferred category; flag uncategorized or miscategorized entries

## 2. Detect duplicates and overlaps

- **Extract action signatures**: For each skill, extract the first verb+object phrase from the description (e.g., "Audit a skill library" → `audit library`, "Compare skill variants" → `compare variants`). This is the skill's action signature.
- **Group by action signature**: Skills with the same or synonymous action signature (e.g., `audit library` ≈ `review catalog`) are potential duplicates. Compare trigger phrases within each group — if >50% of one skill's trigger phrases also appear in or paraphrase the other, flag as duplicate.
- **Check cross-references**: For skills not grouped by action signature, inspect "Do not use for" sections. Mutual cross-references (A says "not for X, use B" and B says "not for Y, use A") suggest related scopes that may overlap or have ambiguous boundaries.
- Flag identical names at different paths.
- For each flagged pair, recommend one of: **merge**, **differentiate** (rewrite boundaries), or **keep** (with rationale).

## 3. Audit categories

- Verify each skill's category matches its actual function (read the procedure, not just the name)
- Singleton categories (1 skill) → review whether the skill fits naturally in an existing category. Do not merge categories that represent distinct capability areas solely based on count
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

The report MUST contain all eight sections. Omit rows only when a section has zero findings; keep the heading with "None found."

```markdown
## Catalog Curation Report

### Inventory
- Total active skills: <N> (root directories with SKILL.md)
- Archived: <N> (in archive/)
- Test fixtures: <N> (in corpus/)
- Inferred categories: <list each category with skill count>

### Description Quality
| Skill | Word count | Starts with verb? | Has trigger phrases? | Issues |
|-------|-----------|-------------------|---------------------|--------|
| ...   | <N>       | yes/no            | yes/no              | <details> |

### Cross-Reference Graph
| Skill | References out (Next steps) | Referenced by | Boundary clarity |
|-------|-----------------------------|---------------|-----------------|
| ...   | <list>                      | <list>        | clear / ambiguous / missing |

### Duplicates / Overlaps
| Skill A | Skill B | Overlap evidence | Recommendation |
|---------|---------|-----------------|----------------|
| ...     | ...     | <shared trigger phrases or purpose text> | merge / differentiate / keep |

### Naming Convention Audit
| Skill | Convention | Issue |
|-------|-----------|-------|
| ...   | kebab-case / other | <details if non-conforming> |

### Gap Analysis
| Pipeline role | Expected skill | Status |
|--------------|---------------|--------|
| ...          | ...           | present / missing / weak coverage |

### Deprecation Candidates
| Skill | Reason | Replacement |
|-------|--------|-------------|
| ...   | ...    | ...         |

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
7. **Update the inventory** — update the README skill inventory to reflect the merge

# Next steps

After curation:
- Execute merge recommendations → use the merge procedure above
- Fix discoverability issues → `skill-trigger-optimization`
- Deprecate identified candidates → `skill-lifecycle-management`
