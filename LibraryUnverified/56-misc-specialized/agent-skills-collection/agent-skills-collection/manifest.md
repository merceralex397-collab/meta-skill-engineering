# Agent Skills Collection Manifest

## What is in this archive

This archive is a **best-effort collection** built from the sources that were directly accessible in this environment.

### Fully included local artifacts

- Uploaded Codex `skill-creator.zip` extracted under `extracted/local/codex-skill-creator/`
- Uploaded Codex `skill-installer.zip` extracted under `extracted/local/codex-skill-installer-bundle/`
- Uploaded `SKILL.md` copy under `extracted/local/openword-SKILL.md`
- Uploaded Claude.ai skill-creator text under `extracted/local/claude-ai-skill-creator.txt`
- Uploaded research notes copied into `notes/`

### Catalogued official skill sets

- **Anthropic official skills:** 17 skill folders listed in `catalogs/anthropics-skills.txt`
- **OpenAI official curated skills:** 35 skill folders listed in `catalogs/openai-curated-skills.txt`
- **OpenAI official system skills:** 3 skill folders listed in `catalogs/openai-system-skills.txt`

### Helper script

- `scripts/fetch_official_skills.py` downloads and extracts the two official GitHub repos on a machine with internet access.

## Why this is not a full mirror of every public skill page

The linked ecosystem mixes:

1. **Spec / authoring docs** (Gemini CLI, OpenCode, GitHub Copilot)
2. **Official repos** (Anthropic, OpenAI)
3. **Large registries / directories** (skills.sh)
4. **Spec index / examples hub** (agentskills.io)

In this environment I could reliably inspect docs pages and repo trees, and fully extract the uploaded local zip files. What I could **not** do here was perform a bulk export of every registry-hosted skill package from public marketplaces, because:

- the registries do not expose a clean bulk-download endpoint through the available tools here
- direct repository archive downloads are blocked from being saved by the sandbox download restrictions
- container-side Python has no outbound network access

So this archive is structured as:

- **full local extraction where available**
- **official repo catalogs where enumerable**
- **a fetch script for completing the mirror outside this sandbox**

## Source summary

See `catalogs/repo-catalog.json` for structured source metadata.

## Recommended next step

Run:

```bash
python scripts/fetch_official_skills.py /path/to/output
```

from a normal internet-connected machine. That will pull the two biggest official repos immediately. After that, the next logical expansion source is the skills.sh registry.
