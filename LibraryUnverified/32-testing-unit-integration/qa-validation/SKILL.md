---
name: qa-validation
description: >
  Trigger — "verify this works", "run the tests", "check acceptance criteria", "QA this change",
  "validate the implementation", "does this pass", "regression check", "test coverage gaps".
  Skip — if the task is writing code (use an implementer skill), reviewing code style
  (use `code-review`), or checking security vulnerabilities (use `security-review`).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: qa-validation
  maturity: draft
  risk: low
  tags: [qa, validation]
---

# Purpose

Verifies that an implementation meets its acceptance criteria by running existing tests,
identifying coverage gaps, performing regression checks, and collecting pass/fail evidence.
Acts as the tester/QA gate before a change is considered complete.

# When to use

- An implementation is complete and needs verification against acceptance criteria.
- The user asks to "test this", "verify this works", or "check if this passes".
- A PR or changeset needs a QA pass before merge.
- Test coverage gaps need to be identified for a module or feature.

# Do NOT use when

- The task is writing application code — use `implementer-node-agent` or `implementer-hub`.
- The task is reviewing code quality or style — use `code-review`.
- The task is checking for security vulnerabilities — use `security-review`.
- The task is planning what to build — use `planner`.

# Operating procedure

1. Read the ticket or plan to extract the **acceptance criteria**. List each criterion as a numbered checklist item.
2. Run `git --no-pager diff --name-only HEAD~5` to identify which files were recently changed (the implementation under test).
3. Detect the test runner: run `cat package.json 2>/dev/null | grep -E '"test"|jest|vitest|mocha'` for JS/TS, `cat pyproject.toml 2>/dev/null | grep -E 'pytest|unittest'` for Python, or `ls Cargo.toml Makefile 2>/dev/null` for Rust/other.
4. Run the full test suite: execute `npm test`, `pytest`, `cargo test`, `go test ./...`, or the equivalent command found in step 3. Capture the output.
5. Parse test results: extract total tests, passed, failed, skipped, and execution time. List each failing test by name and error message.
6. Map failing tests to changed files: for each failure, run `grep -rn '<test-function-name>' test/` to find the test file, then cross-reference with the changed files from step 2.
7. Identify coverage gaps: run `find . -path '*/test*' -name '*<changed-file-stem>*'` for each changed source file. If no corresponding test file exists, flag the file as **uncovered**.
8. For each acceptance criterion from step 1, determine its status:
   - **PASS** — a test explicitly covers it and passes.
   - **FAIL** — a test covers it but fails.
   - **UNTESTED** — no test covers this criterion.
9. For any UNTESTED criteria, write a concrete test description: what the test should do, expected input, expected output, and which test file it belongs in.
10. Run a regression check: execute the full test suite (not just the affected tests) and confirm no previously-passing tests now fail.
11. If a coverage tool is available (`npx jest --coverage`, `pytest --cov`, `cargo tarpaulin`), run it on the changed files and report line coverage percentage.
12. Compile the final QA report with all sections below.

# Decision rules

- If any acceptance criterion has status FAIL, the implementation is **not approved** — return the failure details to the implementer.
- If more than 30 percent of acceptance criteria are UNTESTED, recommend writing tests before approving.
- If the full test suite has failures unrelated to the current change, note them as **pre-existing** and do not block approval.
- If no test suite exists at all, escalate: recommend setting up a test framework before validating.
- If tests pass but the behavior does not match the acceptance criteria description, mark as FAIL with an explanation.

# Output requirements

1. **Acceptance Criteria Checklist** — table with columns: Number, Criterion, Status (PASS/FAIL/UNTESTED), Evidence (test name or observation).
2. **Test Run Summary** — total tests, passed, failed, skipped, execution time.
3. **Failure Details** — for each failure: test name, error message, file path, and which changed file likely caused it.
4. **Coverage Gaps** — list of changed source files with no corresponding test file.
5. **Regression Report** — count of pre-existing failures vs new failures introduced by this change.
6. **Recommended Tests** — for each UNTESTED criterion: proposed test name, description, input, expected output.
7. **Verdict** — APPROVED, BLOCKED (with reasons), or NEEDS-TESTS (with the specific tests to write).

# References

- `references/success-criteria.md` — acceptance criteria extraction patterns
- `references/anti-patterns.md` — common QA failures (false passes, coverage theater)
- Project-local test configuration files (jest.config, pytest.ini, etc.)

# Related skills

- `planner` — provides the acceptance criteria this skill validates against
- `implementer-hub` — produces the implementation this skill verifies
- `implementer-node-agent` — produces individual file changes to verify
- `docs-handoff` — updates docs after QA approval
- `code-review` — reviews code quality (complementary to QA)

# Failure handling

- If the test runner is not found or not configured, search for test files manually and attempt to run them with `node --test`, `python -m pytest`, or language-appropriate defaults.
- If the test suite hangs (no output for 60 seconds), terminate it and report the hang with the last output line as context.
- If acceptance criteria are missing from the ticket, derive them from the plan's expected outcomes and mark as "derived — needs confirmation".
- If coverage tools are unavailable, perform manual coverage analysis by listing test files and matching them to source files by name.
- If tests pass locally but the user reports failures elsewhere, check for environment-specific issues: Node version, missing env vars, or OS-dependent paths.
