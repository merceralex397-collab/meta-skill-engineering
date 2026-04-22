# Surface authority for Meta Skill Studio

This document states which surfaces are authoritative for execution and which are convenience layers.

## Authority hierarchy

| Surface | Authority level | Why |
| --- | --- | --- |
| `scripts/meta-skill-studio.py --mode cli` | **Authoritative** | This is the headless, scriptable, agent-usable execution contract |
| `scripts/meta_skill_studio/app.py` (`StudioCore`) | **Authoritative shared backend** | CLI, TUI, GUI, and WPF should all converge on this workflow truth |
| `scripts/validate-skills.sh` and `scripts/run-evals.sh` | **Authoritative support scripts** | These are the repo’s concrete structural/eval runners used by the CLI |
| `skill-orchestrator/scripts/run_pipeline.py` | **Authoritative pipeline engine** | The CLI wraps it for documented multi-phase workflows |
| Python TUI / tkinter GUI | Convenience shells | Helpful local UX, but not the source of truth |
| `windows-wpf/` application | Convenience shell / Windows delivery surface | Valuable Windows UX and packaging path, but not the only real product path |
| `scripts/meta_skill_studio/opencode_sdk_bridge.mjs` | Assistant-chat helper only | Supports assistant chat, not the authoritative workflow contract |
| `scripts/run-meta-skill-cycle.sh` | Experimental helper | Useful for experiments, not the required platform contract |

## Required alignment rules

1. **Headless first:** every core workflow must remain reachable from the Python CLI without requiring WPF, tkinter, or interactive TUI navigation.
2. **Shared workflow truth:** WPF, TUI, and GUI may rename or restyle actions, but they must not invent a conflicting workflow contract.
3. **Docs follow the CLI:** `docs/cli/action-contract.md` is the published verb inventory. UI text should map to those actions, not diverge from them.
4. **Windows-specific stays layered:** WPF can add bundling, menus, dashboards, and native shell affordances, but the workflow semantics belong to the shared Python backend and documented support scripts.

## What this means for operators

### Use these for automation

- `python scripts/meta-skill-studio.py --mode cli --action ... --format json`
- `python scripts/meta-skill-studio.py --mode cli --action list-actions --format json`
- `python scripts/meta-skill-studio.py --mode cli --action validate-skills --format json`

### Use these for convenience

- `python scripts/meta-skill-studio.py` in TUI mode when working interactively
- `python scripts/meta-skill-studio.py --mode gui` for local tkinter usage
- `windows-wpf/MetaSkillStudio.exe` for Windows-native shell usage

## Explicit non-goal

The WPF shell is **not** allowed to become the only place where create/improve/evaluate/library-management workflows are fully real. If a workflow exists only in WPF, the platform is incomplete.
