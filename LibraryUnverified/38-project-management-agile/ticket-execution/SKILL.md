---
name: ticket-execution
description: Execute ticket-based work using a deterministic stage order, proof requirements, and handoff discipline. Trigger on "pick up ticket", "execute task", "implement issue", "resume ticket work", or when working from issues/epics with a plan-first workflow. Do not use for ticket-pack-builder (creating tickets), review-audit-bridge (reviewing completed work), or workflow-observability (multi-ticket status).
---

# Purpose
Executes a single ticket through defined stages with verification gates between each phase. Implements the Shape Up "hill chart" model: work moves from unknown (uphill/figuring out) to known (downhill/executing) to done. Each transition requires artifact proof—no stage advances on status alone.

# When to use this skill
Use when:
- Picking up a ticket from the backlog to implement
- Resuming work on an in-progress ticket after context switch
- Verifying a ticket is genuinely complete before marking done

Do NOT use when:
- Creating tickets (use ticket-pack-builder)
- Reviewing/auditing completed work (use review-audit-bridge)
- Multiple tickets need parallel coordination (use workflow-observability first)

# Operating procedure

## Stage 1: Claim & Orient (→ in_progress)
1. **Check preconditions**:
   ```bash
   # Verify no blocking dependencies
   grep "depends_on:" tickets/TKT-XXX.md
   # For each dependency, confirm status: done
   ```
2. **Update ticket status**:
   ```yaml
   status: in_progress
   started: [ISO timestamp]
   assignee: [agent-id or human]
   ```
3. **Create working branch**:
   ```bash
   git checkout -b tkt-XXX-[short-description]
   ```
4. **Read ticket fully**: acceptance criteria, constraints, linked context.

## Stage 2: Uphill (figuring out what to do)
This phase ends when you can enumerate all remaining tasks. Work is "uphill" when unknowns remain.

1. **Identify unknowns**: List what you don't yet know how to do.
2. **Investigate**: Read relevant code, run experiments, ask clarifying questions.
3. **Update ticket with findings**:
   ```markdown
   ## Investigation Notes
   - [Finding 1]
   - [Finding 2]
   
   ## Approach
   [Describe chosen implementation approach]
   
   ## Task Breakdown
   - [ ] Task 1
   - [ ] Task 2
   ```
4. **Gate**: Cannot proceed to Stage 3 until task breakdown exists.

## Stage 3: Downhill (executing known work)
All unknowns resolved. Pure execution.

1. **Work through task breakdown**: Check off items as completed.
2. **Commit incrementally**: Each logical change gets a commit.
   ```bash
   git commit -m "tkt-XXX: [what changed]"
   ```
3. **Run verification continuously**:
   ```bash
   npm test  # or equivalent
   npm run lint
   ```
4. **Update progress in ticket**: Note completed tasks.

## Stage 4: Verification (→ review)
1. **Self-check against acceptance criteria**: Each criterion must have evidence.
   ```markdown
   ## Acceptance Evidence
   - [x] Criterion 1: [how verified, e.g., "test in tests/auth.test.ts"]
   - [x] Criterion 2: [screenshot, log output, etc.]
   ```
2. **Run full test suite**: Must pass.
3. **Update ticket status**:
   ```yaml
   status: review
   completed: [ISO timestamp]
   ```
4. **Open PR if applicable**:
   ```bash
   gh pr create --title "TKT-XXX: [title]" --body "Closes TKT-XXX"
   ```

## Stage 5: Done (→ done)
Only after review approval or self-verification for solo work:
1. **Merge branch**:
   ```bash
   git checkout main && git merge --no-ff tkt-XXX-*
   ```
2. **Update ticket**:
   ```yaml
   status: done
   merged: [ISO timestamp]
   ```
3. **Update BOARD.md**: Move ticket to Done column.

## Parallelization Rules
- **Safe to parallelize**: Independent tickets touching different files/modules (`parallel_safe: true`, `overlap_risk: low`)
- **Must serialize**: Tickets with shared dependencies, tickets modifying same files (`overlap_risk: high`)
- **Never parallelize**: Tickets where one's output is another's input

### Parallel safety fields (per-ticket metadata)
When tickets declare parallel metadata, respect these fields:
```yaml
parallel_safe: true       # Can run concurrently with other parallel_safe tickets
overlap_risk: low         # low | medium | high — file/module overlap likelihood
dependencies: []          # List of ticket IDs that must complete first
```

### Process verification
When a ticket's implementation changes a process, workflow, or convention (not just code):
```yaml
pending_process_verification: true   # Set when ticket changes a process
```
A ticket with `pending_process_verification: true` is NOT done until the process change has been exercised at least once in a subsequent ticket execution. The verifying ticket should confirm the new process works and clear the flag.

# Output defaults
Ticket file updated with:
- Status transitions with timestamps
- Investigation notes (if applicable)
- Task breakdown with checkboxes
- Acceptance evidence
- Link to PR/commit

# References
- Shape Up Hill Chart concept: https://basecamp.com/shapeup/3.4-chapter-13
- "Work is like a hill": uphill = figuring out, downhill = executing

# Failure handling
- **Blocked by dependency**: Set `status: blocked`, add `blocked_by: TKT-YYY`, notify or wait
- **Acceptance criteria unclear**: Do not guess. Add clarification request to ticket, set `status: blocked`
- **Tests fail after implementation**: Do not mark done. Debug, fix, re-verify
- **Scope creep during execution**: If new requirements emerge, create new ticket. Do not expand current ticket scope mid-execution
- **Stuck uphill >2 hours**: Flag in ticket, request help or pair. Do not thrash in circles
