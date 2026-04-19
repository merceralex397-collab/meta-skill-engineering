---
name: ticket-audit
description: >
  Audits tickets for completeness, clarity, actionability, and internal consistency.
  Trigger — "audit these tickets", "check ticket quality", "are these tickets actionable",
  "review ticket completeness", "validate acceptance criteria", "score ticket clarity".
  Skip — task is to create new tickets (use ticket-creator) or to implement ticket contents.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ticket-audit
  maturity: draft
  risk: low
  tags: [ticket, audit]
---

# Purpose

Systematically evaluate tickets (issues, tasks, stories) for completeness, clarity, and
actionability. Check required fields, validate acceptance criteria, score scope definition,
identify missing dependencies, sanity-check effort estimates, and flag ambiguous language
that would block an implementer from starting work.

# When to use

- A batch of tickets has been created and needs quality assurance before sprint planning.
- An agent-generated ticket set needs validation before assignment.
- A backlog grooming session requires an objective quality score for each ticket.
- A milestone's tickets need dependency-chain verification.
- Tickets are being imported from external sources and need standardisation checks.

# Do NOT use when

- The task is to create new tickets — use `ticket-creator` instead.
- The task is to implement what a ticket describes — use an implementer skill.
- The audit scope is code quality, not ticket quality — use `code-review`.
- Only a single field needs updating — just edit the ticket directly.

# Operating procedure

1. List all ticket files by running `find . -path '*/tickets/*.md' -o -path '*/issues/*.md' | sort` or reading the ticket index/manifest file.
2. For each ticket, extract and check these required fields: Title, Description, Acceptance Criteria, Priority, Labels, and Assignee (or "unassigned").
3. Score each ticket on a 1–5 scale for **Clarity** (can an implementer start without asking questions?), **Scope** (is the boundary well-defined?), and **Testability** (can completion be objectively verified?).
4. Validate acceptance criteria: each criterion must be a concrete, verifiable statement — flag any that use words like "should work well", "appropriate", or "as needed" as vague.
5. Check for dependency completeness: if a ticket references another ticket, verify the referenced ticket exists and is not in a contradictory state (e.g., blocked ticket depending on another blocked ticket).
6. Verify effort estimates: if present, check that they are consistent with scope (flag tickets estimated at 1 point but spanning 5+ files, or 8 points for a single config change).
7. Run `grep -l 'TODO\|FIXME\|TBD\|PLACEHOLDER' <ticket-files>` to find tickets with unresolved placeholders.
8. Check for duplicates: compare ticket titles and descriptions pairwise and flag any with >80% textual overlap.
9. Compile a summary table: `| Ticket ID | Title | Clarity | Scope | Testability | Issues Found | Status |`.
10. Produce a prioritised list of tickets that need revision before they can be assigned.

# Decision rules

- A ticket missing acceptance criteria always scores Testability ≤ 2.
- A ticket with no clear scope boundary (affects "multiple areas" without listing them) scores Scope ≤ 2.
- Duplicate tickets: keep the more detailed one and recommend closing the other.
- If a ticket has 0 issues, mark it READY. If 1–2 minor issues, mark NEEDS_POLISH. If 3+ issues, mark REWRITE.
- Never rewrite tickets yourself — flag them with specific fix instructions.

# Output requirements

1. **Audit Summary Table** — one row per ticket with scores and status.
2. **Issue Detail List** — for each flagged ticket: ticket ID, issue type, quoted problematic text, and specific fix instruction.
3. **Dependency Graph Check** — list of broken or circular dependency chains.
4. **Duplicate Report** — pairs of tickets with high overlap and recommended action.
5. **Overall Backlog Health** — percentage of tickets that are READY, NEEDS_POLISH, and REWRITE.
6. **Top 5 Fixes** — the five highest-impact improvements to make first.

# References

- Project ticket directory or manifest file
- Ticket templates if defined in `.github/ISSUE_TEMPLATE/` or `docs/templates/`
- Definition of Done document if present in the repo

# Related skills

- `ticket-creator` — creates tickets that this skill audits
- `team-leader` — consumes audit results to decide assignment readiness
- `planner` — generates ticket sets that benefit from auditing
- `backlog-verifier` — validates backlog state at a higher level

# Failure handling

- If no ticket files are found, search for GitHub Issues via the API or report "no tickets found in repo."
- If a ticket uses a non-standard format, attempt best-effort field extraction and note format deviations.
- If the ticket set exceeds 50 tickets, audit in batches of 15 and produce incremental reports.
- If scoring is ambiguous for a ticket, assign the lower score and add a note explaining the uncertainty.
