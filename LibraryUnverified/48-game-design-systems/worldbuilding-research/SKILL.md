---
name: worldbuilding-research
description: >-
  Builds, validates, and maintains fictional world lore — cultures, magic/tech
  systems, timelines, and internal consistency. Trigger phrases: "worldbuilding",
  "lore bible", "magic system", "fictional culture", "timeline consistency",
  "world bible", "canon check", "fantasy setting", "sci-fi universe design",
  "consistency audit". Do NOT use for: real-world market or industry research
  (use market-research), generating visual art prompts for existing worlds (use
  image-prompt-direction), or writing prose/dialogue (this skill builds the
  world, not the narrative).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: worldbuilding-research
  maturity: draft
  risk: low
  tags: [worldbuilding, lore, fiction, consistency, creative-writing]
---

# Purpose

Design, document, and validate fictional worlds with internally consistent lore, cultures, systems, and timelines. Every world element must be traceable through a canon hierarchy, and every new addition must pass a consistency check against established lore.

# When to use this skill

- The user asks to create, expand, or audit a fictional world, setting, or universe.
- A repo contains world-bible files (e.g., `docs/lore/`, `world-bible.md`) that need new entries or consistency checks.
- The task involves designing a magic system, technology framework, or power system with rules and limitations.
- The user needs a culture, faction, or civilization designed from environmental first principles.
- A timeline needs construction, extension, or paradox/conflict auditing.
- The user asks to validate whether a new story element is consistent with established canon.

# Do not use this skill when

- The task is real-world market, industry, or competitor research — use `market-research`.
- The user wants to generate image or art prompts for an existing world — use `image-prompt-direction`.
- The request is to write narrative prose, dialogue, or plot outlines — this skill builds the setting, not the story.
- The task is purely about character personality or arc design without world-system interaction.
- The user needs data analysis on a fictional game's statistics — use `starcraft-data-analysis` or `spreadsheet-analysis`.

# Operating procedure

## Step 1 — Establish canon hierarchy

Before creating or modifying any lore, establish the canon tiers for the world:

| Tier | Canon level | Definition | Override behavior |
|------|------------|-----------|-------------------|
| P | Primary canon | Author-approved, published material; core world-bible entries | Cannot be contradicted |
| S | Secondary canon | Supplementary material, approved expansions, author notes | Yields to Primary if conflict found |
| I | Inferred canon | Logical deductions from P and S material; gap-fills | Yields to S and P; must be flagged as inferred |

When adding new lore, explicitly tag its canon tier. When a conflict is detected between tiers, the higher tier wins and the conflict must be documented.

## Step 2 — Culture generation (geography-first framework)

Build cultures from environmental causation, not from aesthetic borrowing:

1. **Geography & climate** — Define terrain, weather patterns, natural boundaries, and key resources.
2. **Resource base** — What can be grown, mined, hunted, or harvested? What is scarce?
3. **Economy & trade** — What does this culture produce, what must it import, and who are its trade partners or rivals?
4. **Social structure** — How does resource control shape power? Who rules and why? What class or caste divisions emerge from economic realities?
5. **Beliefs & values** — What worldview arises from the relationship between people and environment? What do they celebrate, fear, or consider taboo?
6. **Aesthetics & expression** — Only after the above are established, derive art, architecture, clothing, and language patterns as *expressions* of the deeper structure.

Document each step's causal chain so that the culture can be stress-tested for coherence.

## Step 3 — Magic / technology system design

Use the **cost-limitation framework** (referencing Sanderson's Laws as a structural guide):

1. **Sanderson's First Law (resolution):** An author's ability to solve problems with magic is proportional to how well the reader understands that magic. → Define clear, documented rules for any system used to resolve conflict.
2. **Sanderson's Second Law (limitation):** Weaknesses and limitations are more interesting than powers. → Every capability must have a cost, limit, or trade-off. Document these in a constraints table:

| Capability | Cost / fuel | Hard limit | Side effect | Who can access it |
|-----------|------------|-----------|-------------|-------------------|

3. **Sanderson's Third Law (expansion):** Before adding new powers, explore the implications of existing ones. → Before introducing a new system element, audit existing capabilities for unexplored consequences.
4. **Internal consistency rule:** The system must not contradict itself. If fire magic requires ambient heat, it cannot work at full power in a frozen wasteland without an established exception mechanism.

## Step 4 — Timeline construction and paradox checking

1. Create a **master chronology** with entries formatted as: `[Year/Era] — [Event] — [Canon tier] — [Causal link to prior event]`.
2. For each new entry, run a **paradox check**:
   - Does this event contradict any Primary canon event?
   - Does the causal chain hold? (Can event B happen if event A's consequences are followed through?)
   - Are character lifespans, travel times, and technology levels consistent with the stated era?
3. For time-travel or prophecy elements, document the **temporal model** explicitly (fixed timeline, branching, bootstrap paradox allowed, etc.) and validate all events against that model.

## Step 5 — Character-world interaction validation

When a character interacts with world systems, validate:

- **Access:** Does this character have established access to the magic/tech/resource they're using?
- **Cost:** Is the character paying the established cost? Are consequences shown?
- **Knowledge:** Does the character know what they'd need to know, given their background and the world's information-flow rules?
- **Impact:** Does the character's action have ripple effects consistent with the world's established systems?

Flag any interaction that requires a new rule or exception, and escalate for canon-tier decision.

## Step 6 — Gap analysis and expansion recommendations

After any audit or new-entry session:

1. Identify **underdeveloped areas** — regions, time periods, cultural practices, or system edge-cases that lack documentation.
2. Prioritize gaps by **narrative impact** — which gaps are most likely to cause a consistency error if a story passes through that area?
3. Recommend specific world-bible entries to fill high-priority gaps.

# Decision rules

- **Canon conflicts are errors, not flavor.** Never paper over a contradiction; document it, identify which tier wins, and update the losing entry.
- **Cultures must have causal depth.** If a cultural trait cannot be traced back through the geography-first framework to at least Step 2 (resources), it needs reworking or explicit justification.
- **Magic/tech systems must close.** Every capability needs a documented cost and limit before it can be used in narrative context. Open-ended powers are flagged as incomplete.
- **Timelines are append-only at the Primary tier.** Primary canon timeline entries cannot be silently modified; changes require a retcon entry that documents what changed and why.
- **Inferred canon is provisional.** Any I-tier entry can be overridden without a retcon process, but the override must be documented.
- **Real-world cultural borrowing requires transformation.** Never map a real-world culture 1:1 onto a fictional one. Use the geography-first framework to build from first principles, borrowing structural patterns at most.

# Output structure

Every worldbuilding deliverable must use one of these formats:

## World Bible Entry

```
## [Entry Title]
- Canon tier: P / S / I
- Category: Geography | Culture | System | History | Faction | Species
- Connected entries: [list of related world-bible entries]
- Summary: [2-3 sentence overview]
- Detail: [full description with causal reasoning]
- Open questions: [unresolved elements flagged for future development]
```

## Consistency Check Report

```
## Consistency Check: [Element Being Checked]
- Checked against: [list of canon entries examined]
- Status: CONSISTENT / CONFLICT FOUND / INSUFFICIENT DATA
- Conflicts: [if any — entry vs. entry, with tier comparison and resolution]
- Assumptions made: [any I-tier inferences required to complete the check]
```

## Gap Analysis

```
## Gap Analysis: [World / Region / System]
- Coverage: [% of expected world-bible categories documented]
- Critical gaps: [gaps that could cause narrative consistency errors]
- Recommended entries: [prioritized list of entries to create]
- Estimated effort: [scope of work to fill critical gaps]
```

# Anti-patterns

- **Kitchen-sink worldbuilding:** Adding detail for its own sake without narrative or structural purpose. Every entry should serve either consistency, immersion, or plot enablement. If it does none of these, it's clutter.
- **Inconsistent power scaling:** A character or faction's power level fluctuating without in-world justification. If a mage struggles with a locked door in Chapter 3 but levels a fortress in Chapter 7, the system must explain the difference.
- **Culture-as-costume:** Giving a civilization distinctive clothing, food, and architecture without underlying economic, geographic, or historical reasons. Aesthetics without causation reads as shallow and risks offensive stereotyping.
- **Floating timeline syndrome:** Events that have no anchored dates or causal links, making it impossible to determine sequencing or detect contradictions.
- **Retroactive power-ups:** Introducing a new system capability specifically to solve a plot problem, with no prior foreshadowing or cost structure. This undermines the system's credibility.
- **Monoculture planets/continents:** An entire landmass or species sharing one culture, one language, one government. Real diversity emerges from geographic variation — apply the same principle to fictional worlds.

# Related skills

- `image-prompt-direction` — Translating established world-bible visual descriptions into art generation prompts.
- `competitor-teardown` — Analyzing published fictional worlds for structural patterns (when used as creative research).
- `cv-cover-letter` — No direct relation (negative example: do not route here).

# Failure handling

- If the existing world bible is incomplete or contradictory, begin with a **consistency audit** before adding new material. Document all found conflicts as a Consistency Check Report.
- If the user's request would contradict Primary canon, present the conflict explicitly and offer alternatives: retcon (with documented cost), reinterpretation, or abandonment of the proposed element.
- If the genre or system type is unfamiliar (e.g., hard sci-fi thermodynamics, specific mythological traditions), state knowledge limits and recommend external reference material rather than fabricating plausible-sounding but incorrect rules.
- If the scope is too broad ("build me a whole world"), negotiate a starting scope: one region, one era, one culture, then expand iteratively.
