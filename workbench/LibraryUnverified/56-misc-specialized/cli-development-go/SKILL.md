---
name: cli-development-go
description: >-
  Build Go CLI tools with Cobra commands, Viper config, and structured output.
  Use when creating a new Go CLI binary, adding subcommands with Cobra,
  loading config with Viper, or implementing --json/--quiet output modes.
  Do not use for interactive TUIs (prefer bubbletea-go) or Python CLIs
  (prefer cli-development-python).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cli-development-go
  maturity: draft
  risk: low
  tags: [go, cli, cobra, viper]
---

# Purpose

Build robust Go CLI tools using Cobra for command structure, Viper for config, and idiomatic Go patterns for I/O and error handling.

# When to use this skill

- creating a new Go CLI binary with subcommands
- wiring Cobra commands with Viper config file and env var binding
- adding `--output json|table|text` formatting
- structuring a multi-command Go CLI project

# Do not use this skill when

- building an interactive terminal UI — prefer `bubbletea-go`
- building a Python CLI — prefer `cli-development-python`
- only writing shell scripts — prefer `bash`

# Procedure

1. **Scaffold with Cobra-CLI** — run `cobra-cli init` and `cobra-cli add <cmd>` to generate command files.
2. **Define root command** — set `PersistentPreRunE` on root for global setup (config loading, logger init).
3. **Bind flags to Viper** — call `viper.BindPFlag("key", cmd.Flags().Lookup("flag"))` in `init()`.
4. **Load config** — use `viper.SetConfigName(".myapp")`, `viper.AddConfigPath("$HOME")`, `viper.AutomaticEnv()`.
5. **Structure output** — accept `--output` flag; use `encoding/json` for JSON, `text/tabwriter` for tables.
6. **Handle errors idiomatically** — return `error` from `RunE`, let Cobra print usage on bad args. Use `fmt.Errorf("verb: %w", err)`.
7. **Add completions** — Cobra auto-generates `completion` subcommand; add custom `ValidArgsFunction` for dynamic completions.
8. **Build and test** — `go build -ldflags "-X main.version=$(git describe)"` for version embedding; test with `cobra.Command.ExecuteC()`.

# Project layout

```
myapp/
  cmd/
    root.go          # root command + global flags
    serve.go         # subcommand
    migrate.go       # subcommand
  internal/
    config/config.go # Viper wrapper
    output/output.go # JSON/table formatter
  main.go            # cmd.Execute()
  .myapp.yaml        # default config
```

# Key patterns

```go
var rootCmd = &cobra.Command{
    Use:   "myapp",
    Short: "Does the thing",
    PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
        viper.SetConfigName(".myapp")
        viper.AddConfigPath(".")
        viper.AutomaticEnv()
        viper.SetEnvPrefix("MYAPP")
        _ = viper.ReadInConfig()
        return nil
    },
}

func init() {
    rootCmd.PersistentFlags().StringP("output", "o", "text", "Output format: text|json|table")
    viper.BindPFlag("output", rootCmd.PersistentFlags().Lookup("output"))
}
```

# Decision rules

- Use `RunE` (not `Run`) so errors propagate to `os.Exit(1)` via Cobra.
- Bind every flag to Viper so env vars and config files work as overrides.
- Write to `cmd.OutOrStdout()` instead of `os.Stdout` for testability.
- Validate args in `Args:` field (e.g., `cobra.ExactArgs(1)`) not inside `RunE`.
- Prefer `--output` flag over multiple commands (`list-json`, `list-table`).

# References

- https://cobra.dev/
- https://github.com/spf13/viper

# Related skills

- `cobra-go` — focused Cobra command patterns
- `bubbletea-go` — interactive TUI on top of CLI
- `release-binaries` — cross-compiling and distributing the binary
