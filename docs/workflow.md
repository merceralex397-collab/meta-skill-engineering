# Development Workflow Guide

**For:** AI Agents and Developers  
**Purpose:** Step-by-step workflow for effective work in this codebase  
**Version:** 1.0

---

## Before You Start

### 1. Read AGENTS.md Completely
You MUST read and understand `/home/rowan/Meta-Skill-Engineering/AGENTS.md` before making any changes. Pay special attention to:
- **Critical Behavioral Guardrails** (Sections 1-7)
- **Approval Gates** (when to ask for permission)
- **Verification Protocol** (mandatory pre-flight checklist)

### 2. Verify Current Build Status
Always check the current build status before starting work:

```bash
# In windows-wpf/ directory
dotnet build MetaSkillStudio/MetaSkillStudio.csproj
```

**If build fails:**
- Document current errors
- Propose fix strategy to user
- Get approval before proceeding

**If build passes:**
- Note that baseline is clean
- Proceed with planned work

---

## Standard Workflow

### Phase 1: Investigation (Read-Only Mode)

**Purpose:** Understand scope before making changes

1. **Identify the specific files involved**
   - Use `grep` to find relevant code
   - Read file headers to understand structure
   - Check for existing patterns

2. **Assess the scope**
   - Count files affected
   - Estimate lines of code to change
   - Identify dependencies

3. **Check for similar patterns in codebase**
   - Find examples of how similar problems were solved
   - Look for existing implementations
   - Check for reusable components

**Rule:** Do not make edits during investigation phase.

---

### Phase 2: Planning

**Purpose:** Get user approval for approach

1. **Summarize findings**
   ```
   ## Investigation Results
   - Files affected: [list]
   - Estimated changes: [X] lines across [Y] files
   - Approach: [brief description]
   ```

2. **Propose approach to user**
   - Present 1-3 options if multiple approaches exist
   - Recommend the best option with rationale
   - Wait for explicit approval

3. **Get explicit approval before:**
   - Creating new files (>100 lines)
   - Deploying sub-agents (>1 simultaneously)
   - Architecture changes (refactoring)
   - Major edits (>50 lines)

**Rule:** Never proceed on inferred instructions alone.

---

### Phase 3: Implementation

**Purpose:** Make approved changes with verification

1. **Make changes one at a time**
   - One logical change per edit
   - Verify after each change
   - Document what was changed

2. **Follow the Fix-First Mandate**
   ```
   Priority Order:
   1. Compilation errors (blocking)
   2. Security vulnerabilities (critical)
   3. Test failures (high)
   4. Runtime errors (high)
   5. Code quality issues (medium)
   6. Architecture refactoring (low — only after all above)
   ```

3. **Run verification after EACH significant change:**
    ```bash
    # Unfiltered build check - review ALL output
    dotnet build MetaSkillStudio/MetaSkillStudio.csproj
    
    # If build has many lines, check the end first for summary
    dotnet build 2>&1 | tail -30
    ```

**Rule:** Never use `dotnet build | grep "succeeded"` — this hides errors.

---

### Phase 4: Integration Testing

**Purpose:** Verify all changes work together

1. **Run full build:**
   ```bash
   dotnet build MetaSkillStudio.sln
   ```

2. **Check for:**
   - CS0104 (ambiguous references)
   - CS0246 (type not found)
   - CS1061 (missing property)
   - CS1519 (duplicate code)

3. **Verify integration:**
   - Modified files work with existing code
   - No regressions in unmodified areas
   - LSP errors investigated (not dismissed)

4. **Update documentation:**
    - AGENTS.md if skill structure changed
    - README.md if user-facing changes
    - CONTRIBUTING.md if contribution process changed

**Rule:** Update docs in same commit if scripts/skills changed.

---

### Phase 5: Completion Reporting

**Purpose:** Clear status communication

**Use this format:**
```markdown
## Current State
- Build Status: [PASS/FAIL with error count]
- [If FAIL]: I introduced these errors by [action]
- [If FAIL]: Rollback option: [how to undo]

## Completed (Verified Working)
- [Specific change with file/line evidence]
- [Specific change with file/line evidence]

## In Progress / Blocked
- [What remains with explanation]

## Next Steps
- [Propose next task or ask user]
```

**Never:**
- Use celebratory language when build is broken
- Claim "X% complete" without build verification
- Report progress without citing evidence

---

## Sub-Agent Deployment Protocol

### Default: Sequential Processing

Process tasks one at a time by default.

### When to Use Sub-Agents

**Appropriate for:**
- Independent, parallelizable tasks (e.g., fixing 10 separate SKILL.md files)
- Tasks with no interdependencies
- Tasks that can fail independently

**Not appropriate for:**
- Sequential dependencies (B depends on A)
- Build-critical changes
- First-time verification of new patterns

### Getting Approval for Parallel Deployment

**Format:**
```
I propose deploying [N] sub-agents for [task description]:
- Agent 1: [specific task]
- Agent 2: [specific task]
- ...

Estimated total: [X] lines across [Y] files
Risk: [low/medium/high] because [rationale]

Approve? (yes/no/modify)
```

**Wait for explicit "yes" before deploying >1 agent.**

---

## Task Transition Protocol

When completing a task:

1. **Report completion with verification:**
   ```
   ## Task Complete: [Task Name]
   - Build status: [PASS/FAIL]
   - Changes: [Summary with file/line citations]
   - Verification: [How verified]
   ```

2. **Propose next task:**
   ```
   ## Proposed Next Steps
   Option 1: [Description + estimated effort]
   Option 2: [Description + estimated effort]
   
   What would you like me to work on next?
   ```

3. **Wait for user direction**
   - Do NOT self-direct using "Good! Now I need to..."
   - Do NOT assume next priority
   - Get explicit instruction

---

## Common Workflow Patterns

### Pattern 1: Fixing Build Errors

```
1. Run build → Identify errors
2. Categorize by type (CS0104, CS1061, etc.)
3. Fix one category at a time
4. Verify build after each category
5. Report: "Fixed [N] errors of type [X], build now shows [M] errors"
6. Repeat until 0 errors
```

### Pattern 2: Adding New Files

```
1. Propose: "I need to create [file] (~[N] lines) for [purpose]. Approve?"
2. Wait for "yes"
3. Create file with proper structure
4. Add to project (if needed)
5. Verify build includes new file
6. Report: "Created [file] ([N] lines), build passes"
```

### Pattern 3: Refactoring

```
1. Verify build passes (0 errors)
2. Propose refactoring with scope estimate
3. Wait for explicit approval
4. Make changes in small increments
5. Verify build after each increment
6. If build breaks → rollback first, then reassess
```

---

## Emergency Procedures

### When Build Breaks

1. **Stop immediately** — Do not add more changes
2. **Document current state:**
   ```
   ## Build Broken
   - Errors introduced: [list]
   - Last known good: [commit/sha or state]
   - Cause: [what action broke it]
   ```

3. **Choose approach:**
   - **Option A:** Fix-forward (if fix is trivial and obvious)
   - **Option B:** Rollback to last known good (if fix is complex)

4. **Get user approval** for chosen approach

5. **Execute and verify**

### When Uncertain

**Always ask:**
- "I'm uncertain about [specific thing]. Should I:"
  - A) [Option A]
  - B) [Option B]
  - C) Something else?

**Never:**
- Guess when stakes are high
- Proceed with "probably fine"
- Hide uncertainty in progress reports

---

## Verification Checklist (Pre-Flight)

Before claiming any work complete:

- [ ] Build passes with 0 errors (unfiltered check)
- [ ] No CS0104 ambiguous references in modified files
- [ ] No CS0246 missing types
- [ ] No CS1061 missing properties  
- [ ] No CS1519 duplicate code
- [ ] LSP errors investigated (none dismissed without cause)
- [ ] Integration verified (changes work together)
- [ ] Documentation updated (AGENTS.md, README.md if needed)
- [ ] Evidence cited (file paths, line numbers)

**Shortcut prohibited:** `dotnet build | grep succeeded`

---

## Reference

- **AGENTS.md:** `../AGENTS.md` - Behavioral guardrails and verification protocols
- **CONTRIBUTING.md:** `../CONTRIBUTING.md` - How to contribute to this repository
- **Architecture:** `docs/architecture.md` - System architecture and patterns
- **Code Style:** `docs/code-style.md` - Coding conventions
- **Testing:** `docs/testing-guide.md` - Testing requirements and patterns
- **Security:** `docs/security-guidelines.md` - Security patterns and prevention
- **Troubleshooting:** `docs/troubleshooting.md` - Common issues and solutions
- **Evaluation Cadence:** `docs/evaluation-cadence.md` - When to run which tests
