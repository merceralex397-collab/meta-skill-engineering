---
name: security-best-practices
description: "Perform language and framework specific security best-practice reviews and suggest improvements. Trigger only when the user explicitly requests security best practices guidance, a security review/report, or secure-by-default coding help. Trigger only for supported languages (python, javascript/typescript, go). Do not trigger for general code review, debugging, or non-security tasks. Do not use for security-hardening (OWASP Top 10 controls), security-threat-model (threat enumeration), or auth-patterns (auth implementation)."
---

# Purpose
Load language- and framework-specific secure-coding guidance from this skill's `references/` directory, then apply it: write secure-by-default code, passively flag critical vulnerabilities, or produce a prioritized security report with fixes.

# Operating procedure

## 1. Identify languages and frameworks
- Determine ALL primary languages and frameworks in scope (frontend + backend).
- Inspect the repo if unclear; list evidence for your determination.

## 2. Load reference guidance
- Check `references/` for files matching `<language>-<framework>-<stack>-security.md`.
- Also check `<language>-general-<stack>-security.md` for framework-agnostic guidance.
- For web apps with unspecified frontend framework, load `javascript-general-web-frontend-security.md`.
- Read ALL matching reference files for the identified stack.

## 3. Choose operating mode

**Mode A — Secure-by-default coding**: Use loaded guidance to write new code securely. No report needed.

**Mode B — Passive detection**: While working on other tasks, flag critical vulnerabilities and major deviations from guidance. Focus on highest-impact issues only.

**Mode C — Full security report**: When user explicitly requests a review. Produce `security_best_practices_report.md` (or user-specified path). See Output defaults below.

## 4. Decision tree
- Language/framework unclear → inspect repo, list evidence.
- Matching guidance in `references/` → load and follow.
- No matching guidance → use well-known best practices; if generating a report, disclose that concrete guidance is unavailable.

## 5. Apply fixes
- Fix one finding at a time with concise comments explaining the security rationale.
- Check for regressions — insecure code is often load-bearing. Prefer well-informed fixes over quick patches.
- Follow the project's commit and testing workflow. Use clear commit messages referencing the best practice.
- Consider second-order impacts and inform the user before making changes.

# Overrides
Project documentation or prompt files may override specific best practices. When overriding:
- Comply without arguing, but MAY report the override.
- Suggest documenting the override and rationale in the project for future reference.

# General security advice (language-agnostic)

**Public resource IDs**: Use UUID4 or random hex instead of auto-incrementing integers to prevent enumeration.

**TLS / secure cookies**: Do not flag missing TLS as a vulnerability in dev environments. Only set `Secure` cookie flag when TLS is confirmed. Provide an env flag to toggle. Avoid recommending HSTS — it has lasting side-effects (outages, user lockout) and is inappropriate for most codex-scoped projects.

# Output defaults
Report written to `security_best_practices_report.md` (or user-specified path):
```markdown
## Executive Summary
[1–2 sentence overview of findings]

## Critical Findings
| ID | Location (file:line) | Issue | Impact | Severity |
|----|---------------------|-------|--------|----------|
| 1  | auth.py:45          | ...   | ...    | Critical |

## High / Medium / Low Findings
[Same table format, grouped by severity]

## Recommended Fix Order
[Prioritized list with rationale]
```
After writing, summarize findings to user and state report path.

# References
- https://owasp.org/www-project-application-security-verification-standard/
- https://cheatsheetseries.owasp.org/
- Language/framework guidance: `references/<language>-<framework>-<stack>-security.md`

# Failure handling
- **No reference guidance available**: Use general best practices; disclose gap to user. Still flag critical vulnerabilities with high confidence.
- **Language/framework not identifiable**: Ask user to clarify or inspect package manifests, CI config, and file extensions for evidence.
- **Report too large**: Focus on critical and high severity findings first. Offer to expand to medium/low in a follow-up.
- **Fixes cause regressions**: Roll back, document the issue, and discuss with user before retrying. Do not force security fixes that break functionality.
- **User overrides a best practice**: Comply, document the override, and move on.
