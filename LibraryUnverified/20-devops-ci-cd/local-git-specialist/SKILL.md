---
name: local-git-specialist
description: Applies safe local-only git hygiene for status inspection, diff review, commit crafting, and non-destructive history usage. Trigger on "git commit", "git status", "branch management", "commit message", "diff review". Do NOT use for release-engineering (tagging/publishing), isolation-guidance (worktree strategy), or deployment-pipeline (CI/CD).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: local-git-specialist
  maturity: draft
  risk: low
  tags: [local, git, specialist]
---

# Purpose
Provides safe, precise local git operations for AI agents: what's safe to do autonomously, what requires confirmation, common traps to avoid. Emphasizes non-destructive operations and clear commit hygiene for agent-generated changes.

**Key constraint**: This skill covers local git read/write only. Do not assume GitHub APIs, PR automation, or remote pushes are available. Keep commit scope aligned to the ticket being worked on.

# When to use this skill
Use when:
- Making commits as part of implementation work
- Checking repository state before/after changes
- Working with branches for ticket-based development
- Reviewing history or diffs

Do NOT use when:
- Pushing to remote (different risk profile)
- Complex merge/rebase operations (require human oversight)
- Repository administration (permissions, hooks, etc.)

# Operating procedure

## 1. Safe operations (autonomous OK)

### Status inspection
```bash
git status --short                    # Quick status
git status                            # Full status
git diff --stat                       # Summary of changes
git diff                              # Full diff of unstaged
git diff --cached                     # Diff of staged
git log --oneline -10                 # Recent history
git log --oneline --all -10           # All branches
git branch -v                         # Branch list with last commit
```

### Reading history
```bash
git show <commit>                     # View specific commit
git log --oneline --follow -- <file>  # File history
git blame <file>                      # Line-by-line history
git log --oneline -p -- <file>        # File changes over time
```

### Diffing
```bash
git diff <branch1>..<branch2>         # Branch comparison
git diff HEAD~3                       # Last 3 commits
git diff --name-only <ref>            # Just file names
```

## 2. Safe write operations (autonomous OK)

### Staging changes
```bash
git add <file>                        # Stage specific file
git add -p                            # Interactive staging (careful)
git reset HEAD <file>                 # Unstage file (safe)
```

### Committing
```bash
git commit -m "tkt-XXX: description"  # Commit with message
git commit --amend -m "new message"   # Fix last commit message (local only!)
```

**Commit message format:**
```
tkt-XXX: Brief description (50 chars max)

- Detail 1
- Detail 2

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Branch operations
```bash
git checkout -b tkt-XXX-description   # Create and switch to new branch
git checkout main                     # Switch to main
git branch -d <branch>                # Delete merged branch (safe)
```

## 3. Requires confirmation

### Destructive operations (NEVER autonomous)
```bash
# DANGEROUS - Always require human confirmation:
git reset --hard                      # Loses uncommitted work
git clean -fd                         # Deletes untracked files
git push --force                      # Rewrites remote history
git branch -D <branch>                # Force delete unmerged branch
git rebase                            # Rewrites history
```

### Merge operations (confirm first)
```bash
# Get confirmation before:
git merge <branch>                    # Could have conflicts
git pull                              # Remote changes + merge
```

## 4. Common traps to avoid

### Trap: Editing on wrong branch
**Prevention:**
```bash
# Always verify before committing
git branch --show-current
# Expected: tkt-XXX-description or feature branch
# STOP if on main/master
```

### Trap: Committing sensitive data
**Prevention:**
```bash
# Check staged files before commit
git diff --cached --name-only
# Look for: .env, *.key, *secret*, credentials*
```

### Trap: Large binary files
**Prevention:**
```bash
# Check file sizes before adding
find . -type f -size +1M -not -path "./.git/*"
# Use git-lfs for binaries or exclude them
```

### Trap: Merge commits when not intended
**Prevention:**
```bash
# Use rebase for linear history (if team convention)
git pull --rebase origin main
# Or fetch + merge explicitly
git fetch && git merge --no-ff origin/main
```

### Trap: Lost work from stash
**Prevention:**
```bash
# Don't rely on stash for important work
# If stashing, immediately check:
git stash list
# Apply soon or commit to a WIP branch instead
```

## 5. Agent commit hygiene

### Commit frequency
- Commit after each logical change, not at end of session
- Each commit should pass tests (if applicable)
- Prefer many small commits over few large ones

### Commit message rules
```bash
# Good: Descriptive, references ticket
git commit -m "tkt-042: Add user authentication endpoint

- Implement /api/auth/login route
- Add JWT token generation
- Include rate limiting middleware"

# Bad: Vague or no ticket reference
git commit -m "updates"
git commit -m "fixed stuff"
```

### Before committing checklist
```bash
# 1. On correct branch?
git branch --show-current

# 2. What's being committed?
git diff --cached --stat

# 3. No sensitive files?
git diff --cached --name-only | grep -E '\.(env|key|pem)$'

# 4. Tests pass?
npm test  # or equivalent
```

## 6. Recovery operations

### Undo last commit (keep changes)
```bash
git reset --soft HEAD~1               # Uncommit, keep staged
git reset HEAD~1                      # Uncommit, unstage, keep files
```

### Recover deleted file
```bash
git checkout HEAD -- <file>           # From last commit
git checkout <commit> -- <file>       # From specific commit
```

### Find lost commits
```bash
git reflog                            # Shows all HEAD movements
git checkout <reflog-sha>             # Recover to that point
```

# Output defaults
For any git operation, report:
- What was done
- Current state after operation
- Any warnings or concerns

Example:
```
Committed: tkt-042: Add auth endpoint
Branch: tkt-042-authentication  
Status: Clean working tree, 1 commit ahead of main
```

# References
- Git documentation: https://git-scm.com/docs
- Conventional commits: https://www.conventionalcommits.org

# Failure handling
- **Merge conflict**: Do not auto-resolve. Report conflict files, request guidance
- **Detached HEAD**: Warning! Create branch to save work: `git checkout -b recovery-branch`
- **Uncommitted changes blocking checkout**: Either commit, stash (with confirmation), or abort
- **Push rejected**: Never force push. Fetch, resolve locally, push normally
