#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from meta_skill_studio.app import StudioCore
from meta_skill_studio.cli_contract import (
    ALL_ACTION_NAMES,
    PIPELINE_CHOICES,
    REQUIRED_PLATFORM_DOCS,
    action_requires_role_config,
    list_action_metadata,
    resolve_action_name,
)
from meta_skill_studio.gui import launch_gui
from meta_skill_studio.tui import run_tui


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Meta Skill Studio")
    parser.add_argument("--mode", choices=["tui", "gui", "cli"], default=None, help="Interface mode")
    parser.add_argument("--action", choices=ALL_ACTION_NAMES, help="CLI action")
    parser.add_argument("--brief", help="Create skill brief or pipeline brief")
    parser.add_argument("--skill", help="Skill name")
    parser.add_argument("--goal", help="Objective/goal for the selected workflow")
    parser.add_argument("--library", choices=["LibraryUnverified", "LibraryWorkbench", "Library"], help="Library tier")
    parser.add_argument("--source", help="Source path or URL for import actions")
    parser.add_argument("--destination", help="Destination path for package/install style actions")
    parser.add_argument(
        "--from-library",
        dest="from_library",
        choices=["LibraryUnverified", "LibraryWorkbench", "Library"],
        help="Source library tier",
    )
    parser.add_argument(
        "--to-library",
        dest="to_library",
        choices=["LibraryUnverified", "LibraryWorkbench", "Library"],
        help="Target library tier",
    )
    parser.add_argument("--category", help="Category path for a source skill")
    parser.add_argument("--to-category", dest="to_category", help="Target category path")
    parser.add_argument("--provider", help="Provider name for auth operations")
    parser.add_argument("--logout", action="store_true", help="Log out the provider instead of logging in")
    parser.add_argument("--cases", type=int, default=8, help="Benchmark case count")
    parser.add_argument("--pipeline", choices=PIPELINE_CHOICES, help="Pipeline name for orchestrator actions")
    parser.add_argument("--run-id", dest="run_id", help="Pipeline run id for resume actions")
    parser.add_argument("--run-file", dest="run_file", help="Studio run artifact path")
    parser.add_argument("--before-run", dest="before_run", help="Baseline Studio run artifact path")
    parser.add_argument("--after-run", dest="after_run", help="Updated Studio run artifact path")
    parser.add_argument("--format", choices=["text", "json"], default="text", help="CLI output format")
    parser.add_argument("--setup", action="store_true", help="Run first-time OpenCode model setup")
    return parser


def _require(parser: argparse.ArgumentParser, condition: bool, message: str) -> None:
    if not condition:
        parser.error(message)


def emit_result(core: StudioCore, result: Any, output_format: str) -> None:
    if isinstance(result, Path):
        if output_format == "json":
            print(json.dumps(core.load_run_artifact(result), indent=2, ensure_ascii=True))
        else:
            print(result)
        return

    if isinstance(result, (dict, list)):
        print(json.dumps(result, indent=2, ensure_ascii=True))
        return

    if isinstance(result, str):
        if output_format == "json":
            try:
                parsed = json.loads(result)
            except json.JSONDecodeError:
                print(json.dumps({"value": result}, indent=2, ensure_ascii=True))
            else:
                print(json.dumps(parsed, indent=2, ensure_ascii=True))
        else:
            print(result)
        return

    print(str(result))


def run_cli_action(core: StudioCore, args: argparse.Namespace, parser: argparse.ArgumentParser) -> Any:
    action = resolve_action_name(args.action)
    if not action:
        parser.error("--action is required in --mode cli")

    if action == "create":
        _require(parser, bool(args.brief), "--brief is required for create")
        return core.run_create_skill(args.brief, args.library or "LibraryUnverified")
    if action == "improve":
        _require(parser, bool(args.skill and args.goal), "--skill and --goal are required for improve")
        return core.run_improve_skill(args.skill, args.goal)
    if action == "evaluate-skill":
        return core.run_test_benchmark_evaluate(args.skill)
    if action == "benchmark-skill":
        _require(parser, bool(args.skill and args.goal), "--skill and --goal are required for benchmark-skill")
        return core.run_create_benchmarks(args.skill, args.goal, args.cases)
    if action == "validate-skills":
        return core.run_validate_skills()
    if action == "run-evals":
        return core.run_eval_runner(args.skill)
    if action == "compare-runs":
        _require(parser, bool(args.before_run and args.after_run), "--before-run and --after-run are required for compare-runs")
        return core.run_compare_runs(args.before_run, args.after_run)
    if action == "improvement-brief":
        _require(parser, bool(args.run_file), "--run-file is required for improvement-brief")
        return core.run_improvement_brief(args.run_file)
    if action == "meta-manage":
        _require(parser, bool(args.goal), "--goal is required for meta-manage")
        return core.run_meta_manage(args.goal)
    if action == "catalog-audit":
        return core.run_catalog_audit(args.goal)
    if action == "find-skills":
        _require(parser, bool(args.goal), "--goal is required for find-skills")
        return core.run_find_skills(args.goal)
    if action == "import-skill":
        _require(parser, bool(args.source), "--source is required for import-skill")
        return core.run_import_skill(args.source, args.library or "LibraryUnverified", args.category)
    if action == "promote-skill":
        _require(parser, bool(args.skill and args.category and args.from_library), "--skill, --category, and --from-library are required for promote-skill")
        return core.run_promote_skill(args.skill, args.category, args.from_library)
    if action == "demote-skill":
        _require(parser, bool(args.skill and args.category and args.from_library), "--skill, --category, and --from-library are required for demote-skill")
        return core.run_demote_skill(args.skill, args.category, args.from_library)
    if action == "move-skill":
        _require(parser, bool(args.skill and args.category and args.to_category and args.library), "--skill, --category, --to-category, and --library are required for move-skill")
        return core.run_move_skill(args.skill, args.category, args.to_category, args.library)
    if action == "safety-review":
        _require(parser, bool(args.skill), "--skill is required for safety-review")
        return core.run_safety_review(args.skill, args.goal)
    if action == "provenance-review":
        _require(parser, bool(args.skill), "--skill is required for provenance-review")
        return core.run_provenance_review(args.skill, args.goal)
    if action == "package-skill":
        _require(parser, bool(args.skill), "--skill is required for package-skill")
        return core.run_package_skill(args.skill, args.destination, args.goal)
    if action == "install-skill":
        _require(parser, bool(args.skill), "--skill is required for install-skill")
        return core.run_install_skill(args.skill, args.destination, args.goal)
    if action == "lifecycle-review":
        _require(parser, bool(args.skill), "--skill is required for lifecycle-review")
        return core.run_lifecycle_review(args.skill, args.goal)
    if action == "run-pipeline":
        _require(parser, bool(args.pipeline), "--pipeline is required for run-pipeline")
        if args.pipeline == "creation":
            _require(parser, bool(args.brief), "--brief is required when --pipeline creation is selected")
        else:
            _require(parser, bool(args.skill), "--skill is required for improvement and library-management pipelines")
        return core.run_pipeline(args.pipeline, skill_name=args.skill, brief=args.brief)
    if action == "resume-pipeline":
        _require(parser, bool(args.run_id), "--run-id is required for resume-pipeline")
        return core.resume_pipeline(args.run_id)
    if action == "list-actions":
        return {"actions": list_action_metadata(), "required_platform_docs": list(REQUIRED_PLATFORM_DOCS)}
    if action == "list-skills":
        return core.list_skills_payload(args.library)
    if action == "list-runs":
        return core.list_runs_payload()
    if action == "show-run":
        _require(parser, bool(args.run_file), "--run-file is required for show-run")
        return core.load_run_artifact(args.run_file)
    if action == "list-models":
        return json.loads(core.list_models_json())
    if action == "list-providers":
        return json.loads(core.list_providers_json())
    if action == "auth-provider":
        _require(parser, bool(args.provider), "--provider is required for auth-provider")
        return json.loads(core.auth_provider(args.provider, logout=args.logout))
    if action == "opencode-stats":
        return json.loads(core.opencode_stats_json())
    raise RuntimeError(f"Unsupported action: {args.action}")


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()
    repo_root = Path(__file__).resolve().parents[1]
    core = StudioCore(repo_root)

    mode = args.mode
    if mode is None:
        mode = "tui" if sys.stdin.isatty() else "cli"

    action = resolve_action_name(args.action)

    try:
        if args.setup:
            if mode == "gui":
                core.configure_defaults(force=True)
            elif mode == "cli":
                if action_requires_role_config(action):
                    core.configure_defaults(force=True)
            else:
                core.configure_interactive_tui(force=True)

        if mode == "cli":
            if action_requires_role_config(action) and not core.load_config():
                core.configure_defaults(force=True)
            result = run_cli_action(core, args, parser)
            emit_result(core, result, args.format)
            return 0

        if not core.load_config():
            if mode == "tui":
                core.configure_interactive_tui(force=True)
            else:
                core.configure_defaults(force=True)

        if mode == "tui":
            run_tui(core)
            return 0
        launch_gui(core)
        return 0
    except (RuntimeError, ValueError) as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
