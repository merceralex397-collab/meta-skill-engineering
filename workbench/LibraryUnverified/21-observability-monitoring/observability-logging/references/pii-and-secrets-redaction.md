# PII And Secrets Redaction

Never log these by default:

- raw auth headers
- bearer tokens
- passwords
- API keys
- full payment or identity documents
- unredacted webhook payloads that contain user data

Safer patterns:

- log hash or prefix, not the whole secret
- log subject IDs, not the full profile blob
- redact before serialization, not after ingestion
- treat log pipelines and vendor exporters as untrusted for secret storage

If you are unsure whether a field is sensitive, default to excluding it.
