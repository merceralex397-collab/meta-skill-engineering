# Idempotency And Budgeting

Before increasing retry count, answer:

- can the operation be repeated safely
- what key or dedupe record identifies the logical operation
- how much total time is acceptable before surfacing failure
- what happens if the final response is lost after the side effect already happened

Retry budget should include:

- maximum attempts
- cumulative wait time
- visibility into duplicate delivery
- clear terminal failure behavior
