---
name: queues-cron-workers
description: "Design and configure message queues, cron schedules, and background workers — set up SQS/Cloud Tasks/Bull queues, configure Cloud Scheduler/cron triggers, implement retry policies, wire dead-letter queues, and manage worker concurrency. Use when adding async job processing, scheduled tasks, or queue-based architectures. Do not use for synchronous API design, real-time WebSocket messaging, or batch ETL pipelines."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: queues-cron-workers
  maturity: draft
  risk: low
  tags: [queues, cron, workers, sqs, cloud-tasks, bull, dead-letter]
---

# Purpose

Design, configure, and operate message queues, cron-scheduled jobs, and background worker processes — set up queues (SQS, Cloud Tasks, Bull/BullMQ, Celery), configure cron triggers (Cloud Scheduler, crontab, node-cron), implement retry and backoff policies, wire dead-letter queues for failed messages, and manage worker concurrency and scaling.

# When to use this skill

- Adding async job processing to an application (email sending, image processing, webhook delivery).
- Setting up a message queue with SQS, GCP Cloud Tasks, RabbitMQ, or Bull/BullMQ backed by Redis.
- Configuring scheduled/cron jobs via Cloud Scheduler, crontab, GitHub Actions schedule, or node-cron.
- Implementing retry policies with exponential backoff for failed queue messages.
- Wiring dead-letter queues (DLQ) to capture messages that exceed the retry limit.
- Configuring worker concurrency, rate limiting, and job prioritization.
- Designing queue-based architectures for decoupling producers and consumers.
- Monitoring queue depth, processing latency, and DLQ accumulation.

# Do not use this skill when

- The task is synchronous API request/response design — prefer API design skills.
- The focus is real-time WebSocket or SSE messaging — prefer real-time communication skills.
- The task is batch ETL pipeline design (Spark, Dataflow, Airflow DAGs) — prefer data engineering skills.
- Queue implementation is provider-specific and the question is about provider console/CLI — prefer `aws` or `gcp`.

# Operating procedure

1. **Identify the job type.** Determine if the work is event-driven (triggered by an action), scheduled (runs on a cron), or both. Identify the producer (what enqueues the job) and the consumer (what processes it).
2. **Choose the queue technology.** For AWS: use SQS (standard for at-least-once, FIFO for exactly-once). For GCP: use Cloud Tasks (HTTP target) or Pub/Sub (fan-out). For application-level: use BullMQ (Node.js + Redis) or Celery (Python + Redis/RabbitMQ). Match the choice to the existing stack.
3. **Create the queue.** For SQS: `aws sqs create-queue --queue-name <name> --attributes VisibilityTimeout=60,MessageRetentionPeriod=1209600`. For BullMQ: instantiate `new Queue('job-name', { connection: redisConfig })`. For Cloud Tasks: `gcloud tasks queues create <queue> --max-dispatches-per-second=10 --max-concurrent-dispatches=5`.
4. **Implement the producer.** Send messages with structured payloads: include `jobType`, `payload`, `metadata` (timestamp, correlation ID, retry count). For SQS: `sqs.sendMessage({ QueueUrl, MessageBody: JSON.stringify(payload) })`. For BullMQ: `queue.add('job-name', payload, { delay, attempts, backoff })`.
5. **Implement the consumer/worker.** For BullMQ: `const worker = new Worker('job-name', async (job) => { ... }, { concurrency: 5 })`. For SQS: use a polling loop or Lambda trigger. For Celery: `@app.task def process(payload): ...`. Ensure the handler is idempotent — the same message may be delivered more than once.
6. **Configure retry policy.** Set max retry attempts (typically 3–5). Use exponential backoff: `backoff = min(base * 2^attempt, maxDelay)`. For BullMQ: `{ attempts: 5, backoff: { type: 'exponential', delay: 1000 } }`. For SQS: configure visibility timeout to allow processing time plus backoff delay.
7. **Wire the dead-letter queue.** Create a DLQ: a separate queue that receives messages exceeding the retry limit. For SQS: set `RedrivePolicy: { deadLetterTargetArn: <dlq-arn>, maxReceiveCount: 5 }`. For BullMQ: listen to the `failed` event after all retries and move to a `dead-letter` queue. For Cloud Tasks: configure a dead-letter topic on the queue.
8. **Configure cron/scheduled jobs.** For Cloud Scheduler: `gcloud scheduler jobs create http <name> --schedule="0 */6 * * *" --uri=<url> --http-method=POST`. For node-cron: `cron.schedule('0 */6 * * *', () => { queue.add('scheduled-job', payload) })`. For crontab: `crontab -e` and add the schedule line. Always use UTC for cron expressions in cloud environments.
9. **Set concurrency and rate limits.** For BullMQ: set `concurrency` on the Worker constructor. For Cloud Tasks: set `max-concurrent-dispatches` and `max-dispatches-per-second`. For Celery: set `worker_concurrency` and use `rate_limit` on individual tasks. Match concurrency to downstream capacity (database connections, API rate limits).
10. **Add monitoring and alerting.** Monitor queue depth (messages waiting), processing latency (time from enqueue to completion), DLQ depth, and worker error rate. Alert when queue depth exceeds 2x the normal processing rate, or when DLQ receives any messages.
11. **Test the full flow.** Enqueue a test message, verify the worker processes it, confirm the result. Enqueue a message that will fail, verify it retries the configured number of times, then lands in the DLQ. Verify cron jobs fire at the expected schedule.

# Decision rules

- Use SQS Standard for high-throughput at-least-once delivery; use SQS FIFO when message ordering and exactly-once processing are required.
- Use BullMQ when the application is Node.js and Redis is already in the stack — it provides job scheduling, priorities, and rate limiting out of the box.
- Use Celery when the application is Python and needs distributed task execution with result backends.
- Use Cloud Tasks when targeting an HTTP endpoint and you need built-in retry with configurable dispatch rates.
- Use Pub/Sub when fan-out (one message, many consumers) is needed — SQS and Cloud Tasks are point-to-point.
- Every queue must have a DLQ — messages that cannot be processed must not be silently dropped.
- All consumers must be idempotent — design for at-least-once delivery even when exactly-once is configured.
- Use cron expressions in UTC — local timezone cron in cloud environments causes DST-related scheduling bugs.

# Output requirements

1. **Queue configuration** — queue name, type, visibility timeout/ack deadline, retry policy, DLQ binding.
2. **Producer code** — message format, enqueue implementation, correlation ID generation.
3. **Consumer/worker code** — handler implementation, concurrency settings, error handling, idempotency mechanism.
4. **Cron configuration** — schedule expression, target endpoint or function, timezone.
5. **Monitoring setup** — queue depth metric, DLQ alert, processing latency dashboard.
6. **Test results** — successful processing confirmed, retry behavior verified, DLQ capture confirmed.

# References

- AWS SQS developer guide: https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/
- GCP Cloud Tasks: https://cloud.google.com/tasks/docs
- BullMQ documentation: https://docs.bullmq.io/
- Celery documentation: https://docs.celeryq.dev/
- Cloud Scheduler: https://cloud.google.com/scheduler/docs
- Crontab guru (expression editor): https://crontab.guru/
- `references/preflight-checklist.md`

# Related skills

- `aws` — SQS, EventBridge, and Lambda trigger configuration.
- `gcp` — Cloud Tasks, Pub/Sub, and Cloud Scheduler setup.
- `serverless-patterns` — event-driven serverless architectures.

# Anti-patterns

- Processing queue messages without idempotency — duplicates cause data corruption or double-sends.
- Not setting a DLQ — failed messages retry forever or are silently dropped.
- Using unbounded concurrency — workers overwhelm downstream databases or APIs.
- Setting visibility timeout shorter than expected processing time — causes duplicate processing as the message becomes visible to other consumers.
- Using local timezone in cron expressions — daylight saving time changes cause missed or double executions.
- Storing large payloads (>256KB) directly in queue messages — store in S3/GCS and pass the reference.
- Polling an empty queue in a tight loop without backoff — wastes compute and API calls.

# Failure handling

- If a worker consistently fails on the same message, check the DLQ for the message body and error details. Fix the handler, then replay the DLQ message.
- If queue depth grows faster than workers can process, scale worker count or increase concurrency. Check for slow downstream dependencies.
- If cron jobs do not fire, verify the schedule expression with crontab.guru, check the scheduler service is enabled, and confirm the target endpoint is reachable.
- If messages are processed more than once, verify the consumer is idempotent. Use a deduplication key (message ID or correlation ID) stored in a database to skip already-processed messages.
- If the task requires a specific provider's queue console or CLI configuration, redirect to the `aws` or `gcp` skill for provider-specific commands.
