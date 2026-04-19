---
name: bloated-inline-example
description: >
  Validates third-party API integrations by running contract tests, load
  simulations, and authentication-flow checks against staging endpoints.
---

# API Integration Testing

## Purpose

Provide a repeatable, comprehensive testing procedure for third-party API
integrations. Covers contract conformance, error handling, rate-limit
behavior, authentication flows, and baseline load performance so that
integrations do not break silently after upstream API changes.

## When to use

- Before promoting an integration from development to staging.
- After a third-party vendor announces a breaking API version change.
- During quarterly integration health audits.

## When NOT to use

- For unit-testing internal service logic — use standard test frameworks.
- For end-to-end UI tests that happen to call APIs — use `e2e-test-runner`.
- For monitoring live production endpoints — use `uptime-monitor`.

## Procedure

1. Load the integration manifest from `integrations/<vendor>/manifest.yaml`.
   The manifest declares base URL, auth method, rate limits, and endpoint
   inventory.

2. Validate the manifest against the schema below:

   ```yaml
   # --- Integration Manifest Schema ---
   # Every field is required unless marked optional.
   vendor_name: string          # Human-readable vendor name
   base_url: string             # Staging base URL
   auth:
     method: enum(oauth2, api_key, basic, mtls)
     token_url: string          # Required for oauth2
     client_id_env: string      # Env var holding client ID
     client_secret_env: string  # Env var holding client secret
     api_key_env: string        # Required for api_key method
     cert_path: string          # Required for mtls
     key_path: string           # Required for mtls
   rate_limits:
     requests_per_second: int
     burst: int                 # optional, defaults to 2x rps
     retry_after_header: bool   # does vendor send Retry-After?
   endpoints:
     - path: string
       method: enum(GET, POST, PUT, PATCH, DELETE)
       request_schema: string   # relative path to JSON Schema
       response_schema: string  # relative path to JSON Schema
       idempotent: bool
       paginated: bool          # optional
       cursor_param: string     # required if paginated
   ```

3. For each endpoint in the manifest, run the contract test suite:

   a. Send a well-formed request using golden-file fixtures located in
      `integrations/<vendor>/fixtures/<endpoint>/`.
   b. Validate the response status code against the expected code list.
   c. Validate the response body against the declared `response_schema`
      using `ajv` with strict mode enabled.
   d. Verify response headers include expected caching and content-type
      values.

4. Run negative-path tests for every endpoint:

   | Test case                         | Input mutation               | Expected result              |
   |-----------------------------------|------------------------------|------------------------------|
   | Missing required field            | Drop first required property | 400 or 422 with error body   |
   | Invalid field type                | String where int expected    | 400 or 422 with error body   |
   | Unauthorized request              | Omit auth header/token       | 401 with WWW-Authenticate    |
   | Forbidden scope                   | Use read-only token on write | 403 with error body          |
   | Resource not found                | Use non-existent ID          | 404 with error body          |
   | Duplicate creation (if !idempot.) | Replay POST with same body   | 409 or idempotent 200        |
   | Payload too large                 | Send body > 1 MB             | 413 with error body          |
   | Unsupported media type            | Send text/plain on JSON ep   | 415 with error body          |
   | Rate limit exceeded               | Exceed declared rps by 3x    | 429 with Retry-After header  |

5. Run authentication flow verification:

   For **OAuth 2.0** integrations:
   ```
   Step 1 — Request token with valid credentials.
            Verify: 200, token_type=Bearer, expires_in > 0.
   Step 2 — Request token with invalid client_secret.
            Verify: 401, error=invalid_client.
   Step 3 — Request token with expired refresh_token.
            Verify: 400, error=invalid_grant.
   Step 4 — Use token to call a protected endpoint.
            Verify: 200, response matches schema.
   Step 5 — Use expired token.
            Verify: 401, then refresh and retry succeeds.
   ```

   For **API key** integrations:
   ```
   Step 1 — Call endpoint with valid key. Verify: 200.
   Step 2 — Call endpoint with revoked key. Verify: 401.
   Step 3 — Call endpoint with no key. Verify: 401.
   Step 4 — Call endpoint with key in wrong header. Verify: 401.
   ```

   For **mTLS** integrations:
   ```
   Step 1 — Call with valid cert/key pair. Verify: 200.
   Step 2 — Call with expired cert. Verify: TLS handshake failure.
   Step 3 — Call with cert signed by wrong CA. Verify: TLS failure.
   ```

6. Run the baseline load simulation:

   Configuration parameters:
   ```
   duration:          60 seconds
   concurrency:       10 virtual users
   ramp_up:           linear over first 10 seconds
   target_rps:        declared rate_limit × 0.8
   success_threshold: p99 latency < 2000ms, error_rate < 1%
   tool:              k6
   ```

   Load test result interpretation table:

   | Metric              | Green           | Yellow            | Red               |
   |---------------------|-----------------|-------------------|-------------------|
   | p50 latency         | < 200ms         | 200–500ms         | > 500ms           |
   | p95 latency         | < 500ms         | 500–1500ms        | > 1500ms          |
   | p99 latency         | < 1000ms        | 1000–2000ms       | > 2000ms          |
   | Error rate          | < 0.1%          | 0.1–1%            | > 1%              |
   | Rate limit hits     | 0               | 1–5               | > 5               |
   | Timeout rate        | 0%              | < 0.5%            | ≥ 0.5%            |

7. Run pagination correctness checks for paginated endpoints:

   a. Fetch page 1 with `limit=2`.
   b. Follow cursor to page 2.
   c. Verify no duplicate IDs between pages.
   d. Verify total count header (if present) matches actual count.
   e. Verify empty page returns empty array, not null.
   f. Verify requesting beyond last page returns empty array.

   Known pagination patterns and their cursor parameter names:

   | Pattern            | Cursor param   | Example vendor APIs          |
   |--------------------|----------------|------------------------------|
   | Offset-based       | offset         | Stripe, Slack                |
   | Cursor-based       | cursor / after | GitHub, Shopify              |
   | Page-number-based  | page           | Jira, Confluence             |
   | Keyset-based       | starting_after | Stripe (list endpoints)      |
   | Token-based        | pageToken      | Google Cloud APIs            |

8. Run timeout and retry behavior validation:

   a. Simulate a 30-second server delay using a proxy stub.
   b. Verify the client times out within the configured timeout window
      (default 10 seconds).
   c. Verify the client retries with exponential backoff for 5xx responses.
   d. Verify the client does NOT retry 4xx responses (except 429).
   e. Verify the client respects the `Retry-After` header on 429 responses.

   Retry policy reference table:

   | Attempt | Delay (base 1s) | Jitter range   | Max delay |
   |---------|-----------------|----------------|-----------|
   | 1       | 1s              | 0–500ms        | 1.5s      |
   | 2       | 2s              | 0–1000ms       | 3s        |
   | 3       | 4s              | 0–2000ms       | 6s        |
   | 4       | 8s              | 0–4000ms       | 12s       |
   | 5 (max) | 16s             | 0–8000ms       | 24s       |

9. Aggregate all results into a test report.

   Report template:
   ```markdown
   # Integration Test Report: <vendor_name>
   **Date:** YYYY-MM-DD
   **Environment:** staging
   **Spec version:** <version>

   ## Summary
   | Category          | Pass | Fail | Skip |
   |-------------------|------|------|------|
   | Contract tests    |      |      |      |
   | Negative-path     |      |      |      |
   | Auth flows        |      |      |      |
   | Load baseline     |      |      |      |
   | Pagination        |      |      |      |
   | Timeout & retry   |      |      |      |

   ## Failures
   <details per failure>

   ## Recommendations
   <prioritized list>
   ```

10. Compare results against the previous run stored in
    `integrations/<vendor>/baseline.json`. Flag any regressions.

   Regression detection thresholds:

   | Metric              | Regression if delta exceeds |
   |---------------------|-----------------------------|
   | p50 latency         | +20%                        |
   | p95 latency         | +30%                        |
   | p99 latency         | +50%                        |
   | Error rate          | +0.5 percentage points      |
   | Contract failures   | any new failure             |

   Status code mapping for regression severity:

   | New failure type    | Severity   | Action required              |
   |---------------------|------------|------------------------------|
   | Contract mismatch   | critical   | Block promotion              |
   | Auth flow failure   | critical   | Block promotion              |
   | Latency regression  | warning    | Investigate before promotion |
   | Pagination issue    | warning    | Investigate before promotion |
   | Retry behavior      | info       | Log and review               |

## Output contract

- `reports/<vendor>/integration-test-YYYY-MM-DD.md` — Full test report in
  the template format shown above.
- `reports/<vendor>/baseline.json` — Updated baseline metrics for future
  regression detection.
- Exit code 0 if all critical tests pass; exit code 1 if any critical test
  fails.

## Failure handling

- If the staging endpoint is unreachable, retry three times with 30-second
  intervals. If still unreachable, mark all tests as SKIP and note the
  connectivity failure in the report.
- If fixture files are missing for an endpoint, skip that endpoint's contract
  tests and log a warning. Do not skip negative-path or auth tests.
- If the load test tool (k6) is not installed, skip load tests and note the
  missing dependency. Do not fail the entire run.

## Next steps

- Feed the test report into `skill-evaluation` for quality scoring.
- If regressions are detected, create remediation tickets via
  `ticket-pack-builder`.
- Schedule re-test after vendor confirms fixes using
  `skill-lifecycle-management`.

## HTTP Status Code Reference

Below is the complete table of HTTP status codes referenced throughout
this skill's procedures and assertions.

| Code | Name                    | Meaning in integration context                    |
|------|-------------------------|---------------------------------------------------|
| 200  | OK                      | Successful response                               |
| 201  | Created                 | Resource created successfully                     |
| 204  | No Content              | Successful delete                                 |
| 301  | Moved Permanently       | Endpoint relocated — update base_url              |
| 302  | Found                   | OAuth redirect                                    |
| 304  | Not Modified            | Cache hit — ETag matched                          |
| 400  | Bad Request             | Malformed request body or params                  |
| 401  | Unauthorized            | Auth failed — token expired or invalid            |
| 403  | Forbidden               | Valid auth but insufficient scope                 |
| 404  | Not Found               | Resource does not exist                           |
| 405  | Method Not Allowed      | Wrong HTTP method for endpoint                    |
| 409  | Conflict                | Duplicate resource creation                       |
| 413  | Payload Too Large       | Request body exceeds vendor limit                 |
| 415  | Unsupported Media Type  | Wrong Content-Type header                         |
| 422  | Unprocessable Entity    | Semantic validation failure                       |
| 429  | Too Many Requests       | Rate limit exceeded                               |
| 500  | Internal Server Error   | Vendor-side failure                               |
| 502  | Bad Gateway             | Vendor infrastructure issue                       |
| 503  | Service Unavailable     | Vendor maintenance or overload                    |
| 504  | Gateway Timeout         | Vendor response too slow                          |

## Common OAuth 2.0 Error Codes

| Error code          | Meaning                                          |
|---------------------|--------------------------------------------------|
| invalid_request     | Malformed authorization request                  |
| invalid_client      | Client authentication failed                     |
| invalid_grant       | Authorization grant is invalid or expired        |
| unauthorized_client | Client not authorized for this grant type        |
| unsupported_grant   | Grant type not supported by authorization server |
| invalid_scope       | Requested scope is invalid or unknown            |

## JSON Schema Validation Error Categories

When `ajv` reports validation errors, they fall into these categories:

| Category            | Example                  | Severity |
|---------------------|--------------------------|----------|
| type_mismatch       | Expected int, got string | error    |
| required_missing    | Missing required field   | error    |
| pattern_violation   | Email field is not email | warning  |
| additional_props    | Unexpected extra field   | info     |
| enum_violation      | Value not in enum list   | error    |
| format_invalid      | Date not ISO 8601        | warning  |
| minimum_violation   | Value below minimum      | error    |
| array_length        | Too few/many items       | warning  |
