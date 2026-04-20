---
name: docx-generation
display_name: DOCX Generation
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - conversation
  - external-example
---

# DOCX Generation

## Purpose
Creates formatted Word-style documents when richer office output is needed.

## Why it belongs
A practical artifact skill.

## Suggested trigger situations
- when the user or repo work clearly points at docx generation
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around docx generation

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when docx generation would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports

## Related skills
- `internal-comms`
- `pptx-generation`
- `pdf-generation`
- `xlsx-generation`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
