---
name: game-design-systems
description: Designs core game systems including gameplay loops, economy, progression, combat formulas, difficulty scaling, loot tables, and balance methodology. Use when building or tuning engagement loops, XP curves, damage formulas, drop tables, economy sources/sinks, or data-driven config for game mechanics.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: game-design-systems
  maturity: draft
  risk: low
  tags: [game-design, systems, balance, economy, progression, combat]
---

# Purpose

Design, implement, and balance core game systems: engagement loops, economies, progression, combat formulas, difficulty scaling, loot/reward systems, and data-driven tuning via config tables and scriptable assets.

# When to use this skill

- Designing core/meta/session gameplay loops and player retention structure
- Building economy systems (currency sources/sinks, inflation control, dual currency)
- Creating progression systems (XP curves, skill trees, unlock gating, prestige)
- Implementing damage formulas, cooldown management, buff/debuff stacking rules
- Designing loot tables, weighted random drops, and pity timer mechanics
- Tuning difficulty curves and adaptive difficulty systems
- Setting up data-driven design with ScriptableObjects (Unity) or DataAssets (UE5)

# Do not use this skill when

- The task is about AI behavior implementation — use `ai-npc-behavior`
- The task is about UI layout for game screens — use `game-ui-hud`
- The task is engine-specific Blueprint wiring — use `blueprint-patterns`
- The task is purely about rendering or visual effects — use `shader-vfx`

# Operating procedure

1. **Define the core loop**: identify the moment-to-moment gameplay cycle (e.g., explore → encounter → resolve → reward). Map the engagement loop (action → feedback → motivation → action). Define session structure (how long is a typical play session, what is the session-end hook).
2. **Design the economy**:
   - Identify all **sources** (quest rewards, drops, daily login) and **sinks** (shop purchases, upgrades, consumables).
   - Model currency flow in a spreadsheet: `income_per_hour × play_hours = total_earned`. Ensure sinks ≥ 80% of sources to prevent inflation.
   - For dual currency: soft currency (earnable in-game) for routine purchases, hard currency (premium) for cosmetics or time-skips. Never gate core gameplay behind hard currency.
3. **Build progression systems**:
   - XP curve formula: `xp_required(level) = base_xp × level^exponent` (exponent 1.5–2.0 for gentle curve, 2.5+ for steep late-game).
   - Skill trees: ensure no dead-end paths, provide respec option, gate powerful nodes behind level requirements.
   - Prestige/ascension: reset progress with permanent bonuses (e.g., +5% XP gain per prestige level).
4. **Implement combat/interaction formulas**:
   - Damage: `final_damage = (base_attack × skill_multiplier - defense) × crit_modifier × elemental_bonus`. Clamp to minimum 1.
   - Cooldowns: fixed cooldown with optional CDR stat (`effective_cd = base_cd × (1 - cdr_percent)`). Cap CDR at 40–50%.
   - Buff stacking: define rules — additive (stack values), multiplicative (stack multipliers), or highest-only (no stack).
5. **Design loot and rewards**:
   - Drop tables: assign weights to items (`common: 60, uncommon: 25, rare: 10, epic: 4, legendary: 1`). Normalize weights to probabilities.
   - Pity timer: guarantee a rare+ drop every N attempts (e.g., after 50 pulls without epic, force one). Track counter per player.
   - Positive reinforcement: reward effort (XP), reward mastery (achievement), reward exploration (hidden items).
6. **Tune difficulty**:
   - Difficulty curve: enemies scale by `base_stat × (1 + level × scale_factor)`. Avoid linear scaling past midgame — use diminishing returns.
   - Adaptive difficulty: track player death rate over last 10 encounters. If >60%, reduce enemy HP by 10%. If <10%, increase damage by 5%. Invisible to player.
7. **Make it data-driven**: store all tuning values in external config (JSON, CSV, ScriptableObjects, DataAssets). Never hardcode balance numbers. Use `ScriptableObject` in Unity or `UDataAsset`/`UDataTable` in UE5 for designer-editable configs.

# Decision rules

- Always model economy math in a spreadsheet before implementing — intuition fails at scale.
- Prefer multiplicative stacking for damage buffs (creates interesting build choices) but cap total multiplier.
- XP curves should feel fast early (levels 1-10 in first session) and slow later (last 10% = 30% of total playtime).
- Drop rates below 1% require a pity timer to prevent frustration.
- Difficulty scaling should never make the player feel punished for being good — adaptive systems must be invisible.
- All balance values must be hot-reloadable or at minimum require only a data file change, not a code change.
- Playtest before shipping any balance change; simulation catches math errors but not feel.

# Output requirements

1. `Core Loop Diagram` — text description of engagement cycle with feedback arrows
2. `Economy Model` — sources, sinks, flow rates, inflation analysis
3. `Progression Table` — XP curve values for all levels, unlock requirements
4. `Combat Formulas` — damage, defense, cooldown, stacking rules with example calculations
5. `Loot Table` — item categories, weights, pity timer config
6. `Data Schema` — ScriptableObject/DataAsset/JSON structure for tuning values
7. `Balance Test Plan` — simulation parameters or playtest scenarios to validate

# References

- Schell, *The Art of Game Design* — lenses for loop analysis and player motivation
- Adams & Dormans, *Game Mechanics: Advanced Game Design* — economy and system modeling
- Unity: `ScriptableObject`, `JsonUtility`, Addressable config tables
- UE5: `UDataTable`, `UDataAsset`, `FTableRowBase` for CSV-driven balance
- Godot: `Resource` subclasses, `JSON.parse_string()` for config loading

# Related skills

- `game-ui-hud` — progression UI, loot popups, economy displays
- `ai-npc-behavior` — enemy difficulty feeds into AI aggression parameters
- `asset-pipeline` — data-driven configs as loadable assets
- `blueprint-patterns` — exposing balance values to UE5 designers via BP

# Failure handling

- If economy inflates (players accumulate currency with nothing to spend), add new sinks or reduce source rates.
- If progression feels grindy, flatten the XP curve exponent or add catchup mechanics.
- If combat feels spongy, check that defense scaling doesn't outpace attack scaling.
- If loot feels unrewarding, increase the visible rare drop rate or add guaranteed streak-breaker drops.
- If balance spreadsheet diverges from actual implementation, add automated tests that validate config values match expected ranges.
