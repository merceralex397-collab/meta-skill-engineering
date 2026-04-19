---
name: skill-anti-patterns
display_name: Skill Anti-Patterns
category: Meta Skill Engineering
priority: P2
status: candidate
sources:
  - invented
  - conversation
  - research-docs
---

# Skill Anti-Patterns

## Purpose
Collects common ways skills fail: vague scope, weak triggers, broad but shallow bodies, or missing evals.

## Why it belongs
Handy as a QA lens when editing skills.

## Suggested trigger situations
- when creating, adapting, refining, installing, testing, or packaging a skill
- when a task in the "Meta Skill Engineering" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around skill anti-patterns

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when skill anti-patterns would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **invented** — newly invented extension proposed for this project
- **conversation** — mentioned or implied in this conversation/project context
- **research-docs** — recommended, criticized, or surfaced in the uploaded research markdowns

## Related skills
- `skill-safety-review`
- `skill-reference-extraction`
- `skill-lifecycle-management`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
