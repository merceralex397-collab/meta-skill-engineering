#!/usr/bin/env python3
"""
Meta Skill Studio - Native Windows Build Entry Point
Single-file executable launcher with embedded resources.
"""

from __future__ import annotations

import argparse
import sys
import os
from pathlib import Path

# nuitka-project: --standalone
# nuitka-project: --onefile
# nuitka-project: --windows-console-mode=disable
# nuitka-project: --windows-icon-from-ico=resources/app.ico
# nuitka-project: --windows-company-name="Meta Skill Studio"
# nuitka-project: --windows-product-name="Meta Skill Studio"
# nuitka-project: --windows-file-version=1.0.0.0
# nuitka-project: --windows-product-version=1.0.0.0
# nuitka-project: --windows-file-description="Meta Skill Studio - AI Skill Management Tool"
# nuitka-project: --windows-copyright="Copyright (c) 2026"
# nuitka-project: --include-package=meta_skill_studio
# nuitka-project: --include-data-dir={MAIN_DIRECTORY}/../scripts/meta_skill_studio=meta_skill_studio
# nuitka-project: --enable-plugin=tk-inter


def get_app_dir() -> Path:
    """Get the application directory, works for both dev and frozen builds."""
    if getattr(sys, "frozen", False):
        # Running as compiled executable
        return Path(sys.executable).parent
    else:
        # Running as script
        return Path(__file__).resolve().parent


def setup_environment() -> None:
    """Setup environment for the application."""
    app_dir = get_app_dir()

    # Add bundled libraries to path if they exist
    lib_dir = app_dir / "lib"
    if lib_dir.exists():
        os.environ["PATH"] = str(lib_dir) + os.pathsep + os.environ.get("PATH", "")

    # Set app data directory
    app_data = Path.home() / ".meta-skill-studio"
    app_data.mkdir(parents=True, exist_ok=True)
    os.environ["META_SKILL_STUDIO_HOME"] = str(app_data)


def main() -> int:
    """Main entry point."""
    setup_environment()

    # Import after environment setup
    import sys

    repo_root = get_app_dir().parent

    # Ensure scripts directory is in path
    scripts_dir = repo_root / "scripts"
    if str(scripts_dir) not in sys.path:
        sys.path.insert(0, str(scripts_dir))

    from meta_skill_studio.app import StudioCore
    from meta_skill_studio.gui import launch_gui
    from meta_skill_studio.tui import run_tui

    parser = argparse.ArgumentParser(description="Meta Skill Studio")
    parser.add_argument(
        "--mode", choices=["tui", "gui", "cli"], default=None, help="Interface mode"
    )
    parser.add_argument(
        "--action",
        choices=["create", "improve", "test", "meta-manage", "benchmarks"],
        help="CLI action",
    )
    parser.add_argument("--brief", help="Create skill brief")
    parser.add_argument("--skill", help="Skill name")
    parser.add_argument("--goal", help="Objective/goal for improve/meta/benchmarks")
    parser.add_argument(
        "--library",
        choices=["LibraryUnverified", "LibraryWorkbench"],
        default="LibraryUnverified",
    )
    parser.add_argument("--cases", type=int, default=8, help="Benchmark case count")
    parser.add_argument(
        "--setup", action="store_true", help="Run first-time runtime/model setup"
    )

    args = parser.parse_args()

    core = StudioCore(repo_root)

    mode = args.mode
    if mode is None:
        mode = "gui"  # Default to GUI for Windows app

    if args.setup:
        if mode == "gui":
            core.configure_defaults(force=True)
        else:
            core.configure_interactive_tui(force=True)

    if not core.load_config():
        if mode == "tui":
            core.configure_interactive_tui(force=True)
        elif mode == "gui":
            pass  # GUI will request explicit runtime/model choices
        else:
            core.configure_defaults(force=True)

    if mode == "tui":
        run_tui(core)
        return 0
    if mode == "gui":
        launch_gui(core)
        return 0

    # CLI mode
    if not args.action:
        print("Error: --action is required in CLI mode")
        return 1

    if args.action == "create":
        if not args.brief:
            print("Error: --brief is required for create")
            return 1
        run_file = core.run_create_skill(args.brief, args.library)
    elif args.action == "improve":
        if not args.skill or not args.goal:
            print("Error: --skill and --goal are required for improve")
            return 1
        run_file = core.run_improve_skill(args.skill, args.goal)
    elif args.action == "test":
        run_file = core.run_test_benchmark_evaluate(args.skill)
    elif args.action == "meta-manage":
        if not args.goal:
            print("Error: --goal is required for meta-manage")
            return 1
        run_file = core.run_meta_manage(args.goal)
    elif args.action == "benchmarks":
        if not args.skill or not args.goal:
            print("Error: --skill and --goal are required for benchmarks")
            return 1
        run_file = core.run_create_benchmarks(args.skill, args.goal, args.cases)
    else:
        print(f"Error: Unsupported action: {args.action}")
        return 1

    print(run_file)
    return 0


if __name__ == "__main__":
    sys.exit(main())
