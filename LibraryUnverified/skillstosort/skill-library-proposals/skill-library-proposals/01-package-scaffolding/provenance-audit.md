---
name: provenance-audit
display_name: Provenance Audit
category: Package Scaffolding Skills
priority: P2
status: candidate
sources:
  - invented
  - architecture-spec
  - conversation
---

# Provenance Audit

## Purpose
Tracks where each generated skill came from: user request, repo evidence, imported example, or invented extension.

## Why it belongs
You want evidence and traceability, so provenance deserves its own utility.

## Suggested trigger situations
- when the user or repo work clearly points at provenance audit
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around provenance audit

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when provenance audit would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs
- **conversation** — mentioned or implied in this conversation/project context

## Related skills
- `migration-pack-builder`
- `skill-deprecation-manager`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
