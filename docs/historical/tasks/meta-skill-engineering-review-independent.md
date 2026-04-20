# Meta-Skill-Engineering: Independent Review

**Basis for this review:** My own knowledge of LLM behaviour, instruction-following research, prompt engineering, agent system design, and the externally-installed `skill-creator` SKILL.md (at `/mnt/skills/examples/skill-creator/SKILL.md`) as an independent reference point. I have deliberately not used any rubric, checklist, or standard from the uploaded skill set itself — including `skill-anti-patterns`, `skill-quality-rubric.md`, or `skill-type-playbooks.md` — because using those to evaluate themselves defeats the point.

**Evaluation framework (independent):**

1. **Trigger fidelity** — Does the description match how real users actually phrase requests? Will a language model router reliably fire/not-fire this skill on realistic inputs?
2. **Procedural atomicity** — Are steps singular, imperative, and independently executable? Can an agent complete them without exercising judgment to fill gaps?
3. **Output determinism** — Would two separate agents following this skill produce structurally identical output? Or is there room for drift?
4. **Failure coverage** — Are the most common real-world failure modes explicitly handled, with specific recovery actions?
5. **Context efficiency** — Does the skill's length justify its token cost? Is important content front-loaded?
6. **Self-consistency** — Does the skill demonstrate the qualities it teaches or requires?
7. **Ecosystem navigability** — Can a user navigate the full skill set without hitting dead ends or ambiguous routing?

---

## Comparison with External Reference

The installed `skill-creator` represents a meaningfully different design philosophy than the uploaded set. Key differences worth noting before the individual reviews:

**skill-creator is conversational; the uploaded set is procedural.** skill-creator uses natural prose, says "Cool? Cool.", allows the user to "vibe" without evals, and explicitly calibrates jargon to user sophistication. The uploaded set uses numbered steps, output contract templates, and severity ratings throughout. Neither is wrong — but they serve different users. The uploaded set assumes a technically literate author working in a structured organisation. skill-creator assumes a wide range of users.

**skill-creator treats the workflow as flexible; the uploaded set treats it as fixed.** skill-creator says "figure out where the user is in this process and jump in." The uploaded set specifies phases to follow "in order unless the user clearly wants a lighter pass." The uploaded set will produce more consistent, auditable output. It may frustrate users who want to skip steps.

**skill-creator bakes in user communication guidance; the uploaded set does not.** None of the 16 uploaded skills address how to communicate with the user during execution — tone, jargon calibration, when to ask vs proceed. This is a genuine gap for meta-skills, which are often used by non-expert authors who may need guidance, not just procedure.

These differences matter most for the hub skills (`skill-authoring`, `skill-improver`) where user interaction is heaviest.

---

## Individual Reviews

---

### 1. `skill-adaptation`

**What it does:** Rewrite a skill's environment-specific references for a new context while preserving its procedure.

#### Trigger fidelity
The description works. "Port this skill to Python/Vue/pnpm", "localize this skill for a different environment" are realistic user phrases. The negative routing (don't use for scratch authoring, for improving without context change, for splitting) is specific and distinguishes adjacent skills.

One gap: the description says "preserving the core pattern" but never states what it *produces*. A routing model deciding whether to invoke this skill needs to know the output type. "Producing a rewritten SKILL.md ready to install" should be in the description.

#### Procedural atomicity
Steps 1–4 are good. The invariants-vs-adaptation-points distinction (Steps 3–4) is the skill's best idea — it gives the agent a concrete decision rule rather than asking for judgment. Step 6 (validate) is the weakest: "walk through each procedure step mentally with a real task" asks the agent to simulate execution, which is unreliable. LLMs are poor at mental simulation of multi-step procedures. This step should be a concrete checklist — "Does every tool name in the adapted skill exist in the target environment? Yes/No" — not a request for imagined walkthrough.

#### Output determinism
The Adaptation Summary template is specific and producible. The adapted SKILL.md has no format constraint beyond "the full rewritten file" — since the source skill has a format, this is acceptable.

#### Failure coverage
Four cases covered. "Target context unclear: Stop and ask" is correct agent behaviour. "Adaptation would break core logic: Recommend skill-authoring" is the right escalation. Thin but proportionate to the skill's scope.

#### Issues
- **Medium:** "Walk through each procedure step mentally" in Step 6 is asking an agent to do something unreliable. Replace with a deterministic checklist.
- **Medium:** Output type missing from description.
- **Low:** No guidance on what to do when the source skill uses scripts/ or references/ — does the agent adapt those too, or only SKILL.md? The skill is ambiguous on this.

---

### 2. `skill-anti-patterns`

**What it does:** Check a skill against 12 named anti-patterns and produce a findings report.

#### Trigger fidelity
Reasonable. "Check for anti-patterns", "what's wrong with this skill", "audit before promotion" are plausible user phrases. The post-failure diagnostic trigger is smart — when a skill misbehaves, this is a natural reach.

#### Procedural atomicity
This is the skill's real strength. Each of the 12 patterns has: a name, a severity, a concrete detection signal (not "look for vagueness" but "description says 'Use when task clearly involves X'"), a before example, an after example, and a specific fix. This is exactly how LLM procedural instructions should work. No step requires the agent to exercise vague judgment.

#### Completeness of the anti-pattern list itself
Here's where independent review matters. The 12 patterns are real and important — but they are not complete. From what I know about LLM instruction failure modes, several common ones are missing:

**Missing: Instruction length/complexity anti-pattern.** Skills that are too long degrade instruction following. Research on LLM context use shows that attention is not uniform — instructions buried in the middle of long documents are followed less reliably than those at the beginning or end. A skill can be structurally clean by all 12 criteria and still fail because it's 450 lines long with critical steps on line 380. There is no anti-pattern for this.

**Missing: Implicit agent capability assumption.** Several skills in this set ask agents to "run evals", "spawn subagents", or "open a browser" — capabilities that may not exist in the target client. There's no anti-pattern for "procedure step assumes capabilities the agent may not have." This is particularly relevant for the meta-skills that reference scripts and subagents.

**Missing: Few-shot starvation.** The most reliable way to get LLMs to produce consistent output is to show them an example. Many skills describe output format in prose ("produce a table with columns X, Y, Z") without a concrete filled example. LLMs given a template with real values outperform those given only format descriptions. There is no anti-pattern for "output format described but not exemplified."

**Missing: Hallucination surface.** Vague success criteria ("ensure quality", "verify correctness") invite the agent to invent a definition of success and then confirm it. There's no anti-pattern specifically targeting the hallucination risk created by unmeasurable quality requirements — AP-6 comes close but frames it as an "eval" problem rather than a "model reliability" problem.

#### Output determinism
The output table template is specific and reproducible. "Clean Bill of Health" escape hatch is appropriate.

#### Issues
- **High:** The anti-pattern list is incomplete in ways that matter for LLM reliability. Missing patterns: instruction overload, capability assumption, few-shot starvation, hallucination surface.
- **Medium:** The Quick Scan priority (AP-1, AP-7, AP-12 first) is good triage logic, but the priority isn't explained. Why those three? An author using this skill doesn't know whether to trust the triage order or customise it.
- **Low:** Severity labels (CRITICAL, HIGH, MEDIUM, LOW) are never defined in terms of observable impact. "CRITICAL — causes routing failure" is good but ad hoc. A severity rubric would make the labels consistent and trustworthy.

---

### 3. `skill-authoring`

**What it does:** Write a new SKILL.md from scratch.

#### Trigger fidelity
Good. The description gives concrete trigger phrases and distinguishes clearly from improving, adapting, or installing. The negative boundaries are specific enough to route correctly.

#### Procedural atomicity
Steps 1–6 are well-ordered and mostly atomic. The "Common Authoring Mistakes" sidebar is the standout — five concrete failure modes with diagnostic symptoms and fixes. This is good LLM instruction design: instead of "don't do X", it says "if you see symptom Y, you have problem X, fix it with Z."

Step 5 (validate against anti-patterns) breaks atomicity — it asks the agent to check against a list that lives in a separate skill. The reference is correct but the instruction "check the completed skill for: circular triggers, boilerplate steps, generic output defaults..." is a truncated summary of what skill-anti-patterns actually covers. Either reference the external skill properly with a specific instruction ("run skill-anti-patterns on the completed SKILL.md") or expand the check list inline.

The description-first principle ("write description FIRST — it defines the scope everything else must serve") is the most important insight in the skill. It's correct and well-justified.

#### Output determinism
States what to deliver (SKILL.md + conditional subdirectories). The SKILL.md structure is well-specified. The subdirectory creation conditions ("only if they contain actual files") are clear.

#### Critical gap: No post-authoring workflow
The skill treats authoring as its own terminal step. In reality, authoring is the start of a pipeline: write → test → evaluate → promote. The installed `skill-creator` embeds testing and evaluation *inside* the creation loop. `skill-authoring` in the uploaded set produces a SKILL.md and stops. An author following this skill has no indication that validation is needed before use.

This is not a minor omission. An untested skill is an unknown-quality skill. The skill set contains `skill-testing-harness` and `skill-evaluation` specifically for this purpose — but `skill-authoring` never mentions them.

#### Issues
- **High:** No handoff to testing pipeline after delivery. The output contract should conclude: "Before use, set up trigger tests with `skill-testing-harness` and validate with `skill-evaluation`."
- **Medium:** Step 5 anti-pattern check is a truncated, unattributed summary of a separate skill. Either delegate properly or inline fully.
- **Medium:** No guidance on how to calibrate instruction length. The "under 500 lines" target is mentioned but there's no principle for *why* content belongs inline vs extracted. The installed `skill-creator` has a clearer heuristic: "load order" (always → on trigger → on demand) as the structuring principle.
- **Low:** The frontmatter fields table marks `license` and `compatibility.clients` as "Recommended." Every skill in the set includes them. The inconsistency between stated convention and actual practice will cause authors to omit them.

---

### 4. `skill-benchmarking`

**What it does:** Compare two or more skill variants on the same test cases to pick a winner.

#### Trigger fidelity
Good. "Which is better?", "did the refinement help?", "benchmark these variants" are user-natural phrases. The negative routing to `skill-evaluation` (single skill) and `skill-testing-harness` (building tests) is correct and specific.

#### Procedural atomicity
Strong. The significance thresholds (>5pp pass rate, >10% tokens) convert subjective "is this better?" into objective gates. The blind comparison instruction and round-robin method for >2 variants are both well-specified and producible.

The win-rate scoring rubric (correctness, completeness, conciseness, actionability) is reasonable but has a gap: **it doesn't specify how to weight these dimensions relative to each other.** If variant A scores higher on correctness and lower on conciseness, which wins? The tiebreaker ("favor shorter/cheaper") addresses ties but not trade-offs. A real comparison will often involve trade-offs, not ties.

#### Output determinism
The output template is specific: Summary table → Breakdown → Significance → Recommendation. This will produce consistent results across agents.

#### Issues
- **Medium:** Win-rate rubric lacks trade-off weighting. How should correctness vs conciseness trade-offs be resolved? Add: "Weight correctness above all other dimensions. A correct but verbose output beats an incorrect but concise one."
- **Medium:** "Do NOT use when: Quick spot-check, not systematic comparison" names no alternative. This is a routing dead end — the user is told not to use this skill but not what to do instead.
- **Low:** N≥10 minimum sample is stated without justification. Statistical reliability depends on the expected effect size. For a routing comparison (binary trigger/no-trigger), 10 may be adequate. For nuanced output quality comparison, 10 is likely too low. The skill should acknowledge this: "N≥10 is the practical minimum; for subtle quality differences, aim for N≥20."

---

### 5. `skill-catalog-curation`

**What it does:** Audit a skill library for duplicates, overlaps, and discoverability gaps.

#### Trigger fidelity
Works. Three realistic trigger phrases. Negative routing is specific and correct. Minor issue: the description uses "Use when: …" and "Do not use for: …" with literal colon-phrase formatting, where every other skill in the set uses integrated prose. This is inconsistent — a routing model may parse the format differently from the rest of the set.

#### Procedural atomicity
Six steps are logically ordered. The action-signature extraction method for duplicate detection (verb+object from description, group synonyms, compare overlap) is a concrete algorithm that an agent can execute without judgment. This is good design.

The 50% trigger-phrase overlap threshold for flagging duplicates is a heuristic without a stated basis. Where does 50% come from? If two skills share 3 of 5 trigger phrases, are they duplicates? The threshold is workable but arbitrary. A better anchor: "Flag for manual review; do not automatically recommend merge."

The discoverability check (Step 4) is good. But Step 5 (deprecation by zero invocations) assumes usage metrics exist and are accessible. No guidance on where they live or how to get them. In most real deployments, usage metrics are not easily accessible to an agent. This step will silently fail for most users.

#### Output determinism
Strong. Six mandatory sections even when empty. Specific template. Prioritized actions with explicit labels.

#### Issues
- **Medium:** Description formatting is inconsistent with the rest of the set.
- **Medium:** Deprecation-by-zero-invocations criterion requires usage metrics that are rarely accessible. Either remove or add a specific fallback.
- **Low:** The 50% overlap threshold for duplicate detection has no stated basis or confidence calibration.

---

### 6. `skill-evaluation`

**What it does:** Produce quantitative evidence that a single skill triggers correctly and improves output over a no-skill baseline.

#### Trigger fidelity
Good. "Is this skill working?", "validate before promoting", "does this skill still add value?" are direct and realistic. The negative routing is specific and correct.

#### Procedural atomicity
Steps 1–6 are well-ordered. The test case construction guidance is the strongest in the set — breaking cases into exact-match, paraphrase, indirect, and multi-step categories is both theoretically grounded and practically useful. The anti-pattern warning ("don't name the skill in test prompts — real users don't") reflects genuine understanding of how routing works.

The precision (≥95%) and recall (≥90%) thresholds are stated without justification. These are reasonable starting points but treating them as hard pass/fail lines is aggressive. A skill with 94% precision isn't categorically different from one with 96% — the difference is 1 case in 20. The skill should acknowledge: "These are targets, not bright lines; use judgment when results are near the boundary."

**The baseline comparison step is operationally underspecified.** Steps 4–5 require running the skill "with the skill active" vs "without the skill." How to deactivate a skill is never explained. In different clients this works differently (remove SKILL.md from path, disable in config, etc.). An agent following this skill literally cannot complete Step 5 without already knowing platform-specific mechanics. This is a significant procedural gap.

#### Output determinism
The output format is specific: routing accuracy table, output quality score, baseline comparison, verdict, next action. Consistent and reproducible.

#### Issues
- **High:** Baseline deactivation procedure is completely absent. Step 5 cannot be executed without external knowledge. Add: "Baseline = run the same prompt with this skill's SKILL.md temporarily removed from the client skill directory."
- **Medium:** Precision/recall thresholds are stated as hard pass/fail but are inherently probabilistic. Add a note about boundary cases.
- **Low:** "Does this skill still add value?" as a trigger phrase could fire this skill for an evaluation of a skill that was never in use — the word "still" implies prior use but the skill's procedure doesn't verify this. Minor ambiguity.

---

### 7. `skill-improver`

**What it does:** Improve an existing skill across routing, procedure, support layers, and maintainability.

This is the most important skill in the set and shows both the highest ambition and the most structural problems.

#### Trigger fidelity
Good. "Improve this skill", "this skill is weak/vague/bloated", "harden this SKILL.md" are realistic. The four negative boundaries are specific and prevent routing confusion with adjacent skills.

#### Procedural atomicity
The Mode selection guide is the skill's strongest element — the "30% of file changes" threshold converts a judgment call into a measurable criterion, which is exactly how agent decision rules should work. The Phase 2 failure modes table (11 categories, each mapped to a fix target) is comprehensive.

**Phases 4–5 have an atomicity problem.** Phase 4 ("Rewrite with restraint") and Phase 5 ("Add support layers only if justified") both use the word "prefer" and "only if" — these are agent-judgment invitations, not executable decisions. Compare Phase 3, which gives concrete priority ordering (1. Clarify purpose, 2. Tighten routing, etc.) — that's executable. Phase 4 gives principles, not steps. An agent executing Phase 4 has to decide on its own what "restraint" means in context.

**The output contract does not exist as a section.** Every other skill in the set has a clearly labelled `# Output contract` or `# Output defaults` section. `skill-improver` embeds its output description inside Phase 7 of the workflow: "Return: the improved files, a short rationale for each major change, 2–5 recommended eval prompts for the improved skill, risks or open questions." This means:
- An agent looking for output expectations will not find them reliably
- A human skimming the skill won't see the output specification in a consistent location
- The Phase 7 description is also weaker than peer skills — "short rationale" and "2–5 eval prompts" are not specified formats, unlike the table templates used elsewhere

#### Failure handling
Two paragraphs covering "skill too incomplete to improve" and "user wants a quick pass." This is the thinnest failure handling in the set. The most complex skill has the least failure coverage. Real failure modes for a skill improver include: scope change requested (not just improvement), contradictory requirements (tighten routing AND broaden scope), missing referenced support files, the skill having no discernible core purpose to preserve. None of these are addressed.

#### Self-consistency problem
`skill-improver` teaches "add output contract" as a core improvement action (Phase 3, point 5). It does not have an output contract section itself. This is the most significant self-consistency failure in the set. If an agent ran `skill-improver` on `skill-improver`, it would recommend adding an output contract — to itself.

#### Issues
- **Critical:** The skill that teaches output contracts doesn't have one. Extract the Phase 7 description into a dedicated `# Output contract` section.
- **High:** Failure handling covers 2 of an estimated 5–7 common failure modes. Add: scope change requested, contradictory requirements, missing support files.
- **High:** Phases 4–5 contain principles, not executable steps. Convert "prefer concrete decisions" into "write X, then check Y, then decide Z."
- **Medium:** Phase 7 "recommended eval prompts" are not defined as a format. Are they plain text prompts? JSONL? Labeled by category? Specify.

---

### 8. `skill-installer`

**What it does:** Install a skill package from GitHub, local folder, or archive.

#### Trigger fidelity
Works well. "Install this skill", "add skill from GitHub", "list available skills" are direct user phrases. Negative boundaries are appropriate.

#### Procedural atomicity
This is the most operationally complete skill in the set. Each pathway (GitHub, local, list) has numbered steps. Script options are documented with flag names. Failure conditions have specific actions with named fallbacks (zip download → git sparse checkout).

The safety section is notably thorough for a non-safety skill. Pre-install verification, script scanning, post-install verification, and overwrite protection are all specified. The instruction "scan each script file for: shell commands with sudo, network calls to non-origin URLs, file operations outside the skill directory" is exactly concrete enough for an agent to execute.

#### Output determinism
The output template is the most specific in the set: "Installed: [name], Location: [path], Source: [url], Verified: [checklist], Restart message." An agent following this produces nearly identical output every time.

#### Issues
- **Medium:** The installer's script scanning and `skill-safety-review` both scan for dangerous operations, with no defined relationship between them. A user doesn't know whether the installer check is sufficient or a precursor. This needs one sentence: either "this check replaces a full safety review for trusted sources" or "this check is a prerequisite only — run `skill-safety-review` for full coverage."
- **Low:** `scripts/list-skills.py` requires `--repo` to specify a GitHub repository, but the trigger "list available skills" doesn't imply repo context. An agent invoking this on the trigger phrase alone will hit an error with no recovery path.
- **Low:** The client path table may become stale. No versioning or last-verified date.

---

### 9. `skill-lifecycle-management`

**What it does:** Move skills through formal lifecycle states (draft → beta → stable → deprecated → archived).

#### Trigger fidelity
**AP-11 from first principles:** The description starts with three verbs — "Promote, deprecate, and track." LLM routing is influenced by the first phrase of a description, as it carries the most signal weight. Three co-equal verbs dilute this. A router model reading "Promote, deprecate, and track" has no primary signal to latch onto. Rewrite with a single primary action: "Manage skill lifecycle states..."

Otherwise the trigger phrases are realistic: auditing production-readiness, promoting after evaluation, retiring a superseded skill.

#### Procedural atomicity
Generally good. The promotion criteria (draft → beta: 3 diverse prompts; beta → stable: formal evaluation, 10 cases, ≥90% pass, 2 real projects) are the most specific in the set and represent a genuine quality gate.

One criterion is unverifiable: "Used in at least 2 real projects or sessions without reported failure." Agents cannot verify this. They can't query usage history, and "without reported failure" requires negative evidence. This criterion will either be ignored or fabricated. It should be removed or replaced with something checkable: "Has an open review issue been filed against this skill in the past 30 days? If no tracking system is available, skip this criterion."

#### Output determinism
Specific four-section report. The Dependency Impact table (which references must be updated when deprecating) is uniquely useful and not duplicated elsewhere in the set. ✓

#### Issues
- **Medium:** Description starts with three co-equal verbs; first-phrase signal is diluted.
- **Medium:** "Used in 2 real projects without reported failure" is unverifiable by an agent. Remove or provide a fallback.
- **Low:** The output contract's "Actions (ordered)" section doesn't specify the ordering principle.

---

### 10. `skill-packaging`

**What it does:** Bundle a finished skill into a versioned distributable archive with manifest and checksums.

#### Trigger fidelity
Good. "Package this skill", "bundle for distribution", "prepare a versioned release" are realistic. Negative boundaries are specific.

#### Procedural atomicity
Strong operational skill. The manifest construction (Step 2) with per-file SHA-256 checksums, explicit file enumeration (no wildcards), and semver versioning is specific and complete. Required vs optional fields with "stop and report if required field missing" is exactly right.

The semver guidance (MAJOR/MINOR/PATCH mapped to skill content changes) is the only versioning specification in the entire skill set and is genuinely valuable. It's also correct — MAJOR for breaking changes to procedure or output format, MINOR for additive changes, PATCH for wording fixes.

Step 3 (Client overlays) is materially underspecified. "Create `overlays/<client>.yaml` containing only the delta" — what does a delta look like? What fields can be overridden? What format is the overlay file? Without a concrete example, an agent executing Step 3 has to invent the format, producing inconsistent results.

#### Output determinism
Two deliverables: archive + verification report. Report template is specific with file count and checksum status. The "Ready to install: skill-installer add ..." next-step hint is useful.

#### Issues
- **Medium:** Step 3 (overlays) lacks a concrete example. An agent cannot produce a correct overlay without knowing its format.
- **Low:** No guidance when skill exceeds 100KB warning threshold. Add a specific recovery action (e.g., extract references before packaging).

---

### 11. `skill-provenance`

**What it does:** Document a skill's origin, authorship, evidence basis, and encoded assumptions.

#### Trigger fidelity
The trigger phrases work: "where did this skill come from?", "document this skill's origin", "audit skill trustworthiness." However, these are rare user phrases. Most users don't ask about provenance until a trust problem has already occurred. The skill would benefit from automatic trigger conditions: "Also fire when a skill is imported via `skill-installer` from an external source" — making provenance recording a default step in the import workflow rather than a reactive one.

#### Procedural atomicity
The concrete investigation procedures are the skill's strength — specific git commands (`git log --diff-filter=A -- <skill-path>`), URL scanning from body text, terminology detection as evidence signals. These are executable and don't require agent judgment.

The trust level assessment (Step 5) is subjective. "Known author" is undefined. "Peer-reviewed" is undefined. "Actively maintained" is undefined. An agent assigning trust levels using these criteria will produce inconsistent results across runs. Each level needs a specific, checkable criterion: e.g., "High: git log shows named committer with ≥5 commits across ≥2 months, and a passing eval exists."

#### Output determinism
Two specific artifacts: frontmatter YAML patch + PROVENANCE.md. Both have templates. The "skip PROVENANCE.md if trivial" escape hatch is appropriate and correctly scoped.

#### Issues
- **Medium:** Trust level criteria are subjective. Need concrete thresholds for each level.
- **Medium:** No automatic trigger in import or promotion workflows. The skill is opt-in where it should often be mandatory.
- **Low:** `origin: created` is ambiguous YAML — does it mean "created from scratch" or is "created" a keyword? Template should use a full phrase: `origin: "created from scratch — [motivation]"`.

---

### 12. `skill-reference-extraction`

**What it does:** Move large reference material from a bloated SKILL.md into a references/ directory.

#### Trigger fidelity
Good. Size-based triggers ("exceeds 500 lines or 10KB") are clear objective thresholds. User-phrase triggers ("this skill is too long", "slim it down") are realistic. Negative routing correctly identifies cases where improvement is needed but extraction isn't the tool (under 200 lines, or core procedure masquerading as reference).

#### Procedural atomicity
Strong. The inline-vs-extract classification heuristic ("consulted on every invocation → keep; consulted for specific sub-cases → extract") is a reliable decision rule. The 20-line size threshold for individual blocks is concrete.

Step 7's reference link quality check — requiring both WHEN (what condition triggers the lookup) and WHAT (what the reference contains) in every pointer — is the best single piece of guidance in the entire skill set. It directly prevents the most common failure mode of reference extraction: pointers that say "see references/X.md" with no context for why or when.

**Self-contradiction in Step 5:** The step shows `Full schema: see references/schema.json` as the pointer format. Step 7 explicitly labels this pattern as bad and provides a better format. An agent executing Step 5 before reading Step 7 will implement the wrong pattern. The bad example should be removed from Step 5 or the good format should appear in Step 5 directly.

#### Output determinism
Summary table with before/after line counts, percentage reduction, and verification checklist. Specific and reproducible.

#### Issues
- **Medium:** Steps 5 and 7 contradict each other. Bad example in Step 5 is corrected in Step 7, but an agent working top-to-bottom may implement the wrong pattern first.
- **Low:** "Two skills wanting the same reference → create skill-specific copies" is safe but doesn't acknowledge the shared-spec case (external canonical schema maintained by a third party).

---

### 13. `skill-safety-review`

**What it does:** Audit a skill for safety hazards before publishing or importing.

#### Trigger fidelity
Good. Multiple realistic trigger phrases covering both proactive ("is this safe to publish?") and reactive ("check for destructive operations") scenarios. The "after importing from untrusted source" trigger is exactly right — this is when safety review is most critical and most commonly skipped.

#### Procedural atomicity
Seven of eight steps are well-specified with concrete detection signals. The risk tier system (T1/T2/T3) with mandatory confirmation gates for T1 operations is the right design — it converts a judgment call ("is this risky enough to require confirmation?") into a classification system.

**Step 4 (Prompt injection) is materially underspecified.** The other seven steps each have: a detection target, a specific scan method, and a classification or action. Step 4 says "identify paths where untrusted external content flows into instruction context without sanitization." That's a description of the problem, not a procedure for finding it. An agent executing Step 4 literally doesn't know what to look for. It should match the specification depth of the other steps: "Scan procedure steps for any that read external content (file contents, API responses, user-pasted text). For each, check whether that content is interpolated directly into an instruction or prompt. If yes: flag as injection vector."

#### Output determinism
Specific five-category table. "Omit sections with zero findings" keeps output clean. ✓

**The skill lacks a "partial failure" section.** It mentions partial-failure safety in Step 7 but doesn't specify how to *document* findings about missing rollback paths in the output template. The output format has no section for "Partial Failure Safety" even though Step 7 audits for it.

#### Issues
- **High:** Step 4 (injection) is description-level, not procedure-level. Needs concrete detection instructions.
- **Medium:** Output template doesn't include a section for partial-failure safety findings from Step 7.
- **Medium:** No defined relationship with `skill-installer`'s overlapping safety scan.

---

### 14. `skill-testing-harness`

**What it does:** Build trigger tests (JSONL) and output format tests for a skill's evals/ directory.

#### Trigger fidelity
Good. Five trigger conditions including the concrete "skill lacks an evals/ directory" — an objective, checkable condition that an agent or user can observe directly.

#### Procedural atomicity
The test case category system (exact-match, paraphrase, indirect, multi-step for positives; anti-match, near-miss, similar vocabulary, overly broad for negatives) is grounded in real understanding of how LLM routing works. Paraphrase and indirect cases test routing robustness beyond keyword matching. This is theoretically sound.

The 60%/30%/10% distribution rule (positive/negative/edge) is a reasonable default. The hedge-word detection criterion (>30% of output sentences using "consider/may/could") is immediately checkable.

**The description promises "baseline comparisons" as an output.** The output contract delivers `trigger-positive.jsonl`, `trigger-negative.jsonl`, `output-tests.jsonl`, and `evals/README.md`. There is no baseline comparison file anywhere in the procedure or output contract. This is a false promise in the description — one of the three named outputs doesn't exist. This will cause agents to either invent a file format for baseline comparisons or produce an incomplete output without flagging the gap.

#### Output determinism
The JSONL schemas (with required/optional fields) are specific and reproducible. The README template is complete.

#### Issues
- **Critical:** "Baseline comparisons" in the description has no corresponding artifact in the output contract. Either add `baseline-cases.jsonl` to the procedure and output, or remove "baseline comparisons" from the description.
- **Medium:** Failure handling says "if the skill is too narrow, merge via `skill-variant-splitting`." `skill-variant-splitting` is for splitting skills, not merging them. The correct referral for merging is `skill-catalog-curation`.
- **Low:** The `min_cases` field in output-tests JSONL is a meta-assertion about harness completeness, not a skill quality assertion. This is confusing without clarification.

---

### 15. `skill-trigger-optimization`

**What it does:** Rewrite a skill's description and When-to-use sections to fix routing precision or recall.

#### Trigger fidelity
**This skill's description does not follow the teaching it contains.** The description opens: "Rewrite a skill's description field and 'When to use' triggers to fix routing precision and recall." The skill's own procedure says descriptions should "start with an action verb, include 2–3 realistic trigger phrases users would say, state what the skill produces." The description doesn't include trigger phrases users would say — it describes the skill's function technically. A user thinking "my skill never fires" is not going to use the phrase "routing precision and recall."

The worked example inside the skill body is the best before/after example in the set. The skill knows what a good description looks like. It doesn't use that knowledge on itself.

#### Procedural atomicity
Good. The three-type diagnostic taxonomy (undertriggering, overtriggering, confusion) cleanly covers the problem space. The description rewriting rules in Step 4 are specific and checkable. The verification checklist (Step 6) makes the output testable.

#### Output determinism
Five-section structure with a before/after display. The checklist at the end is verifiable.

#### Issues
- **High:** The skill's description contradicts its own teaching. It should be the demonstration case for the skill working correctly. Fix by rewriting the description using the skill's own Step 4 rules.
- **Medium:** "Genuine overlap with another skill: Recommend adding explicit routing rules to the host configuration." "Host configuration" is undefined jargon. Specify: "Escalate to `skill-catalog-curation` to resolve the boundary at the library level."
- **Low:** Two "Do NOT use" bullets both route to `skill-authoring`. Consolidate.

---

### 16. `skill-variant-splitting`

**What it does:** Split a broad skill into focused variants along a defined axis.

#### Trigger fidelity
The description uses structural observation signals ("disjoint 'For X'/'For Y' sections", "conditional-branch-heavy procedure") rather than user-spoken phrases. These signals are things an agent would observe in a skill, not things a user would say. The user-spoken phrases ("this skill does too much", "split this skill", "create variants") appear in the body's "When to use" section but not in the description — which is the only field the routing model reads.

This is a significant trigger fidelity problem. A user saying "this skill does too much" may not activate this skill because those words don't appear in the description.

#### Procedural atomicity
Eight steps covering the full splitting lifecycle are well-ordered. The axis selection priority framework (fewest variants → most distinct vocabularies → Stack > Domain > Platform > Scope) converts a multi-criteria decision into an ordered rule set. The ">5 variants → hierarchical organisation" red flag prevents runaway splitting.

Step 4 (Extract shared core) asks the agent to "decide: base skill with extensions, or fully independent variants." This is a decision point with no decision rule. Both options are named but neither has criteria for selection. An agent facing this step must choose arbitrarily. Add a specific rule: "If shared content exceeds 30% of each variant's total length, create a base skill; otherwise use fully independent variants."

#### Output determinism
Five-section markdown document with coverage map checkboxes. ✓

#### Issues
- **High:** User-spoken trigger phrases absent from description. Add: "Use when the user says 'this skill does too much', 'split this skill', or 'create variants for X and Y'."
- **Medium:** Step 4 offers two options without selection criteria. Add a threshold-based rule.
- **Low:** No next-step pointer to `skill-catalog-curation` for updating the library index after a split.

---

## Ecosystem-Level Findings

These are problems that cannot be identified by reviewing any single skill — they only appear when you look at the set as a whole.

### The Pipeline Problem

The set contains three evaluation-related skills — `skill-testing-harness`, `skill-evaluation`, `skill-benchmarking` — that form a logical sequence:

1. Build tests (`skill-testing-harness`)
2. Run tests to evaluate a single skill (`skill-evaluation`)
3. Compare variants head-to-head (`skill-benchmarking`)

No skill describes this sequence. A new user who has just authored a skill doesn't know whether to use `skill-testing-harness` first or `skill-evaluation` first, or whether they're alternatives. The set needs either a workflow overview document or a "Next step" pointer at the end of each skill in the pipeline.

### The Merge Gap

Three skills recommend merging skills as a resolution action:
- `skill-catalog-curation` recommends "merge" for duplicates
- `skill-variant-splitting` recommends "merge" for overly narrow splits
- `skill-testing-harness` incorrectly recommends `skill-variant-splitting` for narrow skills (should recommend merging)

No skill in the set describes how to actually perform a merge. Merging is non-trivial: which skill's routing wins? How are procedures combined? What happens to existing evals? This is missing functionality in the ecosystem.

### The Provenance Gap

`skill-provenance` is the most isolated skill in the set. No other skill references it in its "next steps" or "after this, consider..." guidance. The most natural trigger points (after `skill-installer` installs from an external source, after `skill-lifecycle-management` promotes a skill to stable) contain no mention of provenance recording. It exists but will never be used unless a user explicitly asks for it.

### Self-Consistency Failures

Two skills fail to demonstrate the qualities they teach:

1. **`skill-improver`** teaches that skills need dedicated output contract sections. It does not have one.
2. **`skill-trigger-optimization`** teaches that descriptions should open with user-spoken trigger phrases. Its description opens with technical language that users would not naturally say.

These are the most important findings in the set. A meta-skill that doesn't pass its own test provides evidence that the test may be wrong, the skill may be wrong, or both.

### No Communication Guidance

The installed `skill-creator` devotes a significant section to communication calibration — adjusting jargon and tone to the user's technical level. None of the 16 uploaded skills address this. For meta-skills used by authors of varying technical sophistication, this matters. An author who doesn't know what "routing precision" or "trigger recall" means will be confused by `skill-evaluation` without contextual explanation.

---

## Priority Actions

Ordered by impact, using first-principles assessment.

1. **[Critical]** `skill-testing-harness`: Remove "baseline comparisons" from the description or add the artifact. A false promise in a description is worse than a missing feature.
2. **[Critical]** `skill-improver`: Extract output contract from Phase 7 into a dedicated `# Output contract` section. The hub skill must demonstrate the structure it teaches.
3. **[Critical]** `skill-trigger-optimization`: Rewrite the skill's own description to use its own rules. A trigger-optimization skill with a bad trigger is not credible.
4. **[High]** `skill-variant-splitting`: Add user-spoken trigger phrases to the description. Structural signals are not user utterances.
5. **[High]** `skill-authoring`: Add "next step: `skill-testing-harness` before use" to the output contract. Untested skills are unknown-quality skills.
6. **[High]** `skill-evaluation`: Specify how to deactivate a skill for baseline comparison. Step 5 cannot be executed without this.
7. **[High]** `skill-safety-review`: Rewrite Step 4 (injection) to match the specification depth of the other 7 steps.
8. **[High]** `skill-anti-patterns`: The checklist is incomplete. Add anti-patterns for: instruction length/complexity, agent capability assumptions, few-shot starvation, hallucination surface from vague success criteria.
9. **[High]** Write a pipeline overview explaining the sequence: authoring → testing → evaluation → benchmarking → lifecycle management.
10. **[Medium]** `skill-improver`: Expand failure handling from 2 to 5+ cases.
11. **[Medium]** `skill-lifecycle-management`: Fix description (three opening verbs) and make "used in 2 real projects" either verifiable or removable.
12. **[Medium]** `skill-evaluation`: Calibrate precision/recall thresholds as targets, not hard lines.
13. **[Medium]** `skill-benchmarking`: Fix the "quick spot-check" Do NOT use bullet — name an alternative or remove.
14. **[Medium]** `skill-provenance`: Add automatic trigger conditions in import and promotion workflows.
15. **[Medium]** `skill-packaging`: Add concrete overlay example to Step 3.
16. **[Low]** `skill-reference-extraction`: Remove or replace the bad example in Step 5.
17. **[Low]** `skill-catalog-curation`: Reformat description to match the rest of the set's integrated prose style.
18. **[Low]** Consider adding `skill-merging` to the ecosystem. Three skills recommend it; none describe it.

---

*This review used: independent knowledge of LLM instruction-following behaviour, prompt engineering research, agent system design principles, technical writing standards, and the externally-installed `skill-creator` SKILL.md as an independent baseline. No rubric, checklist, or standard from the uploaded skill set was used as an evaluation instrument.*
