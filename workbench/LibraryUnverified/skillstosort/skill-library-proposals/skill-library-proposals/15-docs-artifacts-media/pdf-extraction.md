---
name: pdf-extraction
display_name: PDF Extraction
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - external-example
  - research-docs
---

# PDF Extraction

## Purpose
Extracts structured content, provenance, and layout-aware details from PDFs.

## Why it belongs
A clear public example and broadly useful.

## Suggested trigger situations
- when the user or repo work clearly points at pdf extraction
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around pdf extraction

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when pdf extraction would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns

## Related skills
- `doc-coauthoring`
- `docx`
- `pdf-editor`
- `table-extraction`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
