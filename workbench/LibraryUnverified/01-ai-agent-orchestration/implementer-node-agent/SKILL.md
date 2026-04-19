---
name: implementer-node-agent
description: >
  Trigger — "implement in TypeScript", "write the JS module", "add this Node endpoint",
  "fix this TS file", "create a React component", "update the Express route", "npm package changes".
  Skip — if the task involves multiple modules (use `implementer-hub`), if context is not
  loaded (use `implementer-context`), or if the language is not JavaScript/TypeScript.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: implementer-node-agent
  maturity: draft
  risk: low
  tags: [implementer, node, agent]
---

# Purpose

Writes and modifies JavaScript and TypeScript code in a single file or tightly-coupled
file pair (source + test), following the project's existing conventions for module format,
linting, typing, and test patterns.

# When to use

- A plan or ticket requires creating or modifying a `.js`, `.ts`, `.jsx`, or `.tsx` file.
- A bug fix or feature addition targets a specific Node.js/TypeScript module.
- A test file needs to be created or updated alongside a source file.
- A package.json script, dependency, or configuration needs modification.

# Do NOT use when

- The change spans 3+ files across different modules — use `implementer-hub` for coordination.
- The target language is Python, Rust, Go, or another non-JS/TS language.
- The task is gathering context, not writing code — use `implementer-context`.
- The task is running or evaluating tests — use `qa-validation`.

# Operating procedure

1. Read the context bundle (from `implementer-context`) or the ticket description to identify: target file path, required changes, and acceptance criteria.
2. Run `cat tsconfig.json 2>/dev/null || echo "no tsconfig"` to detect TypeScript mode. Record: strict mode (true/false), target (ES2020, ESNext, etc.), module format (ESM/CJS), and path aliases.
3. Run `cat package.json | grep -A5 '"type"'` to determine if the project uses `"type": "module"` (ESM) or defaults to CJS. Record the module format.
4. Run `cat .eslintrc* .prettierrc* biome.json 2>/dev/null | head -40` to extract lint and format rules: quote style, semicolons, indent width, trailing commas.
5. If the target file exists, read it fully. Identify: existing imports, exported symbols, function signatures, and inline types or interfaces.
6. If the target file does not exist, examine 2 sibling files in the same directory to extract the file template pattern: header comments, import ordering, export style.
7. Write the implementation following these conventions exactly: match the import style (named vs default, path aliases vs relative), match the export style, preserve the existing indentation and formatting.
8. If a test file is required, locate the corresponding test directory or co-located test pattern. Run `ls *test* *spec* __tests__/ 2>/dev/null` in the target directory. Create the test file using the same runner (jest, vitest, mocha, node:test) and assertion style found in existing tests.
9. Run the project linter on the changed file: `npx eslint <file> --fix 2>/dev/null || npx biome check <file> --fix 2>/dev/null` to auto-fix formatting issues.
10. Run the specific test file: `npx jest <test-file> 2>/dev/null || npx vitest run <test-file> 2>/dev/null || node --test <test-file> 2>/dev/null`. Record pass/fail.
11. If tests fail, read the error output, identify the root cause, fix the code, and re-run. Repeat up to 3 times.
12. Run `git --no-pager diff --stat` to confirm only the expected files were modified.

# Decision rules

- If the project uses ESM (`"type": "module"` or `.mjs`), never use `require()` — always use `import`.
- If `tsconfig.json` has `"strict": true`, ensure all variables have explicit types and no `any` is introduced.
- If the project has path aliases (e.g., `@/utils`), use them instead of relative paths that traverse >2 levels.
- If no test runner is detected, write tests using Node.js built-in `node:test` and `node:assert`.
- If a dependency is needed that is not in package.json, add it with `npm install <pkg>` before importing it.
- Never modify files outside the scope specified in the ticket or plan.

# Output requirements

1. **Files Changed** — list of created or modified files with a one-line summary of each change.
2. **Convention Compliance** — confirmation that the implementation matches: module format, lint rules, type strictness, and naming conventions.
3. **Test Results** — test file path, pass/fail count, and any error output from failures.
4. **Dependency Changes** — any packages added or removed, with justification.
5. **Code Diff** — the full `git diff` output for review.

# References

- Node.js ESM documentation: https://nodejs.org/api/esm.html
- TypeScript strict mode: https://www.typescriptlang.org/tsconfig#strict
- Project-local tsconfig.json, package.json, and linter configs

# Related skills

- `implementer-context` — provides the context bundle this skill consumes
- `implementer-hub` — dispatches work to this skill for multi-file changes
- `planner` — produces the tickets and plans this skill implements
- `qa-validation` — validates the output of this skill against acceptance criteria

# Failure handling

- If the linter produces errors that `--fix` cannot resolve, list the remaining errors and apply manual fixes following the reported rule names.
- If the test runner is unrecognized, fall back to `node --test` with `node:assert/strict`.
- If TypeScript compilation fails, run `npx tsc --noEmit <file>` to get the exact error and fix type mismatches.
- If the target file has merge conflict markers (`<<<<<<<`), resolve the conflict before making any changes and flag this in the output.
- If the implementation requires breaking a public API, document the breaking change and suggest a migration path.
