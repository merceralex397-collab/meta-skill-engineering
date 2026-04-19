---
name: skill-trigger-optimization
description: >-
  Fix skill routing when the wrong skill fires or a skill won't fire at all.
  Use when the user says "this skill never fires", "wrong skill fired", 
  "fix the triggers", "why isn't this skill being used?", or when a skill 
  description is vague marketing copy that confuses the routing. Rewrites the 
  description field and "When to use" boundaries so the right skill triggers 
  on the right inputs. Do not use for fixing output quality when routing is 
  correct (use skill-improver) or structural anti-pattern audits (use skill-anti-patterns).
---

## Purpose

Fix skill routing by rewriting the `description` field and "When to use" / "Do NOT use" sections. The description is routing logic — it determines when a host invokes the skill. Bad descriptions cause undertriggering (skill doesn't fire when it should) or overtriggering (fires when it shouldn't).

## When to use

- Skill isn't triggering when it should (undertriggering)
- Skill fires on wrong inputs (overtriggering / false positives)
- User says "fix the triggers", "wrong skill fired", "why isn't this skill being used?"
- Eval shows poor routing precision or recall
- Description is vague, generic, or reads like marketing copy

## When NOT to use

- Skill triggers correctly but produces wrong output → `skill-improver`
- Skill has structural anti-patterns beyond just triggers → `skill-anti-patterns`
- Creating a new skill from scratch → `skill-creator`
- Entire skill needs rewrite → `skill-creator`
- Problem is procedure quality, not routing

## Procedure

1. **Diagnose the routing problem**
   - **Undertriggering**: List 3–5 phrases that should trigger but don't.
   - **Overtriggering**: List inputs that triggered but shouldn't. Identify which skill should have handled them.
   - **Confusion**: Name the confused skill and the distinguishing signal between them.

2. **Analyze current description**
   - Does the first phrase carry the most discriminating signal?
   - Does it include words users actually say?
   - Does it state what the skill produces?
   - Does it have "Do not use" anti-triggers naming alternatives?

3. **Identify discriminating signals**
   - Words or phrases that appear ONLY when this skill should trigger.
   - Context signals (file types, error patterns, user phrasing).
   - Minimal set that reliably separates this skill from neighbors.

4. **Rewrite the description**
   - **First phrase**: Verb + specific object (most discriminating signal).
   - **Include**: 2–3 realistic trigger phrases users say.
   - **Include**: What the skill produces.
   - **Exclude**: Generic filler ("helps with", "assists in").
   - **Exclude**: Marketing language ("powerful", "comprehensive").

   **Worked example — this skill following its own rules:**

   Before (bad):
   ```yaml
   description: >-
     Fix when the wrong skill fires or a skill won't fire at all.
     Use when the user says "this skill never fires"...
   ```
   Problems: Starts with "Fix when" which is vague; doesn't lead with the most discriminating signal.

   After (good — following Step 4 rules):
   ```yaml
   description: >-
     Fix skill routing when the wrong skill fires or a skill won't fire at all.
     Use when the user says "this skill never fires", "wrong skill fired", 
     "fix the triggers", "why isn't this skill being used?", or when a skill 
     description is vague marketing copy that confuses the routing. Rewrites the 
     description field and "When to use" boundaries so the right skill triggers 
     on the right inputs. Do not use for fixing output quality when routing is 
     correct (use skill-improver) or structural anti-pattern audits (use skill-anti-patterns).
   ```
   Transforms applied: Action verb first ("Fix"), specific scope ("skill routing"), concrete triggers (4 quoted user phrases), negative boundary (2 "do not" cases with referrals).

5. **Add explicit anti-triggers**
   - Format: "Do not use for [confusion case] (use `alternative-skill`)."
   - Cover the 2–3 most common false-positive scenarios.
   - Name the alternative skill explicitly.

6. **Verify the rewrite**
   - Undertriggering phrases now match the new description.
   - Overtriggering phrases no longer match.
   - No new confusion introduced with adjacent skills.

## Output contract

Produce a single markdown block with this structure:

```
## Trigger Optimization: [skill-name]

### Problem
Type: [Undertriggering | Overtriggering | Confusion]
Examples: [specific problematic inputs]

### Analysis
- Current first phrase: "[quoted]"
- Missing trigger words: [list]
- Overly generic terms: [list]
- Confused with: [skill names, if any]

### Rewritten Description
**Before**: "[full current description]"
**After**: "[full rewritten description]"

### Verification
- [ ] Undertriggering cases now match
- [ ] Overtriggering cases no longer match
- [ ] No new confusion with adjacent skills
```

# Failure handling

| Problem | Response |
|---------|----------|
| Can't identify discriminating signals | Skill scope may be too broad — recommend `skill-variant-splitting` to split by axis |
| Every fix introduces new false positives | Scopes overlap — redesign skill boundaries before optimizing triggers |
| No usage data available | Write 5 synthetic positive and 5 negative trigger phrases, optimize against those |
| Genuine overlap with another skill | Escalate to `skill-catalog-curation` to resolve the boundary at the library level |
| Host configuration prevents skill loading | Escalate to `skill-catalog-curation` to assess environment-specific skill availability |

# Next steps

After trigger optimization:
- Verify routing improved → `skill-evaluation`
- Build trigger tests → `skill-testing-harness`
- If routing problems persist → `skill-catalog-curation`
