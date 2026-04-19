---
name: tailscale-private-networking
description: "Configure Tailscale for private networking — write ACL policies, set up exit nodes and subnet routers, enable MagicDNS, expose services with Funnel, and manage Tailscale SSH access controls. Use when setting up Tailscale on servers, writing tailnet ACL rules, configuring node sharing, or troubleshooting connectivity. Do not use for traditional VPN appliances, public DNS configuration, or WireGuard manual setup without Tailscale."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: tailscale-private-networking
  maturity: draft
  risk: low
  tags: [tailscale, acl, exit-nodes, subnet-router, magicdns, funnel]
---

# Purpose

Configure Tailscale for private networking across servers, developer machines, and cloud instances — write ACL policies to enforce least-privilege access, set up exit nodes for outbound routing, configure subnet routers to bridge non-Tailscale networks, enable MagicDNS for human-readable names, expose services to the public internet with Funnel, and manage Tailscale SSH for passwordless access.

# When to use this skill

- Installing and authenticating Tailscale on a new server or device
- Writing or editing tailnet ACL policies in `policy.json` or the admin console
- Configuring a node as an exit node for outbound internet routing
- Setting up a subnet router to advertise local network routes (e.g., `10.0.0.0/24`)
- Enabling MagicDNS and configuring custom search domains
- Exposing an internal service to the public internet via Tailscale Funnel
- Setting up Tailscale SSH to replace traditional SSH key management
- Sharing nodes or services with external users via node sharing
- Troubleshooting Tailscale connectivity issues (DERP relays, NAT traversal, firewall)
- Integrating Tailscale with Docker containers or Kubernetes sidecars

# Do not use this skill when

- The task is manual WireGuard configuration without Tailscale — use a general VPN skill
- The task is public DNS record management — use a DNS or domain skill
- The task is traditional VPN appliance setup (OpenVPN, IPSec) — use a VPN skill
- The task is general firewall rules unrelated to Tailscale — use `self-hosting-ops`
- The task is application code changes with no networking component

# Operating procedure

1. **Install Tailscale on the target node.** Use the official install script: `curl -fsSL https://tailscale.com/install.sh | sh`. For Docker, use the `tailscale/tailscale` sidecar image. For Kubernetes, deploy the Tailscale operator or sidecar container. Verify installation with `tailscale version`.
2. **Authenticate the node to the tailnet.** Run `sudo tailscale up --authkey=tskey-auth-...` using a pre-generated auth key (reusable, ephemeral, or tagged). For interactive auth, run `sudo tailscale up` and follow the login URL. Confirm the node appears in the admin console.
3. **Assign ACL tags to the node.** In the Tailscale admin console or `policy.json`, assign tags to the node (e.g., `tag:server`, `tag:monitoring`). Tags are the foundation of ACL rules — every node should be tagged.
4. **Write ACL policies.** Edit the ACL policy file to define access rules. Structure rules as:
   - `{"action": "accept", "src": ["tag:admin"], "dst": ["tag:server:*"]}` — admins can reach all ports on servers
   - `{"action": "accept", "src": ["tag:monitoring"], "dst": ["tag:server:9090"]}` — monitoring can reach Prometheus port only
   - Default deny is implicit — only explicitly allowed traffic passes. Test ACL changes with `tailscale debug acl` before applying.
5. **Configure exit node (if needed).** On the exit node machine, run `sudo tailscale up --advertise-exit-node`. In the ACL policy, add `"autoApprovers": {"exitNode": ["tag:exit-node"]}` or approve manually in the admin console. On client devices, run `tailscale up --exit-node={exit-node-ip}` to route all internet traffic through the exit node.
6. **Configure subnet router (if needed).** On the subnet router machine, run `sudo tailscale up --advertise-routes=10.0.0.0/24,192.168.1.0/24`. Enable IP forwarding: `echo 'net.ipv4.ip_forward = 1' | sudo tee -a /etc/sysctl.d/99-tailscale.conf && sudo sysctl -p /etc/sysctl.d/99-tailscale.conf`. Approve the routes in the admin console or via auto-approvers in the ACL policy.
7. **Enable MagicDNS.** In the Tailscale admin console under DNS settings, enable MagicDNS. Optionally add custom search domains and set global nameservers. Verify with `tailscale status` — nodes should show their MagicDNS names (e.g., `server1.tailnet-name.ts.net`). Test resolution: `dig server1.tailnet-name.ts.net`.
8. **Set up Tailscale Funnel (if exposing to public internet).** Run `tailscale funnel 443 --bg` to expose port 443 publicly. Tailscale provisions a public hostname (`machine-name.tailnet-name.ts.net`) with a valid TLS certificate. Verify with `curl https://machine-name.tailnet-name.ts.net` from a non-tailnet device. Restrict Funnel in ACLs if needed.
9. **Configure Tailscale SSH (if replacing traditional SSH).** In the ACL policy, add SSH rules: `"ssh": [{"action": "accept", "src": ["tag:admin"], "dst": ["tag:server"], "users": ["autogroup:nonroot"]}]`. On target servers, run `sudo tailscale up --ssh`. Disable traditional SSH (`sudo systemctl stop sshd`) only after confirming Tailscale SSH works.
10. **Verify connectivity end-to-end.** From a client node, run `tailscale ping {target-node}` to test direct connectivity. Check `tailscale status` for relay vs. direct connection. If traffic is relayed via DERP, check firewall rules — Tailscale needs UDP 41641 outbound or will fall back to DERP relays.
11. **Document the tailnet topology.** Record: node names and tags, ACL policy summary, subnet routes, exit nodes, Funnel endpoints, and MagicDNS domain. Store the ACL policy in version control.

# Decision rules

- If two nodes cannot establish a direct connection, check that UDP 41641 is allowed outbound on both — DERP relay is functional but adds latency.
- If a service should be accessible from outside the tailnet, use Funnel. If it should be accessible only within the tailnet, use MagicDNS names and ACL rules.
- If you need to bridge a legacy network (office LAN, VPC), use a subnet router. If you need all internet traffic routed through a specific node, use an exit node.
- If ACL changes break connectivity, Tailscale preserves the last working policy — fix the new policy and resubmit. Never leave a broken ACL in place.
- Prefer tagged ACLs (`tag:role`) over user-based ACLs for server-to-server communication. User-based ACLs are appropriate for developer laptop access.
- Use ephemeral auth keys for auto-scaling or CI nodes that should be cleaned up automatically. Use reusable keys for persistent servers.

# Output requirements

1. **Tailscale installation commands** — exact install and `tailscale up` commands for each node
2. **ACL policy** — complete JSON policy file or diff showing the changes
3. **Network topology** — list of nodes, tags, subnet routes, exit nodes, and Funnel endpoints
4. **Connectivity verification** — output of `tailscale ping` and `tailscale status` confirming direct connections
5. **DNS configuration** — MagicDNS settings and any custom search domains

# References

- Tailscale documentation: https://tailscale.com/kb/
- ACL policy reference: https://tailscale.com/kb/1018/acls/
- Subnet routers: https://tailscale.com/kb/1019/subnets/
- Exit nodes: https://tailscale.com/kb/1103/exit-nodes/
- Tailscale Funnel: https://tailscale.com/kb/1223/funnel/
- Tailscale SSH: https://tailscale.com/kb/1193/tailscale-ssh/
- MagicDNS: https://tailscale.com/kb/1081/magicdns/

# Related skills

- `secret-management` — for managing Tailscale auth keys and API tokens
- `self-hosting-ops` — for server provisioning that Tailscale connects
- `docker-containers` — for running Tailscale as a sidecar in containerized deployments
- `terraform-iac` — for provisioning Tailscale resources via the Tailscale Terraform provider

# Anti-patterns

- Using `autogroup:*` in ACL rules instead of specific tags — defeats the purpose of least-privilege
- Running Tailscale as root when the sidecar or userspace mode would suffice
- Forgetting to enable IP forwarding on subnet router nodes — routes are advertised but traffic is dropped
- Approving subnet routes or exit nodes without restricting who can use them in the ACL policy
- Exposing internal services via Funnel without considering that Funnel traffic is publicly accessible
- Using interactive login instead of auth keys for automated/CI server provisioning

# Failure handling

- If `tailscale ping` shows only DERP relay, check firewall rules for UDP 41641 on both endpoints. If direct connections are impossible (symmetric NAT on both sides), accept DERP relay and document the latency impact.
- If a node does not appear in `tailscale status`, verify the auth key is valid and not expired. Rerun `tailscale up` with a fresh key. Check `journalctl -u tailscaled` for errors.
- If ACL changes lock out access, use the admin console (web UI) to revert to the previous policy. The admin console is always accessible as the tailnet owner.
- If Funnel is not reachable from the public internet, confirm the node is online (`tailscale status`) and that Funnel is enabled in the tailnet's capabilities. Check `tailscale funnel status`.
- If subnet routes are not working, verify IP forwarding is enabled (`sysctl net.ipv4.ip_forward`) and that the routes are approved in the admin console.
