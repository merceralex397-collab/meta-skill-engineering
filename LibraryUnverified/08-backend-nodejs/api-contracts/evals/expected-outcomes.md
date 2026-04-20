# Expected Outcomes

A good API Contracts run should:

- identify the affected endpoints, schemas, and client-visible semantics before proposing changes
- classify additive versus breaking changes instead of treating all diffs alike
- discuss error-envelope and status-code compatibility, not just happy-path JSON fields
- recommend versioning, deprecation, or shims only when justified by the actual blast radius
- end with a schema or integration verification step
