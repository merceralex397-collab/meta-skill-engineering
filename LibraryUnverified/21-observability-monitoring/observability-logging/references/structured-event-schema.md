# Structured Event Schema

Aim for a small stable event vocabulary:

- request accepted
- dependency call started or finished
- state transition completed
- retry scheduled
- terminal failure

Useful core fields:

- timestamp
- level
- event
- request_id or correlation_id
- actor_id or tenant_id when appropriate
- resource_id
- duration_ms
- outcome or error_class
- retry_count when relevant

Prefer stable field names and machine-readable values over prose-rich message strings.
