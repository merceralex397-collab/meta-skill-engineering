---
name: skill-anti-patterns
description: >-
  Audit a SKILL.md for structural anti-patterns that reduce routing accuracy,
  output quality, or maintainability. Use when a user says "check this skill
  for anti-patterns", "what's wrong with this skill", or "audit this skill
  before promotion". Also use for post-failure diagnostics when a skill
  misbehaves but the root cause is unclear. Do not use for full skill rewrites
  (use skill-creator), trigger-only fixes (use skill-trigger-optimization),
  surgical fixes to known problems (use skill-improver), or measuring
  routing precision/recall (use skill-evaluation).
---

# Purpose

Scan a SKILL.md against a concrete anti-pattern checklist (AP-1 through AP-16). For each pattern found, report it with a specific fix including before/after examples.

# When to use

- User says "check for anti-patterns", "what's wrong with this skill", "review before promotion"
- Pre-promotion review: draft → stable
- Skill producing unexpected routing or output and root cause is unclear
- Quick structural audit after editing a skill

# When NOT to use

- Full rewrite needed → `skill-creator`
- Only trigger/description needs fixing → `skill-trigger-optimization`
- Specific known problem needs a surgical fix → `skill-improver`
- Need to measure routing precision/recall quantitatively → `skill-evaluation`
- Skill is working correctly — do not audit healthy skills

# Procedure

Read the target SKILL.md. Check each anti-pattern below. For each PRESENT, write the specific fix with before/after.

> **Quick scan priority**
> - **Quick scan (5 min):** Check AP-1, AP-7, AP-12 first — these are routing-critical. If the skill can't be found by the host, nothing else matters.
> - **Full audit (15 min):** Check all 16 in order.
> - **Triage shortcut:** If AP-1 or AP-12 fires, fix those before checking anything else — other issues are moot if routing doesn't work.

**AP-1: Circular trigger language** · `CRITICAL` — causes routing failure
- Pattern: Description says "Use when task clearly involves X" or "Use this for X-related work"
- Example before: `description: Helps with testing tasks when testing is needed.`
- Example after: `description: Generate pytest unit tests for Python functions when the user says "write tests", "add test coverage", or "create unit tests for this module". Do not use for integration or E2E tests (use integration-testing).`
- Fix: Replace circular self-references with specific user phrases and observable task signals

**AP-2: Boilerplate procedure steps** · `HIGH` — skill adds no procedural value
- Pattern: Steps copied from a template like "Keep scope explicit", "Prefer references over inline", "Decide whether content belongs in core"
- Example before: `3. Keep scope explicit and boundaries clear`
- Example after: `3. List all file paths that will be modified and confirm with user before writing`
- Fix: Delete meta-commentary; replace with concrete actions the agent can execute

**AP-3: Generic output defaults** · `HIGH` — inconsistent, unverifiable output
- Pattern: "Structured markdown artifact(s) with clear next steps"
- Example before: `Output: A well-structured document with actionable recommendations`
- Example after: `Output: A markdown document with sections: ## Summary (3 sentences), ## Findings (numbered list with severity labels), ## Recommended Actions (ordered by priority)`
- Fix: Name exact sections, formats, field names, or schemas

**AP-4: Generic failure handling** · `MEDIUM` — skill works in happy path only
- Pattern: "If inputs missing, surface gap as decision item"
- Example before: `If something goes wrong, report the issue and suggest alternatives`
- Example after: `If the target file does not exist: stop, report the missing path, and ask user to confirm correct location before retrying`
- Fix: Name the specific most-common failure mode with exact recovery action

**AP-5: Overloaded purpose** · `MEDIUM` — inconsistent results across cases
- Pattern: Purpose uses "and" 2+ times covering different task families
- Example before: `Creates API tests, generates documentation, and manages deployment configs`
- Example after: `Creates API tests for REST endpoints using pytest and requests`
- Fix: Split into separate skills (use skill-variant-splitting) or narrow to one job

**AP-6: Unmeasurable acceptance criteria** · `LOW` — wasted tokens on subjective checks
- Pattern: Success defined with subjective terms like "good", "appropriate", "high-quality"
- Example before: `Ensure output is high-quality and appropriate for the context`
- Example after: `Output passes if: (1) all required sections present, (2) no placeholder text remains, (3) every code example is syntactically valid`
- Fix: Replace with observable, countable criteria

**AP-7: Missing "Do NOT use when" section** · `HIGH` — causes overtriggering
- Pattern: No negative routing at all, or empty section
- Fix: Add 2-4 confusion cases naming the alternative skill. Format: "Do NOT use when [scenario] (use `skill-name` instead)"

**AP-8: Missing failure handling** · `MEDIUM` — weak agent guidance on errors
- Pattern: No failure handling section at all
- Fix: Add covering the 2-3 most common failure modes with specific recovery actions

**AP-9: Vague procedure verbs** · `MEDIUM` — ambiguous procedure
- Pattern: Steps use "Consider", "Think about", "You might want to", "Ensure"
- Example before: `Consider the user's requirements and think about the best approach`
- Example after: `Read the user's requirements. List the constraints. Select the approach that satisfies the most constraints.`
- Fix: Replace with concrete action verbs: "Read", "List", "Write", "Check", "Run", "Compare"

**AP-10: No authoritative references** · `LOW` — broken or missing context links
- Pattern: No URLs, or only vague "see documentation"
- Example before: `References: relevant documentation`
- Example after: `References: https://docs.github.com/en/copilot/concepts/agents/about-agent-skills`
- Fix: Add 2-3 real URLs to official documentation

**AP-11: Identity-free description** · `HIGH` — fails to distinguish from neighbors
- Pattern: Description doesn't start with an action verb; reads like a noun phrase
- Example before: `description: A skill for handling deployments`
- Example after: `description: Deploy applications to Kubernetes clusters when...`
- Fix: Start description with the primary action verb

**AP-12: Copy-paste category boilerplate** · `CRITICAL` — routing failure across category
- Pattern: Multiple skills in same category share identical description suffixes
- Example: 12 skills all ending with "Use this when creating, adapting, refining, installing, testing, or packaging a skill..."
- Fix: Remove category boilerplate; write skill-specific routing text

**AP-13: Instruction overload** · `HIGH` — degrades instruction following
- Pattern: SKILL.md exceeds 400 lines with critical steps buried in the middle or end
- Detection: Count lines. If >400, check whether the most important procedure steps are in the first 40% of the file.
- Fix: Extract reference material to `references/`. Front-load critical steps. Keep SKILL.md under 400 lines.

**AP-14: Capability assumption** · `HIGH` — procedure fails on some clients
- Pattern: Steps assume tools the agent may not have (subagents, browsers, specific CLIs, MCP servers) without declaring the dependency
- Example before: `Spawn a subagent to run the eval suite in parallel`
- Example after: `Run the eval suite. If subagents are available, spawn parallel runs; otherwise run sequentially.`
- Fix: Declare tool dependencies in frontmatter. Add fallback paths for optional capabilities.

**AP-15: Few-shot starvation** · `MEDIUM` — inconsistent output format
- Pattern: Output format described in prose but no concrete filled example provided
- Example before: `Produce a markdown table with columns for test name, result, and notes`
- Example after: Shows the actual table with 2–3 filled rows demonstrating expected content
- Fix: Add a concrete filled example next to every output format description

**AP-16: Hallucination surface** · `MEDIUM` — agent invents success criteria
- Pattern: Vague quality requirements like "ensure quality", "verify correctness", "make it good" with no measurable criteria
- Example before: `Verify the output meets quality standards`
- Example after: `Verify: (1) all required sections present, (2) no TODO/placeholder text, (3) code examples parse without syntax errors`
- Fix: Replace subjective quality language with concrete, checkable criteria

# Output contract
```
## Anti-Pattern Audit: [skill-name]

| ID | Pattern | Status | Fix |
|----|---------|--------|-----|
| AP-1 | Circular triggers | PRESENT | Replace "clearly involves" with "when user says X" |
| AP-2 | Boilerplate steps | ABSENT | — |
...

### Summary
- PRESENT: 3 (AP-1, AP-4, AP-7)
- ABSENT: 7

### Priority Fixes
1. AP-1: [specific fix]
2. AP-4: [specific fix]
3. AP-7: [specific fix]
```

If all ABSENT: "**Clean Bill of Health** — no anti-patterns detected"

# Failure handling

- **Target SKILL.md not found or empty**: Stop. Report the missing/empty path. Ask user to confirm the correct file location before retrying.
- **Skill too malformed to audit** (e.g. no frontmatter, no procedure section): Report what is missing. Recommend full rewrite via `skill-creator` instead of piecemeal fixes.
- **Ambiguous skill purpose**: If the purpose could be read two ways, audit against both interpretations and flag the ambiguity as an additional finding.

## Next steps

After an anti-pattern audit:
- Fix found issues → `skill-improver`
- If AP-1, AP-11, or AP-12 fired → `skill-trigger-optimization`
- If skill is too malformed for piecemeal fixes → `skill-creator`
