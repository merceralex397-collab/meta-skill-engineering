---
name: observability-logging
description: Instrument backend services with structured logs, metrics, traces, and correlation IDs that answer production questions without leaking secrets. Use this when defining event schemas, trace boundaries, service diagnostics, or alert-friendly log fields. Do not use for one-off debug prints or frontend analytics instrumentation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: observability-logging
  maturity: draft
  risk: low
  tags: [observability, logging, tracing, metrics]
---

# Purpose

Use this skill to make backend behavior explainable in production instead of merely noisy.

# When to use this skill

Use this skill when:

- adding or reviewing structured logging, trace spans, metrics, correlation IDs, or error-reporting fields
- debugging why operators cannot answer latency, failure, throughput, or replay questions from the current telemetry
- deciding what a service, worker, or webhook handler should emit for production triage

# Do not use this skill when

- the task is just temporary local debugging with no intention to keep the instrumentation
- the work is frontend analytics or product event tagging rather than backend runtime visibility
- a narrower active skill already owns the transport problem and observability is only incidental

# Operating procedure

1. Start from operator questions.
   Define what someone on call must be able to answer: what failed, for whom, where it happened, how often, and whether it is still happening.

2. Choose the primary event vocabulary.
   Name stable event types for request lifecycle, external dependency calls, state transitions, retries, and terminal failures instead of free-form prose logs. Example tools: Python `structlog`, Node.js `pino`, Go `slog`/`zap`. Event names like `http.request.completed`, `db.query.slow`, `job.retry.exhausted`.

3. Separate logs, metrics, and traces by job.
   Logs explain specific events, metrics show rates and saturation, and traces connect one request or job across boundaries. Do not expect one signal type to carry all three jobs.

4. Control field quality.
   Keep correlation IDs, actor identifiers, resource identifiers, status, duration, and retry counts explicit. Avoid unbounded-cardinality fields or secret-bearing payload dumps.

5. Verify on unhappy paths.
   Trigger at least one failure or retry path and confirm that the emitted events are searchable, redacted, and correlated well enough to debug the incident.

# Decision rules

- Prefer structured event names and fields over giant interpolated message strings.
- Log enough identifiers to join events, but never raw secrets, tokens, full auth headers, or unnecessary personal data.
- Treat missing correlation IDs and missing error class fields as observability bugs, not polish issues.
- If an event cannot drive a dashboard, alert, trace, or incident timeline, question whether it belongs in the long-term schema.

# Output requirements

1. `Primary Questions`
2. `Event and Field Plan`
3. `Redaction and Cardinality Risks`
4. `Verification`

# Scripts

- `scripts/log_schema_check.py`: validate sample JSON logs for required fields and obvious secret-bearing keys.

# References

Read these only when relevant:

- `references/structured-event-schema.md`
- `references/pii-and-secrets-redaction.md`
- `references/trace-metric-log-triage.md`

# Related skills

- `rate-limits-retries`
- `webhooks-events`
- `background-jobs-queues`

# Anti-patterns

- Using `log.debug`/`log.info` with f-strings instead of structured fields.
- Logging full request/response bodies in production, risking PII and secret exposure.
- Using `print()` instead of a structured logger.
- Metrics with unbounded label cardinality (e.g. `user_id` as a metric label).
- Trace spans wrapping everything instead of meaningful operation boundaries.

# Failure handling

- If the repo already ships logs in a legacy shape, describe the migration path before forcing a clean-schema rewrite.
- If traces or metrics are unavailable in the stack, say so and design the best log-first fallback instead of pretending all three layers exist.
- If the real issue is contract or delivery logic, hand off to the relevant backend skill after documenting the observability gap.
