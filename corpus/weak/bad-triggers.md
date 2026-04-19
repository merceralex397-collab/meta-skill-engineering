---
name: bad-triggers-example
description: >
  Helps with code and development tasks, making things better and
  improving quality for teams working on software projects.
---

# Code Quality Assistant

## Purpose

Improve the overall quality of code in a software project by reviewing
changes, suggesting enhancements, and helping developers follow good
practices during their regular development workflow.

## When to use

- When working on code that needs improvement.
- When a developer wants help with their project.
- When something about the software could be better.
- When changes are being made to the codebase.

## When NOT to use

- When there is no code involved at all.

## Procedure

1. Identify the files that have been changed in the current branch by
   diffing against the base branch (`git diff --name-only main`).
2. For each changed file, parse the AST using the appropriate language
   parser (Tree-sitter for supported languages, regex fallback otherwise).
3. Run the project's configured linter and collect any new violations
   introduced by the changes (compare against baseline lint report).
4. Check function complexity using cyclomatic complexity analysis. Flag
   any function exceeding a threshold of 15.
5. Verify that new public functions have docstrings or JSDoc comments.
   Generate draft documentation for any that are missing.
6. Check for common anti-patterns:
   - Nested callbacks deeper than 3 levels.
   - Raw SQL strings without parameterized queries.
   - Hard-coded secrets or credentials.
   - Unused imports or variables.
7. Produce a review summary listing all findings grouped by severity
   (critical, warning, info).

## Output contract

- `review-summary.md` — Findings grouped by severity with file paths and
  line numbers for each issue.
- Exit code 0 if no critical findings; exit code 1 otherwise.

## Failure handling

- If a file cannot be parsed, skip it and note the parse failure in the
  review summary.
- If the project's linter is not configured, skip linting and recommend
  linter setup in the output.

## Next steps

- Feed critical findings into `ticket-pack-builder` for remediation
  tracking.
- Run `skill-evaluation` on the review summary to assess coverage.
