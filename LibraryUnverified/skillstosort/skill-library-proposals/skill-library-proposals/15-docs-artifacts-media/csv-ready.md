---
name: csv-ready
display_name: CSV Ready
category: Docs, Artifacts, and Media Skills
priority: P2
status: candidate
sources:
  - research-docs
  - external-example
---

# CSV Ready

## Purpose
Shapes extracted or synthesized data into clean CSV-compatible form with stable headers and values.

## Why it belongs
Good for downstream tooling.

## Suggested trigger situations
- when the user or repo work clearly points at csv ready
- when a task in the "Docs, Artifacts, and Media Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around csv ready

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when csv ready would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns
- **external-example** — named or exemplified by an external/public skill surfaced in the uploaded reports

## Related skills
- `document-to-structured-data`
- `image-heavy-pdfs`
- `slack-gif-creator`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
