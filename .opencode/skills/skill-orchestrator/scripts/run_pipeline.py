#!/usr/bin/env python3
"""
Pipeline execution engine for skill-orchestrator.
Runs multi-phase pipelines with state persistence and conditional branching.
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import uuid
from datetime import datetime
from enum import Enum
from pathlib import Path
from typing import Any


class PipelineType(Enum):
    CREATION = "creation"
    IMPROVEMENT = "improvement"
    LIBRARY_MANAGEMENT = "library-management"


class PhaseStatus(Enum):
    PENDING = "pending"
    RUNNING = "running"
    COMPLETED = "completed"
    FAILED = "failed"
    SKIPPED = "skipped"


# Pipeline definitions
PIPELINES = {
    PipelineType.CREATION: [
        "skill-creator",
        "skill-testing-harness",
        "skill-evaluation",
        "skill-trigger-optimization",
        "skill-safety-review",
        "skill-provenance",
        "skill-packaging",
        "skill-lifecycle-management",
    ],
    PipelineType.IMPROVEMENT: [
        "skill-anti-patterns",
        "skill-improver",
        "skill-evaluation",
        "skill-trigger-optimization",
    ],
    PipelineType.LIBRARY_MANAGEMENT: [
        "skill-catalog-curation",
        "skill-lifecycle-management",
    ],
}


class PipelineEngine:
    def __init__(self, repo_root: Path):
        self.repo_root = repo_root
        self.pipelines_dir = repo_root / "tasks" / "pipelines"
        self.pipelines_dir.mkdir(parents=True, exist_ok=True)

    def create_pipeline(
        self,
        pipeline_type: PipelineType,
        target_skill: str | None = None,
        brief: str | None = None,
    ) -> str:
        """Create a new pipeline and return its ID."""
        pipeline_id = str(uuid.uuid4())
        skills = PIPELINES[pipeline_type]

        state = {
            "pipeline_id": pipeline_id,
            "pipeline_type": pipeline_type.value,
            "target_skill": target_skill,
            "brief": brief,
            "start_time": datetime.utcnow().isoformat(),
            "status": "running",
            "current_phase": 0,
            "phases": [
                {
                    "phase_id": i + 1,
                    "skill": skill,
                    "status": PhaseStatus.PENDING.value,
                    "input": {},
                    "output": {},
                    "exit_code": None,
                    "start_time": None,
                    "end_time": None,
                    "decision_branch": None,
                }
                for i, skill in enumerate(skills)
            ],
        }

        self._save_state(pipeline_id, state)
        self._log(
            pipeline_id,
            f"Pipeline created: {pipeline_type.value} for {target_skill or 'new skill'}",
        )
        return pipeline_id

    def run_pipeline(self, pipeline_id: str) -> dict[str, Any]:
        """Execute a pipeline from current state."""
        state = self._load_state(pipeline_id)
        if not state:
            raise ValueError(f"Pipeline not found: {pipeline_id}")

        self._log(
            pipeline_id, f"Resuming pipeline from phase {state['current_phase'] + 1}"
        )

        while state["current_phase"] < len(state["phases"]):
            phase_idx = state["current_phase"]
            phase = state["phases"][phase_idx]

            if phase["status"] == PhaseStatus.COMPLETED.value:
                state["current_phase"] += 1
                continue

            # Execute phase
            result = self._execute_phase(pipeline_id, state, phase_idx)

            # Handle result
            if result["exit_code"] != 0:
                # Check for retry
                retry_count = phase.get("retry_count", 0)
                if retry_count < 2:
                    phase["retry_count"] = retry_count + 1
                    self._log(
                        pipeline_id,
                        f"Retrying phase {phase_idx + 1} (attempt {retry_count + 2})",
                    )
                    continue
                else:
                    phase["status"] = PhaseStatus.FAILED.value
                    state["status"] = "failed"
                    self._save_state(pipeline_id, state)
                    self._generate_report(pipeline_id, state)
                    return self._build_result(state)

            # Success - check for conditional branching
            phase["status"] = PhaseStatus.COMPLETED.value
            self._check_conditional_branch(state, phase_idx, result)

            state["current_phase"] += 1
            self._save_state(pipeline_id, state)

        # All phases complete
        state["status"] = "completed"
        state["end_time"] = datetime.utcnow().isoformat()
        self._save_state(pipeline_id, state)
        self._generate_report(pipeline_id, state)
        self._log(pipeline_id, "Pipeline completed successfully")

        return self._build_result(state)

    def _execute_phase(
        self, pipeline_id: str, state: dict, phase_idx: int
    ) -> dict[str, Any]:
        """Execute a single phase and return result."""
        phase = state["phases"][phase_idx]
        skill = phase["skill"]

        phase["status"] = PhaseStatus.RUNNING.value
        phase["start_time"] = datetime.utcnow().isoformat()
        self._save_state(pipeline_id, state)

        self._log(pipeline_id, f"Executing phase {phase_idx + 1}: {skill}")

        # Build command
        cmd = self._build_command(skill, state, phase_idx)

        # Run skill
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=1800,  # 30 min timeout
                cwd=self.repo_root,
            )

            phase["exit_code"] = result.returncode
            phase["output"] = {
                "stdout": result.stdout,
                "stderr": result.stderr,
            }
            phase["end_time"] = datetime.utcnow().isoformat()

            return {
                "exit_code": result.returncode,
                "stdout": result.stdout,
                "stderr": result.stderr,
            }

        except subprocess.TimeoutExpired:
            phase["exit_code"] = -1
            phase["output"] = {"stdout": "", "stderr": "Timeout after 30 minutes"}
            return {"exit_code": -1, "stdout": "", "stderr": "Timeout"}

        except Exception as e:
            phase["exit_code"] = -1
            phase["output"] = {"stdout": "", "stderr": str(e)}
            return {"exit_code": -1, "stdout": "", "stderr": str(e)}

    def _build_command(self, skill: str, state: dict, phase_idx: int) -> list[str]:
        """Build command arguments for a skill."""
        script_path = self.repo_root / "scripts" / "meta-skill-studio.py"
        cmd = [sys.executable, str(script_path), "--mode", "cli"]

        # Map skill to action
        action_map = {
            "skill-creator": "create",
            "skill-improver": "improve",
            "skill-testing-harness": "test",
            "skill-evaluation": "test",
            "skill-trigger-optimization": "improve",
            "skill-safety-review": "test",
            "skill-provenance": "test",
            "skill-packaging": "test",
            "skill-lifecycle-management": "meta-manage",
            "skill-catalog-curation": "meta-manage",
            "skill-anti-patterns": "test",
        }

        action = action_map.get(skill, "test")
        cmd.extend(["--action", action])

        # Add parameters
        if state.get("target_skill"):
            cmd.extend(["--skill", state["target_skill"]])

        if state.get("brief") and action == "create":
            cmd.extend(["--brief", state["brief"]])

        return cmd

    def _check_conditional_branch(
        self, state: dict, phase_idx: int, result: dict[str, Any]
    ) -> None:
        """Check if we need to insert a conditional branch based on results."""
        stdout = result.get("stdout", "")

        # Parse quality score
        quality_score = self._extract_quality_score(stdout)
        if quality_score is not None and quality_score < 60:
            # Insert improvement phase
            self._insert_phase(
                state,
                phase_idx + 1,
                "skill-improver",
                {"reason": f"quality_score_{quality_score}"},
            )

        # Check trigger precision
        if "trigger_precision" in stdout.lower():
            precision = self._extract_trigger_precision(stdout)
            if precision is not None and precision < 0.80:
                self._insert_phase(
                    state,
                    phase_idx + 1,
                    "skill-trigger-optimization",
                    {"reason": f"precision_{precision}"},
                )

    def _insert_phase(
        self, state: dict, after_idx: int, skill: str, context: dict
    ) -> None:
        """Insert a new phase after the given index."""
        new_phase = {
            "phase_id": len(state["phases"]) + 1,
            "skill": skill,
            "status": PhaseStatus.PENDING.value,
            "input": context,
            "output": {},
            "exit_code": None,
            "start_time": None,
            "end_time": None,
            "decision_branch": True,
        }
        state["phases"].insert(after_idx + 1, new_phase)

    def _extract_quality_score(self, stdout: str) -> int | None:
        """Extract quality score from stdout."""
        import re

        match = re.search(r"quality score[:\s]+(\d+)", stdout, re.IGNORECASE)
        if match:
            return int(match.group(1))
        return None

    def _extract_trigger_precision(self, stdout: str) -> float | None:
        """Extract trigger precision from stdout."""
        import re

        match = re.search(
            r"trigger[\s_]*precision[:\s]+(\d+\.?\d*)", stdout, re.IGNORECASE
        )
        if match:
            return float(match.group(1))
        return None

    def _save_state(self, pipeline_id: str, state: dict) -> None:
        """Persist pipeline state to disk."""
        state_file = self.pipelines_dir / f"{pipeline_id}-state.json"
        with open(state_file, "w") as f:
            json.dump(state, f, indent=2)

    def _load_state(self, pipeline_id: str) -> dict | None:
        """Load pipeline state from disk."""
        state_file = self.pipelines_dir / f"{pipeline_id}-state.json"
        if not state_file.exists():
            return None
        with open(state_file) as f:
            return json.load(f)

    def _log(self, pipeline_id: str, message: str) -> None:
        """Write to pipeline log."""
        log_file = self.pipelines_dir / f"{pipeline_id}-log.md"
        timestamp = datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S")
        with open(log_file, "a") as f:
            f.write(f"[{timestamp}] {message}\n")

    def _generate_report(self, pipeline_id: str, state: dict) -> None:
        """Generate final report."""
        report_file = self.pipelines_dir / f"{pipeline_id}-final-report.json"

        completed = sum(
            1 for p in state["phases"] if p["status"] == PhaseStatus.COMPLETED.value
        )
        failed = sum(
            1 for p in state["phases"] if p["status"] == PhaseStatus.FAILED.value
        )

        report = {
            "pipeline_id": pipeline_id,
            "status": state["status"],
            "pipeline_type": state["pipeline_type"],
            "target_skill": state.get("target_skill"),
            "start_time": state["start_time"],
            "end_time": state.get("end_time"),
            "phases_total": len(state["phases"]),
            "phases_completed": completed,
            "phases_failed": failed,
            "phases": state["phases"],
        }

        with open(report_file, "w") as f:
            json.dump(report, f, indent=2)

    def _build_result(self, state: dict) -> dict[str, Any]:
        """Build return result."""
        completed = sum(
            1 for p in state["phases"] if p["status"] == PhaseStatus.COMPLETED.value
        )
        failed = sum(
            1 for p in state["phases"] if p["status"] == PhaseStatus.FAILED.value
        )

        return {
            "pipeline_id": state["pipeline_id"],
            "status": state["status"],
            "phases_executed": len(state["phases"]),
            "phases_successful": completed,
            "phases_failed": failed,
            "report_path": str(
                self.pipelines_dir / f"{state['pipeline_id']}-final-report.json"
            ),
            "resume_possible": state["status"] == "failed"
            and completed < len(state["phases"]),
        }


def main():
    parser = argparse.ArgumentParser(description="Pipeline execution engine")
    parser.add_argument(
        "--create",
        choices=["creation", "improvement", "library-management"],
        help="Create new pipeline",
    )
    parser.add_argument("--run", metavar="PIPELINE_ID", help="Run pipeline by ID")
    parser.add_argument(
        "--resume", metavar="PIPELINE_ID", help="Resume halted pipeline"
    )
    parser.add_argument("--skill", help="Target skill name")
    parser.add_argument("--brief", help="Skill creation brief")
    parser.add_argument("--repo-root", default=".", help="Repository root path")

    args = parser.parse_args()

    repo_root = Path(args.repo_root).resolve()
    engine = PipelineEngine(repo_root)

    if args.create:
        pipeline_type = PipelineType(args.create)
        pipeline_id = engine.create_pipeline(
            pipeline_type, target_skill=args.skill, brief=args.brief
        )
        print(f"Created pipeline: {pipeline_id}")
        result = engine.run_pipeline(pipeline_id)
        print(json.dumps(result, indent=2))

    elif args.run:
        result = engine.run_pipeline(args.run)
        print(json.dumps(result, indent=2))

    elif args.resume:
        result = engine.run_pipeline(args.resume)
        print(json.dumps(result, indent=2))

    else:
        parser.print_help()
        sys.exit(1)


if __name__ == "__main__":
    main()
