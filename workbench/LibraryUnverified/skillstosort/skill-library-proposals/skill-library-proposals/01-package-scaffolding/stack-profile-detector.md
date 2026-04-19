---
name: stack-profile-detector
display_name: Stack Profile Detector
category: Package Scaffolding Skills
priority: P1
status: candidate
sources:
  - invented
  - research-gap
---

# Stack Profile Detector

## Purpose
Infers language, framework, cloud target, and runtime profile from repo evidence and spec material.

## Why it belongs
This lets generation choose the right packs instead of a generic placeholder.

## Suggested trigger situations
- when the user or repo work clearly points at stack profile detector
- when a task in the "Package Scaffolding Skills" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around stack profile detector

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when stack profile detector would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **research-gap** — called out as missing or underdeveloped in the uploaded reports

## Related skills
- `skill-packager`
- `community-skill-harvester`
- `migration-pack-builder`
- `skill-deprecation-manager`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
