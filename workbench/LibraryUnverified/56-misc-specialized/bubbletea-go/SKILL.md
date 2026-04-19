---
name: bubbletea-go
description: >-
  Build terminal UIs in Go with Bubble Tea's Elm-architecture pattern.
  Use when creating interactive Go TUI apps, implementing Model-Update-View
  loops, composing Bubble Tea components, or integrating Lip Gloss styling.
  Do not use for non-Go TUIs (prefer tui-development) or simple CLI flag
  parsing (prefer cobra-go).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: bubbletea-go
  maturity: draft
  risk: low
  tags: [go, tui, bubbletea]
---

# Purpose

Build interactive terminal UIs in Go using Bubble Tea's Elm-architecture (Model / Update / View) with Lip Gloss styling.

# When to use this skill

- building an interactive TUI application in Go with Bubble Tea
- composing multiple Bubble Tea components (lists, text inputs, tables)
- adding Lip Gloss styles, borders, or layout to a TUI
- converting a Cobra CLI to have interactive selection or forms

# Do not use this skill when

- building a TUI in Python or Rust — prefer `tui-development`
- only need CLI flag/arg parsing — prefer `cobra-go`
- building a non-interactive CLI tool — prefer `cli-development-go`

# Procedure

1. **Define the Model struct** — store all UI state: cursor position, items, selected values, dimensions, error state.
2. **Implement `Init()`** — return initial commands (e.g., `tea.EnterAltScreen`, data fetch commands).
3. **Implement `Update(msg)`** — handle `tea.KeyMsg`, `tea.WindowSizeMsg`, and custom `tea.Msg` types. Return `(model, cmd)`.
4. **Implement `View()`** — render the entire screen as a string using Lip Gloss. Never do I/O in View.
5. **Compose sub-models** — embed child models (e.g., `list.Model`, `textinput.Model`). Delegate messages and render in parent View.
6. **Add Lip Gloss styling** — define styles as package-level vars. Use `lipgloss.NewStyle().Foreground()`, `.Border()`, `.Padding()`.
7. **Handle window resize** — listen for `tea.WindowSizeMsg`, store width/height, pass to child models.
8. **Test with `teatest`** — use `teatest.NewModel()` to send key sequences and assert final view output.

# Key patterns

```go
type model struct {
    choices  []string
    cursor   int
    selected map[int]struct{}
    width, height int
}

func (m model) Init() tea.Cmd { return nil }

func (m model) Update(msg tea.Msg) (tea.Model, tea.Cmd) {
    switch msg := msg.(type) {
    case tea.KeyMsg:
        switch msg.String() {
        case "q", "ctrl+c":
            return m, tea.Quit
        case "up", "k":
            if m.cursor > 0 { m.cursor-- }
        case "down", "j":
            if m.cursor < len(m.choices)-1 { m.cursor++ }
        case "enter", " ":
            if _, ok := m.selected[m.cursor]; ok {
                delete(m.selected, m.cursor)
            } else {
                m.selected[m.cursor] = struct{}{}
            }
        }
    case tea.WindowSizeMsg:
        m.width, m.height = msg.Width, msg.Height
    }
    return m, nil
}
```

# Decision rules

- Never perform I/O in `View()` — it is called on every render cycle.
- Use `tea.Cmd` for async work (HTTP, file reads) — return commands from `Update`.
- Store all state in the Model — no global mutable state.
- Use `tea.Batch()` to combine multiple commands from a single Update.
- Prefer Bubbles library components (`list`, `table`, `textinput`) over hand-rolling.

# References

- https://github.com/charmbracelet/bubbletea
- https://github.com/charmbracelet/bubbles
- https://github.com/charmbracelet/lipgloss

# Related skills

- `cobra-go` — CLI argument parsing before TUI launch
- `tui-development` — cross-language TUI patterns
- `cli-development-go` — non-interactive Go CLIs
