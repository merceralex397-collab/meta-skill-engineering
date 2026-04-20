---
name: self-hosting-ops
description: "Provision and operate self-hosted infrastructure — configure VPS instances, write systemd service units, set up Nginx or Caddy reverse proxies, automate backups, configure firewall rules, and wire monitoring with Prometheus or similar. Use when deploying to self-managed servers, VPS providers, or bare metal instead of managed cloud services. Do not use for managed cloud platform deployments (prefer aws, gcp, vercel skills)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: self-hosting-ops
  maturity: draft
  risk: low
  tags: [self-hosting, vps, systemd, nginx, caddy, backups]
---

# Purpose

Provision and operate self-hosted infrastructure on bare metal or VPS — write systemd service units, configure Nginx or Caddy reverse proxies, automate backups and restore procedures, harden firewall rules, and wire monitoring and alerting with Prometheus, Grafana, or similar tooling.

# When to use this skill

- Provisioning a new VPS instance (Hetzner, DigitalOcean, Linode, OVH, or similar)
- Writing or editing systemd unit files for application services
- Configuring Nginx or Caddy as a reverse proxy with TLS termination
- Setting up automated backup schedules (database dumps, filesystem snapshots, off-site sync)
- Writing firewall rules with `ufw`, `iptables`, or `nftables`
- Configuring Prometheus, node_exporter, and Grafana for server monitoring
- Performing OS-level hardening (SSH config, fail2ban, unattended upgrades)
- Deploying applications via `rsync`, `git pull`, or CI/CD push to a self-managed server

# Do not use this skill when

- The target is a managed cloud platform — use `aws`, `gcp`, or `vercel` skills instead
- The task is container orchestration on Kubernetes — use a Kubernetes-specific skill
- The task is Tailscale networking overlay configuration — use `tailscale-private-networking`
- The task is writing Terraform to provision cloud resources — use `terraform-iac`
- The task is application code changes with no infrastructure impact

# Operating procedure

1. **Inventory the target server.** SSH into the server and record: OS version (`cat /etc/os-release`), available RAM/disk (`free -h`, `df -h`), running services (`systemctl list-units --type=service --state=running`), and open ports (`ss -tlnp`).
2. **Harden SSH access.** Edit `/etc/ssh/sshd_config`: disable `PasswordAuthentication`, set `PermitRootLogin no`, restrict to key-based auth. Restart `sshd`. Verify you can still connect before closing the current session.
3. **Configure the firewall.** Enable `ufw` (or `nftables`). Allow only required ports: 22/tcp (SSH), 80/tcp, 443/tcp, and any application-specific ports. Deny all other inbound traffic. Run `ufw status verbose` to confirm.
4. **Install runtime dependencies.** Install the application runtime (Node.js, Python, Go binary, etc.) using the OS package manager or version manager. Pin the version explicitly — do not use `latest`.
5. **Write the systemd service unit.** Create `/etc/systemd/system/{service-name}.service` with: `ExecStart` pointing to the application binary, `User` set to a non-root service account, `Restart=on-failure`, `RestartSec=5`, `WorkingDirectory`, and environment file reference (`EnvironmentFile=/etc/{service-name}/env`). Run `systemctl daemon-reload && systemctl enable --now {service-name}`.
6. **Configure the reverse proxy.** For Nginx: write a server block in `/etc/nginx/sites-available/{domain}` with `proxy_pass` to the local app port, enable TLS via Certbot (`certbot --nginx -d {domain}`), and `ln -s` to `sites-enabled`. For Caddy: write a Caddyfile block with the domain and `reverse_proxy localhost:{port}` — Caddy handles TLS automatically. Test config (`nginx -t` or `caddy validate`) and reload.
7. **Set up automated backups.** Write a backup script that dumps databases (`pg_dump`, `mysqldump`, or `sqlite3 .backup`), tars application data directories, and syncs to off-site storage (`rsync` to a backup server, `rclone` to S3/B2, or `restic` to a repo). Schedule via cron (`crontab -e`) or a systemd timer. Run a manual test backup and verify restore.
8. **Wire monitoring.** Install `node_exporter` and the application's metrics exporter. Configure Prometheus to scrape both. Set up Grafana dashboards for CPU, memory, disk, and application-specific metrics. Create alerting rules for: disk > 85%, memory > 90%, service restart count > 3 in 5 minutes, and TLS certificate expiry < 14 days.
9. **Configure unattended security updates.** Enable `unattended-upgrades` (Debian/Ubuntu) or `dnf-automatic` (Fedora/RHEL). Confirm only security patches are auto-applied. Schedule a reboot window if kernel updates require it.
10. **Validate the full deployment.** Curl the public endpoint and verify a 200 response with expected content. Check `journalctl -u {service-name} --since '5 min ago'` for errors. Confirm the backup cron or timer is scheduled (`systemctl list-timers` or `crontab -l`). Confirm Prometheus targets are UP in the Prometheus UI.
11. **Document the server runbook.** Record: server IP/hostname, SSH access instructions, service names, backup schedule and restore procedure, monitoring dashboard URL, and escalation contacts.

# Decision rules

- If the application needs zero-downtime deploys, use a blue-green strategy with two systemd units behind the reverse proxy — do not use in-place restart.
- If the server has < 1 GB RAM, skip Prometheus/Grafana on-box and push metrics to an external monitoring service.
- If the VPS provider offers automated snapshots, enable them as a supplement to application-level backups — but never as the sole backup.
- If TLS is required, prefer Caddy (automatic HTTPS) for simplicity. Use Nginx + Certbot when Nginx-specific features (complex rewrites, rate limiting) are needed.
- If multiple services share one server, isolate them with separate service accounts and systemd units — never run everything as root.
- Always test the backup restore procedure before considering backups "configured."

# Output requirements

1. **Server inventory** — OS, resources, IP, SSH access details
2. **systemd unit file(s)** — complete `.service` files ready to install
3. **Reverse proxy config** — Nginx server block or Caddyfile with TLS
4. **Backup configuration** — backup script, cron/timer schedule, and restore procedure
5. **Firewall rules** — `ufw` commands or `nftables` rules applied
6. **Monitoring setup** — Prometheus scrape config, alerting rules, and dashboard reference

# References

- systemd unit file documentation: https://www.freedesktop.org/software/systemd/man/systemd.service.html
- Nginx reverse proxy guide: https://nginx.org/en/docs/http/ngx_http_proxy_module.html
- Caddy reverse proxy: https://caddyserver.com/docs/caddyfile/directives/reverse_proxy
- Prometheus node_exporter: https://github.com/prometheus/node_exporter
- Restic backup tool: https://restic.readthedocs.io/
- UFW firewall: https://help.ubuntu.com/community/UFW

# Related skills

- `docker-containers` — for containerized deployments on self-hosted servers
- `terraform-iac` — for provisioning VPS instances via infrastructure-as-code
- `secret-management` — for injecting secrets into self-hosted services
- `tailscale-private-networking` — for private networking between self-hosted nodes

# Anti-patterns

- Running application processes as root instead of a dedicated service account
- Using `nohup` or `screen` instead of systemd for long-running services
- Skipping firewall configuration because "it's behind a NAT"
- Relying solely on VPS provider snapshots without application-level backups
- Editing Nginx config in `sites-enabled` directly instead of `sites-available` with symlinks
- Hardcoding server IPs in application config instead of using DNS or service discovery

# Failure handling

- If a systemd service fails to start, check `journalctl -u {service-name} -e` for the exact error. Fix the unit file or application config, then `systemctl daemon-reload && systemctl restart {service-name}`.
- If the reverse proxy returns 502, verify the upstream application is running and listening on the expected port. Check `ss -tlnp | grep {port}`.
- If backups fail silently, add error handling to the backup script (`set -euo pipefail`) and send a failure notification (email, webhook, or monitoring alert).
- If disk fills up, identify the largest directories with `du -sh /* | sort -rh | head`, clean log files or old backups, and add a disk-usage alert to prevent recurrence.
- If SSH access is lost after config changes, use the VPS provider's console/VNC access to recover.
