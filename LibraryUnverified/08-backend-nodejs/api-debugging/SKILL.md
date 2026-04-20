---
name: api-debugging
description: >
  Diagnoses API failures including request/response mismatches, schema errors, auth failures, and transport problems.
  Trigger — "debug this API call", "why is this endpoint returning 500", "fix CORS error",
  "API auth not working", "request timeout", "schema validation failing", "curl shows wrong response".
  Skip — pure frontend styling issues, database migration tasks, or API design/planning (use api-contracts).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: api-debugging
  maturity: draft
  risk: low
  tags: [api, debugging]
---

# Purpose

Systematically diagnose API failures by triaging HTTP status codes, diffing expected vs actual
request/response payloads, validating schemas, inspecting auth token flows, debugging CORS
configuration, diagnosing timeouts, and reproducing issues with curl commands. Produce a
root-cause analysis with a verified fix path.

# When to use

- An API endpoint returns an unexpected status code or response body.
- A client receives CORS errors when calling the backend.
- Authentication or authorisation tokens are rejected unexpectedly.
- Request payloads pass client validation but fail server-side schema checks.
- An endpoint times out intermittently or under specific conditions.
- A webhook or callback is not being received by the target.

# Do NOT use when

- The issue is purely frontend rendering or CSS — no API involvement.
- The task is to design a new API — use `api-contracts` for design work.
- The problem is a database migration or schema evolution — use a data-model skill.
- The endpoint works correctly but is slow — use a performance optimisation skill.

# Operating procedure

1. Reproduce the failure: construct a minimal `curl` command that demonstrates the issue, including method, headers, auth token, and body. Run it and capture the full response with `curl -v -X <METHOD> -H 'Content-Type: application/json' -H 'Authorization: Bearer <token>' -d '<body>' <url> 2>&1`.
2. Triage by HTTP status code:
   - **4xx**: Read the response body for error details. Check request headers, auth token expiry, and payload shape.
   - **5xx**: Search server logs with `grep -i 'error\|exception\|traceback' <logfile> | tail -30` or check `docker logs <container> --tail 50`.
   - **Timeout/No response**: Check if the service is running (`ss -tlnp | grep <port>`) and test connectivity (`curl -sS -o /dev/null -w '%{http_code}' http://localhost:<port>/health`).
3. Diff the expected vs actual response: compare the documented/expected response schema against the actual response using `diff <(echo '<expected>') <(echo '<actual>')` or a JSON diff tool.
4. Validate the request payload against the API schema: locate the schema definition (OpenAPI/Swagger file, Zod schema, Pydantic model, or JSON Schema) with `find . -name '*.yaml' -o -name '*.json' | xargs grep -l 'paths\|openapi\|schema'` and check each field.
5. Inspect auth flow: decode the JWT token with `echo '<token>' | cut -d. -f2 | base64 -d 2>/dev/null` and verify issuer, audience, expiry, and scopes match the endpoint requirements.
6. Debug CORS: check server CORS config by searching `grep -rn 'cors\|Access-Control' --include='*.{ts,js,py,go,yaml}' .` and verify allowed origins, methods, and headers match the client's request.
7. Check middleware chain: trace the request path through middleware by reading route definitions and middleware registration order — list each middleware and its effect.
8. Verify environment configuration: check that environment variables for the API (DATABASE_URL, API_KEY, SERVICE_URL) are set and not empty with `env | grep -iE 'database|api|service|redis|port'`.
9. Test the fix: after identifying the root cause, construct a corrected curl command or code change and verify the response matches expectations.
10. Document the root cause, the fix, and a regression test suggestion in the output format below.

# Decision rules

- Always reproduce with curl before reading code — the actual HTTP exchange is ground truth.
- If the status code is 401/403, check auth before anything else — 90% of these are token issues.
- If the response body is empty, check Content-Type headers and serialisation middleware.
- If CORS fails, the fix is always server-side — never disable CORS on the client.
- If logs show no evidence of the request arriving, the problem is routing, DNS, or network — not application code.
- Prefer the simplest explanation: misconfigured env var > code bug > infrastructure issue.

# Output requirements

1. **Reproduction Command** — exact curl command that demonstrates the failure.
2. **Status Code Triage** — what the code means in this context.
3. **Request/Response Diff** — side-by-side comparison of expected vs actual.
4. **Root Cause** — one-paragraph explanation with file paths and line numbers.
5. **Fix** — specific code change or configuration update with before/after.
6. **Regression Test** — suggested test case to prevent recurrence.
7. **Related Endpoints** — list any other endpoints likely affected by the same root cause.

# References

- OpenAPI/Swagger specs if present in the repo
- Server framework documentation (Express, FastAPI, Gin, etc.)
- Auth provider documentation (Auth0, Firebase Auth, Cognito, etc.)
- MDN HTTP Status Code reference: https://developer.mozilla.org/en-US/docs/Web/HTTP/Status

# Related skills

- `api-contracts` — for API design and schema definition issues
- `shell-inspection` — for verifying service and environment state
- `security-review` — for auth-related vulnerabilities discovered during debugging
- `data-model` — for database-related API failures

# Failure handling

- If the API is not running locally, check for Docker containers or remote URLs and adjust reproduction commands accordingly.
- If logs are inaccessible, note the gap and recommend log access setup, then proceed with black-box debugging from curl output alone.
- If the auth token cannot be obtained (e.g., requires OAuth flow), document the auth prerequisites and test with a manually provided token.
- If the root cause spans multiple services, document the cross-service interaction and identify which service owns the fix.
