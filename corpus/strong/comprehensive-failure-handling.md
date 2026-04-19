---
name: comprehensive-failure-handling-example
description: >
  Generates infrastructure-as-code drift reports by comparing deployed cloud
  resources against their Terraform or CloudFormation source definitions,
  producing a prioritized remediation plan.
---

# Infrastructure Drift Detection

## Purpose

Compare live cloud infrastructure state against the declared
infrastructure-as-code definitions (Terraform state files,
CloudFormation stacks) to detect configuration drift. Produce a
prioritized drift report that classifies each deviation by severity and
recommends whether to update the code, reimport the resource, or accept
the drift as intentional.

## When to use

- After a production incident where manual changes may have been made
  outside the IaC pipeline.
- As part of a periodic compliance audit to verify infrastructure
  matches its declared state.
- Before a major Terraform apply to detect and resolve out-of-band
  changes that could cause unexpected destroys.
- When onboarding an existing environment into IaC management.

## When NOT to use

- For greenfield infrastructure provisioning — use `infra-provisioner`
  instead.
- For application-level configuration (feature flags, env vars) — those
  are not infrastructure drift.
- When the drift is already known and accepted — document it rather than
  re-detecting it.

# Procedure

## 1. Collect state snapshots

Retrieve the current declared state:
- Terraform: `terraform show -json` or parse the state file directly
- CloudFormation: `aws cloudformation describe-stack-resources`
- Pulumi: `pulumi stack export`

Retrieve the live state:
- Use the provider's describe/get APIs for each resource type
- Normalize both representations to a comparable schema

## 2. Diff and classify

For each resource, compare declared vs live attributes:
- **Critical drift**: security groups, IAM policies, encryption settings
- **Significant drift**: instance types, storage sizes, network config
- **Minor drift**: tags, descriptions, non-functional metadata

Ignore attributes that are inherently dynamic (creation timestamps,
ARN suffixes, auto-generated IDs).

## 3. Determine root cause

For each drift item, attempt to identify the cause:
- Console edit (no audit trail in IaC commits)
- Automated scaling or self-healing (expected, document as accepted)
- Failed apply that partially completed
- Another team's IaC managing the same resource (ownership conflict)

## 4. Produce remediation plan

For each drift item, recommend one of:
- **Update code**: the live state is correct, update IaC to match
- **Reimport**: the resource was created outside IaC, import it
- **Revert**: the live state is wrong, run apply to restore declared state
- **Accept**: the drift is intentional, add to ignore list

# Output contract

```
## Drift Report: [environment]

### Summary
| Severity | Count | Action needed |
|----------|-------|---------------|
| Critical | N     | Immediate     |
| Significant | N  | Next sprint   |
| Minor    | N     | Backlog       |

### Critical Drift
| Resource | Attribute | Declared | Live | Root cause | Remediation |
|----------|-----------|----------|------|------------|-------------|
| ...      | ...       | ...      | ...  | ...        | ...         |

### Remediation Plan
1. [ordered steps]
```

# Failure handling

| Situation | Action |
|-----------|--------|
| Cannot access live cloud APIs | Report which resources could not be checked. Produce partial report for accessible resources. |
| State file is locked or corrupted | Attempt read-only access. If unavailable, report as blocked and recommend state recovery. |
| Resource type not supported by diff logic | List unsupported types explicitly. Do not silently skip them. |
| Drift volume exceeds 50 resources | Group by service/module. Recommend phased remediation rather than one large apply. |
| Conflicting ownership (multiple IaC sources) | Flag the conflict. Do not recommend remediation until ownership is resolved. |

# Next steps

- After remediation: re-run drift detection to verify zero drift
- For recurring drift: investigate root cause and add preventive controls
- For compliance: archive the drift report as an audit artifact
