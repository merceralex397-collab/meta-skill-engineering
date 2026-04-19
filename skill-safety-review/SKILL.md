---
name: skill-safety-review
description: >-
  Audit a SKILL.md and its bundled scripts for safety hazards — destructive
  operations missing confirmation gates, excessive permissions, prompt injection
  vectors, scope creep, and description-behavior mismatches. Use when a user
  says "review this skill for safety", "is this skill safe to publish",
  "check for destructive operations", or "audit before sharing". Use before
  publishing to a shared registry, after importing from an untrusted source,
  or when a skill performs consequential operations (file deletion, API calls,
  deployments). Do not use for routing or output-quality evaluation (use
  skill-evaluation), structural anti-pattern detection (use skill-anti-patterns),
  or skills that are purely informational with no side effects.
---

# Purpose

Audit a skill for safety hazards before it is published, imported, or promoted. Produces a structured verdict with actionable findings.

# When to use

- Before publishing a skill to a shared registry or marketplace
- After importing a skill from an untrusted or external source
- When a skill performs consequential operations (deletion, network calls, deployments)
- When a skill was flagged during a `repo-process-doctor` or catalog audit

# When NOT to use

- Evaluating routing precision or output quality → `skill-evaluation`
- Detecting structural anti-patterns → `skill-anti-patterns`
- The skill is purely informational with no side effects
- The skill was already reviewed and has not changed

# Procedure

1. **Destructive operations** — scan for file deletion (`rm`, `unlink`), database mutations (`DROP`, `DELETE`), git force operations (`force push`, `reset --hard`), mutating API calls (`POST`/`PUT`/`DELETE`), and system modifications (`chmod`, `service restart`). Flag any that lack an explicit confirmation gate.

2. **Excessive permissions** — check whether the skill requests more access than its described purpose requires: file writes outside its scope, credential use without justification, or network access not implied by the description. Flag any unjustified permission.

   **Permission assessment method:** For each tool or action the skill uses, ask: "Would the skill still achieve its stated purpose without this capability?" If yes → the permission is excessive. If no → necessary, but document the risk.

   **Risk tiers:**
   - **Tier 1 (High):** File deletion, `git push`/`force push`, deployment commands, API calls that mutate production data. Procedure MUST include an explicit confirmation step for every Tier 1 action.
   - **Tier 2 (Medium):** File creation outside the skill's stated scope, network access, credential reads, environment variable writes. Procedure SHOULD note each Tier 2 action and its rationale.
   - **Tier 3 (Low):** File reads, local computation, stdout output. No special handling required.

   If a skill uses a Tier 1 action without a confirmation gate, that alone is sufficient for a "Requires changes" verdict.

3. **Scope creep** — verify every procedure step is implied by the description. Flag steps that affect systems or files outside the stated scope, or that perform actions the user did not request.

   **Concrete scope creep signals:**
   - A procedure step that doesn't contribute to any stated output artifact
   - An output artifact not mentioned in the description or output contract
   - "Bonus" actions after the main procedure completes (e.g., "also run linting" when the skill is about packaging)
   - Steps that reference tools or prerequisites not listed in the skill's setup or assumptions
   - Steps that modify files outside the skill's declared working scope

4. **Prompt injection** — scan procedure steps for any that read external content (file contents, API responses, user-pasted text, environment variables). For each:
   - Check whether that content is interpolated directly into an instruction, prompt, or command string.
   - If the content is used in a shell command, check for unescaped variables (`$VAR` without quoting, string concatenation into commands).
   - If the content is used in a template or prompt, check for delimiter confusion (user content that could contain YAML frontmatter markers, markdown headers, or instruction-like text).
   - Flag each path as an injection vector with severity: High (content flows into shell execution), Medium (content flows into prompt/instruction context), Low (content flows into output only).

5. **Description–behavior mismatch** — compare the `description` field and "When to use" section against actual procedure steps. Flag hidden behaviors, understated severity, or actions not disclosed in the description.

6. **Structural compliance** — run automated validation to establish a quantitative baseline:

   ```bash
   python3 scripts/check_skill_structure.py <skill-dir>/SKILL.md    # 10-point structural score
   python3 scripts/skill_lint.py <skill-dir>               # Format lint
   ./scripts/validate-skills.sh                                      # Full repo compliance
   ```

   Flag any structural score below 8/10 or lint errors as additional findings.

7. **Bundled scripts** — if a `scripts/` directory exists, audit for unsafe operations, hardcoded credentials, and undocumented network calls. Flag any operation not documented in SKILL.md.

8. **Partial-failure safety** — check whether destructive operations have rollback or cleanup paths. Flag any destructive step that leaves corrupted state on mid-operation failure.

9. **Verdict** — classify the skill:
   - **Safe** — no findings.
   - **Safe with warnings** — minor issues documented, usable as-is.
   - **Requires changes** — must fix before publishing or promotion.
   - **Unsafe** — fundamental problems; do not use until redesigned.

# Output contract

Always produce this structure. Omit table sections that have zero findings.

```
## Safety Review: [skill-name]

**Verdict**: [Safe | Safe with warnings | Requires changes | Unsafe]

### Destructive Operations
| Operation | Location | Confirmation gate? | Finding |
|-----------|----------|--------------------|---------|

### Permissions
| Permission | Justified? | Finding |
|------------|------------|---------|

### Injection Risks
| Vector | Severity | Mitigation needed |
|--------|----------|-------------------|

### Scope / Description Mismatch
- [finding]

### Required Changes
1. [change]
```

# Failure handling

- **Skill is too opaque to audit** — verdict is Unsafe; if the reviewer cannot trace behavior, neither can the user.
- **Skill is intentionally destructive** (e.g., cleanup/teardown) — verify the description is explicit about destruction, confirmation gates exist, and scope is bounded. Safe if all three hold.
- **External dependencies are unauditable** — note the trust assumption as a warning; do not mark Safe without disclosure.

# Next steps

After safety review:
- If needs improvement → `skill-improver`
- If safe, manage lifecycle state → `skill-lifecycle-management`
