---
name: runbook-writing
description: "Write operational runbooks with step-by-step procedures, diagnostic commands, escalation paths, recovery actions, and verification checks for production incidents and routine operations. Use when creating incident response playbooks, deployment procedures, on-call guides, or maintenance runbooks. Do not use for architecture decisions (prefer adr-rfc-writing) or user-facing documentation."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: runbook-writing
  maturity: draft
  risk: low
  tags: [runbook, operations, incident-response, playbook, procedures]
---

# Purpose

Write operational runbooks with step-by-step procedures for production incidents, routine maintenance, deployments, and on-call response. Each runbook provides diagnostic commands, escalation paths, recovery actions, rollback steps, and verification checks that an on-call engineer can follow under pressure without prior context.

# When to use this skill

- Creating incident response playbooks for known failure modes (database failover, service outage, certificate expiry).
- Writing deployment procedures with pre-checks, execution steps, and rollback instructions.
- Documenting on-call guides that map alerts to diagnostic and remediation steps.
- Building maintenance runbooks for scheduled operations (log rotation, backups, capacity scaling).
- Converting tribal knowledge from senior engineers into repeatable, documented procedures.

# Do not use this skill

- For architecture decisions or design rationale — prefer `adr-rfc-writing`.
- For user-facing product documentation or help articles.
- For release notes or changelogs — prefer `release-notes`.
- For technical specifications or API contracts — prefer `spec-authoring`.

# Operating procedure

1. **Identify the scenario and audience.** Name the specific operational scenario (e.g., "Database primary failover," "Deploy v2.x to production"). Confirm the target audience: on-call engineer, SRE, or operations team. Assume the reader has system access but may not have deep context.
2. **Define prerequisites.** List required access (SSH keys, VPN, admin roles), tools (kubectl, aws-cli, psql), and permissions. Include how to verify each prerequisite before starting.
3. **Document the trigger condition.** Specify what initiates this runbook: an alert name, a monitoring threshold, a scheduled event, or a manual request. Include the alert source (PagerDuty, Datadog, CloudWatch) and the expected alert payload.
4. **Write diagnostic steps.** Provide exact commands to assess the current state. Each command must include: the command itself in a code block, what output to expect when healthy, and what output indicates the problem. Example: `kubectl get pods -n production | grep -v Running` — any output means unhealthy pods exist.
5. **Write remediation steps.** Number each step. Include the exact command or action, the expected outcome, and a verification check after each step. Use conditional branches: "If output shows X, proceed to step 7. If output shows Y, escalate per section below."
6. **Add rollback procedures.** For every change made during remediation, document how to reverse it. Include the rollback command, the verification that rollback succeeded, and the maximum safe time window for rollback.
7. **Define escalation paths.** Specify when to escalate, who to contact (team, Slack channel, phone number), and what information to include in the escalation message. Provide a template: "Service: X, Impact: Y, Duration: Z, Steps taken: [list]."
8. **Include verification checks.** After remediation, list the checks that confirm the issue is resolved: health endpoints to hit, metrics to verify, log patterns to confirm, and customer-facing behavior to validate.
9. **Add timing and SLA context.** Note the expected time for each major step. Flag steps that may take >5 minutes. Include the SLA target for resolution and when to trigger escalation based on elapsed time.
10. **Format for scanability.** Use headers for each phase (Prerequisites, Diagnosis, Remediation, Rollback, Escalation, Verification). Put all commands in fenced code blocks. Use bold for critical warnings. Use callout blocks for "DANGER" or "CAUTION" steps.

# Decision rules

- Every command in the runbook must be copy-pasteable — no pseudocode, no "run the appropriate command."
- Include expected output for every diagnostic command so the operator can distinguish healthy from unhealthy.
- Assume the reader is under stress — keep sentences short, use numbered steps, avoid paragraphs of explanation.
- Put the most common failure mode first when multiple scenarios share a runbook.
- Time-sensitive steps must include an explicit timeout: "If not resolved within 10 minutes, escalate."
- Never include secrets or passwords — reference the secrets manager path instead.

# Output requirements

1. A Markdown document with clearly separated sections: Overview, Prerequisites, Trigger, Diagnosis, Remediation, Rollback, Escalation, Verification.
2. All commands in fenced code blocks with language hints (bash, sql, etc.).
3. Conditional branches clearly marked with "If... then..." formatting.
4. An escalation contact table with Name/Team, Channel, and When to Contact columns.
5. A metadata header with: Last Updated date, Owner team, and Tested On date.

# References

- Google SRE Book — Managing Incidents chapter: https://sre.google/sre-book/managing-incidents/
- PagerDuty Incident Response guide: https://response.pagerduty.com/
- Atlassian Incident Management handbook: https://www.atlassian.com/incident-management

# Related skills

- `adr-rfc-writing` — for documenting the architectural decisions behind the systems these runbooks operate.
- `release-notes` — for documenting changes that may require new or updated runbooks.
- `spec-authoring` — for the technical specifications that runbook procedures implement.

# Anti-patterns

- Writing runbooks with vague steps like "check the database" without specifying which database, which command, or what to look for.
- Omitting rollback procedures — every change must be reversible or explicitly marked as irreversible with approval requirements.
- Assuming the reader knows the system architecture — include a 2-3 sentence context summary at the top.
- Embedding credentials or IP addresses directly in the runbook instead of referencing a secrets manager or CMDB.
- Writing runbooks that have never been tested — schedule a dry run within 2 weeks of creation.

# Failure handling

- If the system topology is unknown, interview the owning team and document what you learn before writing procedures.
- If a command requires elevated permissions, document the exact permission grant command and who can approve it.
- If the runbook has steps that could cause data loss, prefix them with a `⚠️ DANGER` callout and require explicit confirmation.
- If the runbook references infrastructure that may change (IP addresses, instance IDs), use service discovery names or tags instead and note the lookup method.
- If the runbook becomes longer than 50 steps, split it into sub-runbooks linked from a parent decision tree.
