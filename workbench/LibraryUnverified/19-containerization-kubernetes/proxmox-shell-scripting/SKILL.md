---
name: proxmox-shell-scripting
description: >-
  Automate Proxmox VE management via pvesh, qm, pct, and vzdump CLI tools.
  Use when scripting VM/container creation, managing storage pools, automating
  backups with vzdump, or querying cluster status via pvesh. Do not use for
  generic Linux admin (prefer linux-ubuntu-ops) or systemd service config
  (prefer systemd-services).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: proxmox-shell-scripting
  maturity: draft
  risk: low
  tags: [proxmox, virtualization, shell]
---

# Purpose

Automate Proxmox VE operations using `pvesh`, `qm`, `pct`, and `vzdump` shell commands.

# When to use this skill

- scripting VM creation/cloning with `qm`
- managing LXC containers with `pct`
- automating backups and restores with `vzdump`/`qmrestore`
- querying cluster status and storage via `pvesh`

# Do not use this skill when

- performing generic Ubuntu/Debian admin — prefer `linux-ubuntu-ops`
- writing systemd units for services — prefer `systemd-services`
- managing Docker containers — different tooling

# Procedure

1. **Query cluster** — `pvesh get /cluster/status --output-format json` to check node health.
2. **Create VM** — `qm create <vmid> --name <n> --memory 2048 --cores 2 --net0 virtio,bridge=vmbr0 --scsi0 local-lvm:32`.
3. **Create LXC** — `pct create <ctid> local:vztmpl/<tpl>.tar.zst --hostname <n> --memory 512 --rootfs local-lvm:8`.
4. **Cloud-init** — `qm set <vmid> --cicustom "user=local:snippets/user-data.yml" --ipconfig0 ip=dhcp`.
5. **Backup** — `vzdump <vmid> --storage backup-store --mode snapshot --compress zstd`.
6. **Restore** — `qmrestore /var/lib/vz/dump/vzdump-qemu-<vmid>-*.vma.zst <new-vmid> --storage local-lvm`.
7. **Manage storage** — `pvesm status` to list pools; `pvesm alloc local-lvm <vmid> vm-<vmid>-data 50G`.
8. **Bulk ops** — loop over `pvesh get /cluster/resources --type vm --output-format json | jq -r '.[].vmid'`.

# Key patterns

```bash
#!/usr/bin/env bash
set -euo pipefail

clone_vm() {
  local template_id="$1" new_id="$2" name="$3"
  qm clone "$template_id" "$new_id" --name "$name" --full
  qm set "$new_id" --memory 4096 --cores 4
  qm start "$new_id"
}

backup_all_running() {
  local vmids
  vmids=$(qm list | awk 'NR>1 && $3=="running" {print $1}')
  for vmid in $vmids; do
    vzdump "$vmid" --storage backup --mode snapshot --compress zstd
  done
}
```

# Decision rules

- Always use `--output-format json` with `pvesh` — parse with `jq`.
- Use snapshot mode for backups of running VMs — stop mode causes downtime.
- Prefer `qm clone --full` over manual disk copy for reproducibility.
- Store cloud-init snippets in `/var/lib/vz/snippets/`.
- Test destructive operations on a non-production node first.

# References

- https://pve.proxmox.com/pve-docs/
- https://pve.proxmox.com/pve-docs/qm.1.html

# Related skills

- `linux-ubuntu-ops` — underlying OS administration
- `bash` — shell scripting fundamentals
- `ssh-tmux-remote-workflow` — remote Proxmox management
