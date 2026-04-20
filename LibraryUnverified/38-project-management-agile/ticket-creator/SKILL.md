---
name: ticket-creator
description: >
  Generates well-structured tickets from requirements, bugs, review findings, or feature requests.
  Trigger — "create tickets for this", "write issues from these findings", "generate a ticket",
  "turn this into actionable tickets", "file a bug for this", "decompose into tickets".
  Skip — task is to audit existing tickets (use ticket-audit) or implement ticket contents.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ticket-creator
  maturity: draft
  risk: low
  tags: [ticket, creator]
---

# Purpose

Transform requirements, bug reports, review findings, or feature requests into well-structured,
actionable tickets. Each ticket follows a consistent template with clear acceptance criteria,
proper labels, priority classification, dependency links, and bounded scope so that an
implementer agent or human developer can start work without ambiguity.

# When to use

- A planning session has produced requirements that need to become trackable work items.
- A code review or security review has findings that need remediation tickets.
- A bug report needs to be formalised into a structured ticket.
- A large feature needs decomposition into individually assignable tickets.
- An audit identified gaps that need follow-up work items.

# Do NOT use when

- Tickets already exist and need quality checking — use `ticket-audit` instead.
- The task is to implement what a ticket describes — use an implementer skill.
- The user wants to reorganise or reprioritise existing tickets — use `backlog-verifier`.
- The input is too vague to produce an actionable ticket — ask for clarification first.

# Operating procedure

1. Parse the input source (requirements doc, review findings, bug description, or user message) and extract each distinct work item into a separate entry.
2. For each work item, determine the ticket type: `bug`, `feature`, `chore`, `refactor`, or `spike`.
3. Assign priority using this matrix: P0 = blocks release or causes data loss; P1 = significant user impact; P2 = moderate improvement; P3 = nice-to-have or cleanup.
4. Assign severity for bugs: Critical = system down; High = major feature broken; Medium = workaround exists; Low = cosmetic.
5. Write the ticket title as a concise imperative statement (e.g., "Add rate limiting to /api/login endpoint") — maximum 80 characters.
6. Write the Description section: one paragraph of context, the specific problem or need, and the expected outcome.
7. Write 3–5 Acceptance Criteria as checkboxes, each starting with a measurable verb: "Returns 429 status when rate exceeded", "Unit test covers the new path", "Documentation updated in API.md".
8. Assign labels from the project taxonomy: check for existing labels in `.github/labels.yml`, `labels.json`, or the ticket template, and reuse them.
9. Identify dependencies: search existing tickets for related work using `grep -rl '<keyword>' tickets/` and add `blocked-by: TICKET-ID` or `relates-to: TICKET-ID` links.
10. Write the ticket to the appropriate location: create a markdown file in the `tickets/` directory following the project naming convention, or output the ticket body for GitHub Issue creation.
11. Bound the scope explicitly: add a "Out of Scope" section listing adjacent work that is deliberately excluded from this ticket.

# Decision rules

- One ticket = one deployable unit of work. If a ticket requires changes to 3+ unrelated systems, split it.
- Never create a ticket without at least 3 acceptance criteria.
- If the input is ambiguous, create a `spike` ticket to investigate rather than guessing requirements.
- Reuse existing labels — do not invent new ones unless no existing label fits.
- If a dependency target ticket does not exist yet, create it as a stub and note it needs fleshing out.

# Output requirements

1. **Ticket File(s)** — one markdown file per ticket with these sections:
   - Title, Type, Priority, Severity (if bug), Labels
   - Description
   - Acceptance Criteria (checkbox list)
   - Dependencies (links)
   - Out of Scope
2. **Creation Summary Table** — `| Ticket ID | Title | Type | Priority | Dependencies |`
3. **Dependency Map** — text list showing which tickets block others.
4. **Label Usage Report** — labels applied and any new labels suggested.

# References

- Project ticket templates in `.github/ISSUE_TEMPLATE/` or `docs/templates/`
- Existing ticket index or manifest file
- Label taxonomy from `.github/labels.yml` or equivalent
- Input source document (requirements, review report, bug report)

# Related skills

- `ticket-audit` — validates tickets created by this skill
- `planner` — produces the requirements that feed ticket creation
- `security-review` — generates findings that become remediation tickets
- `team-leader` — assigns created tickets to specialist agents

# Failure handling

- If the input source is too vague to extract concrete work items, create a single `spike` ticket to clarify requirements.
- If the ticket directory or template does not exist, create the directory and note that a template should be established.
- If label taxonomy is missing, use a minimal default set: `bug`, `feature`, `chore`, `high-priority`, `low-priority`.
- If dependency targets cannot be verified, mark dependencies as `unverified` and flag for manual review.
