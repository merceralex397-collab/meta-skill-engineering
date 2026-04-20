---
name: repo-navigation
description: Map a repository's directory roles so the agent can distinguish canonical source, generated output, operational scripts, tests, archives, and user-owned areas before making changes. Use when the repo is large, layered, scaffolded, or has multiple output surfaces, and when the user asks where to work or which folders are safe to edit. Do not use for project-context (loading specific docs and project state), stack-standards (language/framework conventions), or small single-folder projects where the structure is obvious.
---

# Purpose
Efficiently orients an agent in an unfamiliar codebase by establishing a reading order and directory taxonomy. Prevents drift caused by agents editing generated files, confusing test fixtures with source code, or missing key configuration files. Builds a mental model of what's canonical vs. derived.

# When to use this skill
Use when:
- First encountering a new codebase
- Context-switching to a different project
- Unsure which files are safe to edit vs. generated
- Need to find where specific functionality lives

Do NOT use when:
- Already familiar with the repo's structure
- Working on a single well-understood file
- The repo has explicit AGENTS.md with reading order (follow that instead)

# Operating procedure

## 1. Quick orientation (30 seconds)
```bash
# Get repo basics
pwd && ls -la
cat README.md 2>/dev/null | head -50
cat AGENTS.md 2>/dev/null | head -50
```

## 2. Identify package/project type
```bash
# Check for manifest files
ls package.json Cargo.toml pyproject.toml go.mod pom.xml *.csproj 2>/dev/null
```

Map to project type:
- `package.json` → Node.js/TypeScript
- `Cargo.toml` → Rust
- `pyproject.toml` or `setup.py` → Python
- `go.mod` → Go
- `pom.xml` → Java/Maven

## 3. Map directory taxonomy

### Canonical (source of truth, safe to edit)
```
src/           # Main source code
lib/           # Library code
app/           # Application code (frameworks)
components/    # UI components (React, Vue, etc.)
```

### Derived (generated, do NOT edit)
```
dist/          # Build output
build/         # Build output
target/        # Rust/Java build output
node_modules/  # Dependencies
.next/         # Next.js build
__pycache__/   # Python bytecode
*.generated.*  # Explicitly generated files
```

### Configuration (edit carefully)
```
*.config.js    # Build/tool config
*.json         # Config files (package.json, tsconfig.json)
*.yaml/*.yml   # Config files
.env*          # Environment (never commit secrets)
```

### Documentation (safe to edit)
```
docs/          # Documentation
README.md      # Project readme
AGENTS.md      # Agent instructions
CHANGELOG.md   # Version history
```

### Tests (safe to edit)
```
tests/         # Test files
__tests__/     # Jest convention
*.test.ts      # Test files
*.spec.ts      # Test files
test/          # Test directory
```

### Operational (agent infrastructure)
```
.opencode/     # Agent config
.copilot/      # Copilot config
tickets/       # Task tracking
.github/       # GitHub config (workflows, templates)
```

## 4. Build reading order
For a typical project, read in this order:

1. **What is this?** `README.md`, `AGENTS.md`
2. **What's the stack?** `package.json`/`Cargo.toml`/etc., `docs/STACK-PROFILE.md`
3. **How is it structured?** `tree -L 2 -I 'node_modules|dist|target'`
4. **What's the entry point?** 
   - Check `main` field in package.json
   - Look for `src/index.*`, `src/main.*`, `app/page.*`
5. **What are the key abstractions?**
   - Look for `types/`, `interfaces/`, `models/`
   - Check `src/lib/` or `src/core/`

## 5. Verify understanding
Before making changes, confirm:
```bash
# Understand build process
npm run build --dry-run 2>/dev/null || cargo check 2>/dev/null
# Understand test process  
npm test -- --listTests 2>/dev/null || cargo test --no-run 2>/dev/null
```

## 6. Document findings
If no AGENTS.md exists, create mental model:
```markdown
## Repo Mental Model

### Entry Points
- Main: src/index.ts
- CLI: src/cli.ts

### Key Directories
- Business logic: src/services/
- Data models: src/models/
- API routes: src/routes/

### Generated (don't edit)
- dist/, node_modules/

### Build Commands
- Install: npm ci
- Build: npm run build
- Test: npm test
```

# Output defaults
Return a structured summary:
```markdown
## Navigation Summary

**Project type**: [Node.js/TypeScript]
**Entry point**: [src/index.ts]

### Directory map
| Path | Type | Purpose |
|------|------|---------|
| src/ | canonical | Main source code |
| dist/ | derived | Build output |
| tests/ | canonical | Test files |

### Reading order
1. README.md
2. package.json
3. src/index.ts
4. src/types/
```

# References
- AGENTS.md convention: project-specific agent instructions
- Common project layouts vary by ecosystem

# Failure handling
- **No manifest files**: Treat as ad-hoc project. Look for any executable files, Makefile, or shell scripts.
- **Monorepo detected**: Identify sub-packages (`packages/`, `apps/`, `crates/`), navigate each as a separate repo.
- **Conflicting structures**: If both `src/` and `lib/` exist with similar content, check imports to determine which is canonical.
