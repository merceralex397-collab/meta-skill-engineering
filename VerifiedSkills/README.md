# VerifiedSkills

Skills from `workbench/LibraryUnverified/` that have passed rigorous benchmarking.

## Purpose

This directory contains skills that have been evaluated and verified through the meta-skill engineering pipeline. Skills here represent confirmed, working skill definitions.

## Structure

Skills are organized by domain category. Each category may contain:
- `SKILL.md` - the verified skill definition
- `evals/` - evaluation test cases
- `references/` - supporting documentation
- `scripts/` - skill-specific automation

## Categories

Categories are defined by the categorizer agent based on SKILL.md content analysis. See the categorizer audit output for the complete category inventory.

## Benchmarking

Skills are evaluated using:
- `scripts/run-evals.sh` - trigger and behavior tests
- `scripts/run-trigger-optimization.sh` - trigger phrase optimization
- `scripts/run-baseline-comparison.sh` - skill vs baseline comparison

## Status

This directory starts empty and grows as skills pass benchmarking from `workbench/LibraryUnverified/`.
