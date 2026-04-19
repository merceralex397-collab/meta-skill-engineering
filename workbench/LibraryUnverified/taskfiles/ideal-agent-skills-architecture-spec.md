# Ideal Agent Skills Architecture Spec

## Purpose

This document defines a practical architecture for building, testing, packaging, and maintaining high-quality Agent Skills across multiple clients and agent environments.

In plain terms, the goal is to create **one canonical skill system** that can be reused across tools like Codex, Gemini CLI, OpenCode, GitHub Copilot, and future Agent Skills-compatible runtimes without rewriting the skill from scratch for each platform.

A **skill** is best understood as a reusable operating manual for an AI agent:
- a short summary that helps the agent decide when to use it,
- a body of instructions describing how to perform the task,
- optional scripts for repeatable mechanics,
- optional references and assets for supporting material.

The architecture below is designed to optimize for:
- portability,
- routing accuracy,
- evaluation quality,
- maintainability,
- trust and safety,
- team-scale reuse.

---

## Core thesis

The file format is not the hard part.

The hard parts are:
1. **getting the skill to trigger on the right tasks,**
2. **proving that the skill improves outcomes,**
3. **keeping the skill portable across clients,**
4. **preventing skills from turning into unmaintainable prompt blobs,**
5. **ensuring scripts and bundled resources are safe and understandable.**

Therefore, this architecture treats a skill as a product with five layers:

1. **Portable core** – the canonical skill content.
2. **Client overlays** – client-specific metadata and installation wrappers.
3. **Validation** – linting, conformance checks, and static analysis.
4. **Evaluation** – routing tests, output tests, regression comparisons.
5. **Distribution and trust** – packaging, provenance, versioning, and policy.

---

## Design principles

### 1. One canonical source of truth
Maintain a single authoritative skill source, then derive client-specific adaptations from it.

This prevents:
- drift between Codex/Gemini/OpenCode/Copilot variants,
- duplicated maintenance,
- conflicting instructions across platforms.

### 2. Progressive disclosure
Only the minimum information should be loaded initially.

In practice:
- the agent first sees a short summary,
- the full `SKILL.md` body is only loaded when relevant,
- references and assets are only loaded when the instructions explicitly call for them,
- scripts are only executed when needed.

This keeps context smaller and reduces confusion.

### 3. Routing is a first-class concern
The summary metadata, especially the description, is not marketing copy. It is routing logic expressed in natural language.

If routing is weak, the rest of the skill does not matter.

### 4. Instructions first, scripts second
Use the model for reasoning and adaptation.
Use scripts for repeated, mechanical, deterministic operations.

Do not turn the skill into a shell wrapper unless shell determinism is the point of the skill.

### 5. Evaluation before expansion
Do not expand a skill family just because the format makes it easy.
A small number of well-tested skills is better than a large pile of vague ones.

### 6. Portable core, narrow overlays
Keep the core format vendor-neutral.
Put client-specific behavior in overlays or generated files.

### 7. Skills are conditional expertise, not global law
Always-on project rules belong in global repo instructions, policy files, or agent config.
Skills should activate only when their expertise is specifically relevant.

---

## Recommended repository structure

```text
agent-skills/
├─ README.md
├─ registry.yaml
├─ tooling/
│  ├─ validate_skills.py
│  ├─ build_overlays.py
│  ├─ package_skills.py
│  ├─ run_evals.py
│  └─ diff_reports.py
├─ schemas/
│  ├─ skill.schema.json
│  ├─ eval-trigger.schema.json
│  └─ eval-output.schema.json
├─ skills/
│  ├─ api-debugging/
│  │  ├─ SKILL.md
│  │  ├─ references/
│  │  │  ├─ common-errors.md
│  │  │  └─ service-matrix.md
│  │  ├─ scripts/
│  │  │  ├─ replay_request.py
│  │  │  └─ normalize_logs.py
│  │  ├─ assets/
│  │  │  └─ sample-log.json
│  │  ├─ evals/
│  │  │  ├─ trigger.json
│  │  │  ├─ behavior.json
│  │  │  ├─ outputs.json
│  │  │  └─ baselines/
│  │  │     └─ no_skill.json
│  │  ├─ overlays/
│  │  │  ├─ openai/
│  │  │  │  └─ openai.yaml
│  │  │  ├─ gemini/
│  │  │  │  └─ install.json
│  │  │  ├─ opencode/
│  │  │  │  └─ permissions.json
│  │  │  └─ copilot/
│  │  │     └─ metadata.json
│  │  └─ CHANGELOG.md
│  └─ pdf-extraction/
│     └─ ...
└─ dist/
   ├─ codex/
   ├─ gemini/
   ├─ opencode/
   ├─ copilot/
   └─ bundles/
```

This structure separates:
- canonical skill source,
- evaluations,
- overlays,
- build outputs,
- global tooling.

---

## Canonical skill contract

Every skill should be treated as a package with these logical parts.

### 1. Identity
Defines:
- stable name,
- version,
- ownership,
- compatibility,
- license,
- tags.

### 2. Routing metadata
Defines:
- what the skill does,
- when it should trigger,
- related terms and likely task phrasings,
- optional negative boundaries.

### 3. Operating instructions
Defines:
- what workflow the agent should follow,
- key decision points,
- output expectations,
- safety or quality checks,
- when to consult references,
- when to call scripts.

### 4. Deterministic helpers
Defines:
- scripts or command wrappers,
- data transformations,
- validators,
- exact mechanics the model should not reinvent every time.

### 5. Knowledge attachments
Defines:
- longer references,
- schemas,
- examples,
- lookup tables,
- reusable exemplars.

### 6. Evaluation artifacts
Defines:
- trigger tests,
- negative tests,
- output tests,
- baseline comparisons,
- regression history.

### 7. Overlay metadata
Defines:
- client-specific files,
- permissions,
- UI hints,
- packaging details.

---

## Standardized skill authoring template

Each skill should contain a `SKILL.md` with a compact, highly structured layout.

### Recommended shape

```md
---
name: api-debugging
description: Use this skill when investigating API failures, inconsistent HTTP responses, malformed payloads, authentication issues, rate limits, or service-to-service request debugging. Especially relevant when the task involves logs, curl examples, retries, replaying requests, or isolating whether the issue is client-side, server-side, or environmental.
license: Apache-2.0
compatibility:
  - openai
  - gemini
  - opencode
  - copilot
metadata:
  owner: platform-team
  version: 0.4.0
  tags:
    - api
    - debugging
    - logs
    - http
---

# Purpose
Brief explanation of what the skill is for.

# When to use this skill
Concrete triggering examples.

# When not to use this skill
Common near-misses and boundaries.

# Workflow
Ordered steps the agent should follow.

# Decision rules
Important branch points and heuristics.

# Script usage
Exact guidance on when to call scripts.

# Reference usage
Exact guidance on when to open reference files.

# Output requirements
Expected structure and quality checks.

# Failure handling
What to do when information is missing or validation fails.
```

This template is intentionally repetitive and explicit because repetition in structure improves maintainability.

---

## Routing specification

Routing is the most important part of a skill.

### Description design rules
The description must answer two questions:
1. **What does this skill help with?**
2. **When should the agent use it?**

A good description should include:
- domain terms,
- likely user phrasings,
- neighboring synonyms,
- concrete task shapes,
- hints about the outputs or tools involved.

### What the description should avoid
Avoid descriptions that are:
- generic,
- branding-oriented,
- vague,
- too short,
- full of internal jargon with no task cues.

Bad example:

```md
Handles PDFs.
```

Good example:

```md
Use this skill when extracting structured information from PDFs, comparing PDF contents, locating tables or charts inside PDFs, or turning PDF material into markdown summaries, spreadsheets, or cited reports. Especially relevant when the task references pages, appendices, figures, scanned documents, or attachments.
```

### Trigger boundaries
Where useful, include a short “when not to use” section.
This reduces false positives.

Example:
- Use for extracting information from a PDF attachment.
- Do not use for plain text pasted directly into the chat unless the task explicitly requires PDF-specific handling.

### Routing quality metrics
Each skill should track at least:
- true positive rate,
- false positive rate,
- false negative rate,
- trigger precision,
- trigger recall.

These do not need to be mathematically perfect at first, but they should be monitored over time.

---

## Instruction design rules

The body of a skill should teach the agent how to work, not merely what to say.

### Good instruction qualities
Instructions should be:
- procedural,
- scoped,
- concrete,
- compact,
- branch-aware,
- reality-based.

### Use ordered workflows
Where the task is fragile, prefer ordered steps.

Example:
1. Identify the relevant inputs.
2. Validate whether enough information exists.
3. Decide whether a bundled script is needed.
4. Run the script only if its output will materially help.
5. Verify the result.
6. Produce the final answer in the required format.

### Explain rationale selectively
Where flexibility matters, explain why a step exists.
This helps the model adapt rather than mechanically obeying stale instructions.

### Do not over-constrain everything
Too many MUST/ALWAYS/NEVER rules make the model brittle.
Reserve hard rules for:
- safety,
- compliance,
- destructive actions,
- output contracts,
- critical path sequences.

### Prefer branch prompts over giant prose blocks
Use sections like:
- “If the input is missing …”
- “If the task mentions logs …”
- “If the user asks for a spreadsheet …”
- “If the script fails …”

This is easier for both humans and models to navigate.

---

## Reference file strategy

Reference files should hold material that is useful but not always necessary.

### Good reference candidates
Put these in `references/`:
- API error catalogs,
- schema examples,
- decision tables,
- policy notes,
- deep procedural variations,
- exemplar outputs,
- service-specific matrices,
- troubleshooting maps.

### Bad reference candidates
Do not put these in references if they are critical every time:
- the primary workflow,
- the main safety rules,
- the basic output contract.

Those belong in `SKILL.md`.

### Explicit lookup cues
The main skill should tell the agent exactly when to read a reference file.

Good:
- Read `references/common-errors.md` if the response includes 401, 403, or 429.
- Read `references/service-matrix.md` if the issue involves multiple environments or endpoints.

Bad:
- See references folder if needed.

---

## Script strategy

Scripts should exist to remove repeated, mechanical, or deterministic work from the model.

In plain terms:
- if the model would keep redoing the same shell ritual, make it a script;
- if the task needs judgment, comparison, explanation, or adaptation, keep it in the model.

### Good script use cases
Use scripts for:
- parsing logs,
- normalizing data,
- generating exact file structures,
- replaying requests,
- validating schema conformance,
- converting formats,
- diffing outputs,
- packaging artifacts,
- deterministic extraction.

### Poor script use cases
Avoid scripts that:
- simply restate the instructions,
- hide important logic from review,
- are interactive by default,
- require constant manual editing,
- replace reasoning with opaque automation.

### Script rules
All scripts should:
- be non-interactive by default,
- support `--help`,
- return meaningful exit codes,
- emit useful stderr on failure,
- avoid hidden prompts,
- document inputs and outputs,
- be safe to call repeatedly,
- not silently modify unrelated files.

### Script interface convention
Prefer a consistent convention like:

```text
scripts/<tool>.py --input <path> --output <path> [options]
```

Where possible, emit structured outputs such as JSON, Markdown, or clearly labeled plain text.

---

## Assets strategy

Assets should be used for:
- sample inputs,
- golden outputs,
- template files,
- fixture data,
- static examples.

Assets should not contain:
- secrets,
- private credentials,
- large irrelevant binaries,
- opaque files with no documentation.

Every non-obvious asset should be referenced somewhere in either:
- `SKILL.md`,
- a reference file,
- or the eval suite.

---

## Overlay architecture

The portable core should remain stable while client-specific overlays capture platform differences.

### Why overlays exist
Different clients vary in:
- discovery paths,
- UI metadata,
- permission models,
- package/install formats,
- agent policy support,
- skill scanning behavior.

Trying to force all of that into the core skill will make the core worse.

### Overlay model
Each skill may include:

```text
overlays/
├─ openai/
├─ gemini/
├─ opencode/
└─ copilot/
```

These should contain only client-specific data.

### Codex / OpenAI overlay
Use this for:
- `agents/openai.yaml`,
- invocation hints,
- tool dependency declarations,
- UI-specific metadata.

### Gemini overlay
Use this for:
- extension packaging metadata,
- user/workspace install helpers,
- approval-related annotations,
- path access assumptions.

### OpenCode overlay
Use this for:
- permission presets,
- path alias assumptions,
- local/global placement defaults,
- runtime policy patterns.

### Copilot overlay
Use this for:
- user-level install helpers,
- workspace placement hints,
- documentation for repo/user scope,
- metadata needed by wrapper tooling.

### Build rule
No overlay should override the core semantic behavior without explicit documentation.

If a client requires different behavior, record that difference in:
- `CHANGELOG.md`,
- the overlay’s README,
- and the generated dist metadata.

---

## Validation layer

A professional skill system needs linting and static validation.

### Minimum validator checks
Validate:
- presence of `SKILL.md`,
- frontmatter parse success,
- valid name format,
- matching directory name,
- description length bounds,
- references that actually exist,
- script paths that actually exist,
- duplicate tags,
- forbidden secrets or credential strings,
- overly large assets,
- missing eval files,
- malformed overlay files.

### Recommended semantic checks
Also warn on:
- vague descriptions,
- descriptions with no trigger examples,
- skills with no negative evals,
- instructions longer than target thresholds,
- unused reference files,
- scripts mentioned nowhere,
- references mentioned nowhere,
- identical or near-duplicate skills,
- unclear ownership.

### Validation policy levels
Use three levels:
- **error** – build should fail,
- **warning** – build passes but report flags issues,
- **advisory** – informational quality suggestion.

---

## Evaluation architecture

Evaluation should be mandatory, not optional.

### Why evaluation matters
A skill can fail in several ways:
- not triggering when it should,
- triggering when it should not,
- triggering correctly but producing no better output,
- improving quality but increasing cost or latency too much,
- producing overfit behavior on narrow prompts.

### Required eval categories
Every serious skill should have at least four categories.

#### 1. Trigger evals
Test whether the skill activates on relevant prompts.

Example cases:
- clear positives,
- subtle positives,
- near-miss negatives,
- obvious negatives.

#### 2. Behavior evals
Test whether the model follows the intended workflow once the skill is active.

Example checks:
- does it consult the correct reference file,
- does it call the right script only when needed,
- does it avoid unnecessary steps,
- does it follow the output contract.

#### 3. Output evals
Test the final output quality.

Example checks:
- structure,
- completeness,
- correctness,
- citations if applicable,
- formatting,
- absence of hallucinated steps.

#### 4. Baseline comparisons
Compare:
- skill vs no skill,
- new version vs previous version,
- alternative descriptions vs current description.

### Suggested eval file layout

```text
evals/
├─ trigger.json
├─ behavior.json
├─ outputs.json
├─ baselines/
│  ├─ no_skill.json
│  └─ v0.3.0.json
└─ reports/
   └─ latest.md
```

### Trigger eval design guidance
A good trigger eval set includes:
- realistic user phrasing,
- shorthand requests,
- noisy prompts,
- ambiguous prompts,
- prompts where the task is simple enough that the model may skip activation,
- prompts with similar language but wrong domain.

### Output scoring
Use a combination of:
- binary assertions,
- rubric scoring,
- human review for borderline cases.

### Regression gating
A skill update should be blocked when it materially worsens:
- trigger precision,
- trigger recall,
- critical workflow adherence,
- output correctness,
- latency beyond acceptable thresholds.

---

## Skill registry design

A central registry file makes the ecosystem easier to manage.

### Suggested `registry.yaml`
It should contain, per skill:
- name,
- path,
- owner,
- version,
- tags,
- supported clients,
- maturity level,
- risk level,
- maintenance status,
- last eval status,
- changelog pointer.

### Maturity levels
Suggested levels:
- `experimental`
- `draft`
- `tested`
- `production`
- `deprecated`

### Risk levels
Suggested levels:
- `low`
- `moderate`
- `high`
- `destructive`

These are useful when skills can execute scripts or perform side effects.

---

## Security and trust model

Skills are executable knowledge bundles. That means they can be dangerous.

### Threats to consider
- malicious scripts,
- misleading instructions,
- hidden destructive side effects,
- secrets accidentally bundled into assets,
- repo-level skill injection from untrusted projects,
- shadowing trusted skills with similarly named ones,
- hostile references that tell the model to exfiltrate or destroy data.

### Security controls
At minimum, implement:
- trusted-source policy for repo-local skills,
- clear precedence rules,
- skill shadowing warnings,
- script review requirements,
- secret scanning,
- destructive-operation labeling,
- optional allowlists for tool usage,
- version pinning where distributed remotely.

### Provenance metadata
Each production-grade skill should carry provenance fields such as:
- author,
- maintainer,
- source repo,
- commit hash or release tag,
- build timestamp,
- signed package hash if supported.

### Review policy
Before a skill reaches production:
- review the description,
- review scripts,
- review references for hidden instructions,
- inspect eval results,
- inspect negative trigger cases,
- verify ownership.

---

## Packaging and distribution

The packaging system should support multiple targets from one source.

### Distribution targets
Generate:
- a raw portable directory bundle,
- client-specific install bundles,
- optional archive packages,
- optional registry metadata,
- checksum files.

### Build pipeline
Recommended steps:
1. validate core skill,
2. validate overlays,
3. run evals,
4. generate overlay outputs,
5. package per target,
6. write manifest and checksums,
7. publish to dist.

### Versioning
Use semantic versioning where practical:
- patch = minor instruction/script fix,
- minor = additive behavior or new references,
- major = routing changes, major workflow changes, compatibility changes.

### Changelog discipline
Every skill should track:
- description changes,
- workflow changes,
- new or removed scripts,
- client overlay changes,
- eval-result impact.

---

## Observability and telemetry

A mature skill system should measure how skills behave in the wild.

### Useful metrics
Track:
- activation count,
- successful activation rate,
- skipped activation rate,
- false positive reports,
- average tokens added by skill activation,
- average runtime of associated scripts,
- script failure rate,
- evaluation pass rate over time,
- per-client compatibility issues,
- human override frequency.

### Why this matters
Without telemetry, poor routing can look like poor model quality.
Sometimes the model is fine and the skill is just being invoked badly.

### Privacy caution
Telemetry should avoid storing sensitive user content unless explicitly permitted and properly handled.
Prefer aggregate metrics and redacted traces.

---

## Skill composition model

Single flat skills are often not enough for larger workflows.

### Recommended composition pattern
Use three classes of skills:

#### 1. Coordinator skills
These define multi-step workflows and decide which specialized skills to activate.

#### 2. Worker skills
These perform domain-specific operations.
Examples:
- PDF extraction,
- API debugging,
- spreadsheet generation,
- test triage.

#### 3. Validator skills
These verify outputs against contracts.
Examples:
- patch-review skill,
- citation-audit skill,
- formatting-check skill.

### Composition rules
A coordinator skill should:
- activate worker skills only when clearly relevant,
- avoid chaining too many skills at once,
- record handoff expectations,
- define when to stop delegating and synthesize the final answer.

### Avoid uncontrolled skill cascades
Do not create a system where every skill can recursively invoke many others without discipline.
That increases token cost and makes behavior harder to debug.

---

## Anti-patterns to avoid

### 1. Giant monolithic skill
A huge manual covering ten different domains.
This will route badly and become impossible to maintain.

### 2. Description as marketing copy
If the description sounds polished but not operational, routing will suffer.

### 3. No negative tests
This almost always leads to over-triggering.

### 4. Scripts with hidden side effects
If a script mutates files, sends network requests, or deletes data, that must be obvious.

### 5. Duplicate overlapping skills
If two skills both claim “debug APIs” but differ only slightly, the agent may route inconsistently.

### 6. Client-specific hacks in the core
The core should not be cluttered with one platform’s quirks.

### 7. Bundling secrets or credentials
This should be an automatic hard failure.

### 8. No owner, no changelog, no lifecycle status
Unowned skills rot.

---

## Suggested tooling suite

A complete implementation should provide these tools.

### `validate_skills.py`
Checks structural and semantic validity.

### `run_evals.py`
Runs trigger, behavior, and output evals.

### `build_overlays.py`
Generates target-specific metadata from the core skill.

### `package_skills.py`
Builds the dist artifacts for each client.

### `diff_reports.py`
Compares skill versions, routing changes, and eval deltas.

### Optional advanced tools
You may also want:
- `suggest_scripts.py` – identifies repeated patterns that could become scripts,
- `suggest_descriptions.py` – proposes improved routing descriptions,
- `dedupe_skills.py` – identifies overlapping skills,
- `review_traces.py` – scores actual activation traces,
- `security_scan.py` – secrets, dangerous commands, provenance checks.

---

## Rollout plan

### Phase 1: Foundation
Build:
- canonical repository structure,
- validator,
- 3 to 5 exemplar skills,
- manual overlays,
- basic trigger and output evals.

### Phase 2: Productization
Add:
- overlay generation,
- packaging pipeline,
- registry metadata,
- changelog discipline,
- telemetry collection.

### Phase 3: Quality hardening
Add:
- baseline regression gating,
- negative trigger suites,
- script promotion workflow,
- overlap detection,
- destructive skill labeling,
- provenance metadata.

### Phase 4: Ecosystem maturity
Add:
- remote skill distribution,
- signed bundles,
- registry publishing,
- standardized install flows,
- team approvals and trust policies,
- composition-aware coordinator skills.

---

## Example quality bar for a production-ready skill

A skill should not be considered production-ready unless it has:
- a clear description with realistic trigger coverage,
- a bounded scope,
- a structured workflow,
- explicit output requirements,
- at least one negative trigger set,
- at least one no-skill baseline,
- script documentation if scripts exist,
- no secret leakage,
- an owner,
- a changelog,
- a supported-client declaration,
- a passing validation report.

---

## Example maintenance workflow

When updating a skill:
1. change the core instructions or description,
2. run validation,
3. run trigger evals,
4. run output evals,
5. compare against prior baseline,
6. inspect regressions,
7. update overlay outputs if needed,
8. increment version,
9. update changelog,
10. package and publish.

This should be automated where possible.

---

## Future extensions

The following areas are worth designing for even if not implemented immediately.

### 1. Remote skill providers
Allow fetching skills from:
- local filesystem,
- Git repos,
- static HTTP sources,
- blob/object storage,
- curated registries.

### 2. Signed skill bundles
Add package signatures and integrity verification.

### 3. Policy-aware skills
Attach machine-readable metadata for:
- destructive actions,
- network access,
- external tool usage,
- compliance categories.

### 4. Cost-aware routing
Track and optionally expose:
- expected token footprint,
- likely script cost,
- likely runtime,
- probable benefit category.

### 5. Auto-derived skills
Generate candidate skills from:
- successful transcripts,
- repeated debugging traces,
- SOP docs,
- incident postmortems,
- code review patterns.

### 6. Skill graph orchestration
Represent dependencies or recommended pairings between skills without forcing uncontrolled recursion.

---

## Final recommendation

The best practical architecture is:

- **one portable skill core,**
- **thin client overlays,**
- **mandatory evaluation,**
- **strong validation,**
- **disciplined packaging,**
- **clear provenance and trust policy.**

If forced to prioritize, prioritize in this order:
1. routing quality,
2. evaluation quality,
3. maintainable core structure,
4. script discipline,
5. distribution polish.

That ordering matters because a perfectly packaged skill that routes badly is still a bad skill.

---

## Appendix A: concise build checklist

- Write or update `SKILL.md`
- Keep the scope narrow
- Make the description operational
- Add explicit trigger examples
- Add explicit non-trigger examples where useful
- Add references only for optional deep material
- Add scripts only for deterministic repeated mechanics
- Run validation
- Run trigger evals
- Run output evals
- Compare against baseline
- Update overlays
- Update changelog
- Package target outputs
- Publish with provenance metadata

---

## Appendix B: minimal registry example

```yaml
skills:
  - name: api-debugging
    path: skills/api-debugging
    owner: platform-team
    version: 0.4.0
    maturity: tested
    risk: low
    supported_clients:
      - openai
      - gemini
      - opencode
      - copilot
    tags:
      - api
      - logs
      - debugging
    last_eval_status: pass
    changelog: skills/api-debugging/CHANGELOG.md
```

---

## Appendix C: minimal trigger eval example

```json
{
  "skill": "api-debugging",
  "cases": [
    {
      "prompt": "Why am I getting a 429 from this service and how can I verify whether it is rate limiting or a bad token?",
      "should_trigger": true
    },
    {
      "prompt": "Summarize this markdown meeting note.",
      "should_trigger": false
    },
    {
      "prompt": "Compare these HTTP logs and tell me whether the client or the server is malformed.",
      "should_trigger": true
    },
    {
      "prompt": "Write a generic explanation of what an API is.",
      "should_trigger": false
    }
  ]
}
```

---

## Appendix D: decision rule summary

Use always-on project instructions for:
- repo-wide coding rules,
- style expectations,
- default tool preferences,
- universal safety rules.

Use skills for:
- specialist workflows,
- deep task-specific instructions,
- optional procedural expertise,
- multi-step domain logic,
- repeatable structured tasks that are not universally relevant.
