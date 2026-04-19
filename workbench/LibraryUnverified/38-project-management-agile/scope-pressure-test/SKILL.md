---
name: scope-pressure-test
description: Stress-tests whether requested scope is achievable within the time budget, team capacity, and repo state. Trigger — "can we do all this", "is this scope realistic", "pressure test the plan", "are we overcommitted". Skip when scope is already locked and the question is about execution, not feasibility.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: scope-pressure-test
  maturity: draft
  risk: low
  tags: [scope, pressure, test]
---

# Purpose
Tests whether a proposed scope is achievable given stated constraints—time, team, dependencies, and complexity—and surfaces where scope must be cut or phased. This applies the "fixed time, variable scope" principle from Basecamp's Shape Up methodology: the timeline is the constraint, and scope must flex to fit it.

Unlike estimation (which sizes work for a given scope), scope pressure-testing starts with the constraint and asks: "What can realistically be done in this window? What must be cut?"

# When to use this skill
Use when:
- The user says "is this realistic?", "can we do this in X weeks?", "pressure-test the scope", or "what should we cut?"
- A plan has grown through accretion and needs to be challenged against actual capacity
- A brief from a stakeholder is being evaluated before committing
- A previous sprint or project overran and the new scope needs calibration
- An appetite/time-box has been set and the scope must be shaped to fit it

Do NOT use when:
- The user wants risk identification without scope sizing (use `premortem`)
- The user wants logical consistency checking (use `contradiction-finder`)
- The scope is already agreed and the task is implementation
- No timeline or capacity constraint has been stated—without constraints, anything is "possible"

# Operating procedure
1. **Extract stated deliverables**: List every explicit deliverable, feature, or output the scope requires. Be exhaustive—include everything stated, implied, or "obviously required."

2. **Extract stated constraints**: 
   - Timeline (deadline, sprint length, cycle duration)
   - Team size and composition (who is available, what skills)
   - Budget (if relevant)
   - Existing codebase state (greenfield vs. retrofit, technical debt)
   - External dependencies (other teams, third-party services, approvals)

3. **Size each deliverable independently**: For each item, estimate complexity:
   - **Small**: Hours of work, well-understood, minimal dependencies
   - **Medium**: Days of work, some unknowns, moderate dependencies
   - **Large**: Week+ of work, significant unknowns, complex dependencies
   - **Unknown**: Cannot estimate without more information—flag what's missing

4. **Identify hidden work**: Look for implicit work not listed as a deliverable:
   - Environment setup, CI/CD configuration
   - Integration work between deliverables
   - Testing (unit, integration, e2e)
   - Review cycles, stakeholder sign-offs
   - Data migration or backfill
   - Documentation, runbooks

5. **Total the estimate and compare to timeline**: Add realistic estimates (assume Medium complexity items take 3 days, Large take 7). Compare to available time. Calculate the gap in days/hours.

6. **Identify cut candidates**: Rank deliverables by impact-to-effort ratio. Flag items that are:
   - Low impact, high effort → strong cut candidates
   - "Nice to have" vs. "must have"
   - Dependencies of nothing else → safe to defer

7. **Propose a phased alternative**: If total scope exceeds capacity, propose:
   - **Phase 1**: What fits in the timeline, is self-contained, and delivers value
   - **Phase 2**: What gets deferred explicitly (not forgotten)

8. **Flag non-negotiable blockers**: Items that, if missing, make the rest useless—these stay regardless of time pressure.

# Output defaults
A deliverables table with columns: Deliverable | Complexity | Hidden Work | Cut Candidate?

A **Scope Gap** statement: "Estimated effort: X days. Available time: Y days. Gap: Z days."

A **Recommended Cuts** list with rationale for each.

A **Phase 1 / Phase 2 Split** if applicable, with clear criteria for what made the cut.

# Named failure modes of this method

- **Estimation without constraints**: Sizing deliverables without knowing the timeline, team, or capacity—making the analysis meaningless. Fix: refuse to proceed without at least one concrete constraint; provide conditional analysis if needed.
- **Hidden work blindness**: Accepting the stated deliverable list at face value without checking for integration, testing, review, and setup overhead. Fix: always run the hidden work check (step 4).
- **Uniform sizing**: Rating everything as "Medium" to avoid thinking. Fix: force-rank deliverables by complexity and defend each rating with one sentence.
- **Sacred cow protection**: Refusing to cut high-effort, low-impact deliverables because someone senior asked for them. Fix: rank by impact-to-effort ratio regardless of source; let the decision-maker override, not the analyst.
- **Phase 2 graveyard**: Deferring items to "Phase 2" without a realistic plan to ever do them. Fix: if Phase 2 has no committed timeline, call the deferred items "descoped" not "phased."

# References
- https://basecamp.com/shapeup/1.2-chapter-03 — "Setting the Appetite" and fixed time, variable scope
- Basecamp (2019). Shape Up: Stop Running in Circles — full methodology
- Brooks, F. (1975). The Mythical Man-Month — why adding scope always costs more than expected

# Failure handling
If timeline or team capacity is not stated:
1. Name these as required inputs
2. Provide conditional analysis: "If capacity is X days of engineering time, then Y is achievable"
3. Ask for the constraint before providing definitive recommendations
