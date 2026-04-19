---
name: implementer-context
description: >
  Trigger — "gather context for implementation", "load project conventions", "what files do I need",
  "trace imports for", "map the API surface", "what patterns does this project use".
  Skip — if the task is writing code (use `implementer-node-agent`), coordinating multiple
  agents (use `implementer-hub`), or running tests (use `qa-validation`).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: implementer-context
  maturity: draft
  risk: low
  tags: [implementer, context]
---

# Purpose

Gathers the right files, conventions, constraints, and dependency information an implementer
agent needs before it starts coding. Produces a structured context bundle that eliminates
guesswork and prevents convention violations during implementation.

# When to use

- Before an implementer agent starts work on a ticket, to load relevant context.
- When a new contributor (human or agent) needs to understand project conventions quickly.
- When tracing which files will be affected by a planned change.
- When mapping the API surface or import chain for a module under modification.

# Do NOT use when

- The task is writing or modifying code — hand off to `implementer-node-agent` or `implementer-hub`.
- The task is searching for external libraries — use `github-prior-art-research`.
- The task is verifying completed work — use `qa-validation`.
- The context is already loaded and the user wants to proceed with implementation.

# Operating procedure

1. Read the ticket or task description to identify the target files, modules, or features to be changed.
2. Run `find . -name '*.ts' -o -name '*.js' -o -name '*.py' -o -name '*.go' -o -name '*.rs' | head -50` to identify the primary language and file layout.
3. Run `cat package.json` or equivalent manifest file to extract: project name, dependencies, scripts, and engine/runtime constraints.
4. Search for configuration files: run `ls -la .eslintrc* .prettierrc* tsconfig.json .editorconfig biome.json pyproject.toml Cargo.toml go.mod 2>/dev/null` to inventory coding conventions.
5. For each target file from step 1, run `grep -n 'import\|require\|from ' <file>` to trace its import chain. List all direct dependencies as a dependency graph.
6. Run `grep -rn 'export ' <target-dir> | head -30` to map the public API surface of the module being modified.
7. Search for test patterns: run `find . -path '*/test*' -o -path '*__tests__*' -o -name '*.test.*' -o -name '*.spec.*' | head -20` to identify the test runner and test file naming convention.
8. Extract coding conventions by sampling 3 existing files in the same directory: check indentation (tabs vs spaces), quote style, semicolon usage, naming conventions (camelCase vs snake_case), and export style (default vs named).
9. Check for AGENTS.md, CONTRIBUTING.md, or .github/CODEOWNERS to find explicit project conventions or ownership rules.
10. Compile the context bundle document with all findings organized into the output sections below.

# Decision rules

- If the target module has >10 direct imports, create a simplified dependency diagram (list only first-level dependencies, not transitive).
- If conflicting conventions are found (e.g., mixed quote styles), report both and note which is more prevalent.
- If no test files exist for the target module, flag this as a gap and recommend the test pattern used elsewhere in the project.
- If the manifest file is missing or empty, infer the stack from file extensions and directory structure.
- Limit context bundle to files within 2 directory levels of the target — do not crawl the entire repo.

# Output requirements

1. **Target Summary** — ticket ID, target files, and one-sentence description of the planned change.
2. **Dependency Graph** — list of imports/exports for each target file showing what it depends on and what depends on it.
3. **Convention Sheet** — table with columns: Convention, Value, Source (e.g., "Indent: 2 spaces, .editorconfig").
4. **API Surface Map** — list of exported functions/classes/types from the target module with signatures.
5. **Test Pattern** — test runner name, test file naming convention, example test file path, and any gaps.
6. **Constraints & Warnings** — runtime requirements, engine version locks, or known gotchas from config files.

# References

- `references/handoff-contract.md` — context bundle format
- `references/success-criteria.md` — what "sufficient context" means
- Project-local AGENTS.md and CONTRIBUTING.md if they exist

# Related skills

- `implementer-hub` — consumes context bundles to coordinate multi-agent implementation
- `implementer-node-agent` — consumes context bundles to write JS/TS code
- `planner` — produces the tickets that trigger context loading
- `qa-validation` — uses test pattern info from context bundles

# Failure handling

- If the target file does not exist yet (new file), gather context from the nearest sibling files in the same directory.
- If no configuration files are found, report "no explicit conventions" and extract implicit conventions from the 3 largest source files.
- If the import chain is circular, note the cycle and list the files involved rather than recursing infinitely.
- If the repository is very large (>1000 files), restrict discovery to the subtree containing the target files.
