---
name: planning
description: Turn an objective, ticket, bug report, or vague request into a bounded execution plan with dependencies, unknowns, validation, and stop conditions before implementation starts. Use when the user asks to "plan this", "break this down", "design an approach", or when a task is large enough that coding immediately would create avoidable rework. Do not use for trivial one-file edits, reviewing an already-written plan (use review-audit-bridge), or executing an existing plan (use ticket-execution).
---

# Purpose
Decomposes a goal or ticket into an executable plan with sized tasks, explicit dependencies, and testable acceptance criteria. Plans answer "what will we build, in what order, and how will we know it's done?" before any code is written.

# When to use this skill
Use when:
- User says "plan this", "break this down", "create a plan", "decompose this ticket", or "how should we approach X"
- Goal exists but path to achieving it is unclear or undocumented
- Complex feature, migration, or refactor needs sequencing before tickets are created
- Team is about to start work and needs shared understanding of scope and order

Do NOT use when:
- Plan already exists and needs validation (use `plan-review`)
- Task is small enough to execute directly (single-file change, <2 hours)
- Objective itself is unclear—clarify the goal first before planning
- User wants risk analysis only (use `premortem`)

# Operating procedure
1. **Restate goal in one sentence**: "This plan delivers [observable outcome] for [who] by [constraint]." If any part is unclear, stop and ask
2. **Define constraints explicitly**:
   - Timeline (hard deadline? flexible?)
   - Tech stack (locked? negotiable?)
   - Team/resources (who's available?)
   - Out of scope (what are we explicitly NOT doing?)
3. **Decompose into tasks**:
   - Each task completable in one sitting by one person/agent
   - Each task has clear completion state (not "work on X")
   - Use verb phrases: "Create schema for...", "Implement endpoint that...", "Write tests verifying..."
4. **Size each task** using relative points or T-shirt sizes:
   - XS: <1 hour, trivial
   - S: 1-2 hours, straightforward
   - M: 2-4 hours, some complexity
   - L: 4-8 hours, significant work
   - XL: >8 hours—must be split further
5. **Map dependencies**:
   - Explicit notation: "T3 depends on T1, T2"
   - Identify parallelizable work
   - Flag external dependencies (API access, approvals, other teams)
6. **Identify critical path**: The sequence of blocking tasks that determines minimum duration. Name tasks on it
7. **Write acceptance criteria**:
   - Goal-level: What proves the whole plan succeeded?
   - Task-level (for M/L tasks): What proves this task is done?
   - All criteria must be observable, testable, no subjective language
8. **Document knowns, unknowns, assumptions**:
   - Knowns: Confirmed facts the plan depends on
   - Unknowns: Things requiring investigation during execution
   - Assumptions: Unverified beliefs—flag risk if wrong
9. **List top 3 risks**: Most likely causes of failure or overrun (one sentence each)

# Output defaults
```
## Plan: [Goal Name]

### Goal
[One sentence: delivers X for Y by Z]

### Constraints
- Timeline: 
- Stack: 
- Out of scope: 

### Tasks
| ID | Task | Size | Depends On | Acceptance Criteria |
|----|------|------|------------|---------------------|
| T1 | [Verb phrase] | S | - | [Observable condition] |
| T2 | [Verb phrase] | M | T1 | [Observable condition] |

### Critical Path
T1 → T3 → T5 (estimated: N days)

### Unknowns & Assumptions
- **Unknown**: [What needs investigation]
- **Assumption**: [What we're assuming] — risk if wrong: [consequence]

### Top 3 Risks
1. [Risk and mitigation]
2. [Risk and mitigation]
3. [Risk and mitigation]

### Goal Acceptance Criteria
- [ ] [Testable condition 1]
- [ ] [Testable condition 2]
```

# References
- https://www.atlassian.com/agile/project-management/estimation — story points and relative sizing
- Planning poker technique for collaborative estimation

# Failure handling
- **Goal too vague to restate**: Return questions that must be answered: "What user/system will benefit? What's the observable change? What's the constraint?"
- **Task too large to size**: Split into subtasks until each is ≤8 hours
- **Circular dependencies detected**: Flag the cycle, propose resolution (usually: identify the true blocker)
- **Unknown dominates plan**: Create a spike/investigation task first, plan the rest after spike completes
