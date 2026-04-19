---
name: gap-analysis
description: Maps current state against desired state to identify missing pieces, incomplete work, and unaddressed requirements. Trigger — "what's missing", "gap analysis", "compare current vs target", "what else do we need". Skip when the target state is undefined or when doing general brainstorming.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: gap-analysis
  maturity: draft
  risk: low
  tags: [gap, analysis]
---

# Purpose
Compares a current state (repo, plan, codebase, capability, process) against a desired target state and produces an explicit inventory of what is missing, incomplete, or wrong. This is the strategic planning tool used in capability planning, migration design, and compliance audits: define where you are, define where you need to be, systematically enumerate the delta.

# When to use this skill
Use when:
- The user says "what's missing?", "gap analysis", "what do we still need?", or "how far are we from done?"
- A scaffold has been generated and needs auditing against requirements
- A repo is being retrofitted to meet a new standard or pattern
- A plan exists but it's unclear what has been implemented vs. what remains
- Evaluating a vendor or tool against required capabilities
- Migration planning: current system vs. target system

Do NOT use when:
- The user wants drift between a spec and implementation with a canonical reference (use `drift-detection`)
- The user wants to understand why something failed (use `root-cause-analysis`)
- Current and target states are identical—there are no gaps
- The user wants logical contradictions in documents (use `contradiction-finder`)

# Operating procedure
1. **Define the current state explicitly**: Inventory what actually exists. Be concrete:
   - For a repo: list files, modules, agents, tools, docs, tests
   - For a plan: list completed milestones, resolved decisions, implemented features
   - For a capability: list current skills, tools, processes in place
   
   Don't assume—inspect and enumerate.

2. **Define the target state explicitly**: Identify the authoritative description of what should exist:
   - A requirements document
   - A scaffold specification
   - A standard or pattern to comply with
   - The user's stated goal
   - A reference implementation
   
   List every item the target state requires.

3. **Map current to target**: For each item in the target state, determine:
   - **Present**: Exists and meets requirements
   - **Partial**: Exists but incomplete or not fully conformant
   - **Absent**: Does not exist

4. **Identify surplus items**: Things in current state not in target state:
   - **Expected addition**: Implementation detail not requiring target-level spec
   - **Potential scope creep**: Should have been discussed
   - **Legacy to remove**: Old artifact that shouldn't exist

5. **Classify each gap by type**:
   - **Structural gap**: Missing file, module, component, or capability
   - **Behavioral gap**: Component exists but doesn't behave as required
   - **Quality gap**: Exists and behaves but doesn't meet quality bar (missing tests, poor error handling, no docs)
   - **Integration gap**: Components exist but aren't connected properly
   - **Dependency gap**: A gap that blocks other items from being filled

6. **Prioritize gaps for filling**:
   1. Dependency gaps first (unblock other work)
   2. Structural gaps (add missing pieces)
   3. Behavioral gaps (fix incorrect behavior)
   4. Integration gaps (connect components)
   5. Quality gaps (polish and harden)

7. **Estimate effort for each gap**:
   - Small: < 1 day of work
   - Medium: 1-3 days
   - Large: > 3 days
   - Unknown: needs investigation before estimating

# Output defaults
A **Gap Inventory** table with columns: Item | Target State | Current State | Gap Type | Effort | Priority

A **Dependency Gaps** section highlighting items that block other work.

A **Recommended Fill Order** list showing optimal sequence for addressing gaps.

A **Surplus Items** section if there are items in current state not in target.

Total summary: "X items present, Y partial, Z absent. Estimated total effort: N days."

# Named failure modes of this method

- **Vague target state**: Performing gap analysis against a target that is itself incomplete or ambiguous, producing gaps that are really just specification gaps. Fix: if the target is underspecified, say so and list what target definitions are needed before gap analysis can be meaningful.
- **Inventory-only analysis**: Listing present/absent items without classifying gap type (structural vs. behavioral vs. quality vs. integration). Fix: always classify, because the gap type determines the fill strategy.
- **Effort sandbagging**: Marking everything as "Large" or "Unknown" to avoid committing to estimates. Fix: use the simplest honest estimate; if genuinely unknown, name the specific investigation needed.
- **Missing dependency gaps**: Filling gaps in arbitrary order instead of identifying which gaps block others. Fix: always identify dependency gaps first (step 6).
- **Surplus blindness**: Focusing only on what's missing while ignoring what exists but shouldn't—scope creep or legacy artifacts. Fix: always scan for surplus items (step 4).

# References
- Strategic gap analysis frameworks (McKinsey, BCG)
- Capability maturity models (CMMI)
- Migration planning methodologies
- NIST SSDF (https://csrc.nist.gov/Projects/ssdf) — software development framework gap assessment

# Failure handling
If the target state is not defined:
1. State that gap analysis cannot proceed without a target
2. List what documents or specifications are available
3. Ask for the target reference before proceeding

If target state is partially defined:
1. Complete analysis for defined portions
2. Clearly mark which areas could not be analyzed
3. List what additional target specifications are needed
