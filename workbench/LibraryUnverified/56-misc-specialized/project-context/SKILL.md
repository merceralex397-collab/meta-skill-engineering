---
name: project-context
description: Load a repository's source-of-truth documents in a deterministic order before planning, editing, or reviewing. Use when starting work in an unfamiliar repo, resuming a session, or when the user asks for a repo-aware plan, fix, or review and the authoritative docs may be spread across README, AGENTS, specs, tickets, and config files. Do not use when the task is already narrowly scoped and the relevant files are known.
---

# Purpose
Loads canonical project context in a deterministic order so an agent starts every session correctly oriented. Prevents the "blank slate" problem where agents make decisions without understanding project constraints, existing patterns, or current work state. This is the first skill invoked when starting work on any established project.

# When to use this skill
Use when:
- Starting a new session on an existing project
- Context-switching from one project to another
- Beginning work on a ticket or task
- Uncertain about project conventions or current state

Do NOT use when:
- The project has just been scaffolded (no context yet)
- Already loaded context in this session
- Working on a single-file utility with no project structure

# Operating procedure

## 1. Load identity documents (MUST read)
```bash
cat README.md 2>/dev/null | head -100
cat AGENTS.md 2>/dev/null
```
If AGENTS.md exists, follow its reading order exactly.

## 2. Load project brief (MUST read)
```bash
cat docs/BRIEF.md 2>/dev/null
cat docs/spec/CANONICAL-BRIEF.md 2>/dev/null
cat docs/DECISIONS.md 2>/dev/null | head -50  # Recent decisions
cat docs/CONSTRAINTS.md 2>/dev/null
cat docs/process/workflow.md 2>/dev/null
```

## 3. Load stack context (MUST read)
```bash
# Identify stack from manifest
cat package.json 2>/dev/null | head -30
cat Cargo.toml 2>/dev/null | head -30
cat pyproject.toml 2>/dev/null | head -30
cat go.mod 2>/dev/null

# Load stack-specific conventions
cat docs/STACK-PROFILE.md 2>/dev/null
```

## 4. Load current work state (SHOULD read)
```bash
# What's in progress?
cat tickets/BOARD.md 2>/dev/null
# Or check for GitHub issues
gh issue list --state open 2>/dev/null | head -10

# Recent activity
git log --oneline -10 2>/dev/null

# Any uncommitted work?
git status --short 2>/dev/null
```

## 5. Load architecture (IF deep work needed)
```bash
cat docs/ARCHITECTURE.md 2>/dev/null
tree -L 2 -I 'node_modules|dist|target|__pycache__' 2>/dev/null
```

## 6. Build context summary
After loading, synthesize into working memory:
```markdown
## Project Context Summary

**Project:** [name from README]
**Stack:** [from package.json/Cargo.toml/etc.]
**Current state:** [from BOARD.md/git status]

**Key constraints:**
- [from CONSTRAINTS.md]

**Recent decisions:**
- [from DECISIONS.md]

**Active work:**
- [from BOARD.md in_progress column]

**Ready for:**
[What task to pick up next based on context]
```

## 7. Validate context is sufficient
Before proceeding with work, confirm:
- [ ] Know what the project does
- [ ] Know the tech stack
- [ ] Know current work state
- [ ] Know any blocking constraints

If any are unclear, investigate before making changes.

## Reading order priority
1. **Always read**: README.md, AGENTS.md (if exists)
2. **Always read if exists**: docs/BRIEF.md, docs/spec/CANONICAL-BRIEF.md, docs/CONSTRAINTS.md, docs/process/workflow.md
3. **Read before coding**: docs/STACK-PROFILE.md, package.json equivalent
4. **Read before picking tickets**: tickets/BOARD.md, tickets/manifest.json
5. **Read for deep work**: docs/ARCHITECTURE.md, relevant source directories

## Context caching
For long sessions, note context load timestamp:
```
Context loaded: [ISO timestamp]
Last commit when loaded: [short sha]
```
Reload if >1 hour old or if `git log` shows new commits.

# Output defaults
Return a context summary that enables immediate productive work:
```markdown
## Context Loaded

**Project:** [name]
**Purpose:** [one-line from BRIEF]
**Stack:** [language/framework]
**Health:** [GREEN if no blockers, YELLOW if warnings, RED if issues]

**Current Work:**
| Status | Count | Next Up |
|--------|-------|---------|
| in_progress | 1 | TKT-003 |
| ready | 3 | TKT-004 |

**Constraints to Remember:**
- [key constraint 1]
- [key constraint 2]

**Ready to work on:** [recommended next action]
```

# References
- Follows AGENTS.md reading order if present
- Context prevents drift by grounding decisions in project reality

# Failure handling
- **No README found**: Warn "No README.md—project identity unclear. Create one before proceeding with major work."
- **No BRIEF found**: Acceptable for simple projects. Note "No formal brief—inferring purpose from README."
- **Conflicting context files**: Flag specific conflict, ask for resolution before proceeding
- **Stale context**: If BOARD.md older than recent commits, flag "Work state may be outdated. Run workflow-observability to reconcile."
