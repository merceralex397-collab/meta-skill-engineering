---
name: rate-limits-retries
description: Design retry behavior, backoff budgets, and rate-limit handling with explicit failure classification and idempotency rules. Use this when working on client retries, server throttling, `429` handling, queue redelivery, or external API resilience. Do not use for generic error handling that does not involve throttling or retry policy.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: rate-limits-retries
  maturity: draft
  risk: low
  tags: [rate-limit, retries, backoff, idempotency]
---

# Purpose

Use this skill to stop retry logic from becoming accidental traffic amplification or silent data corruption.

# When to use this skill

Use this skill when:

- classifying which failures should retry, fail fast, or surface directly to callers
- designing exponential backoff, jitter, retry budgets, or `Retry-After` handling
- reviewing webhook consumers, workers, or API clients for idempotency under duplicate delivery or throttling

# Do not use this skill when

- the task is generic exception handling with no throttling, retry, or delivery semantics
- the main issue is observability only, not retry policy
- a narrower active skill already owns the transport surface, such as pure websocket session logic

# Operating procedure

1. Classify the failure surface.
   Separate transient, throttling, dependency-unavailable, permanent validation, and authorization failures before writing retry rules.

2. Check idempotency first.
   Confirm whether the operation can be repeated safely. If not, design idempotency keys, dedupe storage, or compensating actions before increasing retries.

3. Set a bounded retry budget.
   Define attempt count, backoff growth, jitter strategy, and maximum elapsed time. Unbounded retries are operational bugs. Example budget: max 3 attempts, base delay 1s, exponential factor 2x, jitter ±500ms, total max wait 15s.

4. Honor server feedback.
   Use `Retry-After`, provider-specific throttling headers, and queue visibility timeouts where they exist instead of fighting the upstream contract.

5. Verify with failure injection.
   Simulate throttling, dependency timeouts, and duplicate delivery so the policy is proven under pressure rather than assumed from happy-path code.

# Decision rules

- Do not retry validation, auth, or contract errors unless the upstream explicitly says they are transient.
- Prefer full or equal jitter on shared clients to avoid synchronized retry storms.
- Treat retry count, total delay, and duplicate side effects as first-class review points.
- If retries can cross process boundaries, persist idempotency context instead of keeping it only in memory.

# Output requirements

1. `Failure Classes`
2. `Retry and Backoff Plan`
3. `Budget and Idempotency`
4. `Verification`

# Scripts

- `scripts/backoff_budget.py`: calculate retry delay windows and total wait budget for a chosen backoff policy.

# References

Read these only when relevant:

- `references/retry-classification-matrix.md`
- `references/backoff-and-jitter-patterns.md`
- `references/idempotency-and-budgeting.md`

# Related skills

- `observability-logging`
- `webhooks-events`
- `background-jobs-queues`

# Anti-patterns

- Retrying `400 Bad Request` or other permanent client errors.
- Using fixed delay instead of exponential backoff.
- Retrying without an idempotency guarantee on the underlying operation.
- Ignoring `Retry-After` headers returned by the upstream.
- Client-side rate limiting without server coordination.
- Retry budget across the entire service exceeding upstream capacity.

# Failure handling

- If the provider does not document throttling behavior, say that explicitly and keep the policy conservative.
- If the operation is not idempotent yet, stop before adding more retries and fix that design gap first.
- If the real issue is queue delivery or webhook semantics, hand off with the retry policy implications attached.
