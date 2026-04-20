---
name: systemd-services
description: >-
  Write systemd unit files for services, timers, and socket activation with
  sandboxing. Use when creating a .service unit, setting up a systemd timer
  as cron replacement, configuring socket activation, or hardening with
  ProtectSystem/DynamicUser. Do not use for generic Linux admin (prefer
  linux-ubuntu-ops) or container process management.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: systemd-services
  maturity: draft
  risk: low
  tags: [systemd, services, linux]
---

# Purpose

Write correct systemd unit files for services, timers, and socket activation with sandboxing and restart policies.

# When to use this skill

- creating a `.service` unit for a daemon or long-running process
- replacing cron with a systemd `.timer` unit
- setting up socket activation for on-demand services
- hardening with `ProtectSystem`, `DynamicUser`, sandboxing directives

# Do not use this skill when

- doing general Linux admin — prefer `linux-ubuntu-ops`
- writing the application code itself — this is for the unit file
- managing Docker/Podman containers — they have their own process management

# Procedure

1. **Create unit file** — `/etc/systemd/system/myapp.service` (system) or `~/.config/systemd/user/` (user).
2. **Set ExecStart** — full paths: `ExecStart=/usr/local/bin/myapp --config /etc/myapp/config.toml`.
3. **Configure restart** — `Restart=on-failure`, `RestartSec=5`, `StartLimitBurst=3`.
4. **Add sandboxing** — `ProtectSystem=strict`, `ProtectHome=yes`, `NoNewPrivileges=yes`, `ReadWritePaths=/var/lib/myapp`.
5. **Set user** — `DynamicUser=yes` for stateless; `User=myapp` with dedicated account for stateful.
6. **Timer (optional)** — `myapp.timer` with `OnCalendar=*-*-* 02:00:00` for scheduled runs.
7. **Enable** — `sudo systemctl daemon-reload && sudo systemctl enable --now myapp.service`.
8. **Verify** — `systemctl status myapp`, `journalctl -u myapp -f --no-pager`.

# Service unit template

```ini
[Unit]
Description=MyApp service
After=network-online.target
Wants=network-online.target

[Service]
Type=notify
ExecStart=/usr/local/bin/myapp serve
Restart=on-failure
RestartSec=5
DynamicUser=yes
StateDirectory=myapp
ProtectSystem=strict
ProtectHome=yes
NoNewPrivileges=yes
PrivateTmp=yes

[Install]
WantedBy=multi-user.target
```

# Timer unit template

```ini
[Unit]
Description=Run myapp backup daily

[Timer]
OnCalendar=*-*-* 02:00:00
Persistent=true
RandomizedDelaySec=300

[Install]
WantedBy=timers.target
```

# Decision rules

- `Type=notify` if the service supports `sd_notify`; otherwise `Type=simple`.
- Always `Restart=on-failure` for production — never leave default `no`.
- `DynamicUser=yes` when no persistent UID is needed.
- `ProtectSystem=strict` makes `/` read-only — allowlist with `ReadWritePaths=`.
- `StateDirectory=myapp` auto-creates `/var/lib/myapp` with correct permissions.

# References

- https://www.freedesktop.org/software/systemd/man/latest/systemd.service.html
- https://www.freedesktop.org/software/systemd/man/latest/systemd.timer.html

# Related skills

- `linux-ubuntu-ops` — managing the service after deployment
- `bash` — ExecStartPre scripts
- `config-files-xdg` — application config file paths
