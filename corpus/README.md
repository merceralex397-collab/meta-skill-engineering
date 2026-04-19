# Test Skill Corpus

This directory contains realistic SKILL.md files organized by quality tier.
Meta-skills — skill-improver, skill-evaluation, skill-anti-patterns,
skill-safety-review, and others — use these files as test inputs to verify
they detect, report, and (where appropriate) repair the right things.

## Categories

### weak/

Skills with known, realistic defects. Meta-skills should identify and
correct these issues. Each file targets a specific class of problem:
vague procedures, missing sections, bloated inline content, overbroad
triggers, or absent output contracts.

### strong/

Well-formed skills that follow every convention in the repo's AGENTS.md.
Meta-skills should preserve these unchanged. Running skill-improver on a
strong skill and getting material modifications is a false-positive signal.

### adversarial/

Edge cases designed to stress-test graceful handling: contradictory
sections, circular cross-references, prompt-injection payloads, and
malformed YAML frontmatter. Meta-skills should flag problems clearly
without crashing or silently corrupting content.

### regression/

Initially seeded with 3 known-failure cases (purpose-lost, boundaries-deleted,
references-broken). Populated over time by harvesting real failures from
meta-skill runs via `scripts/harvest_failures.py`. When a meta-skill
mishandles a skill in production, a reduced reproduction case is added here
so the failure stays covered.
