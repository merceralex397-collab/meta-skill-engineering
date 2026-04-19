---
name: cli-development-python
description: >-
  Build Python CLI tools with Click or Typer, argument validation, and
  rich output. Use when creating a Python CLI app, adding Click/Typer
  commands, implementing --verbose/--json flags, or packaging with
  setuptools entry_points. Do not use for Go CLIs (prefer cli-development-go)
  or shell scripts (prefer bash).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cli-development-python
  maturity: draft
  risk: low
  tags: [python, cli, click, typer]
---

# Purpose

Build Python CLI tools using Click or Typer with proper argument handling, output formatting, and packaging.

# When to use this skill

- creating a new Python CLI tool with commands and subcommands
- adding Click decorators or Typer type hints for argument parsing
- implementing `--verbose`, `--json`, or `--quiet` output modes
- packaging a CLI with `pyproject.toml` entry points

# Do not use this skill when

- building a Go CLI — prefer `cli-development-go`
- writing a Bash script — prefer `bash`
- building an interactive TUI — prefer `tui-development`

# Procedure

1. **Choose framework** — use Typer for new projects (type-hint based, auto-generates help); use Click for complex option inheritance or plugins.
2. **Define app and commands** — create `app = typer.Typer()` and decorate functions with `@app.command()`.
3. **Add typed arguments** — use `Annotated[str, typer.Argument(help="...")]` for positional args, `typer.Option` for flags.
4. **Implement callbacks** — use `@app.callback()` for shared state (verbosity, config path) passed via `typer.Context`.
5. **Format output** — use `rich.console.Console` for tables/panels, `json.dumps(indent=2)` for `--json`, plain text for `--quiet`.
6. **Handle errors** — catch expected exceptions, print with `rich.console.print("[red]Error:[/red] ...")`, raise `typer.Exit(code=1)`.
7. **Add completions** — Typer auto-generates shell completions; run `myapp --install-completion`.
8. **Package** — add `[project.scripts]` in `pyproject.toml`: `myapp = "myapp.cli:app"`. Install with `pip install -e .`.

# Key patterns

```python
import typer
from typing import Annotated
from rich.console import Console

app = typer.Typer(help="My CLI tool")
console = Console()

@app.callback()
def main(ctx: typer.Context,
         verbose: Annotated[bool, typer.Option("--verbose", "-v")] = False):
    ctx.ensure_object(dict)
    ctx.obj["verbose"] = verbose

@app.command()
def greet(
    name: Annotated[str, typer.Argument(help="Name to greet")],
    count: Annotated[int, typer.Option(help="Repetitions")] = 1,
    json_out: Annotated[bool, typer.Option("--json")] = False,
):
    if json_out:
        import json
        print(json.dumps({"name": name, "count": count}))
    else:
        for _ in range(count):
            console.print(f"[green]Hello[/green], {name}!")
```

# Decision rules

- Prefer Typer over Click for new projects — less boilerplate, better type safety.
- Use `Annotated` types (Typer >= 0.9) instead of default value syntax.
- Never call `sys.exit()` directly — use `raise typer.Exit(code=N)`.
- Test CLI commands with `typer.testing.CliRunner` or Click's `CliRunner`.
- Pin CLI deps in `pyproject.toml` with `>=` lower bounds, not exact pins.

# References

- https://typer.tiangolo.com/
- https://click.palletsprojects.com/
- https://rich.readthedocs.io/

# Related skills

- `cli-development-go` — Go CLI patterns
- `bash` — shell scripting
- `tui-development` — interactive terminal UIs
