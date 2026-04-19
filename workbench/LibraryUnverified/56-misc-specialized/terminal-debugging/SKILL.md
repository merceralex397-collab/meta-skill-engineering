---
name: terminal-debugging
description: >-
  Debug processes with strace, ltrace, gdb, and /proc inspection. Use when
  tracing syscalls, debugging a hanging process, inspecting /proc for fd
  leaks, or attaching gdb to a running process. Do not use for app-level
  logging (prefer observability tools) or network capture (tcpdump/wireshark).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: terminal-debugging
  maturity: draft
  risk: low
  tags: [debugging, strace, gdb, terminal]
---

# Purpose

Debug running processes from the terminal using strace, ltrace, gdb, and /proc filesystem inspection.

# When to use this skill

- tracing syscalls to find why a process hangs or crashes
- inspecting file descriptor leaks via `/proc/<pid>/fd`
- attaching `gdb` to a running process for breakpoint debugging
- profiling dynamic library calls with `ltrace`

# Do not use this skill when

- debugging application logic with IDE debuggers or print statements
- capturing network traffic — use `tcpdump` or `wireshark`
- the issue is a shell scripting bug — prefer `bash`

# Procedure

1. **Find process** — `pgrep -a <name>` or `ps aux | grep <name>` to get PID.
2. **Trace syscalls** — `strace -p <pid> -e trace=open,read,write -f -t`.
3. **Trace from start** — `strace -f -o /tmp/trace.log ./myapp` with child processes.
4. **Check open files** — `ls -la /proc/<pid>/fd`; `lsof -p <pid>` for details.
5. **Inspect memory** — `cat /proc/<pid>/maps`; `grep VmRSS /proc/<pid>/status`.
6. **Attach GDB** — `gdb -p <pid>`, then `bt`, `info threads`, `continue`.
7. **Library calls** — `ltrace -p <pid> -e malloc+free` to track allocations.
8. **Signals** — `grep Sig /proc/<pid>/status` for pending/blocked/ignored.

# Common patterns

```bash
# Why is it stuck?
strace -p $(pgrep myapp) -e trace=network,file -f -t 2>&1 | head -100

# FD leak detection
ls /proc/$(pgrep myapp)/fd | wc -l

# CPU profile
perf record -g -p $(pgrep myapp) -- sleep 30
perf report --stdio | head -50

# Core dump
coredumpctl list
coredumpctl gdb <pid>
```

# Decision rules

- `strace -f` to follow child processes — many issues are in forked workers.
- Filter with `-e trace=file,network,process,signal` to reduce noise.
- `strace -c` for syscall summary statistics instead of full trace.
- `ulimit -c unlimited` before running to enable core dumps.
- `gdb -batch -ex bt -p <pid>` for non-interactive backtrace in scripts.

# References

- https://man7.org/linux/man-pages/man1/strace.1.html
- https://sourceware.org/gdb/current/onlinedocs/gdb.html

# Related skills

- `linux-ubuntu-ops` — system-level diagnostics
- `bash` — scripting debug workflows
- `ssh-tmux-remote-workflow` — debugging on remote servers
