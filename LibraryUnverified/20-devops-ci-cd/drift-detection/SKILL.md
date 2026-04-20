---
name: drift-detection
description: Compares implementation or repo state against approved plans to find spec-to-code divergence. Trigger — "has this drifted from the plan", "check for drift", "compare implementation to spec", "are we still on track". Skip when no baseline plan or spec exists to compare against.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: drift-detection
  maturity: draft
  risk: low
  tags: [drift, detection]
---

# Purpose
Compares current implementation or repository state against the approved plan, brief, or specification and identifies where the two have diverged. This is configuration drift detection applied to agent workflows and software projects: the canonical source of truth defines what should exist, and this skill finds where reality has deviated—whether through intentional undocumented changes, accidental omissions, or misinterpretation.

# When to use this skill
Use when:
- The user says "has this drifted from the spec?", "check for drift", "does the code match the plan?", or "audit against the brief"
- A project is midway through execution and compliance with the original brief needs verification
- An agent has been running autonomously and its output needs auditing against its instructions
- A repo is being handed off and actual state needs reconciliation with documentation
- Periodic health checks to ensure implementation stays aligned with design

Do NOT use when:
- Both documents being compared are planning artifacts—no implementation exists yet (use `contradiction-finder`)
- The user wants general consistency checking without a canonical reference
- The plan was intentionally changed—this skill detects unintentional or undocumented deviation
- The user wants to compare two versions of the same document (use git diff)

# Operating procedure
1. **Establish the canonical reference**: Identify the authoritative source of truth:
   - The brief, spec, or approved plan
   - AGENTS.md or agent instructions
   - ADR or decision record
   - Contract or API specification
   
   If multiple candidates exist, ask which is canonical before proceeding. There must be exactly one source of truth.

2. **Extract canonical commitments**: List every explicit commitment in the canonical reference:
   - Deliverables and features
   - Behavioral specifications
   - Conventions and patterns to follow
   - Constraints and boundaries
   - Explicitly excluded scope
   - Quality requirements

3. **Examine current state**: Inspect the actual implementation, file structure, code, or agent outputs against each commitment. For each item, determine:
   - Does it exist?
   - Does it behave as specified?
   - Does it follow stated conventions?

4. **Classify each item**:
   - **Compliant**: Implementation matches the commitment exactly
   - **Partial**: Implementation partially satisfies but is incomplete
   - **Drifted**: Implementation contradicts or ignores the commitment
   - **Added (undocumented)**: Something exists in implementation that was not in the canonical reference

5. **For Added items, sub-classify**:
   - **Acceptable addition**: Reasonable implementation detail not requiring spec-level documentation
   - **Potential scope creep**: Feature or behavior that should have been discussed before adding
   - **Legacy to remove**: Old artifact that no longer belongs

6. **Assess severity of each drift**:
   - **Critical**: Breaks a core requirement, violates a constraint, or introduces security/correctness issues
   - **Significant**: Reduces quality, creates technical debt, or diverges from stated patterns
   - **Minor**: Cosmetic, stylistic, or low-impact divergence

7. **Identify root cause for major drifts**: For each Critical or Significant drift:
   - Intentional deviation needing retrospective approval?
   - Accidental omission during implementation?
   - Misinterpretation of the spec?
   - Spec ambiguity that led to wrong assumption?

# Output defaults
A compliance table with columns: Commitment | Status (Compliant/Partial/Drifted/Added) | Severity | Notes

A **Drift Summary** with counts: X compliant, Y partial, Z drifted, W added

A **Remediation Required** list for all Critical and Significant drifts, with:
- The drift described in one sentence
- Whether to fix implementation or update spec
- Specific action to bring into compliance

# Named failure modes of this method

- **Wrong canonical reference**: Comparing against an outdated or unauthorized version of the spec. Fix: confirm which document is the single source of truth before starting.
- **Completeness illusion**: Checking only the items that are easy to verify (file existence, naming) while skipping behavioral compliance (does it do what the spec says?). Fix: verify behavior, not just structure.
- **False drift from ambiguity**: Flagging implementation choices as drift when the spec was genuinely ambiguous and the implementation made a reasonable interpretation. Fix: classify ambiguous-spec cases separately from clear-spec violations.
- **Missing undocumented additions**: Focusing only on what's missing or wrong while ignoring scope creep—things that exist but were never specified. Fix: always scan for additions, not just omissions.
- **Snapshot bias**: Checking drift at one point in time without considering whether drift is accelerating or being corrected. Fix: note the trend when historical data is available.

# References
- Configuration drift in infrastructure (Terraform, Ansible drift detection patterns)
- Contract testing (Pact, consumer-driven contracts)
- Continuous compliance monitoring practices

# Failure handling
If the canonical reference cannot be determined or is absent:
1. State this explicitly
2. Do not proceed with drift detection without a clear reference point
3. List what documents are available
4. Ask which is authoritative before continuing

If the reference exists but is incomplete (missing sections), perform partial analysis only on documented areas and note which areas could not be checked.
