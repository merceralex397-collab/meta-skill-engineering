---
name: subagent-research-patterns
display_name: Subagent Research Patterns
category: Agentic Orchestration and Autonomy
priority: P1
status: candidate
sources:
  - conversation
  - invented
---

# Subagent Research Patterns

## Purpose
Structures how narrow research lanes gather evidence, summarize it, and hand it back without mutating repo state.

## Why it belongs
Useful because you frequently want parallel research.

## Suggested trigger situations
- when the user or repo work clearly points at subagent research patterns
- when a task in the "Agentic Orchestration and Autonomy" family needs repeatable procedure rather than ad hoc prompting
- when a plan, ticket, or repo state would benefit from explicit guardrails around subagent research patterns

## Notes for implementation
- Keep the scope explicit. The name should not hide unrelated responsibilities.
- Prefer references, scripts, examples, or evals when subagent research patterns would otherwise become a vague checklist.
- Decide whether this belongs in the always-generated core, a stack/profile pack, or the optional registry.

## Source basis
- **conversation** — mentioned or implied in this conversation/project context
- **invented** — newly invented extension proposed for this project

## Related skills
- `approval-gates`
- `session-resume-rehydration`
- `workflow-state-memory`
- `autonomous-run-control`

## Packaging note
This file is a **proposal stub**, not a finished `SKILL.md`. It is meant to help plan the library, category layout, and future implementation order.
