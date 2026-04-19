---
name: format-traps-example
description: Validates CI/CD pipeline configs: GitHub Actions, GitLab CI, and CircleCI
	indented with a tab here
---

# CI Pipeline Validator

## Purpose

Validate CI/CD pipeline configuration files for syntax correctness,
security best practices, and performance anti-patterns before they are
committed. Supports GitHub Actions workflows, GitLab CI pipelines, and
CircleCI configurations.

## When to use

- Before committing changes to `.github/workflows/`, `.gitlab-ci.yml`,
  or `.circleci/config.yml`.
- As a pre-merge check in pull requests that modify pipeline files.
- During periodic CI hygiene audits.

## When NOT to use

- For monitoring live pipeline run failures — use `ci-monitor` instead.
- For generating new pipeline configurations — use `ci-scaffold` instead.
- For secrets management in CI — use `ci-secrets-manager` instead.

## Procedure

1. Detect pipeline files by scanning the repository for known CI config
   paths:
   - `.github/workflows/*.yml` and `.github/workflows/*.yaml`
   - `.gitlab-ci.yml` and `ci/*.gitlab-ci.yml`
   - `.circleci/config.yml`

2. For each detected file, parse the YAML content. Use strict mode that
   rejects duplicate keys and non-string mapping keys.

3. Validate the parsed structure against the platform-specific schema:
   - GitHub Actions: validate against the official workflow JSON Schema.
   - GitLab CI: validate `stages`, `rules`, `needs` DAG consistency.
   - CircleCI: validate orb references, executor declarations, workflow
     job ordering.

4. Run security checks on every pipeline file:
   - Flag any step that uses `${{ github.event.pull_request.title }}`
     or similar user-controlled inputs in `run:` blocks (injection risk).
   - Flag actions/orbs pinned to a mutable tag (`@main`, `@latest`)
     instead of a SHA or semver.
   - Flag secrets passed via environment variables to third-party actions.
   - Flag `pull_request_target` triggers with checkout of PR head.

5. Run performance checks:
   - Flag jobs that could run in parallel but are serialized via
     unnecessary `needs` dependencies.
   - Flag missing caching for package managers (npm, pip, maven).
   - Flag large Docker images used as runners when slim alternatives
     exist.

6. Produce a validation report grouped by file, with findings sorted
   by severity (critical > warning > info).

## Output contract

- `ci-validation-report.md` — Findings per file with severity, location
  (file:line), and remediation guidance.
- Exit code 0 if no critical findings; exit code 1 if any critical finding
  exists.

## Failure handling

- If a YAML file cannot be parsed at all, report the parse error with
  line number and skip further validation for that file.
- If the platform-specific schema is unavailable (e.g., network error
  fetching the GitHub Actions schema), fall back to structural heuristics
  and note reduced validation coverage.
- If the repository contains no CI configuration files, exit with code 0
  and a note that no files were found.

## Next steps

- Route critical findings to `ticket-pack-builder` for tracking.
- Run `skill-evaluation` on the validation report for meta-analysis.
- Feed security findings into `skill-safety-review` for deeper audit.
