---
name: gh-address-comments
description: "Help address review/issue comments on the open GitHub PR for the current branch using gh CLI; verify gh auth first and prompt the user to authenticate if not logged in."
---

# Purpose
A GitHub-focused skill for methodically addressing PR review comments with evidence-based responses.

# When to use this skill
Use when:
- Addressing reviewer comments on a GitHub pull request
- Systematically resolving PR review feedback
- Tracking which review comments have been addressed

Do NOT use when:
- Initial PR creation (different workflow)
- Non-GitHub code review workflows

# Operating procedure
1. List all open review comments
2. Categorize by type (code change, question, nitpick)
3. Address each comment with code change or explanation
4. Mark resolved comments with reply linking to commit
5. Re-request review when all comments addressed

# Output defaults
Code changes, comment responses, and resolved comment tracking

# Failure handling
If a comment is unclear, ask for clarification before acting. Don't guess reviewer intent.
