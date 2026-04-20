---
name: cloud-deploy
description: "Plan and execute cloud deployments using blue-green, canary, or rolling strategies with health checks, rollback triggers, and promotion gates. Use when deploying services to any cloud provider, designing deployment pipelines, or adding rollback and health-check logic. Do not use for provider-specific tooling (prefer aws, gcp, vercel skills) or local development environments."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cloud-deploy
  maturity: draft
  risk: low
  tags: [cloud-deploy, blue-green, canary, rolling, rollback]
---

# Purpose

Select, configure, and execute cloud deployment strategies — blue-green swaps, canary percentage ramps, and rolling updates — with automated health checks, promotion gates, and instant rollback paths across any cloud provider.

# When to use this skill

- Deploying a service to staging or production on any cloud provider.
- Designing or modifying a CI/CD pipeline that includes a deploy stage.
- Adding health-check endpoints, readiness probes, or liveness probes to a deploy target.
- Choosing between blue-green, canary, or rolling strategies for a specific service.
- Writing rollback automation or manual rollback runbooks.
- Configuring promotion gates (approval steps, smoke tests, metric thresholds).

# Do not use this skill when

- The task requires provider-specific CLI commands or console workflows — prefer `aws`, `gcp`, or `vercel` skills.
- The change is purely application logic with no deployment artifact or pipeline impact.
- The target is a local development environment (docker-compose up, localhost).
- Container image building is the focus — prefer `docker-containers` skill.

# Operating procedure

1. **Identify the deployment target.** Determine the cloud provider, compute service (VM, container, serverless), region, and environment (staging, production).
2. **Select the deployment strategy.** Choose blue-green if zero-downtime cutover is required and two full environments are affordable. Choose canary if gradual traffic shifting with metric-based promotion is needed. Choose rolling if the service supports N-1 compatibility and incremental pod/instance replacement.
3. **Define health-check criteria.** Specify the health endpoint path (e.g., `/healthz`), expected HTTP status (200), response timeout (≤5s), and failure threshold (3 consecutive failures).
4. **Configure promotion gates.** List the checks that must pass before traffic shifts: smoke test suite, error-rate threshold (<0.5%), p99 latency threshold, and any manual approval steps.
5. **Write the rollback trigger.** Define automatic rollback conditions: error rate exceeds threshold for >2 minutes, health check fails on new version, or deployment times out past the configured window.
6. **Implement the pipeline stage.** Add the deploy step to the CI/CD config (GitHub Actions, Cloud Build, CodePipeline). Wire in the strategy, health checks, and promotion gates as pipeline conditions.
7. **Execute a dry-run deployment.** Run the pipeline against staging with `--dry-run` or equivalent flag. Verify the deployment plan output matches expectations before applying.
8. **Deploy to staging and validate.** Apply the deployment, monitor the health-check endpoint, run the smoke test suite, and confirm metrics stay within thresholds.
9. **Promote to production.** Trigger the promotion gate. For blue-green, switch the load balancer target group. For canary, ramp traffic from 5% → 25% → 100% with metric checks at each step. For rolling, confirm each batch reaches healthy state before proceeding.
10. **Verify post-deployment.** Confirm all instances/pods report healthy, check error rates and latency in the monitoring dashboard, and validate that rollback artifacts (previous image tag, previous task definition) are recorded.
11. **Document the deployment.** Record the deployed version, strategy used, promotion gate results, and rollback instructions in the deployment log or ticket.

# Decision rules

- If the service cannot run two versions simultaneously, use blue-green over canary.
- If cost constraints prevent duplicate environments, use rolling updates.
- If the service has database migrations, deploy the migration separately before the application deploy.
- Never skip health checks — a deploy without health verification is incomplete.
- If any promotion gate fails, halt the rollout and trigger rollback automatically.
- Prefer immutable deployment artifacts (container images, versioned bundles) over in-place mutations.
- Record the previous known-good version before every deployment.

# Output requirements

1. **Deployment plan** — target, strategy, health-check config, promotion gates, rollback triggers.
2. **Pipeline configuration** — the CI/CD stage definition with strategy and gates wired in.
3. **Health-check specification** — endpoint, thresholds, timeout values.
4. **Rollback runbook** — exact commands or automation to revert to the previous version.
5. **Post-deploy verification** — checklist of metrics and endpoints confirmed healthy.

# References

- Blue-green deployment pattern: https://martinfowler.com/bliki/BlueGreenDeployment.html
- Canary releases: https://martinfowler.com/bliki/CanaryRelease.html
- Kubernetes rolling update strategy: https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#rolling-update-deployment
- `references/preflight-checklist.md`
- `references/verification-checks.md`
- `references/rollback-notes.md`

# Related skills

- `aws` — AWS-specific deploy tooling (CodeDeploy, ECS, Lambda).
- `gcp` — GCP-specific deploy tooling (Cloud Run, Cloud Deploy).
- `vercel` — Vercel deployment and preview environments.
- `docker-containers` — container image building and optimization.

# Anti-patterns

- Deploying directly to production without a staging validation step.
- Using mutable tags (e.g., `latest`) as the deployment artifact — breaks rollback.
- Configuring health checks that always return 200 regardless of application state.
- Skipping database migration ordering — deploying app code that assumes a schema not yet applied.
- Setting canary traffic to 50% on the first step — defeats the purpose of gradual rollout.

# Failure handling

- If health checks fail on the new version within the configured window, execute automatic rollback to the previous known-good version.
- If the CI/CD pipeline times out, mark the deployment as failed, do not promote, and alert the on-call channel.
- If rollback itself fails, escalate immediately — page the on-call engineer and freeze further deployments.
- If the deployment strategy is unclear from the repo context, ask for clarification rather than defaulting to a potentially destructive strategy.
- If provider-specific details are needed, redirect to the appropriate provider skill (`aws`, `gcp`, `vercel`).
