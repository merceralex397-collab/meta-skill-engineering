---
name: deployment-pipeline
description: Defines CI/CD stages, deployment gates, environment protection, and rollback procedures. Trigger on 'CI/CD', 'deploy', 'pipeline', 'GitHub Actions workflow', 'staging environment', 'rollback procedure'. DO NOT USE for semantic versioning, tagging, or changelog generation (use release-engineering) or infrastructure provisioning (use stack-standards).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: deployment-pipeline
  maturity: draft
  risk: low
  tags: [deployment, pipeline]
---

# Purpose
Design CI/CD pipelines that safely move code from commit to production: build → test → lint → staging deploy → smoke test → production deploy. Define gate conditions at each stage and automatic rollback triggers to catch failures before users do.

# When to use this skill
Use when:
- Setting up CI/CD for a new project
- Current deployment is manual or unreliable
- Adding staging/preview environments
- Implementing deployment gates or approval workflows

Do NOT use when:
- Local development scripts only (no deployment target)
- Project already has mature CI/CD (make incremental improvements instead)

# Operating procedure
1. **Define pipeline stages**:
   ```yaml
   # GitHub Actions example structure
   name: Deploy Pipeline
   
   on:
     push:
       branches: [main]
     pull_request:
       branches: [main]
   
   jobs:
     build:
       # Compile, bundle, create artifacts
     
     test:
       needs: build
       # Unit tests, integration tests
     
     lint:
       # Can run parallel to test
       # Type checking, style, security scanning
     
     deploy-staging:
       needs: [test, lint]
       if: github.ref == 'refs/heads/main'
       # Deploy to staging environment
     
     smoke-test:
       needs: deploy-staging
       # Verify staging deployment works
     
     deploy-production:
       needs: smoke-test
       environment: production  # Requires approval
   ```

2. **Configure environment protection** (GitHub example):
   ```yaml
   deploy-production:
     environment:
       name: production
       url: https://myapp.com
     # In GitHub settings: require approval, restrict to main branch
   ```

3. **Implement smoke tests**:
   ```yaml
   smoke-test:
     runs-on: ubuntu-latest
     steps:
       - name: Health check
         run: |
           for i in {1..30}; do
             if curl -sf https://staging.myapp.com/health; then
               echo "Service healthy"
               exit 0
             fi
             sleep 10
           done
           echo "Health check failed"
           exit 1
       
       - name: Critical path test
         run: |
           # Test actual functionality, not just health endpoint
           curl -sf https://staging.myapp.com/api/v1/status
   ```

4. **Define rollback triggers and procedure**:
   ```yaml
   deploy-production:
     steps:
       - name: Deploy
         id: deploy
         run: ./deploy.sh
       
       - name: Verify deployment
         id: verify
         run: ./smoke-test.sh
       
       - name: Rollback on failure
         if: failure() && steps.deploy.outcome == 'success'
         run: |
           echo "Deployment verification failed, rolling back"
           ./rollback.sh ${{ env.PREVIOUS_VERSION }}
   ```

5. **Use concurrency controls**:
   ```yaml
   concurrency:
     group: deploy-${{ github.ref }}
     cancel-in-progress: false  # Don't cancel in-progress deploys
   ```

6. **Secrets and environment variables**:
   ```yaml
   deploy-production:
     env:
       # Use GitHub secrets for sensitive values
       API_KEY: ${{ secrets.PRODUCTION_API_KEY }}
       DATABASE_URL: ${{ secrets.PRODUCTION_DATABASE_URL }}
     steps:
       - name: Deploy
         run: |
           # Never echo secrets; use masked values
           ./deploy.sh --api-key="$API_KEY"
   ```

# Output defaults
```yaml
# .github/workflows/deploy.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main]
  pull_request:

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build
        run: npm ci && npm run build
      - uses: actions/upload-artifact@v4
        with:
          name: build
          path: dist/

  test:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: npm ci && npm test

  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: npm ci && npm run lint && npm run typecheck

  deploy-staging:
    needs: [test, lint]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/download-artifact@v4
      - run: ./deploy.sh staging

  deploy-production:
    needs: deploy-staging
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/download-artifact@v4
      - run: ./deploy.sh production
```

# References
- GitHub Actions Documentation: https://docs.github.com/en/actions
- GitHub Actions Deployment: https://docs.github.com/en/actions/deployment/about-deployments/about-continuous-deployment
- Managing Environments: https://docs.github.com/en/actions/deployment/targeting-different-environments/managing-environments-for-deployment

# Failure handling
- **Flaky tests blocking deployment**: Quarantine flaky tests; fix them but don't let them block all deploys
- **Staging differs from production**: Use infrastructure-as-code; staging should mirror prod configuration
- **Rollback script doesn't work**: Test rollback procedure regularly; include in deployment verification
- **Secrets exposed in logs**: Use `add-mask` command; never echo environment variables containing secrets
- **Concurrent deploys cause conflicts**: Use `concurrency` groups; consider deploy locks for shared infrastructure
