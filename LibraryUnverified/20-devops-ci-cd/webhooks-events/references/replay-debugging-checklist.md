# Replay Debugging Checklist

When debugging webhook failures, be able to answer:

- which delivery ID failed
- whether signature verification passed
- whether the event was acknowledged
- whether downstream work was enqueued or persisted
- whether a replay is safe and how to trigger it

If any of these are unknowable from the current system, the replay path is under-specified.
