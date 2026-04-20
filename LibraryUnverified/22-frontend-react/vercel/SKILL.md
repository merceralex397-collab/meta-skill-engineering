---
name: vercel
description: "Deploy and configure applications on Vercel — set up vercel.json, configure edge and serverless functions, manage environment variables, use preview deployments, wire custom domains, and optimize build settings. Use when deploying to Vercel, editing vercel.json, configuring Vercel functions, or debugging Vercel build/deploy issues. Do not use for self-hosted deployments, non-Vercel serverless platforms, or static-only hosting without Vercel features."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: vercel
  maturity: draft
  risk: low
  tags: [vercel, edge-functions, serverless, preview-deploy]
---

# Purpose

Deploy and configure web applications on Vercel — write and maintain `vercel.json`, configure edge and serverless functions, manage environment variables across environments, leverage preview deployments for PR-based review, wire custom domains with DNS, and optimize build settings for fast deploys.

# When to use this skill

- Creating or editing `vercel.json` configuration
- Deploying a Next.js, SvelteKit, Nuxt, Astro, or static site to Vercel
- Writing Vercel serverless functions (`api/` directory) or edge functions
- Configuring environment variables for development, preview, and production
- Setting up preview deployments for pull request branches
- Wiring a custom domain and configuring DNS records for Vercel
- Debugging Vercel build failures, deploy errors, or function runtime issues
- Configuring redirects, rewrites, headers, or CORS in `vercel.json`
- Optimizing build output, function bundling, or cold start performance on Vercel

# Do not use this skill when

- Deploying to self-hosted infrastructure — use `self-hosting-ops`
- The serverless platform is AWS Lambda or GCP Cloud Functions — use `serverless-patterns`
- The task is Cloudflare Workers at the edge — use `cloudflare-worker-patterns`
- The task is purely static hosting without Vercel-specific features (functions, middleware)
- The task is application business logic with no Vercel platform concern

# Operating procedure

1. **Initialize the Vercel project.** Run `vercel link` to connect the local repo to a Vercel project (or `vercel` for first-time setup). Confirm the framework preset is auto-detected (Next.js, SvelteKit, etc.). If not, set `framework` in `vercel.json`.
2. **Configure `vercel.json`.** Define the project configuration:
   ```json
   {
     "framework": "nextjs",
     "buildCommand": "npm run build",
     "outputDirectory": ".next",
     "regions": ["iad1"],
     "functions": {
       "api/**/*.ts": { "memory": 1024, "maxDuration": 10 }
     }
   }
   ```
   Set `regions` to the closest region to your users. Configure function memory and duration limits based on workload.
3. **Set up environment variables.** Use the Vercel CLI or dashboard to set env vars for each environment:
   - `vercel env add DATABASE_URL production` — production-only
   - `vercel env add NEXT_PUBLIC_API_URL preview` — preview deployments
   - `vercel env add DEBUG development` — local development via `vercel dev`
   - Never commit `.env.production` to the repo. Use `vercel env pull .env.local` for local dev.
4. **Write serverless functions.** Create functions in the `api/` directory (or framework-specific locations like Next.js `app/api/`). Each file exports a default handler. For edge functions, add `export const config = { runtime: "edge" }` to run on Vercel's edge network with lower latency.
5. **Configure redirects, rewrites, and headers.** Add `redirects`, `rewrites`, and `headers` arrays to `vercel.json`. Prefer framework-native routing (Next.js `next.config.js`) over `vercel.json` when both are available.
6. **Test with preview deployments.** Push a branch or open a PR — Vercel auto-deploys a preview at a unique URL. Share the preview URL for review. Preview deployments use the `preview` environment variables. Verify the preview before merging to production.
7. **Wire a custom domain.** In the Vercel dashboard or CLI, add the domain: `vercel domains add example.com`. Configure DNS: add an `A` record pointing to `76.76.21.21` or a `CNAME` to `cname.vercel-dns.com` for subdomains. Vercel provisions TLS automatically. Verify with `vercel domains inspect example.com`.
8. **Optimize build performance.** Enable or configure:
   - Remote caching (`vercel.json`: `"installCommand": "npm ci"` to leverage lockfile caching)
   - Incremental Static Regeneration (ISR) for Next.js pages that can be stale
   - Edge middleware for auth checks, A/B tests, or geolocation routing that runs before the function
   - Function bundling — keep `api/` function dependencies minimal to reduce cold start time
9. **Deploy to production.** Merge the PR to the main branch — Vercel auto-deploys to production. Or run `vercel --prod` for manual production deploys. Verify the deployment at the production URL. Check the Vercel dashboard for build logs, function logs, and error rates.
10. **Monitor and roll back.** Use Vercel's deployment dashboard to monitor function invocation counts, error rates, and duration. If a production deploy introduces issues, use the Vercel dashboard to instantly roll back to the previous deployment (Instant Rollback feature).

# Decision rules

- If the function needs < 50ms response time globally, use edge functions (`runtime: "edge"`). If it needs Node.js APIs or heavy computation, use serverless functions.
- If the project is a static site with no dynamic functions, consider whether Vercel adds value over a simple CDN — use Vercel when you need preview deployments, analytics, or middleware.
- If a redirect or rewrite can be handled by the framework (Next.js `rewrites` in `next.config.js`), prefer framework-level config over `vercel.json` to keep routing logic with the app.
- If environment variables contain secrets, set them as `Sensitive` in the Vercel dashboard — they will be encrypted and hidden from logs.
- If function cold starts are a problem, reduce bundle size first (check with `vercel inspect`), then consider edge functions or splitting large functions.
- If the build takes more than 5 minutes, investigate caching (`npm ci` vs `npm install`), build output size, and whether unused dependencies can be pruned.

# Output requirements

1. **`vercel.json`** — complete project configuration file
2. **Function code** — serverless or edge function implementations
3. **Environment variable list** — names, environments (dev/preview/prod), and which are sensitive
4. **Domain configuration** — DNS records needed for custom domain setup
5. **Deployment verification** — confirmation that preview and/or production deploys succeed with expected behavior

# References

- Vercel documentation: https://vercel.com/docs
- `vercel.json` reference: https://vercel.com/docs/projects/project-configuration
- Vercel serverless functions: https://vercel.com/docs/functions/serverless-functions
- Vercel edge functions: https://vercel.com/docs/functions/edge-functions
- Vercel CLI reference: https://vercel.com/docs/cli
- Vercel environment variables: https://vercel.com/docs/projects/environment-variables

# Related skills

- `serverless-patterns` — for general serverless architecture patterns applicable to Vercel functions
- `cloudflare-worker-patterns` — for edge computing on Cloudflare (alternative to Vercel edge)
- `firebase` — for Firebase-backed applications that may deploy frontend to Vercel

# Anti-patterns

- Committing `.env.production` or `.vercel` directory to the repository
- Using `vercel --prod` from local machines as the primary deploy method instead of git-based deploys
- Setting all environment variables for all environments instead of scoping to dev/preview/production
- Writing edge functions that import Node.js-only modules (edge runtime has limited API surface)
- Ignoring preview deployments and deploying directly to production without PR review
- Configuring long `maxDuration` on functions "just in case" — this hides performance problems and increases cost

# Failure handling

- If the build fails, check the Vercel build logs in the dashboard. Common causes: missing env vars (add them in Vercel settings), dependency install failures (check lockfile), or framework version mismatch.
- If a function returns 500, check Vercel function logs (dashboard → Deployments → Functions tab). Look for unhandled exceptions, missing env vars, or timeout.
- If the custom domain shows a DNS error, verify the DNS records with `dig example.com` and confirm propagation. Vercel requires up to 48 hours for DNS propagation but usually completes in minutes.
- If preview deployments are not triggered, verify the GitHub/GitLab integration is connected and the branch is not excluded in project settings.
- If an edge function fails with "unsupported API" errors, check that all imports are edge-compatible — replace Node.js-only modules with edge-compatible alternatives (e.g., use `crypto.subtle` instead of Node.js `crypto`).
