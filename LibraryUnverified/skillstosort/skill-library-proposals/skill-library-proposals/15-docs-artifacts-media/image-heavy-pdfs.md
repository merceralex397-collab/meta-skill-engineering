---
name: image-heavy-pdfs
display_name: Image-Heavy PDFs
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - research-docs
  - external-example
---

# Image-Heavy PDFs

## Purpose
Specializes PDF handling when the content is primarily diagrams, screenshots, or images rather than text.

## Why it belongs
Useful for certain technical docs.

## Suggested trigger situations
- when the user or repo work clearly points at image-heavy pdfs
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around image-heavy pdfs

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when image-heavy pdfs would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports

## Related skills
- `image-editor`
- `document-to-structured-data`
- `csv-ready`
- `slack-gif-creator`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
