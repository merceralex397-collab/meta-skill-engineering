---
name: linear-address-issue
description: "Read, triage, implement, and update Linear issues through their full lifecycle. Trigger: 'address Linear issue', 'update Linear ticket', 'triage Linear backlog', 'close Linear issue', 'work on LIN-XXX', 'update issue status in Linear', 'create Linear issue from PR'. Do NOT use for GitHub Issues (use gh-address-comments), Jira, or other trackers. Do NOT use for general project management advice unrelated to Linear's specific workflow."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: linear-address-issue
  maturity: draft
  risk: low
  tags: [linear, issue-tracking, triage, workflow, graphql, project-management, cycles]
---

# Purpose

Provide a repeatable procedure for addressing Linear issues — from reading and understanding an issue through implementation, status updates, and resolution. This skill ensures issues move through their lifecycle with proper context, linked artifacts, and accurate status at every stage.

# When to use this skill

Use this skill when:

- a Linear issue needs to be read, understood, and acted upon
- issue status needs to be updated to reflect current work (Backlog → Todo → In Progress → In Review → Done)
- a triage pass is needed on the Linear backlog to prioritize and assign issues
- a PR needs to be linked to a Linear issue for auto-close on merge
- Linear comments need to be written with progress updates, blockers, or implementation notes
- cycle/sprint planning requires capacity estimation against the Linear backlog
- Linear labels, priorities, or project assignments need to be set or corrected

# Do not use this skill when

- the issue tracker is GitHub Issues — use `gh-address-comments` instead
- the task is about Jira, Asana, Notion, or any non-Linear tracker
- the user wants general project management advice without a specific Linear issue context
- the work is about configuring Linear workspace settings, team structure, or integrations (that's admin, not issue addressing)

# Operating procedure

## Step 1 — Read and understand the issue

Before any work, fully parse the issue context:

1. **Read the issue description** — identify the problem statement, expected behavior, and any reproduction steps.
2. **Check acceptance criteria** — if absent, define them before starting work. An issue without acceptance criteria is not ready for implementation.
3. **Review linked issues** — check parent issues, sub-issues, related issues, and blocking/blocked-by relationships.
4. **Check comments** — read all comments for additional context, design decisions, or updated requirements.
5. **Verify assignment and priority** — confirm the issue is assigned to you (or the intended implementer) and priority is appropriate.

Missing acceptance criteria template — add as a comment:

```markdown
## Acceptance Criteria
- [ ] [Specific, testable condition 1]
- [ ] [Specific, testable condition 2]
- [ ] [Edge case or error handling requirement]
- [ ] Tests pass (unit + integration covering new behavior)
- [ ] No regressions in existing functionality
```

## Step 2 — Set status to In Progress

Update the issue status immediately when starting work. Do not leave issues in "Todo" while actively coding.

Via Linear API (GraphQL):

```graphql
mutation {
  issueUpdate(
    id: "issue-uuid"
    input: { stateId: "in-progress-state-uuid" }
  ) {
    success
    issue { identifier title state { name } }
  }
}
```

Via Linear CLI or Git branch naming:
- Branch name convention: `<username>/<issue-identifier>-<short-description>` (e.g., `alice/LIN-123-fix-auth-redirect`)
- Linear's GitHub integration auto-detects branch names matching issue identifiers and can move issues to "In Progress".

## Step 3 — Implement the change

During implementation, follow these practices:

1. **Scope to the issue** — implement only what the issue asks for. If you discover adjacent work, create new Linear issues for it rather than expanding scope.
2. **Reference the issue in commits** — include the issue identifier in commit messages:
   ```
   fix(auth): resolve redirect loop on token expiry

   Fixes LIN-123
   ```
3. **Update the issue with progress** — if implementation takes more than one working session, add a comment:
   ```markdown
   **Progress update** (2024-01-15):
   - ✅ Root cause identified: token refresh race condition
   - ✅ Fix implemented in `src/auth/refresh.ts`
   - 🔄 Writing tests — expect PR by EOD
   - ❌ Blocked: need API credentials for staging env (@bob)
   ```

## Step 4 — Create PR and link to issue

When the implementation is ready:

1. **Create the PR** with the Linear issue identifier in the title or description:
   ```
   PR Title: fix(auth): resolve redirect loop on token expiry [LIN-123]
   ```
   Or in the PR description:
   ```markdown
   Fixes LIN-123

   ## Changes
   - Fixed token refresh race condition
   - Added retry logic with exponential backoff
   ```
2. **Update issue status to "In Review"** — either manually or rely on the GitHub integration's auto-transition.
3. **Link the PR in a Linear comment** — if the integration doesn't auto-link:
   ```markdown
   PR: https://github.com/org/repo/pull/456
   Ready for review. CC @reviewer
   ```

## Step 5 — Handle review feedback

When review comments arrive:

1. **Address each comment** — respond in the PR, not in Linear.
2. **Update the Linear issue** only if review reveals scope changes or blockers:
   ```markdown
   **Review feedback** (2024-01-16):
   - Reviewer requested additional error handling for network timeouts
   - Added try/catch with fallback in commit abc1234
   - Re-requested review
   ```

## Step 6 — Close the issue

After PR merge:

1. **Verify auto-close** — if `Fixes LIN-123` is in the PR description and the GitHub integration is configured, the issue should move to "Done" automatically.
2. **Manual close if needed** — if auto-close doesn't trigger, update status to "Done" and add a closing comment:
   ```markdown
   Resolved in PR #456 (merged to main).
   Deployed to staging — verified fix in staging environment.
   ```
3. **Close as Cancelled** — if the issue is no longer relevant:
   ```markdown
   Cancelled: superseded by LIN-456 which takes a different approach to the auth flow.
   ```

## Step 7 — Triage workflow (for backlog grooming)

When triaging a set of issues:

1. **Filter to untriaged** — issues in "Backlog" or "Triage" with no priority, no assignee, or no cycle.
2. **For each issue, evaluate**:
   - Is the description sufficient to act on? If not, add a comment requesting clarification and label as `needs-info`.
   - Set priority using this rubric:

     | Priority | Criteria |
     |---|---|
     | **Urgent** | Production is broken for users, data loss risk, security vulnerability |
     | **High** | Blocks other planned work, significant user-facing bug, committed deadline |
     | **Medium** | Important improvement, non-blocking bug, planned feature work |
     | **Low** | Nice-to-have, tech debt, minor UX polish |

   - Assign to a team member based on area expertise and current workload.
   - Assign to a cycle if the issue is ready for implementation.
   - Assign to a project if it belongs to a larger initiative.

3. **Label taxonomy** — apply labels consistently:

   | Category | Labels |
   |---|---|
   | Type | `bug`, `feature`, `improvement`, `chore`, `tech-debt` |
   | Area | `frontend`, `backend`, `infra`, `docs`, `design` |
   | Status modifier | `needs-info`, `needs-design`, `blocked`, `duplicate` |

## Step 8 — Cycle and sprint management

When planning a cycle:

1. **Estimate points/sizing** — use issue complexity (not time) as the unit. Simple (1), Medium (2), Complex (3), Epic (5+, should be broken down).
2. **Capacity planning** — team velocity = average points completed per cycle over last 3 cycles. Do not plan more than 80% of velocity (leave room for interrupts).
3. **Carryover policy** — issues not completed by cycle end:
   - If >50% done → carry over to next cycle with a comment explaining remaining work.
   - If not started → move back to backlog and re-prioritize.
   - Never silently carry over — always add a comment explaining why it wasn't completed.

# Decision rules

- **Never start work without reading the full issue** — description, comments, linked issues, and acceptance criteria. Starting from the title alone leads to misimplementation.
- **No issue without acceptance criteria is ready for work** — if criteria are missing, add them (as a comment or edit) before changing status to "In Progress". If you can't define them, the issue needs refinement.
- **Status must reflect reality** — if you're coding, status is "In Progress". If you're waiting for review, status is "In Review". If you stopped working on it, move it back to "Todo". Stale statuses erode trust in the tracker.
- **One issue, one concern** — if you discover additional work during implementation, create new issues. Do not expand scope silently.
- **Link all artifacts** — every issue that results in code should have a linked PR. Every PR should reference its Linear issue. Orphan PRs and orphan issues are failures.
- **Communicate blockers immediately** — if work is blocked, update the issue status, add a comment with the blocker, and @mention the person who can unblock.
- **Close issues conclusively** — a "Done" issue must have either a linked merged PR or a comment explaining the resolution. "Done" with no evidence is meaningless.

# Output requirements

Produce a structured deliverable with these sections:

1. **Issue Summary** — issue identifier, title, current status, priority, assignee, cycle, project. One-line restatement of the problem.
2. **Acceptance Criteria** — explicit, testable conditions for "done" (extracted from issue or defined if missing).
3. **Implementation Notes** — approach taken, key files changed, design decisions made, tradeoffs accepted.
4. **Status Update** — new status with justification, any blockers or dependencies, next action and owner.
5. **Linked Artifacts** — PR link, related issues, deployment status, verification notes.

# Anti-patterns

- **Stale issues (no updates for >1 week)**: an issue marked "In Progress" with no comment or commit activity for 7+ days indicates either silent blocking, context loss, or abandoned work. Update or reassign.
- **Missing acceptance criteria**: starting implementation without clear success conditions leads to scope creep, rework, and "is this done?" debates. Define criteria before coding.
- **Orphan issues (no project or cycle)**: issues floating in the backlog with no project or cycle assignment are invisible to planning. Every issue should either be in a cycle, in a project backlog, or explicitly marked as "icebox."
- **Status not reflecting reality**: "In Progress" issues where no one is working, "In Review" issues with no PR, "Done" issues with no merged code. Status lies destroy trust in the tracker.
- **PR without issue reference**: PRs that don't link to a Linear issue make it impossible to trace why a change was made. Always include the issue identifier.
- **Scope creep in comments**: long comment threads that gradually redefine the issue's scope without updating the description or acceptance criteria. If scope changes, update the issue description.
- **Drive-by triage**: changing priority or assignee without leaving a comment explaining why. Context-free changes are confusing and often reverted.
- **Over-estimation to pad velocity**: inflating point estimates to make cycle completion metrics look better. Estimates should reflect complexity honestly.

# Related skills

- `gh-address-comments` — GitHub Issues and PR comment workflows (similar lifecycle, different platform)
- `frontend-webapp-builder` — frontend implementation work that Linear issues often describe
- `misc-helper` — general helper tasks that may arise during issue triage

# Failure handling

- If the Linear API is unreachable, update status manually in the Linear UI and add a comment noting the API issue.
- If an issue has contradictory requirements in the description vs comments, add a clarification comment tagging the issue creator and set status to "Blocked" with label `needs-info`.
- If an issue is too large to implement in one cycle, break it into sub-issues with clear scope boundaries and link them to the parent.
- If the GitHub integration fails to auto-close an issue on PR merge, close manually and file a bug for the integration team.
- If triage reveals a large number of duplicate issues, merge them: close duplicates with a comment linking to the canonical issue, and consolidate any unique context from the duplicates into the canonical issue's comments.
