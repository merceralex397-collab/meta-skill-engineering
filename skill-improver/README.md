# skill-improver

A meta-skill for improving existing skills.

This package is intentionally opinionated about four things:

1. routing quality,
2. procedural depth,
3. justified support layers,
4. evaluation readiness.

## Why this package has references and evals

The core `SKILL.md` stays focused on the improvement workflow. Supporting heuristics, decision guides, and evaluation patterns are moved into `references/` and `evals/` so the main file does not become a giant context blob.

## Why this package only has small scripts

Improving a skill is mostly judgment work. The scripts here only handle repetitive mechanics:

- structural linting of a skill package,
- bootstrapping eval files.

They do **not** attempt to automatically decide whether a skill is good.
