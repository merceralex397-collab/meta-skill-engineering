---
name: meeting-notes-decision-log
description: "Transform raw meeting notes into structured decision logs with captured decisions, action items, owners, deadlines, and follow-up tracking. Use when formatting meeting transcripts, extracting decisions from discussion notes, or building a running decision log from recurring meetings. Do not use for formal architecture decisions (prefer adr-rfc-writing) or project planning documents."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: meeting-notes-decision-log
  maturity: draft
  risk: low
  tags: [meeting-notes, decision-log, action-items, minutes]
---

# Purpose

Transform raw meeting notes, transcripts, and discussion threads into structured decision logs with clearly captured decisions, assigned action items, identified owners, explicit deadlines, and follow-up tracking. This skill produces artifacts that make it easy to answer "what was decided?" and "who is doing what by when?" without re-reading the entire discussion.

# When to use this skill

- Raw meeting notes or transcripts need to be formatted into structured, searchable records
- Decisions are buried in discussion notes and need to be extracted into a decision log
- Action items must be identified, assigned owners, and given deadlines from unstructured notes
- A recurring meeting series needs a running decision log that tracks decisions over time
- Stakeholders who missed a meeting need a concise summary with clear outcomes
- Follow-up items from a previous meeting need to be tracked and status-updated

# Do not use this skill when

- The decision needs a formal architecture decision record with alternatives analysis — prefer `adr-rfc-writing`
- The output is a project plan or roadmap document — prefer project planning tools
- The task is writing release notes from completed work — prefer `release-notes`
- The document is a technical specification — prefer `spec-authoring`
- The content is a standalone written document (guide, process doc) — prefer `document-writing`

# Operating procedure

1. **Collect raw input** — gather the meeting notes, transcript, Slack thread, or recording summary. Identify the meeting date, attendees, and stated agenda if available.
2. **Identify the meeting metadata** — extract and record: meeting title/series name, date and time, attendees (present and absent), facilitator, and note-taker. If any metadata is missing, mark it as `[UNKNOWN]`.
3. **Extract agenda topics** — list the topics that were actually discussed (not just the planned agenda). Number each topic for reference.
4. **Extract decisions** — scan the notes for decision language: "we agreed", "decision:", "we will", "the approach is", "going forward". For each decision, record: the decision statement (clear, unambiguous), the topic it relates to, who made or approved it, and any dissent or conditions.
5. **Extract action items** — identify commitments: "I'll do", "action:", "TODO", "@name will", "by Friday". For each action item, record: the task description, the owner (specific person, not "the team"), the deadline (specific date, not "soon"), and the status (new, in-progress, done, blocked).
6. **Extract open questions** — identify unresolved items: "we need to figure out", "TBD", "parking lot", "let's revisit". Record each with the topic context and who is responsible for resolving it.
7. **Extract follow-ups from prior meetings** — if this is a recurring meeting, check the previous meeting's action items and open questions. Update their status: done, still in-progress, blocked, or dropped.
8. **Structure the output document** — organize into standard sections in this order: Metadata, Summary (3-5 sentences), Decisions (numbered list), Action Items (table: task, owner, deadline, status), Open Questions, Discussion Notes (condensed by topic), Follow-Up Status (if recurring).
9. **Write the summary** — compose a 3-5 sentence executive summary covering: what was discussed, the key decisions made, and the most important action items. Write for someone who will only read this section.
10. **Format the decision log entry** — if maintaining a running decision log, append each new decision with: decision ID (sequential), date, decision text, context/rationale (1-2 sentences), and participants.
11. **Format the action item table** — create a Markdown table with columns: `#`, `Task`, `Owner`, `Deadline`, `Status`. Sort by deadline ascending. Bold any items that are overdue.
12. **Review for completeness** — verify every decision has an owner or approver, every action item has both an owner and a deadline, and no discussion topic was skipped. Flag any items missing required fields.
13. **Distribute or commit** — save the notes in the agreed-upon location (repo, wiki, shared drive). If using a decision log file, append rather than overwrite.

# Decision rules

- If a discussion point has no clear decision, record it as an open question — do not fabricate a decision.
- If an action item has no explicit owner, assign it to the meeting facilitator by default and flag it for reassignment.
- If a deadline is vague ("next week", "soon"), convert to a specific date based on the meeting date and flag it with `[estimated]`.
- If attendees disagree on what was decided, record both interpretations and mark the decision as `[disputed — needs confirmation]`.
- For recurring meetings, always include a follow-up section even if all prior items are complete — this confirms nothing was dropped.
- Prefer short decision statements (one sentence) with context in a separate field over long narrative decisions.

# Output requirements

1. **Meeting notes document** — Markdown file with metadata header, summary, decisions, action items, open questions, and discussion notes
2. **Decision log entry** — each decision formatted with: ID, date, decision text, rationale, participants
3. **Action item table** — structured table with task, owner, deadline, and status columns
4. **Follow-up tracker** — status update on prior meeting's action items (for recurring meetings)
5. **Distribution note** — confirmation of where the notes were saved and who was notified

# References

- Amazon's "Working Backwards" meeting memo format — structured written memos over slide decks
- Conventional meeting minutes format — Robert's Rules of Order, adapted for technical teams
- DACI decision framework — Driver, Approver, Contributors, Informed

# Related skills

- `adr-rfc-writing` — for formal architecture decisions that need alternatives analysis and long-term record
- `document-writing` — for standalone documents that go beyond meeting notes
- `release-notes` — for summarizing completed work rather than ongoing decisions

# Anti-patterns

- **Recording discussion without extracting decisions** — produces verbose notes nobody reads. Always pull decisions to the top.
- **Action items without owners** — "the team will investigate" results in nobody investigating. Every action item needs one specific person.
- **Action items without deadlines** — undated tasks never get done. Assign a date, even if estimated.
- **Overwriting previous meeting notes** — for recurring meetings, append to the log rather than replacing. History matters for "when did we decide X?"
- **Summarizing tone instead of content** — "productive discussion about the API" tells the reader nothing. Summarize what was decided and what changed.

# Failure handling

- If the raw notes are too fragmentary to extract decisions, produce a "discussion summary" and explicitly mark that no decisions were identified. Request confirmation from the facilitator.
- If multiple people are attributed the same action item in conflicting notes, list both attributions and flag for clarification.
- If the meeting had no agenda and notes are disorganized, impose a topic structure based on natural topic shifts and note the reorganization.
- If prior meeting notes cannot be found for follow-up tracking, start fresh and note that historical continuity was broken.
- If attendee names are ambiguous (first names only, abbreviations), list them as-is and add a `[verify attendees]` flag.
