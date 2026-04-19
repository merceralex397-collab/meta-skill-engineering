---
name: firebase
description: "Configure and develop on Firebase — set up Authentication providers, write Firestore security rules, deploy Hosting sites, build Cloud Functions, manage Storage buckets, and test with the Emulator Suite. Use when working with firebase.json, Firestore rules, Firebase Auth flows, Cloud Functions triggers, or Firebase CLI commands. Do not use for raw GCP services outside the Firebase SDK surface or non-Firebase auth providers."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: firebase
  maturity: draft
  risk: low
  tags: [firebase, firestore, cloud-functions, firebase-auth, hosting]
---

# Purpose

Configure and develop on the Firebase platform — set up Authentication providers, write and test Firestore security rules, deploy Firebase Hosting sites, build Cloud Functions with triggers, manage Cloud Storage buckets, and validate everything locally with the Firebase Emulator Suite.

# When to use this skill

- Creating or editing `firebase.json`, `.firebaserc`, or `firestore.rules`.
- Writing Firestore security rules to control read/write access per collection and document.
- Configuring Firebase Authentication providers (Email/Password, Google, GitHub, anonymous).
- Building Cloud Functions triggered by Firestore writes, Auth events, HTTP requests, or Pub/Sub.
- Deploying a static site or SSR app to Firebase Hosting with rewrites and redirects.
- Configuring Cloud Storage security rules and bucket CORS policies.
- Running the Firebase Emulator Suite for local development and testing.
- Using the Firebase Admin SDK in server-side code for privileged operations.

# Do not use this skill when

- The task involves raw GCP services (BigQuery, Cloud Run, Pub/Sub) outside the Firebase SDK — prefer `gcp`.
- The authentication system uses a non-Firebase provider (Auth0, Clerk, Supabase Auth) — prefer the relevant auth skill.
- The task is about generic serverless patterns not tied to Firebase Cloud Functions — prefer `serverless-patterns`.
- The focus is on Cloudflare or Vercel deployment — prefer those respective skills.

# Operating procedure

1. **Locate the Firebase config.** Find `firebase.json` in the repo root. If absent, run `firebase init` and select the needed services (Firestore, Functions, Hosting, Storage, Emulators).
2. **Verify the project binding.** Check `.firebaserc` for the correct project aliases (default, staging, production). Switch projects with `firebase use <alias>`.
3. **Write Firestore security rules.** Edit `firestore.rules`. Structure rules per collection path: `match /users/{userId}` with conditions like `request.auth.uid == userId`. Use `request.resource.data` to validate incoming writes. Use `resource.data` to check existing document fields.
4. **Test rules with the Emulator.** Start the Firestore emulator: `firebase emulators:start --only firestore`. Write rule unit tests using `@firebase/rules-unit-testing` — test allow and deny cases for each rule path.
5. **Configure Authentication.** In the Firebase Console, enable the required sign-in providers. For Email/Password, enable email enumeration protection. For OAuth providers, set the OAuth client ID and secret. In code, initialize auth with `getAuth(app)` and use `signInWithPopup()`, `signInWithEmailAndPassword()`, or `signInAnonymously()`.
6. **Build Cloud Functions.** Write functions in `functions/src/index.ts`. Use `onDocumentCreated()`, `onDocumentUpdated()` for Firestore triggers. Use `onRequest()` for HTTP triggers. Use `onCall()` for callable functions with automatic auth context. Set the runtime to Node.js 20 in `functions/package.json`.
7. **Configure function secrets and env vars.** Use `firebase functions:secrets:set MY_SECRET` for sensitive values. Access via `process.env.MY_SECRET` in function code. Use `.env.<project>` files for non-sensitive environment config.
8. **Set up Firebase Hosting.** Configure `firebase.json` hosting section: set `public` directory, add `rewrites` for SPA routing (`{"source": "**", "destination": "/index.html"}`), and add API rewrites to Cloud Functions (`{"source": "/api/**", "function": "api"}`).
9. **Configure Cloud Storage rules.** Edit `storage.rules`. Set max file size with `request.resource.size < 5 * 1024 * 1024`. Restrict uploads by content type: `request.resource.contentType.matches('image/.*')`. Require authentication: `request.auth != null`.
10. **Run the full Emulator Suite.** Start all emulators: `firebase emulators:start`. Connect your app to emulators using `connectFirestoreEmulator()`, `connectAuthEmulator()`, and `connectFunctionsEmulator()`. Run integration tests against the emulators.
11. **Deploy incrementally.** Deploy specific services: `firebase deploy --only firestore:rules`, `firebase deploy --only functions:myFunction`, `firebase deploy --only hosting`. Avoid `firebase deploy` without flags — it deploys everything.
12. **Verify the deployment.** Check the Hosting URL for the deployed site. Test Cloud Functions via their HTTP endpoints. Verify Firestore rules by attempting reads/writes from the client. Check the Firebase Console for function logs and error rates.

# Decision rules

- Always test Firestore rules with the Emulator before deploying — rule errors can lock out all users or expose all data.
- Use `onCall` functions over `onRequest` when the client is a Firebase app — `onCall` provides automatic auth context and input validation.
- Use the Admin SDK for server-side operations that bypass security rules (migrations, batch operations, admin dashboards).
- Deploy rules and functions separately from hosting — a hosting deploy should not accidentally change security rules.
- Use Firestore composite indexes only when queries require them — the emulator will log index creation URLs when a query needs one.
- Prefer Firestore real-time listeners (`onSnapshot`) for live UI updates; use `getDoc`/`getDocs` for one-time reads.
- Set Firestore security rules to deny-by-default — explicitly allow only the paths and operations needed.

# Output requirements

1. **Firebase configuration** — `firebase.json` with services, rewrites, and emulator ports configured.
2. **Security rules** — `firestore.rules` and/or `storage.rules` with per-path access control.
3. **Cloud Functions** — function code with trigger type, runtime config, and secret references.
4. **Rule tests** — unit tests covering allow and deny cases for each rule path.
5. **Deploy commands** — the specific `firebase deploy --only` commands used.
6. **Verification** — confirmed the deployed resources are accessible and rules enforce correctly.

# References

- Firebase CLI reference: https://firebase.google.com/docs/cli
- Firestore security rules: https://firebase.google.com/docs/firestore/security/get-started
- Cloud Functions for Firebase: https://firebase.google.com/docs/functions
- Firebase Emulator Suite: https://firebase.google.com/docs/emulator-suite
- Firebase Hosting configuration: https://firebase.google.com/docs/hosting/full-config
- `references/preflight-checklist.md`

# Related skills

- `gcp` — GCP services outside the Firebase SDK (Cloud Run, BigQuery, IAM).
- `serverless-patterns` — generic serverless architecture design.
- `vercel` — alternative hosting and serverless platform.

# Anti-patterns

- Deploying Firestore rules without testing them in the Emulator first — can lock out all users.
- Using `allow read, write: if true;` in production rules — exposes the entire database.
- Putting the Firebase Admin SDK private key in client-side code — it grants full access to all Firebase services.
- Running `firebase deploy` without `--only` flags — accidentally deploys rules, functions, and hosting together.
- Hardcoding the Firebase project config in source instead of using environment-based `.firebaserc` aliases.
- Not connecting to emulators in test/dev environments — tests hit production data.

# Failure handling

- If `firebase deploy` fails with permission errors, verify the active project with `firebase use` and check that the CLI is authenticated with `firebase login`.
- If Firestore rules deny a request that should be allowed, use the Rules Playground in the Firebase Console to simulate the request and inspect the evaluation path.
- If Cloud Functions fail to deploy, check the function runtime version matches the supported Node.js version and that `functions/package.json` dependencies install cleanly.
- If the Emulator Suite fails to start, check for port conflicts and ensure Java 11+ is installed (required for the Firestore emulator).
- If the task involves raw GCP services (BigQuery, Cloud Run, Pub/Sub) not exposed through the Firebase SDK, redirect to the `gcp` skill.
