# Role
You are a senior software engineer operating in a coding CLI with repo access. Your task is to:
1) read a repository review,
2) verify each finding against the codebase,
3) produce a concrete implementation plan for all findings that are truly valid.

# Ground Rules
- Work **read-only**: search, read, and run existing tests; do not modify files unless asked later.
- Prefer **evidence over speculation**. If uncertain, say so and ask for the smallest missing detail.
- You are not coding in this task

# What to Do (deterministic steps)
A. **Normalize the review**
   - Parse REVIEW_SOURCE into atomic findings: {id, title, category (bug|security|perf|api|docs|style|build_ci, etc), evidence_from_review (quotes/links), any file/line hints}.

B. **Validate each finding against the repo**
   - Map each finding to code by exact path + line ranges when possible.
   - Use code search (strings/symbols/flags), read files, think about the content of the file.
   - Decide `validation_status ∈ {valid, invalid, uncertain, already solved, already logged}` and provide one short rationale.
   - Attach **validation_evidence**: code citations (path:start-end), config snippets, or ≤20-line test outputs.
   - Set `severity ∈ {critical, high, medium, low}` and list `blast_radius` (affected components).
   - Refer to standard Agent Skills conventions found on https://agentskills.io

C. **Plan implementations for valid findings only**
   - For each valid item, produce a **minimal-risk change plan**:
     - Change steps with exact files/functions and any config/schema changes.
     - Risks and a fast rollback path.
     - Effort estimate (T-shirt + hours).

D. **Call out invalid/uncertain**
   - Explain why, and list the smallest additional info needed to confirm.

Output as Markdown:

```markdown
# Review Validation & Implementation Plan

**Summary:** N findings — V valid, I invalid, U uncertain. Top risks: <themes>.

## F-001 <Title> — **Valid (Medium)**
**Why:** <1–2 sentence rationale>.
**Plan:** <numbered change steps>.
**Citations:** PATH/FILE:START-END; ...`
```

# Markdown rules

This is the first task of 9. For this task, you will create a plan in the projects root called implementation.md and record your findings.

For all subsequent tasks, you will append your findings to this file. You will NOT overwrite anything on this file.

# Post Review

Stage, commit, and push the review to GitHub

# Stopping Condition
If ambiguity blocks a verdict, stop after triage and emit `open_questions` specifying the minimal info needed.
``# Meta-Skill-Engineering — Architecture Diagrams



> All diagrams are Mermaid. Render in any Mermaid-compatible viewer (GitHub, VS Code, etc.).
> Issues from the [review report](../tasks/review-report-2026-03-20.md) are annotated with ⚠️ markers.

---

## 1. Repository Structure Overview

High-level view of every component and how they relate.

```mermaid
graph TB
    subgraph root["📁 Repository Root"]
        direction TB
        AGENTS["AGENTS.md<br/>Working rules & contracts"]
        README["README.md<br/>Overview & entry points"]
        PLAN["PLAN.md<br/>Remediation tracking"]
        CI["copilot-instructions.md<br/>Project-level Copilot config"]
    end

    subgraph skills["🧩 12 Active Skill Packages"]
        direction TB
        SC[skill-creator]
        SI[skill-improver]
        SE[skill-evaluation]
        STO[skill-trigger-optimization]
        STH[skill-testing-harness]
        SAP[skill-anti-patterns]
        SSR[skill-safety-review]
        SLM[skill-lifecycle-management]
        SCC[skill-catalog-curation]
        SVS[skill-variant-splitting]
        SA[skill-adaptation]
        SB[skill-benchmarking]
    end

    subgraph scripts_root["📜 Root Scripts (dev copies)"]
        direction TB
        RE[run-evals.sh]
        RFC[run-full-cycle.sh]
        RTO[run-trigger-optimization.sh]
        RBC[run-baseline-comparison.sh]
        RCE[run-corpus-eval.sh]
        RRS[run-regression-suite.sh]
        VS[validate-skills.sh]
        STS[sync-to-skills.sh]
        CSS[check_skill_structure.py]
        SL[skill_lint.py]
        CP[check_preservation.py]
        HF[harvest_failures.py]
        IEF[init_eval_files.py]
        QV["quick_validate.py<br/>⚠️ X-1: STALE — contradicts<br/>frontmatter rules"]
    end

    subgraph corpus["🧪 Test Corpus (5/5/5 + 3)"]
        direction LR
        CW["weak/ (5)<br/>bad-triggers, bloated-inline,<br/>missing-boundaries,<br/>no-output-contract, vague-procedure"]
        CS["strong/ (5)<br/>branching-procedure, failure-handling,<br/>rich-references, tight-routing,<br/>well-formed"]
        CAD["adversarial/ (5)<br/>circular-refs, contradictory-purpose,<br/>format-traps, injection, scope-explosion"]
        CR["regression/ (3)<br/>boundaries-deleted, purpose-lost,<br/>references-broken"]
    end

    subgraph evalresults["📊 eval-results/ (gitignored)"]
        direction LR
        ER1["skill-name-timestamp.md"]
        ER2["skill-name-eval.md (symlink)"]
        ER3["summary-timestamp.md"]
        ER4["summary-latest.md (symlink)"]
    end

    subgraph ext["🔌 Extension Tools"]
        direction TB
        VT["mse_validate_skill"]
        VA["mse_validate_all"]
        LT["mse_lint_skill"]
        PR["mse_check_preservation"]
        AV["Auto-validation hook<br/>(triggers on SKILL.md edit)"]
    end

    subgraph archive_dir["📦 archive/ (read-only)"]
        ARC["4 archived skills<br/>(distribution-era artifacts)"]
    end

    scripts_root -->|"sync-to-skills.sh<br/>distributes copies"| skills
    skills -->|"evals produce"| evalresults
    corpus -->|"test fixtures for"| scripts_root
    ext -->|"wraps"| scripts_root
    root -.->|"governs"| skills
```

---

## 2. Skill Cross-Reference Map

Every skill's "When NOT to use" and "Next steps" references. Arrows show routing handoffs.

```mermaid
graph LR
    SC([skill-creator])
    SI([skill-improver])
    SE([skill-evaluation])
    STO([skill-trigger-<br/>optimization])
    STH([skill-testing-<br/>harness])
    SAP([skill-anti-<br/>patterns])
    SSR([skill-safety-<br/>review])
    SLM([skill-lifecycle-<br/>management])
    SCC([skill-catalog-<br/>curation])
    SVS([skill-variant-<br/>splitting])
    SA([skill-adaptation])
    SB([skill-benchmarking])

    SC -->|"Next: build tests"| STH
    SC -->|"Next: evaluate"| SE
    SC -->|"Next: optimize triggers"| STO
    SC -->|"Next: safety audit"| SSR
    SC -->|"Next: manage lifecycle"| SLM

    SE -->|"If routing fails"| STO
    SE -->|"If output fails"| SI
    SE -->|"If comparing"| SB
    SE -->|"eval-results/*.md"| SI

    SI -->|"Verify improvement"| SE
    SI -->|"If routing changed"| STO

    STO -->|"Verify routing"| SE
    STO -->|"Build trigger tests"| STH
    STO -->|"If persistent issues"| SCC

    SSR -->|"If needs fixes"| SI

    STH -->|"Run tests"| SE
    STH -->|"Compare variants"| SB

    SAP -->|"Fix issues"| SI
    SAP -->|"If AP-1/11/12"| STO
    SAP -->|"If too broken"| SC

    SVS -->|"Update catalog"| SCC
    SVS -->|"Evaluate variants"| SE
    SVS -->|"Deprecate original"| SLM

    SCC -->|"Fix discoverability"| STO
    SCC -->|"Deprecate candidates"| SLM

    SA -->|"Verify adapted skill"| SE
    SA -->|"Update routing"| STO
    SA -->|"Safety if tools changed"| SSR

    SB -->|"Improve weaker"| SI
    SB -->|"Deprecate loser"| SLM

    SLM -->|"Before promoting"| SSR
    SLM -->|"Before promoting"| SE
```

---

## 3. Creation Pipeline

End-to-end flow for creating a new skill from scratch.

```mermaid
flowchart TD
    START(["🆕 User: 'Create a skill for X'"])

    subgraph phase1["Phase 1: Authoring"]
        SC["skill-creator<br/>━━━━━━━━━━━━━━━━<br/>Step 1: Define job sentence<br/>Step 2: Choose name<br/>Step 3: Write YAML frontmatter<br/>Step 4: Write sections<br/>Step 5: Calibrate depth<br/>Step 6: Manage size (< 500 lines)<br/>Step 7: Validate against mistakes"]
        SC_OUT["Output: SKILL.md + evals/ + scripts/"]
    end

    subgraph phase2["Phase 2: Test Infrastructure"]
        STH["skill-testing-harness<br/>━━━━━━━━━━━━━━━━<br/>Step 1: Analyze skill<br/>Step 2: Write positive triggers (8-10)<br/>Step 3: Write negative triggers (8)<br/>Step 4: Write behavior tests (3+)<br/>Step 5: Add usefulness criteria<br/>Step 6: Generate edge cases<br/>Step 7: Verify with --dry-run"]
        STH_OUT["Output: evals/ directory<br/>trigger-positive.jsonl<br/>trigger-negative.jsonl<br/>behavior.jsonl"]
    end

    subgraph phase3["Phase 3: Evaluation"]
        SE["skill-evaluation<br/>━━━━━━━━━━━━━━━━<br/>Step 0: Run automated suite<br/>Steps 1-6: Ad-hoc if no evals<br/>Routing accuracy + output quality<br/>+ baseline comparison"]
        SE_OUT["Output: eval-results/skill-eval.md<br/>+ Handoff section"]
    end

    subgraph phase4["Phase 4: Optimization (conditional)"]
        STO["skill-trigger-optimization<br/>━━━━━━━━━━━━━━━━<br/>60/40 train/test split<br/>Baseline → Propose → Validate<br/>ACCEPT/REJECT verdict"]
    end

    subgraph phase5["Phase 5: Safety"]
        SSR["skill-safety-review<br/>━━━━━━━━━━━━━━━━<br/>9-step audit:<br/>destructive ops, permissions,<br/>injection, scope, scripts"]
    end

    subgraph phase6["Phase 6: Promotion"]
        SLM["skill-lifecycle-management<br/>━━━━━━━━━━━━━━━━<br/>draft → beta → stable<br/>Promotion criteria check"]
    end

    START --> SC
    SC --> SC_OUT
    SC_OUT --> STH

    STH --> STH_OUT

    STH_OUT --> SE

    SE --> SE_OUT
    SE_OUT -->|"Routing fails"| STO
    SE_OUT -->|"All pass"| SSR

    STO -->|"Re-evaluate"| SE

    SSR -->|"Safe"| SLM
    SSR -->|"Issues found"| FIX["skill-improver<br/>(fix safety issues)"]
    FIX -->|"Re-audit"| SSR

    SLM --> DONE(["✅ Skill promoted to stable"])

    style SC fill:#e1f5fe
    style STH fill:#e8f5e9
    style SE fill:#fff3e0
    style STO fill:#fce4ec
    style SSR fill:#f3e5f5
    style SLM fill:#e0f2f1

    %% Issue annotations
    ISSUE1["⚠️ CR-1: Phase 3 of skill-creator<br/>overlaps with skill-testing-harness.<br/>Unclear: delegate or inline?"]
    SC -.-> ISSUE1

    ISSUE2["⚠️ D-2: 1024-char description limit<br/>not mentioned in Step 3"]
    SC -.-> ISSUE2

    style ISSUE1 fill:#fff3cd,stroke:#ffc107,color:#856404
    style ISSUE2 fill:#fff3cd,stroke:#ffc107,color:#856404
```

---

## 4. Improvement Pipeline

How an existing skill gets diagnosed, improved, and verified — with the eval-results handoff loop.

```mermaid
flowchart TD
    START(["🔧 User: 'Improve this skill'<br/>or skill fails evaluation"])

    subgraph eval["Evaluation (produces data)"]
        SE["skill-evaluation<br/>Run: ./scripts/run-evals.sh skill-name"]
        ER["eval-results/skill-name-eval.md<br/>━━━━━━━━━━━━━━━━<br/>• Gate verdicts (precision, recall, behavior)<br/>• Usefulness scores (if --usefulness)<br/>• Specific failing prompts<br/>• Handoff section"]
    end

    subgraph diagnose["Diagnosis (optional)"]
        SAP["skill-anti-patterns<br/>━━━━━━━━━━━━━━━━<br/>16 anti-patterns (AP-1 to AP-16)<br/>Severity: CRITICAL / HIGH / MEDIUM<br/>Quick scan priority guide"]
    end

    subgraph improve["Improvement"]
        SI_P1["skill-improver Phase 1<br/>━━━━━━━━━━━━━━━━<br/>Read eval-results/skill-eval.md<br/>Extract: gate status, failing prompts,<br/>precision/recall %, usefulness scores"]

        SI_P2["Phase 2: Diagnose Weakness<br/>━━━━━━━━━━━━━━━━<br/>Eval-driven diagnosis table:<br/>Precision < 80% → overtriggering<br/>Recall < 80% → undertriggering<br/>Behavior < 80% → wrong format<br/>Usefulness < 3/5 → weak output<br/>Structural < 8/10 → package rot"]

        SI_P2H["Heuristic fallback<br/>(if no eval data available)"]

        SI_P3["Phase 3-6: Implement & Self-Review"]
        SI_P7["Phase 7: Verify<br/>./scripts/run-baseline-comparison.sh<br/>5 quality gates"]
    end

    subgraph reopt["Re-optimization (conditional)"]
        STO["skill-trigger-optimization<br/>If routing changed during improvement"]
    end

    START --> SE
    SE -->|"Writes structured report"| ER
    ER -->|"Consumed by"| SI_P1

    START -->|"Optional diagnostic"| SAP
    SAP -->|"Findings feed into"| SI_P1

    SI_P1 --> SI_P2
    SI_P1 -->|"No eval data"| SI_P2H
    SI_P2H --> SI_P3

    SI_P2 --> SI_P3
    SI_P3 --> SI_P7

    SI_P7 -->|"Gates pass"| VERIFY["Re-evaluate → skill-evaluation"]
    SI_P7 -->|"Gates fail"| SI_P3

    SI_P3 -->|"Routing changed"| STO
    STO --> VERIFY

    VERIFY -->|"Loop until passing"| SE

    style SE fill:#fff3e0
    style SAP fill:#ffebee
    style SI_P1 fill:#e1f5fe
    style SI_P2 fill:#e1f5fe
    style SI_P3 fill:#e1f5fe
    style SI_P7 fill:#e1f5fe
    style STO fill:#fce4ec

    %% Issue annotations
    ISSUE1["⚠️ E-1: Only 4/12 skills have<br/>usefulness_criteria in behavior.jsonl.<br/>8 skills have structural-only testing."]
    ER -.-> ISSUE1

    ISSUE2["⚠️ IM-1: Diagnosis table over-maps<br/>usefulness < 3/5 to specific modes.<br/>Judge rationale is more diagnostic."]
    SI_P2 -.-> ISSUE2

    style ISSUE1 fill:#fff3cd,stroke:#ffc107,color:#856404
    style ISSUE2 fill:#fff3cd,stroke:#ffc107,color:#856404
```

---

## 5. Evaluation System Architecture

All scripts, their layers, inputs/outputs, and interdependencies.

```mermaid
flowchart TB
    subgraph inputs["📥 Inputs"]
        SKILL["SKILL.md<br/>(frontmatter + body)"]
        TP["evals/trigger-positive.jsonl<br/>(8-10 cases per skill)"]
        TN["evals/trigger-negative.jsonl<br/>(8 cases per skill)"]
        BJ["evals/behavior.jsonl<br/>(3-4 cases per skill)<br/>⚠️ E-1: Low count"]
    end

    subgraph structural["Layer 1: Structural (no LLM)"]
        VS["validate-skills.sh<br/>All 12 skills at once"]
        CSS["check_skill_structure.py<br/>10-point scorer, single skill"]
        SL["skill_lint.py<br/>Format linter"]
        QV["quick_validate.py<br/>⚠️ X-1: STALE<br/>Allows license, allowed-tools,<br/>metadata, compatibility<br/>in frontmatter"]
    end

    subgraph trigger["Layer 2: Trigger Testing (LLM)"]
        RE_T["run-evals.sh (trigger mode)<br/>━━━━━━━━━━━━━━━━<br/>--observe: JSON routing detection<br/>--strict: differential (2x slower)<br/>--runs N: majority voting"]
    end

    subgraph behavior["Layer 3: Behavior Testing (LLM)"]
        RE_B["run-evals.sh (behavior mode)<br/>━━━━━━━━━━━━━━━━<br/>required_patterns: regex match<br/>forbidden_patterns: regex reject<br/>min_output_lines: length check<br/>expected_sections: heading check"]
    end

    subgraph usefulness["Layer 4: Usefulness Scoring (LLM)"]
        RE_U["run-evals.sh --usefulness<br/>━━━━━━━━━━━━━━━━<br/>LLM-as-Judge, 4 dimensions:<br/>• Correctness (1-5)<br/>• Completeness (1-5)<br/>• Actionability (1-5)<br/>• Conciseness (1-5)<br/>Per-case rubrics via usefulness_criteria"]
    end

    subgraph gates["🚦 5 Quality Gates"]
        G1["Gate 1: Trigger Precision ≥ 80%"]
        G2["Gate 2: Trigger Recall ≥ 80%"]
        G3["Gate 3: Behavior Pass Rate ≥ 80%"]
        G4["Gate 4: Structural Validity = true"]
        G5["Gate 5: Usefulness ≥ 3/5 (opt-in)"]
    end

    subgraph outputs["📤 Outputs"]
        REPORT["eval-results/skill-timestamp.md"]
        LINK["eval-results/skill-eval.md (symlink)"]
        HANDOFF["Handoff section:<br/>primary_failure, failing_cases,<br/>recommended_next_skill"]
    end

    SKILL --> structural
    SKILL --> trigger
    SKILL --> behavior
    SKILL --> usefulness

    TP --> RE_T
    TN --> RE_T
    BJ --> RE_B
    BJ --> RE_U

    RE_T --> G1
    RE_T --> G2
    RE_B --> G3
    CSS --> G4
    RE_U --> G5

    G1 --> REPORT
    G2 --> REPORT
    G3 --> REPORT
    G4 --> REPORT
    G5 --> REPORT
    REPORT --> LINK
    REPORT --> HANDOFF

    style QV fill:#f8d7da,stroke:#dc3545,color:#721c24
    style BJ fill:#fff3cd,stroke:#ffc107
    style RE_U fill:#d4edda,stroke:#28a745

    ISSUE_E2["⚠️ E-2: No token/duration tracking<br/>anywhere in the eval system"]
    RE_T -.-> ISSUE_E2
    style ISSUE_E2 fill:#fff3cd,stroke:#ffc107,color:#856404
```

---

## 6. Full Evaluation Cycle (`run-full-cycle.sh`)

The 5-step orchestrated evaluation with failure harvesting.

```mermaid
flowchart TD
    START(["./scripts/run-full-cycle.sh"])

    S1["Step 1: Structural Validation<br/>━━━━━━━━━━━━━━━━<br/>validate-skills.sh<br/>Checks all 12 skills"]

    S2["Step 2: Trigger & Behavior Evals<br/>━━━━━━━━━━━━━━━━<br/>run-evals.sh --all<br/>(+ --usefulness if enabled)<br/>Produces per-skill reports"]

    S3["Step 3: Corpus Evaluation<br/>━━━━━━━━━━━━━━━━<br/>run-corpus-eval.sh<br/>Tests skill-improver &<br/>skill-anti-patterns against<br/>corpus/weak + strong + adversarial"]

    S4["Step 4: Regression Suite<br/>━━━━━━━━━━━━━━━━<br/>run-regression-suite.sh<br/>Runs corpus/regression/*.json<br/>(3 cases: preservation failures)"]

    S45["Step 4.5: Harvest Failures<br/>━━━━━━━━━━━━━━━━<br/>harvest_failures.py<br/>Scans eval reports for ❌<br/>Creates new regression cases"]

    S5["Step 5: Aggregate Report<br/>━━━━━━━━━━━━━━━━<br/>eval-results/summary-timestamp.md<br/>eval-results/summary-latest.md"]

    START --> S1
    S1 -->|"Pass"| S2
    S1 -->|"Fail"| FAIL1(["❌ Structural issues found"])

    S2 -->|"Per-skill reports written"| S3
    S3 -->|"Corpus reports written"| S4
    S4 -->|"Regression results"| S45

    S45 -->|"New failures → corpus/regression/"| CR["corpus/regression/<br/>new-failure-NNN.json"]
    S45 --> S5

    S5 --> DONE(["📊 Summary report generated"])

    subgraph loop["♻️ Regression Feedback Loop"]
        CR -->|"Next cycle picks up<br/>new regression cases"| S4
    end

    style S1 fill:#e8eaf6
    style S2 fill:#fff3e0
    style S3 fill:#fce4ec
    style S4 fill:#f3e5f5
    style S45 fill:#e0f7fa
    style S5 fill:#e8f5e9

    ISSUE_E3["⚠️ E-3: No iteration workspace.<br/>Results are flat timestamped files.<br/>Cannot diff iteration N vs N+1."]
    S5 -.-> ISSUE_E3
    style ISSUE_E3 fill:#fff3cd,stroke:#ffc107,color:#856404
```

---

## 7. Script Distribution Model

How root scripts get synchronized to per-skill `scripts/` directories.

```mermaid
flowchart LR
    subgraph root["📜 Root scripts/ (source of truth)"]
        RE[run-evals.sh]
        RBC[run-baseline-comparison.sh]
        CSS[check_skill_structure.py]
        SL[skill_lint.py]
        CP[check_preservation.py]
        HF[harvest_failures.py]
        IEF[init_eval_files.py]
        VS[validate-skills.sh]
        RTO[run-trigger-optimization.sh]
    end

    STS["sync-to-skills.sh<br/>━━━━━━━━━━━━━━━━<br/>Manifest-driven sync<br/>Modes: sync | --dry-run | --check<br/>Idempotent (skips unchanged)"]

    subgraph targets["📁 Per-skill scripts/ directories"]
        SE_S["skill-evaluation/scripts/<br/>• run-evals.sh<br/>• check_skill_structure.py<br/>• harvest_failures.py"]
        SB_S["skill-benchmarking/scripts/<br/>• run-evals.sh<br/>• run-baseline-comparison.sh<br/>• check_skill_structure.py"]
        SC_S["skill-creator/scripts/<br/>• run-evals.sh<br/>• check_skill_structure.py<br/>• validate-skills.sh<br/>• init_eval_files.py"]
        STH_S["skill-testing-harness/scripts/<br/>• run-evals.sh<br/>• init_eval_files.py"]
        SI_S["skill-improver/scripts/<br/>• run-baseline-comparison.sh<br/>• check_preservation.py"]
        SAP_S["skill-anti-patterns/scripts/<br/>• check_skill_structure.py<br/>• skill_lint.py"]
        SSR_S["skill-safety-review/scripts/<br/>• check_skill_structure.py<br/>• validate-skills.sh<br/>• skill_lint.py"]
        STO_S["skill-trigger-optimization/scripts/<br/>• run-trigger-optimization.sh"]
    end

    root --> STS
    STS --> targets

    ISSUE_SC2["⚠️ SC-2: Manifest is single<br/>source of truth. No reverse check<br/>if a SKILL.md references a script<br/>not in the manifest."]
    STS -.-> ISSUE_SC2
    style ISSUE_SC2 fill:#fff3cd,stroke:#ffc107,color:#856404
```

---

## 8. Corpus Testing Architecture

How the test corpus is used by meta-skill evaluation.

```mermaid
flowchart TD
    subgraph corpus["🧪 Test Corpus"]
        direction TB
        WEAK["corpus/weak/ (5 skills)<br/>━━━━━━━━━━━━━━━━<br/>bad-triggers<br/>bloated-inline<br/>missing-boundaries<br/>no-output-contract<br/>vague-procedure"]

        STRONG["corpus/strong/ (5 skills)<br/>━━━━━━━━━━━━━━━━<br/>branching-procedure<br/>comprehensive-failure-handling<br/>rich-references<br/>tight-routing<br/>well-formed"]

        ADV["corpus/adversarial/ (5 skills)<br/>━━━━━━━━━━━━━━━━<br/>circular-references<br/>contradictory-purpose<br/>format-traps<br/>injection-attempt<br/>scope-explosion"]

        REG["corpus/regression/ (3 cases)<br/>━━━━━━━━━━━━━━━━<br/>boundaries-deleted-001.json<br/>purpose-lost-001.json<br/>references-broken-001.json"]
    end

    RCE["run-corpus-eval.sh<br/>━━━━━━━━━━━━━━━━<br/>Layer 1: Structural pre-scores<br/>(check_skill_structure.py)<br/><br/>Layer 2: Meta-skill application<br/>(run meta-skill on corpus skill,<br/>compare before/after)"]

    RRS["run-regression-suite.sh<br/>━━━━━━━━━━━━━━━━<br/>Reads corpus/regression/*.json<br/>Runs preservation checks<br/>Verifies fixes stay fixed"]

    HF["harvest_failures.py<br/>━━━━━━━━━━━━━━━━<br/>Extracts ❌ from eval reports<br/>Creates new regression cases"]

    subgraph tested["Tested Meta-Skills"]
        SI_TEST["skill-improver<br/>(Can it improve weak skills?<br/>Does it preserve strong skills?<br/>Does it handle adversarial?)"]
        SAP_TEST["skill-anti-patterns<br/>(Does it detect issues in weak?<br/>Does it pass strong?<br/>Does it resist adversarial?)"]
    end

    WEAK --> RCE
    STRONG --> RCE
    ADV --> RCE
    RCE --> SI_TEST
    RCE --> SAP_TEST

    REG --> RRS

    SI_TEST -->|"Failures"| HF
    SAP_TEST -->|"Failures"| HF
    HF -->|"New cases"| REG

    style WEAK fill:#ffcdd2
    style STRONG fill:#c8e6c9
    style ADV fill:#ffe0b2
    style REG fill:#e1bee7
```

---

## 9. Library Management Pipeline

How the skill catalog is maintained, curated, and governed.

```mermaid
flowchart TD
    START(["📚 'Audit the library'<br/>or catalog maintenance needed"])

    SCC["skill-catalog-curation<br/>━━━━━━━━━━━━━━━━<br/>1. Inventory all skills<br/>2. Detect duplicates & overlaps<br/>3. Check naming conventions<br/>4. Identify discoverability gaps<br/>5. Recommend merges/splits<br/>6. Prioritize actions"]

    SCC_OUT["Curation Report<br/>━━━━━━━━━━━━━━━━<br/>• Duplicate pairs<br/>• Gap analysis<br/>• Merge recommendations<br/>• Split candidates<br/>• Deprecation candidates"]

    STO_FIX["skill-trigger-optimization<br/>(fix discoverability issues)"]

    SVS["skill-variant-splitting<br/>(split overly broad skills)"]

    SLM["skill-lifecycle-management<br/>━━━━━━━━━━━━━━━━<br/>Promote: draft → beta → stable<br/>Deprecate: add notice to SKILL.md<br/>Archive: move to archive/"]

    SE_CHECK["skill-evaluation + skill-safety-review<br/>(required before promotion to stable)"]

    ARCHIVE["archive/<br/>(read-only storage)"]

    START --> SCC
    SCC --> SCC_OUT

    SCC_OUT -->|"Discoverability issues"| STO_FIX
    SCC_OUT -->|"Overly broad skills"| SVS
    SCC_OUT -->|"Deprecation candidates"| SLM
    SCC_OUT -->|"Merge recommendations"| MERGE["Manual merge<br/>(combine into one skill)"]

    SLM -->|"Before promoting"| SE_CHECK
    SE_CHECK -->|"Pass"| SLM
    SLM -->|"Archive"| ARCHIVE

    SVS -->|"Evaluate each variant"| SE_CHECK
    SVS -->|"Deprecate original"| SLM

    style SCC fill:#e8eaf6
    style SLM fill:#e0f2f1
    style SVS fill:#fff9c4

    ISSUE_LM1["⚠️ LM-1: Lifecycle states<br/>(draft/beta/stable) have no<br/>tracking mechanism.<br/>Only deprecation is formalized."]
    SLM -.-> ISSUE_LM1
    style ISSUE_LM1 fill:#fff3cd,stroke:#ffc107,color:#856404
```

---

## 10. Extension Tooling & Auto-Validation

How the Copilot CLI extension integrates with the repo.

```mermaid
flowchart TD
    subgraph agent["🤖 Copilot CLI Agent"]
        EDIT["Agent edits a SKILL.md<br/>(edit or create tool)"]
        MANUAL["Agent calls tool manually"]
    end

    subgraph hook["Auto-Validation Hook"]
        DETECT["onPostToolUse detects<br/>file ending in SKILL.md"]
        AUTO["Auto-runs check_skill_structure.py"]
        INJECT["Injects validation result<br/>into agent context"]
    end

    subgraph tools["Extension Tools"]
        T1["mse_validate_skill<br/>→ check_skill_structure.py<br/>(single skill, 10-point score)"]
        T2["mse_validate_all<br/>→ validate-skills.sh<br/>(all 12 skills)"]
        T3["mse_lint_skill<br/>→ skill_lint.py<br/>(format issues)"]
        T4["mse_check_preservation<br/>→ check_preservation.py<br/>(Jaccard similarity)"]
    end

    subgraph scripts_used["Python Scripts"]
        CSS[check_skill_structure.py]
        VS[validate-skills.sh]
        SL[skill_lint.py]
        CP[check_preservation.py]
    end

    EDIT --> DETECT
    DETECT --> AUTO
    AUTO --> INJECT
    INJECT -->|"Score + warnings<br/>visible to agent"| agent

    MANUAL --> T1
    MANUAL --> T2
    MANUAL --> T3
    MANUAL --> T4

    T1 --> CSS
    T2 --> VS
    T3 --> SL
    T4 --> CP

    style DETECT fill:#e8f5e9
    style INJECT fill:#e8f5e9
```

---

## 11. Known Issues Map

All issues from the review report, mapped to the components they affect.

```mermaid
graph TB
    subgraph critical["🔴 CRITICAL"]
        X1["X-1: quick_validate.py<br/>allows 4 extra frontmatter fields<br/>(license, allowed-tools, metadata,<br/>compatibility) that all other<br/>tools reject.<br/>📍 scripts/quick_validate.py:42"]
    end

    subgraph significant["🟡 SIGNIFICANT"]
        E1["E-1: Usefulness criteria<br/>only in 4/12 skills<br/>(creator, evaluation,<br/>improver, trigger-opt)<br/>📍 evals/behavior.jsonl"]
        E2["E-2: No token/duration<br/>tracking in eval system<br/>📍 scripts/run-evals.sh"]
        E3["E-3: No iteration workspace<br/>(flat timestamped files)<br/>📍 eval-results/"]
        D2["D-2: 1024-char description<br/>limit not enforced<br/>📍 skill-creator, check_skill_structure.py"]
        CR1["CR-1: skill-creator Phase 3<br/>overlaps testing-harness<br/>📍 skill-creator/SKILL.md:180-197"]
    end

    subgraph moderate["🟠 MODERATE"]
        S1["S-1: skill-creator has few<br/>reference files for 328 lines"]
        S2["S-2: Generic 'see references/'<br/>instead of conditional loading"]
        SC1["SC-1: validate-skills.sh and<br/>run-regression-suite.sh<br/>lack --help flags"]
        EV1["EV-1: Manual SKILL.md removal<br/>for baseline in skill-evaluation"]
        LM1["LM-1: No tracking for<br/>draft/beta/stable states"]
    end

    subgraph minor["⚪ MINOR"]
        B1["B-1: Explanatory text<br/>agent already knows"]
        SR1["SR-1: Longest description<br/>(502 chars)"]
        TH1["TH-1: expected_files not<br/>in canonical eval contract"]
        SC2["SC-2: Sync manifest<br/>no reverse-check"]
        IM1["IM-1: Diagnosis table<br/>over-maps usefulness"]
    end

    style critical fill:#f8d7da,stroke:#dc3545,color:#721c24
    style significant fill:#fff3cd,stroke:#ffc107,color:#856404
    style moderate fill:#ffe0b2,stroke:#ff9800,color:#663d00
    style minor fill:#f5f5f5,stroke:#9e9e9e,color:#424242
```

---

## 12. Skill Package Anatomy

What a complete skill package looks like internally.

```mermaid
graph TB
    subgraph pkg["📦 skill-name/"]
        direction TB

        SKILL["SKILL.md<br/>━━━━━━━━━━━━━━━━<br/>YAML frontmatter (name + description)<br/>─────────────────<br/># Purpose<br/># When to use<br/># When NOT to use<br/># Procedure<br/>  ## Step 1..N<br/># Output contract<br/># Failure handling<br/># Next steps<br/># References (optional)"]

        subgraph evals_dir["evals/"]
            TP["trigger-positive.jsonl<br/>{prompt, expected:'trigger',<br/>category, notes}"]
            TN["trigger-negative.jsonl<br/>{prompt, expected:'no_trigger',<br/>category, notes}"]
            BJ["behavior.jsonl<br/>{prompt, expected_sections,<br/>required_patterns,<br/>forbidden_patterns,<br/>min_output_lines,<br/>usefulness_criteria (opt)}"]
        end

        subgraph scripts_dir["scripts/ (8/12 skills)"]
            SCRIPTS["Copies synced from root<br/>via sync-to-skills.sh"]
        end

        subgraph refs_dir["references/ (2/12 skills)"]
            REFS["Extended reference material<br/>loaded on demand"]
        end
    end

    DISCOVERY["Agent Discovery<br/>━━━━━━━━━━━━━━━━<br/>Level 1: name + description<br/>(always in context, ~100 words)"]

    ACTIVATION["Skill Activation<br/>━━━━━━━━━━━━━━━━<br/>Level 2: Full SKILL.md body<br/>(loaded when skill triggers)"]

    ON_DEMAND["On-Demand Resources<br/>━━━━━━━━━━━━━━━━<br/>Level 3: references/, scripts/<br/>(loaded by agent as needed)"]

    SKILL -->|"Frontmatter"| DISCOVERY
    SKILL -->|"Body"| ACTIVATION
    refs_dir --> ON_DEMAND
    scripts_dir --> ON_DEMAND

    style SKILL fill:#e3f2fd
    style evals_dir fill:#e8f5e9
    style scripts_dir fill:#fff3e0
    style refs_dir fill:#fce4ec
```

---

*Diagrams generated from repository state at commit `0a6b902`. Issues reference [review-report-2026-03-20.md](../tasks/review-report-2026-03-20.md).*
