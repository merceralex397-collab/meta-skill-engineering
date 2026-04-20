---
name: community-skill-harvester
description: "Find external skills from public registries, GitHub repos, and official skill collections, then evaluate them for quality, licensing, and fitness for adoption. Use when looking for existing skills before building from scratch, evaluating external skill quality, or migrating community skills into a local library. Do not use when building a novel skill with no external precedent or for quick one-off evaluation (just read the skill directly)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
---

# Community Skill Harvester

Harvests skills from public registries and evaluates them for adoption.

## Procedure

### 1. Search for relevant skills

Search these sources in order:

```bash
# GitHub search by topic
gh search repos --topic agent-skills --limit 20 --json fullName,description,stargazersCount

# Anthropic skills repo
gh api repos/anthropics/skills/contents/skills --jq '.[].name' 2>/dev/null

# Direct GitHub code search
gh search code "name:" --filename SKILL.md --limit 20
```

### 2. Evaluate skill quality

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

### 3. License compatibility check

Acceptable licenses for adoption: `Apache-2.0`, `MIT`, `BSD-2-Clause`, `BSD-3-Clause`, `ISC`, `CC0-1.0`, `Unlicense`.

Copyleft licenses (`GPL-*`, `AGPL-*`): flag for manual review before adoption.

No license: do not import.

### 4. Extract patterns

Before importing, document:
- Structure patterns worth adopting
- Anti-patterns to avoid
- Adaptations needed for local conventions

### 5. Create import proposal

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

### 6. Execute import (if approved)

```bash
mkdir -p COPILOT/[skill-name]-COPILOT-L
# Copy and adapt SKILL.md
# Add provenance section documenting origin, license, import date, modifications
git add COPILOT/[skill-name]-*
git commit -m "feat: import [skill-name] from [source]"
```

## Output contract

A harvest report containing:
- Sources searched and count
- Skills evaluated with scores
- Import proposals for candidates scoring ≥2
- Patterns discovered during evaluation

## Failure handling

- **Registry unavailable**: Fall back to direct GitHub search
- **License unclear**: Do not import; flag for manual review
- **Skill format incompatible**: Extract concepts, rewrite in local format
- **Quality too low**: Document as "reference only", do not import

## References

- Anthropic Skills: https://github.com/anthropics/skills
- SPDX license list: https://spdx.org/licenses/
