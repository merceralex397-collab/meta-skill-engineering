# Research: Output-repository skill systems for Scafforge

## Executive Summary

Scafforge's current generated output skills are coherent as process lanes, but they are too generic and too thin to produce consistent high-quality behavior across stacks. The local catalog is almost entirely governance-oriented, and the most obviously stack-specific skill, `stack-standards`, is still a placeholder.[^1][^2]

Public GitHub ecosystems show that stronger skill systems usually do four things our output pack mostly does not: they encode explicit trigger conditions, provide stepwise workflows or decision trees, bundle reusable references/scripts/evals, and specialize by stack or domain rather than only by process. Anthropic's public skills, Microsoft's skills repo, and OpenWork's OpenCode-native skills all show those patterns in different ways.[^3][^4][^5]

The broadest public rule registries do cover many of the domains you named—React, FastAPI, TypeScript, mobile, testing, Firebase, Cloudflare, and even Unity—but much of that surface is checklist-quality rather than workflow-quality. That means "more categories" alone will not solve the problem; the generated output repo needs a stronger skill architecture and a higher quality bar.[^6][^7][^8][^9]

The highest-leverage shift is to move from "a small set of generic repo lanes" to "a layered system": thin core repo-governance skills, generated stack/domain packs, tool/platform packs, and a meta-skill/eval loop for improving skills after scaffold.[^4][^10][^11]

## Architecture / Ecosystem Overview

A useful way to think about modern skill systems is as a stack, not a single markdown file:

```text
Trigger metadata
  ↓
Workflow body / decision tree
  ↓
Bundled references / scripts / templates / examples / evals
  ↓
External tools / MCPs / CLIs
  ↓
Structured outputs / verification / iteration
```

The stronger public systems explicitly separate these layers. Anthropic's `skill-creator` defines metadata vs. `SKILL.md` body vs. bundled resources as a three-level loading system, then adds tests, baselines, assertions, and timing capture. Microsoft's `skills` repo adds plugin metadata, category trees, symlink-based reuse, and harness-driven verification. OpenWork's `opencode-primitives` skill shows the OpenCode-native version of the same idea: a short skill body that points outward to canonical docs, config precedence, and permission rules rather than trying to inline every fact.[^10][^4][^11]

By contrast, low-end instruction surfaces tend to be either a short repo description (`copilot-instructions`) or a one-paragraph rule (`.clinerules`). Those can be useful, but they rarely encode enough procedure to stabilize multi-step engineering work.[^12][^13]

## Current Scafforge output-skill audit

### What the generated output currently optimizes for

The current generated output pack is optimized for repo governance and lane separation. The default local skill catalog lists `project-context`, `repo-navigation`, `stack-standards`, `ticket-execution`, `review-audit-bridge`, `docs-and-handoff`, `workflow-observability`, `research-delegation`, `local-git-specialist`, and `isolation-guidance`, with `process-doctor` optional.[^1]

Several of these skills do useful job-control work. `project-context` gives a deterministic reading order over canonical docs; `ticket-execution` defines an explicit stage order and parallelization guardrails; `review-audit-bridge` defines prioritized finding categories and blocker behavior; and `workflow-observability` defines output sections plus explicit missing-data language. Those are real workflow contracts, especially for weaker models.[^14][^15][^16][^17]

### Why it still feels weak

The pack feels weak because most of the lanes are broad but under-specified. `stack-standards` explicitly says to replace the file once the real stack is known, which means the one skill that should carry the bulk of framework/domain guidance currently does not.[^2]

The rest of the pack mostly tells the agent what surface to read or what stage it is in, not how to execute substantive work inside a particular stack or domain. `repo-navigation`, `docs-and-handoff`, `research-delegation`, and `local-git-specialist` are useful, but intentionally terse; they do not encode React, FastAPI, Firebase, Cloudflare, CLI/TUI, Unity, review heuristics by language, or documentation-writing procedure in any deep way.[^18][^19]

I also found almost no extra support layers in the scaffolded output skills. There is at least one bundled non-`SKILL.md` artifact under `workflow-observability`, but nothing in the current generated pack resembles the richer reference/script/eval structures visible in the stronger public ecosystems.[^22][^10][^4]

A small skill is not inherently bad. OpenWork's `get-started` skill is tiny because it is narrow and deterministic: it activates on one phrase, emits four exact lines, and then performs a single MCP-backed action. The problem with the current Scafforge output pack is different: several skills are small while targeting broad responsibilities like stack standards or research behavior, so too much behavior is left to model improvisation.[^23][^2][^19]

## What strong public skill systems are doing

### 1. Rich skill frameworks: explicit triggers, phases, resources, and evals

Anthropic's public skills repo is a real skill framework, not just a pile of prompts. Its README defines skills as self-contained folders with `SKILL.md`, highlights diverse categories (creative, technical, enterprise, document), and gives a minimal metadata contract (`name`, `description`) for discovery.[^3]

More importantly, individual skills are procedural. `mcp-builder` defines a four-phase workflow (research, implementation, review/test, evaluations), discusses tool naming, discoverability, context management, annotations, schemas, and evaluation question requirements. `doc-coauthoring` defines staged interaction, branching behavior, and section-by-section drafting. `webapp-testing` includes a decision tree, tells the agent to use helper scripts as black boxes to avoid context pollution, and provides a reconnaissance-then-action testing pattern.[^24][^25][^26]

Anthropic's `skill-creator` is especially relevant to your "is prompt engineering even any good?" question. The strong public pattern is not "ship generic prompting advice"; it is "treat skill quality as an iterative engineering loop" with progressive disclosure, bundled resources, realistic test prompts, baseline-vs-with-skill comparisons, and captured timing/quantitative assertions.[^10]

### 2. Platform/provider libraries: category trees, packaging, and freshness rules

Microsoft's `skills` repo shows another mature pattern: skills are packaged with agents, plugins, commands, and MCP configs, and the repo carries both platform-level principles and a large language/category matrix. `Agents.md` warns against stale SDK knowledge, requires doc-first verification, enumerates core engineering principles, and documents a catalog of 133 skills across core, Python, .NET, TypeScript, and Java.[^4]

It also treats skills as installable, categorized assets rather than ad hoc text blobs. `marketplace.json` describes plugin roots, categories, keywords, strictness, and whether a package ships commands, skills, and agents together. `Agents.md` further documents a creation workflow, symlink-based categorization, references, test scenarios, and a harness command for verification.[^5][^4]

The key lesson is that generated-repo skills do not need to be isolated markdown files. In the strongest ecosystems, skills live inside a package that also includes install metadata, references, tests, and adjacent tools.[^5][^4]

### 3. OpenCode-native patterns: short reference skills and exact micro-skills

OpenWork is especially relevant because it is `.opencode`-native. `opencode-primitives` is a compact reference skill that anchors implementation to canonical OpenCode docs, states exact path and naming rules, and explains permission/config precedence. `get-started` is the opposite kind of skill: a precise deterministic micro-skill for one onboarding interaction.[^11][^23]

Together, those two files show a useful distinction for Scafforge: some output skills should be reference skills (short but exact, pointing outward to canonical docs), while others should be task skills (narrow, highly deterministic behaviors). The current Scafforge output pack mixes those roles together under broad lane names.[^11][^23][^1]

### 4. Curated rule registries: huge breadth, uneven depth

`awesome-cursorrules` is strong evidence that the public ecosystem already covers a huge cross-domain surface. Its catalog spans frontend frameworks, backend/full-stack, mobile, CSS/styling, state management, database/API, testing, hosting/deployments, build tools, language-specific rules, documentation, and utilities. The listed rules include many of the domains you named: React, Next, TypeScript, FastAPI, Flask, Django, Expo, Flutter, SwiftUI, Android, Firebase, and more.[^6]

That breadth is real, but the depth is uneven. The curated repo's own `react.mdc` and `fastapi.mdc` examples are mostly high-level checklists of best practices. They are useful as category seeds or lightweight guidance, but they do not by themselves provide the determinism or execution scaffolding you want from generated-repo skills.[^7][^27]

The repo-level `.cursorrules` file reinforces that point: it is largely about organizing the registry and README structure, plus general advice to focus on repo-level context. It is not a workflow engine.[^28]

### 5. Real project-local rules become much stronger when they bind to actual files and processes

`lew-ui` is a better example of deep repo-local rules. Its base rule says component changes must update component-local MDC docs and changelog sections; the `component-structure` rule then prescribes exact directory layouts, naming conventions, auto-import behavior, export patterns, prop typing conventions, and doc-system-specific metadata fields. That is substantially stronger than a generic React/Vue checklist because it binds directly to the project's file topology and documentation tooling.[^29][^30]

`instructure-ui`'s PR command is similar for review operations: it defines required sections, branch diff inspection steps, AI disclosure, draft-vs-ready rules, and even the exact `gh pr create` body structure. That makes it closer to a reliable repo-local workflow skill than a style guide.[^31]

The React Native Firebase porting workflow is stronger still: it defines a three-step process, an analysis gate, implementation phase, review stop, per-feature commit boundaries, a quality checklist, and an example session. This is the closest thing in the sample set to a reusable migration/porting skill.[^32]

### 6. Domain-specific public rules exist for cloud and game engines, but many are still checklist-shaped

The public rule landscape does include Cloudflare and Unity examples. `flarekit` has a Cloudflare Worker rule covering handler structure, D1/R2 integration, queue handling, scheduled events, local dev, deployment, and edge performance. `Kardx` has a Unity/C# rule covering project context, naming, MVC/ScriptableObject/object pooling patterns, performance guidance, and testing/debugging expectations.[^33][^34]

These examples prove that generated-repo skill packs can reasonably include cloud/runtime and game-engine domains. But they also show a common weakness: outside the very best skill ecosystems, many public domain files still stop at "remember these practices" rather than giving staged procedures, examples, reusable resources, or verification loops.[^33][^34][^27]

### 7. Simpler host instruction surfaces are not enough on their own

The GitHub Copilot instructions example I fetched is essentially a short project description and directory overview with a few content guidelines. The `.clinerules` example is a single instruction about maintaining a top comment block. These are valid low-friction instruction surfaces, but they are not enough to stabilize multi-step engineering work across stacks.[^12][^13]

That matters because a generated repo cannot rely on "some repo-level instructions exist" as proof that its skill system is strong. The difference between a low-friction host instruction file and a high-quality skill system is enormous.[^12][^13][^10]

## Cross-domain taxonomy for generated-repo skills

The public landscape suggests that output-repo skills should be generated in at least eight families:

| Family | What it should cover | Evidence from GitHub | Current Scafforge output |
|---|---|---|---|
| Repo truth/context | canonical docs, reading order, config locations, permissions | `opencode-primitives`, `project-context`-style skills [^11][^14] | Present |
| Lifecycle/workflow | planning, ticket execution, review lanes, handoff | `doc-coauthoring`, `instructure` PR flow, RN Firebase porting [^25][^31][^32] | Present but thin |
| Review/QA/verification | evidence-first review, test plans, eval harnesses | `review-audit-bridge`, `webapp-testing`, `skill-creator` eval loop [^16][^26][^10] | Partially present |
| Stack/framework/domain | React, TypeScript, FastAPI, Flask, Cloudflare, Firebase, CLI/TUI, Unity, etc. | `awesome-cursorrules`, `lew-ui`, `flarekit`, `Kardx` [^6][^29][^33][^34] | Largely absent / placeholder |
| Tool/platform integration | MCPs, Playwright, GitHub CLI, Wrangler, SDK freshness, doc search | `mcp-builder`, Microsoft `Agents.md`, `webapp-testing` [^24][^4][^26] | Mostly absent |
| Artifact writing | specs, docs, research, proposals, handoffs | `doc-coauthoring`, Copilot instructions example [^25][^12] | Very light |
| Migration/porting/refactor | staged analysis, incremental implementation, review gates | RN Firebase porting workflow [^32] | Absent |
| Meta-skill / skill maintenance | create, test, benchmark, improve skills | `skill-creator`, Microsoft creation workflow [^10][^4] | Absent |

The biggest gap is not just "more skills," but "more generated skill families with different levels of determinism." The current Scafforge output pack is disproportionately concentrated in repo context and process control.[^1][^2][^10]

## Quality tiers: what public examples imply

A useful way to think about generated-repo skills is by quality tier:

| Tier | Shape | Example | Value | Limitation |
|---|---|---|---|---|
| 0 | Minimal repo note | `.clinerules`, simple Copilot instructions [^13][^12] | Cheap, easy to maintain | Too weak for complex work |
| 1 | Generic checklist | generic React/FastAPI/Unity/Cloudflare rules [^7][^27][^33][^34] | Broad coverage fast | High output variance |
| 2 | Project-bound playbook | `lew-ui` rules, `instructure` PR command [^29][^30][^31] | Stronger local consistency | Usually still single-file |
| 3 | Full skill with resources and verification | Anthropic skills, Microsoft skills [^24][^10][^4][^5] | Best balance of power and reliability | Higher authoring cost |
| 4 | Deterministic micro-skill | OpenWork `get-started` [^23] | Excellent for narrow moments | Not a substitute for broad skills |

Scafforge's current generated output is mostly Tier 2 for process lanes, Tier 1 or lower for stack/domain behavior, and missing Tier 3 almost entirely for generated-repo-local skills.[^1][^2][^10]

## Implications for Scafforge's generated output skills

Based on this survey, I would not recommend simply expanding the current output repo by adding dozens of short generic skills. The public evidence suggests five more durable changes.

1. **Keep a small core governance pack, but stop making it carry stack knowledge.** `project-context`, `ticket-execution`, `review-audit-bridge`, and `workflow-observability` are useful core lanes; `stack-standards` should become a generated family of stack/domain skills rather than a placeholder monolith.[^14][^15][^16][^17][^2]

2. **Generate stack/domain packs from taxonomy, not one generic "standards" skill.** At minimum, the public evidence supports optional packs for frontend/UI (React/Next/Tailwind), TypeScript, Python API (FastAPI/Flask), cloud/runtime (Cloudflare/GCP/Firebase), tool-integration/MCP, testing/review, and writing/research. Unity support is easy to justify; Unreal support likely needs separate validation because I did not verify a strong Unreal-specific artifact in the final source set.[^6][^7][^27][^33][^34]

3. **Adopt progressive disclosure.** Strong skill systems keep the always-loaded part small, then point to references, scripts, examples, and variant docs only when needed. That pattern is explicit in `skill-creator`, `mcp-builder`, and Microsoft's package structure, and it is missing from the current generated output.[^10][^24][^4][^5]

4. **Give major skills real execution scaffolding.** The best public skills define phases, branching, exact output formats, helper scripts, or checklists that stop unsafe guessing. `doc-coauthoring`, `webapp-testing`, `mcp-builder`, and RN Firebase's porting workflow all show what that looks like.[^24][^25][^26][^32]

5. **Treat skill quality as an evaluated product surface.** Public best practice is not "ship a prompt and hope." It is to ship example prompts, acceptance criteria, structured outputs, and a way to compare baseline vs skill-assisted behavior. If Scafforge wants less variable output, this is the highest-leverage improvement.[^10][^4]

## Key repositories summary

| Repository | Why it matters |
|---|---|
| [anthropics/skills](https://github.com/anthropics/skills) | Best example in this sample of a mature skill system with domain breadth, staged workflows, bundled resources, and skill-eval thinking.[^3][^10][^24][^25][^26] |
| [microsoft/skills](https://github.com/microsoft/skills) | Best example of a large, categorized, installable skill/platform pack with commands, agents, plugins, MCPs, and verification workflow.[^4][^5] |
| [different-ai/openwork](https://github.com/different-ai/openwork) | Best OpenCode-native reference for distinguishing exact micro-skills from documentation/reference skills.[^11][^23] |
| [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules) | Best evidence for category breadth across frameworks and domains, but also a reminder that breadth often arrives as shallow checklist files.[^6][^7][^28] |
| [lewkamtao/lew-ui](https://github.com/lewkamtao/lew-ui) | Strong real-project example of repo-bound structural guidance tied to actual component layout and docs tooling.[^29][^30] |
| [instructure/instructure-ui](https://github.com/instructure/instructure-ui) | Strong command-style review/PR workflow artifact.[^31] |
| [invertase/react-native-firebase](https://github.com/invertase/react-native-firebase) | Strong staged migration/porting workflow example.[^32] |
| [Atyantik/flarekit](https://github.com/Atyantik/flarekit) | Evidence that Cloudflare/runtime-specific rule packs exist.[^33] |
| [dustland/Kardx](https://github.com/dustland/Kardx) | Evidence that Unity/game-dev rule packs exist.[^34] |
| [karenoei/skills-customize-your-github-copilot-experience](https://github.com/karenoei/skills-customize-your-github-copilot-experience) and [yysun/apprun](https://github.com/yysun/apprun) | Useful low-end baselines for minimal host instruction surfaces.[^12][^13] |

## Confidence Assessment

**High confidence**

- The current generated Scafforge output-skill pack is process-heavy and comparatively weak on stack/domain specificity.[^1][^2][^18]
- Rich public skill systems tend to include trigger metadata, staged workflows, reusable references/scripts, and some verification or evaluation story.[^10][^24][^4][^5]
- Public repos already cover many of the areas you listed, especially frontend/web, Python API, mobile, testing, cloud/runtime, and documentation.[^6][^33][^34]

**Medium confidence**

- The exact best default taxonomy for generated-repo skills in Scafforge should likely be narrower than the full public landscape; some domains are common enough to bundle by default, while others should probably be opt-in packs. The evidence strongly supports the category families, but not the exact default set.[^6][^3][^4]
- Unity is clearly represented in the fetched sample; Unreal may also exist in the ecosystem, but I did not verify a strong Unreal-specific artifact in the final source set, so I would treat Unreal as a candidate extension rather than a confirmed default.[^34]

**Inference / judgment**

- My strongest recommendation is architectural, not editorial: fixing the output system means changing the generated-skill model (core + domain packs + resources + evals), not merely making the current ten files longer. That conclusion is an inference from the pattern comparison, but it is strongly supported by the gap between the current scaffold and the best public examples.[^1][^10][^4][^11]

## Footnotes

[^1]: `/home/a/Scafforge/skills/project-skill-bootstrap/references/local-skill-catalog.md:1-70`.
[^2]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/stack-standards/SKILL.md:1-13`.
[^3]: [anthropics/skills](https://github.com/anthropics/skills), `README.md:3-27, 61-88` (commit `b0cbd3df1533b396d281a6886d5132f623393a9c`).
[^4]: [microsoft/skills](https://github.com/microsoft/skills), `Agents.md:1-20, 26-86, 147-255` (commit `34dddbd53c2226379c6da1dac1e05223554a821e`).
[^5]: [microsoft/skills](https://github.com/microsoft/skills), `marketplace.json:1-31, 33-124` (commit `34dddbd53c2226379c6da1dac1e05223554a821e`).
[^6]: [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules), `README.md:34-50, 52-195` (commit `fc2ce049c55b369b498eef396506a7a269a1b461`).
[^7]: [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules), `rules-new/react.mdc:1-78` (commit `fc2ce049c55b369b498eef396506a7a269a1b461`).
[^8]: [Atyantik/flarekit](https://github.com/Atyantik/flarekit), `.cursor/rules/cloudflare-worker-patterns.mdc:1-64` (commit `5aa7e082793904ddaa4595f90a82680369da60fd`).
[^9]: [dustland/Kardx](https://github.com/dustland/Kardx), `.cursor/rules/unity.mdc:1-41` (commit `6293db55796dba507cc0fecd72229aa6e4aabde4`).
[^10]: [anthropics/skills](https://github.com/anthropics/skills), `skills/skill-creator/SKILL.md:45-99, 141-219` (commit `b0cbd3df1533b396d281a6886d5132f623393a9c`).
[^11]: [different-ai/openwork](https://github.com/different-ai/openwork), `.opencode/skills/opencode-primitives/SKILL.md:6-46` (commit `b88e2b53beb2a43053663804885639373e995eb6`).
[^12]: [karenoei/skills-customize-your-github-copilot-experience](https://github.com/karenoei/skills-customize-your-github-copilot-experience), `.github/copilot-instructions.md/instructions:1-22` (commit `7a3d3a9bc32f7b702392ca41c3bbbb815dc6c6d0`).
[^13]: [yysun/apprun](https://github.com/yysun/apprun), `.clinerules:1` (commit `47cb3b10194ee81234c93e1dbbe9c998e6cd0288`).
[^14]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/project-context/SKILL.md:1-18`.
[^15]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/ticket-execution/SKILL.md:1-29`.
[^16]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/review-audit-bridge/SKILL.md:1-40`.
[^17]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/workflow-observability/SKILL.md:1-30`.
[^18]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/repo-navigation/SKILL.md:1-19`; `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/docs-and-handoff/SKILL.md:1-16`.
[^19]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/research-delegation/SKILL.md:1-22`; `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/local-git-specialist/SKILL.md:1-21`.
[^20]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/isolation-guidance/SKILL.md:1-22`.
[^21]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/local-git-specialist/SKILL.md:1-21`.
[^22]: `/home/a/Scafforge/skills/repo-scaffold-factory/assets/project-template/.opencode/skills/workflow-observability/agents/openai.yaml:1-2`.
[^23]: [different-ai/openwork](https://github.com/different-ai/openwork), `.opencode/skills/get-started/SKILL.md:1-22` (commit `b88e2b53beb2a43053663804885639373e995eb6`).
[^24]: [anthropics/skills](https://github.com/anthropics/skills), `skills/mcp-builder/SKILL.md:15-35, 68-75, 94-123, 151-179` (commit `b0cbd3df1533b396d281a6886d5132f623393a9c`).
[^25]: [anthropics/skills](https://github.com/anthropics/skills), `skills/doc-coauthoring/SKILL.md:17-27, 28-101, 104-200` (commit `b0cbd3df1533b396d281a6886d5132f623393a9c`).
[^26]: [anthropics/skills](https://github.com/anthropics/skills), `skills/webapp-testing/SKILL.md:7-33, 35-63, 65-96` (commit `b0cbd3df1533b396d281a6886d5132f623393a9c`).
[^27]: [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules), `rules-new/fastapi.mdc:1-86` (commit `fc2ce049c55b369b498eef396506a7a269a1b461`).
[^28]: [PatrickJS/awesome-cursorrules](https://github.com/PatrickJS/awesome-cursorrules), `.cursorrules:1-82` (commit `fc2ce049c55b369b498eef396506a7a269a1b461`).
[^29]: [lewkamtao/lew-ui](https://github.com/lewkamtao/lew-ui), `.cursor/rules/base.mdc:1-44` (commit `8d76c90bf3fad5fb988cae6cd3d939051b8918d4`).
[^30]: [lewkamtao/lew-ui](https://github.com/lewkamtao/lew-ui), `.cursor/rules/component-structure.mdc:9-29, 45-76, 100-147, 149-220` (commit `8d76c90bf3fad5fb988cae6cd3d939051b8918d4`).
[^31]: [instructure/instructure-ui](https://github.com/instructure/instructure-ui), `.claude/commands/pr.md:1-52` (commit `9fc967538df47b26609ea3ab171b7253d736f89f`).
[^32]: [invertase/react-native-firebase](https://github.com/invertase/react-native-firebase), `.cursor/rules/ai/porting-workflow.md:1-177` (commit `f3941a03c75c802bb7dd0efeeb6ef1654419a03f`).
[^33]: [Atyantik/flarekit](https://github.com/Atyantik/flarekit), `.cursor/rules/cloudflare-worker-patterns.mdc:1-64` (commit `5aa7e082793904ddaa4595f90a82680369da60fd`).
[^34]: [dustland/Kardx](https://github.com/dustland/Kardx), `.cursor/rules/unity.mdc:1-41` (commit `6293db55796dba507cc0fecd72229aa6e4aabde4`).
