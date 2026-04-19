---
name: document-to-structured-data
display_name: Document to Structured Data
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - research-docs
  - external-example
---

# Document to Structured Data

## Purpose
Turns narrative documents into machine-readable fields or structured records.

## Why it belongs
A strong pattern for artifact extraction.

## Suggested trigger situations
- when the user or repo work clearly points at document to structured data
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around document to structured data

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when document to structured data would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports

## Related skills
- `table-extraction`
- `image-editor`
- `image-heavy-pdfs`
- `csv-ready`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
