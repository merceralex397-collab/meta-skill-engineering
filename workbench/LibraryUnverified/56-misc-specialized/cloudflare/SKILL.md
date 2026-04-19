---
name: cloudflare
description: "Configure and deploy on the Cloudflare platform — set up Workers, Pages, D1 databases, R2 storage, KV namespaces, DNS records, WAF rules, and caching policies via wrangler or the dashboard. Use when deploying to Cloudflare, configuring DNS/CDN, or managing Cloudflare-specific resources. Do not use for generic CDN caching theory or non-Cloudflare edge platforms."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cloudflare
  maturity: draft
  risk: low
  tags: [cloudflare, workers, pages, d1, r2, kv, wrangler]
---

# Purpose

Configure, deploy, and manage Cloudflare platform resources — Workers, Pages, D1 databases, R2 object storage, KV namespaces, DNS zones, WAF rules, and cache configurations — using wrangler CLI or the Cloudflare dashboard.

# When to use this skill

- Setting up or modifying a `wrangler.toml` configuration file.
- Creating or managing DNS records, zones, or DNSSEC settings.
- Configuring Cloudflare Pages projects with build settings and environment variables.
- Creating D1 databases, running migrations, or binding D1 to Workers.
- Provisioning R2 buckets, setting CORS policies, or configuring lifecycle rules.
- Creating or updating KV namespaces, binding them to Workers, and managing key-value data.
- Writing WAF custom rules, rate-limiting rules, or firewall expressions.
- Configuring cache rules, page rules, or Cache API usage patterns.
- Deploying Workers or Pages via `wrangler deploy` or `wrangler pages deploy`.

# Do not use this skill when

- Writing Worker application code logic (fetch handlers, Durable Objects) — prefer `cloudflare-worker-patterns`.
- The task involves generic CDN caching theory not specific to Cloudflare's implementation.
- The target platform is Vercel, AWS CloudFront, or another non-Cloudflare edge network.
- The task is about serverless architecture patterns in general — prefer `serverless-patterns`.

# Operating procedure

1. **Identify the Cloudflare resource.** Determine which product is involved: Workers, Pages, D1, R2, KV, DNS, WAF, or caching.
2. **Locate the wrangler.toml.** Check the repo root and subdirectories for `wrangler.toml`. If absent and a Worker/Pages project is needed, create one with `wrangler init`.
3. **Verify account and zone context.** Confirm the `account_id` is set in `wrangler.toml` or environment. For DNS/WAF, confirm the correct `zone_id`.
4. **Configure bindings.** For D1, add `[[d1_databases]]` with `binding`, `database_name`, and `database_id`. For KV, add `[[kv_namespaces]]` with `binding` and `id`. For R2, add `[[r2_buckets]]` with `binding` and `bucket_name`.
5. **Set environment variables and secrets.** Use `wrangler secret put <KEY>` for sensitive values. Use `[vars]` in `wrangler.toml` for non-sensitive config. Never commit secrets to the toml file.
6. **Configure DNS records.** Use `wrangler dns create` or the dashboard to add A, AAAA, CNAME, or TXT records. Set proxy status (orange cloud) for records that should pass through Cloudflare.
7. **Set up WAF rules.** Write firewall expressions using Cloudflare's wirefilter syntax. Apply rate-limiting rules with `requests_per_period` and `period` thresholds. Test rules in log-only mode before enforcing.
8. **Configure caching.** Set cache TTLs via page rules or Cache Rules. Use `Cache-Control` headers for origin-controlled caching. Configure Browser TTL and Edge TTL separately. Use the Cache API in Workers for programmatic cache control.
9. **Deploy the resource.** Run `wrangler deploy` for Workers, `wrangler pages deploy <dir>` for Pages, or `wrangler d1 migrations apply <db>` for D1 schema changes.
10. **Verify the deployment.** Check `wrangler tail` for real-time logs. Hit the deployed URL and confirm expected responses. For DNS changes, use `dig` or `nslookup` to verify propagation.
11. **Set up monitoring.** Enable Cloudflare Analytics for the zone. Configure notification policies for error rate spikes, WAF blocks, or origin health degradation.

# Decision rules

- Use KV for read-heavy key-value data with eventual consistency tolerance (<60s propagation).
- Use D1 for relational data that requires SQL queries and transactional reads.
- Use R2 for large objects (images, files, backups) — it has no egress fees.
- Proxy DNS records (orange cloud) for all web traffic; use DNS-only (grey cloud) for mail servers and non-HTTP services.
- Prefer `wrangler.toml` bindings over hardcoded account/zone IDs in application code.
- Use Pages for static sites and SSR frameworks; use Workers for API endpoints and custom logic.
- Always test WAF rules in log mode before switching to block mode.

# Output requirements

1. **Resource configuration** — the `wrangler.toml` bindings, DNS records, or WAF rules created/modified.
2. **Deployment command** — the exact `wrangler` command used to deploy.
3. **Verification result** — confirmation the resource is live (URL response, DNS propagation, log output).
4. **Secrets inventory** — list of secrets set via `wrangler secret put` (names only, not values).
5. **Rollback path** — previous Worker version ID or Pages deployment ID to revert to.

# References

- Wrangler CLI docs: https://developers.cloudflare.com/workers/wrangler/
- D1 documentation: https://developers.cloudflare.com/d1/
- R2 documentation: https://developers.cloudflare.com/r2/
- KV documentation: https://developers.cloudflare.com/kv/
- Cloudflare WAF custom rules: https://developers.cloudflare.com/waf/custom-rules/
- Pages documentation: https://developers.cloudflare.com/pages/
- `references/preflight-checklist.md`

# Related skills

- `cloudflare-worker-patterns` — Worker application code, Durable Objects, fetch handlers.
- `vercel` — alternative edge/serverless deployment platform.
- `serverless-patterns` — generic serverless architecture design.

# Anti-patterns

- Hardcoding `account_id` or `zone_id` in application code instead of `wrangler.toml`.
- Committing `wrangler secret` values to `wrangler.toml` or source control.
- Enabling WAF block rules without testing in log-only mode first.
- Using KV for write-heavy workloads that need immediate consistency — use D1 or Durable Objects.
- Deploying to production without checking `wrangler tail` for runtime errors.
- Setting DNS records to proxy mode for non-HTTP services (breaks SMTP, SSH).

# Failure handling

- If `wrangler deploy` fails with an authentication error, verify `CLOUDFLARE_API_TOKEN` is set and has the correct permissions (Workers Scripts:Edit, Zone:Edit).
- If DNS changes do not propagate within 5 minutes, check for conflicting records and verify the zone is active.
- If D1 migrations fail, check the migration SQL syntax and run `wrangler d1 migrations list <db>` to confirm migration state.
- If WAF rules block legitimate traffic, immediately switch the rule to log mode and review matched requests.
- If the task requires Worker code changes beyond configuration, redirect to the `cloudflare-worker-patterns` skill.
