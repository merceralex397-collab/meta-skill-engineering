---
name: adr-rfc-writing
description: "Write Architecture Decision Records and RFCs using structured formats like MADR -- capture context, list options with pros/cons, record the decision and rationale, and track status through the RFC lifecycle. Use when documenting architectural decisions, proposing design changes via RFCs, or standardizing decision record templates. Do not use for user-facing documentation, runbooks, or release notes (prefer those specific skills)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: adr-rfc-writing
  maturity: draft
  risk: low
  tags: [adr, rfc, madr, architecture-decisions, design-proposals]
---

# Purpose

Write Architecture Decision Records (ADRs) and Requests for Comments (RFCs) using structured formats like MADR -- capture the context and problem statement, enumerate options with trade-off analysis, record the chosen option with rationale, and manage the document through its lifecycle (draft, proposed, accepted, deprecated, superseded).

# When to use this skill

- Recording an architectural decision after a design discussion or spike
- Proposing a significant design change that needs team review via an RFC
- Creating an ADR template for a project that lacks a decision record standard
- Converting an informal Slack/meeting decision into a durable ADR
- Reviewing or updating the status of existing ADRs (accepted to deprecated/superseded)
- Standardizing ADR/RFC numbering, directory layout, and metadata across a repository
- Analyzing past decisions to understand the rationale behind current architecture

# Do not use this skill when

- The task is user-facing documentation (API docs, README, guides) -- use `document-writing`
- The task is writing operational runbooks or playbooks -- use `runbook-writing`
- The task is drafting release notes or changelogs -- use `release-notes`
- The task is research synthesis without a decision to record -- use `research-synthesis`
- The task is implementation work -- write the ADR before or after, not instead of code

# Operating procedure

1. **Determine the decision scope.** Identify the specific architectural question being decided. Frame it as a single, answerable question: "Which message broker should we adopt?" not "How should we redesign the backend?" If the scope is too broad, split into multiple ADRs.
2. **Choose the format.** Use MADR (Markdown Any Decision Records) as the default format. The standard MADR template sections are: Title, Status, Context, Decision Drivers, Considered Options, Decision Outcome, Pros and Cons of Options, and Links. If the project already has an ADR template, use the existing one.
3. **Assign the ADR number and filename.** Follow the project convention (e.g., `docs/decisions/0042-adopt-rabbitmq.md`). If no convention exists, create one: `docs/decisions/NNNN-short-slug.md` with zero-padded sequential numbering. Add an `index.md` or `README.md` listing all ADRs with number, title, status, and date.
4. **Write the Context section.** Describe the problem, the forces at play, and why a decision is needed now. Include relevant constraints: team size, existing tech stack, performance requirements, compliance needs. Be specific -- "Our current polling approach adds 2-5s latency to event processing" not "We need better performance."
5. **List the Decision Drivers.** Enumerate the criteria that matter most for this decision, ordered by priority. Examples: latency requirements, team expertise, operational complexity, cost, vendor lock-in, ecosystem maturity. These drivers structure the option evaluation.
6. **Enumerate Considered Options.** List 2-5 realistic options. For each option, write a brief description (1-2 sentences) explaining what it entails. Include "Do nothing / status quo" as an option when applicable -- it forces explicit rejection of the current state.
7. **Analyze each option's pros and cons.** For each option, list concrete pros and cons against the decision drivers. Use evidence: benchmark numbers, team survey results, production metrics, vendor documentation. Avoid vague assessments like "better performance" -- quantify or cite.
8. **Record the Decision Outcome.** State the chosen option clearly: "Chosen option: RabbitMQ, because it meets our latency requirements (<100ms P99), the team has prior experience, and it supports our message patterns (pub/sub + work queues)." Explain why the chosen option was preferred over the runner-up.
9. **Set the status.** Mark the ADR status: `proposed` (awaiting review), `accepted` (approved and active), `deprecated` (no longer relevant), or `superseded by ADR-NNNN` (replaced by a newer decision). For RFCs, add intermediate statuses: `draft`, `in-review`, `final-comment-period`, `accepted`, `rejected`.
10. **Add Links and References.** Link to related ADRs, RFCs, GitHub issues, design docs, benchmark results, or external documentation that informed the decision. Link forward to the implementation PR or ticket.
11. **Submit for review.** Open a pull request with the ADR/RFC file. Request review from stakeholders listed in the Context section. Set a review deadline (typically 1-2 weeks for RFCs). Address review comments by updating the document, not in PR comment threads.
12. **Update the ADR index.** Add the new ADR to the project's decision log (index file or README table) with: number, title, status, date, and a one-line summary.

# Decision rules

- If a decision affects more than one team or service boundary, write an RFC with a review period instead of a unilateral ADR.
- If two options are very close in trade-offs, document both thoroughly and call out the deciding factor explicitly -- future readers will want to know why the runner-up was rejected.
- If an ADR is superseded, update the old ADR's status to "Superseded by ADR-NNNN" and link to the new one. Never delete old ADRs.
- If the team cannot reach consensus during the RFC review period, escalate to the designated architecture owner for a final call and record the escalation in the ADR.
- If the decision is trivial (< 1 hour to reverse, no cross-team impact), do not write an ADR -- use a code comment or commit message instead.
- Prefer short, focused ADRs (one decision per record) over comprehensive design documents that bundle multiple decisions.

# Output requirements

1. **ADR/RFC file** -- complete Markdown file following MADR or the project's template, with all sections filled
2. **Updated index** -- the decision log or index file updated with the new entry
3. **Status metadata** -- clear status field (proposed/accepted/deprecated/superseded) with date
4. **Links** -- references to related ADRs, implementation tickets, and source evidence
5. **Review instructions** -- for RFCs, a note on who should review and the review deadline

# References

- MADR template and specification: https://adr.github.io/madr/
- ADR GitHub organization: https://adr.github.io/
- Michael Nygard's original ADR article: https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions
- Lightweight RFC process: https://philcalcado.com/2018/11/19/a_]]lightweight_rfc_process.html
- Joel Parker Henderson's ADR examples: https://github.com/joelparkerhenderson/architecture-decision-record

# Related skills

- `document-writing` -- for user-facing docs, API references, and README files
- `research-synthesis` -- for research spikes that inform ADR options analysis
- `runbook-writing` -- for operational procedures that implement ADR decisions
- `release-notes` -- for communicating shipped changes to users

# Anti-patterns

- Writing ADRs after the decision is fully implemented -- the rationale is already lost or rationalized post-hoc
- Bundling multiple unrelated decisions into a single ADR -- each decision deserves its own record
- Listing options without concrete pros/cons -- "Option A is good" is not analysis
- Using ADRs as design documents with implementation details -- ADRs record the what and why, not the how
- Leaving ADR status as "proposed" indefinitely without resolving it
- Deleting or editing accepted ADRs to reflect later changes instead of writing a superseding ADR

# Failure handling

- If the context is unclear, interview the decision-makers and extract the forces before writing options. Do not guess at constraints.
- If no options beyond the status quo are available, the ADR becomes a "decision to not decide" -- record why the alternatives were rejected or not viable.
- If the RFC review period passes with no feedback, ping reviewers once. If still no response after 3 business days, escalate to the architecture owner and note the silence in the ADR.
- If a previously accepted ADR is discovered to be based on incorrect assumptions, write a new ADR that supersedes it with corrected analysis -- do not silently edit the original.
- If the project has no ADR directory or convention, create `docs/decisions/` with an `index.md` and a template file (`docs/decisions/template.md`) before writing the first ADR.
