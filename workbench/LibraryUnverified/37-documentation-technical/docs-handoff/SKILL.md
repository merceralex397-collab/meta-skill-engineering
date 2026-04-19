---
name: docs-handoff
description: >
  Trigger — "update the docs", "sync README", "write handoff notes", "update changelog",
  "create START-HERE", "docs are stale", "prepare handoff".
  Skip — if the task is writing code, running tests, or reviewing PRs. Prefer `qa-validation`
  for acceptance checks and `planner` for implementation planning.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: docs-handoff
  maturity: draft
  risk: low
  tags: [docs, handoff]
---

# Purpose

Updates README, AGENTS.md, changelogs, and handoff documents after a milestone or ticket
completes. Ensures documentation surfaces stay synchronized with the actual state of the
codebase so the next person (or agent) can resume without re-discovery.

# When to use

- A milestone, sprint, or ticket just completed and docs may be stale.
- Someone asks to update README, AGENTS.md, CHANGELOG, or a START-HERE document.
- A handoff brief is needed for the next session, team, or agent.
- A PR merged significant changes and no doc update accompanied it.

# Do NOT use when

- The task is writing or modifying application code — use an implementer skill.
- The task is verifying acceptance criteria — use `qa-validation`.
- The task is reviewing code quality — use `code-review`.
- The user wants to plan work, not document completed work — use `planner`.

# Operating procedure

1. Run `find . -maxdepth 3 -name 'README*' -o -name 'AGENTS*' -o -name 'CHANGELOG*' -o -name 'START-HERE*'` to build a doc surface inventory.
2. Run `git log --oneline --since="2 weeks ago" -- .` to list recent commits and identify what changed.
3. For each doc file found in step 1, run `git log -1 --format='%ci' -- <file>` to get its last-modified date.
4. Compare each doc's last-modified date against the most recent code commit. Flag any doc older than the latest relevant code change as **stale**.
5. For each stale doc, run `git diff HEAD~10 -- <file>` and `git diff HEAD~10 -- <related-source-dirs>` to identify what content drifted.
6. Draft updated content for each stale doc. For README: update feature list, quickstart, and API surface. For CHANGELOG: add entries grouped by Added/Changed/Fixed/Removed. For AGENTS.md: update agent descriptions, tool lists, and capability summaries.
7. If a START-HERE document is requested, create it with: project purpose (1 paragraph), quickstart (numbered steps), architecture overview (file tree + brief annotations), active tickets or next steps, and key contacts or ownership.
8. Build a handoff checklist table: | Doc | Status | Action Taken | Reviewer Needed |.
9. Validate all internal links in updated docs by running `grep -rn '\[.*\](.*\.md)' <doc-file>` and checking each target exists.
10. Present the final diff of all doc changes and the handoff checklist for review.

# Decision rules

- If a doc file exists but is >30 days stale relative to its source directory, mark it **critical** in the checklist.
- If no CHANGELOG exists and the project has >10 commits, recommend creating one.
- If AGENTS.md references agents or tools that no longer exist in the repo, remove those entries.
- Never invent features or capabilities not evidenced by the codebase — every doc claim must trace to a file or commit.
- Prefer updating existing docs over creating new ones unless a gap is clearly identified.

# Output requirements

1. **Doc Surface Inventory** — table of all doc files with last-modified dates and staleness flags.
2. **Change Summary** — bullet list of what changed in the codebase since each doc was last updated.
3. **Updated Doc Content** — full text of each updated document (or diff if minimal changes).
4. **Handoff Checklist** — table with columns: Doc, Status (current/stale/missing), Action Taken, Reviewer Needed (yes/no).
5. **START-HERE Document** — if requested, the complete document ready to commit.

# References

- `references/handoff-contract.md` — handoff structure and required sections
- `references/success-criteria.md` — what "done" looks like for doc updates
- Keep Changelog format: https://keepachangelog.com/en/1.0.0/

# Related skills

- `planner` — for planning implementation work (upstream of docs)
- `qa-validation` — for verifying acceptance criteria (parallel concern)
- `github-prior-art-research` — for finding doc templates and patterns

# Failure handling

- If `git log` is unavailable (not a git repo), fall back to file modification timestamps via `stat`.
- If a doc references files that no longer exist, flag the broken reference and suggest removal rather than guessing new targets.
- If the scope of changes is too large to summarize in one pass, split by directory and produce per-module doc updates.
- If the user provides no milestone context, ask for the ticket ID or date range before proceeding.
