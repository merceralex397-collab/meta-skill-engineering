# Trace, Metric, And Log Triage

Use each signal for a distinct job:

- logs: explain a specific event with context
- metrics: show rate, latency, error ratios, and saturation
- traces: connect one request or job across service boundaries

Quick rule of thumb:

- if you need to know "how often," use a metric
- if you need to know "what happened here," use a log
- if you need to know "where did this request go," use a trace

Missing one layer does not block progress, but be explicit about the gap.
