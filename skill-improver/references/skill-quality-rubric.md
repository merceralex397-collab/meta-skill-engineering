# Skill Quality Rubric

Use this rubric when deciding what a skill actually needs.

## 1. Routing

Questions:
- Does the description clearly state the main action?
- Does it include realistic trigger phrases or task shapes?
- Are there obvious near-miss tasks that should be excluded?
- Would a cold reader know when *not* to use it?

Red flags:
- generic descriptions,
- purely branding language,
- no examples,
- no negative boundary where overlap is likely.

## 2. Procedure

Questions:
- Does the body tell the agent how to work, not just what to care about?
- Are steps ordered?
- Are common variants handled by phases, modes, or decision tables?
- Is there a finish line or quality gate?

Red flags:
- checklist fog,
- generic best-practice bullets,
- no branch handling,
- no failure handling.

## 3. Support layers

Questions:
- Are references used for optional depth rather than core logic?
- Are scripts only added for repeated mechanics?
- Are evals present when reuse matters?
- Is a manifest present when the skill is meant to persist?

Red flags:
- giant single-file skills,
- ornamental scripts,
- reference dumping,
- no proof the skill helps.

## 4. Maintainability

Questions:
- Is the scope narrow enough to stay coherent?
- Is ownership/versioning implied or explicit?
- Can the package be updated without rewriting everything?
- Is the structure obvious to another maintainer?

Red flags:
- silent repurposing,
- mixed concerns,
- duplicate content across files,
- no change notes for a durable package.
