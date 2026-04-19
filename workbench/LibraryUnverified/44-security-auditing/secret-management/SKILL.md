---
name: secret-management
description: "Store, rotate, and inject secrets using Vault, AWS Secrets Manager, GCP Secret Manager, or environment variables with least-privilege access and rotation policies. Use when adding secrets to applications, configuring secret stores, setting up rotation, or auditing secret access patterns. Do not use for encryption-at-rest design, TLS certificate management, or application-level auth token logic."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: secret-management
  maturity: draft
  risk: low
  tags: [secrets, vault, aws-secrets-manager, gcp-secret-manager, env-vars, rotation]
---

# Purpose

Store, rotate, and inject secrets across environments using HashiCorp Vault, AWS Secrets Manager, GCP Secret Manager, or `.env` files — enforcing least-privilege access, automatic rotation, and zero plaintext exposure in code or logs.

# When to use this skill

- Adding new secrets (API keys, database credentials, tokens) to an application
- Configuring a secret store backend (Vault, AWS Secrets Manager, GCP Secret Manager)
- Setting up automatic secret rotation schedules
- Migrating from `.env` files to a managed secret store
- Auditing secret access patterns or investigating leaked credentials
- Writing CI/CD pipelines that inject secrets at build or deploy time
- Reviewing code for hardcoded secrets or insecure secret handling

# Do not use this skill when

- The task is encryption-at-rest design (use a cryptography or storage-encryption skill)
- The task is TLS certificate provisioning or renewal (use a cert-management skill)
- The task is application-level auth token logic (JWT generation, OAuth flows)
- The task is better handled by `terraform-iac` (Vault infrastructure provisioning) or `docker-containers` (runtime env injection only)

# Operating procedure

1. **Identify the secret type and sensitivity tier.** Classify the secret: database credential, API key, signing key, or service token. Assign a sensitivity tier (critical, standard, low) — critical secrets require rotation ≤ 90 days and audit logging.
2. **Select the secret store.** Choose the backend based on the deployment target:
   - HashiCorp Vault → multi-cloud or self-hosted environments
   - AWS Secrets Manager → AWS-native workloads
   - GCP Secret Manager → GCP-native workloads
   - `.env` files → local development only (never production)
3. **Create the secret entry.** Write the secret to the chosen store using the CLI or API. Use a naming convention: `{env}/{service}/{secret-name}` (e.g., `prod/api-server/db-password`).
4. **Configure least-privilege access policies.** Write IAM policies (AWS), IAM bindings (GCP), or Vault policies that grant read access only to the specific service identity that needs the secret. Deny `list` and `write` to application roles.
5. **Set up rotation.** Configure automatic rotation using the store's native rotation (AWS Lambda rotator, Vault dynamic secrets, GCP automatic replication). Set rotation interval based on sensitivity tier. Verify the application can handle credential refresh without downtime.
6. **Wire injection into the application.** Use the SDK or sidecar pattern to fetch secrets at startup — never bake secrets into container images or config files. For Kubernetes, use External Secrets Operator or Vault Agent Injector. For CI/CD, use the platform's secret variable mechanism (GitHub Actions secrets, GitLab CI variables).
7. **Scan for secret leaks.** Run `git log --all -p | grep -iE '(password|secret|api_key|token)\s*='` or use `trufflehog`/`gitleaks` against the repo. If leaks are found, rotate the affected secret immediately and add `.gitignore` entries or pre-commit hooks.
8. **Validate the secret path end-to-end.** Deploy to a staging environment. Confirm the application reads the secret from the store at runtime (not from environment or config fallback). Check application logs to verify no secret values are printed.
9. **Enable audit logging.** Turn on Vault audit device, AWS CloudTrail for Secrets Manager, or GCP audit logs. Confirm that secret read events are recorded with caller identity and timestamp.
10. **Document the secret lifecycle.** Record the secret name, store location, rotation schedule, owning service, and escalation contact in the project's secrets inventory file.

# Decision rules

- If the secret is used in production, it must live in a managed secret store — never in `.env`, config files, or environment variables baked into images.
- If rotation cannot be automated, document the manual rotation runbook and set a calendar reminder.
- If a secret is found in git history, treat it as compromised — rotate immediately regardless of branch.
- If multiple services need the same secret, create separate store entries per service to maintain independent rotation and audit trails.
- If the application cannot tolerate secret rotation (hard-restart required), fix the application to support credential refresh before enabling rotation.
- Prefer dynamic/short-lived credentials (Vault dynamic secrets, AWS STS) over static long-lived keys.

# Output requirements

1. **Secret store configuration** — the store path, access policy, and rotation config as code (Terraform, JSON policy, or CLI commands)
2. **Application integration** — code or config changes showing how the app fetches the secret at runtime
3. **Rotation verification** — evidence that rotation works (test rotation output, log snippet)
4. **Audit confirmation** — proof that secret access events appear in audit logs
5. **Secrets inventory update** — updated entry in the project secrets inventory with name, location, rotation schedule, and owner

# References

- HashiCorp Vault documentation: https://developer.hashicorp.com/vault/docs
- AWS Secrets Manager documentation: https://docs.aws.amazon.com/secretsmanager/
- GCP Secret Manager documentation: https://cloud.google.com/secret-manager/docs
- `trufflehog` for git secret scanning: https://github.com/trufflesecurity/trufflehog
- `gitleaks` for pre-commit secret detection: https://github.com/gitleaks/gitleaks

# Related skills

- `aws` — for AWS-specific IAM and Secrets Manager infrastructure
- `gcp` — for GCP-specific IAM and Secret Manager infrastructure
- `docker-containers` — for runtime secret injection into containers
- `terraform-iac` — for provisioning secret store infrastructure as code

# Anti-patterns

- Hardcoding secrets in source code, Dockerfiles, or CI config files
- Using a single shared secret across multiple services without independent entries
- Storing production secrets in `.env` files or passing them as CLI arguments
- Logging secret values during application startup or debugging
- Granting `*` access to the secret store instead of per-secret, per-service policies
- Skipping rotation because "we'll do it later"

# Failure handling

- If the secret store is unreachable at application startup, the app must fail loudly (crash with a clear error) — never fall back to a hardcoded default.
- If rotation breaks the application, roll back to the previous secret version using the store's versioning feature, then fix the refresh logic before re-enabling rotation.
- If a secret leak is detected, rotate the compromised secret within the hour, revoke any sessions using the old value, and open an incident ticket.
- If access policy changes lock out a service, use the store's emergency break-glass procedure (Vault root token, AWS root account) and immediately scope back down after recovery.
