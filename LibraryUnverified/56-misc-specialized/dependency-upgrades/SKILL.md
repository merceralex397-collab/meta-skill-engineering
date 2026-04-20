---
name: dependency-upgrades
description: Handles version bumps, lockfile churn, changelog review, and compatibility validation for dependency updates. Trigger on 'upgrade dependency', 'Dependabot PR', 'version bump', 'lockfile conflict', 'security patch'. DO NOT USE for full framework migrations like React 17-to-18 rewrites (use migration-refactor) or adding brand-new dependencies to a project (use stack-standards).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: dependency-upgrades
  maturity: draft
  risk: low
  tags: [dependency, upgrades]
---

# Purpose
Execute dependency upgrades safely: distinguish security patches (merge fast) from feature upgrades (test carefully), review changelogs for breaking changes, ensure lockfiles stay pinned, and verify tests pass before merging. Dependabot automates detection; this skill handles the human judgment part.

# When to use this skill
Use when:
- Dependabot/Renovate PR needs review
- Security advisory requires immediate patching
- Major version upgrade planned (e.g., React 17→18)
- Lockfile conflicts need resolution

Do NOT use when:
- Full framework migration (use migration-pack-builder)
- Adding new dependencies (different workflow)
- Lockfile doesn't exist (create one first)

# Operating procedure
1. **Classify the upgrade type**:
   ```
   Security patch (CVSS ≥7): Merge within 24h after tests pass
   Security patch (CVSS <7): Merge within 1 week
   Patch version (x.y.Z):   Low risk, review changelog briefly
   Minor version (x.Y.z):   Medium risk, check for deprecations
   Major version (X.y.z):   High risk, full migration review needed
   ```

2. **Review changelog and release notes**:
   ```bash
   # For npm packages
   npm info <package> changelog
   # Or check GitHub releases page directly
   
   # Look for:
   # - BREAKING CHANGES section
   # - Deprecation notices
   # - Peer dependency changes
   # - Node/Python version requirements
   ```

3. **Check for peer dependency conflicts**:
   ```bash
   # npm
   npm ls <package>  # Check dependency tree
   npm outdated      # See all outdated packages
   
   # Python
   pip check         # Verify no broken requirements
   pipdeptree        # Visualize dependency tree
   ```

4. **Update lockfile correctly**:
   ```bash
   # npm - regenerate lockfile from package.json
   rm -rf node_modules package-lock.json
   npm install
   
   # Python - use pip-compile for reproducibility
   pip-compile requirements.in --upgrade-package <package>
   
   # Cargo
   cargo update -p <package>
   ```

5. **Run full test suite**:
   ```bash
   # Don't just run unit tests - include integration/e2e
   npm test && npm run test:e2e
   
   # Check TypeScript still compiles
   npm run typecheck
   
   # Run linter to catch deprecation warnings
   npm run lint
   ```

6. **Verify runtime behavior** for significant upgrades:
   ```bash
   # Start dev server, click through critical paths
   npm run dev
   
   # Check bundle size didn't explode
   npm run build
   ls -la dist/
   ```

7. **Document upgrade in PR**:
   ```markdown
   ## Dependency Upgrade: [package] v1.2.3 → v2.0.0
   
   ### Changes
   - [Link to changelog]
   - Breaking: X method renamed to Y
   - Deprecation: Z will be removed in v3
   
   ### Verification
   - [x] Tests pass
   - [x] Typecheck passes
   - [x] No new lint warnings
   - [x] Manual smoke test of [feature]
   
   ### Migration Required
   - Updated calls to X → Y in src/utils.ts
   ```

# Output defaults
```markdown
## Dependency Upgrade Checklist

### Package: [name]
- Current: [version]
- Target: [version]
- Type: Security / Patch / Minor / Major

### Review
- [ ] Changelog reviewed for breaking changes
- [ ] Peer dependencies compatible
- [ ] No deprecation warnings affect our code

### Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] TypeScript/linting passes
- [ ] Bundle size acceptable
- [ ] Manual verification of affected features

### Rollback
- Revert commit: `git revert [SHA]`
- Restore lockfile: `git checkout HEAD~1 -- package-lock.json && npm ci`
```

# References
- https://docs.github.com/en/code-security/dependabot/working-with-dependabot
- https://docs.npmjs.com/cli/v10/commands/npm-update

# Failure handling
- **Conflicting peer dependencies**: Check if other packages need updating together; may need to upgrade as a group
- **Tests fail after upgrade**: Read test output carefully—often the test needs updating, not the code
- **TypeScript errors**: Check if @types package needs separate update; may lag behind main package
- **Breaking change not in changelog**: Check GitHub issues and PRs for migration guidance; document what you learn
- **Lockfile merge conflicts**: Regenerate lockfile; never manually resolve lockfile conflicts
