---
name: aws
description: Provision and configure AWS services — write IAM policies, manage S3 buckets, build Lambda functions, deploy with CDK or CloudFormation, wire API Gateway, configure DynamoDB tables, and set up CloudWatch alarms. Use when tasks involve AWS console/CLI operations, CDK stack definitions, IAM permission debugging, or AWS service integration. Do not use for GCP, Azure, or generic cloud-agnostic architecture patterns.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: aws
  maturity: draft
  risk: low
  tags: [aws, iam, s3, lambda, cdk, cloudformation]
---

# Purpose

Use this skill to provision, configure, and operate AWS services — define infrastructure with CDK or CloudFormation, write least-privilege IAM policies, build and deploy Lambda functions, configure S3, DynamoDB, SQS, API Gateway, and CloudWatch, and debug permission and deployment issues.

# When to use this skill

Use this skill when:

- writing or reviewing IAM policies, roles, and permission boundaries
- creating or modifying S3 buckets (lifecycle rules, CORS, bucket policies, event notifications)
- building Lambda functions (runtime config, layers, environment variables, VPC attachment, memory/timeout tuning)
- defining CDK stacks (`cdk init`, constructs, `cdk synth`, `cdk deploy`) or CloudFormation templates
- configuring API Gateway (REST or HTTP API, routes, authorizers, stages, throttling)
- setting up DynamoDB tables (partition/sort keys, GSIs, capacity modes, TTL)
- wiring SQS queues or SNS topics (dead-letter queues, visibility timeout, fan-out patterns)
- creating CloudWatch alarms, dashboards, or log-based metric filters
- debugging AWS permission errors (`AccessDenied`, `is not authorized to perform`)
- running AWS CLI commands (`aws s3`, `aws lambda`, `aws iam`, `aws cloudformation`)

# Do not use this skill when

- the task targets GCP, Azure, or Firebase services (use the respective skill)
- the task is cloud-agnostic architecture design without AWS-specific implementation
- the task is application business logic that does not interact with AWS services
- a narrower active skill (e.g., `docker-containers` for ECS/Fargate container config) already owns the problem

# Operating procedure

1. Identify the AWS services and region.
   Confirm which AWS services are involved, the target region, and the AWS account structure (single account, multi-account with Organizations). Check for existing CDK/CloudFormation stacks in the repo.

2. Write IAM policies with least privilege.
   Start with the minimum permissions needed. Use specific resource ARNs instead of `*`. Use `aws iam access-analyzer` or CloudTrail logs to identify actually-used permissions. Add conditions (`aws:SourceArn`, `aws:PrincipalOrgID`) to restrict cross-service access. Never use `"Effect": "Allow", "Action": "*", "Resource": "*"`.

3. Define infrastructure as code.
   - **CDK (preferred)**: Define constructs in TypeScript or Python. Use L2 constructs (e.g., `lambda.Function`, `s3.Bucket`) over L1 (`CfnResource`). Run `cdk synth` to generate the CloudFormation template and review before deploying.
   - **CloudFormation**: Use YAML format. Define `Parameters` for environment-specific values. Use `Conditions` for multi-environment templates. Always include `DeletionPolicy: Retain` on stateful resources (databases, S3 buckets with data).

4. Configure Lambda functions.
   Set `memorySize` based on profiling (128 MB minimum, 1024 MB for compute-heavy). Set `timeout` to 2x the expected p99 execution time. Use environment variables for configuration — never hardcode secrets. Wire a dead-letter queue for async invocations. Use Lambda layers for shared dependencies.

5. Set up data stores.
   - **DynamoDB**: Choose partition key for even distribution. Add sort keys for range queries. Use on-demand capacity for unpredictable workloads, provisioned for steady-state. Enable point-in-time recovery. Set TTL for expiring data.
   - **S3**: Enable versioning on buckets with important data. Add lifecycle rules to transition to Glacier or expire objects. Set CORS only when needed. Block public access by default.

6. Wire API Gateway.
   Use HTTP API (v2) for simple Lambda proxies — cheaper and faster. Use REST API (v1) when you need request validation, usage plans, or API keys. Attach a Cognito or Lambda authorizer. Set throttling limits per route. Enable access logging to CloudWatch.

7. Configure monitoring and alarms.
   Create CloudWatch alarms for: Lambda errors (>1% error rate), Lambda duration (>80% of timeout), DynamoDB throttled reads/writes, SQS dead-letter queue depth (>0), API Gateway 5xx rate. Set up SNS notifications to alert the on-call channel. Create a CloudWatch dashboard grouping key metrics per service.

8. Deploy and verify.
   Run `cdk deploy --require-approval broadening` (or `aws cloudformation deploy`). After deployment, test each endpoint/function manually. Check CloudWatch logs for errors. Verify IAM permissions by running the expected operations with the deployed role. Run `cdk diff` before subsequent deploys to review changes.

# Decision rules

- Always use infrastructure as code (CDK or CloudFormation) — never provision via console clicks for anything beyond investigation.
- Default to on-demand DynamoDB capacity unless the workload is steady and predictable.
- Use HTTP API Gateway over REST API unless you need features only available in REST (WAF, request validation, API keys).
- Never put secrets in environment variables or CloudFormation parameters in plaintext — use Secrets Manager or SSM Parameter Store with `SecureString`.
- Prefer `RemovalPolicy.RETAIN` (CDK) / `DeletionPolicy: Retain` (CFN) on all stateful resources.
- Set Lambda concurrency limits (`ReservedConcurrentExecutions`) to prevent runaway costs from unexpected spikes.

# Output requirements

1. `Infrastructure Code` — CDK stack or CloudFormation template with all resources defined
2. `IAM Policy Documents` — least-privilege policies for each role with specific resource ARNs
3. `Deployment Runbook` — step-by-step commands to deploy, verify, and roll back
4. `Monitoring Configuration` — CloudWatch alarms, dashboard definition, and alert routing
5. `Cost Estimate` — expected monthly cost based on anticipated usage (use AWS Pricing Calculator)

# References

Read these only when relevant:

- `references/iam-policy-patterns.md`
- `references/cdk-best-practices.md`
- `references/lambda-optimization.md`

# Related skills

- `firebase`
- `gcp`
- `vercel`
- `docker-containers`

# Anti-patterns

- Using `"Resource": "*"` in IAM policies — grants overly broad access and fails security reviews.
- Creating resources via the AWS console without corresponding IaC — leads to drift and unreproducible environments.
- Setting Lambda timeout to the maximum (900s) "just in case" — masks performance issues and increases cost on failures.
- Storing secrets in Lambda environment variables in plaintext — visible in the console and CloudFormation outputs.
- Deploying CloudFormation stacks without `--no-execute-changeset` review in production — risky updates may replace stateful resources.
- Skipping DynamoDB point-in-time recovery — makes accidental data deletion unrecoverable.

# Failure handling

- If `cdk deploy` fails with a rollback, check the CloudFormation events in the console for the specific resource that failed and its error message.
- If a Lambda function returns `AccessDenied`, use CloudTrail to find the exact API call and missing permission, then add it to the function's execution role.
- If DynamoDB returns `ProvisionedThroughputExceededException`, switch to on-demand capacity or increase provisioned RCU/WCU, and add auto-scaling.
- If API Gateway returns 502, check the Lambda function's CloudWatch logs — a 502 usually means the function crashed, timed out, or returned a malformed response.
- If CloudFormation stack is stuck in `UPDATE_ROLLBACK_FAILED`, identify the resource that cannot roll back, skip it with `continue-update-rollback --resources-to-skip`, then fix manually.
