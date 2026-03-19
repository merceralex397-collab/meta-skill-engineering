---
name: skill-packager
description: "Build distributable bundles, manifests, and checksums for publishing or sharing one or more skills. Use when packaging skills for release, building a distribution bundle, creating CI/CD skill artifacts, or releasing a new library version. Do not use for packaging a single skill in isolation (just create its manifest directly) or when skills aren't ready for release."
---

# Purpose

Orchestrate the full packaging workflow for one or more skills: validate, generate overlays, create manifests, and produce release-ready bundles.

# When to use

- Packaging multiple skills for a coordinated release
- Building a distribution bundle for a skill library
- Creating CI/CD skill artifacts
- Releasing a new library version
- User says "package these skills", "build a release bundle", "prepare skills for distribution"

# When NOT to use

- Packaging a single skill in isolation → `skill-packaging` (or just create its manifest directly)
- Skills aren't ready for release — finish authoring first
- Installing packaged skills → `skill-installer`
- Creating new skills → `skill-creator`

# Procedure

### 1. Scan for skills

```bash
# Find all skill directories
find . -name "SKILL.md" -not -path "*/ARCHIVE/*" | sort
```

Filter by maturity if specified (e.g., only `stable` skills).

### 2. Validate each skill

For each skill:
- Parse YAML frontmatter — required fields: `name`, `description`
- Verify referenced files exist
- Check for placeholder text (`[...]`, `TODO`)
- Flag invalid skills, continue with valid ones

### 3. Determine target clients

From each skill's `compatibility.clients` field, or from a global config parameter. Default: all compatible clients per frontmatter.

### 4. Generate overlays

For each valid skill × compatible client, generate client-specific overlay files using overlay-generator patterns.

### 5. Create per-skill manifests

```yaml
name: <skill-name>
version: <semver from git tags or config>
checksum: <sha256 of SKILL.md>
files:
  - SKILL.md
  - metadata.json  # if generated
```

### 6. Build bundles

Per skill: `skill-name-version.tar.gz` containing SKILL.md + overlays + manifest.

Combined (optional): `skills-bundle-version.tar.gz` with all skills + index.

### 7. Generate release notes

```markdown
# Release Notes: v[version]

## Skills Included
| Name | Version | Status |
|------|---------|--------|
| [name] | [ver] | [new | updated | unchanged] |

## Changes
- Added: [new skills]
- Updated: [modified skills]
- Deprecated: [retired skills]
```

## Output contract

```
dist/
├── index.yaml              # Combined manifest
├── skills-bundle-X.Y.Z.tar.gz  # Combined bundle
├── skill-a-X.Y.Z.tar.gz   # Per-skill bundles
└── RELEASE-NOTES.md
```

## Failure handling

- **Some skills invalid**: Package valid ones, report invalid with specific errors
- **Version conflict**: Use highest version or flag for manual resolution
- **Missing git tags**: Fall back to `0.0.0-dev`
- **Overlay generation fails**: Package base without overlay, note limitation in manifest

## References

- Semver: https://semver.org/
- Agent Skills specification: https://agentskills.io/specification
