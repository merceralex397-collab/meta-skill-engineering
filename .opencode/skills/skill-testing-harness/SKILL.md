---
name: skill-testing-harness
description: >-
  Build trigger tests and behavior tests for a skill's evals/ directory.
  Use when "create tests for this skill", "set up evals", "build a test
  harness", a new skill needs test coverage, or a skill lacks an evals/
  directory. Do not use for running existing tests (use skill-evaluation),
  comparing skill variants (use skill-benchmarking), or updating tests that
  already exist (edit directly).
---

# Purpose

Build test infrastructure for a skill: trigger tests (positive and negative JSONL cases) and output-format tests. Enables repeatable evaluation during development and refinement.

# When to use

- User says "create tests for this skill", "set up evals", "build a test harness"
- New skill needs test coverage
- Skill lacks an evals/ directory or test fixtures
- Skill refinement requires regression tests

# When NOT to use

- Running existing tests → `skill-evaluation`
- Comparing skill variants → `skill-benchmarking`
- Tests exist and need minor edits → edit directly

# Procedure

## Step 1 — Analyze the target skill

Read the target SKILL.md and extract:
- Trigger signals from the `description` field
- Positive cases from "When to use" section
- Negative cases from "When NOT to use" section
- Expected output format from output contract
- Quality criteria from procedure steps

## Step 2 — Create trigger-positive.jsonl

File: `evals/trigger-positive.jsonl`

Each line is a JSON object for a prompt that SHOULD activate the skill. Include 8–15 cases covering core use cases, edge cases, and paraphrasings.

**Positive cases must cover these categories:**
- **(a) Exact match:** Prompts that directly mirror a "When to use" bullet
- **(b) Paraphrase:** Same intent expressed with different vocabulary
- **(c) Indirect:** Requests that imply the skill's purpose without naming it (e.g., describing a problem the skill solves)
- **(d) Multi-step:** Requests where this skill is one component of a larger task

Aim for at least 2 cases per category. If a category produces fewer than 2 natural cases, the skill's trigger surface may be too narrow — note this in the README.

```jsonl
{"prompt": "Create a new skill for handling PDF extraction", "expected": "trigger", "category": "core", "notes": "Direct request matching primary use case"}
{"prompt": "I need a reusable procedure for database migrations", "expected": "trigger", "category": "indirect", "notes": "Implicit skill creation — repeated task pattern"}
{"prompt": "Can you make a skill that handles our deploy workflow?", "expected": "trigger", "category": "paraphrase", "notes": "Casual phrasing"}
{"prompt": "Package this workflow as a skill for the team", "expected": "trigger", "category": "edge", "notes": "Packaging intent implies creation first"}
```

| Field | Required | Description |
|-------|----------|-------------|
| `prompt` | Yes | User message that should trigger the skill |
| `expected` | Yes | Always `"trigger"` for positive cases |
| `category` | Yes | One of: `core`, `indirect`, `paraphrase`, `edge` |
| `notes` | No | Why this case should trigger |

## Step 3 — Create trigger-negative.jsonl

File: `evals/trigger-negative.jsonl`

Each line is a JSON object for a prompt that should NOT activate the skill. Include 8–15 cases covering adjacent skills, out-of-scope tasks, and common confusion.

**Negative cases must cover these categories:**
- **(a) Anti-match:** Prompts that directly mirror a "When NOT to use" bullet
- **(b) Near-miss:** Tasks from adjacent skills that share vocabulary (e.g., "evaluate" vs "build evaluation for")
- **(c) Similar vocabulary, different intent:** Requests using words like "test" or "eval" that mean something else in context
- **(d) Overly broad:** Vague requests that superficially match but shouldn't trigger (e.g., "improve this skill" — too broad for a test harness)

**Minimum distribution across all trigger cases:** 60% positive, 30% negative, 10% edge-case (ambiguous intent where `expected` may be `"trigger"` or `"no_trigger"` depending on interpretation — document the rationale in `notes`).

```jsonl
{"prompt": "Fix the trigger description on this skill", "expected": "no_trigger", "better_skill": "skill-trigger-optimization", "notes": "Trigger fix, not test creation"}
{"prompt": "Run the existing eval suite", "expected": "no_trigger", "better_skill": "skill-evaluation", "notes": "Running tests, not building them"}
{"prompt": "Compare these two skill variants", "expected": "no_trigger", "better_skill": "skill-benchmarking", "notes": "Benchmarking, not test infrastructure"}
{"prompt": "Write a Python function to parse JSON", "expected": "no_trigger", "better_skill": null, "notes": "General coding, not skill engineering"}
```

| Field | Required | Description |
|-------|----------|-------------|
| `prompt` | Yes | User message that should NOT trigger the skill |
| `expected` | Yes | Always `"no_trigger"` for negative cases |
| `better_skill` | Yes | Correct skill name, or `null` if none matches |
| `notes` | No | Why this case should not trigger |

## Step 4 — Create behavior tests

File: `evals/behavior.jsonl`

Each line defines a prompt with expected output characteristics.

**Output quality assertions should check:**
- **(a) Required sections present:** Every section named in the skill's output contract must appear
- **(b) No hallucinated sections:** Flag any output section not specified in the output contract
- **(c) Output length within range:** Set `min_output_lines` based on the skill's complexity. A skill with 3 procedure steps shouldn't produce 200-line output.
- **(d) Concrete vs vague language:** Flag if >30% of output sentences use hedge words ("consider", "may want to", "could potentially", "it might be useful to"). Skills should produce decisions, not suggestions.

```jsonl
{"prompt": "Create trigger tests for skill-authoring", "expected_sections": ["trigger-positive", "trigger-negative"], "required_patterns": ["\"expected\": \"trigger\"", "\"expected\": \"no_trigger\""], "forbidden_patterns": ["TODO", "placeholder", "consider adding"], "min_output_lines": 15, "notes": "Must produce both positive and negative trigger files"}
{"prompt": "Build a full test harness for the pdf-extraction skill", "expected_sections": ["trigger-positive", "trigger-negative", "behavior"], "required_patterns": ["\"better_skill\"", "\"expected_sections\""], "forbidden_patterns": ["may want to", "could potentially"], "min_output_lines": 20, "notes": "Full harness must include all three eval files plus README"}
```

## Step 5 — Create test fixtures (if needed)

Directory: `evals/fixtures/`

Only create fixtures when the skill processes files or external data: sample inputs, mock data for deterministic testing, expected output examples.

## Step 6 — Create evals README

File: `evals/README.md`

```markdown
# Eval Suite for [skill-name]

## Files
| File | Purpose | Case Count |
|------|---------|------------|
| trigger-positive.jsonl | Prompts that SHOULD trigger | N |
| trigger-negative.jsonl | Prompts that should NOT trigger | N |
| behavior.jsonl | Output format/content validation | N |

## Running
- Trigger tests: Feed each prompt to router, verify trigger/no_trigger matches expected
- Output tests: Run skill on each prompt, verify files/patterns/counts

## Adding Cases
Append new JSON lines to the appropriate .jsonl file. Follow the field schema:
- trigger-positive: prompt, expected ("trigger"), category, notes
- trigger-negative: prompt, expected ("no_trigger"), better_skill, notes
```

## Step 7 — Verify the test suite

After creating all eval files, verify they are well-formed and parseable:

```bash
./scripts/run-evals.sh --dry-run <skill-name>
```

This validates JSONL syntax, lists all test cases, and confirms the eval runner can parse them. Fix any errors before delivering the test suite.

# Output contract

```
evals/
├── README.md              # How to run and extend tests
├── trigger-positive.jsonl # 8–15 should-trigger cases
├── trigger-negative.jsonl # 8–15 should-not-trigger cases
├── behavior.jsonl         # Output format/content validation
└── fixtures/              # Optional test data
```

All JSONL files use one JSON object per line, newline-delimited.

# Failure handling

- **No clear triggers in description**: Cannot write trigger tests — flag for `skill-trigger-optimization` first
- **Output format undefined**: Cannot write output tests — flag for `skill-improver` to add output contract
- **Too few distinct trigger phrases**: Minimum 5 positive, 5 negative; if the skill is too narrow, consult `skill-catalog-curation` to assess whether it should be merged
- **Skill too complex for single harness**: Split into sub-capabilities with separate JSONL files per capability
- **No comparable baseline**: Skip baseline comparison; focus on trigger accuracy and output format compliance

# Next steps

After building the test harness:
- Run the tests → `skill-evaluation`
- Compare variants → `skill-benchmarking`
