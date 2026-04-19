---
name: tui-development
description: >-
  Build terminal user interfaces across languages using TUI frameworks.
  Use when building a TUI in Python (Textual/Rich), Rust (Ratatui), or
  evaluating TUI frameworks for a project. Do not use for Go TUIs (prefer
  bubbletea-go) or simple CLI flag parsing (prefer cli-development-go or
  cli-development-python).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: tui-development
  maturity: draft
  risk: low
  tags: [tui, terminal, ui]
---

# Purpose

Build terminal UIs using Textual (Python), Ratatui (Rust), or other TUI frameworks with proper event loops and rendering.

# When to use this skill

- building a TUI app in Python with Textual or Rich
- building a TUI app in Rust with Ratatui or Crossterm
- choosing between TUI frameworks for a new project
- implementing common TUI patterns (lists, tables, forms, split panes)

# Do not use this skill when

- building a Go TUI — prefer `bubbletea-go`
- building a non-interactive CLI — prefer `cli-development-go` or `cli-development-python`
- debugging other processes in terminal — prefer `terminal-debugging`

# Procedure

1. **Choose framework** — Python: Textual (async, CSS styling). Rust: Ratatui (immediate-mode). Go: Bubble Tea.
2. **Event loop** — Textual: subclass `App`, define `compose()` and handlers. Ratatui: `loop { draw(); handle_event(); }`.
3. **Layout** — Textual: CSS grid/dock. Ratatui: `Layout::default().constraints()`.
4. **Widgets** — use library widgets first (DataTable, ListView). Custom: implement `Widget` trait or `render()`.
5. **Input** — Textual: `BINDINGS` class var. Ratatui: match `crossterm::event::KeyCode`.
6. **State** — central struct/model. Update in handlers, render from state.
7. **Styling** — Textual: CSS files. Ratatui: `Style::default().fg(Color::Green)`.
8. **Test** — Textual: `async with app.run_test() as pilot`. Ratatui: test state separately.

# Framework comparison

| Feature | Textual (Python) | Ratatui (Rust) | Bubble Tea (Go) |
|---------|------------------|----------------|-----------------|
| Architecture | Async reactive | Immediate mode | Elm (MVU) |
| Styling | CSS-like | Inline Style | Lip Gloss |
| Widgets | Rich built-in | Basic, composable | Bubbles library |
| Best for | Dashboards | Performance-critical | Go CLI tools |

# Textual quick start

```python
from textual.app import App, ComposeResult
from textual.widgets import Header, Footer, DataTable

class MyApp(App):
    BINDINGS = [("q", "quit", "Quit")]

    def compose(self) -> ComposeResult:
        yield Header()
        yield DataTable()
        yield Footer()

    def on_mount(self) -> None:
        table = self.query_one(DataTable)
        table.add_columns("Name", "Status", "Count")
        table.add_rows([("alpha", "ok", "42")])
```

# Decision rules

- Textual for Python dashboards — best widget library and CSS styling.
- Ratatui for Rust apps needing max performance or minimal binary.
- Bubble Tea for Go CLI tools with interactive selection.
- Always implement quit keybinding (q or Ctrl+C).
- Separate state from rendering — test transitions without a terminal.

# References

- https://textual.textualize.io/
- https://ratatui.rs/
- https://github.com/charmbracelet/bubbletea

# Related skills

- `bubbletea-go` — Go-specific TUI patterns
- `cli-development-python` — non-interactive Python CLI
- `cli-development-go` — non-interactive Go CLI
