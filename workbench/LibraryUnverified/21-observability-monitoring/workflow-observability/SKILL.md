---
name: workflow-observability
description: Inspect provenance, workflow state, invocation logs, and recent transitions to report agent and workflow health. Trigger on "workflow status", "what's blocked", "agent health", "workflow state", "task progress", or when repo health is in question. Do not use for repo-process-doctor (diagnosing specific process failures), ticket-execution (executing a single ticket), or project-context (loading project docs).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: workflow-observability
  maturity: draft
  risk: low
  tags: [workflow, observability]
---

# Purpose
Provides structured status reporting for multi-step agent workflows using observability principles from distributed systems. Answers three questions at any point: What's currently running? What's blocked and why? What completed and with what result? Enables managers and coordinating agents to see workflow state without interrupting executing agents.

# When to use this skill
Use when:
- Multiple agents or tasks are executing in parallel and you need consolidated status
- A workflow has stalled and you need to identify the blocking step
- Debugging why a multi-step process produced unexpected results
- Building dashboards or status reports for long-running autonomous work

Do NOT use when:
- Single-step tasks with obvious pass/fail outcomes
- Real-time performance monitoring (use actual APM tools)
- The workflow has no persistent state to inspect

# Operating procedure

## Phase A: Repo-local state inspection (Scafforge provenance)
When the repo has `.opencode/` infrastructure, read these files in order:

1. `.opencode/meta/bootstrap-provenance.json` — how the repo was scaffolded
2. `.opencode/state/invocation-log.jsonl` — if it exists; if not, report "no invocation data yet" explicitly instead of implying healthy
3. `.opencode/state/last-ticket-event.json` — if it exists
4. `.opencode/state/workflow-state.json` — current machine-readable state
5. `tickets/manifest.json` — ticket system manifest

Produce these required output sections from the provenance data:
1. **Bootstrap** — scaffold origin, version, and template used
2. **Observed Usage** — which agents, tools, skills have been invoked
3. **Missing Or Never-Seen Surfaces** — declared but never-invoked agents/tools/skills
4. **Workflow Drift Risks** — mismatches between declared and actual behavior
5. **Next Fix** — single highest-priority corrective action

If any file has `pending_process_verification: true`, flag it prominently — the process change has not yet been validated in practice.

## Phase B: Distributed workflow observability
For general multi-agent or multi-step workflows (with or without `.opencode/`):

1. **Identify state sources**: Locate workflow state artifacts:
   - `tickets/BOARD.md` — Kanban status
   - `tickets/TICKET-INDEX.md` — all tickets with current status
   - `.opencode/workflow-state.json` — if exists, machine-readable state
   - Git log — recent commits show actual work done
   - Session logs — if available in `.opencode/logs/`

2. **Build current snapshot** using three observability signals:

   **Traces** (what path did work take):
   ```bash
   git log --oneline -20 --all  # Recent commits across branches
   git branch -v                 # Branch positions
   ```
   
   **Metrics** (aggregate counts):
   - Tickets by status: `grep -c "status:" tickets/*.md | sort`
   - Files changed today: `git diff --stat @{yesterday}`
   
   **Logs** (discrete events):
   - Parse any session logs for: started, completed, failed, blocked events
   - Extract error messages and their timestamps

3. **Produce status report**:
   ```markdown
   # Workflow Status: [timestamp]
   
   ## Currently Running
   | Task | Agent | Started | Duration |
   |------|-------|---------|----------|
   | TKT-003 | implementer | 10:15 | 45m |
   
   ## Blocked
   | Task | Blocked By | Since | Action Needed |
   |------|------------|-------|---------------|
   | TKT-005 | TKT-003 | 09:00 | Wait for dependency |
   
   ## Recently Completed
   | Task | Result | Duration | Artifacts |
   |------|--------|----------|-----------|
   | TKT-002 | ✓ done | 2h | src/auth.ts |
   
   ## Anomalies
   - [Any stuck tasks, unusual durations, repeated failures]
   ```

4. **Detect workflow smells**:
   - Task running >2x expected duration → flag as potential stuck
   - Same task failed >2 times → flag as systemic issue
   - Circular dependency detected → flag as blocking
   - No progress in >1 hour with tasks in "in_progress" → flag as stalled

5. **Correlate with git state**:
   ```bash
   git status --porcelain  # Uncommitted changes
   git stash list          # Stashed work
   git diff --cached       # Staged but not committed
   ```
   Uncommitted changes during "completed" status = incomplete task.

# Output defaults
```markdown
# Workflow Observability Report
Generated: [ISO timestamp]

## Summary
- Running: N tasks
- Blocked: N tasks  
- Completed (24h): N tasks
- Health: [GREEN|YELLOW|RED]

## Details
[Sections as above]

## Recommended Actions
1. [Specific action for any RED items]
```

# References
- OpenTelemetry Observability Primer: https://opentelemetry.io/docs/concepts/observability-primer/
- Observability signals: traces (request paths), metrics (aggregates), logs (discrete events)

# Failure handling
- **No state files found**: Report "No workflow state infrastructure detected. Recommend initializing ticket system with ticket-pack-builder."
- **State files stale**: If BOARD.md last modified >24h ago but git shows recent commits, report "State files out of sync with actual work. Manual reconciliation needed."
- **Conflicting signals**: If ticket says "done" but tests fail, report both signals and flag for human review
