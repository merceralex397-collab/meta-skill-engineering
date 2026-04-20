---
name: background-jobs-queues
description: "Designs and implements asynchronous job processing with queues, workers, retries, and scheduling. Use when implementing Celery tasks, BullMQ processors, Sidekiq workers, configuring retry policies, setting up dead-letter queues, designing idempotent jobs, scheduling periodic/cron tasks, or troubleshooting stuck/failed jobs."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: background-jobs-queues
  maturity: draft
  risk: medium
  tags: [background, jobs, queues, celery, bullmq, sidekiq, redis, rabbitmq]
---

# Purpose

Designs and implements asynchronous job processing: queue systems (Celery, BullMQ, Sidekiq, Hangfire), message brokers (Redis, RabbitMQ, SQS), job lifecycle management (enqueue → process → retry → dead-letter), idempotency, scheduled/periodic jobs, worker concurrency, monitoring, and graceful shutdown.

# When to use this skill

Use this skill when:

- Implementing Celery tasks, BullMQ processors, Sidekiq workers, or Hangfire jobs
- Configuring retry policies (max attempts, exponential backoff, jitter)
- Setting up dead-letter queues for failed jobs
- Designing idempotent jobs (safe to re-run without side effects)
- Configuring scheduled/periodic tasks (celerybeat, node-cron, Sidekiq-Cron)
- Scaling workers (concurrency, prefetch, autoscaling)
- Implementing job chaining, workflows, or fan-out/fan-in patterns
- Debugging stuck, zombie, or poison-message jobs
- Setting up queue monitoring and alerting (queue depth, job duration, failure rate)

# Do not use this skill when

- The task is about synchronous API request handling — prefer `api-contracts` or framework-specific skills
- The task is about real-time bidirectional communication — prefer `realtime-websocket`
- The task is about database-level scheduling (pg_cron, MySQL events) — prefer `postgresql` or `sqlite`
- The task is about rate limiting API endpoints — prefer `rate-limits-retries`

# Operating procedure

1. **Identify the job and its trigger.** Determine what initiates the job (API request, schedule, event, another job). Define the job's input payload — keep it small and serializable (IDs, not full objects). Document the expected duration and resource usage.

2. **Choose the queue system and broker.** Match to the project stack:
   - Python → Celery + Redis/RabbitMQ, or Django-Q, or Dramatiq
   - Node.js → BullMQ + Redis, or SQS consumer
   - Ruby → Sidekiq + Redis, or GoodJob (Postgres-backed)
   - .NET → Hangfire + SQL Server/Redis, or MassTransit + RabbitMQ
   - Cloud-native → AWS SQS + Lambda, GCP Cloud Tasks, Azure Queue Storage

3. **Implement the job as an idempotent function.** The job must produce the same result if executed multiple times with the same input. Use idempotency keys: store the key before processing, check for existence before executing side effects. Return early if already processed.

4. **Configure retry policy.** Set max retries (typically 3–5 for transient failures). Use exponential backoff with jitter: `delay = base * 2^attempt + random(0, jitter)`. Classify errors: transient (network timeout, 503) → retry; permanent (400, validation error) → send to dead-letter immediately.

5. **Set up dead-letter queue (DLQ).** Jobs that exhaust all retries go to the DLQ. The DLQ must be monitored — alert if depth > 0. Build a reprocessing mechanism: inspect failed job, fix the root cause, replay from DLQ.

6. **Configure worker concurrency and resources.** Set concurrency based on job type: I/O-bound jobs → higher concurrency (e.g., Celery `--concurrency=16`); CPU-bound jobs → match to available cores. Set memory limits per worker. Configure prefetch to control how many jobs a worker pulls ahead.

7. **Implement graceful shutdown.** Workers must finish in-flight jobs before stopping. Configure shutdown timeout (e.g., Celery `--soft-time-limit`, BullMQ `connection.disconnect()`). Kubernetes: set `terminationGracePeriodSeconds` > max job duration.

8. **Add monitoring and alerting.** Track: queue depth (jobs waiting), job duration (p50, p95, p99), failure rate, DLQ depth. Tools: Flower (Celery), Bull Board (BullMQ), Sidekiq Web UI, Prometheus + Grafana. Alert on: queue depth growing, DLQ non-empty, job duration exceeding SLA.

9. **Test the job.** Unit test the job function with mocked dependencies. Integration test: enqueue → process → verify side effects. Test retry behavior: simulate transient failure, verify retry count and backoff. Test idempotency: run the same job twice, verify no duplicate side effects.

# Decision rules

- **Jobs must be idempotent.** If a job cannot be made idempotent, it must use exactly-once semantics with distributed locking — and you must document why idempotency is impossible.
- **Payloads must be small.** Pass IDs and lookup the data inside the job. Never pass large objects, file contents, or database rows in the payload. This prevents serialization failures and stale data.
- **Retry only transient failures.** Permanent failures (validation errors, missing records, authorization failures) should go directly to the DLQ — retrying them wastes resources and delays the alert.
- **Every queue has a DLQ.** No exceptions. An unmonitored DLQ is the same as no DLQ.
- **Scheduled jobs need overlap protection.** If a cron job runs every 5 minutes but takes 7 minutes, you'll get overlapping executions. Use a distributed lock (Redis `SET NX EX`) or the framework's built-in overlap prevention.
- **Prefer at-least-once delivery.** Exactly-once is expensive and fragile. Design for at-least-once with idempotency.
- **Workers must not store state in memory across jobs.** Each job execution starts clean. Shared state goes in the database or cache.

# Output requirements

1. `Job Definition` — function signature, input payload schema, expected duration
2. `Queue Configuration` — broker, queue name, concurrency, retry policy
3. `Idempotency Strategy` — how duplicate execution is prevented
4. `DLQ Plan` — dead-letter queue setup and reprocessing procedure
5. `Monitoring` — metrics tracked and alert thresholds

# References

Read these when the task involves the relevant pattern:

- `references/implementation-patterns.md` — Celery task patterns, BullMQ processors, Sidekiq workers, job chaining, scheduled jobs, idempotency keys, DLQ processing
- `references/validation-checklist.md` — idempotency verification, retry policy, DLQ monitoring, graceful shutdown, payload size
- `references/failure-modes.md` — poison messages, OOM workers, retry storms, lost jobs, zombie workers

# Anti-patterns

- **The fire-and-forget job.** Enqueuing a job with no retry policy, no DLQ, and no monitoring. The job fails silently and nobody notices for days.
- **Fat payloads.** Passing entire database rows, file contents, or HTML in the job payload. Payload grows, serialization breaks, broker memory spikes.
- **Non-idempotent side effects.** Sending an email on every retry attempt. Charging a credit card twice. Use idempotency keys or deduplication.
- **Unbounded retries.** `max_retries=None` or `retries: Infinity`. A permanently failing job retries forever, consuming worker capacity.
- **Synchronous job in the request path.** Enqueuing a job and then polling for its result in the HTTP response. This is a synchronous call with extra steps. Use a webhook or polling endpoint instead.
- **Global worker for all queues.** One worker process consuming from every queue. A slow job in the `email` queue blocks time-sensitive jobs in the `payment` queue. Use dedicated workers per queue or priority queues.
- **Cron job without overlap protection.** A scheduled job that takes longer than its interval, causing pile-up. Use locking or skip-if-running logic.

# Related skills

- `rate-limits-retries` — retry strategies, backoff patterns, circuit breakers
- `observability-logging` — structured logging, metrics, alerting
- `orm-patterns` — database access patterns within jobs
- `data-model` — schema for job state tables, idempotency key storage
- `webhooks-events` — event-driven job triggers

# Failure handling

- **If the broker (Redis/RabbitMQ) is unreachable:** Jobs cannot be enqueued. The application must handle this gracefully — either queue to an in-memory fallback with disk persistence, or return an error to the caller. Never silently drop the job.
- **If a worker crashes mid-job:** The job must be re-delivered by the broker (visibility timeout expires, message is re-queued). The job must be idempotent to handle this safely. Check the broker's acknowledgement settings — auto-ack before processing means lost jobs on crash.
- **If the DLQ fills up:** This is an operational emergency, not a normal state. Page the on-call engineer. Do not auto-retry from the DLQ without human inspection.
- **If job duration exceeds the timeout:** The worker kills the job (Celery `SoftTimeLimitExceeded`, BullMQ stalled job recovery). Ensure the job is designed to be re-entrant or use checkpointing for long-running work.
- **If you cannot determine whether a job is idempotent:** Assume it is not. Add an idempotency key before deploying. Test by running the job twice with the same payload and verifying no duplicate side effects.
