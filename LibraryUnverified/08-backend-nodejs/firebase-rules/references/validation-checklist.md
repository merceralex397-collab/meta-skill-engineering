# Validation Checklist — Firebase Security Rules

Use this checklist before deploying any rules change.

## Authentication checks

- [ ] Every `match` block has an explicit auth check (`request.auth != null`) or a documented reason for public access
- [ ] No rule grants `write` access to unauthenticated users
- [ ] Custom claims are verified where role-based access is needed (`request.auth.token.role`, `request.auth.token.admin`)
- [ ] After setting custom claims server-side, documented that user must refresh their ID token

## Read rules

- [ ] `get` and `list` are split where users should access specific docs but not browse collections
- [ ] No `allow read: if true` in production (unless intentionally public data, documented)
- [ ] Collection group queries have corresponding rules at the collection group level
- [ ] Queries are structured to match rule conditions (rules are not filters)

## Write rules

- [ ] `create`, `update`, and `delete` are split rather than using blanket `write`
- [ ] `create` rules validate required fields with `request.resource.data.keys().hasAll([...])`
- [ ] `create` rules restrict allowed fields with `request.resource.data.keys().hasOnly([...])`
- [ ] `update` rules restrict which fields can change using `diff().affectedKeys().hasOnly([...])`
- [ ] Type validation is present for all user-supplied fields (`.is string`, `.is int`, `.is bool`)
- [ ] String length limits are enforced (`field.size() <= maxLen`)
- [ ] Numeric range limits are enforced where applicable
- [ ] Server timestamp enforcement is in place (`request.resource.data.createdAt == request.time`)
- [ ] Owner fields (e.g., `userId`, `authorId`) are locked to `request.auth.uid` on create

## Cross-document lookups

- [ ] `get()` and `exists()` calls are counted (max 10 per evaluation)
- [ ] Paths in `get()`/`exists()` are correct (full path with `/databases/$(database)/documents/...`)
- [ ] Lookups are only used where necessary — prefer data denormalization or custom claims

## Cloud Storage rules

- [ ] Upload size limits are enforced (`request.resource.size < limit`)
- [ ] Content type validation is present (`request.resource.contentType.matches(...)`)
- [ ] Path-based access control matches auth (e.g., `userId` in path matches `request.auth.uid`)
- [ ] Download rules do not expose files to unauthenticated users unless intended

## Emulator testing

- [ ] Tests run against the Firestore Emulator, not production
- [ ] `@firebase/rules-unit-testing` is configured with the correct `projectId`
- [ ] Rules file is read from the same path the emulator uses
- [ ] Allow paths are tested: authenticated owner can access their data
- [ ] Deny paths are tested: unauthenticated access is denied
- [ ] Deny paths are tested: other authenticated users cannot access others' data
- [ ] Deny paths are tested: malformed data (missing fields, wrong types) is rejected
- [ ] `testEnv.cleanup()` is called in afterAll to shut down the test environment
- [ ] Emulator is restarted after rules file changes (rules are read on startup)

## Deployment

- [ ] `firebase deploy --only firestore:rules` runs successfully
- [ ] `firebase deploy --only storage` runs successfully (if storage rules changed)
- [ ] Deployed rules version timestamp is checked in Firebase Console
- [ ] Rules deploy is included in CI/CD pipeline or deployment checklist
- [ ] Old rules are version-controlled (rules file is committed to git)
