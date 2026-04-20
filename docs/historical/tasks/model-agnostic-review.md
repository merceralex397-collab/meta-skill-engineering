# Sub Task F — Model Agnostic Review

## Review scope
Searched all 20 SKILL.md files for references to specific LLM/AI providers: codex, claude, anthropic, copilot, openai, gemini, opencode, microsoft, gpt.

## Findings

### 1. community-skill-harvester/SKILL.md — Vendor-specific search sources
**Lines 20-21**: Hardcoded Anthropic skills repo as a search source
**Line 81**: Uses `COPILOT/` path prefix in import example
**Line 105**: References Anthropic Skills URL

**Recommendation**: Replace vendor-specific sources with generic patterns. The Anthropic repo is one example source, not the canonical source. The COPILOT/ path should use a generic agent client path variable.

### 2. skill-installer/SKILL.md — Client-specific install paths
**Lines 33-36**: Hardcoded paths for OpenCode, Copilot, Claude Code, Gemini CLI
**Line 71**: Lists specific client names in script options

**Assessment**: This is ACCEPTABLE. The skill-installer's entire purpose is to install skills into specific agent client directories. Naming the clients and their paths is functional necessity, not vendor bias. The skill correctly treats all clients equally.

### 3. skill-packaging/SKILL.md — Client-specific overlay formats
**Lines 78-100**: Overlay format examples for Copilot, OpenCode, Codex

**Assessment**: This is ACCEPTABLE. Overlay generation requires knowing each client's format. The skill treats all clients equally and the formats are clearly examples.

### 4. skill-packager/SKILL.md — Anthropic reference URL
**Line 90**: References `https://github.com/anthropics/skills`

**Recommendation**: Replace with a more generic reference or remove. The Anthropic skills format is one implementation, not a standard.

### 5. skill-anti-patterns/SKILL.md — Copilot docs reference
**Line 95**: Example uses a GitHub Copilot docs URL as an AP-10 fix example

**Assessment**: This is ACCEPTABLE. It's an example of what a good reference looks like, not a recommendation to use Copilot specifically.

### 6. skill-provenance/SKILL.md — OpenCode path reference
**Line 131**: Lists `.opencode/` as an example path pattern assumption

**Assessment**: This is ACCEPTABLE. It's listing path patterns as indicators of encoded assumptions, not recommending a specific tool.

## Summary of required changes

| Skill | Issue | Action |
|-------|-------|--------|
| community-skill-harvester | Hardcoded Anthropic repo and COPILOT/ path | Generalize search sources and import paths |
| skill-packager | Anthropic-specific reference URL | Replace with generic reference |

All other findings are acceptable — the skills reference specific clients only where functionally necessary (installer, packaging) or as illustrative examples.
