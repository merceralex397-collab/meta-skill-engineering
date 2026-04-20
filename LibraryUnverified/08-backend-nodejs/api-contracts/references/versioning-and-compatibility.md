# Versioning And Compatibility

Use versioning only when compatibility shims or additive changes are not enough.

Prefer, in order:

1. additive fields and endpoints
2. tolerant readers and dual-format support
3. deprecation window with docs and telemetry
4. explicit version split

Before versioning:

- confirm which clients actually depend on the old behavior
- estimate how long both paths must coexist
- define the sunset signal and owner

When keeping one version:

- document default semantics
- update fixtures, SDKs, or generated schema together
- avoid silent interpretation changes hidden behind the same field names
