---
name: skill-variant-splitting
description: >-
  Split a broad skill into focused variants along stack, platform, scope, or
  domain axes. Use when "this skill does too much", "split this skill",
  "create variants for X and Y", a skill has disjoint "For X" / "For Y"
  sections, triggers on unrelated inputs, or has a conditional-branch-heavy
  procedure. Do not use for porting a skill to a different context
  (skill-adaptation), trigger-only fixes (skill-trigger-optimization), or
  catalog-level reorganization (skill-catalog-curation).
---

# Purpose

Split a skill that has grown too broad into focused variants that route precisely and perform well. A skill covering too many cases develops vague triggers, bloated procedures, and diluted output.

# When to use

- User says "this skill does too much", "split this skill", "create variants"
- Skill has disjoint sections ("For Python:" / "For JavaScript:")
- Skill triggers on unrelated inputs causing routing confusion
- Procedure has conditional branches per input type
- Description exceeds two sentences trying to cover all cases

Do NOT use when:
- Skill is focused and working well
- Variations are minor enough for overlays
- Problem is trigger wording only → `skill-trigger-optimization`
- Porting a working skill to a different context → `skill-adaptation`
- Reorganizing skills at catalog level → `skill-catalog-curation`
- Skill genuinely handles one coherent task

# Procedure

1. **Identify splitting signals**
   - Count distinct "modes" — if/else branches, "For X:" sections
   - Count non-overlapping trigger clusters
   - Check whether output format varies by case
   - Check whether different contexts need different tool access or behaviors

2. **Determine split axis**
   - **Stack**: e.g. `skill-testing-python`, `skill-testing-javascript`
   - **Platform**: e.g. `skill-deploy-aws`, `skill-deploy-gcp`
   - **Scope**: e.g. `skill-review-quick`, `skill-review-thorough`
   - **Domain**: e.g. `skill-api-rest`, `skill-api-graphql`

   **Axis selection priority** (when multiple axes seem viable):
   1. Prefer the axis that produces the **fewest variants** while still eliminating conditional branches.
   2. If two axes produce the same variant count, prefer the axis whose variants have the **most distinct trigger vocabularies** (easier routing — fewer false positives).
   3. If still tied, prefer: **Stack > Domain > Platform > Scope**. Stack splits tend to have the cleanest boundaries because different stacks use different terminology.
   4. **Red flag:** If any axis produces >5 variants, the skill may need hierarchical organization (umbrella router + sub-skills) instead of a flat split.

3. **Define variants**
   - Each variant passes the "one sentence" test — scope fits in a single clear sentence
   - Each has distinct, non-overlapping triggers
   - Each has a focused, linear procedure without conditional branches

4. **Extract shared core** (if any)
   - Identify logic common across all variants
   - If shared content exceeds 30% of each variant's total length, create a base skill with extensions
   - If shared content is under 30%, create fully independent variants
   - If creating a base skill: put shared procedure in the base, variant-specific steps in each variant's SKILL.md

5. **Write each variant**
   - Name: `{original}-{variant}`
   - Action-verb-first description
   - Simplified procedure
   - Variant-specific examples

6. **Update routing**
   - Each variant's "Do NOT use when" references siblings by name
   - Example: "Do NOT use for GraphQL APIs → `api-testing-graphql`"

7. **Verify coverage**
   - Every original use case handled by exactly one variant
   - No gaps between variants
   - No overlapping triggers

8. **Deprecate or repurpose original**
   - Fully replaced → deprecate
   - Some general cases remain → keep as lightweight router

# Output contract

Produce a markdown document with this structure:

```
## Variant Split: [original skill name]

### Split Axis
[Stack | Platform | Scope | Domain]: [one-line reasoning]

### Variants
| Variant | Scope | Key Triggers |
|---------|-------|-------------|
| original-a | [focused scope] | "trigger 1", "trigger 2" |
| original-b | [focused scope] | "trigger 3", "trigger 4" |

### Shared Core
[Shared logic description, or "None — fully independent variants"]

### Coverage Map
- [x] Case 1 → variant-a
- [x] Case 2 → variant-b
- [x] No gaps or overlaps confirmed

### Migration
Original skill: [deprecate | keep as router | keep for general cases]
```

# Failure handling

- **No clean axis found**: Skill may be unified after all. Report that no beneficial split axis exists and recommend `skill-improver` instead.
- **Variants overlap on triggers**: Rethink the axis. If overlap persists, document the ambiguous boundary and escalate the decision to the user.
- **More than 5 variants**: Use a two-level hierarchy with an umbrella router skill instead of a flat split.
- **Shared core larger than variant-specific content**: Splitting adds duplication without benefit. Recommend refining the original instead.

## Next steps

After splitting:
- Update the library index → `skill-catalog-curation`
- Evaluate each variant → `skill-evaluation`
- Deprecate the original if fully replaced → `skill-lifecycle-management`
