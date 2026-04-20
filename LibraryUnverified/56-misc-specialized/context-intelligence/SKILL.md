---
name: context-intelligence
description: Manages context window budgets, loading strategies, and compaction techniques for AI-assisted coding sessions. Trigger on 'context window', 'what to load', 'context management', 'context overflow', 'token budget'. DO NOT USE for loading specific project docs into agent context (use project-context) or prompt wording and optimization (use prompt-crafting).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: context-intelligence
  maturity: draft
  risk: low
  tags: [context, intelligence]
---

# Purpose
Manages context window usage intelligently: what to load into context, what to summarize, what to defer. Prevents context rot—where too much information degrades model performance—by curating what's in context rather than just cramming everything in. Implements strategies from Anthropic's context engineering guidance.

# When to use this skill
Use when:
- Session approaching context window limits
- Need to decide what information to include in a prompt
- Working with large codebases where everything won't fit
- Optimizing for accuracy in long-context tasks

Do NOT use when:
- Simple tasks with minimal context needs
- Context window usage is clearly under 50%
- The task explicitly needs full context (e.g., full code review)

# Operating procedure

## 1. Understand context economics
Claude models have context windows up to 200K-1M tokens, but:
- More context ≠ better results
- "Context rot" degrades accuracy as token count grows
- Quality of context matters more than quantity

**Budget allocation:**
| Content Type | Typical Allocation |
|--------------|-------------------|
| System prompt | 1-5K tokens |
| Core instructions | 2-5K tokens |
| Active file(s) | 5-20K tokens |
| Supporting context | 10-50K tokens |
| Conversation history | Remainder |

## 2. Prioritize context by relevance

### Tier 1: Must include (always load)
- Current file being edited
- Direct dependencies of current file
- Active ticket/task description
- Relevant error messages

### Tier 2: Should include (if space)
- Test files for current code
- Type definitions used by current code
- Recent related commits
- Relevant documentation sections

### Tier 3: Can defer (load on demand)
- Tangentially related files
- Historical context (old versions)
- Full project documentation
- Examples from other projects

## 3. Context loading strategies

### Strategy: Focused loading
For implementation tasks:
```bash
# Load only what's directly needed
cat src/auth/login.ts           # File being modified
cat src/auth/types.ts           # Types it uses
cat src/auth/login.test.ts      # Its tests
cat tickets/TKT-042.md          # The ticket
# Total: ~4 files, focused context
```

### Strategy: Progressive loading
Start minimal, expand as needed:
```
1. Load task description
2. Load primary file
3. If blocked, load dependencies
4. If still blocked, load examples
5. If still stuck, load full module
```

### Strategy: Summarized loading
For large files/sections:
```markdown
## Summary of src/database/
- models/: User, Post, Comment entities
- queries/: CRUD operations for each model
- migrations/: 12 migrations, latest adds 'preferences' table
[Full files available on request]
```

## 4. Context placement matters

Following Anthropic's guidance—put long documents at TOP:
```markdown
[LONG DOCUMENTS AND DATA - TOP]

---

[Your query and instructions - BOTTOM]
```

This can improve response quality by up to 30% for complex tasks.

## 5. Context window tracking

Monitor usage during sessions:
```markdown
## Context Budget
Window: 200K tokens
Used: ~45K tokens (22%)
- System prompt: 3K
- Project context: 12K
- Current files: 15K
- Conversation: 15K

Remaining: ~155K tokens
Status: GREEN (under 50%)
```

**Thresholds:**
- GREEN: <50% — operate normally
- YELLOW: 50-75% — start deferring, consider summarizing
- RED: >75% — actively manage, use compaction

## 6. Context compaction techniques

### Technique: Conversation pruning
Remove old turns that aren't relevant:
```
Keep: Last 5 turns, any turns with key decisions
Remove: Investigation turns that led nowhere, verbose tool outputs
```

### Technique: File summarization
Replace large files with summaries:
```markdown
## Summary: src/database/queries.ts (was 2000 lines)
- exports 45 functions
- key patterns: findById, findMany, create, update, delete
- uses Prisma client
- relevant functions for current task: getUserById, updateUser
```

### Technique: Selective history
Keep only turns with:
- Key decisions made
- Errors and their resolutions
- Requirements clarifications

## 7. Grounding responses in quotes

For long document tasks, extract relevant quotes first:
```markdown
Before answering, find relevant sections:

From src/auth/README.md:
> "JWT tokens expire after 24 hours and must be refreshed..."

From docs/API.md:
> "The /refresh endpoint requires the expired token in the Authorization header..."

Now, based on these quotes: [answer]
```

This reduces hallucination and improves accuracy.

## 8. Multi-window workflows

When approaching limits, prepare for context transition:
```markdown
## Context Transition Checkpoint

### Persisted artifacts:
- tickets/TKT-042.md updated with progress
- src/auth/login.ts committed
- START-HERE.md created with handoff

### Next window should load:
1. START-HERE.md (fastest reorientation)
2. tickets/TKT-042.md (remaining work)
3. src/auth/login.ts (current file)
```

# Output defaults
Report context status when relevant:
```markdown
## Context Status
Usage: 45K / 200K (22%)
Strategy: Focused loading
Loaded: 4 files (auth module)
Deferred: test fixtures, migration history
```

# References
- Anthropic context windows: https://docs.anthropic.com/en/docs/build-with-claude/context-windows
- Context rot: accuracy degrades with excess context
- Long documents at top improves retrieval

# Failure handling
- **Context overflow**: Summarize oldest/least relevant content, create checkpoint
- **Key info missing**: Request specific file rather than loading everything
- **Quality degrading**: Reduce context, focus on essentials, possibly start fresh window
- **Can't find relevant info**: Use grep/search to locate before loading entire directories
