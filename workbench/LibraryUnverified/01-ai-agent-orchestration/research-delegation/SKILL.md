---
name: research-delegation
description: Delegate read-only evidence gathering to scoped sub-agents with explicit output contracts, then synthesize findings into persistent artifacts. Use when the user says "research task", "investigate codebase", "gather evidence", "read-only exploration", "parallel research", or when implementation needs an evidence base before coding starts. Do not use for planning (task decomposition), review-audit-bridge (code review findings), or project-context (loading project docs).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: research-delegation
  maturity: draft
  risk: low
  tags: [research, delegation]
---

# Purpose
Delegates research tasks to sub-agents with precise scope definitions, explicit output contracts, and synthesis instructions. Keeps information gathering cleanly separated from code mutation—research agents read and report, implementation agents act on findings. Prevents the common failure mode of "research" that actually makes changes.

# When to use this skill
Use when:
- Need to understand a codebase area before making changes
- Gathering evidence from multiple sources (files, docs, web)
- Preparing a decision with supporting analysis
- The research could be parallelized across multiple sub-agents

Do NOT use when:
- The question can be answered by reading one file (just read it)
- You need to make changes during investigation (that's implementation)
- The "research" is actually prototyping or experimentation

# Operating procedure

## 1. Define research scope precisely
A good research delegation includes:
```markdown
## Research Task: [Descriptive name]

### Question
[Single, specific question to answer]

### Scope
- Files/directories to examine: [explicit list or pattern]
- External sources (if any): [URLs, APIs]
- Time/token budget: [limit]

### Output Contract
Return findings in this format:
- Answer: [direct answer to the question]
- Evidence: [file:line citations]
- Confidence: [high/medium/low with rationale]
- Gaps: [what couldn't be determined]

### Constraints
- READ ONLY: Do not modify any files
- Do not execute code that has side effects
- Do not make network requests unless explicitly listed
```

## 2. Parallelize independent research
If multiple questions are independent, delegate in parallel:
```
Research Task A: How does authentication work?
  Scope: src/auth/
  
Research Task B: What database schema exists?
  Scope: src/models/, migrations/
  
Research Task C: What are the API endpoints?
  Scope: src/routes/, docs/api.md
```

## 3. Enforce read-only constraint
Research agents must not:
- Create files (except findings artifact)
- Modify existing files
- Run commands that mutate state
- Make commits

**Do not use research delegation as a loophole for write-capable background implementation.** If the research task requires creating, modifying, or building anything, it is not research — delegate to an implementation agent instead.

Allowed actions:
- `cat`, `grep`, `find`, `tree`
- `git log`, `git show`, `git blame`
- Read-only API calls
- Web fetches for documentation

## 4. Require evidence-backed findings
Every claim must have a citation:
```markdown
## Finding: User authentication uses JWT

**Evidence:**
- `src/auth/jwt.ts:15` - JwtStrategy class definition
- `src/auth/guards.ts:8` - @UseGuards(JwtAuthGuard) decorator
- `package.json:12` - "@nestjs/jwt": "^10.0.0" dependency

**Confidence:** High - multiple corroborating sources
```

## 5. Synthesize delegated research
After research tasks complete, synthesize:
```markdown
## Research Synthesis: [Topic]

### Summary
[Combined findings in 2-3 sentences]

### Key Findings
1. [Finding from Task A]
2. [Finding from Task B]
3. [Finding from Task C]

### Conflicts/Uncertainties
- [Any contradictory findings]
- [Gaps that remain]

### Implications for Implementation
- [How findings affect next steps]
```

## 6. Convert unresolved outcomes into tickets
Any research question that cannot be resolved with available evidence must not be silently dropped. Convert unresolved research outcomes into explicit blockers or follow-up tickets:
```markdown
## Unresolved → Ticket
- Question: [What couldn't be answered]
- Blocker for: [Which implementation task depends on this]
- Suggested next step: [Spike, external consultation, or deeper investigation]
```

## 7. Persist findings as artifacts
Save research results for future reference:
```bash
mkdir -p docs/research
cat > docs/research/[topic]-[date].md << 'EOF'
# Research: [Topic]
Date: [ISO date]
Researcher: [agent-id]

[Synthesis content]
EOF
```

# Output defaults
```markdown
# Research Findings: [Topic]

## Question
[Original question]

## Answer
[Direct answer, 1-3 sentences]

## Evidence
| Claim | Source | Confidence |
|-------|--------|------------|
| [claim] | [file:line] | high/medium/low |

## Gaps
- [What couldn't be determined]

## Raw Findings
[Detailed findings from each research task]
```

# References
- Read-only delegation prevents accidental mutations during investigation
- Evidence-based findings enable verification and reduce hallucination

# Failure handling
- **Scope too broad**: If research would require reading >50 files, narrow scope or break into sub-tasks
- **No evidence found**: Report "No evidence found in specified scope" rather than guessing
- **Conflicting evidence**: Report both sources and the conflict, don't resolve silently
- **Research agent made changes**: Revert immediately, flag as constraint violation, retry with stricter enforcement
