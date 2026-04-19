---
name: skill-authoring
description: >-
  Create a new SKILL.md with YAML frontmatter, routing description, and structured
  body sections from scratch. Use this for "create a skill for X", "write a skill
  that handles Y", "I need a new skill to do Z", or when a repeated task pattern
  should become a reusable agent procedure. Do not use for improving existing skills
  (skill-improver), fixing triggers only (skill-trigger-optimization),
  adapting to a different context (skill-adaptation), or installing packaged skills
  (skill-installer).
license: Apache-2.0
compatibility:
  clients: [opencode, copilot, codex, gemini-cli, claude-code]
---

# Purpose

Create new agent skills in the SKILL.md format: YAML frontmatter for routing metadata plus a markdown body with Purpose, When to use, Operating procedure, Output defaults, and Failure handling. The description field is routing logic — it determines whether the skill fires.

# When to use this skill

Use when:

- User says "create a skill for X", "write a skill that...", "I need a skill to handle..."
- Repeated task pattern needs capturing as a reusable procedure
- Capability should be packaged for distribution or reuse across projects
- New agent specialization needed for a repo's agent team

Do NOT use when:

- Skill exists and needs improvement → `skill-improver`
- Only description/trigger needs fixing → `skill-trigger-optimization`
- Skill needs adaptation to different context → `skill-adaptation`
- User wants to install a packaged skill → `skill-installer`
- User wants a one-off prompt, not a reusable skill

# Operating procedure

## Step 1 — Define the skill's job in one sentence

Write: "This skill [verb] when [trigger] and produces [output]."

If you cannot write this sentence, the scope is wrong — narrow until it works.

Calibrate specificity to task fragility:
- **High freedom** (prose): Multiple valid approaches, context-dependent decisions
- **Medium freedom** (pseudocode): Preferred pattern with acceptable variation
- **Low freedom** (exact scripts): Fragile operations where consistency is critical

## Step 2 — Choose the name

- Lowercase, hyphens, 2–4 words, under 64 characters
- Describe what it does (verb-noun), not when it's used
- Examples: `premortem`, `gap-analysis`, `acceptance-criteria-hardening`

## Step 3 — Write the YAML frontmatter

```yaml
---
name: skill-name
description: >-
  [Action verb] [specific object] when [task conditions].
  Use this for [2-3 realistic trigger phrases in quotes].
  Do not use for [adjacent non-matching cases with named alternatives].
license: Apache-2.0
compatibility:
  clients: [opencode, copilot, codex, gemini-cli, claude-code]
---
```

### Description field rules

The description is routing logic — the highest-leverage field in the skill.

Requirements:
1. Start with an action verb (not a noun phrase)
2. Include 2–3 realistic trigger phrases users would say
3. State what the skill produces
4. End with "Do not use for..." naming adjacent skills

**Weak**: "Helps with PDFs."
**Strong**: "Extract structured data from PDFs when the task involves tables, scanned pages, or layout-dependent interpretation. Use this for 'extract the table from this PDF', 'convert this PDF to CSV', or 'parse the scanned invoice'. Do not use for plain-text summarization when PDF text is already clean (use text-summarization)."

Flag a description if it: is under 12 words, has no action verb first, has no condition, lacks trigger examples, could apply to multiple skills, or has no negative boundary.

### Frontmatter fields

| Field | Required | Notes |
|-------|----------|-------|
| `name` | Yes | Must match folder name |
| `description` | Yes | Routing logic, not documentation |
| `license` | Recommended | Default: Apache-2.0 |
| `compatibility.clients` | Recommended | Which agent clients support this skill |

## Common authoring mistakes

Watch for these — they are the most frequent causes of weak skills from new authors:

1. **Writing a tutorial instead of a procedure.** Symptoms: "Let me explain…", background history sections, technology overviews. A skill is an operating manual, not a textbook. Cut everything the agent doesn't need mid-task.
2. **Describing goals instead of steps.** Symptoms: "Ensure quality", "Optimize performance" with no HOW. Every step must be a concrete verb the agent can execute: Read, Write, List, Check, Run.
3. **Copying reference material inline.** Symptom: SKILL.md >200 lines filled with lookup tables, API schemas, or configuration catalogs. Extract to `references/` and link from the procedure.
4. **Writing the description last.** The description should be written FIRST — it defines the scope everything else must serve. Writing it last leads to scope drift where the body outgrows the description.
5. **Forgetting negative boundaries.** Every skill must say what it's NOT for with named alternatives. Without this, hosts cannot distinguish adjacent skills and routing degrades.

## Step 4 — Write the body sections

Every SKILL.md body contains these sections in order:

### Purpose (required)
2–3 sentences. What problem does it solve? What output does it produce? No filler.

### When to use this skill (required)
- "Use when:" — 4–6 specific trigger phrases or observable conditions
- "Do NOT use when:" — 3–4 confusion cases with named alternatives

### Operating procedure (required)
Numbered steps. Each starts with a verb. Each is completable and verifiable.

Rules:
- No meta-commentary ("Keep scope explicit", "Think about the approach")
- No hedge verbs ("Consider", "You might want to", "Ensure")
- Use action verbs: Read, List, Write, Check, Run, Compare
- Each step must be independently executable

### Output defaults (required)
Exact format with template showing section names, field names, or schemas.

**Bad**: "Structured markdown with clear next steps"
**Good**: Template with named sections, tables, or code blocks

### Failure handling (required)
Name 2–3 most common failure modes with specific recovery actions.

**Bad**: "If something goes wrong, report the issue"
**Good**: "If target file does not exist: stop, report missing path, ask user to confirm location"

### References (optional)
Real URLs to authoritative documentation. Not vague "see docs".

### Size target
Keep SKILL.md under 500 lines. Over that, extract reference material into `references/` and link from SKILL.md. Challenge each paragraph: "Does this justify its token cost?"

Skills load at three levels:
1. **Metadata** (name + description) — always in context (~100 words)
2. **SKILL.md body** — loaded when skill triggers (target: under 5k words)
3. **Bundled resources** — loaded on demand by the agent

## Step 5 — Validate against anti-patterns

Check the completed skill for:
- Circular triggers ("when task involves X" without defining X)
- Boilerplate procedure steps with no specific action
- Generic output defaults lacking concrete format
- Vague failure handling without recovery actions
- Description that could apply to multiple different skills

## Step 6 — Create the skill folder

```
skill-name/
├── SKILL.md          # Frontmatter + body sections
├── scripts/          # Optional: deterministic automation
├── references/       # Optional: large docs for progressive disclosure
└── templates/        # Optional: reusable output skeletons
```

Only create subdirectories if they contain actual files. Do not add README.md, CHANGELOG.md, or other meta-documentation — the folder contains only what an agent needs to do the job.

# Output defaults

Deliver a complete skill folder containing:

1. **SKILL.md** with valid YAML frontmatter and all required body sections
2. **references/** if the skill needs progressive disclosure for large reference material
3. **scripts/** if the skill includes deterministic automation
4. **templates/** if the skill provides reusable output skeletons

The SKILL.md must pass all Step 5 anti-pattern checks before delivery.

# References

- https://docs.github.com/en/copilot/concepts/agents/about-agent-skills
- https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-skills
- https://docs.anthropic.com/en/docs/claude-code/overview
- https://geminicli.com/docs/cli/skills/

# Failure handling

- **Scope too broad**: Skill handles multiple distinct tasks → split via `skill-variant-splitting`
- **Cannot write one-sentence definition**: Scope is wrong → narrow until the sentence works
- **Overlaps existing skill**: Check catalog → merge or differentiate explicitly in descriptions
- **No clear trigger phrases**: Ask user what words they'd use when they need this capability
- **Description lint-passes but never fires**: Add more trigger phrase variations → test with `skill-testing-harness`
