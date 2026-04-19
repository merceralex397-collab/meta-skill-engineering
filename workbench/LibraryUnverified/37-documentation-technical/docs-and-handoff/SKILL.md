---
name: docs-and-handoff
description: Keeps README, CHANGELOG, API docs, START-HERE.md, context-snapshot.md, BOARD.md, and manifest.json aligned with canonical project state. Trigger on 'update docs', 'README sync', 'handoff', 'documentation health', 'CHANGELOG entry', 'doc drift'. DO NOT USE for initial project scaffolding (use repo-scaffold-factory skill) or ticket/board creation from scratch (use ticket-execution).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: docs-and-handoff
  maturity: draft
  risk: low
  tags: [docs, handoff]
---

# Purpose
Keeps documentation current through the project lifecycle by defining what to update at each stage and in what format. Prevents doc drift—where README says one thing but code does another—by establishing documentation as part of the definition of done, not an afterthought.

# When to use this skill
Use when:
- Completing a ticket that affects user-facing behavior
- Adding new features, APIs, or configuration options
- Changing project structure or conventions
- Periodic documentation health checks

Do NOT use when:
- Internal refactoring with no API/behavior changes
- Test-only changes
- Work in progress (update docs when done, not during)

# Operating procedure

## 1. Documentation inventory
Standard documentation locations:
```
README.md              # Project overview, quick start
AGENTS.md              # Agent-specific instructions
START-HERE.md          # Session restart surface for agents
context-snapshot.md    # Compressed project state for context reload
docs/
├── BRIEF.md           # Project brief (source of truth for "what")
├── ARCHITECTURE.md    # System design
├── API.md             # API documentation
├── CONTRIBUTING.md    # How to contribute
└── CHANGELOG.md       # Version history
tickets/
├── BOARD.md           # Current work status
├── TICKET-INDEX.md    # All tickets
└── manifest.json      # Machine-readable ticket manifest
```

## 2. Update triggers by change type

### New feature added
Update:
- [ ] README.md — Add to features list, usage example if applicable
- [ ] docs/API.md — Document new endpoints/functions
- [ ] docs/CHANGELOG.md — Add to Unreleased section

### Configuration changed
Update:
- [ ] README.md — Update setup/config section
- [ ] .env.example — Add new variables with comments
- [ ] docs/ARCHITECTURE.md — If affects system design

### Breaking change
Update:
- [ ] README.md — Note breaking change prominently
- [ ] docs/CHANGELOG.md — Add to Breaking Changes
- [ ] docs/MIGRATION.md — Create/update migration guide

### Dependency added/removed
Update:
- [ ] README.md — Update prerequisites if user-facing
- [ ] docs/CONTRIBUTING.md — Update dev setup if dev-facing

### Bug fixed
Update:
- [ ] docs/CHANGELOG.md — Add to Fixed section
- [ ] README.md — Remove any workaround notes

## 3. Documentation update format

### README.md structure
```markdown
# Project Name

Brief description.

## Quick Start
[Minimum commands to get running]

## Features
- Feature 1
- Feature 2

## Installation
[Step-by-step]

## Usage
[Examples]

## Configuration
[Environment variables, config files]

## Documentation
- [API Reference](docs/API.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Contributing](docs/CONTRIBUTING.md)

## For AI Agents
See [AGENTS.md](AGENTS.md)
```

### CHANGELOG.md format (Keep a Changelog style)
```markdown
# Changelog

## [Unreleased]
### Added
- New feature X (#PR)

### Changed
- Modified behavior Y (#PR)

### Fixed
- Bug in Z (#PR)

### Removed
- Deprecated feature W

## [1.0.0] - 2024-01-15
### Added
- Initial release
```

### API.md format
```markdown
# API Reference

## Endpoints

### POST /api/auth/login
Authenticate a user.

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "string",
  "expiresAt": "ISO date"
}
```

**Errors:**
- 401: Invalid credentials
- 429: Rate limited
```

## 4. Documentation health check
Run periodically (weekly or per milestone):

```bash
# Check README accuracy
cat README.md | grep -E "npm |yarn |pnpm " | while read cmd; do
  echo "Testing: $cmd"
  # Verify command works
done

# Check for stale docs
find docs/ -name "*.md" -mtime +30 -print
# Review any docs not updated in 30 days

# Check CHANGELOG has unreleased changes
git log --oneline $(git describe --tags --abbrev=0)..HEAD | head -10
# Should match Unreleased section
```

## 5. Board and ticket sync
Keep BOARD.md in sync with ticket reality:

```bash
# Regenerate BOARD.md from tickets
echo "# Project Board" > tickets/BOARD.md
echo "" >> tickets/BOARD.md
for status in "In Progress" "Ready" "Blocked" "Done"; do
  echo "## $status" >> tickets/BOARD.md
  grep -l "status:.*$(echo $status | tr '[:upper:]' '[:lower:]' | tr ' ' '_')" tickets/TKT-*.md 2>/dev/null | while read f; do
    title=$(grep "^title:" "$f" | cut -d: -f2-)
    echo "- [$(basename $f .md)]($f):$title" >> tickets/BOARD.md
  done
  echo "" >> tickets/BOARD.md
done
```

## 6. Documentation as definition of done
For any ticket involving user-facing changes:

```markdown
## Acceptance Criteria
- [ ] Feature implemented
- [ ] Tests passing
- [ ] README updated (if applicable)
- [ ] API docs updated (if applicable)
- [ ] CHANGELOG updated
```

## 7. Handoff document maintenance
When creating handoffs (see handoff-brief skill):
- **START-HERE.md**: Top-level restart surface. Links to BOARD.md, active tickets, and key docs. Update after every milestone.
- **context-snapshot.md**: Compressed project state (architecture summary, active decisions, known risks). Regenerate when context would exceed a fresh agent's budget.
- **BOARD.md**: Reflects current ticket statuses. Regenerate from ticket files, never hand-edit.
- **manifest.json**: Machine-readable ticket index. Keep in sync when tickets are created, moved, or closed.
- Ensure docs referenced in handoff are current
- Note any doc updates needed but deferred

# Output defaults
After documentation update:
```markdown
## Documentation Updated
- README.md: Added feature X to features list
- docs/API.md: Documented /api/new-endpoint
- CHANGELOG.md: Added to Unreleased > Added

Files modified:
- README.md
- docs/API.md
- CHANGELOG.md
```

# References
- Keep a Changelog: https://keepachangelog.com
- Conventional Commits: https://www.conventionalcommits.org
- Documentation-driven development: docs as contract

# Failure handling
- **Can't verify README commands**: Flag specific command as "needs verification"
- **Missing doc section**: Create skeleton with TODO markers rather than leaving blank
- **CHANGELOG out of sync**: Regenerate Unreleased from git log since last tag
- **Conflicting docs**: Pick authoritative source, update others, note resolution
