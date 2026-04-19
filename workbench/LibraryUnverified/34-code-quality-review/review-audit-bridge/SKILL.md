---
name: review-audit-bridge
description: "Structure repo review, implementation audit, security review, and QA passes so they produce short, actionable findings rather than vague commentary. Use when the user says 'review this PR', 'audit implementation', 'security review', 'QA pass', or when Codex/OpenCode needs a deterministic review sequence after planning or implementation. Do not use for planning (task decomposition), process-doctor (workflow repair), or prompt-crafting (improving prompts)."
---

# Purpose
Structures code review, security review, QA, and implementation audit passes to produce short, actionable findings rather than vague commentary. Separates signal from noise—only surfaces issues that genuinely matter: bugs, security vulnerabilities, logic errors, architectural violations. Never comments on style, formatting, or trivial matters.

# When to use this skill
Use when:
- Reviewing a PR before merge
- Conducting a security audit of new code
- Running QA on completed implementation
- Validating that implementation matches specification

Do NOT use when:
- Writing new code (that's implementation)
- Fixing issues (that's implementation after review)
- The code hasn't been written yet

# Operating procedure

## 1. Define review scope
```markdown
## Review Scope
- Type: [code-review | security-audit | qa-validation | spec-compliance]
- Target: [PR #X | branch | commit range | directory]
- Focus: [specific areas of concern, if any]
```

## 2. Check in priority order
Review in this sequence — stop and report critical findings immediately:
1. **Correctness**: Does the code do what the spec/ticket says? Logic errors, off-by-one, wrong return values
2. **Regressions**: Does new code break existing behavior? Check callers, dependents, integration points
3. **Security**: Injection, auth bypass, secrets exposure, unsafe deserialization
4. **Tests**: Are critical paths covered? Do tests actually assert the right things?
5. **Maintainability**: Readability, coupling, duplication (flag only if severe)

Findings come first — no praise or summaries before the findings section.

## 3. Apply severity classification
Only flag issues at these levels:

| Severity | Criteria | Blocks Merge? |
|----------|----------|---------------|
| **Critical** | Security vulnerability, data loss risk, crashes | Yes |
| **Major** | Incorrect behavior, logic errors, broken functionality | Yes |
| **Minor** | Edge case bugs, performance issues (non-blocking) | No, but track |
| **Note** | Suggestions, alternatives, questions | No |

**Explicitly ignore:**
- Formatting (that's linter's job)
- Naming style (unless misleading)
- Comment style
- Import ordering
- Line length

## 4. Require evidence for every finding
Each finding must have:
```markdown
### Finding: [Short title]
**Severity:** Major
**Location:** `src/auth/validate.ts:45-52`

**Issue:**
[Specific description of what's wrong]

**Evidence:**
[Code snippet or test case that demonstrates the problem]

**Suggested Fix:**
[Concrete fix, not vague advice]
```

Bad finding (reject):
> "This function could be improved"

Good finding:
> "validate() returns true for empty strings, bypassing validation. Evidence: `validate("") === true`. Fix: Add `if (!input) return false;` at line 46."

## 5. Triage findings against actual impact
Before finalizing, verify each finding:
- [ ] Is this actually a bug, or expected behavior?
- [ ] Does this affect production code paths?
- [ ] Is there existing mitigation (other code, tests)?
- [ ] Would fixing this break something else?

Drop findings that fail triage.

## 6. Produce structured report
```markdown
# Review Report: [PR/Branch/Scope]
Reviewer: [agent-id or human]
Date: [ISO date]

## Summary
- Critical: N
- Major: N
- Minor: N
- Notes: N
- **Verdict:** [APPROVE | REQUEST_CHANGES | NEEDS_DISCUSSION]

## Blocking Issues
[Only Critical and Major]

### 1. [Title]
- Severity: Critical
- Location: [file:line]
- Issue: [description]
- Evidence: [proof]
- Fix: [suggestion]

## Non-Blocking Issues
[Minor and Notes, for tracking]

## What Looks Good
[Briefly note well-implemented areas—builds trust]
```

## 7. Handle disputed findings
If implementation author contests a finding:
1. Re-verify evidence
2. If evidence still holds, escalate to second reviewer
3. If evidence disproven, drop finding with note
4. Document resolution either way

## 8. Generate follow-up tickets (if applicable)
For non-blocking issues that should be tracked:
```markdown
## Follow-up Tickets Recommended
- TKT-XXX: [Minor issue to address later]
- TKT-YYY: [Tech debt noted during review]
```

# Output defaults
```markdown
# Review Report

## Verdict: [APPROVE | REQUEST_CHANGES]

## Blocking (N)
[List of Critical/Major findings]

## Non-Blocking (N)
[List of Minor/Note findings]

## Follow-up Tickets
[List of tickets to create]
```

# References
- GitHub PR review workflow: https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/about-pull-request-reviews
- Review statuses: Comment (feedback only), Approve (ship it), Request Changes (must fix)

# Failure handling
- **No issues found**: Return "APPROVE" with brief note on what was reviewed, not an empty report
- **Cannot determine if bug**: Flag as "NEEDS_DISCUSSION" with specific question, don't guess
- **Review scope too large**: Break into focused passes (security, logic, API contract) rather than one giant review
- **Conflicting reviewer opinions**: Escalate to third reviewer or author for tiebreak
