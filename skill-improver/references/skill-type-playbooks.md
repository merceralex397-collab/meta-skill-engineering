# Skill-Type Playbooks

Different skill types need different improvement strategies.

## Domain skill

Examples: React patterns, FastAPI patterns, Firebase patterns.

Improve by focusing on:
- concrete stack choices,
- decision rules,
- anti-patterns,
- version-sensitive gotchas,
- testing and failure modes.

Common mistake:
- generic framework advice with no repo or stack binding.

## Operational skill

Examples: release workflow, PR creation, triage, migration, patch planning.

Improve by focusing on:
- ordered steps,
- exact commands where appropriate,
- verification gates,
- stop conditions,
- rollback or blocker paths.

Common mistake:
- starting steps are clear, but completion checks are absent.

## Artifact skill

Examples: docx, pdf, pptx, xlsx generation.

Improve by focusing on:
- input/output contract,
- file paths and artifact expectations,
- defaults,
- script/tool usage boundaries,
- user review loop.

Common mistake:
- vague output expectations or no file-quality checks.

## Meta-skill

Examples: skill creator, skill improver, prompt hardening, planning.

Improve by focusing on:
- scope boundaries,
- mode selection,
- evaluation loop,
- preserving original intent,
- support-layer justification.

Common mistake:
- becoming a giant “think better” instruction blob with no operational structure.
