---
name: autonomous-backlog-maintenance
description: >
  Trigger — "groom the backlog", "close stale tickets", "split this epic", "re-prioritize tickets",
  "backlog health check", "find duplicate issues", "triage open tickets".
  Skip — when the user is creating new tickets (not maintaining existing ones), when the backlog
  has fewer than 5 items, or when manual human triage is explicitly requested.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: autonomous-backlog-maintenance
  maturity: draft
  risk: low
  tags: [backlog, grooming, triage, tickets, maintenance]
---

# Purpose

Autonomously maintain a project backlog by closing stale tickets, splitting oversized work items,
detecting duplicates, and recalculating priority based on evidence. Keeps the backlog actionable
without requiring constant human grooming sessions.

# When to use

- The backlog has accumulated tickets with no update in 30+ days.
- An epic or ticket scope exceeds what a single agent can complete in one run.
- Priority rankings are outdated because project goals or dependencies have shifted.
- Duplicate or near-duplicate tickets exist across the backlog.
- A periodic backlog health audit is needed.

# Do NOT use when

- The user is actively creating new tickets — let them finish first.
- The backlog has fewer than 5 items (manual review is faster).
- The user explicitly requests human-only triage with no automation.
- Tickets belong to a different team's backlog that this agent does not own.

# Operating procedure

1. Run `grep -rl "status" tickets/ docs/tickets/` (or equivalent) to locate all ticket files in the repo.
2. Parse each ticket and extract: `id`, `title`, `status`, `last_updated`, `assignee`, `labels`, `description length (words)`.
3. Load results into a table: `| ID | Title | Status | Days Since Update | Word Count | Labels |`.
4. **Staleness check**: Flag every open ticket with no update in >30 days. List them under a "Stale Candidates" heading.
5. **Duplicate detection**: Compare ticket titles and descriptions pairwise using keyword overlap. Flag pairs with >60% keyword overlap as potential duplicates.
6. **Oversized ticket detection**: Flag any ticket whose description exceeds 500 words or whose acceptance criteria list exceeds 8 items — these need splitting.
7. For each oversized ticket, propose a split: create 2–4 sub-tickets with MECE scopes, each under 200 words, each with ≤3 acceptance criteria.
8. **Priority recalculation**: Score each open ticket as `priority = (blocked_count × 3) + (dependency_count × 2) + (days_since_update > 14 ? 1 : 0)`. Rank by descending score.
9. Produce a **Backlog Health Report** with sections: Stale (count + list), Duplicates (count + pairs), Oversized (count + split proposals), Priority Ranking (top 10).
10. For stale tickets with no linked PR and no comments in 60+ days, draft a close-comment explaining the reason and mark status as `closed-stale`.
11. Write all proposed changes as a diff or patch — do not auto-apply without confirmation unless the agent is in full-autonomous mode.

# Decision rules

- Never close a ticket that has a linked open PR — mark it `blocked` instead and note the PR.
- Never close a ticket labeled `pinned`, `evergreen`, or `do-not-close` regardless of staleness.
- Splitting must preserve the original ticket as a parent/epic linking to the new sub-tickets.
- Priority recalculation must not override manually-set `P0` or `critical` labels — keep those at top.
- If duplicate detection confidence is below 60% keyword overlap, flag for human review instead of auto-merging.

# Output requirements

1. **Backlog Health Report** — summary table with stale count, duplicate count, oversized count, top-10 priority list.
2. **Stale Ticket Actions** — list of tickets to close with drafted close-comments.
3. **Split Proposals** — for each oversized ticket, the proposed sub-tickets with titles and acceptance criteria.
4. **Priority Ranking** — ordered list of all open tickets by computed score.
5. **Duplicate Pairs** — table of suspected duplicates with overlap percentage.

# References

- `references/checkpoint-rules.md` — checkpoint after each batch of ticket modifications.
- `references/failure-escalation.md` — escalate when ticket data is malformed or inaccessible.
- Repo ticket directory conventions (typically `tickets/`, `docs/tickets/`, or `.github/ISSUES/`).

# Related skills

- `collaboration-checkpoints` — checkpoint after bulk backlog changes before applying.
- `goal-decomposition` — splitting oversized tickets uses decomposition patterns.
- `human-interrupt-handling` — escalate to human when auto-close confidence is low.
- `autonomous-run-control` — backlog sweeps are long-running operations needing run budgets.

# Failure handling

- **Ticket parse failure**: If a ticket file is malformed, skip it, log the path and error, and continue with remaining tickets.
- **No tickets found**: If the ticket directory is empty or missing, report "No backlog found at expected paths" and list paths searched.
- **Ambiguous duplicates**: If overlap is 40–60%, list the pair under "Needs Human Review" instead of auto-acting.
- **Split disagreement**: If a ticket cannot be cleanly split into MECE sub-tasks, keep it intact and flag it as "Needs manual decomposition".
