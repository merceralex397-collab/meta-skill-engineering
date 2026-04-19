---
name: security-threat-model
description: "Repository-grounded threat modeling that enumerates trust boundaries, assets, attacker capabilities, abuse paths, and mitigations, and writes a concise Markdown threat model. Trigger on 'threat model', 'enumerate threats', 'abuse paths', 'AppSec threat modeling', or 'attack surface analysis'. Do not use for security-best-practices (language-specific secure coding), security-hardening (OWASP Top 10 controls), or security-ownership-map (git ownership analysis)."
---

# Purpose
Deliver an actionable AppSec-grade threat model specific to a repository or project path — not a generic checklist. Anchor every architectural claim to repo evidence, keep assumptions explicit, and prioritize realistic attacker goals over boilerplate.

# Operating procedure

## 1. Collect inputs
- Repo root path and any in-scope paths.
- Intended usage, deployment model, internet exposure, and auth expectations (if known).
- Any existing repository summary or architecture spec.
- Use prompts in `references/prompt-template.md` to generate a repository summary.
- Follow the required output contract in `references/prompt-template.md`. Use it verbatim when possible.

## 2. Scope and extract the system model
- Identify primary components, data stores, and external integrations from the repo summary.
- Identify how the system runs (server, CLI, library, worker) and its entrypoints.
- Separate runtime behavior from CI/build/dev tooling and from tests/examples.
- Map in-scope locations to components; explicitly exclude out-of-scope items.
- Do not claim components, flows, or controls without evidence.

## 3. Derive boundaries, assets, and entry points
- Enumerate trust boundaries as concrete edges between components (protocol, auth, encryption, validation, rate limiting).
- List assets that drive risk (data, credentials, models, config, compute, audit logs).
- Identify entry points (endpoints, upload surfaces, parsers, job triggers, admin tooling, logging sinks).

## 4. Calibrate attacker capabilities
- Describe realistic attacker capabilities based on exposure and intended usage.
- Explicitly note non-capabilities to avoid inflated severity.

## 5. Enumerate threats as abuse paths
- Map attacker goals to assets and boundaries (exfiltration, privilege escalation, integrity compromise, DoS).
- Classify each threat and tie to impacted assets.
- Keep threats small in number but high in quality.

## 6. Prioritize with likelihood × impact
- Use qualitative likelihood and impact (low/medium/high) with short justifications.
- Set overall priority (critical/high/medium/low), adjusted for existing controls.
- State which assumptions most influence the ranking.

## 7. Validate with user
- Summarize key assumptions that affect threat ranking or scope.
- Ask 1–3 targeted questions to resolve missing context (deployment model, authn/authz, data sensitivity, multi-tenancy).
- Pause for feedback. If user declines, state which assumptions remain and how they influence priority.

## 8. Recommend mitigations
- Distinguish existing mitigations (with evidence) from recommended ones.
- Tie mitigations to concrete locations and control types (authZ checks, input validation, schema enforcement, sandboxing, rate limits, secrets isolation, audit logging).
- Prefer specific implementation hints over generic advice.

## 9. Quality check before finalizing
- Confirm all discovered entrypoints are covered.
- Confirm each trust boundary is represented in threats.
- Confirm runtime vs CI/dev separation.
- Confirm user clarifications are reflected.
- Confirm format matches `references/prompt-template.md`.
- Write final Markdown to `<repo-or-dir-name>-threat-model.md`.

## Risk prioritization guidance
- **High**: pre-auth RCE, auth bypass, cross-tenant access, sensitive data exfiltration, key/token theft, model/config integrity compromise, sandbox escape.
- **Medium**: targeted DoS of critical components, partial data exposure, rate-limit bypass with measurable impact, log poisoning affecting detection.
- **Low**: low-sensitivity info leaks, noisy DoS with easy mitigation, issues requiring unlikely preconditions.

# Output defaults
Threat model written to `<repo-or-dir-name>-threat-model.md` following the contract in `references/prompt-template.md`. Includes: system model, trust boundaries, asset inventory, threat table (ID, category, likelihood, impact, priority), mitigations (existing + recommended), assumptions, and open questions.

# References
- Output contract and prompt template: `references/prompt-template.md`
- Optional controls/asset list: `references/security-controls-and-assets.md`
- https://owasp.org/www-community/Threat_Modeling
- https://cheatsheetseries.owasp.org/

Only load reference files you need. Keep the final result concise, grounded, and reviewable.

# Failure handling
- **User won't answer validation questions**: State which assumptions remain unresolved, mark affected recommendations as conditional, and proceed with explicit caveats.
- **Repo too large to fully analyze**: Focus on declared entrypoints and sensitive paths (auth, crypto, API boundaries). State which areas were not analyzed.
- **No clear trust boundaries**: Report that the architecture lacks explicit boundaries — this is itself a finding. Recommend boundary definition as first mitigation.
- **Conflicting architecture signals**: Flag contradictions (e.g., code says microservices, deploy says monolith) and ask user to clarify before finalizing.
