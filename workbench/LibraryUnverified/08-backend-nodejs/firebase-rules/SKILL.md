---
name: firebase-rules
description: >-
  Write, test, and deploy Firestore and Cloud Storage security rules.
  Use when creating match/allow rules, validating request.auth and resource.data fields,
  implementing custom-claims-based RBAC, writing rule unit tests with @firebase/rules-unit-testing,
  or debugging silent denies in the Firebase Emulator.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: firebase-rules
  maturity: draft
  risk: medium
  tags: [firebase, security-rules, firestore, cloud-storage, emulator]
---

# Purpose

Write, test, and deploy Firestore and Cloud Storage security rules. This skill covers the full lifecycle: authoring rules with correct syntax, testing them locally with the Firebase Emulator Suite, and deploying with `firebase deploy --only firestore:rules` or `firebase deploy --only storage`.

# When to use this skill

Use this skill when:

- writing or modifying `firestore.rules` or `storage.rules` files
- implementing access control patterns (owner-only, role-based, field-level validation)
- setting up `@firebase/rules-unit-testing` test suites
- debugging why a read/write is being denied in the emulator or production
- reviewing rules for security gaps before deployment
- adding data validation logic inside security rules (type checks, field constraints)

# Do not use this skill when

- the task is about Firestore SDK usage (queries, listeners, CRUD) — use `firebase-sdk` instead
- the task is about Cloud Functions triggers or Admin SDK initialization — use `firebase-sdk` instead
- the task is about BigQuery export or analytics — use `bigquery` instead
- the task is purely about data model design — use `data-model` instead

# Operating procedure

1. **Identify the rules file and version.** Confirm `rules_version = '2';` is at the top. Version 2 is required for collection group queries and recursive wildcards (`{document=**}`).

2. **Map the access pattern to match paths.** Structure `match` blocks to mirror the Firestore document hierarchy. Use specific paths (`/users/{userId}`) before wildcards.

   ```
   match /databases/{database}/documents {
     match /users/{userId} {
       allow read: if request.auth != null && request.auth.uid == userId;
       allow write: if request.auth != null && request.auth.uid == userId;
     }
   }
   ```

3. **Choose the right granularity.** Split `read` into `get` and `list`. Split `write` into `create`, `update`, and `delete`. This prevents users from listing all documents when they should only get their own.

4. **Add data validation for writes.** Use `request.resource.data` (the incoming document) to validate fields on `create` and `update`:

   ```
   allow create: if request.resource.data.keys().hasAll(['name', 'email'])
                  && request.resource.data.name is string
                  && request.resource.data.name.size() > 0
                  && request.resource.data.name.size() <= 100;
   ```

5. **Use helper functions for reuse.** Extract common checks into functions at the top of the rules file:

   ```
   function isOwner(userId) {
     return request.auth != null && request.auth.uid == userId;
   }
   function hasRole(role) {
     return request.auth != null && request.auth.token[role] == true;
   }
   ```

6. **Start the emulator and run tests.**

   ```bash
   firebase emulators:start --only firestore
   # In another terminal:
   npm test  # runs @firebase/rules-unit-testing suite
   ```

7. **Write rule unit tests.** Each test should authenticate as a specific user context and assert allow/deny:

   ```javascript
   import { initializeTestEnvironment, assertSucceeds, assertFails } from '@firebase/rules-unit-testing';

   const testEnv = await initializeTestEnvironment({
     projectId: 'my-project',
     firestore: { rules: fs.readFileSync('firestore.rules', 'utf8') },
   });

   // Should allow owner to read their own doc
   const ownerCtx = testEnv.authenticatedContext('user123');
   await assertSucceeds(getDoc(doc(ownerCtx.firestore(), 'users/user123')));

   // Should deny other users
   const otherCtx = testEnv.authenticatedContext('other-user');
   await assertFails(getDoc(doc(otherCtx.firestore(), 'users/user123')));
   ```

8. **Deploy rules.** After tests pass:

   ```bash
   firebase deploy --only firestore:rules
   firebase deploy --only storage  # if storage rules changed
   ```

9. **Verify in production.** Check the Firebase Console → Firestore → Rules tab to confirm the deployed version matches. Monitor the Rules Evaluation dashboard for unexpected denies.

# Decision rules

- Always use `rules_version = '2'` — version 1 lacks recursive wildcards and collection group support.
- Prefer `get` + `list` over `read` and `create` + `update` + `delete` over `write` for fine-grained control.
- Validate incoming data shape on `create`; validate only changed fields on `update`.
- Use `request.auth.token` (custom claims) for role-based access, not a Firestore document lookup from rules (rules cannot read other documents efficiently in all cases — use `get()` sparingly as it counts against the 10-call limit per evaluation).
- Never use `allow read: if true` or `allow write: if true` in production.
- Keep rules under ~40KB and nesting under 10 levels to avoid evaluation limits.
- For Storage rules, validate `request.resource.contentType` and `request.resource.size` on uploads.

# Output requirements

1. `Rules File` — the modified `.rules` file with correct match paths and conditions
2. `Test Coverage` — unit tests covering allow and deny paths for each rule
3. `Deployment Command` — the exact `firebase deploy` command to run
4. `Access Matrix` — a summary of who can do what on which paths

# References

Read these when working on specific aspects:

- `references/implementation-patterns.md` — common rule patterns with code examples
- `references/validation-checklist.md` — pre-deployment verification steps
- `references/failure-modes.md` — debugging denies and common mistakes

# Related skills

- `firebase-sdk` — client/server SDK usage for Firestore, Auth, Storage
- `data-model` — Firestore document/collection schema design
- `api-contracts` — API boundary contracts that rules help enforce

# Anti-patterns

- **God rule at root level.** Placing `allow read, write: if true` at the database root and "planning to tighten later." This never happens and exposes all data.
- **Checking auth in some rules but not others.** Inconsistent auth checks across sibling match blocks leave gaps. Use a helper function.
- **Using `resource.data` when you mean `request.resource.data`.** `resource.data` is the *existing* document; `request.resource.data` is the *incoming* data on writes. Confusing them causes rules to reference wrong fields.
- **Not testing deny paths.** Only testing that valid users can access data. You must also test that invalid users, unauthenticated users, and malformed data are denied.
- **Deeply nested recursive wildcards.** Using `{document=**}` at a high level overrides specific match blocks below it. Place recursive wildcards only where you truly need catch-all behavior.
- **Relying on client-side filtering.** Rules are not filters — a query that could return unauthorized documents will fail entirely, even if the user has access to some of those documents. Structure queries to match rule boundaries.

# Failure handling

- **Silent deny with no error detail.** Firebase rules deny silently. Use the Emulator's debug logging (`firebase emulators:start --debug`) and `@firebase/rules-unit-testing` to identify which condition failed. In production, check the Rules Evaluation metrics in the Firebase Console.
- **Rules not deployed after update.** If you edited `firestore.rules` but forgot to deploy, production still runs the old version. Always deploy after changes and verify the timestamp in the Console.
- **Emulator rules out of sync.** The emulator reads rules on startup. If you edit rules after starting the emulator, restart it or use `--import` with the latest rules.
- **Evaluation limit exceeded.** Firestore rules have a limit of ~1000 expressions per evaluation and 10 `get()`/`exists()` calls. Refactor complex conditions into helper functions and minimize cross-document lookups.
- **Custom claims not propagated.** After setting custom claims via Admin SDK, the user must refresh their ID token (sign out/in or `getIdToken(true)`) before rules see the new claims.
