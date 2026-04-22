# Plugin-eval disposition for Meta-Skill-Engineering

This document records which `plugin-eval` techniques were adopted into the wider platform, which were adapted in a narrower form, and which remain specialized to plugin-eval.

## Decision summary

| Technique from plugin-eval | Disposition | Meta Skill Studio form | Why |
| --- | --- | --- | --- |
| Versioned result schemas | **Adopted** | `meta-skill-studio-run` with `schema_version: 1` | The repo needed stable headless artifacts instead of path-only stdout or ad hoc prose |
| Measurement plan artifact | **Adopted** | `evaluate-skill` embeds a `measurement_plan` section | The platform needs a documented measurement workflow, not only pass/fail output |
| Compare outputs | **Adopted** | `compare-runs` action emits structured before/after deltas | Improvement work needs an auditable comparison surface |
| Improvement brief generation | **Adopted** | `evaluate-skill` emits `improvement_brief`; `improvement-brief` can rehydrate one from an existing run | Evaluation should feed improvement work directly |
| Fixture-driven evaluation | **Adopted** | Existing JSONL eval suites plus benchmark JSONL generation remain first-class | This already matches the repo’s skill eval model and is worth keeping explicit |
| Observed vs estimated usage | **Adapted narrowly** | Run artifacts record estimated vs observed workflow steps and duration, not token-budget accounting | OpenCode does not provide a normalized cross-workflow token stream today, so workflow-step evidence is the stable comparison we can support now |
| Beginner-friendly start/router commands | **Adapted narrowly** | `list-actions` and the published action contract replace a fuzzy router | Agents need deterministic verbs more than chat-style routing in the authoritative CLI |
| Token budget grading and baseline bands | **Rejected repo-wide** | Keep specialized to plugin-eval | Useful for plugin/skill-budget analysis, but too opinionated for every repo workflow |
| Metric-pack extension system | **Rejected repo-wide** | Not adopted into Studio CLI | The repo does not yet need a general extension SDK for every workflow artifact |
| Plugin-manifest specific checks | **Rejected repo-wide** | Remain plugin-eval only | They do not generalize to Meta-Skill-Engineering’s root skill packages |

## Canonical evaluation artifacts after plan 13

### 1. Orchestrated evaluation run

`evaluate-skill` now produces one versioned Studio run artifact containing:

- raw validation, eval, and judge command payloads
- a top-level `summary`
- a `measurement_plan`
- an `improvement_brief`
- stable `artifacts` pointers such as `eval-results/`

### 2. Improvement brief artifact

`improvement-brief` produces a new run artifact whose payload focuses on:

- source run reference
- quality score, when available
- prioritized findings
- concrete next actions

### 3. Comparison artifact

`compare-runs` produces a run artifact containing:

- before/after run references
- quality and duration deltas when available
- action/status context for both runs

## What was intentionally not broadened

1. **Token economics as a repo-wide gate:** useful in plugin-eval, but the Studio CLI currently cannot measure token usage consistently across OpenCode-driven workflows and shell-script-driven workflows.
2. **Metric-pack architecture:** good for a standalone evaluator product, unnecessary complexity for the current repository hardening pass.
3. **Chat-first router entrypoints:** the hardening goal here is deterministic headless automation, so explicit verbs beat natural-language dispatch.

## Review rule

Any future evaluation feature should state one of three dispositions up front:

- adopted repo-wide
- adapted narrowly
- kept specialized

If that decision is not written down, the feature is not ready to become part of the authoritative platform contract.
