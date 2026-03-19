---
name: skill-packaging
description: >-
  Bundle a completed skill folder into a versioned distributable archive
  with manifest, integrity checksums, and client-specific overlays. Use when
  a user says "package this skill", "bundle for distribution", "prepare a
  versioned release", "generate overlays for this skill", or "publish to
  multiple agent clients". Do not use for installing bundles
  (use skill-installer), writing new skills (use skill-creator), or
  documenting skill origin and trust chain (use skill-provenance).
---

# Purpose

Bundle a finished skill folder into a distributable archive (tar.gz or zip) containing a manifest, per-file SHA-256 checksums, and optional client-specific overlays so the skill can be versioned, shared, and installed elsewhere.

# When to use

- User asks to package, bundle, or prepare a skill for distribution
- A skill needs a versioned release artifact
- Publishing a skill to a registry or transferring it between repos

# When NOT to use

- Skill is still being written or refined — finish authoring first
- User wants to install a bundle — use **skill-installer**
- User wants to create a skill from scratch — use **skill-creator**
- User wants to document origin or trust chain — use **skill-provenance**
- Simple folder copy with no versioning or integrity needs

# Procedure

## 1. Validate the skill folder

- Confirm `SKILL.md` exists and has valid YAML frontmatter with `name`, `description`, and `license`
- Walk every path referenced in SKILL.md (scripts/, references/) — fail if any file is missing
- Reject skills whose frontmatter `name` is empty or whose `description` is under 20 characters

## 2. Build the manifest

Create `manifest.yaml` at the bundle root:

```yaml
name: <from frontmatter>
version: <semver — prompt user if absent>
description: <from frontmatter>
license: <from frontmatter>
files:          # exhaustive list, no globs
  - SKILL.md
  - scripts/validate.sh
  # … every included file
checksums:      # per-file SHA-256
  SKILL.md: af3b…
  scripts/validate.sh: 9c01…
compatibility:
  clients: <from frontmatter or prompt>
```

- List every file explicitly — no wildcards
- Compute SHA-256 per file, not a single concatenated hash
- If `version` is missing from frontmatter, ask the user; do not default to `1.0.0`

**Version format — use semver (MAJOR.MINOR.PATCH):**
- MAJOR: breaking changes to SKILL.md structure, output format, or removed procedure steps
- MINOR: new procedure steps, new reference files, expanded output contract
- PATCH: typos, wording fixes, example updates that don't change behavior

**Required vs optional fields:**
- Required: `name`, `version`, `description`, `license`, `compatibility`. If any required field is missing, **stop and report the specific missing field** — never silently default or skip.
- Optional: `resources`, `scripts`, `evals`. Omit from manifest if the skill folder doesn't contain them.

## 3. Generate client overlays (only when needed)

If the skill targets multiple agent clients, generate per-client overlay files.

**Supported overlay formats:**

Copilot (`overlays/copilot/metadata.json`):
```json
{
  "name": "<name>",
  "version": "<version>",
  "description": "<description>",
  "main": "SKILL.md"
}
```

OpenCode (`overlays/opencode/permissions.yaml`):
```yaml
name: <name>
description: "<description>"
permissions:
  read: ["src/**/*", "tests/**/*", "docs/**/*"]
  write: ["src/**/*", "tests/**/*"]
```

Codex (`overlays/codex/openai.yaml`):
```yaml
name: <name>
description: "<description>"
schema_version: "1.0"
capabilities: [file_read, file_write, shell_execute]
```

Overlays must not contradict the base manifest — they extend it.
All overlays must reference the same name and description as the canonical SKILL.md.

Skip this step entirely when the skill is client-agnostic or targets a single client.

For batch overlay generation across a skill library, iterate over all skill
directories and generate overlays for each.

## 4. Create the archive

- Default format: `tar.gz`. Use `zip` only if the user requests it.
- Name: `<name>-<version>.tar.gz`
- Include: `manifest.yaml`, `SKILL.md`, and every file listed in the manifest
- Exclude: `.git/`, `node_modules/`, `__pycache__/`, `.DS_Store`

**Pre-archive validation checklist:**
- SKILL.md must be present and parseable — valid YAML frontmatter + Markdown body
- Every file listed in `manifest.yaml` `files:` must exist on disk
- No files in the archive root that aren't listed in the manifest — stray files indicate an incomplete manifest or leftover artifacts. Scan the folder and reject or warn.
- Total uncompressed size: warn if >100KB. Skills should be lean; large archives usually indicate reference material that should be extracted or binary files that don't belong.

## 5. Verify the archive

- Extract to a temp directory
- Confirm every file in `manifest.yaml` is present
- Re-hash each file and compare to manifest checksums
- Confirm SKILL.md frontmatter still parses after extraction
- Delete the temp directory

## 6. Report results

Print the verification report (see Output contract).

# Output contract

Every successful run produces exactly:

1. **The archive file** at the path the user specified (or cwd)
2. **A verification report** printed to the user:

```
Archive: skill-name-0.2.0.tar.gz (14 KB)
Files:   5 packaged, 5 verified
Checksums: all matched

Contents:
  manifest.yaml
  SKILL.md
  scripts/validate.sh
  references/schema.md
  overlays/copilot.yaml

Ready to install:
  skill-installer add ./skill-name-0.2.0.tar.gz
```

If verification fails, print the specific mismatches and do **not** declare success.

# Failure handling

| Failure | Action |
|---|---|
| `SKILL.md` missing or unparseable frontmatter | Stop. Print which required fields are missing or malformed. |
| Referenced file does not exist | Stop. List every missing path so the user can fix all at once. |
| No `version` in frontmatter | Ask the user for a semver version. Do not guess. |
| Checksum mismatch after extraction | Print the file(s) that differ and the expected vs actual hashes. Re-bundle. |
| Overlay contradicts base manifest | Stop. Explain the conflict. Overlays may only add or narrow, not replace base values. |

## Next steps

After packaging:
- Install the package → `skill-installer`
- Register in the catalog → `skill-registry-manager`
- Manage lifecycle state → `skill-lifecycle-management`

## References

- Agent Skills specification: https://agentskills.io/specification
- Semantic Versioning: https://semver.org/
