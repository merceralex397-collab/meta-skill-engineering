---
name: cost-monitoring
description: "Monitor and reduce cloud spending — set up cost allocation tags, configure budget alerts, right-size compute instances, evaluate reserved vs spot pricing, detect idle resources, and build cost dashboards. Use when analyzing cloud bills, setting up cost controls, or optimizing resource utilization across AWS/GCP/Azure. Do not use for application performance optimization or capacity planning without cost context."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cost-monitoring
  maturity: draft
  risk: low
  tags: [cost-monitoring, finops, right-sizing, budget-alerts]
---

# Purpose

Monitor, analyze, and reduce cloud infrastructure spending — implement cost allocation tagging, configure budget alerts and anomaly detection, right-size compute and storage resources, evaluate reserved instance and spot/preemptible pricing, detect idle and orphaned resources, and build cost visibility dashboards.

# When to use this skill

- Analyzing a cloud bill to identify the top cost drivers by service, team, or environment.
- Setting up cost allocation tags across AWS, GCP, or Azure resources.
- Configuring budget alerts and spending thresholds with automated notifications.
- Right-sizing compute instances based on utilization metrics (CPU, memory, network).
- Evaluating reserved instances, savings plans, or committed use discounts vs on-demand pricing.
- Identifying idle resources: unattached EBS volumes, unused Elastic IPs, idle load balancers, stopped instances with attached storage.
- Setting up spot/preemptible instance strategies for fault-tolerant workloads.
- Building cost dashboards in AWS Cost Explorer, GCP Billing, or third-party tools (Infracost, Vantage).

# Do not use this skill when

- The task is application performance tuning without a cost reduction objective — prefer performance-specific skills.
- The task is capacity planning focused on scaling up to meet demand rather than reducing spend.
- The focus is on a specific provider's CLI or console workflows — prefer `aws` or `gcp` for provider-specific commands.
- The task involves billing disputes or account-level commercial negotiations.

# Operating procedure

1. **Gather the current cost baseline.** Pull the last 3 months of billing data from the provider's cost management tool (AWS Cost Explorer, GCP Billing Reports, Azure Cost Management). Export to CSV if a third-party tool is used.
2. **Identify top cost drivers.** Sort spending by service, then by resource tag (team, environment, project). Flag any single resource or service consuming >30% of total spend.
3. **Audit cost allocation tags.** List all active resources and check for the required tag set (e.g., `team`, `environment`, `project`, `cost-center`). Flag untagged resources and generate a remediation list.
4. **Apply missing tags.** Use the provider CLI to tag untagged resources: `aws resourcegroupstaggingapi tag-resources`, `gcloud resource-manager tags bindings create`, or Terraform `default_tags`.
5. **Configure budget alerts.** Create a monthly budget at the account or project level. Set alert thresholds at 50%, 80%, and 100% of the budget. Wire notifications to Slack, email, or PagerDuty. For AWS: use AWS Budgets. For GCP: use Budget Alerts in Billing.
6. **Enable anomaly detection.** Turn on AWS Cost Anomaly Detection or GCP billing anomaly alerts. Set the sensitivity to flag daily spend increases >20% above the trailing 7-day average.
7. **Right-size compute instances.** Pull CPU and memory utilization metrics for the last 14 days. Flag instances with average CPU <20% and memory <30% as over-provisioned. Recommend downsizing by one instance family step (e.g., m5.xlarge → m5.large).
8. **Evaluate reserved vs spot pricing.** For steady-state workloads running >80% of the month, calculate savings from 1-year no-upfront reserved instances. For fault-tolerant batch workloads, calculate savings from spot/preemptible instances with interruption handling.
9. **Detect idle and orphaned resources.** Scan for: unattached EBS volumes, unused Elastic IPs, load balancers with zero targets, stopped instances with attached storage, unused NAT gateways, and empty S3 buckets with lifecycle policies disabled.
10. **Build the cost dashboard.** Create a dashboard showing: monthly spend trend, spend by service, spend by team/environment tag, top 10 costliest resources, and budget vs actual.
11. **Document recommendations.** Produce a prioritized list of cost savings actions ranked by estimated monthly savings. Include the specific resource, current cost, recommended action, and projected savings.

# Decision rules

- Prioritize savings actions by dollar impact — address the largest waste first.
- Only recommend reserved instances for workloads with >80% steady-state utilization over 3+ months.
- Use spot instances only for workloads that can tolerate interruption (batch, CI runners, stateless workers).
- If a resource is untagged and the owner cannot be identified, flag it for review before terminating.
- Never delete resources without confirming they have no active consumers — check network connections, DNS references, and dependent services.
- Treat cost allocation tagging as a prerequisite for all other optimization work — without tags, attribution is impossible.
- Set budget alerts before optimizing — you need visibility into whether changes are working.

# Output requirements

1. **Cost baseline** — current monthly spend, top 5 services, top 5 resources by cost.
2. **Tag audit report** — count of tagged vs untagged resources, remediation list.
3. **Budget alert configuration** — thresholds, notification channels, alert policy names.
4. **Right-sizing recommendations** — instance ID, current size, utilization metrics, recommended size, estimated savings.
5. **Idle resource report** — resource type, ID, age, last-used date, estimated monthly waste.
6. **Savings action plan** — prioritized list with estimated monthly savings per action.

# References

- AWS Cost Explorer: https://docs.aws.amazon.com/cost-management/latest/userguide/ce-what-is.html
- GCP Billing Reports: https://cloud.google.com/billing/docs/how-to/reports
- AWS Cost Anomaly Detection: https://docs.aws.amazon.com/cost-management/latest/userguide/manage-ad.html
- FinOps Foundation principles: https://www.finops.org/framework/principles/
- Infracost (IaC cost estimation): https://www.infracost.io/docs/
- `references/preflight-checklist.md`

# Related skills

- `aws` — AWS-specific billing tools, Cost Explorer, Budgets API.
- `gcp` — GCP Billing, committed use discounts, recommender API.
- `cloud-deploy` — deployment decisions that affect cost (instance types, regions, scaling).

# Anti-patterns

- Optimizing costs without a tagging strategy — savings cannot be attributed to teams.
- Purchasing reserved instances for workloads with unpredictable or declining usage.
- Using spot instances for stateful services without data persistence strategies.
- Setting a single budget alert at 100% — by the time it fires, the overspend has already happened.
- Terminating idle resources without checking for scheduled or seasonal usage patterns.
- Focusing on small savings (pennies/month) while ignoring large waste (hundreds/month).

# Failure handling

- If billing data is inaccessible, verify IAM permissions for the cost management APIs (e.g., `ce:GetCostAndUsage` for AWS, `billing.viewer` for GCP).
- If cost allocation tags are not appearing in billing reports, check that tag activation is enabled (AWS: activate user-defined tags in Billing; GCP: export labels to BigQuery billing export).
- If budget alerts are not firing, verify the notification channel is configured and test with a threshold below current spend.
- If right-sizing recommendations conflict with performance requirements, defer to the application team and document the trade-off.
- If the provider is unknown or multi-cloud, produce provider-agnostic recommendations and flag items that need provider-specific follow-up.
