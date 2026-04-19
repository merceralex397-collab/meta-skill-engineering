# Retry Classification Matrix

Classify errors before you retry them:

- validation or contract error: fail fast
- auth or permission error: fail fast unless token refresh is a separate explicit step
- upstream 429 or throttling: retry with provider hints and bounded backoff
- timeout or connection reset: usually retryable if the operation is idempotent
- unknown 5xx: retry conservatively and observe

Document the mapping instead of letting each caller invent its own rules.
