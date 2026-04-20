---
name: repo-evidence-gathering
description: >
  Systematically collects facts from a repository to support architectural, planning, or review decisions.
  Trigger — "gather evidence from the repo", "what does this codebase use", "inventory the project",
  "analyze dependencies", "detect conventions", "what's the tech stack".
  Skip — task requires code edits, performance benchmarking, or runtime profiling.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: repo-evidence-gathering
  maturity: draft
  risk: low
  tags: [repo, evidence, gathering]
---

# Purpose

Gather structured, factual evidence from a repository — file inventory, dependency graphs,
test coverage signals, coding conventions, and tech-stack identifiers — so that downstream
agents (planners, reviewers, architects) can make decisions grounded in reality rather than
assumptions.

# When to use

- A planning or review agent needs verified facts about the repo before acting.
- You need a tech-stack summary, dependency tree, or convention report for a new codebase.
- A ticket or architecture decision requires evidence about test coverage or code patterns.
- Onboarding a new contributor who needs a concise codebase orientation.

# Do NOT use when

- The task requires modifying code — hand off to an implementer skill instead.
- Runtime performance profiling or load testing is needed (use a benchmarking skill).
- The question is about external services, not the repository itself.
- A security-specific audit is required — use `security-review` instead.

# Operating procedure

1. Run `find . -maxdepth 2 -type f | head -80` and `ls -la` at repo root to build an initial file-tree inventory.
2. Identify the package manifest(s) — search for `package.json`, `Cargo.toml`, `pyproject.toml`, `go.mod`, `pom.xml`, `Gemfile` using `find . -maxdepth 3 -name '<manifest>'`.
3. Extract direct dependencies from each manifest and list them in a table: `| Dependency | Version | Purpose (inferred) |`.
4. Run `grep -r 'import\|require\|from ' --include='*.ts' --include='*.py' --include='*.go' -l | head -30` to map internal module relationships.
5. Locate test directories by running `find . -type d -name '__tests__' -o -name 'test' -o -name 'tests' -o -name 'spec'` and count test files per directory.
6. Detect coding conventions: run `head -40` on 3–5 representative source files and note indent style, naming conventions (camelCase vs snake_case), import order, and comment patterns.
7. Identify CI/CD configuration by checking for `.github/workflows/`, `Jenkinsfile`, `.gitlab-ci.yml`, `Makefile`, or `Taskfile.yml`.
8. Check for documentation artifacts: `README.md`, `AGENTS.md`, `CONTRIBUTING.md`, `docs/` directory.
9. Summarise all findings into the output format below, citing file paths and line numbers for every claim.
10. Flag any gaps — missing tests, undocumented modules, stale dependencies — as open questions for downstream agents.

# Decision rules

- Every claim must cite at least one file path. No unsupported assertions.
- If a file is too large to read fully, sample the first 50 and last 20 lines rather than skipping it.
- Prefer breadth over depth: cover all top-level directories before diving deep into any one.
- When two conventions conflict (e.g., mixed indent styles), report both with file-path examples.
- Do not infer intent — report what the code does, not what it was probably meant to do.

# Output requirements

1. **Tech Stack Summary** — languages, frameworks, runtimes with version evidence.
2. **Dependency Inventory** — table of direct dependencies per manifest.
3. **File Structure Map** — top-level directory tree with purpose annotations.
4. **Convention Report** — indent, naming, import, and comment patterns observed.
5. **Test Coverage Signal** — test directory locations, file counts, and runner configuration.
6. **CI/CD Overview** — pipelines detected, triggers, and key steps.
7. **Gap List** — missing docs, untested modules, stale configs.

# References

- Project manifest files (`package.json`, `Cargo.toml`, `pyproject.toml`, etc.)
- CI/CD workflow files (`.github/workflows/*.yml`)
- `AGENTS.md` or `CONTRIBUTING.md` if present

# Related skills

- `shell-inspection` — for runtime environment checks complementing repo evidence
- `security-review` — consumes evidence gathered here for vulnerability analysis
- `ticket-creator` — turns gap-list findings into actionable tickets
- `context-summarization` — condenses evidence reports for downstream agents

# Failure handling

- If the repository is empty or has fewer than 3 files, report "insufficient codebase" and stop.
- If a manifest file is unparseable, quote the first 10 lines and flag it as corrupt.
- If file-system access is restricted, list which paths failed and continue with accessible ones.
- If conventions are contradictory across the repo, present both variants with counts rather than picking one.
