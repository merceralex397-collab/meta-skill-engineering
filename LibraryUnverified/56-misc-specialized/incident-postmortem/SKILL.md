---
name: incident-postmortem
description: Turns failures or regressions into structured root-cause, corrective-action, and prevention artifacts following blameless SRE practices. Trigger on "write postmortem", "incident review", "root cause analysis", "blameless retrospective". Do NOT use for process-doctor (diagnosing agent process failures) or error-handling (code-level error design).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: incident-postmortem
  maturity: draft
  risk: low
  tags: [incident, postmortem]
---

# Purpose
Write blameless postmortems following Google SRE practices: document what happened, identify contributing factors (not blame), and produce actionable prevention items. The goal is organizational learning, not punishment—if people fear postmortems, incidents get hidden.

# When to use this skill
Use when:
- User-visible downtime or degradation occurred
- Data loss of any kind happened
- On-call engineer had to intervene (rollback, traffic reroute)
- Resolution took longer than SLO allows
- Monitoring failed to detect the issue
- Any stakeholder requests a postmortem

Do NOT use when:
- Issue was caught in staging/preview (log it, but no formal postmortem)
- No user impact and quick self-recovery
- Root cause is already well-understood and tracked

# Operating procedure
1. **Establish timeline** (facts only, no interpretation yet):
   ```markdown
   ## Timeline (all times UTC)
   - 14:23 - Deploy of commit abc123 to production
   - 14:31 - First error alert fires (PagerDuty incident #1234)
   - 14:35 - On-call acknowledges, begins investigation
   - 14:42 - Root cause identified: database connection pool exhausted
   - 14:45 - Rollback initiated
   - 14:52 - Service restored, monitoring confirms recovery
   ```

2. **Document impact**:
   ```markdown
   ## Impact
   - **Duration**: 29 minutes (14:23 - 14:52)
   - **Users affected**: ~12,000 (15% of active users)
   - **Severity**: SEV2 - Partial service degradation
   - **Business impact**: ~$X revenue loss, Y support tickets
   ```

3. **Identify contributing factors** (NOT root cause singular—incidents rarely have one cause):
   ```markdown
   ## Contributing Factors
   1. New feature increased database connections per request from 1 to 3
   2. Connection pool size unchanged from original config (10 connections)
   3. Load testing did not include new feature enabled
   4. Monitoring alert threshold set too high to catch gradual degradation
   ```

4. **Write blameless narrative**: Use passive voice or system descriptions, not "Person X failed to...":
   - ❌ "John didn't test the connection pool changes"
   - ✅ "The testing process did not include connection pool behavior verification"

5. **Define action items** with owners and due dates:
   ```markdown
   ## Action Items
   | ID | Action | Owner | Priority | Due Date |
   |----|--------|-------|----------|----------|
   | 1 | Increase connection pool to 50 | @backend-team | P1 | 2024-01-15 |
   | 2 | Add connection pool utilization metric | @sre | P1 | 2024-01-20 |
   | 3 | Update load test to enable all features | @qa | P2 | 2024-01-30 |
   | 4 | Lower alert threshold to 70% pool usage | @sre | P1 | 2024-01-15 |
   ```

6. **Review and publish**: Share with engineering team, not just incident participants. Learning should spread.

# Output defaults
```markdown
# Postmortem: [Incident Title]

**Date**: [YYYY-MM-DD]  
**Authors**: [names]  
**Status**: Draft | Reviewed | Published

## Summary
[2-3 sentence description of what happened and impact]

## Impact
- Duration: [X minutes/hours]
- Users affected: [number or percentage]
- Severity: [SEV1-4]

## Timeline
[Chronological events with timestamps]

## Contributing Factors
[Numbered list of factors that combined to cause the incident]

## Action Items
[Table with ID, action, owner, priority, due date]

## Lessons Learned
### What went well
- [e.g., Alerting fired within 8 minutes]
### What could be improved
- [e.g., Runbook was outdated]

## Supporting Information
- [Links to dashboards, logs, related incidents]
```

# References
- https://sre.google/sre-book/postmortem-culture/
- https://sre.google/workbook/postmortem-analysis/

# Failure handling
- **Blame creeping into language**: Review and rewrite any sentence that names an individual as the cause; focus on process/system gaps
- **Action items too vague**: Each item must be specific enough to verify completion; "improve monitoring" → "add metric X with alert at threshold Y"
- **No one owns action items**: Assign owner at postmortem review meeting; unowned items don't get done
- **Postmortem not reviewed**: Schedule review meeting within 72 hours of incident; stale postmortems lose context
