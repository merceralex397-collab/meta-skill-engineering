---
name: skill-provenance
description: >-
  Audit and record origin, authorship, evidence basis, license, and trust level
  for a skill. Use when someone asks "where did this skill come from?",
  "audit this skill's origin", "document provenance", or when evaluating
  external skills for adoption or establishing trust before execution.
  Do not use for quality assessment (use skill-evaluation), lifecycle state
  tracking (use skill-lifecycle-management), or safety audits
  (use skill-safety-review).
---

# Purpose

Produce a provenance record for a skill—answering "where did this come from, why was it written this way, and can we trust it?" Prevents skills from becoming untraceable prompt blobs.

# When to use

- User asks "where did this come from?", "document origin", "add provenance"
- "Audit this skill's origin", "check the license", "establish trust level"
- Evaluating external skills for adoption
- Auditing existing skills for license compliance
- Publishing a skill to a shared registry that requires attribution
- Importing a skill from an external source that needs trust assessment
- A skill encodes non-obvious assumptions that future editors must understand

# When NOT to use

- Creating new skills from scratch where provenance is simply "authored here" and obvious
- Git history alone provides sufficient provenance for internal-only skills
- You need quality evaluation → `skill-evaluation`
- You need lifecycle state tracking → `skill-lifecycle-management`
- You need safety or security audit → `skill-safety-review`

# Modes

This skill operates in two modes:

- **Audit mode** — Read-only assessment: investigate origin, verify source claims, check license, assign trust level. Produces an audit report.
- **Record mode** — Write provenance metadata: produce a frontmatter patch and PROVENANCE.md. Produces written artifacts.

Use audit mode when evaluating skills for adoption or compliance. Use record mode when documenting provenance for an existing skill. Both modes can be combined: audit first, then record the findings.

# Procedure

## Audit mode steps (read-only assessment)

### A1. Identify origin type

| Origin Type | Default Trust |
|-------------|---------------|
| `authored` | High |
| `forked` | Medium |
| `imported` | Medium |
| `generated` | Low |
| `unknown` | Untrusted |

### A2. Extract provenance metadata

From SKILL.md frontmatter: `name`, `source`, `license`.

From git history:
```bash
git log --follow --format="%H %an %ad %s" -- SKILL.md | tail -1
git log --follow --oneline -- SKILL.md
```

### A3. Verify source claims

If `source` claims external origin:
```bash
curl -s "<original-raw-url>" > /tmp/original.md
diff SKILL.md /tmp/original.md
```
If different → origin is `forked`, not `imported`.

### A4. Check license compatibility

| License | Category | Action |
|---------|----------|--------|
| Apache-2.0, MIT, BSD-* | Permissive | OK for any use |
| GPL-3.0, AGPL-3.0 | Copyleft | Check derivative work implications |
| (none) | Unknown | Do not adopt — flag for manual review |

### A5. Assign trust level

| Origin | License OK | Source Verified | Mods Reviewed | Trust |
|--------|-----------|----------------|---------------|-------|
| authored | N/A | N/A | N/A | HIGH |
| forked | YES | YES | YES | HIGH |
| forked | YES | YES | NO | MEDIUM |
| imported | YES | YES | N/A | MEDIUM |
| generated | N/A | N/A | YES | MEDIUM |
| unknown | * | * | * | UNTRUSTED |

## Record mode steps (write provenance artifacts)

## 1. Document origin
- If adapted: source URL, percentage original vs adapted
- If created from scratch: state "created", note the motivating requirement
- If derived from multiple sources: list all, note which informed which sections

**If origin is unclear**, investigate before defaulting to "unknown":
- Run `git log --diff-filter=A -- <skill-path>` to find the initial commit; the commit message often names the source or motivation.
- Search commit messages near that date for phrases like "imported from", "based on", "ported from", or "adapted".
- Check the SKILL.md file itself for license headers, attribution comments, or URLs in the body text — these are provenance signals left by the original author.

## 2. Record authorship
- Original author(s) with identifiers
- Adapter(s) if modified, with date of adaptation
- Reviewer(s) if peer-reviewed

## 3. Document evidence basis
- Which documentation informed the procedure steps?
- Which standards or best practices were referenced?
- Which expert knowledge was encoded and by whom?
- Which failure patterns were learned from?

**If evidence sources aren't explicitly documented**, extract them from the skill content:
- Scan the SKILL.md body for URLs — these are almost always evidence sources. Record each with a note on which section it informs.
- Check for a `references/` directory; if present, each file is an evidence source. Note its content and which procedure steps rely on it.
- Look for terminology that implies a specific framework or standard (e.g., "OWASP Top 10", "12-factor", "semver") — these name the evidence basis even when not linked.

## 4. Catalog encoded assumptions
- Runtime environment requirements (OS, tool versions, runtimes)
- Expected conventions (file layout, naming, package manager)
- Implicit dependencies not declared in frontmatter

**Concrete heuristic for finding hidden assumptions**: Search the procedure text for these indicators:
- **Tool names** (`npm`, `pip`, `docker`, `cargo`, `go`) → assumes that tool is installed and is the project's package manager
- **Path patterns** (`/src`, `.opencode/`, `docs/`) → assumes a specific directory layout
- **File extensions** (`.py`, `.ts`, `.rs`, `.go`) → assumes a specific language stack
- **Command invocations** (`git`, `make`, `curl`) → assumes CLI tooling availability

Each match is an encoded assumption. Record it with the specific line or section where it appears so future editors can assess whether it still holds.

## 5. Assess trust level

| Level | Criteria |
|-------|----------|
| **High** | Known author, peer-reviewed, tested, actively maintained |
| **Medium** | Known source, partial testing, no formal review |
| **Low** | Unknown or unverified source, untested |
| **Untrusted** | External import, not yet reviewed |

Assign one level. Write a one-sentence rationale.

# Output contract

Produce artifacts based on the mode used:

**Audit mode output:**
```markdown
# Provenance Audit: [skill-name]

## Origin
- Type: [authored|forked|imported|generated|unknown]
- Source: [URL or "this repo"]
- License: [SPDX]

## Trust Assessment
- License: [PASS|WARN|FAIL]
- Source verified: [YES|NO|UNABLE]
- Modifications reviewed: [YES|NO|N/A]
- Overall trust: [HIGH|MEDIUM|LOW|UNTRUSTED]

## Recommendation
[Action to take]
```

**Record mode output — two artifacts:**

**1. Frontmatter patch** — merge into the skill's existing YAML metadata:
```yaml
metadata:
  provenance:
    origin: "https://... or created"
    adaptation_pct: 30  # omit if 0
    trust: high | medium | low | untrusted
    evidence: ["URL or short citation", ...]
```

**2. PROVENANCE.md** — co-located next to SKILL.md:
```markdown
# Provenance: [skill-name]

## Origin
- **Source**: [URL or "created"]
- **Author**: [name/handle]
- **Adapted by**: [name] on [YYYY-MM-DD]

## Evidence Basis
- [source] — informed [which sections]

## Encoded Assumptions
- [assumption with specifics]

## Trust Level: [High|Medium|Low|Untrusted]
**Rationale**: [one sentence]
```

If the skill is too trivial for a full record, produce the frontmatter patch only and skip PROVENANCE.md. Note the decision in the commit message.

# Failure handling

| Scenario | Action |
|----------|--------|
| Origin unknown | Set `origin: "unknown"`, trust to `low`, add `## Gaps` section listing what is missing |
| Author unreachable | Document what is known, mark authorship as "unverified" in PROVENANCE.md |
| Assumptions undeterminable | Set trust to `low` with rationale "assumptions undocumented—treat as fragile" |
| Multiple conflicting sources | List all sources with which takes precedence and why |
| Cannot reach source URL | Mark "source unverified", lower trust level |
| License unclear | Flag for manual review, do not auto-adopt |
| Skill has no frontmatter | Treat as unknown origin, untrusted |

## Next steps

After recording provenance:
- Run safety review → `skill-safety-review`
- Update lifecycle maturity based on trust level → `skill-lifecycle-management`

## References

- SPDX license identifiers: https://spdx.org/licenses/
