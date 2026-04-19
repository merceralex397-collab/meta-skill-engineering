# Validation And Response Shapes

FastAPI gets sloppy when request and response boundaries are implicit.

Use these rules:

- validate inbound payloads with request models instead of loose dict access
- use `response_model` or typed return models for outbound payloads
- do not expose ORM-only fields, internal enum names, or nullable implementation details unless the contract requires them
- normalize error payloads so clients can depend on status code plus a stable body shape

Model choices:

- request model: what clients may send
- response model: what clients are guaranteed to receive
- internal domain object: what business logic uses internally

Avoid:

- one giant model reused for create, update, read, and internal storage
- optional-everything update models without explicit patch semantics
- returning different shapes from the same endpoint based on branchy handler code

When changing a field:

1. decide whether the change is additive, breaking, or merely internal
2. update examples or generated schema output
3. verify at least one route test that serializes the changed model
