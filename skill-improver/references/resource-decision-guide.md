# Resource Decision Guide

Use this guide to decide whether a skill improvement should stay in `SKILL.md` or be split out.

## Add a reference file when

- the detail is useful but not needed every run,
- the skill supports multiple variants,
- you need tables, heuristics, or extended examples,
- keeping everything in `SKILL.md` would make routing-context too heavy.

## Keep content in `SKILL.md` when

- it is part of the core workflow,
- it is a safety or boundary rule,
- it is needed nearly every time,
- removing it would make the skill ambiguous.

## Add a script when

- the same CLI or file-transformation work keeps recurring,
- the task is deterministic or easily validated,
- the script can expose a clean `--help` contract,
- the script reduces repeated shell noise or file scaffolding.

## Do not add a script when

- the task mostly requires judgment,
- the script would hide important reasoning,
- the script exists only to make the package feel more “complete”,
- the task is unlikely to recur.

## Add evals when

- routing quality matters,
- the skill is likely to be reused,
- the skill edits files or emits structured artifacts,
- the user wants evidence, not just a rewrite.

## Add a manifest when

- the skill will be kept, shared, versioned, or packaged,
- multiple runtimes or overlays are in play,
- the package should be linted or tracked over time.
