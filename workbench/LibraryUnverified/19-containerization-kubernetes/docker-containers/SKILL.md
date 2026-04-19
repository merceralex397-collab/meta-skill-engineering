---
name: docker-containers
description: "Author Dockerfiles, optimize multi-stage builds, write docker-compose services, configure health checks, reduce image size, and scan images for vulnerabilities. Use when creating or editing Dockerfiles, docker-compose.yml, container runtime configuration, or debugging container build/run issues. Do not use for container orchestration (prefer kubernetes/ECS skills) or application code changes unrelated to containerization."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: docker-containers
  maturity: draft
  risk: low
  tags: [docker, dockerfile, compose, multi-stage, containers]
---

# Purpose

Author production-quality Dockerfiles with multi-stage builds, write docker-compose service definitions, optimize image size and layer caching, configure container health checks, manage build arguments and runtime secrets, and scan images for security vulnerabilities.

# When to use this skill

- Creating a new Dockerfile or editing an existing one.
- Writing or modifying a `docker-compose.yml` for local development or production.
- Optimizing a Docker image for size (reducing layers, choosing smaller base images).
- Implementing multi-stage builds to separate build-time and runtime dependencies.
- Adding `HEALTHCHECK` instructions or container health-check configurations.
- Debugging `docker build` failures, layer caching issues, or runtime container errors.
- Scanning Docker images for CVEs using `docker scout`, Trivy, or Snyk.
- Configuring `.dockerignore` to exclude unnecessary files from the build context.
- Setting up Docker BuildKit features (cache mounts, secret mounts, SSH forwarding).

# Do not use this skill when

- The task is about container orchestration (Kubernetes deployments, ECS task definitions, Nomad jobs) — prefer orchestration-specific skills.
- The change is purely application code with no Dockerfile or container configuration impact.
- The task involves cloud-provider container services (ECR, GCR, ACR) — prefer `aws` or `gcp` for registry-specific commands.
- The focus is on CI/CD pipeline design — prefer `cloud-deploy` for deployment strategy.

# Operating procedure

1. **Identify the containerization target.** Determine the application runtime (Node.js, Python, Go, Rust, Java), its dependency installation method, and its build/start commands.
2. **Choose the base image.** Select the smallest suitable base: `alpine` variants for minimal size, `slim` variants for Debian compatibility, or `distroless` for production security. Pin to a specific tag (e.g., `node:20-alpine3.19`), never use `latest`.
3. **Write the .dockerignore.** Exclude `node_modules`, `.git`, `*.md`, test files, local env files, and any files not needed in the build context. Place `.dockerignore` next to the Dockerfile.
4. **Implement multi-stage build.** Stage 1 (`builder`): install all dependencies and compile/build the application. Stage 2 (`runtime`): copy only the built artifacts and production dependencies from the builder stage. This eliminates build tools from the final image.
5. **Optimize layer ordering.** Copy dependency manifests (`package.json`, `requirements.txt`, `go.mod`) before source code. Run `npm ci` / `pip install` / `go mod download` as a separate layer so dependency installs are cached when only source code changes.
6. **Configure build arguments and secrets.** Use `ARG` for build-time variables (app version, build date). Use `--mount=type=secret` (BuildKit) for sensitive build-time values (private registry tokens). Never embed secrets in `ENV` or `RUN` commands.
7. **Set the runtime user.** Add `RUN addgroup -S app && adduser -S app -G app` and `USER app` before the `CMD`. Never run containers as root in production.
8. **Add health checks.** Add `HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD curl -f http://localhost:${PORT}/healthz || exit 1`. For compose, use the `healthcheck` key with `test`, `interval`, `timeout`, and `retries`.
9. **Write docker-compose.yml.** Define services with `build` (context and Dockerfile path), `ports`, `environment`, `volumes` (for local development bind mounts), `depends_on` (with `condition: service_healthy` for health-check ordering), and `networks`.
10. **Build and test locally.** Run `docker build -t app:local .` and verify the image size with `docker images app:local`. Run the container and confirm the health check passes with `docker inspect --format='{{.State.Health.Status}}' <container>`.
11. **Scan for vulnerabilities.** Run `docker scout cves app:local` or `trivy image app:local`. Fix critical and high CVEs by updating base images or pinning patched package versions.
12. **Document image metadata.** Add `LABEL` instructions for `org.opencontainers.image.source`, `org.opencontainers.image.version`, and `org.opencontainers.image.description`.

# Decision rules

- Use multi-stage builds for every production image — single-stage is only acceptable for simple scripts or development images.
- Use `alpine` base images unless the application requires glibc-specific dependencies (in that case, use `slim`).
- Pin base image tags to specific versions, not `latest` or major-only tags.
- Use `COPY --from=builder` to transfer only artifacts — never install build tools in the runtime stage.
- Use `npm ci` over `npm install` for deterministic Node.js dependency installation.
- If the image exceeds 500MB, investigate — most production images should be under 200MB.
- Use BuildKit (`DOCKER_BUILDKIT=1`) for all builds — it enables cache mounts, secret mounts, and parallel stage execution.
- Run containers as non-root unless the application explicitly requires root (and document why).

# Output requirements

1. **Dockerfile** — multi-stage, optimized layer order, pinned base image, non-root user, health check.
2. **docker-compose.yml** — service definitions with health checks, proper depends_on ordering, and environment variable configuration.
3. **.dockerignore** — excludes all unnecessary files from the build context.
4. **Image size report** — final image size and base image used.
5. **Vulnerability scan result** — summary of critical/high CVEs found and remediation status.

# References

- Dockerfile best practices: https://docs.docker.com/build/building/best-practices/
- Multi-stage builds: https://docs.docker.com/build/building/multi-stage/
- Docker Compose specification: https://docs.docker.com/compose/compose-file/
- BuildKit documentation: https://docs.docker.com/build/buildkit/
- Docker Scout: https://docs.docker.com/scout/
- `references/preflight-checklist.md`

# Related skills

- `aws` — ECR image registry, ECS task definitions, Fargate runtime.
- `vercel` — containerized deployment alternatives.
- `terraform-iac` — infrastructure-as-code for container registries and orchestration resources.
- `secret-management` — runtime secret injection into containers.

# Anti-patterns

- Using `latest` as the base image tag — breaks reproducibility and caching.
- Running `apt-get update && apt-get install` without `--no-install-recommends` and without cleaning the apt cache in the same layer.
- Copying the entire source tree before installing dependencies — invalidates the dependency cache on every code change.
- Embedding secrets in `ENV` instructions — they persist in image layers and are visible via `docker history`.
- Running as root in production containers.
- Using `ADD` when `COPY` would suffice — `ADD` has implicit tar extraction and URL download behavior that causes surprises.
- Ignoring `.dockerignore` — large build contexts slow down builds and may leak sensitive files.

# Failure handling

- If `docker build` fails at a `RUN` step, check the specific command's exit code and stderr. Common causes: missing package in the base image, network issues during `apt-get`/`npm install`, or incorrect `WORKDIR`.
- If the image is unexpectedly large, use `docker history <image>` to identify which layers contribute the most size. Check for unneeded build tools in the runtime stage.
- If health checks fail after container start, verify the application is listening on the expected port and that the health endpoint exists. Check `docker logs <container>` for startup errors.
- If vulnerability scanning reports critical CVEs in the base image, update to the latest patched tag or switch to a distroless/alpine variant.
- If the task involves orchestration (Kubernetes, ECS, Swarm), redirect to the appropriate orchestration skill.
