from __future__ import annotations

import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import textwrap
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Callable, Dict, List, Optional
from urllib.parse import urlparse

OPENCODE_RUNTIME = {"name": "opencode", "command": "opencode"}

ROLE_ORDER = ["create", "improve", "test", "orchestrate", "judge"]
ROLE_LABELS = {
    "create": "Creating skills",
    "improve": "Improving skills",
    "test": "Testing / Benchmarking / Evaluating",
    "orchestrate": "Meta Manage",
    "judge": "LLM Judge",
}

MODEL_PROBES = [
    ["models"],
    ["model", "list"],
    ["list-models"],
    ["--models"],
    ["--list-models"],
    ["help", "models"],
]

STOP_TOKENS = {
    "usage",
    "options",
    "help",
    "model",
    "models",
    "default",
    "version",
    "reasoning",
    "output",
    "input",
    "json",
    "yaml",
}


@dataclass
class DetectedRuntime:
    name: str
    command: str
    models: List[str]


class StudioCore:
    def __init__(self, repo_root: Path) -> None:
        self.repo_root = repo_root
        self.state_dir = repo_root / ".meta-skill-studio"
        self.config_file = self.state_dir / "config.json"
        self.opencode_config_file = repo_root / ".opencode" / "opencode.json"
        self.runs_dir = self.state_dir / "runs"
        self.library_unverified = repo_root / "LibraryUnverified"
        self.library_workbench = repo_root / "LibraryWorkbench"
        self.library_verified = repo_root / "Library"
        self.ensure_layout()

    def ensure_layout(self) -> None:
        self.state_dir.mkdir(parents=True, exist_ok=True)
        self.runs_dir.mkdir(parents=True, exist_ok=True)
        self.library_unverified.mkdir(parents=True, exist_ok=True)
        self.library_workbench.mkdir(parents=True, exist_ok=True)
        self._touch_readme(
            self.library_unverified / "README.md",
            "# LibraryUnverified\n\nRaw, untested, unevaluated skills go here before workbench promotion.\n",
        )
        self._touch_readme(
            self.library_workbench / "README.md",
            "# LibraryWorkbench\n\nSkills under active evaluation and benchmark iteration live here.\n",
        )
        self._touch_readme(
            self.library_verified / "README.md",
            "# Library\n\nVerified skills approved for production use live here.\n",
        )
        (self.library_workbench / "benchmarks").mkdir(parents=True, exist_ok=True)

    @staticmethod
    def _touch_readme(path: Path, content: str) -> None:
        if not path.exists():
            path.write_text(content, encoding="utf-8")

    @staticmethod
    def _safe_skill_slug(skill_name: str) -> str:
        slug = re.sub(r"[^A-Za-z0-9._-]+", "-", skill_name).strip("._-")
        if not slug:
            raise ValueError("Skill name must contain at least one alphanumeric character.")
        return slug

    def list_skills(self) -> List[str]:
        skills: List[str] = []
        for item in sorted(self.repo_root.iterdir()):
            if item.is_dir() and (item / "SKILL.md").is_file():
                skills.append(item.name)
        return skills

    def detect_runtimes(self) -> List[DetectedRuntime]:
        command_path = self.resolve_runtime_command(OPENCODE_RUNTIME["command"])
        if not command_path:
            return []

        discovered_models = self.discover_models(command_path)
        configured_model = self._configured_opencode_model()

        merged_models: List[str] = []
        for candidate in [configured_model, *discovered_models]:
            if candidate and candidate not in merged_models:
                merged_models.append(candidate)

        return [
            DetectedRuntime(
                name=OPENCODE_RUNTIME["name"],
                command=command_path,
                models=merged_models or ["auto"],
            )
        ]

    def _configured_opencode_model(self) -> Optional[str]:
        if not self.opencode_config_file.exists():
            return None

        try:
            config = json.loads(self.opencode_config_file.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            return None

        model = config.get("model")
        if isinstance(model, str):
            normalized = model.strip()
            return normalized or None
        return None

    def discover_models(self, command: str) -> List[str]:
        models: List[str] = []
        for probe in MODEL_PROBES:
            out = self._run_probe([command, *probe])
            if not out:
                continue
            models.extend(self._extract_models(out))
        deduped = []
        for model in models:
            if model not in deduped:
                deduped.append(model)
        return deduped[:30]

    def _run_probe(self, cmd: List[str]) -> str:
        try:
            proc = subprocess.run(
                cmd,
                cwd=self.repo_root,
                capture_output=True,
                text=True,
                timeout=8,
                check=False,
            )
        except (FileNotFoundError, subprocess.TimeoutExpired, OSError):
            return ""
        output = f"{proc.stdout}\n{proc.stderr}".strip()
        return output[:12000]

    def _extract_models(self, output: str) -> List[str]:
        tokens: List[str] = []
        for line in output.splitlines():
            line = line.strip()
            if not line or len(line) > 120:
                continue
            for tok in re.findall(r"[A-Za-z0-9][A-Za-z0-9._:/-]{2,}", line):
                lower_tok = tok.lower()
                if lower_tok in STOP_TOKENS:
                    continue
                if re.fullmatch(r"[0-9.]+", tok):
                    continue
                if "/" in tok or "-" in tok or any(ch.isdigit() for ch in tok):
                    tokens.append(tok.strip(".,:;[](){}<>\"'"))
        return [tok for tok in tokens if tok]

    def load_config(self) -> Dict:
        if not self.config_file.exists():
            return {}
        return json.loads(self.config_file.read_text(encoding="utf-8"))

    def save_config(self, config: Dict) -> None:
        self.config_file.write_text(
            json.dumps(config, indent=2, ensure_ascii=True) + "\n",
            encoding="utf-8",
        )

    def configure_interactive_tui(
        self,
        force: bool = False,
        input_fn: Callable[[str], str] = input,
        output_fn: Callable[[str], None] = print,
    ) -> Dict:
        existing = self.load_config()
        if existing and not force:
            return existing
        runtimes = self.detect_runtimes()
        if not runtimes:
            raise RuntimeError(
                "OpenCode runtime not detected. Install the repo-local OpenCode SDK/runtime dependencies or expose `opencode` on PATH."
            )
        opencode_runtime = runtimes[0]
        output_fn("\nOpenCode detected:")
        output_fn(f"  command: {opencode_runtime.command}")
        roles_cfg: Dict[str, Dict[str, str]] = {}
        for role in ROLE_ORDER:
            output_fn(f"\nSelect OpenCode model for {ROLE_LABELS[role]}:")
            model = self._choose_model(input_fn, output_fn, opencode_runtime)
            roles_cfg[role] = {"runtime": opencode_runtime.name, "model": model}
        config = self.build_config(runtimes, roles_cfg)
        self.save_config(config)
        return config

    def build_config(self, runtimes: List[DetectedRuntime], roles_cfg: Dict[str, Dict[str, str]]) -> Dict:
        existing = self.load_config()
        created_at = existing.get("created_at", self._now())
        return {
            "version": 1,
            "created_at": created_at,
            "last_updated": self._now(),
            "detected_runtimes": [
                {"name": rt.name, "command": rt.command, "models": rt.models} for rt in runtimes
            ],
            "roles": roles_cfg,
        }

    def configure_defaults(self, force: bool = False) -> Dict:
        existing = self.load_config()
        if existing and not force:
            return existing
        runtimes = self.detect_runtimes()
        if not runtimes:
            raise RuntimeError(
                "OpenCode runtime not detected. Install the repo-local OpenCode SDK/runtime dependencies or expose `opencode` on PATH."
            )
        chosen = runtimes[0]
        model = chosen.models[0] if chosen.models else "auto"
        roles_cfg = {role: {"runtime": chosen.name, "model": model} for role in ROLE_ORDER}
        config = self.build_config(runtimes, roles_cfg)
        self.save_config(config)
        return config

    @staticmethod
    def _pick_index(input_fn: Callable[[str], str], max_count: int) -> int:
        while True:
            raw = input_fn(f"Choose [1-{max_count}]: ").strip()
            if raw.isdigit():
                idx = int(raw) - 1
                if 0 <= idx < max_count:
                    return idx
            print("Invalid selection.")

    def _choose_model(
        self,
        input_fn: Callable[[str], str],
        output_fn: Callable[[str], None],
        runtime: DetectedRuntime,
    ) -> str:
        models = runtime.models or ["auto"]
        output_fn(f"Models detected for {runtime.name}:")
        for idx, model in enumerate(models, start=1):
            output_fn(f"  {idx}. {model}")
        output_fn(f"  {len(models) + 1}. Enter custom model")
        while True:
            raw = input_fn(f"Choose [1-{len(models) + 1}]: ").strip()
            if raw.isdigit():
                idx = int(raw)
                if 1 <= idx <= len(models):
                    return models[idx - 1]
                if idx == len(models) + 1:
                    custom = input_fn("Custom model id: ").strip()
                    if custom:
                        return custom
            output_fn("Invalid model selection.")

    def resolve_runtime_command(self, runtime_name: str) -> Optional[str]:
        command = self._resolve_local_opencode_command()
        if command:
            return command

        command = shutil.which(runtime_name)
        if command:
            return command
        cfg = self.load_config()
        for rt in cfg.get("detected_runtimes", []):
            if rt.get("name") == runtime_name:
                cmd = rt.get("command")
                if cmd and Path(cmd).exists():
                    return cmd
        return None

    def _resolve_local_opencode_command(self) -> Optional[str]:
        candidates = [
            self.repo_root / ".opencode" / "node_modules" / "opencode-windows-x64" / "bin" / "opencode.exe",
            self.repo_root / ".opencode" / "node_modules" / "opencode-windows-x64-baseline" / "bin" / "opencode.exe",
            self.repo_root / ".opencode" / "node_modules" / "opencode-linux-x64" / "bin" / "opencode",
            self.repo_root / ".opencode" / "node_modules" / "opencode-linux-arm64" / "bin" / "opencode",
            self.repo_root / ".opencode" / "node_modules" / ".bin" / "opencode.cmd",
            self.repo_root / ".opencode" / "node_modules" / ".bin" / "opencode",
        ]
        for candidate in candidates:
            if candidate.exists():
                return str(candidate)
        return None

    def run_role_prompt(self, role: str, prompt: str) -> Dict:
        cfg = self.load_config()
        role_cfg = cfg.get("roles", {}).get(role)
        if not role_cfg:
            raise RuntimeError(f"Role not configured: {role}")
        runtime = OPENCODE_RUNTIME["name"]
        model = role_cfg.get("model", self._configured_opencode_model() or "auto")
        command = self.resolve_runtime_command(runtime)
        if not command:
            raise RuntimeError("OpenCode runtime not available. Install the repo-local SDK/runtime dependencies or add opencode to PATH.")
        cmd = self.build_runtime_command(runtime, command, prompt, model)
        return self._run_command(cmd)

    @staticmethod
    def build_runtime_command(runtime: str, command: str, prompt: str, model: str) -> List[str]:
        cmd = [command, "-p", prompt]
        if model and model != "auto":
            cmd.extend(["--model", model])
        return cmd

    def _run_command(self, cmd: List[str]) -> Dict:
        prepared_cmd = self._prepare_command(cmd)
        start = datetime.now(timezone.utc)
        try:
            proc = subprocess.run(
                prepared_cmd,
                cwd=self.repo_root,
                capture_output=True,
                text=True,
                timeout=1800,
                check=False,
            )
            stdout = proc.stdout or ""
            stderr = proc.stderr or ""
            code = proc.returncode
        except subprocess.TimeoutExpired as exc:
            stdout = exc.stdout or ""
            stderr = (exc.stderr or "") + "\nTimed out."
            code = 124
        except OSError as exc:
            stdout = ""
            stderr = str(exc)
            code = 127
        duration = (datetime.now(timezone.utc) - start).total_seconds()
        return {
            "command": prepared_cmd,
            "exit_code": code,
            "stdout": stdout,
            "stderr": stderr,
            "duration_seconds": round(duration, 3),
            "started_at": start.isoformat(),
            "ended_at": datetime.now(timezone.utc).isoformat(),
        }

    def save_run(self, action: str, payload: Dict) -> Path:
        payload = dict(payload)
        stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
        run_path = self.runs_dir / f"{stamp}-{self._safe_run_slug(action)}.json"

        result = payload.pop("result", None) or {}
        if not result:
            result = {}
            for key in (
                "runtime_result",
                "validation_result",
                "eval_result",
                "judge_result",
                "command_result",
                "pipeline_result",
            ):
                if key in payload:
                    result[key] = payload.pop(key)

        data = {
            "kind": "meta-skill-studio-run",
            "schema_version": 1,
            "action": action,
            "created_at": self._now(),
            "repo_root": str(self.repo_root),
            "status": payload.pop("status", self._status_from_result(result)),
            "workflow": payload.pop("workflow", {"action": action}),
            "input": payload.pop("input", {}),
            "summary": payload.pop("summary", {}),
            "artifacts": payload.pop("artifacts", {}),
            "measurements": payload.pop("measurements", {}),
            "measurement_plan": payload.pop("measurement_plan", None),
            "comparison": payload.pop("comparison", None),
            "improvement_brief": payload.pop("improvement_brief", None),
            "notes": payload.pop("notes", []),
            "result": result,
        }
        if payload:
            data["result"]["legacy_payload"] = payload
        run_path.write_text(json.dumps(data, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
        return run_path

    def list_runs(self) -> List[Path]:
        return sorted(self.runs_dir.glob("*.json"), reverse=True)

    @staticmethod
    def _safe_run_slug(action: str) -> str:
        return re.sub(r"[^A-Za-z0-9._-]+", "-", action).strip("-") or "run"

    @staticmethod
    def _status_from_result(result: Dict[str, object]) -> str:
        if not result:
            return "succeeded"

        exit_codes: List[int] = []
        for value in result.values():
            if isinstance(value, dict) and isinstance(value.get("exit_code"), int):
                exit_codes.append(value["exit_code"])
        if exit_codes and any(code != 0 for code in exit_codes):
            return "failed"
        return "succeeded"

    @staticmethod
    def _measurement_summary(
        expected_steps: int,
        observed_steps: int,
        duration_seconds: Optional[float] = None,
        estimated_runtime_calls: Optional[int] = None,
        observed_runtime_calls: Optional[int] = None,
    ) -> Dict[str, object]:
        payload: Dict[str, object] = {
            "estimated": {
                "workflow_steps": expected_steps,
            },
            "observed": {
                "workflow_steps": observed_steps,
            },
            "comparison": {
                "workflow_step_delta": observed_steps - expected_steps,
            },
        }
        if duration_seconds is not None:
            payload["observed"]["duration_seconds"] = round(duration_seconds, 3)
        if estimated_runtime_calls is not None:
            payload["estimated"]["runtime_calls"] = estimated_runtime_calls
        if observed_runtime_calls is not None:
            payload["observed"]["runtime_calls"] = observed_runtime_calls
            if estimated_runtime_calls is not None:
                payload["comparison"]["runtime_call_delta"] = observed_runtime_calls - estimated_runtime_calls
        return payload

    def _prepare_command(self, cmd: List[str]) -> List[str]:
        if not cmd:
            raise RuntimeError("Command cannot be empty.")

        head = cmd[0]
        if head.lower().endswith(".sh"):
            if os.name == "nt":
                bash_command = self._resolve_bash_command()
                if not bash_command:
                    raise RuntimeError("Git Bash (or another bash executable) is required to execute .sh scripts on Windows.")
                bash_script = self._windows_path_for_bash(Path(head).resolve())
                return [bash_command, bash_script, *cmd[1:]]
            return cmd

        if head.lower().endswith(".py"):
            return [sys.executable, head, *cmd[1:]]

        return cmd

    def _resolve_bash_command(self) -> Optional[str]:
        candidates = [
            r"C:\Program Files\Git\bin\bash.exe",
            r"C:\Program Files\Git\usr\bin\bash.exe",
            shutil.which("bash"),
        ]
        for candidate in candidates:
            if candidate and Path(candidate).exists():
                if "WindowsApps" in str(candidate):
                    continue
                return str(candidate)
        return None

    @staticmethod
    def _windows_path_for_bash(path: Path) -> str:
        drive = path.drive.rstrip(":").lower()
        tail = path.as_posix().split(":/", 1)[1] if ":/" in path.as_posix() else path.as_posix()
        return f"/{drive}/{tail}"

    def load_run_artifact(self, run_reference: Path | str) -> Dict[str, object]:
        run_path = self._resolve_run_path(run_reference)
        try:
            return json.loads(run_path.read_text(encoding="utf-8"))
        except json.JSONDecodeError as exc:
            raise RuntimeError(f"Run artifact is not valid JSON: {run_path}") from exc

    def list_runs_payload(self) -> Dict[str, object]:
        runs: List[Dict[str, object]] = []
        for run_path in self.list_runs()[:50]:
            try:
                payload = self.load_run_artifact(run_path)
            except RuntimeError:
                payload = {}
            runs.append(
                {
                    "file": str(run_path),
                    "action": payload.get("action", run_path.stem),
                    "created_at": payload.get("created_at"),
                    "status": payload.get("status", "unknown"),
                    "kind": payload.get("kind", "unknown"),
                }
            )
        return {"runs": runs, "count": len(runs)}

    def list_skills_payload(self, library_name: Optional[str] = None) -> Dict[str, object]:
        if library_name:
            normalized_library = self._normalize_library_name(library_name)
            return {
                "scope": "library",
                "library": normalized_library,
                "skills": self._list_library_skills(normalized_library),
            }
        return {
            "scope": "repo-root",
            "skills": [
                {
                    "name": skill_name,
                    "path": str(self.repo_root / skill_name),
                }
                for skill_name in self.list_skills()
            ],
        }

    def _list_library_skills(self, library_name: str) -> List[Dict[str, object]]:
        entries: List[Dict[str, object]] = []
        root = self._library_root(library_name)
        for skill_md in sorted(root.rglob("SKILL.md")):
            skill_root = skill_md.parent
            relative = skill_root.relative_to(root)
            entries.append(
                {
                    "name": skill_root.name,
                    "category": str(relative.parent).replace("\\", "/") if relative.parent != Path(".") else "",
                    "path": str(skill_root),
                }
            )
        return entries

    def _resolve_run_path(self, run_reference: Path | str) -> Path:
        candidate = Path(run_reference).expanduser()
        if not candidate.is_absolute():
            if (self.runs_dir / candidate).exists():
                candidate = self.runs_dir / candidate
            else:
                candidate = (self.repo_root / candidate).resolve()
        if not candidate.exists():
            raise RuntimeError(f"Run artifact does not exist: {run_reference}")
        return candidate

    def _save_runtime_workflow_run(
        self,
        action: str,
        workflow_area: str,
        role: str,
        input_payload: Dict[str, object],
        result: Dict[str, object],
        summary: Optional[Dict[str, object]] = None,
        artifacts: Optional[Dict[str, object]] = None,
        notes: Optional[List[str]] = None,
    ) -> Path:
        runtime_calls = 1 if isinstance(result.get("exit_code"), int) else None
        return self.save_run(
            action,
            {
                "workflow": {
                    "area": workflow_area,
                    "role": role,
                    "execution_surface": "python-cli",
                },
                "input": input_payload,
                "summary": summary or {"exit_code": result.get("exit_code")},
                "artifacts": artifacts or {},
                "measurements": self._measurement_summary(
                    expected_steps=1,
                    observed_steps=1,
                    duration_seconds=result.get("duration_seconds") if isinstance(result.get("duration_seconds"), (int, float)) else None,
                    estimated_runtime_calls=runtime_calls,
                    observed_runtime_calls=runtime_calls,
                ),
                "notes": notes or [],
                "result": {"runtime_result": result},
            },
        )

    def run_validate_skills(self) -> Path:
        validate_cmd = [str(self.repo_root / "scripts" / "validate-skills.sh")]
        command_result = self._run_command(validate_cmd)
        return self.save_run(
            "validate-skills",
            {
                "workflow": {
                    "area": "evaluation",
                    "execution_surface": "python-cli",
                    "script": "scripts/validate-skills.sh",
                },
                "summary": {
                    "exit_code": command_result.get("exit_code"),
                },
                "artifacts": {"validator": str(self.repo_root / "scripts" / "validate-skills.sh")},
                "measurements": self._measurement_summary(
                    expected_steps=1,
                    observed_steps=1,
                    duration_seconds=command_result.get("duration_seconds") if isinstance(command_result.get("duration_seconds"), (int, float)) else None,
                ),
                "result": {"command_result": command_result},
            },
        )

    def run_eval_runner(self, target_skill: Optional[str] = None) -> Path:
        eval_cmd = [str(self.repo_root / "scripts" / "run-evals.sh")]
        if target_skill:
            eval_cmd.append(target_skill)
        else:
            eval_cmd.append("--all")
        command_result = self._run_command(eval_cmd)
        return self.save_run(
            "run-evals",
            {
                "workflow": {
                    "area": "evaluation",
                    "execution_surface": "python-cli",
                    "script": "scripts/run-evals.sh",
                },
                "input": {"target_skill": target_skill},
                "summary": {
                    "exit_code": command_result.get("exit_code"),
                    "target_skill": target_skill or "all",
                },
                "artifacts": {"results_dir": str(self.repo_root / "eval-results")},
                "measurements": self._measurement_summary(
                    expected_steps=1,
                    observed_steps=1,
                    duration_seconds=command_result.get("duration_seconds") if isinstance(command_result.get("duration_seconds"), (int, float)) else None,
                ),
                "result": {"command_result": command_result},
            },
        )

    def run_find_skills(self, search_query: str) -> Path:
        prompt = textwrap.dedent(
            f"""
            Find external agent skills relevant to the following topic:
            {search_query}

            Requirements:
            - Focus on GitHub-hosted or publicly documented skills.
            - Summarize candidate skills, likely use case, and import suitability.
            - Prefer concise actionable output.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "find-skills",
            "library",
            "orchestrate",
            {"search_query": search_query},
            result,
            summary={"exit_code": result.get("exit_code"), "search_query": search_query},
        )

    def run_create_skill(self, brief: str, target_library: str = "LibraryUnverified") -> Path:
        if target_library not in {"LibraryUnverified", "LibraryWorkbench"}:
            raise RuntimeError("target_library must be LibraryUnverified or LibraryWorkbench")
        prompt = textwrap.dedent(
            f"""
            You are operating inside the Meta-Skill-Engineering repository.
            Create a new skill package based on this brief:
            {brief}

            Requirements:
            - Follow AGENTS.md constraints exactly.
            - Use skill-creator workflow plus skill-testing-harness and skill-evaluation.
            - Place draft output in {target_library}.
            - Include SKILL.md with required section order.
            - Provide benchmark/eval scenarios (trigger-positive, trigger-negative, behavior).
            - Return a concise implementation report.
            """
        ).strip()
        result = self.run_role_prompt("create", prompt)
        return self._save_runtime_workflow_run(
            "create",
            "authoring",
            "create",
            {"brief": brief, "target_library": target_library},
            result,
            summary={"exit_code": result.get("exit_code"), "target_library": target_library},
        )

    def run_improve_skill(self, skill_name: str, goal: str) -> Path:
        prompt = textwrap.dedent(
            f"""
            Improve the skill package `{skill_name}` in this repository.
            Improvement goal:
            {goal}

            Requirements:
            - Run a baseline evaluation mindset first.
            - Identify anti-patterns, then apply improvements.
            - Preserve AGENTS.md section-order and inventory constraints.
            - Output before/after summary and test updates.
            """
        ).strip()
        result = self.run_role_prompt("improve", prompt)
        return self._save_runtime_workflow_run(
            "improve",
            "authoring",
            "improve",
            {"skill_name": skill_name, "goal": goal},
            result,
            summary={"exit_code": result.get("exit_code"), "skill_name": skill_name},
        )

    def run_test_benchmark_evaluate(self, target_skill: Optional[str] = None) -> Path:
        cfg = self.load_config()
        role_cfg = cfg.get("roles", {}).get("test", {})
        runtime = OPENCODE_RUNTIME["name"]
        model = role_cfg.get("model", self._configured_opencode_model() or "auto")

        validate_cmd = [str(self.repo_root / "scripts" / "validate-skills.sh")]
        eval_cmd = [str(self.repo_root / "scripts" / "run-evals.sh")]
        if target_skill:
            eval_cmd.append(target_skill)
        else:
            eval_cmd.append("--all")
        eval_cmd.extend(["--runtime", runtime, "--model", model])

        validate_result = self._run_command(validate_cmd)
        eval_result = self._run_command(eval_cmd)
        judge_prompt = textwrap.dedent(
            f"""
            Review the following validation and evaluation outputs and provide:
            1) overall quality score (0-100),
            2) routing quality notes,
            3) behavior quality notes,
            4) highest priority fixes,
            5) a short ordered remediation list.

            Validation output:
            {validate_result["stdout"][-6000:]}

            Eval output:
            {eval_result["stdout"][-6000:]}
            """
        ).strip()
        judge_result = self.run_role_prompt("judge", judge_prompt)
        quality_score = self._extract_quality_score(judge_result.get("stdout", ""))
        improvement_brief = self._build_improvement_brief(
            source_action="evaluate-skill",
            target_skill=target_skill,
            quality_score=quality_score,
            validate_result=validate_result,
            eval_result=eval_result,
            judge_result=judge_result,
        )
        measurement_plan = {
            "kind": "measurement-plan",
            "schema_version": 1,
            "objective": "Capture structural validation, eval execution, and judge synthesis for the selected scope.",
            "steps": [
                {
                    "id": "validate-skills",
                    "command": "scripts/validate-skills.sh",
                    "artifact": "result.validation_result",
                },
                {
                    "id": "run-evals",
                    "command": f"scripts/run-evals.sh {target_skill or '--all'} --runtime {runtime} --model {model}",
                    "artifact": "result.eval_result",
                },
                {
                    "id": "judge-summary",
                    "command": "judge role prompt",
                    "artifact": "result.judge_result",
                },
            ],
            "expected_artifacts": [
                ".meta-skill-studio/runs/<timestamp>-evaluate-skill.json",
                "eval-results/",
            ],
        }
        observed_steps = 3
        total_duration = sum(
            item.get("duration_seconds", 0.0)
            for item in (validate_result, eval_result, judge_result)
            if isinstance(item.get("duration_seconds"), (int, float))
        )
        return self.save_run(
            "evaluate-skill",
            {
                "workflow": {
                    "area": "evaluation",
                    "execution_surface": "python-cli",
                    "role": "judge",
                },
                "input": {"target_skill": target_skill, "runtime": runtime, "model": model},
                "summary": {
                    "target_skill": target_skill or "all",
                    "quality_score": quality_score,
                    "validation_exit_code": validate_result.get("exit_code"),
                    "eval_exit_code": eval_result.get("exit_code"),
                    "judge_exit_code": judge_result.get("exit_code"),
                },
                "artifacts": {
                    "eval_results_dir": str(self.repo_root / "eval-results"),
                },
                "measurements": self._measurement_summary(
                    expected_steps=3,
                    observed_steps=observed_steps,
                    duration_seconds=total_duration,
                    estimated_runtime_calls=1,
                    observed_runtime_calls=1,
                ),
                "measurement_plan": measurement_plan,
                "improvement_brief": improvement_brief,
                "result": {
                    "validation_result": validate_result,
                    "eval_result": eval_result,
                    "judge_result": judge_result,
                },
            },
        )

    def run_meta_manage(self, objective: str) -> Path:
        prompt = textwrap.dedent(
            f"""
            Execute meta-library management against this repository.
            Objective:
            {objective}

            Requirements:
            - Keep root inventory aligned to 17 repo-owned top-level skills.
            - Use skill-catalog-curation and skill-lifecycle-management principles.
            - Propose concrete, minimal-risk changes with rationale.
            - Include action checklist.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "meta-manage",
            "library",
            "orchestrate",
            {"objective": objective},
            result,
            summary={"exit_code": result.get("exit_code"), "objective": objective},
        )

    def run_catalog_audit(self, objective: Optional[str] = None) -> Path:
        goal = objective or "Audit library/workbench organization, misplaced skills, and category drift without changing the 17 root skill packages."
        prompt = textwrap.dedent(
            f"""
            Audit the Meta-Skill-Engineering library surfaces.
            Objective:
            {goal}

            Requirements:
            - Keep the 17 repo-owned root skill packages distinct from LibraryUnverified and LibraryWorkbench.
            - Identify duplication, misplaced packages, and missing validation/eval follow-up.
            - Return concrete findings, recommended fixes, and any safe repo changes you make.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "catalog-audit",
            "library",
            "orchestrate",
            {"objective": goal},
            result,
            summary={"exit_code": result.get("exit_code")},
        )

    def run_create_benchmarks(self, skill_name: str, benchmark_goal: str, cases: int = 8) -> Path:
        prompt = textwrap.dedent(
            f"""
            Create benchmark cases for skill `{skill_name}`.
            Benchmark goal:
            {benchmark_goal}

            Produce JSONL cases with fields:
            - prompt
            - expected
            - category
            - required_patterns (array)
            - forbidden_patterns (array)
            - min_output_lines

            Return exactly {cases} cases.
            """
        ).strip()
        generation = self.run_role_prompt("judge", prompt)
        benchmark_dir = self.library_workbench / "benchmarks"
        benchmark_dir.mkdir(parents=True, exist_ok=True)
        safe_skill_name = self._safe_skill_slug(skill_name)
        out_file = benchmark_dir / f"{safe_skill_name}-{datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')}.jsonl"
        extracted = self._extract_jsonl(generation.get("stdout", ""), cases)
        if not extracted:
            extracted = [
                json.dumps(
                    {
                        "prompt": f"Benchmark scenario {i + 1} for {skill_name}",
                        "expected": "trigger",
                        "category": "baseline",
                        "required_patterns": [skill_name],
                        "forbidden_patterns": [],
                        "min_output_lines": 6,
                    },
                    ensure_ascii=True,
                )
                for i in range(cases)
            ]
        out_file.write_text("\n".join(extracted) + "\n", encoding="utf-8")
        return self.save_run(
            "benchmark-skill",
            {
                "workflow": {
                    "area": "evaluation",
                    "execution_surface": "python-cli",
                    "role": "judge",
                },
                "input": {"skill_name": skill_name, "benchmark_goal": benchmark_goal, "cases": cases},
                "summary": {
                    "skill_name": skill_name,
                    "case_count": len(extracted),
                    "exit_code": generation.get("exit_code"),
                },
                "artifacts": {"benchmark_file": str(out_file)},
                "measurements": self._measurement_summary(
                    expected_steps=1,
                    observed_steps=1,
                    duration_seconds=generation.get("duration_seconds") if isinstance(generation.get("duration_seconds"), (int, float)) else None,
                    estimated_runtime_calls=1,
                    observed_runtime_calls=1,
                ),
                "result": {"judge_generation": generation},
            },
        )

    def run_safety_review(self, skill_name: str, goal: Optional[str] = None) -> Path:
        objective = goal or "Audit for unsafe instructions, hidden escalation paths, and missing safety boundaries."
        prompt = textwrap.dedent(
            f"""
            Perform a safety review for the skill package `{skill_name}`.
            Goal:
            {objective}

            Requirements:
            - Apply the repo's skill-safety-review mindset.
            - Call out severity, evidence, and remediation for each finding.
            - State whether the skill is safe to advance unchanged.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "safety-review",
            "governance",
            "orchestrate",
            {"skill_name": skill_name, "goal": objective},
            result,
            summary={"exit_code": result.get("exit_code"), "skill_name": skill_name},
        )

    def run_provenance_review(self, skill_name: str, goal: Optional[str] = None) -> Path:
        objective = goal or "Audit origin, trust, reference quality, and provenance follow-up for the skill package."
        prompt = textwrap.dedent(
            f"""
            Perform a provenance review for the skill package `{skill_name}`.
            Goal:
            {objective}

            Requirements:
            - Apply the repo's skill-provenance workflow.
            - Identify missing evidence, trust gaps, and required follow-up records.
            - Return a concise disposition and next steps.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "provenance-review",
            "governance",
            "orchestrate",
            {"skill_name": skill_name, "goal": objective},
            result,
            summary={"exit_code": result.get("exit_code"), "skill_name": skill_name},
        )

    def run_package_skill(self, skill_name: str, destination: Optional[str], goal: Optional[str] = None) -> Path:
        objective = goal or "Prepare packaging outputs and verify the skill package is ready to distribute internally."
        prompt = textwrap.dedent(
            f"""
            Execute packaging work for the skill package `{skill_name}`.
            Goal:
            {objective}

            Requirements:
            - Apply the repo's skill-packaging workflow.
            - Use `{destination}` as the package destination if it is provided.
            - Return the produced artifact paths or the blocking reasons if packaging cannot complete.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "package-skill",
            "distribution",
            "orchestrate",
            {"skill_name": skill_name, "destination": destination, "goal": objective},
            result,
            summary={"exit_code": result.get("exit_code"), "skill_name": skill_name},
        )

    def run_install_skill(self, skill_name: str, destination: Optional[str], goal: Optional[str] = None) -> Path:
        objective = goal or "Install the target skill into the intended client skill directory with minimal risk."
        prompt = textwrap.dedent(
            f"""
            Execute installation work for the skill package `{skill_name}`.
            Goal:
            {objective}

            Requirements:
            - Apply the repo's skill-installer workflow.
            - Use `{destination}` as the install destination if it is provided.
            - Return the install target, actions taken, and any required manual follow-up.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "install-skill",
            "distribution",
            "orchestrate",
            {"skill_name": skill_name, "destination": destination, "goal": objective},
            result,
            summary={"exit_code": result.get("exit_code"), "skill_name": skill_name},
        )

    def run_lifecycle_review(self, skill_name: str, goal: Optional[str] = None) -> Path:
        objective = goal or "Review the lifecycle state, promotion readiness, and retirement risks for the target skill."
        prompt = textwrap.dedent(
            f"""
            Execute lifecycle review for the skill package `{skill_name}`.
            Goal:
            {objective}

            Requirements:
            - Apply skill-lifecycle-management guidance.
            - Call out current state, promotion/retirement readiness, and required next actions.
            - Keep root inventory and library/workbench areas distinct.
            """
        ).strip()
        result = self.run_role_prompt("orchestrate", prompt)
        return self._save_runtime_workflow_run(
            "lifecycle-review",
            "distribution",
            "orchestrate",
            {"skill_name": skill_name, "goal": objective},
            result,
            summary={"exit_code": result.get("exit_code"), "skill_name": skill_name},
        )

    def run_pipeline(self, pipeline_name: str, skill_name: Optional[str] = None, brief: Optional[str] = None) -> Path:
        pipeline_script = self.repo_root / "skill-orchestrator" / "scripts" / "run_pipeline.py"
        cmd = [str(pipeline_script), "--repo-root", str(self.repo_root), "--create", pipeline_name]
        if skill_name:
            cmd.extend(["--skill", skill_name])
        if brief:
            cmd.extend(["--brief", brief])
        command_result = self._run_command(cmd)
        pipeline_id = self._extract_pipeline_id(command_result.get("stdout", ""))
        artifacts = {}
        if pipeline_id:
            artifacts = {
                "pipeline_state": str(self.repo_root / "tasks" / "pipelines" / f"{pipeline_id}-state.json"),
                "pipeline_report": str(self.repo_root / "tasks" / "pipelines" / f"{pipeline_id}-final-report.json"),
            }
        return self.save_run(
            "run-pipeline",
            {
                "workflow": {
                    "area": "orchestration",
                    "execution_surface": "python-cli",
                    "script": "skill-orchestrator/scripts/run_pipeline.py",
                },
                "input": {"pipeline": pipeline_name, "skill_name": skill_name, "brief": brief},
                "summary": {"exit_code": command_result.get("exit_code"), "pipeline_id": pipeline_id},
                "artifacts": artifacts,
                "measurements": self._measurement_summary(
                    expected_steps=1,
                    observed_steps=1,
                    duration_seconds=command_result.get("duration_seconds") if isinstance(command_result.get("duration_seconds"), (int, float)) else None,
                ),
                "result": {"command_result": command_result},
            },
        )

    def resume_pipeline(self, pipeline_id: str) -> Path:
        pipeline_script = self.repo_root / "skill-orchestrator" / "scripts" / "run_pipeline.py"
        cmd = [str(pipeline_script), "--repo-root", str(self.repo_root), "--resume", pipeline_id]
        command_result = self._run_command(cmd)
        return self.save_run(
            "resume-pipeline",
            {
                "workflow": {
                    "area": "orchestration",
                    "execution_surface": "python-cli",
                    "script": "skill-orchestrator/scripts/run_pipeline.py",
                },
                "input": {"pipeline_id": pipeline_id},
                "summary": {"exit_code": command_result.get("exit_code"), "pipeline_id": pipeline_id},
                "artifacts": {
                    "pipeline_state": str(self.repo_root / "tasks" / "pipelines" / f"{pipeline_id}-state.json"),
                    "pipeline_report": str(self.repo_root / "tasks" / "pipelines" / f"{pipeline_id}-final-report.json"),
                },
                "measurements": self._measurement_summary(
                    expected_steps=1,
                    observed_steps=1,
                    duration_seconds=command_result.get("duration_seconds") if isinstance(command_result.get("duration_seconds"), (int, float)) else None,
                ),
                "result": {"command_result": command_result},
            },
        )

    def run_import_skill(self, source: str, target_library: str = "LibraryUnverified", category: Optional[str] = None) -> Path:
        normalized_library = self._normalize_library_name(target_library)
        if self._looks_like_url(source):
            with tempfile.TemporaryDirectory(prefix="meta-skill-import-") as temp_dir:
                source_path, remote_metadata = self._prepare_remote_import_source(source, Path(temp_dir))
                return self._finalize_import(
                    source_path,
                    source_reference=source,
                    source_type="github",
                    target_library=normalized_library,
                    category=category,
                    extra_artifacts=remote_metadata,
                )

        source_path = Path(source).expanduser().resolve()
        if not source_path.exists() or not source_path.is_dir():
            raise RuntimeError(f"Import source does not exist or is not a directory: {source}")
        return self._finalize_import(
            source_path,
            source_reference=str(source_path),
            source_type="local",
            target_library=normalized_library,
            category=category,
        )

    def run_promote_skill(self, skill_name: str, category: str, from_library: str) -> Path:
        normalized_source = self._normalize_library_name(from_library)
        target_library = {
            "LibraryUnverified": "LibraryWorkbench",
            "LibraryWorkbench": "Library",
        }.get(normalized_source)
        if not target_library:
            raise RuntimeError("Only LibraryUnverified and LibraryWorkbench can be promoted.")

        source_path = self._skill_path(normalized_source, category, skill_name)
        destination = self._skill_path(target_library, category, skill_name)
        self._move_skill_tree(source_path, destination)

        return self.save_run(
            "promote-skill",
            {
                "workflow": {
                    "area": "library",
                    "execution_surface": "python-cli",
                },
                "input": {
                    "skill_name": skill_name,
                    "category": category,
                    "from_library": normalized_source,
                    "to_library": target_library,
                },
                "summary": {
                    "skill_name": skill_name,
                    "from_library": normalized_source,
                    "to_library": target_library,
                },
                "artifacts": {"destination": str(destination)},
            },
        )

    def run_demote_skill(self, skill_name: str, category: str, from_library: str) -> Path:
        normalized_source = self._normalize_library_name(from_library)
        target_library = {
            "Library": "LibraryWorkbench",
            "LibraryWorkbench": "LibraryUnverified",
        }.get(normalized_source)
        if not target_library:
            raise RuntimeError("Only Library and LibraryWorkbench can be demoted.")

        source_path = self._skill_path(normalized_source, category, skill_name)
        destination = self._skill_path(target_library, category, skill_name)
        self._move_skill_tree(source_path, destination)

        return self.save_run(
            "demote-skill",
            {
                "workflow": {
                    "area": "library",
                    "execution_surface": "python-cli",
                },
                "input": {
                    "skill_name": skill_name,
                    "category": category,
                    "from_library": normalized_source,
                    "to_library": target_library,
                },
                "summary": {
                    "skill_name": skill_name,
                    "from_library": normalized_source,
                    "to_library": target_library,
                },
                "artifacts": {"destination": str(destination)},
            },
        )

    def run_move_skill(self, skill_name: str, category: str, target_category: str, library: str) -> Path:
        normalized_library = self._normalize_library_name(library)
        source_path = self._skill_path(normalized_library, category, skill_name)
        destination = self._skill_path(normalized_library, target_category, skill_name)
        self._move_skill_tree(source_path, destination)
        return self.save_run(
            "move-skill",
            {
                "workflow": {
                    "area": "library",
                    "execution_surface": "python-cli",
                },
                "input": {
                    "skill_name": skill_name,
                    "category": category,
                    "target_category": target_category,
                    "library": normalized_library,
                },
                "summary": {
                    "skill_name": skill_name,
                    "library": normalized_library,
                    "target_category": target_category,
                },
                "artifacts": {"destination": str(destination)},
            },
        )

    def run_compare_runs(self, before_run: str, after_run: str) -> Path:
        before_payload = self.load_run_artifact(before_run)
        after_payload = self.load_run_artifact(after_run)
        before_quality = self._quality_score_from_run(before_payload)
        after_quality = self._quality_score_from_run(after_payload)
        before_duration = self._duration_from_run(before_payload)
        after_duration = self._duration_from_run(after_payload)

        comparison = {
            "kind": "comparison",
            "schema_version": 1,
            "before": {
                "file": str(self._resolve_run_path(before_run)),
                "action": before_payload.get("action"),
                "status": before_payload.get("status"),
                "quality_score": before_quality,
                "duration_seconds": before_duration,
            },
            "after": {
                "file": str(self._resolve_run_path(after_run)),
                "action": after_payload.get("action"),
                "status": after_payload.get("status"),
                "quality_score": after_quality,
                "duration_seconds": after_duration,
            },
            "deltas": {
                "quality_score": (after_quality - before_quality) if before_quality is not None and after_quality is not None else None,
                "duration_seconds": (after_duration - before_duration) if before_duration is not None and after_duration is not None else None,
            },
        }
        return self.save_run(
            "compare-runs",
            {
                "workflow": {
                    "area": "evaluation",
                    "execution_surface": "python-cli",
                },
                "input": {"before_run": before_run, "after_run": after_run},
                "summary": {
                    "before_action": before_payload.get("action"),
                    "after_action": after_payload.get("action"),
                    "quality_delta": comparison["deltas"]["quality_score"],
                },
                "comparison": comparison,
                "artifacts": {
                    "before_run": str(self._resolve_run_path(before_run)),
                    "after_run": str(self._resolve_run_path(after_run)),
                },
                "result": {
                    "before_summary": before_payload.get("summary", {}),
                    "after_summary": after_payload.get("summary", {}),
                },
            },
        )

    def run_improvement_brief(self, run_reference: str) -> Path:
        source_run_path = self._resolve_run_path(run_reference)
        source_payload = self.load_run_artifact(source_run_path)
        brief = source_payload.get("improvement_brief")
        if not isinstance(brief, dict):
            source_result = source_payload.get("result", {})
            validate_result = source_result.get("validation_result", {}) if isinstance(source_result, dict) else {}
            eval_result = source_result.get("eval_result", {}) if isinstance(source_result, dict) else {}
            judge_result = source_result.get("judge_result", {}) if isinstance(source_result, dict) else {}
            brief = self._build_improvement_brief(
                source_action=str(source_payload.get("action", "unknown")),
                target_skill=source_payload.get("summary", {}).get("target_skill") if isinstance(source_payload.get("summary"), dict) else None,
                quality_score=self._quality_score_from_run(source_payload),
                validate_result=validate_result if isinstance(validate_result, dict) else {},
                eval_result=eval_result if isinstance(eval_result, dict) else {},
                judge_result=judge_result if isinstance(judge_result, dict) else {},
            )
        return self.save_run(
            "improvement-brief",
            {
                "workflow": {
                    "area": "evaluation",
                    "execution_surface": "python-cli",
                },
                "input": {"run_file": str(source_run_path)},
                "summary": {
                    "source_action": source_payload.get("action"),
                    "quality_score": self._quality_score_from_run(source_payload),
                },
                "artifacts": {"source_run": str(source_run_path)},
                "improvement_brief": brief,
                "result": {"source_summary": source_payload.get("summary", {})},
            },
        )

    def _extract_pipeline_id(self, text: str) -> Optional[str]:
        created_match = re.search(r"Created pipeline:\s*([0-9a-fA-F-]{36})", text)
        if created_match:
            return created_match.group(1)
        json_match = re.search(r'"pipeline_id"\s*:\s*"([0-9a-fA-F-]{36})"', text)
        if json_match:
            return json_match.group(1)
        return None

    def _quality_score_from_run(self, payload: Dict[str, object]) -> Optional[int]:
        summary = payload.get("summary")
        if isinstance(summary, dict):
            quality = summary.get("quality_score")
            if isinstance(quality, int):
                return quality
            if isinstance(quality, float):
                return round(quality)
        improvement_brief = payload.get("improvement_brief")
        if isinstance(improvement_brief, dict):
            quality = improvement_brief.get("quality_score")
            if isinstance(quality, int):
                return quality
        return None

    def _duration_from_run(self, payload: Dict[str, object]) -> Optional[float]:
        measurements = payload.get("measurements")
        if not isinstance(measurements, dict):
            return None
        observed = measurements.get("observed")
        if not isinstance(observed, dict):
            return None
        duration = observed.get("duration_seconds")
        if isinstance(duration, (int, float)):
            return round(float(duration), 3)
        return None

    def _extract_quality_score(self, text: str) -> Optional[int]:
        patterns = [
            r"quality score\s*[:\-]?\s*(\d{1,3})\s*/\s*100",
            r"overall quality score\s*[:\-]?\s*(\d{1,3})",
            r"quality score\s*[:\-]?\s*(\d{1,3})",
        ]
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                value = int(match.group(1))
                return max(0, min(100, value))
        return None

    def _extract_priority_lines(self, text: str, max_items: int = 5) -> List[str]:
        priorities: List[str] = []
        for raw_line in text.splitlines():
            line = raw_line.strip()
            if not line:
                continue
            if re.match(r"^([-*]|\d+[.)])\s+", line):
                cleaned = re.sub(r"^([-*]|\d+[.)])\s+", "", line).strip()
                if cleaned and cleaned not in priorities:
                    priorities.append(cleaned)
            if len(priorities) >= max_items:
                break
        return priorities

    def _build_improvement_brief(
        self,
        source_action: str,
        target_skill: Optional[str],
        quality_score: Optional[int],
        validate_result: Dict[str, object],
        eval_result: Dict[str, object],
        judge_result: Dict[str, object],
    ) -> Dict[str, object]:
        priorities = self._extract_priority_lines(str(judge_result.get("stdout", "")))
        if not priorities:
            if validate_result.get("exit_code") not in {None, 0}:
                priorities.append("Resolve structural validation failures before making deeper changes.")
            if eval_result.get("exit_code") not in {None, 0}:
                priorities.append("Fix failing eval cases or runtime issues reported by run-evals.sh.")
            if judge_result.get("exit_code") not in {None, 0}:
                priorities.append("Re-run the judge summary after evaluation output is stable.")
        if quality_score is not None and quality_score < 80:
            priorities.append("Raise the quality score to at least 80 before treating the skill as stable.")
        if not priorities:
            priorities.append("Keep the current workflow stable and re-run evaluation after the next material change.")

        next_actions: List[str] = []
        if validate_result.get("exit_code") not in {None, 0}:
            next_actions.append("Run the structural validator first and fix any frontmatter/section issues.")
        if eval_result.get("exit_code") not in {None, 0}:
            next_actions.append("Inspect the eval report and repair the failing trigger or behavior cases.")
        if quality_score is not None and quality_score < 80:
            next_actions.append("Route the findings into skill-improver and re-run evaluate-skill.")
        if not next_actions:
            next_actions.append("Use the priorities list as the next improvement backlog.")

        return {
            "kind": "improvement-brief",
            "schema_version": 1,
            "source_action": source_action,
            "target_skill": target_skill or "all",
            "quality_score": quality_score,
            "priorities": priorities[:5],
            "next_actions": next_actions[:4],
        }

    def list_models_json(self) -> str:
        runtimes = self.detect_runtimes()
        payload = []
        for runtime in runtimes:
            for model in runtime.models:
                payload.append(
                    {
                        "runtime": runtime.name,
                        "model": model,
                        "provider": self._infer_provider_name(model),
                        "recommended": any(token in model.lower() for token in ("minimax", "kimi", "bigpickle", "big-pickle")),
                    }
                )
        return json.dumps({"models": payload}, indent=2, ensure_ascii=True)

    def list_providers_json(self) -> str:
        command = self.resolve_runtime_command(OPENCODE_RUNTIME["command"])
        providers: List[Dict[str, object]] = []
        raw_output = ""
        if command:
            raw_output = self._run_probe([command, "auth", "list"])
            providers = self._parse_provider_output(raw_output)
        return json.dumps({"providers": providers, "raw": raw_output}, indent=2, ensure_ascii=True)

    def auth_provider(self, provider: str, logout: bool = False) -> str:
        command = self.resolve_runtime_command(OPENCODE_RUNTIME["command"])
        if not command:
            raise RuntimeError("OpenCode runtime not detected.")
        args = [command, "auth", "logout" if logout else "login", provider]
        result = self._run_command(args)
        return json.dumps({"provider": provider, "logout": logout, "result": result}, indent=2, ensure_ascii=True)

    def opencode_stats_json(self) -> str:
        command = self.resolve_runtime_command(OPENCODE_RUNTIME["command"])
        if not command:
            return json.dumps({"stats": {}, "raw": "", "available": False}, indent=2, ensure_ascii=True)
        raw_output = self._run_probe([command, "stats"])
        return json.dumps({"stats": self._parse_stats_output(raw_output), "raw": raw_output, "available": True}, indent=2, ensure_ascii=True)

    def _normalize_library_name(self, library_name: str) -> str:
        normalized = library_name.strip()
        if normalized not in {"LibraryUnverified", "LibraryWorkbench", "Library"}:
            raise RuntimeError(f"Unsupported library: {library_name}")
        return normalized

    def _library_root(self, library_name: str) -> Path:
        normalized = self._normalize_library_name(library_name)
        if normalized == "LibraryUnverified":
            return self.library_unverified
        if normalized == "LibraryWorkbench":
            return self.library_workbench
        return self.library_verified

    def _sanitize_relative_segment(self, value: str) -> str:
        segment = value.strip().replace("\\", "/").strip("/")
        if not segment or ".." in segment.split("/"):
            raise RuntimeError(f"Unsafe category path: {value}")
        return segment

    def _skill_path(self, library_name: str, category: str, skill_name: str) -> Path:
        return self._library_root(library_name) / self._sanitize_relative_segment(category) / self._safe_skill_slug(skill_name)

    @staticmethod
    def _looks_like_url(source: str) -> bool:
        return source.startswith("http://") or source.startswith("https://")

    def _copy_skill_tree(self, source: Path, destination: Path, replace: bool = False) -> None:
        if replace and destination.exists():
            shutil.rmtree(destination)
        if destination.exists():
            raise RuntimeError(f"Destination already exists: {destination}")
        destination.parent.mkdir(parents=True, exist_ok=True)

        for item in source.rglob("*"):
            if item.is_symlink():
                raise RuntimeError(f"Symlinks are not allowed in imported skills: {item}")
            if any(part in {".git", "node_modules"} for part in item.parts):
                raise RuntimeError(f"Disallowed path in imported skill: {item}")

        shutil.copytree(source, destination)

    def _move_skill_tree(self, source: Path, destination: Path) -> None:
        if not source.exists():
            raise RuntimeError(f"Skill source does not exist: {source}")
        staging = destination.parent / f"{destination.name}.staging"
        if staging.exists():
            shutil.rmtree(staging)
        self._copy_skill_tree(source, staging, replace=False)
        if destination.exists():
            shutil.rmtree(destination)
        destination.parent.mkdir(parents=True, exist_ok=True)
        staging.rename(destination)
        shutil.rmtree(source)

    def _finalize_import(
        self,
        source_path: Path,
        source_reference: str,
        source_type: str,
        target_library: str,
        category: Optional[str],
        extra_artifacts: Optional[Dict[str, object]] = None,
    ) -> Path:
        if not (source_path / "SKILL.md").is_file():
            raise RuntimeError("Import source must contain SKILL.md at its root.")

        skill_name = source_path.name
        target_root = self._library_root(target_library)
        target_category = self._sanitize_relative_segment(category or "imported")
        destination = target_root / target_category / skill_name
        self._copy_skill_tree(source_path, destination, replace=True)

        artifacts = {"destination": str(destination)}
        if extra_artifacts:
            artifacts.update(extra_artifacts)

        return self.save_run(
            "import-skill",
            {
                "workflow": {
                    "area": "library",
                    "execution_surface": "python-cli",
                },
                "input": {
                    "source": source_reference,
                    "target_library": target_library,
                    "category": target_category,
                    "skill_name": skill_name,
                    "source_type": source_type,
                },
                "summary": {
                    "skill_name": skill_name,
                    "target_library": target_library,
                    "source_type": source_type,
                },
                "artifacts": artifacts,
            },
        )

    def _prepare_remote_import_source(self, source: str, temp_root: Path) -> tuple[Path, Dict[str, object]]:
        repo_url, ref, relative_path = self._parse_github_source(source)
        checkout_root = temp_root / "checkout"
        clone_cmd = ["git", "clone", "--depth", "1"]
        if ref:
            clone_cmd.extend(["--branch", ref])
        clone_cmd.extend([repo_url, str(checkout_root)])
        clone_result = self._run_command(clone_cmd)
        if clone_result["exit_code"] != 0:
            raise RuntimeError(f"Failed to clone GitHub source: {clone_result['stderr'] or clone_result['stdout']}".strip())

        candidate_path = (checkout_root / relative_path).resolve() if relative_path else self._discover_import_root(checkout_root)
        if not candidate_path.is_dir():
            raise RuntimeError(f"Imported GitHub path does not exist: {candidate_path}")

        return candidate_path, {"repo_url": repo_url, "ref": ref or "HEAD", "relative_path": relative_path or "."}

    def _parse_github_source(self, source: str) -> tuple[str, Optional[str], Optional[str]]:
        parsed = urlparse(source)
        if parsed.scheme not in {"http", "https"} or parsed.netloc.lower() not in {"github.com", "www.github.com"}:
            raise RuntimeError("Only GitHub repository URLs are supported for remote imports.")

        parts = [part for part in parsed.path.split("/") if part]
        if len(parts) < 2:
            raise RuntimeError("GitHub import URLs must include an owner and repository name.")

        owner = parts[0]
        repo = parts[1][:-4] if parts[1].endswith(".git") else parts[1]
        repo_url = f"https://github.com/{owner}/{repo}.git"

        if len(parts) >= 4 and parts[2] in {"tree", "blob"}:
            ref = parts[3]
            relative_path = "/".join(parts[4:]) or None
            if parts[2] == "blob" and relative_path:
                relative_path = str(Path(relative_path).parent)
            return repo_url, ref, relative_path

        if len(parts) > 2:
            raise RuntimeError("Use a repository URL or a standard GitHub tree URL for remote imports.")

        return repo_url, None, None

    def _discover_import_root(self, checkout_root: Path) -> Path:
        if (checkout_root / "SKILL.md").is_file():
            return checkout_root

        candidates = [
            path.parent
            for path in checkout_root.rglob("SKILL.md")
            if ".git" not in path.parts and "node_modules" not in path.parts
        ]
        unique_candidates = []
        for candidate in candidates:
            if candidate not in unique_candidates:
                unique_candidates.append(candidate)

        if len(unique_candidates) == 1:
            return unique_candidates[0]
        if not unique_candidates:
            raise RuntimeError("No SKILL.md file was found in the cloned GitHub source.")
        raise RuntimeError("The GitHub repository contains multiple skill packages. Use a direct tree URL to the skill you want to import.")

    @staticmethod
    def _infer_provider_name(model_name: str) -> str:
        if "/" in model_name:
            return model_name.split("/", 1)[0]
        if ":" in model_name:
            return model_name.split(":", 1)[0]
        if model_name.startswith("gpt"):
            return "openai"
        if model_name.startswith("claude"):
            return "anthropic"
        if model_name.startswith("gemini"):
            return "google"
        if model_name.startswith("minimax"):
            return "minimax"
        if model_name.startswith("kimi"):
            return "moonshot"
        if model_name.startswith("bigpickle") or model_name.startswith("big-pickle"):
            return "bigpickle"
        return "opencode"

    def _parse_provider_output(self, output: str) -> List[Dict[str, object]]:
        providers: List[Dict[str, object]] = []
        for raw_line in output.splitlines():
            line = raw_line.strip()
            if not line:
                continue
            lower = line.lower()
            if lower.startswith("provider") or lower.startswith("name") or lower.startswith("usage"):
                continue
            parts = re.split(r"\s{2,}|\t+", line)
            name = parts[0].strip("-* ").strip()
            if not name:
                continue
            authenticated = "yes" in lower or "true" in lower or "logged in" in lower or "authenticated" in lower
            providers.append({"name": name, "authenticated": authenticated, "raw": line})
        return providers

    def _parse_stats_output(self, output: str) -> Dict[str, object]:
        stats: Dict[str, object] = {}
        for raw_line in output.splitlines():
            line = raw_line.strip()
            if ":" not in line:
                continue
            key, value = line.split(":", 1)
            key = re.sub(r"[^a-z0-9]+", "_", key.strip().lower()).strip("_")
            stats[key] = value.strip()
        return stats

    def _extract_jsonl(self, text: str, max_cases: int) -> List[str]:
        rows: List[str] = []
        for line in text.splitlines():
            line = line.strip()
            if not line or not line.startswith("{") or not line.endswith("}"):
                continue
            try:
                obj = json.loads(line)
            except json.JSONDecodeError:
                continue
            if isinstance(obj, dict) and "prompt" in obj:
                rows.append(json.dumps(obj, ensure_ascii=True))
            if len(rows) >= max_cases:
                break
        return rows

    @staticmethod
    def _now() -> str:
        return datetime.now(timezone.utc).isoformat()
