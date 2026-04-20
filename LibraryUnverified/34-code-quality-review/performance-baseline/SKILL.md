---
name: performance-baseline
description: Establishes measurable performance baselines, regression thresholds, and CI gates using concrete profiling and benchmarking tools. Trigger on "performance baseline", "benchmark", "latency regression", "profiling", "load test". Do NOT use for deployment-pipeline (CI setup) or external-api-client (API timeouts).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: performance-baseline
  maturity: draft
  risk: low
  tags: [performance, baseline]
---

# Purpose
Establish concrete, reproducible performance baselines so regressions are caught before they reach production. This skill covers what to measure, which tools to use, how to set thresholds, and how to integrate performance gates into CI.

# When to use this skill
Use when:
- Setting up performance benchmarks for a new service or CLI tool
- Adding a CI performance gate to prevent regressions
- Investigating a reported latency or throughput regression
- Profiling before optimizing (measure first, then fix)

Do NOT use when:
- Fixing a known performance bug (just fix it, then verify against baseline)
- Setting up deployment infrastructure (use deployment-pipeline)
- Tuning API client timeouts (use external-api-client)

# Operating procedure

## 1. Define what to measure

Choose metrics appropriate to the component type:

| Component | Key Metrics |
|-----------|-------------|
| HTTP API | p50, p95, p99 latency; requests/sec throughput; error rate under load |
| CLI tool | Wall-clock execution time; startup time; memory peak |
| Library function | Ops/sec; allocation rate; time per iteration |
| Frontend | Largest Contentful Paint (LCP); Time to Interactive (TTI); bundle size |

**Rule**: Always measure p95 and p99, not just average. Averages hide tail latency that real users experience.

## 2. Establish baseline measurements

### CLI / script benchmarks — hyperfine
```bash
# Benchmark a CLI command with statistical rigor
hyperfine --warmup 3 --min-runs 10 \
  'my-tool process input.dat' \
  --export-json baseline.json

# Compare two implementations
hyperfine --warmup 3 \
  'my-tool-v1 process input.dat' \
  'my-tool-v2 process input.dat'
```

### HTTP API benchmarks — k6
```javascript
// load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 },   // Ramp up
    { duration: '1m',  target: 50 },   // Sustain
    { duration: '10s', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<200', 'p(99)<500'],  // ms
    http_req_failed: ['rate<0.01'],                   // <1% errors
  },
};

export default function () {
  const res = http.get('http://localhost:3000/api/endpoint');
  check(res, { 'status 200': (r) => r.status === 200 });
  sleep(1);
}
```
```bash
k6 run load-test.js --out json=baseline-results.json
```

### Python benchmarks — pytest-benchmark
```python
# test_performance.py
def test_parse_large_file(benchmark):
    data = open("fixtures/large-input.json").read()
    result = benchmark(parse_document, data)
    assert result is not None

# Run: pytest test_performance.py --benchmark-json=baseline.json
```

### Rust benchmarks — criterion
```rust
// benches/parsing.rs
use criterion::{criterion_group, criterion_main, Criterion};

fn bench_parse(c: &mut Criterion) {
    let input = include_bytes!("../fixtures/sample.bin");
    c.bench_function("parse_replay", |b| {
        b.iter(|| parse_replay(input))
    });
}
criterion_group!(benches, bench_parse);
criterion_main!(benches);
```
```bash
cargo bench -- --save-baseline main
```

## 3. Set regression thresholds

Define concrete pass/fail criteria:

```yaml
# .github/performance-thresholds.yml (example config)
thresholds:
  api_p95_latency_ms: 200       # Must stay under 200ms
  api_p99_latency_ms: 500
  api_throughput_rps: 1000       # Must sustain 1000 req/s
  cli_execution_time_s: 2.5     # Must complete in 2.5s
  memory_peak_mb: 256            # Must not exceed 256MB
  regression_tolerance: 0.10     # 10% regression = failure
```

**Rule**: Set thresholds from actual baseline + tolerance. A 10% regression tolerance is a reasonable default. For latency-sensitive paths, use 5%.

## 4. Add to CI as performance gate

### GitHub Actions example
```yaml
# .github/workflows/perf.yml
name: Performance Gate
on: [pull_request]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Run benchmarks
        run: |
          hyperfine --warmup 3 --min-runs 10 \
            'cargo run --release -- process fixtures/input.dat' \
            --export-json current.json

      - name: Compare against baseline
        run: |
          python scripts/compare-perf.py \
            --baseline perf/baseline.json \
            --current current.json \
            --threshold 0.10
```

### Comparison script pattern
```python
# scripts/compare-perf.py
import json, sys

baseline = json.load(open(sys.argv[1]))
current = json.load(open(sys.argv[2]))
threshold = float(sys.argv[3])

baseline_mean = baseline["results"][0]["mean"]
current_mean = current["results"][0]["mean"]
regression = (current_mean - baseline_mean) / baseline_mean

if regression > threshold:
    print(f"FAIL: {regression:.1%} regression (threshold: {threshold:.0%})")
    print(f"  baseline: {baseline_mean:.3f}s → current: {current_mean:.3f}s")
    sys.exit(1)
print(f"PASS: {regression:.1%} change (within {threshold:.0%} threshold)")
```

## 5. Profile before optimizing

**Rule**: Never optimize without profiling data. Intuition about bottlenecks is wrong more often than right.

### Python — py-spy
```bash
# Sample a running process
py-spy record -o profile.svg -- python my_service.py
# Top-like live view
py-spy top -- python my_service.py
```

### Rust / C / system — perf
```bash
# Record CPU profile
perf record -g ./target/release/my-tool process input.dat
perf report
# Generate flamegraph
perf script | stackcollapse-perf.pl | flamegraph.pl > flame.svg
```

### Node.js — built-in profiler + clinic
```bash
# V8 CPU profile
node --prof app.js
node --prof-process isolate-*.log > profile.txt
# Clinic.js for automated diagnosis
npx clinic doctor -- node app.js
```

### Browser / frontend — Chrome DevTools
1. Open DevTools → Performance tab
2. Record page load or interaction
3. Look for long tasks (>50ms), layout thrash, excessive JS execution
4. Use Lighthouse for automated scoring

## 6. Maintain baselines over time
- **Update baselines on intentional changes**: When you deliberately change performance characteristics (e.g., added encryption), update the baseline
- **Store baselines in version control**: Keep `perf/baseline.json` (or equivalent) committed alongside code
- **Review baseline drift quarterly**: Gradual 1% regressions compound; re-baseline only when drift is understood

# Output defaults
```markdown
## Performance Baseline: [Component]

### Metrics
| Metric | Baseline | Threshold | Tool |
|--------|----------|-----------|------|
| p95 latency | 145ms | <200ms | k6 |
| p99 latency | 320ms | <500ms | k6 |
| Throughput | 1,200 rps | >1,000 rps | k6 |
| CLI exec time | 1.8s | <2.5s | hyperfine |
| Memory peak | 180MB | <256MB | /usr/bin/time -v |

### CI Gate
- Runs on: every PR
- Comparison: against `perf/baseline.json`
- Regression tolerance: 10%

### Next Steps
- [ ] Baseline captured and committed
- [ ] CI workflow added
- [ ] Threshold reviewed with team
```

# References
- hyperfine (CLI benchmarking): https://github.com/sharkdp/hyperfine
- k6 (HTTP load testing): https://grafana.com/docs/k6/latest/
- py-spy (Python profiler): https://github.com/benfred/py-spy
- pytest-benchmark: https://pytest-benchmark.readthedocs.io/
- Criterion.rs (Rust benchmarks): https://bheisler.github.io/criterion.rs/book/
- Brendan Gregg's flamegraph tools: https://github.com/brendangregg/FlameGraph

# Failure handling
- **Noisy benchmarks in CI**: Use dedicated runners or `--min-runs` with statistical outlier detection; avoid shared CI runners for latency-sensitive benchmarks
- **Baseline does not exist yet**: Run benchmarks on `main`, commit results as `perf/baseline.json`, then enable the CI gate
- **Regression is real but intentional**: Update the baseline with a comment explaining why (e.g., "added TLS — +15ms expected")
- **Profile shows no single hotspot**: Look for death-by-a-thousand-cuts — many small allocations, excessive syscalls, or serialization overhead; use allocation profilers (e.g., `tracemalloc`, `dhat`)
- **Benchmark results vary across machines**: Normalize by recording hardware specs alongside results; compare only same-runner results in CI
