---
name: process-doctor
description: Diagnose and repair broken agent processes — doom loops, workflow stalls, status-over-evidence routing, context rot, conflicting instructions. Use when the user says "agent keeps failing", "workflow stalled", "process broken", "doom loop", "agent repeating", or a specific named process produces wrong results. Do not use for workflow-observability (status reporting), review-audit-bridge (code review findings), or error-handling (code-level error design).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: process-doctor
  maturity: draft
  risk: low
  tags: [process, doctor]
---

# Purpose
Diagnoses broken or ineffective agent processes within a specific repository. Identifies what's causing repeated failures, inconsistent outputs, or workflow stalls. Unlike repo-process-doctor (which audits infrastructure), this skill focuses on diagnosing why a specific process isn't working as expected.

# When to use this skill
Use when:
- An agent keeps making the same mistake
- Workflow completes but output is wrong
- Process worked before but stopped working
- Need to understand why a specific failure occurred

Do NOT use when:
- Setting up new infrastructure (use repo-process-doctor)
- The issue is with code logic, not process
- First-time debugging (try normal debugging first)

# Operating procedure

## 1. Identify the failing process
```markdown
## Process Under Investigation
- Name: [e.g., "ticket execution", "PR review", "deployment"]
- Expected behavior: [what should happen]
- Actual behavior: [what's happening instead]
- Frequency: [always | sometimes | once]
```

## 2. Gather diagnostic evidence
```bash
# Recent relevant commits
git log --oneline -20 --all | grep -i "[process name]"

# Recent changes to process files
git log --oneline -10 -- AGENTS.md tickets/ .opencode/

# Check for error patterns in logs
grep -r "error\|fail\|Error\|FAIL" .opencode/logs/ 2>/dev/null | tail -20
```

## 3. Common failure patterns

### Pattern: Doom loop
**Symptom**: Agent repeats same action indefinitely
**Diagnosis**: Check for missing exit condition or success detection
```bash
# Look for repeated similar commits
git log --oneline -20 | sort | uniq -d
```
**Fix**: Add explicit success criteria and loop limits

### Pattern: Status-over-evidence routing
**Symptom**: Agent marks tasks done without verification
**Diagnosis**: Check ticket acceptance criteria vs. actual evidence
```bash
# Find tickets marked done
grep -l "status: done" tickets/*.md | while read f; do
  echo "=== $f ==="
  grep -A5 "Acceptance Evidence" "$f" || echo "NO EVIDENCE SECTION"
done
```
**Fix**: Require evidence artifacts before status transitions

### Pattern: Impossible read-only delegation
**Symptom**: Research agent can't complete because it needs to create files
**Diagnosis**: Check agent permissions vs. required actions
```bash
grep -A5 "permissions:" .opencode/agents/*.yaml
```
**Fix**: Either expand permissions or change delegation target

### Pattern: Context rot
**Symptom**: Agent makes decisions inconsistent with project context
**Diagnosis**: Check when context was last loaded
```bash
# Check if context files changed since session start
git diff --name-only HEAD~10 -- docs/ AGENTS.md README.md
```
**Fix**: Reload project context; add context refresh trigger

### Pattern: Conflicting instructions
**Symptom**: Agent alternates between different behaviors
**Diagnosis**: Check for contradictory guidance
```bash
# Find potential conflicts
grep -r "must\|always\|never" AGENTS.md docs/*.md .opencode/skills/*.md | sort
```
**Fix**: Resolve conflicts by establishing clear precedence

## 4. Build diagnosis report
```markdown
# Process Diagnosis: [Process Name]
Date: [ISO date]

## Symptom
[What's going wrong]

## Root Cause
[Identified pattern from step 3]

## Evidence
- [Specific file:line or log entry]
- [Command output showing the issue]

## Fix
[Specific change to make]

## Verification
[How to confirm the fix worked]
```

## 5. Apply fix and verify
```bash
# Make the fix
[edit files as needed]

# Verify fix
[run the process again]
[check for expected behavior]
```

## 6. Document prevention
Add to project knowledge base:
```markdown
## Learned: [date] - [issue summary]
- Problem: [brief description]
- Fix: [what was done]
- Prevention: [how to avoid in future]
```

# Output defaults
```markdown
# Process Diagnosis Report

## Issue: [Short description]
## Status: [DIAGNOSED | FIXED | NEEDS_ESCALATION]

## Root Cause
[Pattern identified]

## Fix Applied
[Changes made, or recommended changes]

## Verification
- [ ] Process ran successfully after fix
- [ ] No recurrence in subsequent runs
```

# References
- Common agent failure modes: doom loops, status-over-evidence, context rot
- Process doctor is reactive; repo-process-doctor is proactive audit

# Failure handling
- **Cannot reproduce issue**: Document conditions under which it occurred, add monitoring
- **Multiple possible causes**: Test each hypothesis one at a time
- **Fix requires structural changes**: Escalate to repo-process-doctor for infrastructure repair
- **Issue is in external dependency**: Document workaround and file upstream issue
