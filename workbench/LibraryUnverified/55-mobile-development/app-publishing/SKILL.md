---
name: app-publishing
description: >-
  Guides mobile app submission to Google Play and Apple App Store — signing,
  assets, review compliance, release tracks, and version management. Trigger
  phrases: "publish app", "app store submission", "Google Play release",
  "TestFlight", "App Store review", "signing key", "provisioning profile",
  "store listing", "content rating", "data safety form", "phased release",
  "app rejection". Do NOT use for: app development or coding (use relevant
  coding skills), UI/UX design (use image-prompt-direction), market research
  for app ideas (use market-research), or financial tracking of app revenue
  (use financial-tracker-ops).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: app-publishing
  maturity: draft
  risk: low
  tags: [mobile, app-store, google-play, ios, android, publishing, release]
---

# Purpose

Produce platform-specific submission checklists, asset manifests, and readiness reports for publishing mobile applications to Google Play Store and Apple App Store. Every checklist item must reference the specific platform requirement it satisfies, and no submission should proceed until all blocking items are resolved.

# When to use this skill

- The user is preparing to submit an app to Google Play or Apple App Store for the first time.
- A release update needs to go through the store review process.
- The user needs to generate or validate store listing assets (screenshots, feature graphics, app previews).
- Signing keys, certificates, or provisioning profiles need to be set up or rotated.
- The user has received a store rejection and needs to diagnose and resolve it.
- A repo's CI/CD pipeline needs app-publishing steps documented or audited.
- The user asks about release tracks, phased rollouts, TestFlight, or beta distribution.

# Do not use this skill when

- The task is about writing app code, fixing bugs, or implementing features — use relevant coding skills.
- The user needs UI/UX mockups or design assets created from scratch — use `image-prompt-direction`.
- The request is about market research for an app idea — use `market-research`.
- The task is tracking app revenue or financial metrics — use `financial-tracker-ops`.
- The user needs to analyze app analytics data — use `spreadsheet-analysis`.

# Operating procedure

## Step 1 — Identify target platform(s) and release type

Determine the scope:

| Dimension | Options |
|-----------|---------|
| Platform | Google Play, Apple App Store, or both |
| Release type | New app (first submission) or Update (existing listing) |
| Distribution | Production, Beta (closed/open testing), Internal testing |
| Urgency | Standard review, Expedited review (Apple only, limited use) |

## Step 2 — Google Play Console checklist

Complete every item before submission:

### Signing & Build

- [ ] **App signing key:** Enroll in Google Play App Signing (recommended). Upload the upload key to Play Console; Google manages the app signing key. Back up the upload key securely — loss requires key reset through Google support.
- [ ] **AAB format:** Submit as Android App Bundle (.aab), not APK. Play Console requires AAB for new apps.
- [ ] **Version code:** Integer, must be strictly higher than any previously uploaded version code. Cannot be reused.
- [ ] **Version name:** User-facing version string (e.g., "2.1.0"). Follow semantic versioning (see Step 5).

### Store Listing

- [ ] **App title:** Max 30 characters.
- [ ] **Short description:** Max 80 characters.
- [ ] **Full description:** Max 4,000 characters.
- [ ] **Screenshots:** Minimum 2, maximum 8 per device type. Required dimensions:
  - Phone: 16:9 or 9:16 aspect ratio, minimum 320px, maximum 3840px on any side. PNG or JPEG.
  - 7-inch tablet: same requirements (strongly recommended).
  - 10-inch tablet: same requirements (strongly recommended).
- [ ] **Feature graphic:** 1024 × 500 px, PNG or JPEG. Required for Play Store featuring.
- [ ] **App icon:** 512 × 512 px, PNG, 32-bit with alpha. Must match the in-app launcher icon.

### Compliance

- [ ] **Content rating questionnaire:** Complete the IARC questionnaire in Play Console. Failure to complete results in app removal. Answer honestly — misrating is a policy violation.
- [ ] **Data safety form:** Declare all data collected, shared, and security practices. Must accurately reflect app behavior including SDKs and third-party libraries. Review every SDK's data practices.
- [ ] **Target audience and content:** If the app may appeal to children, additional compliance requirements (COPPA, Families Policy) apply.
- [ ] **Privacy policy:** Required if the app collects any personal or sensitive data. Must be accessible via a URL in the store listing AND within the app.
- [ ] **Ads declaration:** Declare whether the app contains ads, including SDK-served ads.

### Release Tracks (progression order)

1. **Internal testing** — Up to 100 testers. No review required. Use for rapid iteration.
2. **Closed testing (alpha/beta)** — Invite-only via email lists or Google Groups. Triggers review.
3. **Open testing (beta)** — Anyone can join. Publicly visible. Triggers review.
4. **Production** — Full public release. Always go through at least closed testing first.

### Review Timeline

- Typical: 1–3 days for established accounts, up to 7+ days for new developer accounts or first submissions.
- Policy violations can add delays or result in rejection.

## Step 3 — Apple App Store Connect checklist

Complete every item before submission:

### Certificates & Signing

- [ ] **Apple Developer Program membership:** Active ($99/year individual, $299/year enterprise). Required before any submission.
- [ ] **Distribution certificate:** Create in Xcode or Apple Developer portal. Maximum 3 active distribution certificates per account.
- [ ] **Provisioning profile:** App Store distribution profile linking the certificate, App ID, and entitlements. Must be regenerated if certificate or entitlements change.
- [ ] **App ID & Bundle ID:** Register in the Developer portal. Bundle ID must match `PRODUCT_BUNDLE_IDENTIFIER` in Xcode exactly. Cannot be changed after first submission.

### Store Listing

- [ ] **App name:** Max 30 characters.
- [ ] **Subtitle:** Max 30 characters.
- [ ] **Description:** No hard character limit, but keep it concise (Apple recommends focusing on key features).
- [ ] **Keywords:** Max 100 characters total, comma-separated.
- [ ] **Screenshots:** Required per device size. Minimum dimensions:
  - 6.7" display (iPhone 15 Pro Max): 1290 × 2796 px or 2796 × 1290 px.
  - 6.5" display (iPhone 14 Plus): 1284 × 2778 px or 2778 × 1284 px.
  - 5.5" display (iPhone 8 Plus): 1242 × 2208 px or 2208 × 1242 px.
  - iPad Pro 12.9" (6th gen): 2048 × 2732 px or 2732 × 2048 px.
  - iPad Pro 12.9" (2nd gen): 2048 × 2732 px or 2732 × 2048 px.
  - Format: PNG or JPEG, no alpha/transparency. Minimum 2, maximum 10 per device.
- [ ] **App preview videos (optional):** 15–30 seconds, recorded on device, matching screenshot dimensions.
- [ ] **App icon:** 1024 × 1024 px, PNG, no alpha, no rounded corners (Apple applies the mask).

### Compliance & Review Guidelines

- [ ] **App Store Review Guidelines compliance:** Review sections 1–5 thoroughly. Common rejection triggers:
  - **Guideline 2.1 (Performance):** App crashes or has bugs. Test exhaustively on physical devices.
  - **Guideline 2.3 (Accurate metadata):** Screenshots don't match actual app UI. Use real app screenshots.
  - **Guideline 3.1.1 (In-App Purchase):** Digital goods/services must use Apple IAP. No redirecting to external payment.
  - **Guideline 4.0 (Design):** App is a thin wrapper around a website. Must provide native value.
  - **Guideline 5.1.1 (Data Collection):** Privacy policy missing or insufficient. Must be accessible before account creation.
  - **Guideline 5.1.2 (Data Use and Sharing):** App Tracking Transparency (ATT) prompt required before tracking.
- [ ] **App Privacy "Nutrition Labels":** Declare all data types collected and linked to identity in App Store Connect.
- [ ] **Age rating:** Complete the questionnaire in App Store Connect. Drives content restrictions.
- [ ] **Export compliance:** If the app uses encryption (including HTTPS), declare via the export compliance information.

### TestFlight (Beta Testing)

- [ ] **Internal testing:** Up to 100 testers from your App Store Connect team. No review required.
- [ ] **External testing:** Up to 10,000 testers via email or public link. Requires Beta App Review (usually faster than full review).
- [ ] **Build expiration:** TestFlight builds expire after 90 days. Plan testing timelines accordingly.

### Release Options

- **Manual release:** You control when the approved build goes live.
- **Automatic release:** Goes live immediately upon approval.
- **Phased release:** Rolls out to 1%, 2%, 5%, 10%, 20%, 50%, 100% of users over 7 days. Can pause (not reverse) during rollout.

### Review Timeline

- Typical: 24–48 hours. Can be longer during holidays or for complex apps.
- Expedited review: Available for critical bug fixes via the Resolution Center. Limited use — Apple tracks frequency.

## Step 4 — Store asset requirements (consolidated reference)

| Asset | Google Play | Apple App Store |
|-------|------------|----------------|
| App icon | 512 × 512 px, PNG, 32-bit with alpha | 1024 × 1024 px, PNG, no alpha, no rounded corners |
| Feature graphic | 1024 × 500 px, PNG/JPEG | N/A |
| Phone screenshots | Min 320px, max 3840px, 16:9 or 9:16, PNG/JPEG | Device-specific (see Step 3), PNG/JPEG, no alpha |
| Tablet screenshots | Same as phone (recommended) | iPad-specific sizes required if app runs on iPad |
| Video preview | YouTube link (optional, store listing) | 15–30s device recording, device-resolution, no alpha |
| Screenshot count | 2–8 per device type | 2–10 per device size |

## Step 5 — Version management

Follow **semantic versioning** (MAJOR.MINOR.PATCH):

- **MAJOR:** Breaking changes, major feature overhaul, redesign.
- **MINOR:** New features, backward-compatible additions.
- **PATCH:** Bug fixes, minor improvements.

Platform-specific rules:

- **Android `versionCode`:** Monotonically increasing integer. Increment by 1 for each upload. Never reuse. Independent of the user-facing version name.
- **iOS `CFBundleVersion` (build number):** Must be unique per bundle ID per platform. Use build counter (e.g., "1", "2", "3") or timestamp-based (e.g., "20240115.1"). Must increase for each TestFlight/App Store upload.
- **iOS `CFBundleShortVersionString`:** The user-facing version (e.g., "2.1.0"). Must increase for App Store submissions (not required to increase for TestFlight builds within the same version).

## Step 6 — Common rejection reasons and prevention

| # | Rejection reason | Platform | Prevention |
|---|-----------------|----------|------------|
| 1 | Crashes / bugs in review | Both | Test on physical devices matching reviewer environments. Apple uses current-gen devices. |
| 2 | Misleading metadata / screenshots | Both | Only use actual in-app screenshots. Don't show features that aren't in the submitted build. |
| 3 | Missing privacy policy | Both | Add privacy policy URL to store listing AND in-app settings before submission. |
| 4 | Incomplete data safety / privacy labels | Both | Audit every SDK and API call for data collection. Err on the side of over-declaring. |
| 5 | In-app purchase bypass (Apple) | iOS | All digital goods/services must use Apple IAP. Physical goods and "reader" apps have exemptions. |
| 6 | Insufficient content / webview wrapper | iOS | Provide meaningful native functionality. A WebView of your website is not an app. |
| 7 | Missing ATT prompt (Apple) | iOS | Implement `AppTrackingTransparency` before any tracking. Must request permission before accessing IDFA. |
| 8 | Incorrect content rating | Both | Re-take the rating questionnaire if app content has changed. Misrating leads to removal. |
| 9 | Missing upload key / expired certificate | Both | Securely back up signing keys. Set calendar reminders for certificate expiration dates. |
| 10 | Target API level too low (Google) | Android | Google requires targeting a recent Android API level (currently API 34+). Check current requirements. |

# Decision rules

- **Never submit to production without beta testing first.** Use internal testing → closed testing → open testing → production (Google) or internal TestFlight → external TestFlight → App Store (Apple).
- **Always verify signing credentials before upload day.** Expired certificates or lost upload keys block releases and can take days to resolve.
- **Screenshots must be from the submitted build.** If the UI changed since screenshots were taken, retake them. Mismatched screenshots trigger rejection.
- **When in doubt about data collection, over-declare.** Under-declaring data practices is a policy violation. Over-declaring is conservative but safe.
- **Phased release is the default recommendation for production updates.** Unless the update is a critical security fix, use phased rollout to catch issues before 100% exposure.
- **Version codes/build numbers must always increase.** Plan numbering schemes before the first release. Retrofitting is painful.
- **If an app targets children under 13, apply COPPA/GDPR-K compliance before anything else.** This affects every other checklist item.

# Output structure

Every app-publishing deliverable must use one of these formats:

## Platform Checklist

```
## [Platform] Submission Checklist: [App Name] v[Version]
- Release type: New / Update
- Distribution: Internal / Closed Beta / Open Beta / Production
- Target date: YYYY-MM-DD

### Signing & Build
- [ ] [Item] — Status: ✅ Done / ⚠️ In progress / ❌ Blocked ([reason])

### Store Listing
- [ ] [Item] — Status

### Compliance
- [ ] [Item] — Status

### Testing
- [ ] [Item] — Status

### Blocking issues: [count]
- [List any ❌ items with resolution steps]
```

## Asset Manifest

```
## Store Asset Manifest: [App Name] v[Version]

| Asset | Required spec | File | Status |
|-------|--------------|------|--------|
| App icon (Google) | 512×512 PNG, alpha | assets/icon-512.png | ✅ |
| App icon (Apple) | 1024×1024 PNG, no alpha | assets/icon-1024.png | ❌ Missing |
| Feature graphic | 1024×500 PNG/JPEG | assets/feature.png | ✅ |
| [Phone screenshots] | [spec] | [files] | [status] |

### Missing assets: [count]
### Action items: [list of assets to create/fix]
```

## Submission Readiness Report

```
## Submission Readiness: [App Name] v[Version] → [Platform]
- Assessment date: YYYY-MM-DD
- Overall status: READY / NOT READY

### Readiness summary
| Category | Items | Complete | Blocking |
|----------|-------|----------|----------|
| Signing | X | X | X |
| Store listing | X | X | X |
| Compliance | X | X | X |
| Testing | X | X | X |
| Assets | X | X | X |

### Blocking items
1. [Item] — [What's needed] — [Estimated resolution time]

### Risks
- [Risk] — [Mitigation]

### Recommended submission date: YYYY-MM-DD
### Expected review completion: YYYY-MM-DD (based on [typical/expedited] review)
```

# Anti-patterns

- **Skipping beta testing:** Submitting directly to production without internal or closed testing. Store reviewers are not QA testers — they will reject buggy apps, and re-review adds days of delay.
- **Incomplete privacy policies:** A one-paragraph privacy policy that doesn't cover actual data practices. Must specifically describe what data is collected, how it's used, who it's shared with, and how users can request deletion.
- **Missing accessibility:** No VoiceOver/TalkBack support, no dynamic type support, no sufficient color contrast. Both platforms increasingly enforce accessibility. Apple Guideline 1.0 and Google's accessibility best practices are review factors.
- **Hardcoded version numbers:** Maintaining version numbers in multiple places without automation. Use build systems (Gradle `versionCode`/`versionName`, Xcode build settings, or CI environment variables) as single source of truth.
- **Last-minute certificate panic:** Discovering on submission day that a certificate expired or an upload key was lost. Maintain a signing credentials inventory with expiration alerts.
- **Ignoring rejection feedback:** Resubmitting with minimal changes and hoping for a different reviewer. Read the rejection reason carefully, fix the root cause, and include a reviewer note explaining the changes.
- **Screenshot fraud:** Using mockups, pre-release UI, or competitor screenshots. Both platforms require screenshots of the actual submitted build running on real (or official simulator) hardware.

# Related skills

- `image-prompt-direction` — Creating app icon concepts, feature graphics, and promotional imagery.
- `business-idea-evaluation` — Evaluating whether an app idea is worth the publishing investment.
- `market-research` — Researching the competitive landscape before choosing a category and positioning.
- `competitor-teardown` — Analyzing competing apps' store listings, ratings, and feature sets.

# Failure handling

- If signing keys are lost or compromised, document the platform-specific recovery process (Google: upload key reset request; Apple: revoke and reissue certificate, update provisioning profiles) and estimate the timeline impact.
- If the app is rejected, parse the rejection reason against the common rejections table (Step 6), produce a specific remediation plan, and estimate the time to resolve and resubmit.
- If store asset requirements have changed since last submission, audit all assets against current specs (Step 4) before uploading.
- If the user is unsure which platform to target first, recommend starting with Android (faster review, more forgiving process) unless the target audience is iOS-heavy, then provide a dual-platform timeline.
- If compliance requirements are unclear (e.g., COPPA applicability), recommend consulting legal counsel and flag the submission as blocked on legal review rather than guessing.
