---
name: terraform-iac
description: "Write and manage Terraform infrastructure — author HCL modules, configure providers and backends, manage state files, run plan/apply workflows, detect drift, and structure multi-environment deployments. Use when creating or editing .tf files, debugging Terraform plan output, refactoring modules, or configuring remote state. Do not use for Pulumi, CloudFormation, or other IaC tools outside the Terraform ecosystem."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: terraform-iac
  maturity: draft
  risk: low
  tags: [terraform, hcl, iac, state-management, modules]
---

# Purpose

Write and manage Terraform infrastructure-as-code — author HCL resource definitions and reusable modules, configure providers and remote state backends, run `plan`/`apply` workflows safely, detect and resolve state drift, and structure multi-environment deployments with workspaces or directory-based separation.

# When to use this skill

- Creating new `.tf` files to define cloud infrastructure (VPCs, instances, databases, IAM, etc.)
- Editing existing Terraform modules or resource definitions
- Configuring a remote state backend (S3 + DynamoDB, GCS, Terraform Cloud)
- Debugging `terraform plan` output — understanding what will be created, changed, or destroyed
- Refactoring inline resources into reusable modules
- Setting up multi-environment structure (dev/staging/prod) with shared modules
- Resolving state drift, importing existing resources, or recovering from state corruption
- Configuring provider authentication and version constraints

# Do not use this skill when

- The IaC tool is Pulumi, CloudFormation, CDK, or Ansible — this skill is Terraform-only
- The task is application code changes with no infrastructure component
- The task is platform-specific deployment config (Vercel, Heroku) without Terraform involvement
- The task is Docker image building — use `docker-containers` (Terraform can deploy containers but does not build images)

# Operating procedure

1. **Assess the current Terraform state.** Run `terraform init` to initialize providers and backend. Run `terraform plan` to see the current delta between code and infrastructure. Check for any existing drift. Review `.terraform.lock.hcl` for provider version pins.
2. **Structure the project layout.** Organize files by concern:
   - `main.tf` — primary resource definitions
   - `variables.tf` — input variable declarations with types, descriptions, and defaults
   - `outputs.tf` — output value definitions
   - `providers.tf` — provider configuration and version constraints
   - `backend.tf` — remote state backend configuration
   - `versions.tf` — `required_version` and `required_providers` block
   - `modules/` — reusable modules, each with their own `main.tf`, `variables.tf`, `outputs.tf`
3. **Configure the remote state backend.** Set up S3 + DynamoDB (AWS), GCS (GCP), or Terraform Cloud as the backend. Enable state locking. Configure the backend block in `backend.tf` with bucket, key, region, DynamoDB table (for S3), and `encrypt = true`. Run `terraform init -migrate-state` if switching backends.
4. **Write resource definitions.** Define resources using the provider's resource types. Use `data` sources to reference existing infrastructure. Always set explicit `depends_on` only when Terraform cannot infer the dependency automatically. Tag all resources with at minimum `Name`, `Environment`, and `ManagedBy = "terraform"`.
5. **Extract reusable modules.** When a resource pattern repeats across environments, extract it into a module under `modules/{module-name}/`. Define all configurable values as input variables. Output IDs and ARNs that downstream resources need. Pin the module source version when referencing remote modules.
6. **Structure multi-environment deployments.** Choose one approach:
   - **Directory-based**: separate directories per environment (`envs/dev/`, `envs/prod/`) each with their own `backend.tf` and `terraform.tfvars`, calling shared modules
   - **Workspace-based**: single directory using `terraform workspace` — simpler but riskier for production (easy to apply to the wrong workspace)
   - Prefer directory-based for production systems.
7. **Run the plan/apply workflow.** Always run `terraform plan -out=tfplan` first. Review the plan output line by line — check for unexpected destroys or replacements. Only proceed with `terraform apply tfplan` after confirming the plan is correct. In CI/CD, store the plan artifact and require human approval before apply.
8. **Handle state operations carefully.** For importing existing resources: `terraform import {resource_type}.{name} {cloud_id}`. For removing resources from state without destroying: `terraform state rm {resource_address}`. For moving resources between modules: `terraform state mv {old_address} {new_address}`. Always back up state before state operations: `terraform state pull > state-backup.json`.
9. **Detect and resolve drift.** Run `terraform plan` regularly (in CI on a schedule). If drift is detected, determine whether the infrastructure change was intentional (update the code to match) or accidental (run `terraform apply` to reconcile). Document the drift resolution in the commit message.
10. **Validate and format.** Run `terraform fmt -recursive` to enforce canonical formatting. Run `terraform validate` to catch syntax and type errors. Use `tflint` for provider-specific lint rules. Run these in CI as a gate before plan.
11. **Lock provider versions.** Pin provider versions in `versions.tf` using `~>` (pessimistic constraint) to allow patch updates but not minor/major. Commit `.terraform.lock.hcl` to version control. Update providers deliberately with `terraform init -upgrade`.

# Decision rules

- If `terraform plan` shows a resource will be destroyed and recreated, investigate the `forces replacement` reason before proceeding — this often indicates a non-updatable attribute change.
- If state is corrupted or lost, do not run `terraform apply` — first restore from backup or reimport all resources.
- If a module is used by more than two environments, it must live in `modules/` with versioned releases — not copy-pasted inline.
- If the plan shows more than 20 changes, split the work into smaller targeted applies using `-target` — then remove the target flag in follow-up runs.
- Prefer `count` for simple conditional resources and `for_each` for collections. Never use `count` to iterate over a list of distinct resources — use `for_each` with a map.
- If a resource attribute is sensitive (passwords, keys), mark it with `sensitive = true` in the variable and output definitions.

# Output requirements

1. **HCL files** — complete `.tf` files with resources, variables, outputs, and provider config
2. **Plan output** — `terraform plan` output showing the expected changes
3. **Module structure** — module directory layout with `variables.tf`, `main.tf`, `outputs.tf`
4. **Backend configuration** — remote state backend config with locking enabled
5. **CI/CD integration** — plan/apply pipeline steps (if modifying CI/CD)

# References

- Terraform documentation: https://developer.hashicorp.com/terraform/docs
- Terraform language reference: https://developer.hashicorp.com/terraform/language
- Terraform best practices: https://developer.hashicorp.com/terraform/cloud-docs/recommended-practices
- tflint: https://github.com/terraform-linters/tflint
- Terraform state management: https://developer.hashicorp.com/terraform/language/state

# Related skills

- `aws` — for AWS provider resources and IAM configuration
- `gcp` — for GCP provider resources and IAM bindings
- `docker-containers` — for Terraform-managed container infrastructure
- `secret-management` — for managing secrets referenced in Terraform configs

# Anti-patterns

- Running `terraform apply` without reviewing the plan output first
- Using `terraform apply -auto-approve` in production without a prior saved plan
- Storing state files locally or in version control instead of a remote backend with locking
- Hardcoding environment-specific values instead of using variables and `.tfvars` files
- Using `terraform taint` to force recreation when `terraform apply -replace` is available (v0.15.2+)
- Creating monolithic `main.tf` files with hundreds of resources instead of splitting into modules
- Using `-target` as a permanent workflow instead of fixing the dependency graph

# Failure handling

- If `terraform plan` fails with provider errors, run `terraform init -upgrade` to refresh providers. Check that credentials are configured (`AWS_PROFILE`, `GOOGLE_APPLICATION_CREDENTIALS`, etc.).
- If `terraform apply` fails mid-way, do not panic — Terraform records partial state. Run `terraform plan` to see the remaining delta and re-apply.
- If state locking fails (`Error acquiring the state lock`), check for stale locks. Use `terraform force-unlock {lock-id}` only after confirming no other apply is running.
- If a resource is manually deleted outside Terraform, run `terraform plan` to detect the drift, then either reimport or let Terraform recreate it.
- If a module upgrade introduces breaking changes, pin the previous version, review the changelog, and update variable usage before upgrading.
