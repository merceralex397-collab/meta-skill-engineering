---
name: stack-standards
description: Extract the actual coding, validation, and operational conventions for the active stack from a real repository instead of guessing generic best practices. Trigger on "stack conventions", "project standards", "coding style", "what patterns does this repo use", or when onboarding to an unfamiliar codebase. Do not use for security-best-practices (secure coding), testing (test strategy), or deployment-pipeline (CI/CD setup).
---

# Purpose
A generated-per-project skill that encodes the project's actual stack conventions—not generic best practices, but the specific choices this repo uses. This is a placeholder-then-fill pattern: scaffolding creates the skeleton, then project-skill-bootstrap populates it with real values detected from the codebase.

# When to use this skill
Use when:
- Writing new code and need to know which patterns this project uses
- Reviewing code and need to verify it follows project conventions
- Onboarding to an unfamiliar codebase and need stack context
- The project has been scaffolded and needs its stack-standards filled in

Do NOT use when:
- The repo has no detectable stack (pure documentation repo)
- Making a one-off script that won't live in the repo
- The stack-standards skill has not been populated yet (populate first)

# Operating procedure (TEMPLATE)
This skill file should be customized per-project. Below is the template structure with `[PLACEHOLDER]` values to fill.

## 1. Language & Runtime
```yaml
primary_language: [TypeScript | Python | Rust | Go | ...]
runtime: [Node.js 20 | Python 3.11 | Rust stable | ...]
secondary_languages: [CSS, SQL, Shell]
```

## 2. Package Management
```yaml
package_manager: [npm | pnpm | yarn | pip | cargo | ...]
lockfile: [package-lock.json | pnpm-lock.yaml | ...]
install_command: [npm ci | pnpm install --frozen-lockfile | ...]
```

## 3. Code Style
```yaml
formatter: [prettier | black | rustfmt | gofmt | ...]
formatter_config: [.prettierrc | pyproject.toml | ...]
linter: [eslint | ruff | clippy | golangci-lint | ...]
linter_config: [.eslintrc.js | ruff.toml | ...]
```

**Style rules specific to this project:**
- [e.g., "Use single quotes for strings"]
- [e.g., "No default exports"]
- [e.g., "snake_case for functions, PascalCase for classes"]

## 4. Testing
```yaml
test_framework: [vitest | pytest | cargo test | go test | ...]
test_command: [npm test | pytest | cargo test | ...]
test_location: [tests/ | __tests__/ | src/**/*.test.ts | ...]
coverage_tool: [c8 | coverage.py | tarpaulin | ...]
coverage_threshold: [80% | none specified]
```

## 5. Build & Bundle
```yaml
build_tool: [tsc | vite | webpack | none | ...]
build_command: [npm run build | cargo build --release | ...]
output_directory: [dist/ | build/ | target/release/ | ...]
```

## 6. Project Structure
```
[ROOT]/
├── src/           # [Description: main source code]
├── tests/         # [Description: test files]
├── docs/          # [Description: documentation]
├── scripts/       # [Description: build/dev scripts]
└── ...
```

## 7. Framework Conventions
If using a framework, document its specific patterns:
```yaml
framework: [React | FastAPI | Axum | Gin | ...]
routing: [file-based | explicit | ...]
state_management: [zustand | redux | context | ...]
api_style: [REST | GraphQL | tRPC | ...]
```

## 8. Database & Data
```yaml
database: [PostgreSQL | SQLite | Firestore | none | ...]
orm: [Prisma | SQLAlchemy | Diesel | none | ...]
migrations: [prisma migrate | alembic | manual | ...]
```

## 9. Safety Rules
Hard rules that must not be violated:
- [ ] No `any` types in TypeScript (use `unknown` + type guards)
- [ ] No `unwrap()` in Rust production code (use `?` or explicit handling)
- [ ] No raw SQL queries (use parameterized queries only)
- [ ] No secrets in code (use environment variables)
- [ ] [Project-specific rule]

## 10. Validation Commands
Run these to validate code before committing:
```bash
[npm run lint && npm run typecheck && npm test]
[cargo clippy && cargo test]
[ruff check . && pytest]
```

# Output defaults
When invoked, this skill returns the filled-in stack standards for the current project, enabling agents to write code that matches project conventions.

# References
- Detected from: package.json, Cargo.toml, pyproject.toml, go.mod, etc.
- Framework docs: [Link to relevant framework documentation]

# Failure handling
- **Standards not yet populated**: Return "Stack standards not configured. Run stack-profile-detector and project-skill-bootstrap first."
- **Conflicting signals**: If package.json says one thing but code uses another, flag discrepancy and ask for clarification
- **Unknown pattern**: If asked about a convention not documented here, state "Not specified in stack-standards. Check codebase for precedent or make explicit decision."
