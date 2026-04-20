---
name: ssh-tmux-remote-workflow
description: >-
  Configure SSH connections, multiplexing, jump hosts, and tmux sessions for
  remote work. Use when setting up SSH config with ProxyJump, creating
  persistent tmux sessions, syncing files with rsync, or debugging SSH
  connection issues. Do not use for systemd services on the remote host
  (prefer systemd-services) or container orchestration.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ssh-tmux-remote-workflow
  maturity: draft
  risk: low
  tags: [ssh, tmux, remote]
---

# Purpose

Configure SSH connections, key management, jump hosts, and tmux sessions for reliable remote workflows.

# When to use this skill

- setting up `~/.ssh/config` with host aliases, ProxyJump, and multiplexing
- creating persistent tmux sessions for long-running remote work
- syncing files between local and remote with `rsync`
- debugging SSH connection failures or key issues

# Do not use this skill when

- writing systemd unit files on the remote host — prefer `systemd-services`
- the task is container orchestration — different domain
- doing local terminal debugging — prefer `terminal-debugging`

# Procedure

1. **Generate keys** — `ssh-keygen -t ed25519 -C "user@machine"`. Copy: `ssh-copy-id user@host`.
2. **Configure SSH** — edit `~/.ssh/config` with host blocks for aliases, jump hosts, multiplexing.
3. **Enable multiplexing** — `ControlMaster auto`, `ControlPath ~/.ssh/sockets/%r@%h-%p`, `ControlPersist 600`.
4. **Jump hosts** — use `ProxyJump bastion` to reach internal hosts through a bastion.
5. **Persistent tmux** — `ssh host -t 'tmux new -s work || tmux attach -t work'`.
6. **Sync files** — `rsync -avz --exclude .git/ ./src/ host:~/project/src/`.
7. **Port forwarding** — `ssh -L 8080:localhost:3000 host` for local access to remote services.
8. **Debug** — `ssh -vvv host` for verbose output; check `/var/log/auth.log` on server.

# SSH config example

```
Host *
    AddKeysToAgent yes
    IdentityFile ~/.ssh/id_ed25519
    ControlMaster auto
    ControlPath ~/.ssh/sockets/%r@%h-%p
    ControlPersist 600
    ServerAliveInterval 60

Host bastion
    HostName bastion.example.com
    User deploy

Host internal
    HostName 10.0.1.50
    User deploy
    ProxyJump bastion
```

# tmux essentials

```bash
tmux new -s dev           # new named session
tmux attach -t dev        # reattach
# Ctrl-b d  detach    Ctrl-b c  new window
# Ctrl-b %  vsplit    Ctrl-b "  hsplit
```

# Decision rules

- Use `ed25519` keys — faster and more secure than RSA.
- Use `ProxyJump` over `ssh -J` — config is version-controllable.
- Create `~/.ssh/sockets/` directory — missing dir causes silent multiplexing failure.
- Set `ServerAliveInterval 60` to prevent dropped idle connections.
- Use `tmux new -s name || tmux attach -t name` to be idempotent.

# References

- https://man.openbsd.org/ssh_config
- https://github.com/tmux/tmux/wiki

# Related skills

- `linux-ubuntu-ops` — server management after SSH access
- `bash` — scripting remote commands
- `terminal-debugging` — debugging remote processes
