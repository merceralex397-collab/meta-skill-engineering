---
name: pptx-generation
display_name: PowerPoint Generation
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - conversation
  - external-example
---

# PowerPoint Generation

## Purpose
Creates slide decks with structured sections and presentation-friendly summaries.

## Why it belongs
Useful when packaging research or proposals.

## Suggested trigger situations
- when the user or repo work clearly points at powerpoint generation
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around powerpoint generation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when powerpoint generation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports

## Related skills
- `meeting-notes-decision-log`
- `internal-comms`
- `docx-generation`
- `pdf-generation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
