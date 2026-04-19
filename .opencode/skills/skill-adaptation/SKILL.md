---
name: skill-adaptation
description: >-
  Adapt an existing skill to a different repository, stack, team, or project
  context while preserving the core pattern. Use when asked to "port this skill
  to Python/Vue/pnpm", "customize this library skill for our project", or
  "localize this skill for a different environment". Do not use for writing a
  new skill from scratch (use skill-creator), improving an existing
  project-specific skill without changing context (use skill-improver), or
  splitting one skill into stack-specific variants (use skill-variant-splitting).
---

# Purpose

Rewrite a skill's context-dependent references — tools, paths, conventions,
terminology — for a different target environment while keeping its core
procedure and safety constraints intact.

# When to use

- Porting a skill between stacks (React → Vue, npm → pnpm, pytest → unittest)
- Installing a library skill that references tools or paths absent in the target project
- Localizing a skill for different team conventions, file structure, or domain terminology

# When NOT to use

- Creating a skill from scratch → `skill-creator`
- Improving an already project-specific skill without changing context → `skill-improver`
- Splitting a broad skill into focused per-stack variants → `skill-variant-splitting`
- Only the trigger description needs fixing → `skill-trigger-optimization`
- Skill already works correctly in the target context

# Procedure

1. **Read the source skill end-to-end.** Identify the problem it solves, the output it produces, and every assumption it encodes (tools, paths, naming, domain terms).

2. **Define the target context.** Collect: stack/language, file structure conventions, available tools, forbidden tools, domain terminology. If any of these are unclear, ask before proceeding — do not guess.

3. **Catalog adaptation points** — references that MUST change:
   - Tool/command references (`npm` → `pnpm`, `pytest` → `unittest`)
   - File paths and glob patterns (`src/components/` → `app/ui/`)
   - Naming conventions (camelCase → snake_case)
   - Output format references (Markdown → org-mode)
   - Domain terminology ("user" → "customer")

4. **Catalog invariants** — things that must NOT change:
   - Core procedure logic and step ordering
   - Safety constraints and quality checks
   - The fundamental problem being solved

   **Always check these as candidate invariants:**
   - Skill name pattern (the naming convention, not the specific name)
   - Output artifact format and required sections
   - Failure handling table structure and coverage
   - "When NOT to use" boundaries — these define the skill's identity

   **These are NEVER invariants** (they exist to be adapted):
   - Specific tool or command names
   - File paths, glob patterns, directory structures
   - Code examples and inline snippets
   - Technology-specific terminology and jargon

   **Heuristic:** For each line, ask: "If I remove or change this, does it change what the skill DOES (its purpose and contracts) or only HOW it does it (its implementation details)?" Lines that change what it does are invariants. Lines that change how it does it are adaptation points.

5. **Produce the adapted SKILL.md:**
   - Preserve frontmatter structure
   - Update `description` if the context change affects routing
   - Replace every cataloged adaptation point with its target equivalent
   - Replace or add context-specific examples where the skill uses few-shot patterns
   - Do NOT add provenance/history sections to the adapted skill

6. **Review and adapt support layers.** If the source skill includes `references/`, `scripts/`, `evals/`, or `assets/` directories, review each for context-dependent content. Adapt file paths, tool commands, example data, and environment assumptions in support files the same way you adapted SKILL.md. Test scripts may need updated commands or fixtures; reference docs may cite tools absent in the target context.

7. **Validate the adaptation:**

   **Zero dangling references:**
   - Every tool/command reference exists in the target environment
   - Every file path pattern matches the target structure
   - No leftover references to the source context survive

   **Procedure integrity check:**
   - Walk through each procedure step mentally with a real task from the target context. If a step doesn't make sense or produces no useful result in the new context, the adaptation is incomplete.
   - Verify every step still produces a concrete artifact or decision — not just "do the equivalent thing".
   - Check for target-context-specific failure modes the original skill didn't need to cover. For example, porting from npm to pnpm may introduce phantom dependency issues that need a new failure-handling entry.

# Output contract

Deliver exactly two artifacts:

1. **Adaptation summary** (inline in response):

```
## Adaptation Summary

**Source skill**: [name]
**Target context**: [stack / project / team]

### Changes
| Original | Adapted | Reason |
|----------|---------|--------|
| npm install | pnpm add | Project uses pnpm |
| src/components/ | app/ui/ | Target file structure |

### Invariants preserved
- [Core procedure element kept intact]
```

2. **Adapted SKILL.md** — the full rewritten file, ready to drop in.

# Failure handling

- **Target context unclear**: Stop and ask — "What stack? What file structure? What tools are available?" Do not assume.
- **No equivalent tool in target**: State the gap explicitly, suggest the closest alternative, and note any behavioral difference.
- **Adaptation would break core logic**: The skill may not be portable. Recommend `skill-creator` to build a purpose-built replacement instead.
- **Source skill is ambiguous**: State each assumption you made and why. Flag any assumption the user should verify.

# Next steps

After adapting a skill:
- Verify the adapted skill works → `skill-evaluation`
- Update routing for the new context → `skill-trigger-optimization`
- Review safety if the adaptation changed tool usage → `skill-safety-review`
