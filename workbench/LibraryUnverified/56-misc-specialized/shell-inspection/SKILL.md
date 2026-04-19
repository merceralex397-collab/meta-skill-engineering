---
name: shell-inspection
description: >
  Read-only shell diagnostics to verify environment state, tool versions, ports, processes, and system resources.
  Trigger — "check environment", "verify tool versions", "what's running on port",
  "inspect system state", "check disk space", "diagnose environment setup".
  Skip — task requires installing software, modifying configs, or writing code.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: shell-inspection
  maturity: draft
  risk: low
  tags: [shell, inspection]
---

# Purpose

Run read-only shell commands to diagnose and document the current system state — tool
versions, environment variables, running processes, open ports, file permissions, disk
and memory usage, and network connectivity. Produce a structured report that downstream
agents can use to validate prerequisites or troubleshoot failures.

# When to use

- A build or test failure may be caused by environment misconfiguration.
- A new developer needs to verify their local setup matches project requirements.
- An agent needs to confirm runtime prerequisites before executing a plan.
- Debugging a "works on my machine" discrepancy between environments.
- Pre-deployment verification of server state.

# Do NOT use when

- The task requires installing packages or modifying system configuration — hand off to a setup skill.
- The investigation is purely about source code, not the runtime environment — use `repo-evidence-gathering`.
- The user needs a security audit — use `security-review` for that scope.
- The system is remote and requires SSH — this skill operates on the local shell only.

# Operating procedure

1. Run `uname -a` and `cat /etc/os-release 2>/dev/null || sw_vers 2>/dev/null` to identify the OS and kernel version.
2. Check required tool versions by running `node --version`, `python3 --version`, `go version`, `rustc --version`, `java -version`, `docker --version`, `git --version` — skip tools that return "command not found" and note them as missing.
3. Run `env | grep -iE 'PATH|HOME|NODE_ENV|PYTHON|RUST|GOPATH|JAVA_HOME|DATABASE_URL|PORT|API' | sort` to capture relevant environment variables (redact any values containing "secret", "key", "token", or "password").
4. Check running processes with `ps aux --no-headers | grep -iE 'node|python|java|docker|postgres|redis|nginx|mysql' | head -20` to identify active services.
5. Inspect open ports with `ss -tlnp 2>/dev/null || netstat -tlnp 2>/dev/null` and list listening ports with their associated PIDs.
6. Check disk usage with `df -h /` and memory with `free -h 2>/dev/null || vm_stat 2>/dev/null` — flag if disk is above 90% or available memory is below 500MB.
7. Verify file permissions on key project files: run `ls -la` on the repo root, any `.env` files, and config directories — flag world-readable secrets files.
8. Test network connectivity by running `curl -sS -o /dev/null -w '%{http_code}' https://registry.npmjs.org/ 2>/dev/null` (or equivalent) for package registries the project depends on.
9. Check for Docker state if applicable: run `docker ps --format 'table {{.Names}}	{{.Status}}	{{.Ports}}'` and `docker compose ps 2>/dev/null`.
10. Compile all results into the output format below, marking each check as PASS, WARN, or FAIL.

# Decision rules

- Never run commands that modify state — no `rm`, `mv`, `install`, `write`, or `stop`.
- Redact any environment variable value that looks like a credential (contains key, secret, token, password).
- If a command times out after 10 seconds, mark it as TIMEOUT and move on.
- Report exact version strings, not just "installed" — downstream agents need precise versions.
- When a check fails, include the raw command output so the user can diagnose further.

# Output requirements

1. **System Info** — OS, kernel, architecture.
2. **Tool Versions Table** — `| Tool | Version | Status (OK/Missing/Wrong) |`
3. **Environment Variables** — relevant vars with redacted secrets.
4. **Running Services** — active processes relevant to the project.
5. **Port Map** — `| Port | PID | Process | Protocol |`
6. **Resource Status** — disk and memory usage with PASS/WARN/FAIL.
7. **Permission Flags** — any files with insecure permissions.
8. **Network Connectivity** — registry reachability results.
9. **Overall Readiness** — READY / NOT READY with list of blockers.

# References

- Project README or AGENTS.md for expected tool versions
- `package.json` engines field, `.tool-versions`, `.nvmrc`, `rust-toolchain.toml`
- Docker Compose files for expected service dependencies

# Related skills

- `repo-evidence-gathering` — complements shell inspection with source-level evidence
- `security-review` — uses permission and environment findings for security assessment
- `ticket-creator` — converts environment issues into setup tickets
- `api-debugging` — may follow shell-inspection when debugging service connectivity

# Failure handling

- If a command is not available on the current OS, skip it and note the OS limitation.
- If permission is denied for a check, log the command and error, mark as DENIED, and continue.
- If the environment has no project context (empty directory), report "no project detected" and list only system-level findings.
- If more than 5 checks return FAIL, add a prominent banner: "Environment has critical issues — resolve before proceeding."
