---
name: github-prior-art-research
description: >
  Trigger — "find existing implementations", "search GitHub for", "is there a library for",
  "prior art", "has anyone built", "existing solutions for", "look for similar repos".
  Skip — if the user already has a chosen library and wants integration help. Prefer
  `implementer-node-agent` for writing code and `planner` for task decomposition.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: github-prior-art-research
  maturity: draft
  risk: low
  tags: [github, prior, art, research]
---

# Purpose

Searches GitHub for existing implementations, patterns, libraries, and solutions before
building from scratch. Returns structured findings with quality signals, license
compatibility data, and an adopt-vs-adapt-vs-build recommendation.

# When to use

- Before starting a new module or feature, to check if a quality implementation already exists.
- When evaluating whether to adopt a dependency or build in-house.
- When the user asks "has anyone built X" or "find a library for Y".
- When a planner needs prior art evidence to inform scope and effort estimates.

# Do NOT use when

- The user has already chosen a specific library and needs integration help — use an implementer skill.
- The task is code review or QA — use `code-review` or `qa-validation`.
- The search target is internal/private repositories the agent cannot access.
- The user wants documentation updates — use `docs-handoff`.

# Operating procedure

1. Extract the search intent: identify the specific functionality, language, and ecosystem (e.g., "TypeScript JWT validation library").
2. Construct 3–5 GitHub search queries using search syntax: `language:X topic:Y stars:>50`, `"function-name" language:X`, and `in:readme "keyword"`.
3. Execute each query using the GitHub code search or repository search tool. Record the top 5 results per query.
4. Deduplicate results across queries into a single candidate list.
5. For each candidate repository, collect: stars, last commit date, open issues count, license (SPDX identifier), presence of tests (check for `test/`, `__tests__/`, or `*_test.*` patterns), and CI badge status.
6. Run a license compatibility check: list the candidate's license and compare against the target project's license. Flag any copyleft (GPL, AGPL) candidates when the target is permissive (MIT, Apache-2.0).
7. Score each candidate on a 1–5 scale across: activity (commits in last 6 months), test coverage (test directory exists + CI passes), documentation quality (README length > 500 chars + examples present), community (stars > 100, multiple contributors), and license compatibility.
8. Classify each candidate as: **Adopt** (use as dependency), **Adapt** (fork and modify), or **Skip** (insufficient quality or compatibility).
9. If no suitable candidates are found, note the gap and recommend building from scratch with specific design hints gathered from the partial matches.
10. Compile the final report with the ranked candidate table and a clear recommendation.

# Decision rules

- Reject any candidate with no commits in the last 12 months unless it is explicitly "complete/stable" (e.g., a spec implementation).
- Reject any candidate whose license is incompatible with the target project.
- Prefer candidates with >80% of: tests present, CI configured, >200 stars, >3 contributors.
- If two candidates are close in score, prefer the one with more recent activity.
- Never recommend adopting a dependency with known unpatched CVEs — check the GitHub Security tab.

# Output requirements

1. **Search Queries Used** — list of exact queries executed.
2. **Candidate Table** — columns: Repo, Stars, Last Commit, License, Tests, CI, Score, Verdict (Adopt/Adapt/Skip).
3. **Top Recommendation** — 2–3 sentences on the best option with justification.
4. **License Compatibility Summary** — one line per candidate confirming compatible/incompatible.
5. **Build-From-Scratch Notes** — if no candidate qualifies, list the design patterns observed in partial matches that should inform a custom implementation.

# References

- GitHub code search syntax: https://docs.github.com/en/search-github/searching-on-github/searching-code
- SPDX license list: https://spdx.org/licenses/
- `references/success-criteria.md`

# Related skills

- `planner` — consumes prior art findings to inform implementation plans
- `implementer-node-agent` — implements the chosen solution
- `implementer-context` — loads context for the chosen library integration
- `docs-handoff` — documents the adoption decision

# Failure handling

- If GitHub search API is unavailable or rate-limited, fall back to `gh search repos` CLI command with the same query terms.
- If no results are found for any query, broaden the search: remove language filters, reduce star thresholds, and try alternative keywords.
- If license information is missing from a repo, check for LICENSE file manually and flag as "license unverified" rather than assuming permissive.
- If the search scope is too broad (>100 results per query), add narrowing filters (stars, language, topic) and re-run.
