---
name: vague-procedure-example
description: >
  Guides teams through incident post-mortem analysis to identify root causes
  and produce actionable follow-up items after production outages.
---

# Incident Post-Mortem Analysis

## Purpose

Facilitate structured post-mortem reviews after production incidents so that
teams document what happened, why it happened, and what concrete changes will
prevent recurrence. Produces a post-mortem document and a prioritized list of
remediation tickets.

## When to use

- After any SEV-1 or SEV-2 production incident has been resolved.
- When a near-miss is escalated by the on-call engineer.
- During quarterly reliability reviews to re-examine unresolved incidents.

## When NOT to use

- For pre-deployment risk assessments — use `deployment-risk-review` instead.
- For live incident coordination — use `incident-commander` instead.
- When the event was a planned maintenance window with expected impact.

## Procedure

1. Gather the relevant stakeholders and review the situation thoroughly.
2. Analyze the incident from multiple perspectives to understand what went wrong.
3. Apply industry best practices to identify contributing factors.
4. Ensure all important details are captured in the documentation.
5. Think carefully about what could be improved going forward.
6. Synthesize findings into a holistic view of the incident.
7. Leverage lessons learned to drive continuous improvement.
8. Validate that the analysis meets quality standards and covers all bases.
9. Coordinate with appropriate teams to ensure alignment on next steps.
10. Finalize the deliverables and share with stakeholders as appropriate.

## Output contract

Produces two artifacts:

1. **Post-mortem document** saved to `docs/postmortems/YYYY-MM-DD-<slug>.md`
   containing timeline, root cause, impact summary, and action items.
2. **Remediation tickets** as a Markdown checklist with owner, priority, and
   target completion date for each item.

## Failure handling

- If the incident timeline cannot be reconstructed, note gaps explicitly and
  mark the post-mortem as DRAFT until gaps are filled.
- If stakeholders disagree on root cause, document each perspective and
  escalate to the engineering director for resolution.

## Next steps

- Feed remediation tickets into `ticket-pack-builder` for tracking.
- Schedule a follow-up review using `skill-lifecycle-management` to verify
  remediation items were completed within 30 days.
