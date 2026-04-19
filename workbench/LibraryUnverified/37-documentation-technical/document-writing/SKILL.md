---
name: document-writing
description: "Compose technical documents with clear structure, audience-appropriate tone, logical section flow, and scannable formatting. Use when writing technical guides, internal documentation, process documents, or any prose artifact that needs professional structure and clarity. Do not use for code-heavy specs (prefer spec-authoring), decision records (prefer adr-rfc-writing), or operational runbooks (prefer runbook-writing)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: document-writing
  maturity: draft
  risk: low
  tags: [document-writing, technical-writing, structure, audience]
---

# Purpose

Compose technical documents with clear structure, audience-appropriate tone, logical section flow, and scannable formatting. This skill produces polished prose artifacts — guides, process documents, internal documentation, onboarding materials, and technical overviews — that communicate complex information with professional clarity.

# When to use this skill

- Writing a technical guide, how-to document, or internal knowledge base article
- Creating process documentation or standard operating procedures
- Drafting onboarding materials, team playbooks, or contributor guides
- Producing any prose artifact that requires heading hierarchy, audience targeting, and professional structure
- Restructuring or rewriting an existing document that lacks clear organization
- The deliverable is a Markdown, Google Doc, or Confluence page intended for human readers

# Do not use this skill when

- The document is a code-heavy technical specification with API schemas — prefer `spec-authoring`
- The output is a formal architecture decision record or RFC — prefer `adr-rfc-writing`
- The document is an operational runbook with step-by-step incident response — prefer `runbook-writing`
- The task is extracting structured data from a document rather than writing one — prefer `document-to-structured-data`
- The output is a formatted Word or PDF file requiring programmatic generation — prefer `docx-generation` or `pdf-generation`

# Operating procedure

1. **Identify the audience** — determine the primary reader (engineers, managers, external users, new hires). Note their technical level, what they already know, and what decisions they need to make after reading.
2. **Define the document purpose** — write a single sentence stating what the reader should be able to do or decide after reading. This becomes the litmus test for every section.
3. **Gather source material** — collect all relevant inputs: existing docs, code comments, Slack threads, meeting notes, ticket descriptions. List sources explicitly for traceability.
4. **Create the outline** — build a heading hierarchy (H1 title, H2 major sections, H3 subsections). Standard sections include: Overview, Prerequisites, Procedure/Content, Examples, Troubleshooting, References. Validate that the outline covers the stated purpose.
5. **Write the introduction** — open with a 2-3 sentence summary stating what the document covers, who it is for, and what prerequisite knowledge is assumed. Do not bury the purpose below background context.
6. **Draft each section** — write in short paragraphs (3-5 sentences max). Lead each paragraph with the key point. Use bullet lists for sets of 3+ related items. Use numbered lists only for sequential steps.
7. **Apply scannable formatting** — use bold for key terms on first use, code formatting for commands/paths/identifiers, callout blocks for warnings and prerequisites, and tables for comparison data.
8. **Calibrate tone** — match the audience: direct and terse for senior engineers, explanatory for new hires, formal for external-facing docs. Remove hedging language ("maybe", "it seems") and filler words.
9. **Add examples** — include at least one concrete example per major concept. Examples should be copy-pasteable where applicable (commands, config snippets, API calls).
10. **Cross-reference related documents** — link to prerequisite docs, deeper dives, and related procedures. Use relative links for repo-internal docs and full URLs for external resources.
11. **Review pass: completeness** — verify every section in the outline has been written. Check that the document fulfills its stated purpose sentence.
12. **Review pass: consistency** — ensure terminology is consistent throughout (no switching between "deploy" and "ship" for the same concept). Verify heading levels follow hierarchy without skipping.
13. **Review pass: formatting** — confirm Markdown renders correctly, all links resolve, code blocks have language identifiers, and no orphaned TODOs remain.

# Decision rules

- Prefer shorter documents with clear structure over comprehensive documents with poor navigation.
- When the audience is mixed (technical and non-technical), lead with a plain-language summary and put technical details in later sections.
- If source material conflicts, note the conflict explicitly rather than silently choosing one version.
- Use diagrams or tables when explaining relationships between 3+ concepts — prose alone loses readers.
- If a section exceeds 500 words, break it into subsections or extract a separate document.

# Output requirements

1. **Document file** — Markdown file with proper heading hierarchy (H1 title, H2+ sections)
2. **Frontmatter** — title, author/owner, date, status (draft/review/final), and audience
3. **Table of contents** — auto-generated or manual TOC for documents exceeding 5 sections
4. **Change log** — for living documents, a dated change log at the bottom or in version control
5. **Review checklist** — confirmation that completeness, consistency, and formatting passes were performed

# References

- Google Developer Documentation Style Guide — https://developers.google.com/style
- Microsoft Writing Style Guide — https://learn.microsoft.com/en-us/style-guide/
- Diátaxis documentation framework — https://diataxis.fr/

# Related skills

- `adr-rfc-writing` — for formal architecture decisions and RFC proposals
- `runbook-writing` — for operational runbooks and incident response procedures
- `spec-authoring` — for code-heavy technical specifications and API contracts

# Anti-patterns

- **Writing without defining the audience first** — produces documents with inconsistent depth and tone that serve nobody well.
- **Burying the purpose** — placing two paragraphs of background before stating what the document is about loses readers immediately.
- **Wall-of-text paragraphs** — paragraphs longer than 5 sentences in technical docs go unread. Break them up.
- **Inconsistent terminology** — switching between synonyms ("service", "app", "server" for the same thing) creates confusion.
- **Orphaned TODOs in published documents** — `TODO: fill in later` in a shipped doc signals incomplete work. Resolve or remove before publishing.

# Failure handling

- If the audience is unclear, draft for the most technical reader and add a plain-language summary at the top. Flag the audience ambiguity for the requester.
- If source material is insufficient to write a complete section, insert a clearly-marked `[NEEDS INPUT: specific question]` placeholder rather than inventing content.
- If the document scope grows beyond the original purpose, split into multiple documents and create a parent index page linking them.
- If formatting requirements are unknown (Markdown vs Confluence vs Google Docs), default to Markdown and note that conversion may be needed.
- If the document contradicts existing published documentation, flag the conflict explicitly and recommend which version should be canonical.
