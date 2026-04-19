# Breaking Change Checklist

Treat these as breaking until proven otherwise:

- removing an endpoint or method
- removing a response field
- making an optional field required
- changing field type or semantics
- changing a success or error status code clients branch on
- changing sort, pagination, or filter defaults without compatibility notes

Review flow:

1. list affected clients and automation
2. classify change as additive, breaking, or internal
3. decide on versioning, deprecation, or shim strategy
4. prove the outcome with schema diff plus runtime example

Do not call a change "non-breaking" just because the UI still works.
