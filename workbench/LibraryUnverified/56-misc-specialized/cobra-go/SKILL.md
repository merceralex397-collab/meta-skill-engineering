---
name: cobra-go
description: >-
  Write Cobra command definitions, persistent flags, argument validators, and
  completion functions. Use when adding or refactoring Cobra subcommands,
  wiring PersistentPreRunE chains, defining custom ValidArgsFunction, or
  fixing command hierarchy issues. Do not use for full CLI project setup
  (prefer cli-development-go) or TUI work (prefer bubbletea-go).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cobra-go
  maturity: draft
  risk: low
  tags: [go, cobra, cli]
---

# Purpose

Write correct Cobra command definitions with proper flag binding, argument validation, and shell completions.

# When to use this skill

- adding a new subcommand to an existing Cobra CLI
- fixing flag binding, `PersistentPreRunE` chains, or arg validators
- implementing dynamic shell completion (`ValidArgsFunction`)
- restructuring a command hierarchy (grouping, aliases, hidden commands)

# Do not use this skill when

- scaffolding a full CLI project with config/output layers — prefer `cli-development-go`
- building interactive TUI — prefer `bubbletea-go`
- the project uses a different CLI framework (urfave/cli, kong)

# Procedure

1. **Create command file** — one file per command in `cmd/`. Define `var fooCmd = &cobra.Command{...}`.
2. **Set `Use` correctly** — format: `"verb [flags] <required> [optional]"`. Cobra parses this for help text.
3. **Choose `RunE` over `Run`** — always return `error` so Cobra can exit non-zero on failure.
4. **Add args validation** — use `Args: cobra.ExactArgs(1)` or `cobra.MinimumNArgs(1)`. Custom validators: `Args: func(cmd, args) error`.
5. **Register flags in `init()`** — use `Flags()` for local, `PersistentFlags()` for inherited. Bind to Viper if present.
6. **Chain PreRunE** — use `PersistentPreRunE` on parent for shared setup. Call parent's PreRunE from child if overriding.
7. **Add completions** — implement `ValidArgsFunction` returning `([]string, cobra.ShellCompDirective)`.
8. **Register in parent** — call `parentCmd.AddCommand(fooCmd)` in `init()`.

# Key patterns

```go
var deployCmd = &cobra.Command{
    Use:   "deploy <environment>",
    Short: "Deploy to target environment",
    Args:  cobra.ExactArgs(1),
    ValidArgsFunction: func(cmd *cobra.Command, args []string, toComplete string) ([]string, cobra.ShellCompDirective) {
        return []string{"staging", "production"}, cobra.ShellCompDirectiveNoFileComp
    },
    RunE: func(cmd *cobra.Command, args []string) error {
        env := args[0]
        dryRun, _ := cmd.Flags().GetBool("dry-run")
        return runDeploy(cmd.Context(), env, dryRun)
    },
}

func init() {
    deployCmd.Flags().Bool("dry-run", false, "Preview without applying")
    rootCmd.AddCommand(deployCmd)
}
```

**PersistentPreRunE chain** (child calling parent):
```go
child.PersistentPreRunE = func(cmd *cobra.Command, args []string) error {
    if parent.PersistentPreRunE != nil {
        if err := parent.PersistentPreRunE(cmd, args); err != nil {
            return err
        }
    }
    return childSetup(cmd)
}
```

# Decision rules

- One command per file — keeps `cmd/` navigable.
- Always use `RunE` — bare `Run` swallows errors.
- Never call `os.Exit()` inside a command — return errors and let `Execute()` handle exit codes.
- Use `cmd.Context()` to pass cancellation and deadlines.
- Mark internal commands with `Hidden: true` rather than removing them.

# References

- https://cobra.dev/
- https://pkg.go.dev/github.com/spf13/cobra

# Related skills

- `cli-development-go` — full CLI project setup
- `bubbletea-go` — interactive TUI after CLI parsing
- `release-binaries` — building and distributing the binary
