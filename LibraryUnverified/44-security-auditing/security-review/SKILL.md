---
name: security-review
description: >
  Read-only security audit covering OWASP Top 10, secrets exposure, input validation, and dependency vulnerabilities.
  Trigger — "security review", "check for vulnerabilities", "audit auth", "find secrets in code",
  "OWASP scan", "check for SQL injection", "review input validation".
  Skip — task requires fixing vulnerabilities (hand off to implementer), performance testing, or UI-only changes.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: security-review
  maturity: draft
  risk: low
  tags: [security, review]
---

# Purpose

Perform a structured, read-only security review of a codebase. Identify vulnerabilities
mapped to OWASP Top 10 categories, detect hardcoded secrets, audit input validation and
auth/authz flows, scan dependency manifests for known CVEs, and flag injection-prone patterns
(SQLi, XSS, command injection). Produce a severity-ranked findings report.

# When to use

- Before a release or merge of security-sensitive code.
- A PR touches auth, input handling, database queries, or API boundaries.
- A new dependency is added and needs vetting.
- A ticket requests a threat model or attack surface analysis.
- Onboarding a codebase that has never had a formal security review.

# Do NOT use when

- The task is to fix a known vulnerability — use an implementer skill for remediation.
- The concern is purely about code style or formatting.
- Performance or load-testing is the goal, not security.
- The review scope is limited to UI/CSS with no data handling.

# Operating procedure

1. Run `grep -rn 'password\|secret\|api_key\|token\|AWS_\|PRIVATE_KEY' --include='*.{ts,js,py,go,java,yml,yaml,json,env}' .` to detect hardcoded secrets and credentials.
2. Search for SQL injection patterns: run `grep -rn 'execute\|raw\|query\|cursor\|sql' --include='*.py' --include='*.ts' --include='*.js' .` and check if queries use parameterised bindings.
3. Search for XSS patterns: run `grep -rn 'innerHTML\|dangerouslySetInnerHTML\|v-html\|document\.write\|\$sce\.trustAsHtml' .` and verify output encoding.
4. Audit authentication flows: locate auth middleware/modules with `grep -rl 'auth\|login\|session\|jwt\|bearer' --include='*.{ts,js,py,go}' .` and trace token validation, expiry, and refresh logic.
5. Check authorisation: for each protected endpoint, verify that role/permission checks exist before data access.
6. Inspect dependency manifests for known vulnerabilities: list all dependencies and check for any `npm audit`, `pip audit`, `cargo audit`, or equivalent tooling configuration.
7. Review CORS configuration: search for `Access-Control-Allow-Origin`, `cors(`, or equivalent and verify origins are not wildcarded in production configs.
8. Check for insecure cryptographic usage: search for `md5\|sha1\|DES\|ECB` and flag any non-deprecated usage.
9. Verify rate limiting and abuse prevention: search for rate-limit middleware or configuration.
10. Compile all findings into the output format below, assigning each a severity (Critical / High / Medium / Low / Info) and OWASP category.

# Decision rules

- Classify severity by exploitability × impact: Critical = remotely exploitable + data breach; Info = theoretical only.
- A hardcoded secret in any non-test file is always High or Critical.
- Missing input validation on user-facing endpoints is at least Medium.
- Flag dependency vulnerabilities only if the vulnerable code path is reachable or cannot be confirmed unreachable.
- When in doubt between two severity levels, pick the higher one and note the uncertainty.

# Output requirements

1. **Findings Table** — `| # | Finding | Severity | OWASP Category | File:Line | Recommendation |`
2. **Secrets Scan Summary** — count of potential secrets found, with file locations.
3. **Auth/Authz Flow Diagram** — text description of token lifecycle and permission checks.
4. **Dependency Risk List** — flagged packages with known CVEs or missing audit tooling.
5. **Attack Surface Map** — list of entry points (endpoints, CLI args, file uploads) and their validation status.
6. **Remediation Priority** — ordered list of top 5 items to fix first.

# References

- OWASP Top 10 (2021): https://owasp.org/Top10/
- CWE/SANS Top 25: https://cwe.mitre.org/top25/
- Project dependency manifests and lock files
- CI/CD security scanning configuration if present

# Related skills

- `repo-evidence-gathering` — provides the file inventory this skill audits
- `ticket-creator` — converts findings into remediation tickets
- `shell-inspection` — verifies runtime security configuration (TLS, permissions)
- `code-review` — general code quality review (non-security-focused)

# Failure handling

- If a file type is unrecognised, skip it but log the path in a "skipped files" appendix.
- If grep results exceed 200 matches for a pattern, sample the first 30 and note the total count.
- If no auth module is found, explicitly state "no authentication layer detected" as a Critical finding.
- If dependency audit tools are unavailable, list unverified packages and recommend installing audit tooling.
