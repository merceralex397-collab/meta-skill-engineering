# Subagent 1 Report - Evidence Audit

## Verdict

Most of the Task 2 report's hard factual scaffolding is sound: the repo-owned package count is 26, the 22/4 split is real, and its strongest schema and packaging-role comparisons are supported by the package contracts and scripts. The main material factual flaw is its provenance analysis: `provenance-audit` is not merely read-heavy or read-only, because it explicitly tells the agent to write provenance back into `SKILL.md`. Several merge recommendations may still be reasonable, but the report states them more definitively than the repository evidence alone supports.

## Verified Claims

- **Claim:** The repo-owned inventory is 26 skill packages split into 22 root packages and 4 packages under `skill creator/`.
  **Status:** Verified.
  **Evidence:** `README.md` states the 26/22/4 inventory explicitly, and the on-disk package set matches that split with `SKILL.md` present in each repo-owned package listed there; `AGENTS.md` also sets the active inventory boundary at 26 repo-owned top-level skill packages.

- **Claim:** `skill-authoring` is a narrow greenfield authoring skill, while `skill-improver` is for improving an existing package.
  **Status:** Verified.
  **Evidence:** `skill creator/skill-authoring/SKILL.md` is explicitly for creating a new `SKILL.md` from scratch and names `skill-improver` as the alternative for existing skills. `skill-improver/SKILL.md` is explicitly for improving an existing skill package and defines surgical edit, structural refactor, and package upgrade modes.

- **Claim:** `anthropiuc-skill-creator` is broader than `skill-authoring` and `skill-improver`, covering drafting, evaluation, iterative improvement, benchmarking, description optimization, and packaging.
  **Status:** Verified.
  **Evidence:** `skill creator/anthropiuc-skill-creator/SKILL.md` covers drafting, evals, reviewer generation, benchmarking, description optimization, and packaging. Supporting automation exists in `skill creator/anthropiuc-skill-creator/scripts/run_eval.py`, `skill creator/anthropiuc-skill-creator/scripts/run_loop.py`, `skill creator/anthropiuc-skill-creator/scripts/aggregate_benchmark.py`, and `skill creator/anthropiuc-skill-creator/scripts/package_skill.py`.

- **Claim:** `microsoft-skill-creator` is a real specialization backed by Microsoft-specific research workflows and templates.
  **Status:** Verified.
  **Evidence:** `skill creator/microsoft-skill-creator/SKILL.md` is explicitly scoped to Microsoft technologies and Learn MCP / `mslearn`. `skill creator/microsoft-skill-creator/references/skill-templates.md` provides Microsoft-specific skill templates and CLI fallback guidance.

- **Claim:** `community-skill-harvester` is an acquisition/adoption workflow rather than a normal greenfield creation workflow.
  **Status:** Verified.
  **Evidence:** `skill creator/community-skill-harvester/SKILL.md` centers on searching public sources, license checks, scoring external skills, pattern extraction, and import proposals. That is materially different from creating a new skill from scratch.

- **Claim:** The evaluation stack has a real schema mismatch: `skill-testing-harness` produces JSONL eval files while `skill-eval-runner` expects YAML files, and `skill-eval-runner`'s own manifest points back to JSONL artifacts.
  **Status:** Verified.
  **Evidence:** `skill-testing-harness/SKILL.md` specifies `evals/trigger-positive.jsonl`, `evals/trigger-negative.jsonl`, and `evals/output-tests.jsonl`. `skill-eval-runner/SKILL.md` looks for `evals/triggers.yaml`, `evals/outputs.yaml`, and `evals/baselines.yaml`. `skill-eval-runner/manifest.yaml` instead lists `evals/trigger-positive.jsonl`, `evals/trigger-negative.jsonl`, and `evals/behavior.jsonl`.

- **Claim:** `skill-improver` reinforces the repo's JSONL-centered eval pattern.
  **Status:** Verified.
  **Evidence:** `skill-improver/scripts/init_eval_files.py` bootstraps `evals/trigger-positive.jsonl`, `evals/trigger-negative.jsonl`, and `evals/behavior.jsonl`, not the YAML suite described by `skill-eval-runner/SKILL.md`.

- **Claim:** The packaging family is differentiated mainly by scope: `skill-packaging` is single-skill packaging, `skill-packager` is broader release orchestration, and `overlay-generator` is the per-client overlay conversion layer.
  **Status:** Verified.
  **Evidence:** `skill-packaging/SKILL.md` packages one finished skill folder with manifest, checksums, optional overlays, archive creation, and verification. `skill-packager/SKILL.md` scans skills, validates many skills, generates overlays, produces per-skill and combined bundles, and writes release notes. `overlay-generator/SKILL.md` exists specifically to derive client-specific overlays from a canonical `SKILL.md`.

- **Claim:** `skill-installer`'s documented scope is broader than what its scripts currently implement.
  **Status:** Verified.
  **Evidence:** `skill-installer/SKILL.md` promises installs from GitHub, local folders, and archives. The actual scripts are `skill-installer/scripts/install-skill-from-github.py`, `skill-installer/scripts/list-skills.py`, and `skill-installer/scripts/github_utils.py`; there is no companion script for local-folder or archive installs.

## Problems

- **Severity:** High
  **Issue:** Disproven characterization of `provenance-audit` as essentially read-heavy/read-only.
  **Why it is wrong or weak:** The report's provenance section depends on an audit-vs-record distinction that the package does not currently honor. `provenance-audit` explicitly tells the agent to add a provenance section to `SKILL.md`, so it is not a read-only auditor. That makes the report's later "audit vs mutation handoff" framing inaccurate on the current repository evidence.
  **Evidence:** `provenance-audit/SKILL.md`, `skill-provenance/SKILL.md`

- **Severity:** Medium
  **Issue:** Partly true but overstated claim that the provenance pair differs mainly by output mode.
  **Why it is wrong or weak:** The two provenance skills overlap, but they differ by more than "report vs recorded metadata." `provenance-audit` focuses on origin type, source verification, license compatibility, and trust assignment; `skill-provenance` adds authorship, evidence basis, encoded assumptions, a frontmatter patch, and `PROVENANCE.md`. The overlap is real, but the summary compresses materially distinct workflow content.
  **Evidence:** `provenance-audit/SKILL.md`, `skill-provenance/SKILL.md`

- **Severity:** Medium
  **Issue:** Unsupported certainty in the phrase "four clear duplication or near-duplication problems."
  **Why it is wrong or weak:** The repository shows overlapping contracts and some direct schema collisions, but it does not provide user-routing telemetry, invocation history, or comparative eval results showing that these packages actually "compete for the same job" in practice. For some pairs the evidence supports "overlap cluster worth consolidation review" more strongly than "clear duplication problem."
  **Evidence:** `skill-description-optimizer/SKILL.md`, `skill-trigger-optimization/SKILL.md`, `skill-evaluation/SKILL.md`, `skill-eval-runner/SKILL.md`, `skill-packaging/SKILL.md`, `skill-packager/SKILL.md`

- **Severity:** Low
  **Issue:** Unsupported process claim that every `SKILL.md` and all materially relevant support files were read.
  **Why it is wrong or weak:** That may be true, but it is not verifiable from repository evidence. In an evidence audit, that statement should be treated as author attestation, not as a repository-backed fact.
  **Evidence:** `tasks/Task 2 - Condense Report.md`

- **Severity:** Low
  **Issue:** The installer mismatch is real but the report understates how broad it is.
  **Why it is wrong or weak:** The report correctly notes that the scripts mostly cover GitHub install/list flows, but it misses a stronger contradiction: the client install paths documented in `SKILL.md` do not match the defaults implemented in the scripts. That omission does not make the report false, but it leaves stronger repo evidence unused.
  **Evidence:** `skill-installer/SKILL.md`, `skill-installer/scripts/github_utils.py`

## Better Alternatives

- Replace the provenance summary with: "`provenance-audit` and `skill-provenance` both inspect provenance and both can modify package artifacts; the former centers on source/license/trust verification, while the latter centers on durable provenance recording via frontmatter and `PROVENANCE.md`."

- Replace "four clear duplication or near-duplication problems" with "four overlap clusters worth consolidation review" unless comparative routing evidence or usage data is added.

- Tighten the installer observation to: "`skill-installer/SKILL.md` promises GitHub, local-folder, and archive installs, but the shipped scripts only implement GitHub install/list flows, and their default client paths diverge from the paths documented in the skill contract."

- Keep the inventory claim and the review-process claim separate. The package counts are repository-verifiable; the claimed reading depth is not.

## Confidence

High on counts, package roles, path checks, and file-format mismatches because those are directly verifiable from `SKILL.md`, manifests, and scripts. Medium on the overlap/merge conclusions because those are partly interpretive and the repository does not contain real routing-confusion or usage data.
