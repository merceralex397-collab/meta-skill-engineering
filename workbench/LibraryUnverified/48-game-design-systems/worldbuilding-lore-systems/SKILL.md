---
name: worldbuilding-lore-systems
description: Designs and maintains worldbuilding data systems — lore databases (factions, characters, locations, events), narrative graph structures, codex/encyclopedia unlock systems, faction reputation matrices, timeline/history tracking, consistency validation, procedural lore generation, and engine integration via DataTables/DataAssets/Resources. Use when building or organizing game world lore, narrative data, or world bible documentation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: worldbuilding-lore-systems
  maturity: draft
  risk: low
  tags: [worldbuilding, lore, narrative, factions, codex]
---

# Purpose

Provide structured approaches for building and maintaining game world lore — from data-driven databases of factions, characters, and locations, to in-game codex systems, faction diplomacy state machines, timeline management, and consistency validation tools.

# When to use this skill

- Designing lore databases: entity tables for factions, characters, locations, events, items with cross-references
- Building narrative graph structures: branching dialogue trees, quest dependency chains, timeline event ordering
- Implementing player-facing codex/encyclopedia systems with unlock tracking and categorization
- Creating faction systems: relationship matrices, reputation values, diplomacy state machines (allied/neutral/hostile)
- Managing timelines: era definitions, causal event chains, chronological consistency
- Setting up data-driven lore: JSON/YAML lore files, wiki-style cross-references, search and filter
- Integrating lore data with engines: Unity ScriptableObjects/DataTables, UE5 DataAssets/DataTables, Godot Resources
- Writing world bible documentation: living design documents, term glossaries, style guides

# Do not use this skill when

- The task is about gameplay code or engine APIs — use `unity`, `unreal-engine`, or `ue5-blueprint`
- The task is about AI NPC behavior logic — use `ai-npc-behavior`
- The task is about dialogue system implementation (code), not dialogue content/structure — use the relevant engine skill

# Operating procedure

1. **Define the lore schema.** Identify core entity types: factions, characters, locations, events, items, cultures. Define fields for each (name, description, relationships, tags, era, status). Establish primary keys and cross-reference foreign keys.
2. **Choose data format.** For small projects: YAML/JSON files with `$ref`-style cross-references. For medium projects: SQLite or spreadsheet-backed DataTables. For large projects: dedicated CMS or wiki with structured export to engine-native format.
3. **Build the faction system.** Model factions with a relationship matrix (faction_a, faction_b, disposition: -100 to +100). Define state thresholds: hostile (<-50), unfriendly (-50 to -10), neutral (-10 to +10), friendly (+10 to +50), allied (>+50). Track reputation changes as events with timestamps and causes.
4. **Structure the timeline.** Define eras with start/end dates. Place events on the timeline with: date, location, participants, causes, consequences, and cross-references. Validate causal chains — an event's causes must precede it chronologically.
5. **Design the codex system.** Categories: world history, factions, characters, bestiary, items, locations. Each entry has: title, body text, unlock condition (quest, discovery, item), cross-links to related entries, and optional images/icons. Track player unlock state separately from content.
6. **Implement consistency checks.** Build validation rules: no orphan references (every referenced entity must exist), timeline causality (causes before effects), faction symmetry (if A is hostile to B, B is hostile to A), naming consistency (glossary terms used correctly).
7. **Integrate with engine.** Unity: `ScriptableObject` per lore entry with `[CreateAssetMenu]`, or CSV → DataTable import. UE5: `UDataAsset` subclasses or DataTable rows with `FTableRowBase` structs. Godot: `.tres` Resource files with exported properties. All engines: generate runtime lookup dictionaries keyed by ID.
8. **Context-aware dialogue.** Tag NPC dialogue lines with required lore states (faction reputation, quest progress, discovered codex entries). At runtime, filter dialogue options by checking the player's lore state against line prerequisites.
9. **Procedural lore generation.** Name generators using culture-specific phoneme tables and syllable rules. History generators using event templates with randomized participants and outcomes. Culture templates defining values, taboos, aesthetics, and naming conventions.
10. **Maintain the world bible.** Structure as: overview → cosmology → geography → cultures → factions → key characters → history → glossary. Keep it as a living document with version tracking. Flag speculative/unconfirmed entries separately from canonical lore.

# Decision rules

- Every lore entity needs a unique ID, a display name, and at least one cross-reference to another entity.
- Faction relationships must be bidirectional — update both sides of the matrix simultaneously.
- Codex entries should be unlockable, not all available from the start — gated discovery drives exploration.
- Prefer data-driven lore (JSON/DataTable) over hardcoded strings — enables localization and modding.
- Timeline events need explicit causality links — "because of X" not just "after X."
- Keep the world bible authoritative — if lore conflicts with the bible, the in-game data is wrong.

# Output requirements

1. `Lore Schema` — entity types, fields, relationships, and data format (JSON/YAML/DataTable)
2. `Faction Matrix` — relationship values between all factions with state thresholds
3. `Timeline` — chronological event list with causal links and era assignments
4. `Codex Structure` — categories, entry template, unlock conditions
5. `Consistency Report` — orphan references, causality violations, naming inconsistencies found

# References

- [UE5 DataTable Documentation](https://docs.unrealengine.com/5.0/en-US/data-driven-gameplay-elements-in-unreal-engine/)
- [Unity ScriptableObject Manual](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [Godot Resource System](https://docs.godotengine.org/en/stable/tutorials/scripting/resources.html)
- Kobold's Guide to Worldbuilding (general reference for world bible structure)
- Ryan Hipple — Game Architecture with ScriptableObjects (data container patterns)

# Related skills

- `unity-scriptableobject-events` — SO-based data containers for lore in Unity
- `ai-npc-behavior` — NPCs that use lore context for dialogue and behavior
- `game-design-systems` — gameplay systems that interact with lore (quest, reputation)
- `ue5-blueprint` — Blueprint-driven codex UI and faction logic in UE5

# Failure handling

- If cross-references point to missing entities, log a warning and display a placeholder — never crash on missing lore.
- If faction relationships are asymmetric, auto-correct by averaging both sides and log the discrepancy.
- If timeline causality is violated, flag the event and block publishing until resolved.
- If the world bible and in-game data disagree, treat the bible as source of truth and create a ticket to fix the game data.
