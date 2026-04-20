# Delivery And Idempotency

Assume providers retry. Design for it.

Safe receiver rules:

- use delivery ID or event ID as a dedupe key
- acknowledge quickly once the event is durably recorded or enqueued
- keep slow side effects out of the public receiver path
- make downstream consumers idempotent too when retries can cross boundaries

If no provider delivery ID exists, create an application-level dedupe strategy and document its limits.
