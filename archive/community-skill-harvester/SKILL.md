---
name: community-skill-harvester
description: "Find external skills from public registries, GitHub repos, and official skill collections, then evaluate them for quality, licensing, and fitness for adoption. Use when looking for existing skills before building from scratch, evaluating external skill quality, or migrating community skills into a local library. Do not use when building a novel skill with no external precedent or for quick one-off evaluation (just read the skill directly)."
---

# Purpose

Find external skills from public registries, GitHub repos, and skill collections, then evaluate them for quality, licensing, and fitness for adoption. Produces a harvest report with scored candidates and import proposals.

# When to use

- Looking for existing skills before building from scratch
- Evaluating external skill quality for potential adoption
- Migrating community skills into a local library
- User says "find skills for X", "search for existing skills", "are there skills for this?"

# When NOT to use

- Building a novel skill with no external precedent → `skill-creator`
- Quick one-off evaluation of a skill already in hand (just read the SKILL.md directly)
- Improving an already-adopted skill → `skill-improver`
- Auditing internal library quality → `skill-catalog-curation`

# Procedure

> **Dependency**: Steps 1 use the `gh` CLI. If `gh` is unavailable, fall back to
> `curl` against the GitHub REST API or direct web search.

## 1. Search for relevant skills

Search these sources in order:

```bash
# GitHub search by topic
gh search repos --topic agent-skills --limit 20 --json fullName,description,stargazersCount

# Search for SKILL.md files across GitHub
gh search code "name:" --filename SKILL.md --limit 20

# Search known skill registries and collections
# Add repository URLs for any skill registries relevant to your ecosystem
```

## 2. Evaluate skill quality

For each candidate, apply this checklist:

| Criterion | Check | Weight |
|-----------|-------|--------|
| Has SKILL.md | File exists | Required |
| Clear purpose | Non-vague description | High |
| Concrete procedures | Numbered steps, not platitudes | High |
| License specified | SPDX identifier present | Required |
| Active maintenance | Commits in last 6 months | Medium |
| Usage evidence | Stars, forks, downloads | Medium |

**Scoring:** Required checks must pass or skill is rejected. Score 0-6 on remaining criteria. Score ≥4 = adopt candidate. Score 2-3 = adapt candidate. Score <2 = reject.

## 3. License compatibility check

Acceptable licenses for adoption: `Apache-2.0`, `MIT`, `BSD-2-Clause`, `BSD-3-Clause`, `ISC`, `CC0-1.0`, `Unlicense`.

Copyleft licenses (`GPL-*`, `AGPL-*`): flag for manual review before adoption.

No license: do not import.

## 4. Extract patterns

Before importing, document:
- Structure patterns worth adopting
- Anti-patterns to avoid
- Adaptations needed for local conventions

## 5. Create import proposal

```markdown
# Skill Import Proposal: [name]

## Source
- URL: [github.com/org/repo/skills/name]
- License: [SPDX]
- Quality Score: [N/6]

## Rationale
[Why adopt this skill]

## Required Adaptations
- [ ] [Adaptation 1]
- [ ] [Adaptation 2]

## Recommendation
[ADOPT | ADAPT | REFERENCE_ONLY | REJECT]
```

## 6. Execute import (if approved)

```bash
mkdir -p skills/[skill-name]
# Copy and adapt SKILL.md
# Add provenance section documenting origin, license, import date, modifications
git add skills/[skill-name]
```

Confirm with the user before committing:
```
About to commit imported skill [skill-name]. Proceed? [y/N]
```

```bash
git commit -m "feat: import [skill-name] from [source]"
```

# Output contract

Produce exactly this structure:

```markdown
## Harvest Report

### Sources Searched
| Source | Query | Results Found |
|--------|-------|---------------|
| GitHub Topics | agent-skills | N |
| GitHub Code | SKILL.md | N |

### Candidates Evaluated
| Skill | Source | Quality Score | License | Recommendation |
|-------|--------|---------------|---------|----------------|
| name | URL | N/6 | SPDX | ADOPT / ADAPT / REFERENCE_ONLY / REJECT |

### Import Proposals
[One proposal block per candidate scoring ≥2 — use the template from step 5]

### Patterns Discovered
- [Structural or procedural patterns worth adopting]
```

# Failure handling

- **Registry unavailable**: Fall back to direct GitHub search
- **License unclear**: Do not import; flag for manual review
- **Skill format incompatible**: Extract concepts, rewrite in local format
- **Quality too low**: Document as "reference only", do not import

## Next steps

After harvesting skills:
- Record provenance for adopted skills → `skill-provenance`
- Safety review for untrusted sources → `skill-safety-review`
- Adapt to local conventions → `skill-adaptation`

## References

- Agent Skills specification: https://agentskills.io/specification
- SPDX license list: https://spdx.org/licenses/
