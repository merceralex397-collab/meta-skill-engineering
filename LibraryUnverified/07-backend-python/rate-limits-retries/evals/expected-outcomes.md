# Expected Outcomes

A good Rate Limits Retries run should:

- classify transient, throttling, and permanent failures separately
- define a bounded retry budget instead of vague "add exponential backoff"
- surface idempotency and duplicate-side-effect risks explicitly
- account for Retry-After or equivalent upstream hints where available
- end with a failure-injection or replay verification step
