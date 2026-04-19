---
name: planner
description: >
  Trigger — "plan this ticket", "break down this task", "create an implementation plan",
  "what steps to implement", "decompose this feature", "scope this work".
  Skip — if the user wants code written (use an implementer skill), if the task is
  searching for prior art (use `github-prior-art-research`), or if the task is
  validating completed work (use `qa-validation`).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: planner
  maturity: draft
  risk: low
  tags: [planner]
---

# Purpose

Converts a single ticket or feature request into a bounded, ordered implementation plan
with concrete steps, file touch lists, dependency edges, and acceptance criteria. Produces
a plan that implementer agents can execute without re-interpreting the original ticket.

# When to use

- A ticket or feature request needs to be broken into ordered implementation steps.
- An implementer agent needs a plan before it can start coding.
- The user asks "how should I implement this" or "what's the plan for this ticket".
- A large task needs scope bounding to prevent scope creep during implementation.

# Do NOT use when

- The user wants code written — hand off to `implementer-hub` or `implementer-node-agent`.
- The task is discovering existing solutions — use `github-prior-art-research`.
- The task is validating completed work — use `qa-validation`.
- The task is updating documentation — use `docs-handoff`.
- The ticket is already a well-defined single-file change with no ambiguity.

# Operating procedure

1. Read the ticket or feature request in full. Extract: title, description, any linked issues, and stated acceptance criteria.
2. Run `find . -maxdepth 3 -type f -name '*.ts' -o -name '*.js' -o -name '*.py' -o -name '*.go' -o -name '*.rs' | head -40` to understand the project structure and identify the primary language.
3. Run `cat README.md 2>/dev/null | head -50` to understand the project purpose and architecture.
4. Identify all files that will need to be created or modified by running `grep -rn '<key-terms-from-ticket>' src/ lib/ app/ 2>/dev/null | head -30` using domain terms from the ticket.
5. Build a **file touch list**: for each file, note whether it is CREATE, MODIFY, or DELETE, and write a one-line description of the change.
6. Identify dependencies between steps: if file A imports from file B, and both need changes, B must be modified first. Run `grep -n 'import.*from' <file-A>` to trace these edges.
7. Order all steps using dependency-first topological sort: types → data models → services → routes/handlers → tests → documentation.
8. For each step, write a concrete action: what to do, which file, what the expected outcome is, and how to verify it (e.g., "test X passes" or "endpoint returns 200").
9. Extract or synthesize acceptance criteria: if the ticket has explicit criteria, list them. If not, derive them from the described behavior (e.g., "user can log in" → "POST /login returns 200 with valid credentials and 401 with invalid").
10. Estimate scope: count the total files touched and lines likely changed. If >20 files or >500 lines, split into 2–3 sub-plans and mark dependencies between them.
11. Run `git --no-pager log --oneline -10` to check recent changes that might conflict with or inform the plan.
12. Compile the final plan document with all sections below.

# Decision rules

- If the ticket is vague (no clear deliverable), ask the user for clarification before producing a plan. List the specific questions.
- If the ticket implies >20 file changes, split into sub-plans rather than producing one monolithic plan.
- If acceptance criteria are missing from the ticket, synthesize them and mark as "derived — needs confirmation".
- If the plan requires a new dependency, include an explicit step for adding it to the manifest.
- Never include "research" or "explore options" as a plan step — that work should be done before planning via `github-prior-art-research`.
- Each step must be completable by a single implementer agent in one pass.

# Output requirements

1. **Ticket Summary** — ticket ID, title, and one-paragraph restatement of the goal.
2. **File Touch List** — table with columns: File Path, Action (CREATE/MODIFY/DELETE), Description.
3. **Implementation Steps** — numbered list where each step has: action, target file(s), expected outcome, and verification method.
4. **Dependency Graph** — list of step-to-step dependencies (e.g., "Step 3 depends on Step 1").
5. **Acceptance Criteria** — numbered list of testable conditions that define "done".
6. **Scope Estimate** — files touched, estimated lines changed, and risk level (low/medium/high).
7. **Open Questions** — any ambiguities in the ticket that need user input before implementation.

# References

- `references/handoff-contract.md` — plan-to-implementer handoff format
- `references/success-criteria.md` — plan quality standards
- `references/anti-patterns.md` — common planning failures (scope creep, missing deps)

# Related skills

- `implementer-hub` — consumes plans and orchestrates multi-file implementation
- `implementer-node-agent` — consumes individual steps from the plan
- `implementer-context` — gathers context needed to inform the plan
- `qa-validation` — validates the implementation against the plan's acceptance criteria

# Failure handling

- If the ticket references files or modules that do not exist, flag each missing reference and ask whether they should be created or if the ticket is outdated.
- If dependency ordering creates a cycle, identify the cycle and suggest extracting shared types into a new common module to break it.
- If the project has no existing tests, include a step for setting up the test infrastructure before writing test steps.
- If the scope estimate exceeds the plan's target (>500 lines), split into sub-plans and present both the split and the rationale to the user.
