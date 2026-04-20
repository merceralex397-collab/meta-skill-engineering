from __future__ import annotations

import json
import os
import re
import shutil
import subprocess
import textwrap
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Callable, Dict, List, Optional

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
        start = datetime.now(timezone.utc)
        try:
            proc = subprocess.run(
                cmd,
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
        duration = (datetime.now(timezone.utc) - start).total_seconds()
        return {
            "command": cmd,
            "exit_code": code,
            "stdout": stdout,
            "stderr": stderr,
            "duration_seconds": round(duration, 3),
            "started_at": start.isoformat(),
            "ended_at": datetime.now(timezone.utc).isoformat(),
        }

    def save_run(self, action: str, payload: Dict) -> Path:
        stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
        run_path = self.runs_dir / f"{stamp}-{action}.json"
        data = {"action": action, "created_at": self._now(), **payload}
        run_path.write_text(json.dumps(data, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
        return run_path

    def list_runs(self) -> List[Path]:
        return sorted(self.runs_dir.glob("*.json"), reverse=True)

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
        return self.save_run(
            "create-skill",
            {
                "input": {"brief": brief, "target_library": target_library},
                "runtime_result": result,
            },
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
        return self.save_run(
            "improve-skill",
            {"input": {"skill_name": skill_name, "goal": goal}, "runtime_result": result},
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
            4) highest priority fixes.

            Validation output:
            {validate_result["stdout"][-6000:]}

            Eval output:
            {eval_result["stdout"][-6000:]}
            """
        ).strip()
        judge_result = self.run_role_prompt("judge", judge_prompt)
        return self.save_run(
            "test-benchmark-evaluate",
            {
                "input": {"target_skill": target_skill, "runtime": runtime, "model": model},
                "validation_result": validate_result,
                "eval_result": eval_result,
                "judge_result": judge_result,
                "artifacts": {
                    "eval_results_dir": str(self.repo_root / "eval-results"),
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
        return self.save_run("meta-manage", {"input": {"objective": objective}, "runtime_result": result})

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
            "create-benchmarks",
            {
                "input": {"skill_name": skill_name, "benchmark_goal": benchmark_goal, "cases": cases},
                "judge_generation": generation,
                "artifacts": {"benchmark_file": str(out_file)},
            },
        )

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
