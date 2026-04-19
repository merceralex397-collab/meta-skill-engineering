---
name: serverless-patterns
description: "Design serverless architectures using Lambda, Cloud Functions, or Workers — handle cold starts, configure event triggers, implement fan-out patterns, wire step functions and state machines, and keep functions stateless with external state stores. Use when designing or refactoring serverless systems, debugging cold start latency, or choosing between serverless patterns. Do not use for long-running container workloads, Kubernetes deployments, or monolithic server applications."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: serverless-patterns
  maturity: draft
  risk: low
  tags: [serverless, lambda, cloud-functions, cold-start, step-functions]
---

# Purpose

Design and implement serverless architectures using AWS Lambda, GCP Cloud Functions, or Cloudflare Workers — minimize cold start latency, configure event triggers (HTTP, queue, schedule, storage), implement fan-out and aggregation patterns, wire step functions and state machines for orchestration, and enforce stateless function design with external state stores.

# When to use this skill

- Designing a new serverless system or decomposing a monolith into functions
- Writing or editing Lambda, Cloud Function, or Worker handler code
- Configuring event source mappings (API Gateway, SQS, S3, Pub/Sub, Cron)
- Debugging cold start latency or function timeout issues
- Implementing fan-out/fan-in patterns for parallel processing
- Wiring AWS Step Functions, GCP Workflows, or Temporal for orchestration
- Choosing between serverless and container-based deployment for a workload
- Optimizing function bundle size, memory allocation, or concurrency settings

# Do not use this skill when

- The workload requires long-running processes (> 15 minutes) — use containers or VMs
- The task is Kubernetes pod/deployment configuration — use a Kubernetes skill
- The task is Terraform provisioning of serverless infra — use `terraform-iac` (then return here for function logic)
- The task is Cloudflare Worker-specific patterns at the edge — use `cloudflare-worker-patterns`
- The task is application business logic with no serverless architecture concerns

# Operating procedure

1. **Classify the workload.** Determine if the workload fits serverless constraints: execution time < 15 min (Lambda) or < 540s (Cloud Functions), stateless per invocation, event-driven trigger, bursty or unpredictable traffic. If any constraint is violated, recommend containers instead.
2. **Choose the platform and runtime.** Select AWS Lambda, GCP Cloud Functions, or Cloudflare Workers based on the existing cloud provider and latency requirements. Choose the runtime (Node.js, Python, Go, Rust) — prefer compiled runtimes (Go, Rust) for cold-start-sensitive paths.
3. **Design the function boundary.** Each function should do exactly one thing. Map the function to a single event source. Define the input event schema and output contract. If a function needs to call another function, use an event bus or queue — not direct invocation.
4. **Configure the event trigger.** Wire the event source:
   - HTTP → API Gateway (AWS) or HTTP trigger (GCP) with route, method, and auth config
   - Queue → SQS/SNS (AWS) or Pub/Sub (GCP) with batch size and visibility timeout
   - Schedule → EventBridge rule (AWS) or Cloud Scheduler (GCP) with cron expression
   - Storage → S3 event notification (AWS) or GCS trigger (GCP) with event type filter
5. **Minimize cold start impact.** Apply these techniques in order of effectiveness:
   - Reduce bundle size: tree-shake dependencies, exclude dev packages, use Lambda layers or GCP buildpacks for shared libs
   - Set provisioned concurrency (Lambda) or min instances (Cloud Functions) for latency-critical paths
   - Use a compiled runtime (Go, Rust) instead of interpreted (Python, Node.js) for sub-100ms cold starts
   - Keep the handler initialization outside the handler function (module-level SDK clients, DB connection pools)
6. **Externalize state.** Functions must be stateless between invocations. Store state in: DynamoDB/Firestore (key-value), S3/GCS (blobs), Redis/Memorystore (cache), or RDS/Cloud SQL (relational). Pass state references (IDs, keys) between functions via events, not in-memory.
7. **Implement fan-out when needed.** For parallel processing: publish N messages to a queue/topic from a dispatcher function, let N worker functions process in parallel, aggregate results via a completion handler or Step Function. Set concurrency limits to avoid downstream throttling.
8. **Wire orchestration for multi-step workflows.** Use AWS Step Functions or GCP Workflows for sequential/branching logic across functions. Define the state machine in ASL (Amazon States Language) or YAML. Include retry policies with exponential backoff and catch blocks for error states.
9. **Set resource limits and alarms.** Configure: memory (start at 256 MB, benchmark up), timeout (set to 2× expected P99 duration), reserved concurrency (protect downstream services), and DLQ for failed events. Create CloudWatch/Cloud Monitoring alarms for: error rate > 1%, duration P99 > 80% of timeout, throttle count > 0.
10. **Test locally and deploy.** Use SAM CLI (`sam local invoke`), Functions Framework, or Miniflare for local testing. Write integration tests that invoke the function with real event payloads. Deploy via IaC (SAM, Serverless Framework, Terraform) — never via console clicks.
11. **Verify in production.** Invoke the function with a test event. Check logs (CloudWatch, Cloud Logging) for successful execution. Confirm the DLQ is empty. Verify downstream effects (database write, queue message, API response).

# Decision rules

- If function execution time regularly exceeds 60 seconds, evaluate whether the workload belongs in a container instead.
- If cold start latency exceeds the user-facing SLA, enable provisioned concurrency or switch to a compiled runtime before adding architectural complexity.
- If two functions always execute sequentially with no other consumers, consider merging them into one function to reduce invocation overhead.
- If the function fan-out degree exceeds 100, add concurrency limits and implement backpressure — unbounded fan-out will throttle downstream services.
- If the function needs a persistent connection (WebSocket, long-poll), use API Gateway WebSocket API or a container — standard Lambda/Cloud Functions are request-response only.
- Prefer event-driven invocation (queue, event bus) over synchronous HTTP chains between functions.

# Output requirements

1. **Function code** — handler implementation with typed event input and output
2. **Event source configuration** — trigger config as IaC (SAM template, Terraform, or serverless.yml)
3. **Resource limits** — memory, timeout, concurrency, and DLQ configuration
4. **Orchestration definition** — Step Function ASL or Workflow YAML (if multi-step)
5. **Cold start analysis** — measured or estimated cold start time with optimization rationale
6. **Monitoring setup** — alarms for error rate, duration, and throttling

# References

- AWS Lambda developer guide: https://docs.aws.amazon.com/lambda/latest/dg/
- GCP Cloud Functions documentation: https://cloud.google.com/functions/docs
- Cloudflare Workers documentation: https://developers.cloudflare.com/workers/
- AWS Step Functions developer guide: https://docs.aws.amazon.com/step-functions/latest/dg/
- Serverless Framework: https://www.serverless.com/framework/docs
- AWS SAM CLI: https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/

# Related skills

- `aws` — for AWS-specific Lambda, API Gateway, and SQS configuration
- `gcp` — for GCP-specific Cloud Functions and Pub/Sub configuration
- `cloudflare-worker-patterns` — for Cloudflare Workers edge-specific patterns
- `terraform-iac` — for provisioning serverless infrastructure as code

# Anti-patterns

- Calling Lambda functions synchronously from other Lambda functions (use queues or Step Functions)
- Storing state in `/tmp` and expecting it to persist across invocations
- Setting timeout to the maximum "just in case" instead of benchmarking actual duration
- Deploying large monolithic bundles with unused dependencies that inflate cold start time
- Using serverless for steady-state, high-throughput workloads where containers are more cost-effective
- Ignoring DLQ — failed events silently disappear without retry or alerting

# Failure handling

- If a function times out, check whether the root cause is downstream latency (DB, API) or bundle size. Increase timeout only after addressing the root cause.
- If the DLQ accumulates messages, investigate the failure pattern before replaying — blind replay can cause duplicate processing.
- If throttling occurs, check whether reserved concurrency is too low or downstream services are the bottleneck. Scale downstream before raising concurrency.
- If Step Function execution fails, inspect the failed state's error output. Add retry with backoff for transient errors and catch blocks for permanent failures.
- If cold starts spike after a deployment, compare the new bundle size to the previous version — dependency additions are the most common cause.
