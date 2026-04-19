---
name: release-notes
description: "Generate release notes from commits, PRs, and tags — group changes by type (features, fixes, breaking), follow semantic versioning, write audience-appropriate summaries, and call out migration steps for breaking changes. Use when preparing release documentation, automating changelogs, or writing upgrade guides. Do not use for internal decision records (prefer adr-rfc-writing) or operational runbooks."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: release-notes
  maturity: draft
  risk: low
  tags: [release-notes, changelog, semver, breaking-changes]
---

# Purpose

Generate release notes from commits, pull requests, and tags. Group changes by type, follow semantic versioning conventions, write audience-appropriate summaries, highlight breaking changes with migration steps, and produce documentation suitable for CHANGELOG files, GitHub Releases, and customer-facing announcements.

# When to use this skill

- Preparing release documentation for a new version tag or deployment.
- Automating changelog generation from git history or PR metadata.
- Writing upgrade guides that call out breaking changes and required migration steps.
- Producing audience-targeted release summaries (developer-facing vs. end-user-facing).
- Generating release notes for GitHub Releases, npm publish, PyPI upload, or similar distribution channels.

# Do not use this skill

- For internal architecture decision records — prefer `adr-rfc-writing`.
- For operational runbooks or incident response procedures — prefer `runbook-writing`.
- For detailed technical specifications — prefer `spec-authoring`.
- For commit message formatting or conventional commits enforcement — that is a linting/CI concern.

# Operating procedure

1. **Determine the version range.** Identify the previous release tag and the current release tag or HEAD. Run `git log --oneline <prev-tag>...<curr-tag>` to collect all commits in scope.
2. **Collect PR metadata.** For each merge commit, extract the PR number, title, labels, and author. Use `gh pr list --state merged --base main --search "merged:>YYYY-MM-DD"` or query the GitHub API for richer metadata.
3. **Classify changes by type.** Group into categories: **Breaking Changes**, **New Features**, **Bug Fixes**, **Performance Improvements**, **Documentation**, **Dependencies**, **Internal/Chores**. Use PR labels (e.g., `breaking`, `feature`, `bugfix`) as the primary signal. Fall back to conventional commit prefixes (`feat:`, `fix:`, `perf:`) when labels are absent.
4. **Determine the version bump.** Apply semantic versioning: MAJOR for breaking changes, MINOR for new features, PATCH for bug fixes. If the repo uses a different versioning scheme, follow it and document the rule.
5. **Write the Breaking Changes section first.** For each breaking change: describe what changed, why, and the exact migration steps. Include before/after code snippets when API signatures or config formats changed.
6. **Write summaries for each category.** Each entry gets one line: a concise description, the PR number as a link, and the author. Group related PRs into a single entry when they form a logical unit.
7. **Add a highlights section.** Write 2-4 sentences summarizing the most impactful changes for the target audience. For end-user notes, focus on visible behavior changes. For developer notes, focus on API changes and new capabilities.
8. **Include upgrade instructions.** List prerequisite steps (dependency updates, config changes, data migrations) in execution order. Provide rollback guidance if the upgrade is reversible.
9. **Format the output.** Use Markdown with `##` headers for each category. Include the version number and release date at the top. Link PR numbers to the repository.
10. **Review for completeness.** Verify every merged PR appears in the notes. Cross-check breaking changes against the diff. Confirm version bump logic matches the content.

# Decision rules

- Breaking changes always appear first and in bold — they are the most critical information for consumers.
- When a PR has no label and no conventional commit prefix, classify it as Internal/Chores.
- Omit pure refactoring or CI-only changes from user-facing release notes — include them only in developer-facing changelogs.
- If a single PR contains both a feature and a breaking change, list it under Breaking Changes with a note about the new feature.
- Use the PR title as the default summary — rewrite only if the title is unclear or misleading.
- Attribute each entry to the PR author unless the project convention omits attribution.

# Output requirements

1. A Markdown document with version number, release date, and categorized change entries.
2. A **Breaking Changes** section with migration steps (even if empty — state "None" explicitly).
3. Each entry links to the source PR or commit.
4. A highlights summary of 2-4 sentences at the top for quick scanning.
5. Upgrade instructions section when breaking changes or dependency updates are present.

# References

- Semantic Versioning specification: https://semver.org/
- Keep a Changelog format: https://keepachangelog.com/
- Conventional Commits: https://www.conventionalcommits.org/
- GitHub Releases API: https://docs.github.com/en/rest/releases

# Related skills

- `document-writing` — for longer-form narrative documentation around releases.
- `adr-rfc-writing` — for decision records that explain why changes were made.
- `runbook-writing` — for operational deployment procedures that accompany releases.

# Anti-patterns

- Dumping raw `git log` output as release notes — unreadable and useless to consumers.
- Omitting breaking changes or burying them at the bottom — causes upgrade failures and erodes trust.
- Writing vague entries like "various improvements" or "bug fixes" without specifics.
- Mixing developer jargon into end-user-facing notes without audience adaptation.
- Skipping the version bump analysis and manually picking a version number without checking for breaking changes.

# Failure handling

- If no commits exist between the two tags, report "No changes found between <prev> and <curr>" and halt.
- If PR metadata is unavailable (no GitHub access), fall back to commit messages and note the reduced fidelity.
- If breaking changes are detected but no migration steps are provided by the PR authors, flag each one as "Migration steps needed — contact PR author" rather than omitting.
- If the version range is ambiguous (no previous tag), prompt for clarification or default to the full history since the last tagged release.
- If conventional commit prefixes are inconsistent, classify by diff analysis (new files = feature, deleted tests = suspect) and flag uncertain classifications.
