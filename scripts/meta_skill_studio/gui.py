from __future__ import annotations

import json
import os
import threading
from pathlib import Path

from .app import ROLE_LABELS, ROLE_ORDER, StudioCore


def launch_gui(core: StudioCore) -> None:
    if _is_wsl_without_display():
        raise RuntimeError("GUI unavailable in WSL without DISPLAY. Use TUI mode.")
    try:
        import tkinter as tk
        from tkinter import messagebox, simpledialog
        from tkinter.scrolledtext import ScrolledText
    except ModuleNotFoundError as exc:
        raise RuntimeError("tkinter is not installed. Use TUI mode or install python3-tk.") from exc

    class StudioGui(tk.Tk):
        def __init__(self) -> None:
            super().__init__()
            self.title("Meta Skill Studio")
            self.geometry("1200x760")
            self._build()
            self._load_runs()
            if not core.load_config():
                self.after(100, self._reconfigure)

        def _build(self) -> None:
            top = tk.Frame(self)
            top.pack(fill=tk.X, padx=8, pady=8)
            buttons = [
                ("Create skill", self._create_skill),
                ("Improve skill", self._improve_skill),
                ("Test/benchmark/eval", self._test_skill),
                ("Meta Manage", self._meta_manage),
                ("Create benchmarks", self._create_benchmarks),
                ("Reconfigure", self._reconfigure),
                ("Refresh runs", self._load_runs),
            ]
            for label, handler in buttons:
                tk.Button(top, text=label, command=handler).pack(side=tk.LEFT, padx=4)

            body = tk.PanedWindow(self, orient=tk.HORIZONTAL, sashrelief=tk.RAISED)
            body.pack(fill=tk.BOTH, expand=True, padx=8, pady=(0, 8))

            left = tk.Frame(body)
            right = tk.Frame(body)
            body.add(left, width=340)
            body.add(right)

            tk.Label(left, text="Run History").pack(anchor=tk.W)
            self.run_list = tk.Listbox(left)
            self.run_list.pack(fill=tk.BOTH, expand=True)
            self.run_list.bind("<<ListboxSelect>>", self._on_run_select)

            tk.Label(right, text="Output").pack(anchor=tk.W)
            self.output = ScrolledText(right, wrap=tk.WORD)
            self.output.pack(fill=tk.BOTH, expand=True)

            self.status_var = tk.StringVar(value="Ready")
            tk.Label(self, textvariable=self.status_var, anchor=tk.W).pack(fill=tk.X, padx=8, pady=(0, 8))

        def _set_output(self, text: str) -> None:
            self.output.delete("1.0", tk.END)
            self.output.insert(tk.END, text)
            self.output.see(tk.END)

        def _append_output(self, text: str) -> None:
            self.output.insert(tk.END, text)
            self.output.see(tk.END)

        def _load_runs(self) -> None:
            self.run_files = core.list_runs()
            self.run_list.delete(0, tk.END)
            for run in self.run_files:
                self.run_list.insert(tk.END, run.name)

        def _on_run_select(self, _event: object) -> None:
            selected = self.run_list.curselection()
            if not selected:
                return
            run_file = self.run_files[selected[0]]
            self._set_output(run_file.read_text(encoding="utf-8"))

        def _run_async(self, label: str, fn, *args) -> None:
            self.status_var.set(f"Running: {label}")
            self._append_output(f"\n\n=== {label} ===\n")

            def runner() -> None:
                try:
                    run_file = fn(*args)
                    data = json.loads(Path(run_file).read_text(encoding="utf-8"))
                    self.after(0, lambda: self._append_output(json.dumps(data, indent=2) + "\n"))
                    self.after(0, self._load_runs)
                    self.after(0, lambda: self.status_var.set(f"Completed: {run_file.name}"))
                except Exception as exc:  # noqa: BLE001
                    self.after(0, lambda: messagebox.showerror("Error", str(exc)))
                    self.after(0, lambda: self.status_var.set("Failed"))

            threading.Thread(target=runner, daemon=True).start()

        def _create_skill(self) -> None:
            brief = simpledialog.askstring("Create skill", "Skill brief:")
            if not brief:
                return
            target = simpledialog.askstring(
                "Create skill",
                "Target library (LibraryUnverified or LibraryWorkbench):",
                initialvalue="LibraryUnverified",
            )
            if not target:
                target = "LibraryUnverified"
            self._run_async("Create skill", core.run_create_skill, brief, target)

        def _improve_skill(self) -> None:
            skill = simpledialog.askstring("Improve skill", "Skill name:")
            if not skill:
                return
            goal = simpledialog.askstring("Improve skill", "Improvement goal:")
            if not goal:
                return
            self._run_async("Improve skill", core.run_improve_skill, skill, goal)

        def _test_skill(self) -> None:
            skill = simpledialog.askstring("Test skill", "Skill name (blank for all):", initialvalue="")
            if skill == "":
                skill = None
            self._run_async("Test / benchmark / evaluate", core.run_test_benchmark_evaluate, skill)

        def _meta_manage(self) -> None:
            objective = simpledialog.askstring("Meta Manage", "Objective:")
            if not objective:
                return
            self._run_async("Meta Manage", core.run_meta_manage, objective)

        def _create_benchmarks(self) -> None:
            skill = simpledialog.askstring("Create benchmarks", "Skill name:")
            if not skill:
                return
            goal = simpledialog.askstring("Create benchmarks", "Benchmark goal:")
            if not goal:
                return
            cases = simpledialog.askinteger(
                "Create benchmarks",
                "Case count:",
                initialvalue=8,
                minvalue=1,
                maxvalue=100,
            )
            if not cases:
                cases = 8
            self._run_async("Create benchmarks", core.run_create_benchmarks, skill, goal, cases)

        def _reconfigure(self) -> None:
            try:
                runtimes = core.detect_runtimes()
                if not runtimes:
                    raise RuntimeError(
                        "OpenCode runtime not detected. Install the repo-local OpenCode SDK/runtime dependencies or expose `opencode` on PATH."
                    )
                opencode_runtime = runtimes[0]
                roles_cfg = {}
                for role in ROLE_ORDER:
                    model_default = opencode_runtime.models[0] if opencode_runtime.models else "auto"
                    model = simpledialog.askstring(
                        "Configuration",
                        f"{ROLE_LABELS[role]} OpenCode model:",
                        initialvalue=model_default,
                    )
                    if not model:
                        raise RuntimeError("Configuration cancelled.")
                    roles_cfg[role] = {"runtime": opencode_runtime.name, "model": model}
                config = core.build_config(runtimes, roles_cfg)
                core.save_config(config)
                messagebox.showinfo("Configuration", "OpenCode model configuration updated.")
            except Exception as exc:  # noqa: BLE001
                messagebox.showerror("Configuration error", str(exc))

    app = StudioGui()
    app.mainloop()


def _is_wsl_without_display() -> bool:
    try:
        with open("/proc/version", "r", encoding="utf-8") as handle:
            is_wsl = "microsoft" in handle.read().lower()
    except OSError:
        is_wsl = False
    return bool(is_wsl and not os.environ.get("DISPLAY"))

