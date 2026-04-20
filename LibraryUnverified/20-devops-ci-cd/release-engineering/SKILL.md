---
name: release-engineering
description: Execute releases using semantic versioning and conventional commits — version bump, changelog generation, git tags, and package publishing. Use when the user says "cut a release", "version bump", "changelog", "tag and publish", "semantic versioning", or when shipping work needs a repeatable release procedure. Do not use for deployment-pipeline (CI/CD orchestration), dependency-upgrades (consuming upstream updates), or docs-and-handoff (documentation-only updates).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: release-engineering
  maturity: draft
  risk: low
  tags: [release, engineering]
---

# Purpose
Execute releases using semantic versioning and conventional commits: determine version bump from commit history, generate changelogs automatically, create git tags and GitHub releases. This makes releases predictable and automated rather than manual and error-prone.

# When to use this skill
Use when:
- Cutting a new release of a library/package
- Setting up automated release pipeline
- Writing release notes for a deployment
- Determining what version number to use

Do NOT use when:
- Continuous deployment without versioning (use deployment-pipeline)
- Internal tools that don't need formal releases

# Operating procedure
1. **Follow Semantic Versioning (SemVer)**:
   ```
   Given version MAJOR.MINOR.PATCH:
   
   MAJOR (1.0.0 → 2.0.0): Breaking API changes
     - Removed public function
     - Changed function signature
     - Renamed public class
   
   MINOR (1.0.0 → 1.1.0): New features, backwards compatible
     - Added new endpoint
     - Added optional parameter
     - Added new class
   
   PATCH (1.0.0 → 1.0.1): Bug fixes, backwards compatible
     - Fixed null pointer
     - Corrected calculation
     - Updated documentation
   
   Pre-release: 1.0.0-alpha.1, 1.0.0-beta.2, 1.0.0-rc.1
   ```

2. **Use Conventional Commits** (enables automation):
   ```
   Format: <type>[optional scope]: <description>
   
   Types and their version impact:
   ─────────────────────────────────────────────────
   feat:     → MINOR bump (new feature)
   fix:      → PATCH bump (bug fix)
   docs:     → No release (documentation only)
   style:    → No release (formatting)
   refactor: → No release (code change, no behavior change)
   perf:     → PATCH bump (performance improvement)
   test:     → No release (adding tests)
   chore:    → No release (maintenance)
   
   BREAKING CHANGE in footer or ! after type → MAJOR bump
   
   Examples:
   feat(api): add user search endpoint
   fix(auth): handle expired tokens correctly
   feat(api)!: change response format for /users
   
   feat: redesign settings page
   
   BREAKING CHANGE: settings are now stored per-workspace
   ```

3. **Generate changelog from commits**:
   ```bash
   # Using conventional-changelog
   npx conventional-changelog -p angular -i CHANGELOG.md -s
   
   # Or manually structured:
   ## [1.2.0] - 2024-01-15
   
   ### Added
   - User search endpoint (#123)
   - Export to CSV feature (#145)
   
   ### Fixed
   - Token expiration handling (#134)
   - Memory leak in background worker (#156)
   
   ### Changed
   - Improved error messages for validation failures
   
   ### Breaking Changes
   - Response format for /users changed (see migration guide)
   ```

4. **Create release workflow**:
   ```yaml
   # .github/workflows/release.yml
   name: Release
   
   on:
     push:
       tags:
         - 'v*'
   
   jobs:
     release:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
           with:
             fetch-depth: 0  # Need full history for changelog
         
         - name: Build
           run: npm ci && npm run build
         
         - name: Generate changelog
           id: changelog
           run: |
             npx conventional-changelog -p angular -r 1 > RELEASE_NOTES.md
         
         - name: Create GitHub Release
           uses: softprops/action-gh-release@v1
           with:
             body_path: RELEASE_NOTES.md
             files: |
               dist/*.tar.gz
               dist/*.zip
   ```

5. **Tag releases properly**:
   ```bash
   # After version bump in package.json/Cargo.toml/etc.
   git add -A
   git commit -m "chore(release): 1.2.0"
   git tag -a v1.2.0 -m "Release v1.2.0"
   git push && git push --tags
   
   # Use annotated tags (-a) for releases
   # Lightweight tags for temporary markers
   ```

6. **Publish to package registries**:
   ```bash
   # npm
   npm publish --access public
   
   # PyPI
   python -m build
   twine upload dist/*
   
   # crates.io
   cargo publish
   
   # GitHub Packages
   npm publish --registry=https://npm.pkg.github.com
   ```

7. **Version bump automation**:
   ```bash
   # Let tool determine version from commits
   npx standard-version        # npm/node
   npx semantic-release        # Fully automated
   cargo release minor         # Rust
   
   # Or manually:
   npm version minor           # 1.1.0 → 1.2.0
   npm version patch           # 1.2.0 → 1.2.1
   npm version major           # 1.2.1 → 2.0.0
   ```

# Output defaults
```markdown
## Release Checklist: v[X.Y.Z]

### Pre-release
- [ ] All tests passing on main
- [ ] CHANGELOG.md updated
- [ ] Version bumped in package files
- [ ] Breaking changes documented with migration guide
- [ ] Release notes drafted

### Release
- [ ] Tag created: `git tag -a vX.Y.Z -m "Release vX.Y.Z"`
- [ ] Tag pushed: `git push --tags`
- [ ] GitHub Release created with notes
- [ ] Package published to registry
- [ ] Documentation updated

### Post-release
- [ ] Announce in relevant channels
- [ ] Monitor for issues
- [ ] Update dependent projects

### Release Notes Template
## What's New in vX.Y.Z

### Highlights
- [Major feature or fix]

### Full Changelog
[Link to generated changelog]

### Breaking Changes
[Migration instructions if any]

### Contributors
@contributor1, @contributor2
```

# References
- https://semver.org/
- https://www.conventionalcommits.org/en/v1.0.0/

# Failure handling
- **Accidental breaking change released as minor**: Release new major version immediately with same changes; document in changelog
- **Tag pushed but release failed**: Delete tag (`git tag -d vX.Y.Z && git push --delete origin vX.Y.Z`), fix issue, re-tag
- **Package registry publish failed**: Check credentials, retry; most registries allow republish within short window
- **Changelog generator missed commits**: Ensure commits follow conventional format; manually add missing entries
- **Version conflict in registry**: Cannot overwrite published versions; bump version and republish
