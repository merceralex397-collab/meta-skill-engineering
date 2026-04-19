---
name: skill-installer
description: >-
  Install a skill package into the local agent client from a GitHub repository,
  local folder, or archive. Use when the user says "install this skill", "add
  skill from GitHub", or "list available skills". Do not use for creating new
  skills (use skill-creator), packaging skills for distribution (use
  skill-packaging), or improving already-installed skills (use skill-improver).
---

# Purpose

Install a skill package into the local agent client's skill directory so it becomes discoverable on next restart. Supports GitHub repos, local folders, and tarballs/zips.

# When to use

- User says "install this skill", "add skill from GitHub", "list available skills"
- Installing from a GitHub repo by owner/repo or URL
- Installing from a local folder or archive
- Listing what skills are available vs already installed

Do NOT use when:

- Creating a new skill from scratch → `skill-creator`
- Packaging a skill for distribution → `skill-packaging`
- Improving an already-installed skill → `skill-improver`
- Reconfiguring or removing an already-installed skill (manual task)

# Client skill paths

| Client | Path |
|--------|------|
| OpenCode | `.opencode/skills/<name>/` |
| Copilot | `.github/skills/<name>/` or `.vscode/skills/<name>/` |
| Claude Code | `.claude/skills/<name>/` (project) or `~/.claude/skills/<name>/` (user) |
| Gemini CLI | `.gemini/skills/<name>/` |

# Operating procedure

## Install from GitHub

1. Run: `scripts/install-skill-from-github.py --repo <owner>/<repo> --path <path/to/skill>`
   - Or by URL: `--url https://github.com/<owner>/<repo>/tree/<ref>/<path>`
2. Script downloads via zip (public repos) or falls back to git sparse checkout on auth errors
3. Pass `--dest <path>` to override the default install location for the target client
4. Confirm the installed folder contains SKILL.md
5. Tell user: "Restart the agent client to pick up the new skill"

## Install from local source

1. **Folder**: Verify SKILL.md exists, then copy to the client skill path
2. **Archive** (tar.gz / zip): Extract to temp dir, verify SKILL.md, copy to client skill path
3. Never overwrite an existing destination — abort and ask the user first

## List available skills

1. Run: `scripts/list-skills.py` (or `--format json` for machine output)
2. Present results with "(already installed)" annotations
3. Ask user which skills to install

## Script reference

> **Network required** — these scripts make GitHub API calls. Request sandbox
> escalation if running in a restricted environment.

| Script | Purpose |
|--------|---------|
| `scripts/install-skill-from-github.py` | Download and install from any GitHub path |
| `scripts/list-skills.py` | List available skills with install status |

Install script options: `--repo`, `--path` (repeatable), `--url`, `--ref` (default: main), `--dest`, `--method auto|download|git`, `--name`, `--client copilot|codex|opencode|claude-code|gemini-cli`.

The `--client` flag selects the default install path for the target agent client (default: `copilot`). Use `--dest` to override the path entirely. The list script requires `--repo` to specify which GitHub repository to query.

# Safety

- **Overwrite protection**: Scripts abort if the destination directory already exists. Never force-overwrite without explicit user confirmation.
- **Path traversal**: Scripts validate that skill paths are relative and inside the target directory. Do not bypass this by constructing absolute paths.
- **Source trust**: Only install skills from repositories the user explicitly identifies. Do not auto-discover and install skills without user approval.
- **Pre-install verification (URL/GitHub sources)**:
  1. Verify the repository is not archived (archived repos receive no maintenance).
  2. Check the SKILL.md has valid YAML frontmatter (`name` and `description` fields present and non-empty).
  3. If the skill has a `scripts/` directory, scan each script file for: shell commands with `sudo`, network calls to non-origin URLs, file operations outside the skill directory. Flag any found and ask user to confirm before proceeding.
- **Post-install verification**: Run `cat SKILL.md | head -5` to confirm frontmatter parsed correctly. Check that the skill directory appears at the expected client skill path.
- **If verification fails**: Remove the installed skill directory immediately. Report which specific check failed and stop.
- **Archive extraction**: The zip extractor rejects archives containing paths that escape the destination directory.
- **Credentials**: Scripts use `GITHUB_TOKEN` / `GH_TOKEN` if set. Never log or echo token values.

# Output contract

After a successful install, report exactly:

```
## Installed: <skill-name>

**Location**: <full path to installed skill directory>
**Source**: <GitHub URL or local path>

Verified:
- SKILL.md present
- No overwrite conflict

Restart the agent client to pick up the new skill.
```

On failure, report the specific error (see Failure handling) and stop — do not partially install.

# Failure handling

| Condition | Action |
|-----------|--------|
| Destination already exists | Abort. Ask user: overwrite, skip, or rename? |
| No SKILL.md in source | Abort: "Not a valid skill package — no SKILL.md found" |
| HTTP 401/403/404 on download | Fall back to git sparse checkout; if that also fails, tell user to set `GITHUB_TOKEN` |
| Archive contains path-traversal entries | Abort: "Archive rejected — contains paths outside destination" |
| Network unavailable | Report the error and suggest sandbox escalation or manual download |

## Next steps

After installing a skill:
- If imported from external source → run `skill-provenance` to record origin and trust
- If imported from untrusted source → run `skill-safety-review` before use
- Verify the skill works → `skill-evaluation`
