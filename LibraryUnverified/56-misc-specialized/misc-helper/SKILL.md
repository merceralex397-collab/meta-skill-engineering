---
name: misc-helper
description: "Catch-all micro-skill for quick utility tasks that take under 5 minutes and don't fit any domain skill. Triggers: 'convert this file', 'reformat this data', 'quick calculation', 'transform this text', 'one-liner to do X', 'help me with this small thing'. Do NOT use when the task requires domain expertise (use the domain skill), involves multi-step workflows, touches production code, or would take more than 5 minutes."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: misc-helper
  maturity: draft
  risk: low
  tags: [utility, catch-all, quick-task, format-conversion, text-transform]
---

# Purpose

A genuine catch-all micro-skill for one-off utility tasks that are too small to warrant a dedicated skill. Handles file format conversions, quick calculations, text transformations, data reformatting, one-liner scripts, and similar ephemeral work.

# When to use this skill

Use this skill when:

- the task is a one-off utility job: format conversion (JSON↔YAML↔TOML↔CSV), text munging (regex replace, case conversion, encoding changes), quick math/date calculations, or data reshaping
- the task will take **under 5 minutes** of agent effort end-to-end
- no existing domain skill covers the task—you have checked the skill library first
- the user asks for a "quick script", "one-liner", or "just convert/reformat this"

# Do not use this skill when

- the task requires **domain expertise**—use the domain skill instead (e.g., `bigquery-skill` for SQL optimization, `fastapi-patterns` for API design, `tauri-solidjs` for desktop app scaffolding)
- the task involves **multi-step workflows** with branching logic or significant decision-making
- the output will be committed as **production code** that needs tests, reviews, or maintenance
- the task would take more than 5 minutes—escalate to an appropriate domain skill or break it into tickets
- the task is a factual lookup with no transformation (just answer the question directly)

# Operating procedure

1. **Classify the task.** Determine the category: format conversion, text transformation, quick calculation, data reshaping, or one-liner script.
2. **Check for a better skill.** Scan active skills to confirm no domain skill covers this task. If one does, redirect immediately.
3. **Execute the minimal viable solution.** Use the simplest tool for the job:
   - Format conversion → `jq`, `yq`, `python -c`, `csvtool`, or inline code
   - Text transformation → `sed`, `awk`, `tr`, or a short Python/Node snippet
   - Quick calculation → `bc`, `python -c`, or `node -e`
   - Data reshaping → `jq`, `pandas` one-liner, or shell pipes
4. **Validate the output.** Spot-check the result: correct format, no data loss, expected row/field count.
5. **Return the result** with a one-line description of what was done.

# Decision rules

- **5-minute rule**: if the task will take more than 5 minutes, stop and recommend the correct domain skill or suggest creating a ticket.
- **Complexity escalation**: if the task grows beyond the original ask (e.g., "also validate the schema" or "handle edge cases"), pause and reassess whether a domain skill should take over.
- **No persistence**: misc-helper results are ephemeral. If the user wants the transformation to be repeatable, recommend encoding it as a script or a new skill.
- **Tool selection**: prefer standard CLI tools (`jq`, `sed`, `awk`) over writing scripts. Prefer scripts over installing new dependencies.
- **Idempotency**: when transforming files in-place, always confirm with the user or create a backup first.

# Scope boundaries

**In scope:**
- File format conversions (JSON↔YAML↔TOML↔CSV↔XML)
- Text transformations (regex, case, encoding, line ending normalization)
- Quick calculations (date math, unit conversion, base conversion)
- Data reshaping (flatten nested JSON, pivot CSV columns, extract fields)
- One-liner scripts (shell pipes, `awk` programs, `jq` filters)
- Quick lookups that require minimal synthesis (checking a file's encoding, counting lines, sampling data)

**Out of scope:**
- Anything requiring domain expertise (database optimization, API design, security review)
- Multi-step workflows with branching logic
- Production code that needs tests or maintenance
- Tasks that would benefit from a dedicated skill being created

# Output requirements

Return exactly:

1. **`Result`** — the transformed data, calculation answer, or generated one-liner
2. **`What was done`** — a single sentence describing the transformation applied (e.g., "Converted config.yaml to JSON using yq and pretty-printed with jq")

Do not add commentary, recommendations, or follow-up suggestions unless the task revealed a problem.

# Anti-patterns

- **Skill avoidance**: using misc-helper repeatedly for tasks in the same domain instead of learning or creating the proper domain skill. If you use misc-helper for the same category 3+ times, create a dedicated skill.
- **Scope creep**: letting a "quick conversion" grow into a multi-step data pipeline. Stop and redirect.
- **Over-engineering**: writing a 50-line script for a task `jq '.[] | .name'` would handle.
- **Silent data loss**: converting formats without validating that all fields survived the transformation.
- **Dependency installation**: installing new packages for a one-off task when a built-in tool would suffice.

# Related skills

- `bigquery-skill` — for any data query or analytics task
- `fastapi-patterns` — for any Python API task
- `tauri-solidjs` — for any desktop application task

# Failure handling

- If the task is ambiguous, classify it and state your interpretation before executing.
- If a format conversion loses data (e.g., YAML comments stripped in JSON conversion), warn the user and offer alternatives.
- If the task exceeds 5 minutes or grows in scope, stop and recommend the appropriate domain skill or suggest creating a ticket.
- If no CLI tool handles the format, fall back to a short Python/Node script and note the dependency.
