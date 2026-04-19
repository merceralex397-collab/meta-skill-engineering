# Research Report #2: Specific Skill Examples Deep-Dive

**Status:** Complete  
**Purpose:** Companion to Report #1. Where Report #1 established the landscape and gap analysis, this report goes domain-by-domain, surfacing real skill file excerpts and making explicit quality judgements. The goal is to show what a high-quality generated-repo skill looks like in practice, and to identify what Scafforge is missing, weak in, or mis-scoped.

---

## Executive Summary

After examining 50+ specific skill and rules files across 20+ domains, the picture is clear: the majority of the public ecosystem — cursor rules, agent instructions, SKILL.md files — is **Tier 1 or 2 quality** (generic checklists or shallow domain stubs). The exemplars worth emulating are concentrated in a small number of sources: `anthropics/skills`, `different-ai/openwork`, a few high-quality cursor-rules collections, and the `openclaw/skills` prompt engineering skill. The key differentiators are: explicit triggers, staged executable workflows, bundled references or code, domain depth, and self-review/verification loops.

Scafforge's current generated-repo skill pack suffers most from:
1. **Missing domain skills entirely** — no Python, no React, no FastAPI, no game engine, no LLM app skills
2. **`stack-standards` is a literal placeholder** — the most important domain-specific skill is a 3-line stub
3. **No verification or self-review mechanisms** in any output skill
4. **Governance skills are well-formed; domain skills are absent** — the inverse of what generated repos need most often

---

## Part 1: Domain-by-Domain Skill Survey

### 1. Prompt Engineering

**Source examined:** `openclaw/skills` — `skill-with-prompt-engineering/SKILL.md`

**Quality rating:** ★★★★☆ (Tier 3 — full workflow + reference framework)

**What it does well:**

The skill encodes a complete 16-technique prompt engineering framework directly into the SKILL.md. It operates in two explicit modes (Mode 1: create a prompt; Mode 2: build a SKILL.md file) and makes the trigger condition unambiguous:

> "Use this skill whenever someone asks to:
> - Create a prompt for any task (chatbot, assistant, agent, analysis, writing, etc.)
> - Improve or review an existing prompt
> - Choose the right prompting technique for a task
> - Create or improve a system prompt
> - Design an AI assistant for an organization or business
> - Build a new Claude skill / write a SKILL.md
> - 'Make Claude always do X'"

Each mode has a numbered, sequential step flow. Critically, both modes include a **self-review with iterative refinement loop**:

> "### Step 4 — Self-Review with ReAct + Iterative Refinement
> After drafting, do NOT send to the user immediately. Run 2-3 review rounds using these criteria:
> - Is the AI role defined clearly enough?
> - Is there enough context for the AI to work without guessing?
> - Are any instructions ambiguous or open to multiple interpretations?
> - Are there 'must not do' rules covering likely failure cases?
> - Is the output format clear enough?"

The skill also carries the complete 16-technique reference (Zero-shot, Few-shot, Chain of Thought, Tree of Thought, ReAct, Meta Prompting, Iterative Refinement, etc.) directly inside the SKILL.md. This is a bundled reference library inside a single file — progressive disclosure without needing external resources.

**Contrast with Scafforge's `agent-prompt-engineering` skill:**

Scafforge's own `agent-prompt-engineering` is a **Scafforge package skill** (not a generated-repo skill), but it's worth comparing:

- Scafforge's skill is workflow-oriented (6 steps: analyze, apply contracts, remove anti-patterns, apply model-specific techniques, weak-model hardening, verify)
- It correctly references external `references/` files (`prompt-contracts.md`, `anti-patterns.md`, `weak-model-profile.md`)
- It addresses agent-coordination prompt problems specifically, which the Gen AI Space skill does not
- Missing: self-review loop, bundled technique taxonomy, trigger specificity for "do I use this skill?"

**What a generated-repo prompt engineering skill should have:**
- Mode 1: write a system prompt for a new agent
- Mode 2: audit and harden an existing agent prompt
- Explicit trigger conditions (when to activate, what requests qualify)
- Bundled mini-reference for the top 6-8 most relevant prompt engineering patterns
- A self-review checklist (ambiguity check, scope check, delegation boundary check, stop-condition check)

---

### 2. OpenCode Integration Skills

**Sources examined:**
- `different-ai/openwork` — `opencode-bridge/SKILL.md`
- `different-ai/openwork` — `opencode-primitives/SKILL.md`
- `different-ai/openwork` — `solidjs-patterns/SKILL.md`
- `different-ai/openwork` — `tauri-solidjs/SKILL.md`
- `different-ai/openwork` — `cargo-lock-manager/SKILL.md`
- `different-ai/openwork` — `release/SKILL.md`

**Quality range:** ★★★☆☆ — ★★★★★ (Tier 2–4)

**opencode-bridge** is Tier 3: it documents the three communication channels between OpenWork UI and OpenCode runtime (CLI invocation, SQLite database access, MCP bridge), includes exact SQL schema, Rust and SolidJS code snippets for querying, and flags gotchas:

> "**Common Gotchas**
> - Database is SQLite; use read-only access to avoid conflicts with running OpenCode.
> - Message parts are JSON-encoded strings; parse them in the UI.
> - Session IDs are UUIDs; tool call IDs are also UUIDs.
> - Cost is in USD; tokens are raw counts."

**solidjs-patterns** is Tier 4: it identifies a specific class of bugs ("'UI stuck' bugs are actually state coupling bugs") and provides the exact code fix with reasoning:

> "A single `busy()` boolean creates deadlocks:
> - Long-running task sets `busy(true)`
> - A permission prompt appears and its buttons are disabled by `busy()`
> - The task can't continue until permission is answered
> - The user can't answer because buttons are disabled
>
> Fix: permission UI must be disabled only by a **permission-specific** pending state."

It also includes a **practical checklist** for any SolidJS UI change:
> - Does any button depend on a global flag that could be true during long-running work?
> - Could two async actions overlap and fight over one boolean?
> - Is any UI state duplicated (can be derived instead)?

**cargo-lock-manager** is Tier 4 — a micro-skill so specific it names exact error messages as triggers:

> "Triggers when user mentions:
> - 'cargo test --locked failed'
> - 'cannot update the lock file'
> - 'Cargo.lock is out of date'
> - 'PR failed with --locked error'"

**Key lesson:** The best OpenWork skills don't document the whole framework — they identify the specific error class, the wrong mental model most developers have, and the right fix. This is domain depth.

**tauri-solidjs** is Tier 2 — well-structured but primarily a reference card (project structure, dependencies, code snippets). No reasoning about why patterns exist, no pitfalls beyond gotchas at the bottom.

**cargo-lock-manager and release** together illustrate a key design principle: **operational skills** (how to do a specific recurring task) and **domain skills** (how the framework works) are different skill shapes and should be kept separate.

---

### 3. Frontend Design and UI/UX

**Sources examined:**
- `anthropics/skills` — `frontend-design/SKILL.md`
- `tailwind-react-firebase-cursorrules-prompt-file/general-ui-ux-rules.mdc`
- `tailwind-react-firebase-cursorrules-prompt-file/firebase-rules.mdc`
- `lewkamtao/lew-ui` — `.cursor/rules/base.mdc`, `component-structure.mdc`

**Quality range:** ★★☆☆☆ — ★★★★☆

**`anthropics/skills` frontend-design** is Tier 3+: it couples strong aesthetic position (anti-AI-slop, bold design direction) with concrete implementation rules (Tailwind v4, CSS cascade layers, explicit brand color system). Its key strength is telling the model *what not to do*:

> "Anti-patterns:
> - Generic 'starter pack' CSS with predictable colors and layouts
> - Bland gradients (blue-to-purple) or default Tailwind colors without customization
> - The same card/button/form patterns seen everywhere
> - Safe, neutral design choices when bold choices are appropriate"

**general-ui-ux-rules.mdc** (PatrickJS/awesome-cursorrules) is Tier 1: a correct checklist but no reasoning, no trigger conditions, no weighting between rules. Example:

> "- Always design and implement for mobile screens first, then scale up to larger screens.
> - Use Tailwind's responsive prefixes (sm:, md:, lg:, xl:) to adjust layouts for different screen sizes.
> - Create a design system with consistent colors, typography, spacing, and component styles."

Three rules of equal weight covering entirely different concerns. No guidance on when to apply which. An agent reading this gets no help deciding what matters in a given situation.

**firebase-rules.mdc** is Tier 0 — three bullet points:
> "- Implement proper security rules in Firebase.
> - Use Firebase SDK's offline persistence for better performance and offline support.
> - Optimize queries to minimize read/write operations."

This is so sparse it provides negative value — it implies Firebase is covered when it completely isn't.

**What a strong UI/UX skill should have:**
- A stated design philosophy (what aesthetic point of view is this project pursuing?)
- Anti-patterns specific to the chosen framework
- A decision tree for layout choices (when to use grid vs flex vs absolute)
- Accessibility mandates, not suggestions
- A checklist for reviewing a new component before calling it done

---

### 4. React / TypeScript

**Sources examined:**
- `anthropics/skills` — `web-artifacts-builder/SKILL.md`
- `different-ai/openwork` — `solidjs-patterns/SKILL.md` (see §2)
- `PatrickJS/awesome-cursorrules` — `react-typescript-nextjs-cursorrules-prompt-file`
- `lewkamtao/lew-ui` — `component-structure.mdc`

**Quality range:** ★★☆☆☆ — ★★★★☆

**web-artifacts-builder** (anthropics) is Tier 4 for its domain: it defines the stack exactly (React 18, TypeScript, Vite, Tailwind CSS 3, shadcn/ui) and gives the full artifact shape:

> "Build everything into a single self-contained HTML file that can run in a Claude artifact viewer.
> - Inline all styles and scripts
> - Use CDN links (unpkg, esm.sh, cdn.jsdelivr.net)
> - No build step required"

It also has explicit anti-AI-slop guidance:
> "Always strive for 'delightful and functional' rather than 'technically correct'. AI-generated UIs often feel mechanical — prefer intentional visual hierarchy, thoughtful whitespace, and purposeful color usage."

**Key differentiator:** The skill tells the model exactly how to bundle the artifact. This is mechanical procedure, not vague advice.

**react cursorrules** from awesome-cursorrules are consistently Tier 1 — they enumerate conventions but don't explain *why* those conventions exist or *when* to deviate. A model following them will produce technically correct but context-free React code.

**What a strong React/TypeScript generated-repo skill should include:**
- State management strategy: when to use local state vs context vs external store vs URL state
- Component anatomy template: props interface → render → hooks → side effects
- Testing strategy: unit vs integration vs e2e decision tree
- Import/export conventions (named vs default, barrel files yes/no)
- File naming and co-location rules

---

### 5. Python, FastAPI, Flask, LLM/ML

**Sources examined:**
- `PatrickJS/awesome-cursorrules` — `python-fastapi-best-practices-cursorrules-prompt-f/.cursorrules`
- `PatrickJS/awesome-cursorrules` — `python-llm-ml-workflow-cursorrules-prompt-file/.cursorrules`
- `PatrickJS/awesome-cursorrules` — multiple individual `.mdc` files from `python-llm-ml-workflow`

**Quality range:** ★★☆☆☆ — ★★★☆☆

**python-fastapi-best-practices** is Tier 2: it correctly identifies key idioms (RORO pattern, Pydantic v2, async operations, lifespan context managers over on_event) and names the right tools (asyncpg, SQLAlchemy 2.0). The explicit naming is valuable:

> "Use the Receive an Object, Return an Object (RORO) pattern.
> Use def for pure functions and async def for asynchronous operations.
> Use type hints for all function signatures.
> Prefer Pydantic models over raw dictionaries for input validation."

However it's still essentially a prose dump. No workflow, no decision trees, no "when do I use HTTPException vs middleware for error handling?".

**python-llm-ml-workflow** is Tier 3: the technology stack declaration is exemplary:
```
Python Version: Python 3.10+
Dependency Management: Poetry / Rye
Code Formatting: Ruff (replaces black, isort, flake8)
LLM Framework: langchain, transformers
Vector Database: faiss, chroma (optional)
Experiment Tracking: mlflow, tensorboard (optional)
```

And the ML-specific guidelines go beyond generic Python:
> "**LLM Prompt Engineering:** Dedicate a module or files for managing Prompt templates with version control.
> **Context Handling:** Implement efficient context management for conversations, using suitable data structures like deques.
> **Experiment Configuration:** Use hydra or yaml for clear and reproducible experiment configurations.
> **Data Pipeline Management:** Employ scripts or tools like dvc to manage data preprocessing and ensure reproducibility."

**The individual .mdc files** (e.g., `data-validation-with-pydantic.mdc`, `uv-dependency-management.mdc`, `llm-prompt-engineering.mdc`) are each 100–300 bytes — micro-rules that are extremely narrow and context-sparse on their own. This decomposition pattern is intentional for Cursor (which loads rules by glob pattern) but doesn't translate directly to SKILL.md format.

**Lesson:** There is no single strong Python/FastAPI skills reference. The best approach is to synthesize from multiple sources into a single project-specific skill. That synthesis is exactly what `project-skill-bootstrap` is supposed to do — and currently doesn't because `stack-standards` is a placeholder.

---

### 6. Firebase and Google Cloud

**Sources examined:**
- `tailwind-react-firebase-cursorrules-prompt-file/firebase-rules.mdc` (examined in §3)
- `invertase/react-native-firebase` `.cursor/rules/ai/porting-workflow.md` (known from Report #1)
- `Atyantik/flarekit` Cloudflare Worker patterns (known from Report #1)

**Finding:** No high-quality standalone Firebase or Google Cloud SKILL.md files were found. The best Firebase guidance is embedded in stack-specific rules collections (like the React+Firebase ones above), not in dedicated skills.

This represents a real gap in the public ecosystem. Firebase has distinct concerns (Security Rules, offline persistence, real-time listeners, Auth flows, Cloud Functions, Firestore vs Realtime Database tradeoffs) that warrant a dedicated skill but none exists in the SKILL.md format.

**What a strong Firebase skill would cover:**
- Security Rules authoring: `request.auth` patterns, field-level access, recursive document access
- Read-cost management: denormalization strategy, composite index guidelines
- Offline-first patterns: `enablePersistence`, optimistic writes, conflict resolution
- Auth flows: Sign-in methods, token refresh, custom claims
- Cloud Functions: triggers (onCreate, onUpdate, onCall), cold start mitigation, retry safety

**What a strong Google Cloud skill would cover:**
- Service selection: when to use Cloud Run vs Cloud Functions vs GKE
- IAM patterns: principle of least privilege, service accounts vs user accounts
- Secret Manager integration
- Cloud Build / deployment pipeline basics

---

### 7. Cloudflare / Edge Workers

**Source examined:** `Atyantik/flarekit` — `.cursor/rules/cloudflare-worker-patterns.mdc` (from Report #1)

**Quality rating:** ★★★☆☆ (Tier 3)

This rule covers the Cloudflare primitives that matter: D1 (SQLite at edge), R2 (object storage), Queue, Scheduled Workers (cron), and the edge-specific mental model. Key quality signal — it states *when to use each primitive*:

> "Use D1 for relational data that needs SQL queries. R2 for large files/blobs. Queue for background tasks. Scheduled Workers for periodic work."

And it flags the critical edge constraint:
> "Remember: Workers are stateless, 128MB RAM limit, 50ms CPU time limit. No Node.js native modules."

This level of constraint documentation is what makes a skill useful for code generation. Without knowing the 50ms CPU limit, a model will generate code that works locally but fails in production.

**What's missing:**
- Wrangler CLI workflow (how to develop, preview, deploy)
- Wasm integration patterns
- Durable Objects for stateful coordination
- KV vs D1 vs Durable Objects selection guidance

---

### 8. Unity (C#)

**Sources examined:**
- `PatrickJS/awesome-cursorrules` — `unity-cursor-ai-c-cursorrules-prompt-file/.cursorrules`
- `dustland/Kardx` — `.cursor/rules/unity.mdc` (from Report #1)

**Quality range:** ★★☆☆☆ — ★★★★☆

The PatrickJS Unity cursorrules is minimal:
```
// Unity Tower Defense Game using Nintendo Ringcon
// Language: C#
// Unity Version: 2021.3.18f1
// Instructions
// Ensure the game mechanics are intuitive and responsive.
// Focus on optimizing performance for real-time gameplay.
// Implement modular code structure for easy updates and feature additions.
```

This is project-specific context given as a rule file — Tier 0.

The `dustland/Kardx` Unity rule is far stronger (Tier 3):
> "Design patterns required: MVC for UI (no MonoBehaviours in UI logic), ScriptableObject for game data, object pooling for frequently spawned objects.
> Performance: Cache GetComponent calls, avoid string-based access, prefer direct references. Avoid Update() loops for event-driven logic."

And it includes anti-patterns:
> "Do not use FindObjectOfType at runtime. Do not use Camera.main in Update(). Do not use SendMessage."

**What a strong Unity skill should include:**
- Architecture pattern for the project type (mobile vs PC vs VR)
- Performance budget: CPU/GPU frame budget, what profiler thresholds trigger investigation
- Asset naming convention
- ScriptableObject event system for decoupled communication
- Coroutines vs async/await for Unity decision rule
- Common Unity-specific bugs to avoid (Destroy vs DestroyImmediate, null check on Unity objects)

---

### 9. Unreal Engine 5

**Source examined:** `DWFullen/UE5StackLearning` — `agents/instructions/ue5-blueprint.md`

**Quality rating:** ★★★★☆ (Tier 3–4)

This is one of the strongest game-engine agent instruction files found. It includes:
- A Blueprint types table (7 types with "when to use")
- Naming convention tables with prefix rules
- Variable best practices (numbered, specific)
- Graph organization rules
- Three communication pattern patterns (Direct Reference, Blueprint Interface, Event Dispatcher) with actual Blueprint pseudocode
- Animation Blueprint state machine structure
- UMG Widget binding patterns
- Performance guidelines with concrete rules
- A debugging techniques table
- Common pitfalls table with solutions

The pitfalls table is the standout:

| Pitfall | Solution |
|---------|---------|
| Object reference is not valid | Always check `Is Valid` before using references |
| Blueprint not updating after C++ change | Recompile Blueprint (Compile button) |
| Tick running when not needed | Uncheck `Start with Tick Enabled` in Class Defaults |
| Cast failing silently | The exec pin on Cast's "Cast Failed" output will fire — handle it |
| Event Dispatcher not firing | Ensure `Bind` is called before `Call` |

This "known pitfalls with exact solutions" format is highly reusable and gives a weak model clear guardrails against the five most common Unreal mistakes.

**Missing:** C++ interoperability depth, packaging/deployment, multi-platform target rules, linting/code review conventions.

---

### 10. MCP (Model Context Protocol) Servers

**Source examined:** `anthropics/skills` — `mcp-builder/SKILL.md` (from Report #1)

**Quality rating:** ★★★★☆ (Tier 3+)

This is the gold standard for tool-building skills. It encodes the full MCP development lifecycle:
1. Research phase (check existing servers, understand tools needed)
2. Implement phase (MCP Python SDK or TypeScript SDK, tool registration, annotations)
3. Review/test phase (run locally, iterate)
4. Evals phase (automated test scenarios)

The tool naming rules are the most valuable section:
> "Tool names should be verb-first (e.g., `get_file`, `list_repos`, `search_code`).
> Always include `annotations` on every tool: `readOnlyHint`, `destructiveHint`, `idempotentHint`, `openWorldHint`.
> Annotations are contract signals — they tell the host what kind of operation this is."

And the implementation rules prevent common mistakes:
> "Prefer returning structured data (dicts/objects) over plain strings so hosts can display it usefully.
> Always handle errors gracefully — return error content, not exceptions, so the model can reason about failure."

**What's missing:** No coverage of MCP transports (stdio vs HTTP/SSE vs WebSocket), no guidance on authentication patterns for hosted MCP servers.

---

### 11. API Design and Schemas

**Sources examined:**
- `PatrickJS/awesome-cursorrules` — FastAPI rules (see §5)
- General API design patterns from REST/GraphQL rules

**Finding:** No strong standalone API schema or OpenAPI specification SKILL.md was found. The existing FastAPI rules cover implementation but not design. API schema quality (versioning strategy, pagination patterns, error envelope format, authentication flow design) has no strong equivalent in the SKILL.md ecosystem.

**What a strong API/Schema skill would include:**
- Resource naming conventions (plural nouns, hierarchy depth limit)
- OpenAPI spec-first vs code-first decision rule
- Error envelope standard (RFC 7807 problem details format)
- Versioning strategy (path versioning vs header versioning)
- Pagination patterns: cursor vs offset — when to use each
- Auth flow documentation: OAuth2 scopes, JWT claims, API key rotation

---

### 12. Planning / Brainstorming / Thinking Skills

**Sources examined:**
- `openclaw/skills` — `skill-with-prompt-engineering` (see §1, Mode 1 is essentially a planning/thinking skill)
- `danielmiessler/Personal_AI_Infrastructure` — multiple reasoning files

**Quality range:** ★★☆☆☆ — ★★★☆☆

The Gen AI Space skill's Mode 1 workflow is instructive for planning skills:
1. Analyze the use case
2. Ask for missing information (with a defined template)
3. Build the plan
4. Self-review with iterative refinement
5. Explain the design

This maps well to a planning skill. The key principle is **information gathering before action** — the skill does not start planning until it has: goals, constraints, audience, background context, and output format.

**What a strong planning/brainstorming skill would have:**
- Trigger: any request to "plan", "think through", "brainstorm", "design approach for"
- Information gathering step: 5-item intake (goal, constraints, unknowns, audience, artifact type)
- Explicit thinking modes: divergent (generate options) vs convergent (select + justify)
- Anti-pattern: jumping to implementation before constraints are surfaced
- Output format: decision log with rejected options documented
- Stop condition: a plan is not done until unknowns are either resolved or explicitly flagged

---

### 13. Document Writing

**Sources examined:**
- `anthropics/skills` — `doc-coauthoring/SKILL.md` (from Report #1)
- `anthropics/skills` — `docx/SKILL.md` (directory seen, not fetched — generates Word files)
- `anthropics/skills` — `pptx/SKILL.md` (directory seen)

**Quality:** `doc-coauthoring` is Tier 3: it has a 3-stage workflow (scope + outline agreement → draft → review cycle). The key strength is the collaborative contract:

> "Stage 1 ends when user approves the outline. Do not begin writing Stage 2 until explicit approval is received."

This is a stop condition — the skill won't proceed until verified. The approval gate prevents wasted work.

The docx and pptx skills are artifact generators (tools calling Python python-docx or python-pptx) — a different shape from a writing guidance skill, but equally valuable as an example of skills that wrap tooling procedures.

**What a strong document writing skill would include:**
- Document type classification: spec vs design doc vs runbook vs ADR vs RFC
- For each type: required sections, target audience, expected length, review criteria
- Voice and tone guide: technical docs vs user-facing docs
- The approval gate pattern from doc-coauthoring
- Metadata template: author, date, status, reviewers, decision fields

---

### 14. Code Review and Best Practices by Language

**Sources examined:**
- `invertase/react-native-firebase` — `.cursor/rules/ai/porting-workflow.md` (from Report #1)
- `instructure/instructure-ui` — `.claude/commands/pr.md` (from Report #1)
- Multiple cursor rules covering TypeScript conventions

**Quality range:** ★★★☆☆ — ★★★★☆

The `invertase` porting workflow is Tier 3: it defines a 3-step port workflow with priority categories (breaking changes first, then functionality, then performance) and a quality checklist. The explicit ordering is valuable — a model following it won't optimize premature concerns.

The `instructure-ui` PR workflow is Tier 4 in its domain: it gives exact `gh cli` commands for every step of the PR process. Zero ambiguity.

**Lesson for Scafforge:** A code review skill should not be a list of "good code" principles — it should be a **workflow** with explicit stages, a defined output artifact (structured findings list), and a stop condition (either approve or enumerate blockers). The existing `review-audit-bridge` in the project template is reasonable but lacks the explicit output format required.

---

### 15. CLI and TUI Development

**Finding:** No strong dedicated CLI-building SKILL.md was found in the public ecosystem. The `digi4care/opencode-mastery` has a `svelte-cli` skill but it was not publicly accessible. The `microsoft/skills` library likely contains CLI-related skills but these are internal.

**What a strong CLI skill would include:**
- Framework selection decision tree: click vs typer vs argparse (Python), cobra vs urfave/cli (Go), commander vs yargs vs clack (Node)
- UX principles specific to CLI: progressive disclosure, --help quality standards, flag naming conventions
- Error exit codes and error message formatting
- Interactive mode vs non-interactive mode design
- Completion script generation
- Input validation and sanitization patterns

**TUI specifically:** No TUI SKILL.md was found. TUI frameworks (textual, bubbletea, ratatui, blessed-contrib) are complex enough to warrant dedicated skills. A strong TUI skill would cover: layout model (panels, grids, flexbox analogues), event loop integration, terminal capability detection, mouse vs keyboard navigation.

---

### 16. Bash Scripting

**Finding:** No strong bash scripting SKILL.md or cursor rule with procedural depth was found. Most bash-related rules were peripheral (e.g., "run tests with `npm run test`").

**What a strong bash scripting skill would include:**
- `set -euo pipefail` as mandatory preamble with explanation
- ShellCheck compatibility requirements
- Variable quoting rules (always quote `"$var"`)
- Command substitution: `$(cmd)` vs backticks
- Error handling patterns: trap ERR, local exit codes
- Portability: bash vs sh, macOS vs Linux differences
- Logging pattern: stderr for diagnostics, stdout for output
- Testing: bats-core for unit tests

---

### 17. ChatGPT / OpenAI API Apps

**Sources examined:**
- `anthropics/skills` — `claude-api/SKILL.md` (from Report #1, Claude-specific)
- `python-llm-ml-workflow` (see §5, partially covers LLM apps)

**Finding:** No strong OpenAI-specific skills in SKILL.md format were found in the public space. The most relevant is the Anthropics `claude-api` skill which covers the Claude API — it's an excellent model:

> "Decision tree for API vs Agent SDK:
> - Single-turn completion → Messages API
> - Multi-turn with history → Messages API + manual history management
> - Tool use with orchestration → Agent SDK
> - Streaming required → Messages API with stream=True
> - Batch processing → Batch Messages API"

An OpenAI-equivalent would provide the same decision tree for: Completions API vs Chat API vs Assistants API vs Realtime API vs Batch API, plus the model selection guide (GPT-4o vs o1 vs o3 vs GPT-4o-mini for different use cases).

---

### 18. llama.cpp

**Finding:** No llama.cpp SKILL.md was found. Only a few cursor rules mention llama.cpp peripherally as a library dependency.

**What a llama.cpp skill would cover:**
- Model format: GGUF quantization levels (Q4_K_M vs Q8_0 vs F16 tradeoffs)
- Compilation flags: LLAMA_METAL, LLAMA_CUDA, LLAMA_BLAS
- Context window management: n_ctx, n_batch, rope scaling
- Sampling parameters: temperature, top_p, top_k, repetition_penalty
- Grammar-constrained generation for structured output
- Server mode vs library mode
- Python bindings: llama-cpp-python vs direct C API

---

### 19. Anthropics SKILL.md Collection — Full Inventory

**All 17 skills in `anthropics/skills`:**

| Skill | Type | Quality | What it does |
|-------|------|---------|--------------|
| `algorithmic-art` | Creative/generative | ★★★★☆ | p5.js generative art, seeded randomness, HTML artifact |
| `brand-guidelines` | Configuration | ★★★☆☆ | Stores org brand spec for downstream use |
| `canvas-design` | Design | ★★★☆☆ | 2-step philosophy + PDF/PNG canvas output |
| `claude-api` | Developer | ★★★★☆ | Multi-language SDK support, decision tree for API selection |
| `doc-coauthoring` | Writing | ★★★★☆ | 3-stage collaborative doc workflow with approval gates |
| `docx` | Artifact | ★★★★☆ | Wraps python-docx to generate Word documents |
| `frontend-design` | Design | ★★★★☆ | Design thinking, bold aesthetic, anti-AI-slop |
| `internal-comms` | Writing | ★★★☆☆ | Internal announcement/memo writing |
| `mcp-builder` | Developer | ★★★★★ | Full MCP lifecycle: research, implement, test, evals |
| `pdf` | Artifact | ★★★★☆ | Wraps reportlab/weasyprint to generate PDFs |
| `pptx` | Artifact | ★★★★☆ | Wraps python-pptx to generate PowerPoint |
| `skill-creator` | Meta | ★★★★★ | Full eval loop for creating new skills, progressive disclosure |
| `slack-gif-creator` | Creative | ★★★☆☆ | Pillow/gifski for animated GIFs |
| `theme-factory` | Design | ★★★★☆ | 10 presets, applies to any artifact type |
| `web-artifacts-builder` | Frontend | ★★★★☆ | React+TS+Vite+Tailwind, single HTML output, anti-slop |
| `webapp-testing` | Testing | ★★★★☆ | Playwright, decision tree, helper scripts |
| `xlsx` | Artifact | ★★★★☆ | Wraps openpyxl to generate Excel spreadsheets |

**Pattern:** Anthropics splits skills into three shapes:
1. **Workflow skills** (mcp-builder, doc-coauthoring, webapp-testing) — staged procedures
2. **Artifact skills** (docx, pptx, pdf, xlsx) — tool-wrapping with clear input/output contract
3. **Creative direction skills** (frontend-design, theme-factory, algorithmic-art) — aesthetic opinionation + technique

None of these shapes are represented in Scafforge's current generated-repo skill pack.

---

### 20. Release Engineering

**Source examined:** `different-ai/openwork` — `release/SKILL.md`

**Quality:** ★★★★☆ (Tier 4)

This is the best example of a Tier 4 micro-skill — it has exactly 6 steps, each with exact commands, zero prose padding:

```
## Prepare
Confirm the repo is on `main` and clean.

## Bump
Update versions in package.json files...
pnpm bump:patch / pnpm bump:minor / pnpm bump:major / pnpm bump:set -- 0.1.21

## Merge
Merge the version bump into `main`. Make sure no secrets or credentials are committed.

## Tag
git tag vX.Y.Z
git push origin vX.Y.Z

## Rerun
gh workflow run "Release App" --repo different-ai/openwork -f tag=vX.Y.Z

## Verify
gh run list --workflow "Release App" --limit 5
gh release view vX.Y.Z --repo different-ai/openwork
```

**Key design principle:** Every step produces a verifiable artifact. You can run `gh release view` to confirm the release happened. There is no step that cannot be verified.

---

## Part 2: Scafforge Agent Prompt Engineering — Quality Audit

The Scafforge `agent-prompt-engineering` (package-level skill, not generated-repo) was examined at `/home/a/Scafforge/skills/agent-prompt-engineering/SKILL.md`.

**Assessment: ★★★☆☆ (Tier 2–3)**

**Strengths:**
1. Clear domain: coordination prompts for multi-agent systems — not generic prompt writing
2. Correct 6-step workflow with distinct phases
3. References bundled resources (`prompt-contracts.md`, `anti-patterns.md`, `weak-model-profile.md`)
4. Anti-pattern taxonomy is sophisticated (status-over-evidence routing, impossible read-only delegation)
5. Model-specific technique step is forward-looking and correct

**Weaknesses:**
1. **No trigger conditions** — the description says "Use when prompt wording controls how agents coordinate" but gives no activation examples. Compare to the Gen AI Space skill which lists 9 exact trigger phrases.
2. **No self-review loop** — Step 6 (Final Verification) is 4 questions but no iteration protocol. What happens if one question fails? There's no "go back to step X" instruction.
3. **No before/after examples inline** — it references `references/examples.md` but the examples are not visible in the description header, meaning a cold read of the skill gives no concrete grounding
4. **Weak-model hardening step is too abstract** — "Outputs are short but highly structured" and "Proof is required before stage transitions" are correct principles but don't tell a model how to identify *which specific output* needs restructuring in the prompt it's hardening
5. **No escalation path** — what happens when the prompt cannot be fixed (e.g., the agent design itself is fundamentally broken)? There's no "stop and raise blocker" instruction.

**Comparison to Gen AI Space skill:**

| Dimension | Gen AI Space | Scafforge agent-prompt-engineering |
|-----------|-------------|-----------------------------------|
| Trigger clarity | 9 exact phrases | 1 vague clause |
| Self-review loop | Explicit 3-round iteration | Single pass, no iteration |
| Examples | Inline in references section | External file reference only |
| Anti-patterns | Bundled 16-technique reference | External file reference only |
| Stop conditions | Explicit ("do not send until review passes") | Implicit |
| Escalation path | Not applicable | Missing entirely |

The Scafforge skill is doing the right job but needs: better triggers, an explicit iteration protocol in the verification step, inline before/after examples in the description block, and an escalation path for cases where a prompt is unfixable.

---

## Part 3: Critical Quality Patterns

### What makes a skill Tier 4 (deterministic micro-skill)?

1. **Trigger specificity:** Not "use when you need X" but "activate when the user says any of: [list of exact phrases]"
2. **Executable steps:** Not "consider performance" but `cargo check --locked 2>&1 | head -20`
3. **Verification gates:** Not "make sure it works" but `gh release view vX.Y.Z` to confirm
4. **Pitfall documentation:** The top 5 mistakes in this domain, with exact solutions
5. **Decision trees:** Not "choose the right approach" but a table of (condition → action) pairs
6. **Constraint naming:** Exact limits, sizes, timeouts, quotas relevant to the domain
7. **Anti-pattern list:** What NOT to do, with the same specificity as what to do

### What makes a skill Tier 0–1 (near-worthless)?

1. **Correct but unweighted lists** — "Be consistent. Use meaningful names. Follow conventions." Every item is true; none tells you what to do in a specific situation.
2. **Generic tool mentions** — "Use Pydantic for validation" without explaining which validation scenarios require Pydantic vs simpler approaches.
3. **Missing stop conditions** — a skill that tells you how to start but not when you're done
4. **No domain constraints** — failing to name the specific limits of the environment (memory limits, CPU budget, network access constraints)
5. **Placeholder language** — "Replace this with stack-specific rules once the real project stack is known." (`stack-standards/SKILL.md`)

### The "skills are a context-loading problem" insight

The anthropics `skill-creator` nailed this: skills are read into the model's context window. Every word competes for limited attention. The solution is **progressive disclosure**:
- Description header: ~100 words always loaded — must be trigger-clear
- SKILL.md body: <500 lines loaded on explicit activation
- `references/` folder: unlimited, loaded on demand by the skill workflow

Scafforge's generated-repo skills have no `references/` folders. All content lives in the SKILL.md body. This means the only way to add depth is to make the skill longer — which makes it more expensive to load. The architecture needs to change.

---

## Part 4: Missing Skill Categories for Generated Repos

### Missing entirely from current generated-repo output pack:

1. **stack-specific implementation skill** — the most critical missing skill; `stack-standards` is a placeholder
2. **Domain testing skill** — how to test the specific tech stack (unit, integration, e2e, snapshot, property-based)
3. **Database/persistence skill** — ORM patterns, migration workflow, query optimization for chosen DB
4. **Authentication/authorization skill** — auth flows, JWT patterns, session management for chosen stack
5. **Deployment/CI skill** — specific pipeline steps for the chosen cloud/infra target
6. **LLM integration skill** — how to integrate AI capabilities into the product being built (not how to operate the AI agent building it)
7. **API client skill** — how to safely call external APIs (retry patterns, rate limiting, circuit breakers, SDK vs raw HTTP)
8. **Error handling skill** — stack-specific error patterns, logging standards, observability setup
9. **Performance baseline skill** — profiling tools for the stack, what metrics to capture, acceptable thresholds
10. **Security hardening skill** — OWASP top 10 translated to the chosen stack, dependency audit commands

### Missing patterns (present in external examples, absent in Scafforge output):

| Pattern | Example | Missing in Scafforge output |
|---------|---------|----------------------------|
| Approval gates | doc-coauthoring Stage 1 gate | All current skills |
| Decision trees | claude-api API selection table | All current skills |
| Self-review loops | Gen AI Space skill iterative refinement | All current skills |
| Pitfall tables | UE5 blueprint pitfalls table | All current skills |
| Verification commands | release skill `gh release view` | ticket-execution only (weak) |
| Bundled references | anthropics skills `references/` | None in generated pack |
| Constraint naming | Cloudflare 50ms CPU limit | None in generated pack |
| Anti-pattern lists | frontend-design anti-AI-slop section | review-audit-bridge only (partial) |

---

## Part 5: Specific Recommendations

### 1. Fix the `stack-standards` placeholder immediately

The placeholder message:
> "Current scaffold mode: `__STACK_LABEL__`
> Replace this file with stack-specific rules once the real project stack is known."

This skill should instead contain a minimal useful skeleton that `project-skill-bootstrap` fills in during scaffold synthesis. At minimum, the synthesized skill should include:
- Language + version
- Key frameworks + versions
- Dependency management tool
- Linting/formatting tools
- Testing framework
- 5–7 language-specific conventions
- 3–5 things NOT to do

### 2. Add a `references/` subdirectory pattern to generated skills

Adopt the anthropics architecture: short descriptions (always loaded), SKILL.md body (loaded on activation), `references/` (loaded on demand). For example, `ticket-execution/references/lifecycle.md` could hold the full stage rules table without bloating the always-loaded description.

### 3. Add self-review loops to at least 3 skills

`review-audit-bridge`, `docs-and-handoff`, and `ticket-execution` should all include explicit iteration loops: "run review criteria → if any fail, go back to step N → re-run until all pass."

### 4. Add approval gates to `docs-and-handoff`

The handoff doc should not be finalized until the team lead (or orchestrator) confirms it. Add: "Do not mark handoff complete until the restart surface has been acknowledged."

### 5. Create a `pitfalls` reference in governance skills

The UE5 skill's pitfalls table format should be adopted in `ticket-execution` (most common ticket lifecycle mistakes), `workflow-observability` (most common observability gaps), and `research-delegation` (most common research anti-patterns).

### 6. Add decision trees to `research-delegation` and `isolation-guidance`

Replace prose descriptions with tables:
- When to use a read-only delegate vs a write-capable subagent
- When to isolate in a branch vs work in-place
- When research findings become blockers vs confirmed facts

### 7. Add trigger specificity to all skills

Every skill description should have a "Use this skill when:" list with 4–8 concrete phrases or scenarios. Currently most skills use a single generic clause.

### 8. Agent prompt engineering — required improvements

Apply the findings from Part 2:
- Add 8+ trigger phrases to the description
- Add an explicit iteration protocol to Step 6
- Add a before/after example in the description header (not just references/)
- Add an escalation path ("if the prompt design itself is broken, stop and create a blocker ticket")

### 9. New skills for the next generation

**High priority (most generated repos need these):**
- `stack-implementation` — synthesized from canonical brief, covers patterns, conventions, testing, error handling for the specific stack
- `domain-testing` — testing strategy and tool-specific patterns for the stack
- `deployment-pipeline` — CI/CD steps for the target infra

**Medium priority:**
- `api-standards` — REST/GraphQL/MCP API design conventions for the project
- `llm-integration` — if the project includes AI features
- `auth-patterns` — authentication/authorization conventions for the stack

**Domain-specific (generated when applicable):**
- `firebase-sdk` — Firestore, Auth, Cloud Functions patterns
- `fastapi-patterns` — async routes, Pydantic models, lifecycle, error handling
- `unity-csharp` — patterns, performance, MonoBehaviour lifecycle
- `ue5-blueprint` — Visual Scripting patterns, naming, communication, performance

---

## Part 6: Confidence Assessment

**High confidence:**
- Quality tier assessments of examined skills (based on direct file examination)
- Identification of missing skill categories (confirmed by exhaustive search across public sources)
- Anti-pattern identification (consistent finding across all sources)
- Anthropics/openwork skill quality analysis (complete files examined)

**Medium confidence:**
- Cloudflare/Firebase/GCP gap assessment (may be strong private skills not discoverable)
- "No strong CLI/TUI skills exist" (possible gap in search strategy)
- Gen AI Space technique count (counted from the SKILL.md; original framework document not verified)

**Low confidence / not verified:**
- llama.cpp and local LLM skill landscape (search returned no results; gap may be a search limitation)
- Microsoft internal skills library contents (not publicly accessible)
- Minimax-specific skill patterns (not examined; insufficient public material found)

---

## Footnotes

[^1]: `openclaw/skills` — `skills/golofu/skill-with-prompt-engineering/SKILL.md` — Gen AI Space prompt engineering skill, 16-technique framework, dual-mode (prompt creation + SKILL.md building). SHA: 2184694c.

[^2]: `different-ai/openwork` — `.opencode/skills/opencode-bridge/SKILL.md` — Three-channel OpenCode-OpenWork integration (CLI, SQLite, MCP). SHA: fb2b195e.

[^3]: `different-ai/openwork` — `.opencode/skills/solidjs-patterns/SKILL.md` — SolidJS signal reactivity, scoped async actions, deadlock analysis. SHA: 949a953d.

[^4]: `different-ai/openwork` — `.opencode/skills/cargo-lock-manager/SKILL.md` — Cargo.lock CI failure resolution with exact error trigger phrases. SHA: e7522f8d.

[^5]: `different-ai/openwork` — `.opencode/skills/release/SKILL.md` — 6-step release workflow with exact commands and verification step. SHA: 97bd034b.

[^6]: `different-ai/openwork` — `.opencode/skills/tauri-solidjs/SKILL.md` — Tauri 2.x + SolidJS native app reference. SHA: c6a5c469.

[^7]: `PatrickJS/awesome-cursorrules` — `rules/python-fastapi-best-practices-cursorrules-prompt-f/.cursorrules` — FastAPI best practices checklist. SHA: d314fcdc.

[^8]: `PatrickJS/awesome-cursorrules` — `rules/python-llm-ml-workflow-cursorrules-prompt-file/.cursorrules` — Python LLM/ML workflow with full tech stack declaration. SHA: 1d2e6d6a.

[^9]: `PatrickJS/awesome-cursorrules` — `rules/unity-cursor-ai-c-cursorrules-prompt-file/.cursorrules` — Minimal Unity project context (Tier 0). SHA: 7982bd6b.

[^10]: `PatrickJS/awesome-cursorrules` — `rules/tailwind-react-firebase-cursorrules-prompt-file/firebase-rules.mdc` — Three-bullet Firebase stub (Tier 0). SHA: cf20b099.

[^11]: `PatrickJS/awesome-cursorrules` — `rules/tailwind-react-firebase-cursorrules-prompt-file/general-ui-ux-rules.mdc` — Correct but unweighted UI/UX checklist (Tier 1). SHA: 217b7f9b.

[^12]: `DWFullen/UE5StackLearning` — `agents/instructions/ue5-blueprint.md` — Strong UE5 Blueprint instruction file, pitfalls table, communication patterns. SHA: b062d845.

[^13]: `Scafforge/skills/agent-prompt-engineering/SKILL.md` — Scafforge package skill for prompt hardening. Local file, audited in Part 2.

[^14]: `Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/stack-standards/SKILL.md` — Explicit placeholder, 3-line stub. Local file.

[^15]: `anthropics/skills` — full directory listing of 17 skills: algorithmic-art, brand-guidelines, canvas-design, claude-api, doc-coauthoring, docx, frontend-design, internal-comms, mcp-builder, pdf, pptx, skill-creator, slack-gif-creator, theme-factory, web-artifacts-builder, webapp-testing, xlsx. SHA: b0cbd3df.

[^16]: `dustland/Kardx` — `.cursor/rules/unity.mdc` — Stronger Unity rule with architecture mandates and anti-patterns. From Report #1.

[^17]: `Atyantik/flarekit` — `.cursor/rules/cloudflare-worker-patterns.mdc` — Cloudflare D1/R2/Queue patterns, edge constraints. From Report #1.

[^18]: `invertase/react-native-firebase` — `.cursor/rules/ai/porting-workflow.md` — 3-step port workflow with priority ordering. From Report #1.

---

*Report generated during Scafforge research session. Research only — no product changes in this session.*
