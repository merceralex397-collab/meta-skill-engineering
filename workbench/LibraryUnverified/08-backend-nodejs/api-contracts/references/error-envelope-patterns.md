# Error Envelope Patterns

Clients need stable failure contracts too.

Prefer an error body shape that makes branching possible without string scraping:

- machine-readable code
- human-readable message
- optional field or path details for validation failures
- correlation or request ID when the platform exposes one

Good rules:

- keep validation failures distinct from auth failures and server failures
- do not overload one status code for unrelated failure classes
- do not leak stack traces or raw provider messages to clients unless the contract explicitly allows it

If the codebase already has an envelope, preserve it consistently instead of adding a second one.
