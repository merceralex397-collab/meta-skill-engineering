---
name: docx
display_name: DOCX
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - external-example
  - research-docs
---

# DOCX

## Purpose
An external-style skill focused specifically on DOCX-oriented tasks and document handling.

## Why it belongs
Included because it appears directly in the public example set.

## Suggested trigger situations
- when the user or repo work clearly points at docx
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around docx

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when docx would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns

## Related skills
- `xlsx-generation`
- `doc-coauthoring`
- `pdf-extraction`
- `pdf-editor`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
