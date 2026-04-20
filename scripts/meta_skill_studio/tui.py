from __future__ import annotations

import json
from pathlib import Path
from typing import Optional

from .app import ROLE_LABELS, StudioCore


def _read_nonempty(prompt: str) -> str:
    while True:
        value = input(prompt).strip()
        if value:
            return value
        print("Value required.")


def _select_target_skill(core: StudioCore) -> Optional[str]:
    skills = core.list_skills()
    print("\nAvailable skills:")
    for idx, skill in enumerate(skills, start=1):
        print(f"  {idx}. {skill}")
    raw = input("Skill index (blank for --all): ").strip()
    if not raw:
        return None
    if not raw.isdigit():
        print("Invalid index.")
        return None
    idx = int(raw) - 1
    if idx < 0 or idx >= len(skills):
        print("Invalid index.")
        return None
    return skills[idx]


def _print_run_preview(run_file: Path) -> None:
    data = json.loads(run_file.read_text(encoding="utf-8"))
    print(f"\nRun saved: {run_file}")
    print(f"Action: {data.get('action')}")
    if "runtime_result" in data:
        result = data["runtime_result"]
        print(f"Exit: {result.get('exit_code')} | Duration: {result.get('duration_seconds')}s")
        print(result.get("stdout", "")[-1200:])
    if "judge_result" in data:
        judge = data["judge_result"]
        print("\nJudge output:")
        print(judge.get("stdout", "")[-1200:])
    if "artifacts" in data:
        print("Artifacts:", data["artifacts"])


def run_tui(core: StudioCore) -> None:
    print("Meta Skill Studio (TUI)")
    print("Cross-platform user-facing workflow for create/improve/test/meta-manage/benchmarks.\n")

    if not core.load_config():
        print("First run setup required.")
        core.configure_interactive_tui(force=True)

    while True:
        print("\nMenu")
        print("1. Create skill")
        print("2. Improve skill")
        print("3. Test / benchmark / evaluate skill")
        print("4. Meta Manage")
        print("5. Create benchmarks")
        print("6. Reconfigure OpenCode models")
        print("7. Show recent runs")
        print("8. Exit")
        choice = input("Select option: ").strip()

        try:
            if choice == "1":
                brief = _read_nonempty("Skill brief: ")
                target = input("Target library [u=Unverified, w=Workbench] (default u): ").strip().lower()
                library = "LibraryWorkbench" if target == "w" else "LibraryUnverified"
                run_file = core.run_create_skill(brief=brief, target_library=library)
                _print_run_preview(run_file)
            elif choice == "2":
                skill = _read_nonempty("Skill name: ")
                goal = _read_nonempty("Improvement goal: ")
                run_file = core.run_improve_skill(skill_name=skill, goal=goal)
                _print_run_preview(run_file)
            elif choice == "3":
                target_skill = _select_target_skill(core)
                run_file = core.run_test_benchmark_evaluate(target_skill=target_skill)
                _print_run_preview(run_file)
            elif choice == "4":
                objective = _read_nonempty("Meta-manage objective: ")
                run_file = core.run_meta_manage(objective=objective)
                _print_run_preview(run_file)
            elif choice == "5":
                skill = _read_nonempty("Skill name for benchmark generation: ")
                goal = _read_nonempty("Benchmark goal: ")
                raw_cases = input("Number of benchmark cases (default 8): ").strip()
                cases = int(raw_cases) if raw_cases.isdigit() else 8
                run_file = core.run_create_benchmarks(skill_name=skill, benchmark_goal=goal, cases=cases)
                _print_run_preview(run_file)
            elif choice == "6":
                print("Re-running OpenCode model setup.")
                config = core.configure_interactive_tui(force=True)
                print("Updated roles:")
                for role, role_cfg in config["roles"].items():
                    print(f"  {ROLE_LABELS[role]}: {role_cfg['runtime']} / {role_cfg['model']}")
            elif choice == "7":
                runs = core.list_runs()
                if not runs:
                    print("No runs found.")
                else:
                    for run in runs[:20]:
                        print(run)
            elif choice == "8":
                break
            else:
                print("Invalid selection.")
        except Exception as exc:  # noqa: BLE001
            print(f"Error: {exc}")

