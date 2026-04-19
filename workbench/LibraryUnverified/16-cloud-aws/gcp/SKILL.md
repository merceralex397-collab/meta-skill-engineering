---
name: gcp
description: "Provision and operate GCP services — deploy to Cloud Run, query BigQuery datasets, configure IAM roles and service accounts, manage GCS buckets, wire Pub/Sub topics, set up Cloud Build pipelines, and push to Artifact Registry. Use when tasks involve gcloud CLI, GCP console configuration, or GCP service integration. Do not use for Firebase-specific features (prefer firebase skill) or AWS/Azure services."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: gcp
  maturity: draft
  risk: low
  tags: [gcp, cloud-run, bigquery, iam, gcs, pubsub]
---

# Purpose

Provision, configure, and operate Google Cloud Platform services — deploy containerized services to Cloud Run, query and manage BigQuery datasets, configure IAM roles and service accounts, manage GCS buckets, wire Pub/Sub topics and subscriptions, set up Cloud Build CI/CD pipelines, and push container images to Artifact Registry.

# When to use this skill

- Deploying a container to Cloud Run with `gcloud run deploy`.
- Creating or querying BigQuery datasets, tables, or scheduled queries.
- Configuring IAM roles, service accounts, and workload identity federation.
- Creating GCS buckets, setting lifecycle policies, or configuring public access.
- Setting up Pub/Sub topics, subscriptions, and push/pull configurations.
- Writing `cloudbuild.yaml` for Cloud Build CI/CD pipelines.
- Pushing or pulling container images from Artifact Registry.
- Configuring VPC connectors, Cloud NAT, or private service connections.
- Using `gcloud` CLI commands for any GCP resource provisioning.

# Do not use this skill when

- The task involves Firebase-specific features (Auth, Firestore rules, Hosting, Emulators) — prefer `firebase`.
- The target is AWS or Azure — prefer `aws` or the relevant Azure skill.
- The task is about generic deployment strategy (blue-green, canary) — prefer `cloud-deploy`.
- Infrastructure is managed via Terraform — prefer `terraform-iac` for HCL authoring, but use this skill for `gcloud` validation and GCP-specific decisions.

# Operating procedure

1. **Identify the GCP project and region.** Confirm the active project with `gcloud config get-value project`. Set the target region with `gcloud config set run/region <region>`. Verify billing is enabled on the project.
2. **Enable required APIs.** Run `gcloud services enable <api>` for each service needed (e.g., `run.googleapis.com`, `bigquery.googleapis.com`, `pubsub.googleapis.com`, `cloudbuild.googleapis.com`, `artifactregistry.googleapis.com`).
3. **Configure IAM.** Create a dedicated service account: `gcloud iam service-accounts create <name> --display-name="<description>"`. Grant the minimum required roles: `gcloud projects add-iam-policy-binding <project> --member="serviceAccount:<sa>" --role="roles/<role>"`. Prefer predefined roles over primitive roles (Viewer/Editor/Owner).
4. **Deploy to Cloud Run.** Build the container: `gcloud builds submit --tag <region>-docker.pkg.dev/<project>/<repo>/<image>:<tag>`. Deploy: `gcloud run deploy <service> --image=<image> --region=<region> --service-account=<sa> --allow-unauthenticated` (or `--no-allow-unauthenticated` for private services). Set environment variables with `--set-env-vars` and secrets with `--set-secrets`.
5. **Set up Artifact Registry.** Create a repository: `gcloud artifacts repositories create <repo> --repository-format=docker --location=<region>`. Configure Docker auth: `gcloud auth configure-docker <region>-docker.pkg.dev`.
6. **Configure BigQuery.** Create a dataset: `bq mk --dataset <project>:<dataset>`. Create tables with schema: `bq mk --table <dataset>.<table> schema.json`. Load data: `bq load --source_format=CSV <dataset>.<table> gs://<bucket>/<file>`. Schedule queries: `bq query --schedule='every 24 hours' --display_name="<name>" '<SQL>'`.
7. **Manage GCS buckets.** Create a bucket: `gcloud storage buckets create gs://<bucket> --location=<region> --uniform-bucket-level-access`. Set lifecycle rules: create a `lifecycle.json` with age-based deletion or storage class transitions, apply with `gcloud storage buckets update gs://<bucket> --lifecycle-file=lifecycle.json`.
8. **Wire Pub/Sub.** Create a topic: `gcloud pubsub topics create <topic>`. Create a push subscription: `gcloud pubsub subscriptions create <sub> --topic=<topic> --push-endpoint=<url> --ack-deadline=60`. Create a pull subscription: `gcloud pubsub subscriptions create <sub> --topic=<topic> --ack-deadline=60`. Set dead-letter topic with `--dead-letter-topic=<dlq>` and `--max-delivery-attempts=5`.
9. **Set up Cloud Build.** Write `cloudbuild.yaml` with steps: build the container, push to Artifact Registry, deploy to Cloud Run. Configure build triggers: `gcloud builds triggers create github --repo-name=<repo> --branch-pattern="^main$" --build-config=cloudbuild.yaml`.
10. **Verify the deployment.** For Cloud Run: `gcloud run services describe <service> --region=<region>` and `curl <service-url>`. For BigQuery: run a test query. For Pub/Sub: publish a test message with `gcloud pubsub topics publish <topic> --message="test"` and verify delivery.
11. **Set up monitoring.** Configure uptime checks in Cloud Monitoring for Cloud Run URLs. Set alert policies for error rate (>1%), latency (p99 >2s), and instance count. Enable Cloud Logging and create log-based metrics for application errors.

# Decision rules

- Use Cloud Run for stateless HTTP services and containers — it scales to zero and requires no cluster management.
- Use Cloud Functions (2nd gen) only for event-driven triggers that do not need custom container images.
- Use BigQuery for analytical queries over large datasets — do not use it as a transactional database.
- Use Pub/Sub for async messaging between services — set ack deadlines based on expected processing time.
- Always use dedicated service accounts per service — never use the default compute service account in production.
- Prefer Artifact Registry over Container Registry (deprecated) for container images.
- Use workload identity federation over exported service account keys when authenticating from external systems (GitHub Actions, other clouds).
- Set Cloud Run min-instances >0 for latency-sensitive services to avoid cold starts.

# Output requirements

1. **Service configuration** — the `gcloud` commands or `cloudbuild.yaml` used to provision and deploy.
2. **IAM configuration** — service accounts created, roles granted, and the principle of least privilege rationale.
3. **Verification result** — confirmed the service is reachable, queries return expected results, or messages are delivered.
4. **Monitoring setup** — uptime checks, alert policies, and log-based metrics configured.
5. **Rollback path** — previous Cloud Run revision ID or Artifact Registry image tag to revert to.

# References

- Cloud Run documentation: https://cloud.google.com/run/docs
- BigQuery documentation: https://cloud.google.com/bigquery/docs
- IAM best practices: https://cloud.google.com/iam/docs/using-iam-securely
- Pub/Sub documentation: https://cloud.google.com/pubsub/docs
- Cloud Build documentation: https://cloud.google.com/build/docs
- Artifact Registry: https://cloud.google.com/artifact-registry/docs
- `references/preflight-checklist.md`

# Related skills

- `firebase` — Firebase Auth, Firestore, Hosting, Cloud Functions within the Firebase SDK.
- `aws` — AWS service equivalents (Lambda, SQS, S3, ECR).
- `terraform-iac` — managing GCP resources via Terraform HCL.

# Anti-patterns

- Using the default compute service account for Cloud Run services — it has overly broad permissions.
- Granting `roles/owner` or `roles/editor` to service accounts — use specific predefined roles.
- Exporting service account keys when workload identity federation is available.
- Using Container Registry (`gcr.io`) for new projects — it is deprecated in favor of Artifact Registry.
- Deploying Cloud Run services without setting memory and CPU limits — leads to unexpected costs.
- Skipping API enablement — `gcloud` commands fail with confusing errors when the service API is not enabled.
- Hardcoding project IDs in `cloudbuild.yaml` — use `$PROJECT_ID` substitution variable.

# Failure handling

- If `gcloud run deploy` fails with permission errors, verify the deployer has `roles/run.admin` and the service account has `roles/run.invoker`.
- If BigQuery queries fail with access denied, check that the querying identity has `roles/bigquery.dataViewer` on the dataset and `roles/bigquery.jobUser` on the project.
- If Pub/Sub messages are not being delivered, check subscription ack deadline, push endpoint health, and dead-letter queue for failed deliveries.
- If Cloud Build triggers do not fire, verify the GitHub connection is authorized and the branch pattern matches.
- If the task involves Firebase-specific features (Firestore rules, Auth providers, Emulator Suite), redirect to the `firebase` skill.
