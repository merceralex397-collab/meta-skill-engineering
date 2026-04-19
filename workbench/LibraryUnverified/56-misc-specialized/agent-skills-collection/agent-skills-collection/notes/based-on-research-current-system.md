# Research Report 2: Specific Skill Examples for Generated Output Repositories

*Follows up on Report 1. Provides concrete skill file content, domain-by-domain analysis, and template sketches across 25+ domains.*

---

## Executive Summary

This report examines specific skill and rule files from GitHub — not repository overviews, but the actual `.cursor/rules/*.mdc`, `.opencode/skills/*/SKILL.md`, and `.github/skills/*/SKILL.md` files themselves — across 25+ domains. The strongest examples share three characteristics absent from the current Scafforge generated output: **trigger specificity** (exact activation conditions plus DO NOT TRIGGER conditions), **procedural phase structure** (named phases with verification gates), and **domain-binding** (actual tool names, library choices, file conventions, and anti-patterns specific to that stack).

The current Scafforge output pack's biggest gap is not quantity but depth. `stack-standards` is explicitly a placeholder; most skills have no references directory, no scripts, no examples, no negative patterns. A credible output skill catalog should contain approximately 20-30 domain-specific skills in 7-8 families, with deeper domain skills running 200-500 lines each.[^1][^2]

---

## 1. Anthropic Claude API / AI SDK Integration

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/claude-api/SKILL.md` (SHA: `0fa842997c9e8c447151abced6abb0c4c6fb1b7f`)[^3]

This is one of the strongest skills in the survey. Its description field reads: *"TRIGGER when: code imports `anthropic`/`@anthropic-ai/sdk`... DO NOT TRIGGER when: code imports `openai`/other AI SDK, general programming, or ML/data-science tasks."* — explicit DO NOT TRIGGER conditions in the description are the single most impactful pattern for preventing undertriggering.[^3]

It provides:
1. Language detection logic — scans project files (`requirements.txt`, `package.json`, `*.go`) to infer which SDK applies
2. A surface decision table — maps use case to API tier (single call / workflow / agent)
3. A decision tree — four branches with prose explanation and real examples
4. An "agent check" gate — four criteria (complexity, value, viability, cost of error) that must all pass
5. A model table with exact model ID strings, pricing, and hard rule ("ALWAYS use claude-opus-4-6")
6. Common pitfalls section — 10 concrete mistakes with the fix inline
7. A reading guide — tells the agent which bundled reference files to read for each task type

The skill body points to `{lang}/claude-api/README.md`, `shared/tool-use-concepts.md`, etc., demonstrating progressive disclosure through bundled references.[^3]

**Template sketch for a generated repo**:
```
skills/llm-integration/SKILL.md
  - description: explicit trigger + DO NOT TRIGGER conditions
  - Language detection (project files -> infer SDK)
  - Provider/SDK decision table (OpenAI / Anthropic / Gemini / local)
  - Use-case decision tree (completion / tool use / agent / multimodal)
  - Model IDs + auth pattern + env vars (project-specific)
  - Common pitfalls (model ID drift, streaming for large outputs, token limits)
  - Reading guide -> references/anthropic.md, references/openai.md, references/local-llms.md
skills/llm-integration/references/anthropic.md
skills/llm-integration/references/openai.md
skills/llm-integration/references/local-llms.md
```

---

## 2. React / TypeScript Frontend

**File examined**: [cheshirecode/dotfiles](https://github.com/cheshirecode/dotfiles) `.cursor/rules/04-typescript.mdc` (SHA: `5ba2652e3a8d12154b57241c19e07f0d1c83d0ef`)[^4]

This 300+ line rule is the most comprehensive frontend rule in the survey:

- tsconfig snippets: `strict, noImplicitAny, noUncheckedIndexedAccess, exactOptionalPropertyTypes` with rationale for each
- Side-by-side code examples: `Button` with explicit types (correct) vs `React.FC<any>` (wrong)
- State management decision tree: server state -> SWR; local -> useState; global -> minimal Jotai
- Project-specific library rationale: wouter over react-router-dom (8x smaller), SWR over React Query
- Bundle splitting CRITICAL warning: "Keep React + React-DOM together in ONE chunk" prevents `Cannot set properties of undefined (setting 'Children')` — a non-obvious gotcha
- Anti-patterns table: 9 never-use vs 9 always-use patterns
- Quality checklist: 10 items before work is complete[^4]

**Template sketch for a generated repo**:
```
skills/react-typescript/SKILL.md
  - globs: **/*.tsx, **/*.jsx
  - Stack binding: router / fetching / state (from canonical brief)
  - TypeScript strict config with rationale
  - Component pattern (functional-only, explicit return types, no React.FC)
  - State management decision tree
  - Testing (project-specific runner + commands)
  - Bundle splitting gotchas (project-specific Vite config)
  - Anti-patterns table (project-specific)
  - Quality checklist (project CI commands)
```

---

## 3. Web Frontend Design and UI Aesthetics

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/frontend-design/SKILL.md` (SHA cached from session)[^5]

Rather than generic "write clean CSS", this encodes an aesthetic decision framework:
- Opens by naming the problem: "avoid generic AI slop aesthetics"
- Forces a pre-coding phase: choose tone (brutally minimal / maximalist / retro-futuristic) and answer "what makes this unforgettable?"
- Specific anti-patterns: "NEVER use Inter, Roboto, Arial; NEVER use purple gradients on white"
- Covers typography, color theory, motion, spatial composition as procedural directives[^5]

**Template sketch for a generated repo**:
```
skills/ux-design/SKILL.md
  - Design thinking phase BEFORE any code
  - Brand tokens -> references/brand.md (from canonical brief)
  - Typography guide (project fonts)
  - Color system (CSS custom properties)
  - Component library (shadcn/ui or Radix or MUI or custom -- project-specific)
  - Motion guidelines
  - Anti-aesthetics list (project-specific)
  - Accessibility minimum (WCAG level, contrast ratios)
```

---

## 4. Web Artifacts Builder (React + shadcn/ui)

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/web-artifacts-builder/SKILL.md` (SHA: `8b39b19f259b4216ecb07574741dd8eaa9863a07`)[^6]

Treats initialization and bundling as scripted black-box operations:

```
1. bash scripts/init-artifact.sh <project-name>  -> creates Vite+React+shadcn project
2. Edit generated files
3. bash scripts/bundle-artifact.sh               -> inlines all assets into one HTML
4. Share bundle.html
5. (Optional) Test with Playwright
```

Step 5 is explicitly optional with stated rationale: "avoid adding latency before user sees the artifact." The skill lists exactly what the init script installs (40+ shadcn components, path aliases, Radix deps).[^6]

**Template sketch for a generated repo**:
```
skills/scaffold-feature/SKILL.md
  - Phase 1: Clarify (feature, location, required tests)
  - Phase 2: Run scaffold command or manual steps
  - Phase 3: Implement -> link to stack-standards skill
  - Phase 4: Test -> project test suite command
  - Phase 5: PR prep (commit convention, PR template)
scripts/scaffold.sh
```

---

## 5. Visual Theming and Design Systems

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/theme-factory/SKILL.md` (SHA: `90dfceaf2ecdc191a4dcfb0069768a9560638998`)[^7]

Short, narrow, deterministic: show showcase -> user selects -> apply from `themes/` directory. Names 10 themes with personality descriptions. Includes fallback for custom theme creation.[^7]

Demonstrates that a short skill with clear narrow scope and a reference directory is sufficient. Not every skill needs 500 lines.

**Template sketch for a generated repo**:
```
skills/design-tokens/SKILL.md
  - Token inventory -> references/tokens.md
  - How to apply a token change (CSS custom property / Tailwind config / JS file)
  - Dark mode procedure
  - Contrast validation command
references/tokens.md   (brand colors, spacing, typography, component tokens)
```

---

## 6. MCP Server Development

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/mcp-builder/SKILL.md` (SHA cached from session)[^8]

Four-phase workflow with phase gates (Research -> Implementation -> Review/Test -> Evaluations):
- Phase 1 requires fetching live MCP spec via WebFetch before writing code
- Tool annotation guidance: readOnlyHint, destructiveHint, idempotentHint, openWorldHint
- Output schema: `outputSchema` + `structuredContent` in responses
- Phase 4: 10 evaluation questions per tool, each independent, read-only, complex, verifiable -- this is built-in quality testing[^8]

**Template sketch for a generated repo**:
```
skills/mcp-development/SKILL.md
  - Phase 1: Study MCP spec + framework docs (TypeScript SDK or Python FastMCP)
  - Phase 2: Tool design (naming, input schema, output schema, annotations)
  - Phase 3: Implementation (shared utilities, pagination, error messages)
  - Phase 4: Testing (MCP Inspector: npx @modelcontextprotocol/inspector)
  - Phase 5: 10-question evaluations per tool
references/typescript-patterns.md
references/python-patterns.md
references/mcp-best-practices.md
```

---

## 7. Document Co-Authoring and Technical Writing

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/doc-coauthoring/SKILL.md` (SHA cached from session)[^9]

3-stage structured workflow:
- Stage 1: 5 meta-context questions -> info dump -> 5-10 numbered clarifying questions -> explicit exit condition ("enough context when you can ask about edge cases, not basics")
- Stage 2: Brainstorm 5-20 options per section -> curate -> gap-check -> draft via `str_replace`
- Stage 3: Fresh-context reader testing to find blind spots[^9]

**Template sketch for a generated repo**:
```
skills/document-writing/SKILL.md
  - Stage 1: Context (meta questions, info dump, clarifying loop)
  - Stage 2: Structure (scaffold placeholders -> section-by-section draft)
  - Stage 3: Review (reader testing, gap check)
  - Output location: docs/<type>/<date>-<slug>.md
references/doc-types.md   (PRD, RFC, ADR, runbook templates)
```

---

## 8. Web Application Testing

**File examined**: [anthropics/skills](https://github.com/anthropics/skills) `skills/webapp-testing/SKILL.md` (SHA cached from session)[^10]

Decision tree as primary navigation:
- Static HTML? -> Read file directly -> write Playwright script
- Dynamic, no server -> run with_server.py helper
- Dynamic, server running -> Reconnaissance-Then-Action: navigate -> networkidle -> screenshot -> identify selectors -> execute

Key pattern: treat `scripts/with_server.py` as a black box ("run --help, don't read source") to prevent context pollution. The name "Reconnaissance-Then-Action" is memorable.[^10]

**Template sketch for a generated repo**:
```
skills/testing/SKILL.md
  - Decision tree: unit / integration / e2e / performance
  - Unit: test framework + runner (project-specific)
  - Integration: setup/teardown pattern, fixtures
  - E2E: Playwright/Cypress procedure, server management
  - Naming + location conventions (project-specific)
  - Coverage gate (project threshold)
references/test-patterns.md
```

---

## 9. Unreal Engine 5

**Files examined**:
- [chongdashu/unreal-mcp](https://github.com/chongdashu/unreal-mcp) `.cursor/rules/unreal.mdc` (SHA: `bcc2fcb09f87868894964a3732cf728be7797e1f`)[^11]
- [emmareynne/unreal_2.5d_starter_docs](https://github.com/emmareynne/unreal_2.5d_starter_docs) `.cursor/rules/unreal.mdc` (SHA: `02844815a47fcd696b752584f785d3c220fc12fc`)[^12]

The first provides: exact UE version (5.5.4) with deprecation caution, complete coordinate system reference (X/Y/Z axis colors, handedness), and a unit table (cm, kg, degrees, m/s, degrees C, N, N*m). A reference skill -- without it the agent guesses coordinate conventions.[^11]

The second provides a learning-guide orientation for newcomers: best practices, incremental steps, plugin awareness.[^12]

Both lack: C++ naming conventions, GameMode/PlayerController architecture, asset organization, performance patterns.

**Template sketch for a generated repo**:
```
skills/unreal-engine/SKILL.md
  - UE version + deprecation warning
  - Coordinate system + unit reference table
  - Naming: AMyActor, UMyComponent, FMyStruct
  - Architecture: GameMode / PlayerController / Character / Component separation
  - Blueprint vs C++ decision guide
  - Asset organization (Content Browser folder structure)
  - Performance: object pooling, avoid tick, LOD
references/ue5-api-gotchas.md
references/blueprint-patterns.md
```

---

## 10. Unity / C# Game Development

**File examined**: Kardx `.cursor/rules/unity.mdc` (from session)[^13]

Covers: project context (Ring-Con tower defense), naming (PascalCase/camelCase), MVC/ScriptableObject/object pooling, Update() avoidance, LOD groups, NUnit testing.

Lacks: code examples, asset pipeline guidance, Input System guidance.

**Template sketch for a generated repo**:
```
skills/unity/SKILL.md
  - globs: *.cs, *.unity, *.prefab
  - Project context: genre, platform, Unity version
  - Naming: PascalCase classes, camelCase private fields
  - Architecture: MVC or ECS (choose one, document it)
  - ScriptableObject patterns (game data, events, channels)
  - Object pooling pattern
  - Input system: new Input System vs legacy (choose one)
  - Performance: avoid Update(), use events, coroutines vs UniTask
  - Testing: NUnit + Test Runner
references/unity-patterns.md
```

---

## 11. Google Cloud Platform (GCP)

**File examined**: [masterpiece-alliances/macc-website](https://github.com/masterpiece-alliances/macc-website) `.cursor/rules/gcp.mdc` (SHA: `2f1a6469addcdce4cbce3e0f8c4c9124f893b55f`)[^14]

7 major sections covering Code Organization, Patterns/Anti-patterns, Performance, Security, Testing, Common Pitfalls, Tooling. Notable:
- Terraform module structure (main.tf, README, environments/, scripts/)
- GCP design patterns: Service Facade, Pub/Sub Fanout, Cloud Functions Chaining, Idempotent Operations
- Anti-patterns: Long-running processes in Cloud Functions (time limits), hardcoded credentials
- IAP for access control, Cloud Secret Manager for secrets[^14]

**Template sketch for a generated repo**:
```
skills/gcp/SKILL.md
  - globs: **/*.tf, **/gcp/**/*.py
  - Project services in use (Cloud Run / Functions / Pub/Sub / etc.)
  - Auth: DefaultGoogleCredentials, service accounts, workload identity
  - Secret management: always Secret Manager
  - Terraform: remote state in Cloud Storage
  - Cost monitoring: budget alerts
  - CI/CD: Cloud Build triggers
references/terraform-modules.md
references/iam-guide.md
```

---

## 12. Firebase

**File examined**: [mrzacsmith/cursor-rules-npm](https://github.com/mrzacsmith/cursor-rules-npm) `.cursor/rules/firebase.mdc` (SHA: `25aad5683114d8428cd8c108b9cffbfdbe7bc84b`)[^15]

Covers: Auth (onAuthStateChanged + React context/Zustand), Firestore (security rules, indexing, query constraints), Storage (signed URLs, file validation), Functions (modular SDK imports for bundle size).[^15]

Lacks: code examples, Firestore data modeling guidance, emulator development workflow.

**Template sketch for a generated repo**:
```
skills/firebase/SKILL.md
  - Project services active (Auth / Firestore / Storage / Functions / Hosting)
  - Auth providers + onAuthStateChanged pattern
  - Firestore data model -> references/data-model.md
  - Security rules overview -> references/security-rules.md
  - Storage: bucket structure + signed URL pattern
  - Functions: cold start, retry, Pub/Sub triggers
  - Emulator: firebase emulators:start + test patterns
references/data-model.md
references/security-rules.md
```

---

## 13. Cloudflare Workers / Edge

**File examined**: [Atyantik/flarekit](https://github.com/Atyantik/flarekit) `.cursor/rules/cloudflare-worker-patterns.mdc` (from session)[^16]

Covers: Worker entry pattern (satisfies ExportedHandler<Env>), D1+Drizzle+WeakMap connection pooling, R2 storage, queue handlers, scheduled cron events, `wrangler dev --persist-to` for local dev, edge-first cold start minimization.[^16]

**Template sketch for a generated repo**:
```
skills/cloudflare/SKILL.md
  - globs: **/workers/**/*.ts, **/wrangler.json
  - Project services: D1 / KV / R2 / Queues / DO / AI
  - Worker entry pattern with ExportedHandler<Env>
  - Env binding types location (worker-configuration.d.ts)
  - Database: D1 + Drizzle ORM setup
  - Queue processing pattern
  - Local dev: wrangler dev --persist-to command
  - Deployment: wrangler deploy + CI/CD
references/hono-patterns.md
references/d1-schema.md
```

---

## 14. FastAPI (Python)

**File examined**: PatrickJS awesome-cursorrules `rules-new/fastapi.mdc` (from session)[^17]

The canonical "weak checklist" example. Every item reads "Use proper X" or "Implement proper Y" with no code, no specific package names, no tradeoffs. This file will generate highly variable output.

**Template sketch for a generated repo** (strong version):
```
skills/fastapi/SKILL.md
  - globs: **/*.py, **/app/**/*.py
  - Project structure: exact layout (app/, models/, routes/, services/, dependencies/)
  - Router: APIRouter with domain prefix and tags
  - Pydantic models: BaseModel, Field validators, response_model
  - Dependency injection: get_db, current_user patterns
  - Auth: which method (JWT/API keys) + exact pattern used in this project
  - Error handling: HTTPException codes + custom exception handlers
  - Testing: pytest + httpx AsyncClient + test database fixture
  - Deployment: uvicorn workers, Dockerfile, health check endpoint
references/api-contracts.md
references/auth-patterns.md
```

---

## 15. Flask (Python)

Public Flask rules are rarer and even thinner than FastAPI. Most are just bullet lists.

**Template sketch for a generated repo**:
```
skills/flask/SKILL.md
  - globs: **/*.py, **/app/**/*.py
  - Application factory: create_app() with config object
  - Blueprint registration: which blueprints exist in this project
  - Extensions: Flask-SQLAlchemy / Flask-Migrate / Flask-Login (project-specific)
  - Error handlers: JSON error response format
  - Testing: Flask test client fixture, test database setup
  - Config: FLASK_ENV, DotEnv, class-based Config
references/blueprints.md
```

---

## 16. Python Best Practices

Standalone Python quality skills are sparse in the public ecosystem; the best are stack-specific.

**Template sketch for a generated repo**:
```
skills/python/SKILL.md
  - globs: **/*.py
  - Python version: must match pyproject.toml / .python-version
  - Type hints: required on all public functions
  - Dataclass vs namedtuple vs TypedDict decision guide
  - Error handling: explicit except clauses, no bare except
  - File I/O: pathlib.Path everywhere, context managers
  - Testing: pytest, fixtures, parametrize, conftest scope
  - Formatting: black + ruff (or project-specified)
  - Anti-patterns: mutable defaults, shadowing builtins, type: ignore without comment
```

---

## 17. Shell / Bash Scripting

**File examined**: [bossjones/til](https://github.com/bossjones/til) `.cursor/rules/proxmox-rules/proxmox-shell-scripting.mdc` (SHA: `f4acec8ce7f3e224a5fe4eb4652511fa7d7b46e2`)[^18]

Uses formal `<rule>` XML with filters, actions, metadata. Provides:
- Domain-specific command vocabulary (qm create, pct create, pvesm add)
- A complete working `<example>` script (validation function, error handling, logging)
- An `<example type="invalid">` block -- most rules omit negative examples[^18]

**Template sketch for a generated repo**:
```
skills/bash/SKILL.md
  - globs: **/*.sh, **/*.bash, **/scripts/**
  - Shebang: #!/usr/bin/env bash (portability)
  - set flags: set -euo pipefail at top of every script
  - Input validation pattern with named variables
  - Error handling: trap ERR + cleanup function
  - Usage() function at top
  - Testing: bats-core if used
  - Complete script template with all sections
  - Anti-patterns: unquoted variables, no error handling, set +e disabling
```

---

## 18. CLI Development (Python: Click / Typer)

No strong CLI-dev skill files found in the survey. Domain is underserved.

**Template sketch for a generated repo** (synthesised from library docs):
```
skills/cli-development/SKILL.md
  - globs: **/cli/**/*.py, **/__main__.py
  - Framework: Typer (type-annotated) vs Click (mature)
  - Command group structure: app = typer.Typer(), sub-commands as modules
  - Annotated[] pattern for Typer v0.12+
  - Rich integration: Console, Progress, Table, Spinner
  - Config: platformdirs for XDG compliance, TOML for user config
  - Output: stdout data, stderr diagnostics, --quiet/--json flags
  - Exit codes: 0 success, 1 error, 2 usage error
  - Testing: CliRunner
references/commands.md   (command tree for this project)
```

---

## 19. TUI Development (Python: Textual)

Very sparse in the public skill/rule ecosystem.

**Template sketch for a generated repo**:
```
skills/tui-development/SKILL.md
  - description: triggers on "TUI", "terminal UI", "Textual", "interactive CLI"
  - Framework: Textual (reactive, CSS-like) vs urwid vs curses
  - App -> Screen -> Widget hierarchy
  - Reactive attributes: reactive() + watch_* methods
  - Textual CSS for layout and colors
  - Event handling: on_button_pressed, on_key, Message/MessagePump
  - Testing: app.run_test() pilot client
references/widget-catalog.md
```

---

## 20. API Design and JSON/OpenAPI Schemas

Standalone schema design skills are underrepresented in the public ecosystem.

**Template sketch for a generated repo**:
```
skills/api-schema/SKILL.md
  - globs: **/*.json, **/*.yaml, **/openapi.yaml
  - Versioning strategy: URL /v1/ vs header (choose one)
  - Naming: plural nouns, kebab-case URLs, camelCase JSON
  - Response envelope: {data, meta, errors} vs flat (choose one)
  - Error schema: RFC 9457 Problem Details
  - Pagination: cursor-based or offset-based (choose one)
  - Auth: exact header names
  - OpenAPI: auto-generated or schema-first?
references/openapi.yaml   (canonical schema)
```

---

## 21. Planning / Brainstorming / Thinking

No direct planning-skill files found in the .opencode/skills/ survey. The closest analogue is Anthropic's doc-coauthoring Stage 1 (context gathering, info dump, clarifying questions). Microsoft's Agents.md encodes "Think Before Coding" as a first-class standing principle.[^19][^20]

**Template sketch for a generated repo**:
```
skills/planning/SKILL.md
  - description: triggers on "plan", "design", "architect", "think through", "brainstorm"
  - Phase 1: Problem framing (goal, not stated solution)
  - Phase 2: Constraint identification (must-be-true, must-not)
  - Phase 3: Option generation (minimum 3 approaches with tradeoffs table)
  - Phase 4: Decision (explicit criteria, recommendation, risks)
  - Phase 5: Plan output (numbered steps, verify-each, blockers surfaced)
  - Output: docs/decisions/<date>-<slug>.md or tickets/
  - Gate: do NOT start implementation until plan is confirmed
```

---

## 22. Prompt Engineering

The current Scafforge `agent-prompt-engineering` package skill handles prompt hardening during the scaffold cycle. The question is whether the generated repo also needs a prompt-crafting skill for ongoing team use.

Anthropic's `skill-creator` SKILL.md contains the best prompt engineering guidance found in the survey:
- "Pushy descriptions" -- descriptions should slightly over-trigger to prevent undertriggering
- Three-level progressive disclosure (metadata -> body -> bundled resources)
- Evaluate with baseline comparison: spawn skill and no-skill runs in the same turn[^21]

Microsoft's Agents.md: "If uncertain, ask. If multiple interpretations exist, present them."[^22]

Gap in Scafforge's current `agent-prompt-engineering` skill: It likely does NOT cover the pushy-description pattern, progressive disclosure architecture, or skill evaluation loops. These belong in both the package skill and a generated-repo meta-skill.

**Template sketch for a generated repo**:
```
skills/prompt-crafting/SKILL.md
  - description: triggers on "write a prompt", "improve this prompt"
  - Trigger optimization: pushy description, explicit DO NOT TRIGGER conditions
  - Structure template: [role][context][task][output format][constraints][examples]
  - Chain-of-thought encoding where reasoning improves quality
  - Negative examples inside the prompt itself
  - Evaluation: 3 realistic inputs, compare baseline vs prompted, iterate
  - Anti-patterns: vague instructions, over-length, missing context, implicit tone
```

---

## 23. Google Gemini / Google AI SDK

**File examined**: [VidAIze/cursor-project-rules](https://github.com/VidAIze/cursor-project-rules) `.cursor/rules/10-google-generative-ai.mdc` (SHA: `53cf17311f2a745e89292a729efccb36aa778d56`)[^23]

This is a context-dump of Google AI API documentation structure -- a reference file, not a procedural skill. Google AI SDK patterns are underserved in the public skill ecosystem compared to OpenAI and Anthropic.

**Template sketch for a generated repo**:
```
skills/google-ai/SKILL.md
  - description: triggers on "Gemini", "Vertex AI", "Google AI"
  - Model selection: Pro / Flash / Ultra tradeoffs
  - Auth: ADC vs API key vs Vertex AI endpoint
  - SDK: google-generativeai (AI Studio) vs vertexai (enterprise)
  - Multimodal: image/audio/video input handling
  - Streaming: generate_content with stream=True
  - Safety settings: thresholds per project policy
references/models.md
```

---

## 24. Local LLM Development (llama.cpp / Ollama)

No strong `.cursor/rules/` or `.opencode/skills/` files found for llama.cpp specifically.

**Template sketch for a generated repo** (synthesised):
```
skills/local-llm/SKILL.md
  - description: triggers on "local model", "llama", "ollama", "llama.cpp", "offline"
  - Runtime: Ollama (easiest, REST API) vs llama-cpp-python vs vLLM (production)
  - Ollama pattern: requests.post("http://localhost:11434/api/generate", ...)
  - OpenAI compatibility: Ollama /v1/ endpoints (drop-in replacement)
  - llama-cpp-python: Llama() key params (n_gpu_layers, n_ctx)
  - Model files: GGUF format, quantization guide (Q4_K_M for balance)
  - Structured output: grammar files or Pydantic constrained generation
references/model-registry.md   (approved models and GGUF paths)
```

---

## 25. Skill Authoring (Meta-Skill)

**File examined**: [z3z1ma/agent-loom](https://github.com/z3z1ma/agent-loom) `.opencode/skills/skill-authoring/SKILL.md` (SHA: `ae9f5528d396b7ed35fd1a6569efb104edef7073`)[^24]

Four quality criteria: Specific, Procedural, Reusable, Low ceremony.
Naming regex: `^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$` shown explicitly.
"No hand-wavy steps like 'just fix it' -- if a step requires a command, include the command."
A `## Manual notes` section preserved across updates.[^24]

**Template sketch for a generated repo**:
```
skills/skill-authoring/SKILL.md
  - description: triggers on "create a skill", "write a skill", "add a skill"
  - Quality checklist: specific, procedural, reusable, low ceremony
  - Structure: name, description (triggers + non-triggers), preconditions, steps, examples, gotchas
  - Naming regex + examples
  - Location: .opencode/skills/<name>/SKILL.md
  - Progressive disclosure: body < 300 lines; references/ for deep content
  - After writing: test with 2-3 realistic prompts, iterate
  - Update vs create decision (prefer update for improvements)
```

---

## Cross-Domain Patterns: What the Strongest Skills Share

| Pattern | Description | Strongest examples |
|---|---|---|
| Trigger precision | description names exact phrases/imports + explicit DO NOT TRIGGER | claude-api[^3] |
| Decision trees before action | Multiple-choice decision mapped to specific procedures | claude-api, react-typescript, webapp-testing[^3][^4][^10] |
| Procedural phases with verification | Named phases with exit conditions | mcp-builder, doc-coauthoring[^8][^9] |
| Negative examples | Explicit wrong patterns alongside correct patterns | react-typescript, proxmox-shell[^4][^18] |
| Progressive disclosure via references | Body stays short; deep content in references/ | claude-api reading guide, mcp-builder[^3][^8] |

---

## Current Scafforge Output Pack Assessment

| Skill | Trigger quality | Has phases | Anti-patterns | References | Overall |
|---|---|---|---|---|---|
| project-context | Good | N/A | No | No | Tier 2 |
| ticket-execution | Good | Partial | No | No | Tier 2 |
| review-audit-bridge | Good | Yes | Partial | No | Tier 2 |
| workflow-observability | Medium | Yes | No | 1 yaml | Tier 2 |
| stack-standards | Placeholder | No | No | No | Tier 0 |
| repo-navigation | Weak | No | No | No | Tier 1 |
| docs-and-handoff | Medium | No | No | No | Tier 1 |
| research-delegation | Weak | No | No | No | Tier 1 |
| local-git-specialist | Medium | No | No | No | Tier 1 |
| isolation-guidance | Medium | No | No | No | Tier 1 |

Zero domain/stack skills. Zero references directories. Zero examples or anti-patterns. A React+TypeScript+FastAPI+Firebase project gets the same skill pack as a Unity game or a Cloudflare Worker project.[^1][^2]

---

## Recommended Skill Catalog for Generated Repos

**Tier A -- Core (always generated)**
- project-context (keep, add explicit triggers)
- ticket-execution (keep, add anti-patterns)
- review-audit-bridge (keep, add references/security-checklist.md and references/language-specifics.md)
- workflow-observability (keep)
- planning (ADD -- structured problem framing before implementation)
- skill-authoring (ADD -- meta-skill for creating/updating project skills)

**Tier B -- Stack pack (generated from canonical brief)**
- [frontend] -- React+TS or Vue or Svelte
- [backend] -- FastAPI or Flask or Express
- [testing] -- Vitest or pytest or Jest
- [database] -- Firestore or Postgres or DynamoDB
- [deployment] -- Vercel or Cloud Run or Workers or ECS

**Tier C -- Platform packs (activated when platform detected)**
- gcp, firebase, cloudflare, or aws
- llm-integration (when AI features present)

**Tier D -- Optional domain packs**
- unity or unreal-engine (game projects)
- cli-development or tui-development (terminal tool projects)
- api-schema (API-first projects)
- bash (DevOps/infrastructure)
- document-writing (all projects benefit)
- prompt-crafting (AI-feature projects)
- local-llm (offline AI projects)

---

## Confidence Assessment

**High confidence** (verified from specific file content with SHAs):
Sections 1-13, 17, 22, 25 are drawn from actual retrieved files.
The five cross-domain patterns are derived from multiple independent files.
The generated-repo skill assessment reflects direct inspection of scaffold output.

**Medium confidence** (synthesised):
Sections 15 (Flask), 18 (CLI), 19 (TUI), 24 (llama.cpp) -- no strong public skill files found; templates from library docs.
Section 21 (Planning) -- no direct file found; structure inferred from doc-coauthoring Stage 1.

**Lower confidence**:
Minimax and ChatGPT app patterns specifically were not found; would fall under llm-integration references.
Flask is even less well-supported than FastAPI in the public skill ecosystem.

---

## Key Repositories Summary

| Repository | Domain | Key File |
|---|---|---|
| [anthropics/skills](https://github.com/anthropics/skills) | AI SDK, Frontend Design, Testing, Writing, MCP | claude-api, frontend-design, mcp-builder, webapp-testing, doc-coauthoring[^3][^5][^8][^9][^10] |
| [cheshirecode/dotfiles](https://github.com/cheshirecode/dotfiles) | React + TypeScript | 04-typescript.mdc[^4] |
| [chongdashu/unreal-mcp](https://github.com/chongdashu/unreal-mcp) | Unreal Engine 5 | unreal.mdc[^11] |
| [emmareynne/unreal_2.5d_starter_docs](https://github.com/emmareynne/unreal_2.5d_starter_docs) | UE5 learning guide | unreal.mdc[^12] |
| [dustland/Kardx](https://github.com/dustland/Kardx) | Unity / C# | unity.mdc[^13] |
| [masterpiece-alliances/macc-website](https://github.com/masterpiece-alliances/macc-website) | GCP / Terraform | gcp.mdc[^14] |
| [mrzacsmith/cursor-rules-npm](https://github.com/mrzacsmith/cursor-rules-npm) | Firebase | firebase.mdc[^15] |
| [Atyantik/flarekit](https://github.com/Atyantik/flarekit) | Cloudflare Workers | cloudflare-worker-patterns.mdc[^16] |
| [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules) | FastAPI, React (thin checklists) | fastapi.mdc[^17] |
| [bossjones/til](https://github.com/bossjones/til) | Shell / Bash | proxmox-shell-scripting.mdc[^18] |
| [z3z1ma/agent-loom](https://github.com/z3z1ma/agent-loom) | Skill authoring meta-skill | skill-authoring/SKILL.md[^24] |
| [VidAIze/cursor-project-rules](https://github.com/VidAIze/cursor-project-rules) | Google Gemini AI SDK | 10-google-generative-ai.mdc[^23] |

---

## Footnotes

[^1]: `/home/a/Scafforge/skills/project-skill-bootstrap/references/local-skill-catalog.md:1-69` -- current output skill catalog.
[^2]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/` -- all template skills confirmed SKILL.md only, no references/ or scripts/ subdirectories.
[^3]: [anthropics/skills](https://github.com/anthropics/skills), `skills/claude-api/SKILL.md` (SHA: 0fa842997c9e8c447151abced6abb0c4c6fb1b7f).
[^4]: [cheshirecode/dotfiles](https://github.com/cheshirecode/dotfiles), `.cursor/rules/04-typescript.mdc` (SHA: 5ba2652e3a8d12154b57241c19e07f0d1c83d0ef).
[^5]: [anthropics/skills](https://github.com/anthropics/skills), `skills/frontend-design/SKILL.md` -- cached from session.
[^6]: [anthropics/skills](https://github.com/anthropics/skills), `skills/web-artifacts-builder/SKILL.md` (SHA: 8b39b19f259b4216ecb07574741dd8eaa9863a07).
[^7]: [anthropics/skills](https://github.com/anthropics/skills), `skills/theme-factory/SKILL.md` (SHA: 90dfceaf2ecdc191a4dcfb0069768a9560638998).
[^8]: [anthropics/skills](https://github.com/anthropics/skills), `skills/mcp-builder/SKILL.md` -- cached from session.
[^9]: [anthropics/skills](https://github.com/anthropics/skills), `skills/doc-coauthoring/SKILL.md` -- cached from session.
[^10]: [anthropics/skills](https://github.com/anthropics/skills), `skills/webapp-testing/SKILL.md` -- cached from session.
[^11]: [chongdashu/unreal-mcp](https://github.com/chongdashu/unreal-mcp), `.cursor/rules/unreal.mdc` (SHA: bcc2fcb09f87868894964a3732cf728be7797e1f).
[^12]: [emmareynne/unreal_2.5d_starter_docs](https://github.com/emmareynne/unreal_2.5d_starter_docs), `.cursor/rules/unreal.mdc` (SHA: 02844815a47fcd696b752584f785d3c220fc12fc).
[^13]: [dustland/Kardx](https://github.com/dustland/Kardx), `.cursor/rules/unity.mdc` -- cached from session.
[^14]: [masterpiece-alliances/macc-website](https://github.com/masterpiece-alliances/macc-website), `.cursor/rules/gcp.mdc` (SHA: 2f1a6469addcdce4cbce3e0f8c4c9124f893b55f).
[^15]: [mrzacsmith/cursor-rules-npm](https://github.com/mrzacsmith/cursor-rules-npm), `.cursor/rules/firebase.mdc` (SHA: 25aad5683114d8428cd8c108b9cffbfdbe7bc84b).
[^16]: [Atyantik/flarekit](https://github.com/Atyantik/flarekit), `.cursor/rules/cloudflare-worker-patterns.mdc` -- cached from session.
[^17]: [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules), `rules-new/fastapi.mdc` -- cached from session.
[^18]: [bossjones/til](https://github.com/bossjones/til), `.cursor/rules/proxmox-rules/proxmox-shell-scripting.mdc` (SHA: f4acec8ce7f3e224a5fe4eb4652511fa7d7b46e2).
[^19]: [anthropics/skills](https://github.com/anthropics/skills), `skills/doc-coauthoring/SKILL.md:28-101` -- Stage 1 as planning pattern.
[^20]: [microsoft/skills](https://github.com/microsoft/skills), `Agents.md:30-37` -- "Think Before Coding" standing principle.
[^21]: [anthropics/skills](https://github.com/anthropics/skills), `skills/skill-creator/SKILL.md:65-68` -- pushy description principle.
[^22]: [microsoft/skills](https://github.com/microsoft/skills), `Agents.md:26-88` -- engineering principles as standing instructions.
[^23]: [VidAIze/cursor-project-rules](https://github.com/VidAIze/cursor-project-rules), `.cursor/rules/10-google-generative-ai.mdc` (SHA: 53cf17311f2a745e89292a729efccb36aa778d56).
[^24]: [z3z1ma/agent-loom](https://github.com/z3z1ma/agent-loom), `.opencode/skills/skill-authoring/SKILL.md` (SHA: ae9f5528d396b7ed35fd1a6569efb104edef7073).
