#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

from meta_skill_studio.app import StudioCore
from meta_skill_studio.gui import launch_gui
from meta_skill_studio.tui import run_tui


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Meta Skill Studio")
    parser.add_argument("--mode", choices=["tui", "gui", "cli"], default=None, help="Interface mode")
    parser.add_argument(
        "--action",
        choices=["create", "improve", "test", "meta-manage", "benchmarks"],
        help="CLI action",
    )
    parser.add_argument("--brief", help="Create skill brief")
    parser.add_argument("--skill", help="Skill name")
    parser.add_argument("--goal", help="Objective/goal for improve/meta/benchmarks")
    parser.add_argument("--library", choices=["LibraryUnverified", "LibraryWorkbench"], default="LibraryUnverified")
    parser.add_argument("--cases", type=int, default=8, help="Benchmark case count")
    parser.add_argument("--setup", action="store_true", help="Run first-time runtime/model setup")
    return parser.parse_args()


def run_cli_action(core: StudioCore, args: argparse.Namespace) -> int:
    if not args.action:
        raise RuntimeError("--action is required in --mode cli")
    if args.action == "create":
        if not args.brief:
            raise RuntimeError("--brief is required for create")
        run_file = core.run_create_skill(args.brief, args.library)
    elif args.action == "improve":
        if not args.skill or not args.goal:
            raise RuntimeError("--skill and --goal are required for improve")
        run_file = core.run_improve_skill(args.skill, args.goal)
    elif args.action == "test":
        run_file = core.run_test_benchmark_evaluate(args.skill)
    elif args.action == "meta-manage":
        if not args.goal:
            raise RuntimeError("--goal is required for meta-manage")
        run_file = core.run_meta_manage(args.goal)
    elif args.action == "benchmarks":
        if not args.skill or not args.goal:
            raise RuntimeError("--skill and --goal are required for benchmarks")
        run_file = core.run_create_benchmarks(args.skill, args.goal, args.cases)
    else:
        raise RuntimeError(f"Unsupported action: {args.action}")
    print(run_file)
    return 0


def main() -> int:
    args = parse_args()
    repo_root = Path(__file__).resolve().parents[1]
    core = StudioCore(repo_root)

    mode = args.mode
    if mode is None:
        mode = "tui" if sys.stdin.isatty() else "cli"

    if args.setup:
        if mode == "gui":
            core.configure_defaults(force=True)
        else:
            core.configure_interactive_tui(force=True)

    if not core.load_config():
        if mode == "tui":
            core.configure_interactive_tui(force=True)
        elif mode == "gui":
            # GUI will request explicit runtime/model choices from the user.
            pass
        else:
            core.configure_defaults(force=True)

    if mode == "tui":
        run_tui(core)
        return 0
    if mode == "gui":
        launch_gui(core)
        return 0
    return run_cli_action(core, args)


if __name__ == "__main__":
    raise SystemExit(main())
