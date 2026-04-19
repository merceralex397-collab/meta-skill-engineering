# Validation Checklist

A background job or queue change is not ready to merge until every applicable item below is verified.

---

## Idempotency

- [ ] The job produces the same result when executed multiple times with the same input.
- [ ] An idempotency key or deduplication mechanism exists (database record, Redis `SET NX`, BullMQ `jobId`, Sidekiq unique jobs).
- [ ] Side effects with external systems use provider-level idempotency keys where available (e.g., Stripe `idempotency_key`, SendGrid dedup).
- [ ] Re-running a completed job does not send duplicate emails, charge duplicate payments, or create duplicate records.
- [ ] The idempotency guard is tested: unit test enqueues the same job twice and asserts single execution.

## Retry policy

- [ ] `max_retries` is set to a finite number (typically 3–5 for transient, 0 for permanent failures).
- [ ] Backoff strategy is exponential with jitter — not fixed delay, not linear.
- [ ] The backoff formula is documented: `delay = base * 2^attempt + random(0, jitter)`.
- [ ] Transient errors (network timeout, 503, connection refused) trigger retry.
- [ ] Permanent errors (400, 404, validation failure, deserialization error) do NOT trigger retry — they go directly to the dead-letter queue.
- [ ] `retry_on` / error classification is explicit — not a bare `except Exception`.
- [ ] Each retry attempt is logged with attempt number, error, and next retry time.

## Dead-letter queue (DLQ)

- [ ] A DLQ exists for every production queue — no queue operates without one.
- [ ] Jobs that exhaust all retries are moved to the DLQ automatically (Celery `task_reject`, BullMQ default, Sidekiq dead set).
- [ ] The DLQ is monitored — alert fires when DLQ depth > 0.
- [ ] A reprocessing procedure exists: inspect → fix root cause → replay from DLQ.
- [ ] DLQ jobs retain the original payload, error message, stack trace, and attempt count.
- [ ] DLQ has a retention policy — old failed jobs are purged after N days (30–90 days typical).

## Graceful shutdown

- [ ] Workers finish in-flight jobs before stopping — `SIGTERM` triggers graceful shutdown, not `SIGKILL`.
- [ ] Shutdown timeout is configured and exceeds the longest expected job duration.
- [ ] Celery: `--soft-time-limit` and `worker_shutdown_timeout` are set.
- [ ] BullMQ: `Worker.close()` is called on process signals.
- [ ] Sidekiq: `timeout` in `sidekiq.yml` is set.
- [ ] Kubernetes: `terminationGracePeriodSeconds` > worker shutdown timeout.
- [ ] No data loss occurs during rolling deploys — jobs are not lost between old and new worker versions.

## Job payload

- [ ] Payloads are small — pass record IDs, not full objects. Target < 1 KB.
- [ ] Payloads are serializable (JSON-safe). No datetime objects, model instances, or file handles.
- [ ] The job fetches current data from the database inside the job — not stale data from the payload.
- [ ] Payload schema changes are backward-compatible — old workers can process new payloads and vice versa during deploys.
- [ ] Sensitive data (passwords, tokens, PII) is NOT in the payload — pass a reference and look it up.

## Scheduled / periodic jobs

- [ ] Overlap protection exists — a long-running cron job does not stack with the next invocation.
- [ ] Method: distributed lock (`SET NX EX` in Redis), framework support (Celery `expires`, Sidekiq unique), or database-level advisory lock.
- [ ] The schedule is defined in configuration (celerybeat, `repeat` in BullMQ, `cron` in Sidekiq), not hardcoded.
- [ ] Scheduled jobs have an `expires` or TTL — if not picked up within the interval, they are skipped rather than queued.
- [ ] Clock skew between scheduler and workers is accounted for (use UTC, NTP-synced clocks).

## Concurrency and scaling

- [ ] Worker concurrency is set based on job type: I/O-bound → higher concurrency, CPU-bound → match cores.
- [ ] Prefetch multiplier is tuned — too high starves other workers, too low increases idle time.
- [ ] Memory limits are set per worker process (`--max-memory-per-child` in Celery, OS-level limits).
- [ ] Workers can be horizontally scaled without coordination (stateless workers).
- [ ] Queue-specific workers exist for isolation: critical jobs on dedicated workers, bulk jobs on separate workers.

## Monitoring and observability

- [ ] Queue depth metric is exported (Prometheus, Datadog, CloudWatch).
- [ ] Job duration is tracked (p50, p95, p99) with histogram metric.
- [ ] Failure rate is tracked per queue and per task type.
- [ ] DLQ depth is monitored with alerting.
- [ ] Worker count and utilization are visible in dashboards.
- [ ] Structured logging in jobs includes: `job_id`, `queue`, `task_name`, `attempt`, `duration`.
- [ ] A monitoring UI exists: Flower (Celery), Bull Board (BullMQ), Sidekiq Web UI.

## Broker configuration

- [ ] Redis: persistence is enabled (`RDB` + `AOF`) if job durability matters. Without persistence, a Redis restart loses all queued jobs.
- [ ] RabbitMQ: queues are declared as `durable`, messages as `persistent`.
- [ ] SQS: visibility timeout > max job duration. If the job takes longer than visibility timeout, SQS re-delivers it.
- [ ] Connection pooling is configured — workers share connections rather than opening one per job.
- [ ] Broker health check is in the application's readiness probe.

---

Related skills: `rate-limits-retries`, `observability-logging`, `data-model`.
