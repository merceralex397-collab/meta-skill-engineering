# Failure Modes — Firebase Security Rules

## Rules too permissive — `allow read/write: if true`

**Symptom:** Any user (or no user) can read/write any document.
**Cause:** Placeholder rules left from development, or overly broad wildcard match at root.
**Fix:** Replace with specific auth checks. Search rules file for `if true` and eliminate every instance.
**Prevention:** CI check that greps for `if true` in rules files and fails the build.

## Missing auth check on a match block

**Symptom:** Unauthenticated users can access data.
**Cause:** Rule uses `allow read;` or `allow read: if someCondition` without checking `request.auth != null`.
**Fix:** Add `request.auth != null &&` to every condition, or use a helper function.
**Prevention:** Lint rules: every `allow` must reference `request.auth` or call a function that does.

## Rules not deployed after update

**Symptom:** Changes to `firestore.rules` have no effect in production.
**Cause:** Developer edited the file but forgot to run `firebase deploy --only firestore:rules`.
**Fix:** Deploy immediately. Add deployment to CI/CD pipeline.
**Prevention:** Include rules deployment in the standard deploy script; never deploy code without rules.

## Emulator rules out of sync

**Symptom:** Tests pass locally but behavior differs in production, or vice versa.
**Cause:** Emulator reads rules on startup. If rules file changed after start, emulator uses old rules.
**Fix:** Restart the emulator after every rules change.
**Prevention:** Use a file watcher or test script that restarts the emulator before running tests.

## Evaluation limits exceeded

**Symptom:** Requests fail with PERMISSION_DENIED even though rules look correct.
**Cause:** Rules exceed ~1000 expression evaluations or 10 `get()`/`exists()` calls per request.
**Fix:** Simplify rules. Move complex authorization logic to Cloud Functions. Denormalize data to avoid cross-document lookups.
**Prevention:** Keep rules flat. Use custom claims instead of document lookups for role checks.

## Silent deny with no debug info

**Symptom:** Client gets `FirebaseError: Missing or insufficient permissions` with no detail about which rule failed.
**Cause:** Firebase does not expose which rule condition failed for security reasons.
**Debug approach:**
1. Reproduce in the emulator with `--debug` flag: `firebase emulators:start --debug`
2. Use `@firebase/rules-unit-testing` to isolate the exact condition
3. Add/remove conditions one at a time to find the failing check
4. Check the Firestore Rules Evaluation dashboard in Firebase Console for deny patterns

## Confusing `resource.data` with `request.resource.data`

**Symptom:** Rules allow or deny unexpectedly on writes.
**Cause:** `resource.data` is the document *as it currently exists* in Firestore. `request.resource.data` is the document *as it will exist after the write*. Using the wrong one checks stale or nonexistent data.
**Fix:** Use `request.resource.data` for validating incoming data on create/update. Use `resource.data` to check existing document state (e.g., ownership, status).
**Key distinction:**
- `create`: `resource.data` does not exist yet → use `request.resource.data`
- `update`: `resource.data` is the old doc, `request.resource.data` is the new doc
- `delete`: `resource.data` is the doc being deleted, `request.resource.data` does not exist

## Custom claims not visible in rules

**Symptom:** `request.auth.token.admin` returns null/undefined even after calling `setCustomUserClaims()`.
**Cause:** Custom claims are embedded in the ID token. After setting claims, the user's existing token still has the old claims.
**Fix:** Force token refresh: client calls `firebase.auth().currentUser.getIdToken(true)` or user signs out and back in.
**Prevention:** Document the refresh requirement in your auth flow. Consider showing a "refresh required" message.

## Query does not match rule scope

**Symptom:** A query returns PERMISSION_DENIED even though individual documents would pass the rule.
**Cause:** Firestore rules evaluate against the *query* constraints, not individual documents. If a query could potentially return unauthorized documents, the entire query is denied.
**Example:** Rule requires `request.auth.uid == resource.data.userId`, but query does not include `.where('userId', '==', currentUser.uid)`. Firestore rejects the query.
**Fix:** Ensure every query includes `where` clauses that match the rule conditions.

## Recursive wildcard override

**Symptom:** Specific match rules are bypassed.
**Cause:** A `match /{document=**}` at a parent level with `allow read: if true` overrides all child rules because rules are OR'd together.
**Fix:** Remove or restrict the recursive wildcard. Place catch-all rules only at the most specific level needed.

## Storage rules: wrong content type

**Symptom:** Upload fails with permission denied.
**Cause:** `request.resource.contentType` does not match the regex in rules. Browser may send different MIME types than expected.
**Fix:** Test with actual upload payloads. Consider allowing common variants (e.g., `image/jpg` alongside `image/jpeg`). Log the actual content type the client sends.
