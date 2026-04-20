# Ideal Agent Skills Architecture Spec (Implementation-Focused)

Version: 2  
Status: Proposed reference architecture  
Audience: tool authors, agent framework builders, internal platform teams, serious skill authors

---

## 1. Purpose

This document defines an implementation-focused architecture for a portable **Agent Skills** system.

In plain terms:

- a **skill** is a reusable task-specific playbook for an agent,
- the **portable core** contains the actual expertise,
- **client overlays** adapt that skill to specific runtimes such as Codex, Gemini CLI, OpenCode, or Copilot,
- an **evaluation harness** proves the skill helps,
- a **validator and packaging layer** keeps the system consistent and shippable.

The goal is not just to make valid skills. The goal is to make skills that are:

1. portable,
2. discoverable,
3. testable,
4. safe,
5. maintainable,
6. worth activating.

---

## 2. Design principles

### 2.1 Portable core, client-specific edges
Keep the main skill logic in a vendor-neutral format. Add product-specific behavior in overlays.

### 2.2 Progressive disclosure
Only load the minimum needed at each stage:

- catalog metadata first,
- `SKILL.md` body only when activated,
- scripts/references/assets only when the instructions call for them.

### 2.3 Routing is a first-class problem
The `description` field is not decoration. It is routing metadata. A bad description can make a strong skill useless.

### 2.4 Evaluation is mandatory
A skill without trigger tests and baseline comparisons is not finished.

### 2.5 Scripts should handle mechanics, not judgment
Use scripts for deterministic repeated actions. Use the model for interpretation, summarization, prioritization, and adaptation.

### 2.6 Trust boundaries matter
Repo-level skills can be hostile or broken. Skills need provenance, validation, and permission-aware loading.

---

## 3. Reference repository layout

```text
agent-skills/
├── README.md
├── skills/
│   ├── pdf-extraction/
│   │   ├── SKILL.md
│   │   ├── references/
│   │   │   ├── table-extraction.md
│   │   │   └── image-heavy-pdfs.md
│   │   ├── scripts/
│   │   │   ├── extract_tables.py
│   │   │   └── summarize_layout.py
│   │   ├── assets/
│   │   │   └── sample.pdf
│   │   ├── overlays/
│   │   │   ├── openai/
│   │   │   │   └── openai.yaml
│   │   │   ├── gemini/
│   │   │   │   └── metadata.yaml
│   │   │   ├── copilot/
│   │   │   │   └── install-notes.md
│   │   │   └── opencode/
│   │   │       └── permissions.yaml
│   │   ├── evals/
│   │   │   ├── trigger-positive.jsonl
│   │   │   ├── trigger-negative.jsonl
│   │   │   ├── behavior.jsonl
│   │   │   └── expected-outcomes.md
│   │   ├── tests/
│   │   │   └── test_extract_tables.py
│   │   ├── manifest.yaml
│   │   └── CHANGELOG.md
│   └── repo-patch-planning/
│       └── ...
├── tooling/
│   ├── validate_skill.py
│   ├── package_skill.py
│   ├── generate_overlays.py
│   ├── run_evals.py
│   ├── score_evals.py
│   ├── install_skill.py
│   └── diff_baseline.py
├── schemas/
│   ├── manifest.schema.json
│   ├── eval-case.schema.json
│   └── overlay.schema.json
├── docs/
│   ├── authoring-guide.md
│   ├── routing-guide.md
│   ├── security-model.md
│   └── portability-matrix.md
└── registry/
    ├── catalog.json
    └── signatures/
```

---

## 4. The skill object model

Every skill should be treated as five layers.

### 4.1 Layer A: routing metadata
This is the minimal catalog-facing surface:

- `name`
- `description`
- optionally tags, compatibility, allowed tools, risk level, and cost hints

### 4.2 Layer B: behavioral instructions
This is the body of `SKILL.md`. It tells the agent what to do once the skill is active.

### 4.3 Layer C: deterministic helpers
Scripts, reusable command wrappers, validation helpers, formatters, converters, and test harnesses.

### 4.4 Layer D: references and examples
Large or specialized material that should only be read if relevant.

### 4.5 Layer E: operations
Validation, packaging, provenance, security, installation, evaluation, and reporting.

---

## 5. Canonical `SKILL.md` template

The template below is intended to be portable across clients.

```markdown
---
name: pdf-extraction
description: Extract structured information from PDFs when the task involves tables, page layout interpretation, OCR fallback decisions, or document-to-structured-data conversion. Use this for requests involving screenshots of PDF pages, table recovery, layout-aware summarization, and extracting data from scanned or image-heavy PDFs. Do not use for ordinary plain-text summarization when the PDF text is already clean and directly readable.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: platform-team
  domain: documents
  maturity: stable
  risk: low
  tags: [pdf, extraction, ocr, tables, layout]
allowed-tools:
  - file_search
  - python
  - python_user_visible
---

# Purpose

Use this skill to extract data from PDFs when simple text parsing is not enough.

# When to use this skill

Use this skill when:

- a PDF contains tables that may be lost in plain text extraction,
- visual page structure matters,
- scanned or image-heavy pages may require OCR fallback,
- the user wants structured output such as CSV, JSON, or spreadsheet-ready rows,
- a chart, diagram, or figure must be interpreted from the page image.

Do not use this skill when:

- the document is already plain, clean text and the user only wants a normal summary,
- the task is unrelated to PDFs,
- another skill more specifically matches the document domain and this skill would add no value.

# Operating procedure

1. Determine whether text extraction alone is sufficient.
2. If tables, diagrams, or layout matter, inspect the relevant page images.
3. Use OCR only if visual inspection or parsed text is insufficient.
4. Normalize extracted data into a clearly structured format.
5. State uncertainties explicitly, especially for low-confidence OCR.
6. Preserve provenance by linking extracted outputs to page numbers.

# Output defaults

Prefer outputs in this order unless the user asked otherwise:

1. concise answer,
2. structured table,
3. JSON or CSV-ready rows,
4. file artifact when the extracted volume is large.

# Scripts

- `scripts/extract_tables.py`: attempts deterministic table extraction.
- `scripts/summarize_layout.py`: creates page-level layout metadata.

# References

Read these only when relevant:

- `references/table-extraction.md` for complex merged-cell or broken-border tables.
- `references/image-heavy-pdfs.md` for scan-heavy and low-text documents.

# Failure handling

If extraction quality is poor:

- report the failure mode,
- indicate which pages are affected,
- fall back to screenshot-based inspection,
- avoid fabricating cell values.
```

### 5.1 Why this template is structured this way

It deliberately separates:

- routing,
- activation criteria,
- operational sequence,
- defaults,
- helper paths,
- failure handling.

That structure improves both agent execution and human maintainability.

---

## 6. Description-writing rules

The `description` field should follow these rules.

### 6.1 Required properties
A good description should answer:

- what the skill does,
- when it should trigger,
- what concrete tasks or signals imply relevance,
- when it should not trigger if confusion is likely.

### 6.2 Strong pattern

```text
[Main action] when [task conditions]. Use this for [examples]. Do not use for [adjacent non-matching cases].
```

### 6.3 Bad example

```text
Helps with PDFs.
```

Why bad:

- too broad,
- no task conditions,
- no routing hints,
- no negative boundary.

### 6.4 Better example

```text
Extract structured data from PDFs when the task involves tables, scanned pages, layout-dependent interpretation, or conversion to CSV/JSON. Use this for screenshot-based page inspection and document-to-structured-data workflows. Do not use for plain-text summarization when the parsed PDF text is already clean.
```

### 6.5 Description lint rules
Flag a description if it:

- is under 12 words,
- has no action verb,
- has no “when” or equivalent condition phrase,
- lacks concrete examples,
- is obviously generic,
- duplicates another skill too closely,
- claims to handle a domain wider than the body actually supports.

---

## 7. Script design standard

Scripts should behave like tiny CLIs.

### 7.1 Requirements
Every script should:

- support `--help`,
- be non-interactive by default,
- exit non-zero on failure,
- print machine-parseable output when applicable,
- document inputs and outputs,
- avoid hidden state,
- avoid side effects unless explicitly intended.

### 7.2 Example helper script contract

```text
scripts/extract_tables.py \
  --input path/to/file.pdf \
  --pages 1,2,5 \
  --format json \
  --output out/tables.json
```

### 7.3 Example Python skeleton

```python
#!/usr/bin/env python3
import argparse
import json
import sys
from pathlib import Path


def main() -> int:
    parser = argparse.ArgumentParser(description="Extract tables from a PDF")
    parser.add_argument("--input", required=True)
    parser.add_argument("--pages", default="")
    parser.add_argument("--format", choices=["json", "csv"], default="json")
    parser.add_argument("--output", required=True)
    args = parser.parse_args()

    input_path = Path(args.input)
    if not input_path.exists():
        print(json.dumps({"error": f"input file not found: {input_path}"}))
        return 2

    result = {
        "input": str(input_path),
        "pages": args.pages,
        "format": args.format,
        "tables": [],
        "status": "not_implemented"
    }

    Path(args.output).write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({"status": "ok", "output": args.output}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
```

### 7.4 Promotion rule
If the same shell or Python logic appears in 3 or more successful traces, it becomes a candidate shared script.

---

## 8. Reference-file design standard

References are not dumping grounds. They should be purpose-labeled.

### 8.1 Good reference names

- `references/complex-taxonomy-mapping.md`
- `references/scanned-pdf-failure-modes.md`
- `references/repo-patch-strategy.md`

### 8.2 Bad reference names

- `references/notes.md`
- `references/misc.md`
- `references/stuff.md`

### 8.3 Reference instruction pattern
Inside `SKILL.md`, explicitly tell the agent when to read a reference.

Good:

```markdown
Read `references/scanned-pdf-failure-modes.md` only if page text extraction is sparse or obviously broken.
```

Bad:

```markdown
See references/ for more info.
```

---

## 9. Client overlay model

Use a portable core with client overlays generated or maintained separately.

### 9.1 Overlay directory rule

```text
skill-root/
└── overlays/
    ├── openai/
    ├── gemini/
    ├── copilot/
    └── opencode/
```

### 9.2 OpenAI Codex overlay example

`overlays/openai/openai.yaml`

```yaml
name: pdf-extraction
version: 1
summary: Layout-aware PDF data extraction and table recovery
invocation:
  policy: auto
  aliases:
    - pdf tables
    - extract pdf data
    - scanned pdf extraction
tools:
  required:
    - file_search
    - python
  optional:
    - python_user_visible
ui:
  category: Documents
  suggested_prompts:
    - Extract the tables from this PDF and return JSON.
    - Summarize this image-heavy PDF and note uncertain fields.
limits:
  max_reference_reads: 2
```

### 9.3 Gemini overlay example

`overlays/gemini/metadata.yaml`

```yaml
name: pdf-extraction
activation:
  requires_user_approval: true
  description: Allows the agent to load PDF extraction instructions and read bundled files in this skill directory.
permissions:
  read_paths:
    - references/
    - scripts/
precedence:
  scope: workspace
```

### 9.4 OpenCode overlay example

`overlays/opencode/permissions.yaml`

```yaml
skill: pdf-extraction
permissions:
  read:
    - ./references/**
    - ./scripts/**
  execute:
    - ./scripts/extract_tables.py
notes:
  - Compatible with .opencode/skills, .claude/skills, and .agents/skills layouts.
```

### 9.5 Copilot overlay example

`overlays/copilot/install-notes.md`

```markdown
Install locations:

- repository: `.github/skills/pdf-extraction/`
- user: `~/.config/github-copilot/skills/pdf-extraction/`

Use custom instructions for always-on repo guidance.
Use this skill only for layout-aware or extraction-heavy PDF tasks.
```

### 9.6 Overlay generation strategy
Do not hand-author every overlay if the differences are mostly mechanical. Use a generator:

```text
generate_overlays.py --skill skills/pdf-extraction
```

The generator should:

- read the core manifest,
- render product-specific templates,
- warn on unsupported fields,
- preserve hand-edited blocks where necessary.

---

## 10. Manifest template

Each skill should carry a portable manifest.

`manifest.yaml`

```yaml
schema_version: 1
skill_id: pdf-extraction
canonical_name: pdf-extraction
version: 0.1.0
owner: platform-team
status: stable
summary: Layout-aware PDF extraction and table recovery
risk_level: low
categories:
  - documents
  - extraction
compatibility:
  clients:
    - openai-codex
    - gemini-cli
    - opencode
    - github-copilot
requires:
  tools:
    - file_search
    - python
optional_tools:
  - python_user_visible
artifacts:
  - references/table-extraction.md
  - references/image-heavy-pdfs.md
  - scripts/extract_tables.py
  - scripts/summarize_layout.py
security:
  network_required: false
  executes_local_code: true
  writes_files: true
  destructive_actions: false
provenance:
  source_repo: https://example.com/org/agent-skills
  maintainer: platform-team@example.com
  signed: false
metrics:
  trigger_precision_target: 0.90
  trigger_recall_target: 0.85
  usefulness_target: 0.80
```

### 10.1 Why add a manifest
The core spec may not require a separate manifest, but operational systems benefit from one for:

- registry indexing,
- security review,
- packaging,
- overlay generation,
- versioning,
- compatibility reporting,
- SLO and KPI tracking.

---

## 11. Evaluation architecture

Evaluation should be split into three categories.

### 11.1 Trigger evaluation
Does the agent load the skill when it should?

### 11.2 Negative trigger evaluation
Does the agent avoid loading the skill when it should not?

### 11.3 Behavior evaluation
Once loaded, does the skill improve execution?

### 11.4 Eval file format
Use JSONL for easy batch runs.

#### `evals/trigger-positive.jsonl`

```json
{"id":"tp-001","prompt":"Extract the tables from this scanned PDF and return CSV-ready rows.","should_trigger":true}
{"id":"tp-002","prompt":"This PDF has broken page layout. Inspect screenshots and recover the key values.","should_trigger":true}
```

#### `evals/trigger-negative.jsonl`

```json
{"id":"tn-001","prompt":"Summarize this plain text article.","should_trigger":false}
{"id":"tn-002","prompt":"Explain what a PDF is.","should_trigger":false}
```

#### `evals/behavior.jsonl`

```json
{"id":"bh-001","prompt":"Extract the financial table on pages 3 and 4 and preserve page provenance.","assertions":["mentions page numbers","returns structured rows","does not fabricate missing cells"]}
{"id":"bh-002","prompt":"Identify why the text parser failed on the scanned pages and use an appropriate fallback.","assertions":["diagnoses failure mode","uses visual inspection or OCR fallback","states uncertainty explicitly"]}
```

### 11.5 Scoring rubric
Each evaluation run should produce:

- trigger decision,
- false-positive / false-negative count,
- output usefulness score,
- protocol compliance score,
- baseline delta,
- token and runtime cost.

### 11.6 Baseline modes
Compare against:

- no skill,
- previous skill version,
- competing skill version,
- hand-written control prompt.

### 11.7 Human review fields
Each behavior case should optionally support reviewer input:

```json
{
  "id": "bh-003",
  "human_review": {
    "score": 4,
    "notes": "Correct fallback choice, but uncertainty wording is too weak."
  }
}
```

---

## 12. Validator rules

A validator should fail or warn on the following.

### 12.1 Hard failures

- missing `SKILL.md`,
- invalid YAML frontmatter,
- missing `name` or `description`,
- directory name does not match canonical name when strict mode is on,
- referenced file does not exist,
- duplicate script names in same skill,
- malformed eval JSONL,
- invalid manifest schema,
- prohibited executable paths,
- unsafe symlink traversal.

### 12.2 Warnings

- description too generic,
- skill body too long,
- no negative eval cases,
- no baseline configuration,
- missing failure-handling section,
- script has no `--help`,
- references are present but never mentioned,
- multiple skills have overlapping descriptions,
- manifest risk level missing,
- no changelog,
- no owner metadata.

### 12.3 Example validator output

```text
[PASS] skills/pdf-extraction/SKILL.md exists
[PASS] YAML frontmatter parsed
[WARN] description may be too broad: "helps with PDFs"
[PASS] referenced file exists: references/table-extraction.md
[FAIL] missing eval file: evals/trigger-negative.jsonl
[WARN] script scripts/extract_tables.py does not expose --help
```

### 12.4 Validator command

```bash
python tooling/validate_skill.py skills/pdf-extraction --strict
```

---

## 13. Packaging model

A package step should produce one or more outputs depending on target client.

### 13.1 Packaging outputs

- portable folder bundle,
- `.skill` archive where relevant,
- generated overlay bundle,
- registry metadata entry,
- signed manifest if signing is enabled.

### 13.2 Package command

```bash
python tooling/package_skill.py skills/pdf-extraction --targets openai gemini opencode copilot
```

### 13.3 Packaging steps

1. validate skill,
2. normalize line endings and frontmatter,
3. verify referenced files,
4. run overlay generation,
5. optionally run eval smoke tests,
6. assemble bundle,
7. compute checksums,
8. write package manifest,
9. optionally sign.

---

## 14. Installation model

### 14.1 Installation scopes
Support:

- project-local,
- user-global,
- admin-managed,
- built-in system skills,
- remote registry installs.

### 14.2 Scope precedence
Recommended order:

1. project local,
2. user global,
3. admin managed,
4. built-in system.

If the same canonical name appears at multiple scopes:

- load the highest-priority trusted version,
- warn about shadowing,
- expose an override path for advanced users.

### 14.3 Installer responsibilities
The installer should:

- prevent path traversal,
- verify checksums when available,
- prompt or enforce trust policies for repo-local skills,
- preserve local edits unless overwrite is requested,
- support dry-run mode.

---

## 15. Security model

### 15.1 Threats
Potential threats include:

- malicious repo-level skills,
- deceptive descriptions that hijack unrelated tasks,
- scripts with unsafe filesystem or network behavior,
- overlay metadata that requests broader permissions than needed,
- poisoned references,
- conflicting shadowed skills.

### 15.2 Minimum safeguards

- trust gate for repo skills,
- path allowlist for bundled file reads,
- explicit execute permissions for scripts,
- manifest risk level,
- signature support for curated registries,
- install-time checksum verification,
- destructive-action flagging,
- audit log of activation and script execution.

### 15.3 Recommended manifest security fields

```yaml
security:
  network_required: false
  executes_local_code: true
  writes_files: true
  destructive_actions: false
  secrets_required: []
  privileged_paths: []
```

### 15.4 Trust policy example

```yaml
trust_policy:
  repo_skills: prompt
  signed_registry_skills: allow
  unsigned_remote_skills: deny
  local_user_skills: allow
```

---

## 16. Registry and discovery model

A registry should be treated as a convenience layer, not a trust guarantee.

### 16.1 Catalog fields

```json
{
  "skill_id": "pdf-extraction",
  "version": "0.1.0",
  "summary": "Layout-aware PDF extraction and table recovery",
  "owner": "platform-team",
  "risk_level": "low",
  "categories": ["documents", "extraction"],
  "compatibility": ["openai-codex", "gemini-cli", "opencode", "github-copilot"],
  "checksum": "sha256:...",
  "signed": false
}
```

### 16.2 Discovery ranking inputs
Rank skills using:

- description-query relevance,
- install count,
- activation success rate,
- false-positive rate,
- eval quality completeness,
- maintainer trust tier,
- freshness.

### 16.3 What not to do
Do not rank only by popularity. Popular low-quality skills can crowd out high-value niche skills.

---

## 17. Telemetry and metrics

Track the following.

### 17.1 Routing metrics

- activation rate,
- trigger precision,
- trigger recall,
- false-positive rate,
- shadow conflict rate.

### 17.2 Execution metrics

- completion rate,
- useful outcome score,
- script success rate,
- average references read,
- average token overhead.

### 17.3 Operational metrics

- validation pass rate,
- package success rate,
- time to first useful version,
- regression rate by release.

### 17.4 Human review metrics

- reviewer agreement,
- average quality score,
- frequent failure modes,
- most common trigger confusions.

---

## 18. Build pipeline

A reference CI/CD pipeline should look like this.

### 18.1 Pull request checks

1. schema validation,
2. skill lint,
3. path safety scan,
4. script smoke tests,
5. trigger eval smoke run,
6. changed-skill diff summary.

### 18.2 Main-branch checks

1. full eval suite,
2. baseline comparison,
3. packaging,
4. registry metadata update,
5. optional signing,
6. release notes generation.

### 18.3 Release artifacts

- packaged skill bundles,
- updated registry entries,
- eval report,
- diff-to-baseline report,
- checksums.

---

## 19. Porter/transpiler architecture

A serious multi-client setup should not manually maintain multiple parallel skill trees. Use a porter.

### 19.1 Canonical source
The canonical source of truth is the portable core plus manifest.

### 19.2 Porter responsibilities
The porter should:

- map discovery paths,
- render overlay files,
- convert metadata field names,
- omit unsupported features,
- generate install hints,
- flag ambiguous mappings.

### 19.3 Porter command examples

```bash
python tooling/generate_overlays.py skills/pdf-extraction --target openai
python tooling/generate_overlays.py skills/pdf-extraction --target gemini
python tooling/generate_overlays.py skills/pdf-extraction --all
```

### 19.4 Porter warning examples

```text
[WARN] target github-copilot does not support field: allowed-tools
[WARN] target gemini requires activation approval metadata; default value inserted
[WARN] target opencode permissions file generated with conservative defaults
```

---

## 20. Example end-to-end workflow

### 20.1 Authoring
1. create skill folder,
2. write `SKILL.md`,
3. add references and scripts only if they are justified,
4. create manifest,
5. write trigger-positive, trigger-negative, and behavior evals.

### 20.2 Validation
Run:

```bash
python tooling/validate_skill.py skills/pdf-extraction --strict
```

### 20.3 Evaluation
Run:

```bash
python tooling/run_evals.py skills/pdf-extraction --mode all
python tooling/score_evals.py skills/pdf-extraction/results/latest.json
```

### 20.4 Packaging
Run:

```bash
python tooling/package_skill.py skills/pdf-extraction --targets openai gemini opencode copilot
```

### 20.5 Installation
Run:

```bash
python tooling/install_skill.py dist/pdf-extraction-openai.skill --scope user
```

### 20.6 Release
Publish:

- package artifact,
- registry metadata,
- changelog,
- eval summary,
- compatibility matrix.

---

## 21. Recommended authoring standards

### 21.1 Naming
Use lowercase kebab-case directory and canonical names.

Good:

- `pdf-extraction`
- `repo-patch-planning`
- `release-note-generation`

Bad:

- `PDFSkill`
- `My Skill`
- `misc-helper`

### 21.2 Body structure
Recommended section order:

1. Purpose
2. When to use this skill
3. Do not use this skill when
4. Operating procedure
5. Output defaults
6. Scripts
7. References
8. Failure handling

### 21.3 Tone
Instructions should be:

- operational,
- concise,
- conditional where appropriate,
- specific about failure handling,
- explicit about uncertainty.

Avoid:

- vague platitudes,
- excessive ALL CAPS rules,
- excessive repetition,
- overclaiming scope.

---

## 22. Recommended quality gates

Do not mark a skill as stable until it meets all of the following.

- valid structure,
- at least 2 positive trigger cases,
- at least 2 negative trigger cases,
- at least 2 behavior cases,
- baseline comparison completed,
- no hard validator failures,
- named owner,
- manifest present,
- failure handling present,
- documented script contract for every executable helper.

For production use, add:

- signed package,
- audit logging,
- trust policy enforcement,
- release notes,
- compatibility verification against target clients.

---

## 23. Rollout phases

### Phase 1: manual authoring
Goal: establish conventions.

Deliverables:

- core layout,
- validator,
- simple packaging,
- small eval harness.

### Phase 2: standardized overlays
Goal: reduce per-client drift.

Deliverables:

- overlay generator,
- compatibility matrix,
- shadow conflict reporting.

### Phase 3: registry and trust model
Goal: controlled distribution.

Deliverables:

- catalog,
- checksums,
- signatures,
- trust policies.

### Phase 4: eval-first optimization
Goal: measurable quality.

Deliverables:

- baseline diffing,
- routing dashboards,
- trigger confusion reports,
- regression alerts.

### Phase 5: composition and orchestration
Goal: complex workflows.

Deliverables:

- skill composition graph,
- sub-skill recommendations,
- dependency-aware activation,
- workflow-level evals.

---

## 24. Strong opinions

### 24.1 A skill repo without evals is incomplete
It may still be usable, but it is not trustworthy.

### 24.2 A giant all-in-one skill is usually a design smell
Split by actual routing boundary, not by random topic granularity.

### 24.3 Client-specific fields should not pollute the core body
Put them in overlays or manifest-driven generation.

### 24.4 Skills should compete against baselines
Otherwise you are measuring effort, not quality.

### 24.5 Scripts should be promoted from repeated traces, not invented prematurely
Do not script speculative complexity that may never recur.

---

## 25. Minimal starter kit

If you want the leanest viable serious system, start with this.

```text
skills/
  <skill>/
    SKILL.md
    manifest.yaml
    evals/
      trigger-positive.jsonl
      trigger-negative.jsonl
      behavior.jsonl
tooling/
  validate_skill.py
  run_evals.py
  package_skill.py
```

This is enough to be materially better than ad hoc skill writing.

---

## 26. Final recommendation

The best practical architecture is:

- **one portable canonical skill source**,
- **manifest-backed overlays for each client**,
- **validator + packaging pipeline**,
- **trigger and behavior evals with baseline comparisons**,
- **explicit trust and provenance controls**,
- **clear separation between model judgment and scripted mechanics**.

That architecture is flexible enough for solo use, disciplined enough for team adoption, and structured enough to survive ecosystem churn.

If the field keeps evolving, the portable core and eval harness will remain the most durable assets.

