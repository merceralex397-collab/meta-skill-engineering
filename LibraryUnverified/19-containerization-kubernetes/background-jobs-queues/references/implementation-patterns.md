# Implementation Patterns

Concrete patterns for background job and queue systems. Reference these when building or reviewing async job infrastructure.

---

## Celery task patterns (Python)

**Basic shared task with retry:**

```python
from celery import shared_task
from celery.exceptions import SoftTimeLimitExceeded
import requests

@shared_task(
    bind=True,
    max_retries=5,
    default_retry_delay=60,
    soft_time_limit=300,
    acks_late=True,
    reject_on_worker_lost=True,
)
def send_webhook(self, webhook_id: int):
    """Deliver a webhook payload to the registered URL."""
    try:
        webhook = Webhook.objects.get(id=webhook_id)
        response = requests.post(
            webhook.url,
            json=webhook.payload,
            timeout=10,
            headers={"X-Idempotency-Key": str(webhook.idempotency_key)},
        )
        response.raise_for_status()
        webhook.mark_delivered()
    except requests.exceptions.Timeout as exc:
        raise self.retry(exc=exc, countdown=60 * (2 ** self.request.retries))
    except requests.exceptions.HTTPError as exc:
        if exc.response.status_code >= 500:
            raise self.retry(exc=exc, countdown=60 * (2 ** self.request.retries))
        # 4xx errors are permanent — do not retry
        webhook.mark_failed(reason=str(exc))
    except SoftTimeLimitExceeded:
        webhook.mark_failed(reason="Timed out after 300s")
```

**Celery task with idempotency key:**

```python
from django.core.cache import cache

@shared_task(bind=True, max_retries=3)
def charge_payment(self, payment_id: int):
    lock_key = f"charge_payment:{payment_id}"
    if not cache.add(lock_key, "processing", timeout=3600):
        return {"status": "already_processing", "payment_id": payment_id}

    try:
        payment = Payment.objects.select_for_update().get(id=payment_id)
        if payment.status == "charged":
            return {"status": "already_charged", "payment_id": payment_id}

        result = stripe.PaymentIntent.create(
            amount=payment.amount_cents,
            currency=payment.currency,
            idempotency_key=str(payment.idempotency_key),
        )
        payment.status = "charged"
        payment.stripe_payment_intent_id = result.id
        payment.save()
        return {"status": "charged", "payment_id": payment_id}
    except stripe.error.CardError:
        payment.status = "failed"
        payment.save()
        raise  # goes to DLQ after max_retries
    finally:
        cache.delete(lock_key)
```

**Celerybeat schedule configuration:**

```python
# celery.py or settings.py
from celery.schedules import crontab

CELERY_BEAT_SCHEDULE = {
    "cleanup-expired-sessions": {
        "task": "accounts.tasks.cleanup_expired_sessions",
        "schedule": crontab(minute=0, hour="*/6"),  # every 6 hours
        "options": {"expires": 3600},  # skip if not picked up within 1 hour
    },
    "send-daily-digest": {
        "task": "notifications.tasks.send_daily_digest",
        "schedule": crontab(minute=0, hour=8),  # daily at 08:00 UTC
        "options": {"expires": 7200},
    },
    "sync-inventory": {
        "task": "warehouse.tasks.sync_inventory",
        "schedule": 300.0,  # every 5 minutes (interval)
    },
}
```

---

## BullMQ patterns (Node.js / TypeScript)

**Job definition and processor:**

```typescript
import { Queue, Worker, Job } from "bullmq";
import IORedis from "ioredis";

const connection = new IORedis({ host: "localhost", port: 6379, maxRetriesPerRequest: null });

// Define the queue
const emailQueue = new Queue("email", {
  connection,
  defaultJobOptions: {
    attempts: 5,
    backoff: { type: "exponential", delay: 5000 },
    removeOnComplete: { count: 1000 },
    removeOnFail: { count: 5000 },
  },
});

// Enqueue a job
await emailQueue.add("send-welcome", {
  userId: "user_abc123",
  templateId: "welcome_v2",
}, {
  jobId: `welcome-${userId}`,  // idempotency: same jobId won't be added twice
  delay: 5000,  // wait 5 seconds before processing
  priority: 1,  // lower number = higher priority
});

// Define the worker/processor
const emailWorker = new Worker("email", async (job: Job) => {
  const { userId, templateId } = job.data;
  const user = await db.users.findById(userId);
  if (!user) throw new Error(`User ${userId} not found`);

  await emailService.send({
    to: user.email,
    template: templateId,
    variables: { name: user.name },
  });

  return { sent: true, email: user.email };
}, {
  connection,
  concurrency: 10,
  limiter: { max: 100, duration: 60_000 },  // rate limit: 100 jobs/minute
});

emailWorker.on("failed", (job, err) => {
  logger.error({ jobId: job?.id, err: err.message }, "Email job failed");
});

emailWorker.on("completed", (job, result) => {
  logger.info({ jobId: job.id, result }, "Email job completed");
});
```

**BullMQ repeatable (scheduled) jobs:**

```typescript
await emailQueue.add("daily-digest", { type: "digest" }, {
  repeat: { pattern: "0 8 * * *" },  // cron: daily at 08:00
  jobId: "daily-digest",  // prevents duplicate schedules
});
```

---

## Sidekiq worker patterns (Ruby)

```ruby
class SendInvoiceWorker
  include Sidekiq::Worker

  sidekiq_options queue: :billing,
                  retry: 5,
                  dead: true,  # send to dead set after retries exhausted
                  lock: :until_executed  # sidekiq-unique-jobs gem

  sidekiq_retry_in do |count, _exception|
    (count ** 4) + 15 + (rand(10) * (count + 1))  # exponential + jitter
  end

  def perform(invoice_id)
    invoice = Invoice.find(invoice_id)
    return if invoice.sent?  # idempotency guard

    pdf = InvoicePdfGenerator.call(invoice)
    InvoiceMailer.send_invoice(invoice, pdf).deliver_now
    invoice.update!(status: :sent, sent_at: Time.current)
  end
end

# Enqueue
SendInvoiceWorker.perform_async(invoice.id)

# Scheduled
SendInvoiceWorker.perform_at(30.minutes.from_now, invoice.id)
```

---

## Job chaining and workflows

**Celery chain (sequential):**

```python
from celery import chain

workflow = chain(
    fetch_data.s(source_id=42),
    transform_data.s(),
    load_data.s(destination="warehouse"),
)
workflow.apply_async()
```

**Celery chord (fan-out then aggregate):**

```python
from celery import chord

workflow = chord(
    [process_chunk.s(chunk_id=i) for i in range(10)],
    aggregate_results.s()
)
workflow.apply_async()
```

**BullMQ flow (parent-child dependencies):**

```typescript
import { FlowProducer } from "bullmq";

const flowProducer = new FlowProducer({ connection });
await flowProducer.add({
  name: "generate-report",
  queueName: "reports",
  data: { reportId: "rpt_001" },
  children: [
    { name: "fetch-sales", queueName: "data", data: { source: "sales" } },
    { name: "fetch-inventory", queueName: "data", data: { source: "inventory" } },
  ],
});
```

---

## Idempotency key implementation

```python
import hashlib
from django.db import models

class ProcessedJob(models.Model):
    idempotency_key = models.CharField(max_length=64, unique=True, db_index=True)
    result = models.JSONField(null=True)
    processed_at = models.DateTimeField(auto_now_add=True)

def run_idempotent(key: str, func, *args, **kwargs):
    """Execute func only if this key hasn't been processed before."""
    existing = ProcessedJob.objects.filter(idempotency_key=key).first()
    if existing:
        return existing.result

    result = func(*args, **kwargs)
    ProcessedJob.objects.create(idempotency_key=key, result=result)
    return result

# Usage in a task
@shared_task(bind=True, max_retries=3)
def process_order(self, order_id: int):
    key = f"process_order:{order_id}"
    return run_idempotent(key, _do_process_order, order_id)
```

---

## Dead-letter queue processing pattern

```python
# Celery: custom DLQ handler
from celery.signals import task_failure

@task_failure.connect
def handle_task_failure(sender=None, task_id=None, exception=None,
                        args=None, kwargs=None, traceback=None, **kw):
    if sender.request.retries >= sender.max_retries:
        FailedJob.objects.create(
            task_name=sender.name,
            task_id=task_id,
            args=args,
            kwargs=kwargs,
            exception=str(exception),
            traceback=str(traceback),
        )
        alert_ops_team(task_name=sender.name, task_id=task_id, error=str(exception))
```

```typescript
// BullMQ: listen for failed events after all retries
emailWorker.on("failed", async (job: Job, err: Error) => {
  if (job && job.attemptsMade >= (job.opts.attempts ?? 1)) {
    await db.failedJobs.create({
      queueName: "email",
      jobId: job.id,
      data: job.data,
      error: err.message,
      failedAt: new Date(),
    });
    await alerting.notify("dlq", {
      queue: "email",
      jobId: job.id,
      error: err.message,
    });
  }
});
```

---

## Priority queue setup

```python
# Celery: route tasks to priority queues
CELERY_TASK_ROUTES = {
    "payments.tasks.*": {"queue": "critical"},
    "notifications.tasks.*": {"queue": "default"},
    "reports.tasks.*": {"queue": "low"},
}

# Start workers with queue priority
# celery -A proj worker -Q critical,default,low --concurrency=8
```

```typescript
// BullMQ: priority is per-job (lower number = higher priority)
await paymentQueue.add("charge", { paymentId: 123 }, { priority: 1 });
await reportQueue.add("generate", { reportId: 456 }, { priority: 10 });
```

---

Related skills: `rate-limits-retries`, `observability-logging`, `orm-patterns`, `data-model`.
