---
name: overlay-generator
display_name: Overlay Generator
category: Package Scaffolding Skills
priority: P1
status: candidate
sources:
  - invented
  - architecture-spec
---

# Overlay Generator

## Purpose
Produces host-specific overlays for OpenAI, Gemini, OpenCode, Copilot, or other clients from a shared skill source.

## Why it belongs
The architecture docs explicitly favor one source skill with multiple target packages.

## Suggested trigger situations
- when the user or repo work clearly points at overlay generator
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around overlay generator

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when overlay generator would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **architecture-spec** — supported by the uploaded ideal skill architecture specs

## Related skills
- `skill-eval-runner`
- `skill-description-optimizer`
- `skill-packager`
- `community-skill-harvester`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
