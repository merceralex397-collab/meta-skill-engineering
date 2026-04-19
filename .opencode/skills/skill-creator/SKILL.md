---
name: skill-creator
description: >-
  Create new agent skills from scratch and iteratively improve them through
  test-review-improve cycles. Use this for "create a skill for X", "write a
  skill that handles Y", "I need a new skill to do Z", "turn this workflow
  into a skill", or when a repeated task pattern should become a reusable
  agent procedure. Do not use for splitting a broad skill into variants
  (skill-variant-splitting), adapting a skill to a different environment
  (skill-adaptation), improving an existing skill without full iteration
  (skill-improver), or running standalone evaluations without creation
  intent (skill-evaluation).
---

# Purpose

Create new agent skills and iteratively improve them through structured
draft-test-review-improve cycles.

# When to use

- User says "create a skill for X", "write a skill that…", "I need a skill to handle…"
- Repeated task pattern needs capturing as a reusable procedure
- Existing conversation contains a workflow the user wants to turn into a skill
- User has a draft skill and wants to iterate on it with test feedback
- Capability should be packaged for reuse across projects

# When NOT to use

- Skill exists and needs improvement without full creation iteration → `skill-improver`
- Only the description/trigger needs fixing → `skill-trigger-optimization`
- Skill needs porting to a different environment → `skill-adaptation`
- User wants to split one broad skill into several → `skill-variant-splitting`
- User wants a standalone evaluation without creation → `skill-evaluation`

# Procedure

## Phase 1 — Capture intent

Start by understanding what the user wants the skill to do. The current
conversation may already contain a workflow the user wants to capture. If so,
extract answers from the conversation history first — tools used, step
sequence, corrections made, input/output formats observed.

Gather answers to:

1. What should this skill enable the agent to do?
2. When should this skill trigger? (specific user phrases and contexts)
3. What is the expected output format?
4. Are there edge cases, dependencies, or environmental requirements?

Ask questions about edge cases, input/output formats, example files, success
criteria, and dependencies. Research available documentation and similar
skills if useful context exists.

## Phase 2 — Write the SKILL.md

### Step 1 — Define the skill's job in one sentence

Write: "This skill [verb] when [trigger] and produces [output]."

If you cannot write this sentence, the scope is wrong — narrow until it works.

### Step 2 — Choose the name

- Lowercase, hyphens, 2–4 words, under 64 characters
- Describe what it does (verb-noun), not when it's used
- Must match the parent directory name

### Step 3 — Write the YAML frontmatter

```yaml
---
name: skill-name
description: >-
  [Action verb] [specific object] when [task conditions].
  Use this for [2-3 realistic trigger phrases in quotes].
  Do not use for [adjacent non-matching cases with named alternatives].
---
```

**Description rules — this is the highest-leverage field in the skill:**

1. Start with an action verb (not a noun phrase)
2. Include 2–3 realistic trigger phrases users would actually say
3. State what the skill produces
4. End with "Do not use for…" naming adjacent skills
5. Make it slightly assertive about when to trigger — agents tend to
   under-trigger rather than over-trigger
6. Include keywords and contexts that should activate the skill
7. Keep the description under 1024 characters (hard limit per the Agent Skills specification)

Flag a description if it: is under 12 words, has no action verb first,
has no condition, lacks trigger examples, could apply to multiple skills,
has no negative boundary, or exceeds 1024 characters.

### Step 4 — Write the body sections

Every SKILL.md body contains these sections in order:

**Purpose** (required) — 2–3 sentences. What problem does it solve?
What output does it produce?

**When to use** (required) — 4–6 specific trigger phrases or observable
conditions as "Use when:" plus 3–4 confusion cases as "When NOT to use:"
with named alternatives.

**Procedure** (required) — Numbered steps. Each starts with a verb. Each
is completable and verifiable. No meta-commentary, no hedge verbs.
Use action verbs: Read, List, Write, Check, Run, Compare.

**Output contract** (required) — Exact format with template showing section
names, field names, or schemas. Include a concrete filled example where
possible — agents produce more consistent output when shown examples
rather than only format descriptions.

**Failure handling** (required) — Name the 2–3 most common failure modes
with specific recovery actions. Not "if something goes wrong, report the
issue" but "if target file does not exist: stop, report missing path, ask
user to confirm location."

**References** (optional) — Real URLs to authoritative documentation.

### Step 5 — Calibrate instruction depth

Match instruction specificity to task fragility:

- **High freedom** (prose): Multiple valid approaches, context-dependent
- **Medium freedom** (pseudocode): Preferred pattern with acceptable variation
- **Low freedom** (exact steps/scripts): Fragile operations, consistency critical

Explain the *why* behind important instructions. Agents follow reasoning-based
instructions more reliably than rigid imperatives without context. If you find
yourself writing ALWAYS or NEVER in all caps, reframe as reasoning.

### Step 6 — Manage skill size

Keep SKILL.md under 500 lines. Skills load at three levels:

1. **Metadata** (name + description) — always in context (~100 words)
2. **SKILL.md body** — loaded when skill triggers (target: under 5k words)
3. **Bundled resources** — loaded on demand by the agent

If approaching the limit, extract reference material into `references/` and
link clearly from SKILL.md. For large reference files (>300 lines), include
a table of contents.

Organize by variant when supporting multiple domains:
```
skill-name/
├── SKILL.md
└── references/
    ├── variant-a.md
    ├── variant-b.md
    └── variant-c.md
```

### Step 7 — Validate against common authoring mistakes

Check the completed skill for:

1. **Tutorial instead of procedure** — "Let me explain…", background sections.
   A skill is an operating manual, not a textbook. Cut everything the agent
   doesn't need mid-task.
2. **Goals instead of steps** — "Ensure quality" with no HOW. Every step must
   be a concrete verb the agent can execute.
3. **Reference material inline** — SKILL.md >200 lines with lookup tables or
   API schemas. Extract to `references/`.
4. **Description written last** — Write the description FIRST. It defines the
   scope everything else must serve.
5. **Missing negative boundaries** — Every skill must say what it's NOT for
   with named alternatives.
6. **Circular triggers** — "when task involves X" without defining X.
7. **Implicit capability assumptions** — Steps that assume tools the agent
   may not have. Declare dependencies explicitly.
8. **No output example** — Format described in prose but not exemplified.

## Phase 3 — Create test cases

After writing the skill draft, create 2–5 realistic test prompts — the kind
of thing a real user would actually say when they need this skill.

Good test prompts are:
- Concrete and specific with realistic detail (file paths, names, context)
- A mix of lengths and styles (formal, casual, terse, detailed)
- Include edge cases and near-miss scenarios
- Include cases that should NOT trigger the skill

Save test cases as JSONL files in `evals/` using the canonical format (see AGENTS.md "Eval Suite Structure"):

- `evals/trigger-positive.jsonl` — prompts that SHOULD trigger the skill
- `evals/trigger-negative.jsonl` — prompts that should NOT trigger the skill
- `evals/behavior.jsonl` — output quality checks

For details on field schemas, delegate to `skill-testing-harness` or refer to AGENTS.md.

Phase 3 creates seed eval files (2–5 cases each). For comprehensive test suites (8+ cases, adversarial scenarios, edge coverage), route to `skill-testing-harness` afterward.

After creating eval files, validate the new skill's structure and verify eval files are parseable:

```bash
python3 scripts/check_skill_structure.py <skill-dir>/SKILL.md    # Structural scoring
./scripts/run-evals.sh --dry-run <skill-name>                    # Verify JSONL is parseable
```

Share the test cases with the user: "Here are test cases I'd like to try.
Do these look right, or do you want to add or change any?"

## Phase 4 — Test and review

For each test case, execute the skill's procedure against the test prompt
and capture the output.

If baseline comparison is possible:
- **New skill**: Run without the skill as baseline
- **Improving existing skill**: Use the previous version as baseline

Organize results by iteration:
```
workspace/
└── iteration-1/
    ├── test-1/
    │   ├── with_skill/
    │   └── baseline/
    └── test-2/
        ├── with_skill/
        └── baseline/
```

Present results to the user for review. For each test case, show:
- The prompt
- The skill's output
- The baseline output (if available)
- Any quantitative metrics (pass rates, timing)

Ask for specific feedback: "How does this look? What would you change?"

## Phase 5 — Improve and iterate

Based on user feedback and test results:

1. **Generalize from feedback** — Avoid overfitting to specific test cases.
   The skill will be used across many different prompts. Prefer reasoning-based
   improvements over rigid rules.

2. **Keep the skill lean** — Remove instructions that aren't pulling their
   weight. Read test transcripts to identify unproductive steps.

3. **Look for repeated work** — If test runs independently produce similar
   helper scripts or patterns, bundle that as a script in `scripts/`.

4. **Apply improvements** and rerun all test cases into a new iteration
   directory.

5. **Repeat** until:
   - The user says they're satisfied
   - Feedback is empty (everything looks good)
   - No meaningful progress is being made

## Phase 6 — Finalize the skill folder

```
skill-name/
├── SKILL.md          # Frontmatter + body sections
├── evals/            # Test cases (if created)
├── scripts/          # Optional: deterministic automation
├── references/       # Optional: large docs for progressive disclosure
└── assets/           # Optional: templates, static files used in output
```

Only create subdirectories if they contain actual files.

After finalization, recommend next steps:
- Run `skill-testing-harness` to build a formal eval suite
- Run `skill-evaluation` to validate routing accuracy and output quality
- Run `skill-trigger-optimization` to optimize the description for routing
- Run `skill-safety-review` if the skill executes code or writes files

# Output contract

Deliver a complete skill folder containing:

1. **SKILL.md** with valid YAML frontmatter and all required body sections
2. **evals/** with test cases if iteration was performed
3. **references/** if the skill needs progressive disclosure
4. **scripts/** if the skill includes deterministic automation
5. **assets/** if the skill provides templates or static files

The SKILL.md must pass all Phase 2 Step 7 validation checks before delivery.

# Failure handling

- **Scope too broad**: Skill handles multiple distinct tasks → split via
  `skill-variant-splitting` or narrow the scope
- **Cannot write one-sentence definition**: Scope is wrong → keep narrowing
  until the sentence works
- **Overlaps existing skill**: Check catalog → merge or differentiate
  explicitly in descriptions
- **No clear trigger phrases**: Ask user what words they'd use when they
  need this capability
- **Description never fires in practice**: Add more trigger phrase
  variations, make description more assertive about when to activate
- **User wants to skip testing**: Proceed without iteration but note that
  untested skills have unknown quality

# Next steps

1. Build test infrastructure → `skill-testing-harness`
2. Evaluate routing and output quality → `skill-evaluation`
3. Optimize trigger description → `skill-trigger-optimization`
4. Review for safety hazards → `skill-safety-review`
5. Manage lifecycle state → `skill-lifecycle-management`
6. Compare variants if multiple drafts exist → `skill-benchmarking` (optional, not in standard pipeline)

# References

## Skill structure reference

```
skill-name/
├── SKILL.md (required)     # Frontmatter + instructions
│   ├── YAML frontmatter    # name, description (routing logic)
│   └── Markdown body       # procedure, output contract, failure handling
└── Bundled Resources (optional)
    ├── scripts/    - Executable code for deterministic/repetitive tasks
    ├── references/ - Docs loaded into context as needed
    └── assets/     - Files used in output (templates, icons, fonts)
```

- Agent Skills specification: https://agentskills.io/specification
- What are skills: https://agentskills.io/what-are-skills
