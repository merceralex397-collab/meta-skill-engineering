---
name: linux-ubuntu-ops
description: >-
  Administer Ubuntu/Debian systems with apt, systemctl, journalctl, ufw, and
  user management. Use when managing packages with apt, troubleshooting
  services with journalctl, configuring UFW firewall rules, or managing
  users and permissions. Do not use for writing systemd unit files (prefer
  systemd-services) or packaging software for distribution (prefer
  packaging-installers).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: linux-ubuntu-ops
  maturity: draft
  risk: low
  tags: [linux, ubuntu, ops, admin]
---

# Purpose

Administer Ubuntu/Debian systems: package management, service control, log analysis, user management, and firewall configuration.

# When to use this skill

- installing, updating, or removing packages with `apt`
- diagnosing service failures with `journalctl` and `systemctl`
- configuring firewall rules with `ufw` or `iptables`
- managing users, groups, and file permissions

# Do not use this skill when

- writing systemd unit files from scratch — prefer `systemd-services`
- packaging software for distribution — prefer `packaging-installers`
- the task is shell scripting — prefer `bash`

# Procedure

1. **Update package index** — `sudo apt update` before installing. Use `apt list --upgradable` to review.
2. **Install packages** — `sudo apt install -y pkg1 pkg2`. Pin versions for servers: `sudo apt install nginx=1.24.0-1`.
3. **Diagnose service** — `systemctl status svc`, then `journalctl -u svc --since "10 min ago" --no-pager -n 50`.
4. **Check resources** — `df -h` (disk), `free -h` (memory), `ss -tlnp` (listening ports), `top -bn1 | head -20`.
5. **Manage users** — `sudo adduser --disabled-password deploy`, `sudo usermod -aG sudo deploy`.
6. **Configure firewall** — `sudo ufw allow 22/tcp`, `sudo ufw allow 443/tcp`, `sudo ufw enable`, `sudo ufw status verbose`.
7. **Set up unattended upgrades** — `sudo apt install unattended-upgrades`, `sudo dpkg-reconfigure -plow unattended-upgrades`.
8. **Clean up** — `sudo apt autoremove -y && sudo apt autoclean`.

# Common diagnostics

```bash
# Why did a service fail?
journalctl -u myservice --since "1 hour ago" -p err --no-pager

# What is using port 8080?
ss -tlnp | grep 8080

# Disk usage by directory
du -sh /var/log/* | sort -rh | head -10

# Failed login attempts
journalctl _COMM=sshd --since today | grep "Failed"

# Kernel messages
dmesg --level=err,warn | tail -20
```

# Decision rules

- Always run `apt update` before `apt install` — stale indexes cause version mismatches.
- Use `systemctl` not legacy `/etc/init.d/` scripts.
- Prefer `ufw` over raw `iptables` unless complex NAT rules are needed.
- Never run services as root — create a dedicated user or use `DynamicUser=yes` in systemd.
- Use `journalctl` over manual log file tailing — it handles rotation and filtering.

# References

- https://manpages.ubuntu.com/
- https://wiki.debian.org/UnattendedUpgrades

# Related skills

- `systemd-services` — writing unit files
- `ssh-tmux-remote-workflow` — remote server management
- `terminal-debugging` — process-level debugging
