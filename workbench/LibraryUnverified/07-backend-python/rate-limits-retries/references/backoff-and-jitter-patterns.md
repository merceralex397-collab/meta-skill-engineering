# Backoff And Jitter Patterns

Useful patterns:

- exponential backoff: delay grows each attempt
- capped backoff: upper bound prevents runaway wait times
- full jitter: randomize across the full window to reduce herd effects
- equal jitter: keep a floor while still spreading retries

Avoid:

- fixed interval retries against a hot provider
- infinite retries without a total elapsed time limit
- synchronized clients all retrying on the same second boundary
