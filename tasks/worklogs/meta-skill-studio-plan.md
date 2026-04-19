# Meta Skill Studio Plan

## Goal
Build a hardened user-facing system that can create, improve, test, benchmark, and meta-manage skill packages through both terminal (TUI/CLI) and desktop GUI entrypoints.

## Scope Decisions

1. Keep the 16 top-level skill package inventory as-is (no condensation): current repo already matches the required 16-package boundary.
2. Reuse and upgrade existing orchestration assets instead of replacing:
   - `scripts/run-evals.sh` (extended for behavior tests and runtime selection)
   - `scripts/validate-skills.sh` (structural validation)
3. Add a single product surface called **Meta Skill Studio** with two interfaces:
   - TUI/CLI for Linux/WSL/Windows terminals
   - Tkinter GUI for desktop platforms with display support

## Execution Plan

1. Baseline and branch carryover
   - Switch to `main`.
   - Pull useful, non-duplicate branch additions: broad eval coverage and skill manifests, plus validation and behavior-eval improvements.
2. Runtime and model layer
   - Auto-detect installed runtimes: `codex`, `gemini`, `copilot`, `opencode`, `kilocode`.
   - Probe candidate model lists per runtime.
   - Persist role-based runtime/model assignments:
     - create
     - improve
     - test
     - orchestrate
     - judge
3. Workflow engine
   - Implement first-class actions:
     1. Create skill
     2. Improve skill
     3. Test / benchmark / evaluate skill
     4. Meta Manage
     5. Create benchmarks
   - Capture command outputs and judge output into run artifacts.
4. Interfaces
   - TUI menu with role actions and reconfiguration path.
   - GUI with run history panel, output viewer, and action buttons.
5. Libraries and UX hardening
   - Add `LibraryUnverified/` and `LibraryWorkbench/` with clear purpose.
   - Add benchmark output path under `LibraryWorkbench/benchmarks/`.
6. Documentation and verification
   - Document launch and operation in `README.md`.
   - Run smoke checks for syntax/help/dry-run behavior.

## Verification Checklist

- `./scripts/meta-skill-studio.py --help`
- `./scripts/meta-skill-studio.py --mode cli --action test --skill skill-creator`
- `./scripts/run-evals.sh --dry-run --all --runtime copilot --model auto`
- `./scripts/validate-skills.sh`

