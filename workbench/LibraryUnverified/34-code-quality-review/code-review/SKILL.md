---
name: code-review
description: Perform a findings-first code review that prioritizes bugs, regressions, missing tests, unsafe changes, and misleading assumptions over summary fluff. Use when the user asks for a review of code, a patch, a PR, or a diff, especially when they want concrete findings with file references and severity ordering. Do not use for general architecture discussion with no code surface.
---

# Purpose

Use this skill to review code as a risk-finding exercise, not a style recital.

# Operating procedure

1. Read the changed surface and infer the intended behavior.
2. Check correctness first, then regressions, then missing tests, then maintainability.
3. Prefer concrete findings with proof over general commentary.
4. Only summarize after the findings.

# Decision rules

- Lead with findings, ordered by severity.
- If there are no findings, say that explicitly and mention residual risk or test gaps.
- Do not waste output on praise or a changelog.

# Output requirements

Return:

1. `Findings`
2. `Open Questions`
3. `Residual Risk`

# References

- Read `references/finding-severity.md` for ordering.
- Read `references/review-checklist.md` for the default pass sequence.
- Read `references/no-finding-output.md` for the zero-bug case.
