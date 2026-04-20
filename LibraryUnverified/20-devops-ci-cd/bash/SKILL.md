---
name: bash
description: >-
  Write and fix Bash scripts with correct quoting, error handling, and
  ShellCheck compliance. Use when writing shell scripts, fixing shell quoting
  bugs, automating CLI tasks with Bash, or adding set -euo pipefail safety.
  Do not use for Python/Go CLI apps (prefer cli-development-python or
  cli-development-go) or for systemd unit files (prefer systemd-services).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: bash
  maturity: draft
  risk: low
  tags: [bash, shell, scripting]
---

# Purpose

Write portable, safe Bash scripts with proper quoting, error handling, and ShellCheck compliance.

# When to use this skill

- writing or editing `.sh` / `.bash` scripts
- automating file operations, build steps, or deploy tasks in shell
- debugging quoting errors, glob expansion, or word-splitting bugs
- adding safety headers (`set -euo pipefail`) to existing scripts

# Do not use this skill when

- building a Go CLI binary — prefer `cli-development-go`
- building a Python CLI tool — prefer `cli-development-python`
- writing systemd unit files — prefer `systemd-services`
- the script is a cross-platform `.sh`/`.ps1` pair — prefer `cross-platform-shell`

# Procedure

1. **Add safety header** — start every script with `#!/usr/bin/env bash` and `set -euo pipefail`.
2. **Quote all expansions** — wrap every `$variable` in double quotes: `"$var"`, `"${arr[@]}"`. Use `shellcheck` to catch misses.
3. **Use `[[ ]]` over `[ ]`** — double brackets prevent word-splitting and support regex. Never use unquoted `test`.
4. **Prefer `printf` over `echo`** — `echo` behavior varies across platforms. Use `printf '%s\n' "$msg"`.
5. **Handle errors explicitly** — use `trap cleanup EXIT` for temp file removal. Check command exit codes with `if ! cmd; then`.
6. **Use local variables in functions** — declare with `local` to avoid polluting the global scope.
7. **Validate inputs** — check `$#` for argument count, validate file existence with `[[ -f "$path" ]]`, reject empty strings.
8. **Run ShellCheck** — execute `shellcheck -o all script.sh` and fix every warning before committing.

# Key patterns

```bash
#!/usr/bin/env bash
set -euo pipefail

readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

cleanup() { rm -f "${tmp_file:-}"; }
trap cleanup EXIT

main() {
  local input="${1:?Usage: $0 <input-file>}"
  [[ -f "$input" ]] || { printf 'File not found: %s\n' "$input" >&2; return 1; }
  local tmp_file
  tmp_file="$(mktemp)"
  # process...
}

main "$@"
```

**Array iteration:**
```bash
files=("$SCRIPT_DIR"/*.txt)
for f in "${files[@]}"; do
  [[ -e "$f" ]] || continue
  process_file "$f"
done
```

# Decision rules

- Never use `eval` or unquoted `$()` in conditionals.
- Redirect stderr for user-facing messages: `echo "error" >&2`.
- Use `readonly` for constants, `local` for function-scoped variables.
- Prefer `command -v` over `which` for portability.
- If a script exceeds 200 lines, consider rewriting in Python or Go.

# References

- https://www.shellcheck.net/
- https://mywiki.wooledge.org/BashPitfalls
- https://google.github.io/styleguide/shellguide.html

# Related skills

- `cross-platform-shell` — multi-OS shell scripts
- `cli-development-python` — complex CLI tools
- `terminal-debugging` — debugging shell execution
- `linux-ubuntu-ops` — system administration tasks
