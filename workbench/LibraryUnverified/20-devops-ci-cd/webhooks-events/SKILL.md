---
name: webhooks-events
description: Design inbound webhook handling and event delivery flows with signature verification, quick acknowledgement, idempotency, and replay-safe processing. Use this when working on webhook receivers, event envelopes, queue fanout, or provider callback behavior. Do not use for generic REST endpoints that do not receive external event delivery.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: webhooks-events
  maturity: draft
  risk: low
  tags: [webhooks, events, idempotency, delivery]
---

# Purpose

Use this skill to keep webhook handling safe under duplicate delivery, slow downstream processing, and unverifiable payloads.

# When to use this skill

Use this skill when:

- implementing or reviewing inbound webhook receivers from third-party providers
- designing internal event envelopes, replay flow, dead-letter handling, or queue fanout after receipt
- debugging duplicate events, signature mismatches, delayed acknowledgements, or missing replay safety

# Do not use this skill when

- the task is a normal synchronous REST endpoint with no external event delivery semantics
- the main issue is only retry tuning or telemetry, not event receipt and processing flow
- a narrower active skill already owns a different transport, such as websocket session state

# Operating procedure

1. Define the inbound contract.
   Identify provider event types, signature headers, timestamp fields, unique delivery IDs, and acknowledgement expectations before touching handler code.

2. Verify before trust.
   Validate signatures and basic envelope shape before mutating internal state. Treat unverifiable or malformed events as hostile input. Example: Stripe webhooks use `Stripe-Signature` header with HMAC-SHA256 of the raw body. GitHub uses `X-Hub-Signature-256`.

3. Acknowledge quickly, process asynchronously.
   Keep the public receiver small. Persist or enqueue work, return the provider-required acknowledgement, then perform slower business logic off the critical path where possible.

4. Make duplicate delivery safe.
   Use provider delivery IDs or application-level idempotency keys so retries and replay tooling do not create duplicate side effects.

5. Preserve replay and triage paths.
   Capture enough metadata to re-run a delivery, inspect failures, and reconcile missing downstream effects without asking the provider to guess what happened.

# Decision rules

- Signature verification and delivery ID handling are part of correctness, not optional hardening.
- Prefer durable enqueue or persistence before expensive downstream calls when provider timeouts are tight.
- Keep event envelopes versionable and explicit; do not let random payload blobs become the only contract.
- If a provider retries aggressively, assume your receiver will see duplicates and out-of-order arrivals.

# Output requirements

1. `Inbound Contract`
2. `Ack and Processing Flow`
3. `Signature and Replay Controls`
4. `Verification`

# Scripts

- `scripts/webhook_signature.py`: sign or verify webhook payloads with HMAC-SHA256 for local test fixtures and receiver checks.

# References

Read these only when relevant:

- `references/inbound-signature-verification.md`
- `references/delivery-and-idempotency.md`
- `references/replay-debugging-checklist.md`

# Related skills

- `rate-limits-retries`
- `observability-logging`
- `background-jobs-queues`

# Anti-patterns

- Processing webhook payload before verifying the signature.
- Blocking on slow business logic before returning `200` to the provider.
- Storing webhook body in application logs instead of a durable queue.
- Trusting webhook source IP without signature verification.
- No dead-letter or replay mechanism for failed processing.

# Failure handling

- If the provider documentation is incomplete, say exactly which assumptions are inferred and keep the receiver conservative.
- If the current design performs business logic before verification or acknowledgement, call that out as a delivery risk immediately.
- If the real issue is retry budget or telemetry, hand off to the adjacent skill after preserving the webhook-specific constraints.
