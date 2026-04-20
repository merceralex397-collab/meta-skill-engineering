---
name: isolation-guidance
description: Chooses when work should use in-place edits versus worktrees, temp copies, or isolated lanes — preferring the lightest mechanism that prevents cross-ticket contamination. Trigger on "branch isolation", "worktree", "parallel work", "cross-ticket contamination". Do NOT use for local-git-specialist (git operations) or deployment-pipeline (environment separation).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: isolation-guidance
  maturity: draft
  risk: low
  tags: [isolation, guidance]
---

# Purpose
Keeps agent work properly isolated to prevent cross-contamination: branch discipline (one ticket = one branch), no cross-ticket file edits, environment separation for risky operations. Prevents the failure mode where agent work on Ticket A accidentally affects Ticket B.

**Key principle**: Do not invent an isolation setup the repo has not enabled. If the repo uses simple branches, use branches. If worktrees are not part of the team workflow, do not introduce them without explicit approval. Always prefer the lightest isolation mechanism that prevents contamination.

# When to use this skill
Use when:
- Starting work on a new ticket
- Multiple tickets might touch similar files
- Running operations that could have side effects
- Parallel work streams need separation

Do NOT use when:
- Single simple change with no risk of contamination
- Already in an isolated environment
- Quick read-only investigation

# Operating procedure

## 1. Branch isolation (default for all ticket work)

### Rule: One ticket = One branch
```bash
# Starting work on TKT-042
git checkout main
git pull origin main
git checkout -b tkt-042-add-auth

# All work for TKT-042 happens here
# Commit frequently to this branch
# Never switch branches with uncommitted work
```

### Rule: No cross-ticket edits
If working on TKT-042 and you discover TKT-043 needs a fix:
```bash
# WRONG: Fix it on TKT-042 branch
# This contaminates TKT-042 with unrelated changes

# RIGHT: Stash, switch, fix, return
git stash
git checkout main && git checkout -b tkt-043-fix
# make fix, commit
git checkout tkt-042-add-auth
git stash pop
```

Or better: Create a ticket for the fix and leave a note.

## 2. Worktree isolation (for parallel work)

### When to use worktrees
- Need to work on two tickets simultaneously
- Long-running task (build, test) shouldn't block other work
- Comparison between branches needs both checked out

### Setup worktrees
```bash
# Create worktree for second ticket
git worktree add ../project-tkt-043 -b tkt-043-feature

# Now you have:
# /project/           <- main working directory, tkt-042 branch
# /project-tkt-043/   <- separate directory, tkt-043 branch

# Work in each independently
cd ../project-tkt-043
# ... work on TKT-043 ...

# Cleanup when done
git worktree remove ../project-tkt-043
```

## 3. File-level isolation

### Rule: Tickets should have minimal file overlap
When planning tickets:
- Identify files each ticket touches
- If overlap >30%, consider serializing tickets
- If overlap unavoidable, one ticket must complete first

### Detecting overlap
```bash
# List files touched by current branch
git diff --name-only main...HEAD

# Compare with another branch
git diff --name-only main...tkt-043

# Find overlap
comm -12 <(git diff --name-only main...tkt-042 | sort) \
         <(git diff --name-only main...tkt-043 | sort)
```

### Handling overlap
If files overlap:
1. **Preferred**: Serialize—complete one ticket before starting other
2. **If parallel**: Coordinate via clear sections (e.g., "TKT-042 owns lines 1-50, TKT-043 owns 51-100")
3. **If unavoidable**: One ticket creates, other ticket extends (define interface first)

## 4. Environment isolation

### When to create isolated environments
- Installing new dependencies
- Running database migrations
- Testing destructive operations
- Building with different configurations

### Virtual environment isolation
```bash
# Python
python -m venv .venv-tkt-042
source .venv-tkt-042/bin/activate

# Node.js (using different node_modules)
mkdir -p .isolated/tkt-042
cd .isolated/tkt-042
npm install  # Separate node_modules
```

### Container isolation (for risky operations)
```bash
# Run in container to prevent host contamination
docker run --rm -v $(pwd):/work -w /work node:20 npm test
```

## 5. Database isolation

### Rule: Never modify shared databases during development
```bash
# Use local/test database
export DATABASE_URL="sqlite://./test.db"
# Or
export DATABASE_URL="postgres://localhost/project_dev"
```

### Migration isolation
```bash
# Test migrations on copy first
cp production.db test-migration.db
DATABASE_URL="sqlite://./test-migration.db" npm run migrate

# Only if successful, apply to dev
```

## 6. Isolation checklist

Before starting ticket work:
- [ ] On dedicated branch for this ticket?
- [ ] No uncommitted changes from other work?
- [ ] Know which files this ticket will touch?
- [ ] No overlap with in-progress tickets?
- [ ] Using appropriate database (dev/test, not prod)?

Before committing:
- [ ] All changes relate to this ticket?
- [ ] No accidental changes to unrelated files?
- [ ] Environment variables not committed?

Before merging:
- [ ] Branch is up to date with main?
- [ ] No conflicts introduced?
- [ ] Tests pass in isolation?

## 7. Recovery from contamination

### Accidentally edited file belonging to another ticket
```bash
# Discard specific file changes
git checkout HEAD -- <file>

# If already committed, revert that file
git show HEAD:<file> > <file>
git add <file>
git commit --amend
```

### Mixed commits from multiple tickets
```bash
# Interactive rebase to separate (advanced)
git rebase -i main
# Mark commits to edit, split them by ticket
```

### Wrong branch entirely
```bash
# Cherry-pick commits to correct branch
git log --oneline  # Find commits to move
git checkout correct-branch
git cherry-pick <commit-sha>
# Then remove from wrong branch
```

# Output defaults
Report isolation status when starting work:
```markdown
## Isolation Status
- Branch: tkt-042-add-auth
- Base: main (up to date)
- Files to touch: src/auth/, tests/auth/
- Overlap with active work: None
- Environment: Dev database, local node_modules
```

# References
- Git worktrees: https://git-scm.com/docs/git-worktree
- Branch-per-feature workflow standard practice

# Failure handling
- **Uncommitted changes blocking switch**: Stash or commit before switching
- **Worktree already exists**: Remove or use existing
- **Contaminated commit discovered**: Use `git reset` or interactive rebase to clean
- **Merge conflict from parallel work**: Stop, coordinate with other work stream, resolve together
