# Failure Modes

Specific background job and queue failure modes to watch for. Each entry describes the symptom, root cause, and mitigation.

---

## 1. Poison message blocking the queue

**Symptom:** A queue stops processing. Workers pick up the same job, fail, retry, fail again — indefinitely. All other jobs in the queue are blocked behind it.

**Root cause:** A single malformed or permanently-failing job is at the head of a FIFO queue. The retry policy re-enqueues it at the front. There is no max-retry limit, or the DLQ is not configured.

**Mitigation:**
- Set `max_retries` to a finite number (3–5).
- Configure a DLQ — jobs that exhaust retries move there automatically.
- Use per-job visibility timeout so that other jobs are not blocked: most modern queue systems (BullMQ, SQS, Sidekiq) process jobs independently.
- For Celery with `acks_late=True`, set `reject_on_worker_lost=True` so crashed workers don't re-queue the poison message indefinitely.

---

## 2. Worker OOM on large payload

**Symptom:** Worker process is killed by the OS OOM killer. Job disappears (no success, no failure logged). The job is re-delivered and kills the next worker.

**Root cause:** The job payload contains a large object (full file content, large JSON blob, base64-encoded image). Deserializing it exceeds worker memory.

**Mitigation:**
- Keep payloads small (< 1 KB). Pass references (S3 URLs, database IDs), not data.
- Set memory limits on workers: Celery `--max-memory-per-child=200000` (200 MB), Kubernetes `resources.limits.memory`.
- For jobs that inherently process large data, stream the data instead of loading it all into memory.
- Monitor worker memory usage and alert before OOM.

---

## 3. Job retry storm

**Symptom:** A downstream service goes down. Hundreds of jobs fail simultaneously. All retry at the same time, creating a thundering herd that overwhelms the service when it recovers.

**Root cause:** All jobs use the same fixed retry delay (e.g., 60 seconds). They all failed at the same time, so they all retry at the same time.

**Mitigation:**
- Use exponential backoff WITH jitter: `delay = base * 2^attempt + random(0, base)`.
- Implement a circuit breaker: if failure rate exceeds a threshold, stop enqueuing new jobs and pause retries.
- Use rate limiting on the worker: BullMQ `limiter`, Celery `rate_limit='10/m'`.
- Alert on retry rate, not just failure rate — a spike in retries is an early warning.

---

## 4. Lost jobs from broker restart without persistence

**Symptom:** After a Redis restart, the queue is empty. Jobs that were enqueued but not yet processed are gone. No errors, no DLQ — they simply vanished.

**Root cause:** Redis was configured without persistence (`save ""` and no `appendonly`), or RabbitMQ queues were declared as non-durable.

**Mitigation:**
- Redis: enable `appendonly yes` and configure `appendfsync everysec` for durability.
- RabbitMQ: declare queues as `durable: true` and publish messages with `persistent: true` (delivery_mode=2).
- SQS: inherently durable (messages persist for up to 14 days).
- For critical jobs, write the job to a database table first (transactional outbox pattern), then enqueue. A sweeper re-enqueues any jobs that were written but not delivered.

---

## 5. Zombie workers holding jobs

**Symptom:** Jobs show as "active" or "processing" in the dashboard but no progress is made. The worker that claimed them is dead but didn't release the jobs.

**Root cause:** A worker process was killed with `SIGKILL` (no graceful shutdown), or the host crashed, or a network partition separated the worker from the broker. The broker thinks the worker is still processing.

**Mitigation:**
- Celery: set `acks_late=True` so the message is only acknowledged after successful processing. Set `visibility_timeout` so unacknowledged messages are re-delivered.
- BullMQ: configure stalled job recovery — BullMQ automatically detects stalled jobs and re-queues them (default: 30-second check interval).
- Sidekiq: uses `BRPOPLPUSH` with a processing set. The reliability plugin detects zombie jobs.
- SQS: visibility timeout automatically re-delivers messages not deleted within the timeout.
- Always implement graceful shutdown (`SIGTERM` handling) to minimize zombie occurrences.

---

## 6. Clock skew in scheduled jobs

**Symptom:** Cron jobs run at unexpected times or run twice. A job scheduled for 08:00 UTC runs at 08:00 and again at 08:03.

**Root cause:** Multiple scheduler instances are running (common in horizontal scaling). Each scheduler independently evaluates the cron expression and enqueues the job. Or, the scheduler host's clock is skewed from UTC.

**Mitigation:**
- Run exactly ONE scheduler instance: Celery Beat with `--scheduler django_celery_beat.schedulers:DatabaseScheduler` and a database lock, BullMQ `QueueScheduler` singleton, Sidekiq-Cron with Redis-based dedup.
- Use a distributed lock for the scheduler (Redis `SET NX EX`, database advisory lock).
- Synchronize all clocks with NTP. Use UTC everywhere.
- Each scheduled job should have a deduplication window: if it ran within the last N minutes, skip.

---

## 7. Deserialization failure on payload schema change

**Symptom:** After a deploy, workers start failing with `KeyError`, `TypeError`, or `ValidationError` on every job. The queue fills up rapidly.

**Root cause:** The new code changed the expected job payload schema (renamed a field, changed a type, added a required field). Jobs enqueued by the old code have the old schema. New workers can't deserialize them.

**Mitigation:**
- Make payload schema changes backward-compatible: add new fields with defaults, don't remove or rename fields in the same deploy.
- Use a two-phase deploy: (1) deploy workers that accept both old and new schema, (2) deploy producers that send the new schema, (3) remove old schema support after all old jobs have drained.
- Version the payload: include a `schema_version` field and handle each version in the worker.
- Monitor deserialization error rate after every deploy.

---

## 8. Memory leak from connection pool per job

**Symptom:** Worker memory grows linearly with the number of jobs processed. Eventually the worker is killed by OOM or becomes unresponsive.

**Root cause:** Each job invocation opens a new database connection, HTTP client, or Redis connection instead of reusing a pool. Connections accumulate because they are not closed.

**Mitigation:**
- Initialize connection pools at the worker level, not the job level. In Celery, use `worker_init` signal or module-level singletons.
- In BullMQ, pass a shared `IORedis` connection to the worker.
- Set connection pool `max_size` and `idle_timeout` so unused connections are reclaimed.
- Monitor worker memory and open file descriptors (`/proc/PID/fd` count).

---

## 9. Job timeout shorter than actual processing time

**Symptom:** Jobs are killed mid-execution and immediately retried, leading to repeated partial work. Sidekiq shows "Timeout::Error", Celery raises `SoftTimeLimitExceeded`.

**Root cause:** The job timeout (soft time limit, visibility timeout) is shorter than the job's actual execution time, especially for jobs with variable duration (e.g., processing large files).

**Mitigation:**
- Set timeout >= p99 job duration + safety margin.
- For SQS, set visibility timeout >= max processing time. Extend it mid-processing with `ChangeMessageVisibility` for long jobs.
- For variable-duration jobs, implement checkpointing: save progress to the database so the next attempt can resume rather than restart.
- Monitor job duration distributions and alert when p95 approaches the timeout threshold.

---

## 10. Duplicate job execution from at-least-once delivery

**Symptom:** Customers receive the same email twice, payments are charged twice, duplicate records appear in the database.

**Root cause:** The broker delivered the message at least once (as guaranteed), the job executed successfully but the acknowledgement was lost (network blip, worker restart). The broker re-delivers the message and the job runs again.

**Mitigation:**
- Make every job idempotent — this is the primary defense.
- Use an idempotency key stored in the database: before processing, check if the key exists; after processing, insert the key; use a transaction to make this atomic.
- For payment-like operations, use the payment provider's idempotency key feature.
- For email-like operations, use a "sent" flag on the record and check it before sending.
- Accept that at-least-once delivery is the norm — design around it rather than fighting it.

---

Related skills: `rate-limits-retries`, `observability-logging`, `data-model`, `webhooks-events`.
