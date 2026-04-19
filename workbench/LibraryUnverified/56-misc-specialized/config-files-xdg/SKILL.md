---
name: config-files-xdg
description: >-
  Place config, data, cache, and state files in XDG-compliant paths with
  proper defaults. Use when choosing where to store config/data/cache files,
  implementing XDG Base Directory lookup, migrating from dotfiles to XDG
  paths, or adding --config flag overrides. Do not use for systemd unit
  config (prefer systemd-services) or build tool config that has ecosystem
  conventions.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: config-files-xdg
  maturity: draft
  risk: low
  tags: [config, xdg, files]
---

# Purpose

Place application config, data, cache, and state files in XDG Base Directory compliant paths with correct fallbacks.

# When to use this skill

- deciding where a CLI tool should read/write config, data, or cache
- implementing XDG Base Directory resolution with platform fallbacks
- migrating a tool from `~/.myapp` to XDG-compliant paths
- adding `--config` flag that overrides default config path

# Do not use this skill when

- writing systemd unit files — prefer `systemd-services`
- working with language-specific config (`.eslintrc`, `pyproject.toml`) — those follow ecosystem conventions
- the app is server-only with env-var-based config

# Procedure

1. **Classify files** — config (user-editable settings), data (persistent app state), cache (regenerable), runtime (sockets, locks).
2. **Resolve XDG paths** — read `$XDG_CONFIG_HOME` (default `~/.config`), `$XDG_DATA_HOME` (default `~/.local/share`), `$XDG_CACHE_HOME` (default `~/.cache`), `$XDG_STATE_HOME` (default `~/.local/state`).
3. **Create app subdirectory** — use `$XDG_CONFIG_HOME/myapp/`, not `$XDG_CONFIG_HOME/myapp.conf`. Create dirs with `0700` permissions.
4. **Support overrides** — accept `--config` flag and `MYAPP_CONFIG` env var. Priority: flag > env > XDG path > default.
5. **Handle platform differences** — on macOS use `~/Library/Application Support` if XDG vars are unset; on Windows use `%APPDATA%`.
6. **Add migration** — if `~/.myapp` exists and XDG path does not, offer to migrate. Print the action taken.

# XDG directory mapping

| Category | Env var | Default | Example |
|----------|---------|---------|---------|
| Config | `$XDG_CONFIG_HOME` | `~/.config` | `~/.config/myapp/config.toml` |
| Data | `$XDG_DATA_HOME` | `~/.local/share` | `~/.local/share/myapp/db.sqlite` |
| Cache | `$XDG_CACHE_HOME` | `~/.cache` | `~/.cache/myapp/http-cache/` |
| State | `$XDG_STATE_HOME` | `~/.local/state` | `~/.local/state/myapp/logs/` |
| Runtime | `$XDG_RUNTIME_DIR` | `/run/user/$UID` | `/run/user/1000/myapp.sock` |

# Key patterns

```go
func configDir() string {
    if v := os.Getenv("XDG_CONFIG_HOME"); v != "" {
        return filepath.Join(v, "myapp")
    }
    home, _ := os.UserHomeDir()
    return filepath.Join(home, ".config", "myapp")
}
```

```python
from pathlib import Path
import os

def config_dir() -> Path:
    xdg = os.environ.get("XDG_CONFIG_HOME")
    base = Path(xdg) if xdg else Path.home() / ".config"
    d = base / "myapp"
    d.mkdir(parents=True, exist_ok=True)
    return d
```

# Decision rules

- Config files are user-editable (TOML/YAML). Data files are app-managed (SQLite, JSON state).
- Cache must be safe to delete without data loss — the app must regenerate it.
- Never write to `$XDG_CONFIG_HOME` at runtime — config is read-only; use state or data for writes.
- Use `0700` permissions for directories, `0600` for files containing secrets.
- Support `$XDG_CONFIG_DIRS` (colon-separated search path) for system-level defaults.

# References

- https://specifications.freedesktop.org/basedir-spec/latest/

# Related skills

- `bash` — resolving paths in shell scripts
- `cli-development-go` — integrating XDG with Viper config
- `cli-development-python` — integrating XDG with Click/Typer
