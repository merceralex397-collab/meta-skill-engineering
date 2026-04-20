# Subagent 2 Report - Merge Skeptic Review

## Verdict

Task 2 correctly spots real routing confusion, but it overstates how much of that confusion requires mergers. The only clearly justified merge is folding `skill-description-optimizer` into `skill-trigger-optimization`. The provenance and evaluation clusters look more like adjacent stages with blurry handoffs than true duplicates. The packaging cluster is mostly a primitive-wrapper-transform stack, not a single collapsed capability.

## Recommendations That Hold Up

### Merge `skill-description-optimizer` into `skill-trigger-optimization`

This one is well supported. Both packages diagnose undertriggering and overtriggering, extract discriminating signals, compare sibling skills, and validate with positive and negative prompts. A user asking "why is this skill not firing?" or "fix the routing" could plausibly land in either package. The narrower package is effectively a description-only mode of the broader one.

### Keep `skill-packaging` and `skill-packager` separate, but make the wrapper relationship explicit

Task 2 is right that users can confuse these two because both mention manifests, checksums, overlays, and bundles. But the current scope split is real: `skill-packaging` is a one-skill bundler with archive verification, while `skill-packager` is a release orchestrator that scans many skills, emits combined bundles, and writes release notes. The recommendation holds up only as a boundary clarification: `skill-packager` should delegate per-skill packaging rather than compete with it.

### Clarify `skill-lifecycle-management` vs `skill-deprecation-manager`

Task 2 is right not to merge these. Routing confusion is plausible because both mention deprecation, but the operational scopes differ: `skill-lifecycle-management` decides state transitions across the library, while `skill-deprecation-manager` executes one retirement with notices, archive movement, and registry updates.

### Clarify `skill-catalog-curation` vs `skill-registry-manager`

This also holds up as a boundary fix, not a merge. Both scan the whole library, so users could confuse them. But `skill-catalog-curation` produces an audit and recommendations about overlap, discoverability, and taxonomy drift, while `skill-registry-manager` mutates `skills-lock.json` and `CATALOG.md`.

## Recommendations That Overreach

### Merge `provenance-audit` and `skill-provenance`

This is too aggressive. The preserved boundary is read-only assessment versus durable package recording.

`provenance-audit` is framed as an audit of a skill or artifact origin chain, license state, source verification, and trust assignment. Its natural output is a trust/compliance report, and its target can be broader than a skill package. `skill-provenance` is narrower but deeper: it writes a `metadata.provenance` patch and a `PROVENANCE.md`, and it records authorship, evidence basis, and encoded assumptions inside the package.

The overlap is real because `provenance-audit` currently strays into mutating the skill by telling the user to add a provenance section. That is a boundary bug, not proof that the packages should be merged.

### Merge `skill-eval-runner` into `skill-evaluation`

This also overreaches. The preserved boundary is suite execution versus evaluative judgment.

`skill-eval-runner` assumes an existing eval suite and runs it. That makes it the natural CI or regression entry point. `skill-evaluation` can proceed even when no evals exist, creates minimum ad hoc cases when needed, interprets inconclusive results, and decides the next action for a single skill. Those are not the same job even if both report precision, recall, and baseline value.

Task 2 is correct that the eval artifacts are inconsistent: `skill-testing-harness` and `skill-improver/scripts/init_eval_files.py` produce JSONL-style files, while `skill-eval-runner` names YAML suite files and its own `manifest.yaml` points back to JSONL artifacts. But schema mismatch is an integration defect, not by itself a reason to collapse the execution layer into the judgment layer.

### Absorb `overlay-generator` into the packaging family

This is only partially justified. The preserved boundary is overlay-only conversion and validation versus archive production.

`overlay-generator` has a legitimate standalone trigger surface when the canonical `SKILL.md` changed and the user needs client overlays regenerated or consistency-checked without cutting a release archive. Packaging consumes overlays, but it does not automatically subsume the client-metadata projection problem. If anything, `overlay-generator` should be narrowed to overlay-only work rather than absorbed by default.

## Boundary Fixes Better Than Merges

### Provenance chain

Make `provenance-audit` strictly read-only: audit origin, license, source verification, modifications, and trust for a skill or artifact, then emit a report.

Make `skill-provenance` strictly write-focused: consume audit findings or equivalent evidence, then write `metadata.provenance` and `PROVENANCE.md` for a skill package. That preserves the assessment-to-record handoff instead of collapsing both into one package.

### Evaluation chain

Make `skill-testing-harness` own the canonical eval artifact schema.

Make `skill-eval-runner` execute existing eval artifacts only and emit raw regression metrics.

Make `skill-evaluation` consume runner output when available, or assemble minimum ad hoc cases when no suite exists, then issue the single-skill readiness verdict and remediation handoff. That is a clean build-run-judge sequence.

### Packaging chain

Keep `overlay-generator` for overlay regeneration, conversion, and consistency validation.

Keep `skill-packaging` for one-skill archive production and verification.

Keep `skill-packager` for many-skill release orchestration, combined indexes, and release notes. It should call into the single-skill packaging flow rather than replicate it.

## Confidence

Medium-high. I reviewed the Task 2 report, `README.md`, `AGENTS.md`, and directly inspected the main overlap candidates plus supporting files, including `skill-eval-runner/manifest.yaml`, `skill-improver/scripts/init_eval_files.py`, and `skill-packager/manifest.yaml`. This is a contract-level taxonomy review; I did not have usage telemetry or live routing data.
