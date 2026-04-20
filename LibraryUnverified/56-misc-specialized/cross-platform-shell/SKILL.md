---
name: cross-platform-shell
description: >-
  Write shell scripts that work on Linux, macOS, and Windows (Git Bash/WSL).
  Use when a script must run on multiple OSes, replacing platform-specific
  commands with portable alternatives, or writing Makefile recipes that work
  cross-platform. Do not use for Bash-only scripts on Linux (prefer bash)
  or PowerShell-only automation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cross-platform-shell
  maturity: draft
  risk: low
  tags: [shell, cross-platform, portable]
---

# Purpose

Write shell scripts and Makefile recipes that execute correctly on Linux, macOS, and Windows (Git Bash / WSL / MSYS2).

# When to use this skill

- a shell script must run on both Linux and macOS (BSD vs GNU coreutils)
- writing Makefile recipes or CI scripts targeting multiple OS runners
- replacing `sed -i`, `grep -P`, `readlink -f` with portable alternatives
- a project has contributors on macOS, Linux, and Windows

# Do not use this skill when

- the script only targets Linux — prefer `bash`
- the task is Windows PowerShell automation — different domain
- building a compiled CLI tool — prefer `cli-development-go`

# Procedure

1. **Use POSIX sh when possible** — start with `#!/bin/sh` for max portability. Only use `#!/usr/bin/env bash` if arrays or `[[ ]]` are needed.
2. **Avoid GNU extensions** — use `sed 's/a/b/'` not `sed -i 's/a/b/'` (macOS `sed -i` requires `''`). Use `grep -E` not `grep -P`.
3. **Use portable path resolution** — replace `readlink -f` with `cd "$(dirname "$0")" && pwd -P`.
4. **Handle line endings** — ensure scripts use LF. Add `.gitattributes`: `*.sh text eol=lf`.
5. **Abstract OS differences** — detect OS with `uname -s` and branch: `case "$(uname -s)" in Darwin*) ... ;; Linux*) ... ;; MINGW*|MSYS*) ... ;; esac`.
6. **Use `env` for interpreters** — `#!/usr/bin/env python3` not `#!/usr/bin/python3`.
7. **Test in CI on all targets** — use GitHub Actions matrix with `ubuntu-latest`, `macos-latest`, `windows-latest`.

# Portability cheat sheet

| Task | GNU/Linux | macOS (BSD) | Portable |
|------|-----------|-------------|----------|
| In-place edit | `sed -i 's/a/b/'` | `sed -i '' 's/a/b/'` | Write to temp, `mv` |
| Regex grep | `grep -P` | N/A | `grep -E` |
| Resolve symlink | `readlink -f` | N/A | `cd && pwd -P` |
| Create temp file | `mktemp` | `mktemp` | `mktemp -t prefix.XXXXXX` |
| Date format | `date -d` | `date -j -f` | Use `date +%s` for epoch |

# Decision rules

- If you need `sed -i`, write to a temp file and `mv` it back — works everywhere.
- Prefer `printf` over `echo -e` — echo behavior varies across shells.
- Use `command -v` instead of `which` — POSIX compliant.
- Avoid associative arrays — they require Bash 4+ (macOS ships Bash 3).
- In Makefiles, set `SHELL := /bin/bash` only if Bash features are required.

# References

- https://pubs.opengroup.org/onlinepubs/9799919799/
- https://wiki.ubuntu.com/DashAsBinSh

# Related skills

- `bash` — Linux-focused shell scripting
- `linux-ubuntu-ops` — Linux-specific operations
- `cli-development-go` — compiled cross-platform binary (avoids shell entirely)
