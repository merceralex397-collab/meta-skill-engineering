---
name: starcraft-data-analysis
description: >
  Analyzes StarCraft II replay data, meta-game trends, build orders, and balance
  statistics using structured methodology and SC2-specific data sources. Trigger
  phrases: "analyze this replay", "SC2 build order", "matchup win rates", "balance
  analysis", "meta-game trends", "patch impact on", "parse replay file". Do NOT use
  for playing the game, writing bots/AI agents, general gaming discussion, or
  StarCraft lore/story questions — this skill is strictly data analysis.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: starcraft-data-analysis
  maturity: draft
  risk: low
  tags: [starcraft2, replay-analysis, build-order, meta-game, balance, esports-analytics]
---

# Purpose

Perform structured analysis of StarCraft II data — replays, build orders, matchup statistics, balance trends, and meta-game shifts — using SC2-specific tools, data sources, and statistical methodology appropriate for competitive gaming analytics.

# When to use this skill

- User wants to analyze one or more SC2 replay files (.SC2Replay) for build orders, timings, or decision points
- User asks about matchup win rates, map balance, or meta-game trends across a patch or time period
- User wants build order optimization, deviation analysis, or benchmark comparisons
- User needs patch impact assessment: how balance changes affected matchup dynamics
- User asks for unit composition analysis, resource trade efficiency, or cost-effectiveness comparisons
- A project involves building or improving SC2 replay parsing or analytics tools

# Do not use this skill when

- The user wants to build a StarCraft II bot or AI agent — that is a software engineering task, not analysis
- The request is about StarCraft lore, campaign story, or universe worldbuilding — prefer `worldbuilding-research`
- The user wants general gaming advice not grounded in data ("how do I get better at SC2" without replays)
- The task is about a different game entirely, even other RTS games
- The user needs image/video analysis of gameplay footage — this skill works with structured data, not video

# Operating procedure

## Phase 1 — Scope and data identification

1. Clarify the analysis question:
   - **Replay analysis**: Single replay deep-dive, or batch analysis across many replays?
   - **Meta-game analysis**: Which matchup(s)? Which league/MMR range? Which patch/time period?
   - **Build order analysis**: Specific build comparison, or archetype detection across a set?
   - **Balance analysis**: Which units/changes? Pre-patch vs post-patch comparison?
2. Identify the data source and sample:
   - **User-provided replays**: .SC2Replay files to be parsed directly.
   - **Aggregated statistics**: Data from SC2ReplayStats, Aligulac, Spawning Tool, or Liquipedia.
   - **Patch notes**: Official Blizzard patch notes for the relevant game version.
3. Record the **patch version(s)** covered by the data. Never mix data from different balance patches without explicit notation.

## Phase 2 — Replay file parsing (when applicable)

4. Parse .SC2Replay files using **s2protocol** (Blizzard's official Python library):
   - Extract **header**: map name, game version, game length, player names, races, result.
   - Extract **tracker events**: unit births, unit deaths, upgrades, resource collection rates.
   - Extract **game events**: player commands, camera movements (for APM/attention analysis).
5. Reconstruct the **build order** for each player:
   - Record each production event with its **supply count** and **game-time timestamp** (seconds).
   - Normalize timestamps to real-time seconds (Faster game speed: 1 game second ≈ 0.725 real seconds).
   - Identify the build archetype by matching the first 30 supply of production against known build order databases (Spawning Tool).
6. Extract key timing benchmarks:
   - **Worker count at key intervals**: 3:00, 5:00, 7:00 (real-time).
   - **First expansion timing**.
   - **First tech structure timing** (e.g., Cybernetics Core, Factory, Lair).
   - **First army engagement timing and composition**.
   - **Supply block duration**: Total seconds supply-blocked in the first 10 minutes.

## Phase 3 — Build order analysis

7. Compare extracted build orders against **benchmark timings** for the archetype:
   - Source benchmarks from Spawning Tool's top-rated builds or professional replays at the relevant patch.
   - Calculate **deviation** from benchmark: seconds early/late for each key structure and unit.
   - Classify deviations: **On time** (±5s), **Slightly late** (6-15s), **Significantly late** (>15s), **Early** (suggests different build).
8. Identify **optimization targets**: the deviations with the highest impact on the build's effectiveness.
9. Detect **build order losses**: games where the outcome was largely determined by the build choice (e.g., blind counter, failed cheese).

## Phase 4 — Meta-game analysis

10. For meta-game questions, aggregate data across the specified sample:
    - **Matchup win rates**: Calculate win rate with **95% confidence intervals** using the Wilson score interval (appropriate for win/loss proportions).
    - **Minimum sample size**: Do not report win rates with fewer than 30 games in the sample. For meaningful conclusions, target n ≥ 100.
    - **Stratify by skill bracket**: Always separate data by league or MMR range. A build that dominates in Diamond may be irrelevant in GM. Standard brackets: Bronze-Gold, Platinum-Diamond, Master, Grandmaster, Professional.
    - **Map-specific analysis**: Report per-map win rates if the sample supports it. Note maps with known positional advantages.
11. Detect **meta shifts** by comparing archetype popularity and win rates across consecutive time periods or patches.

## Phase 5 — Balance analysis

12. For balance questions, apply these analytical frameworks:
    - **Unit cost-effectiveness**: Mineral + gas cost vs average combat value (damage dealt before death) in standard engagements. Use resource-normalized comparisons.
    - **Composition counters**: Map out the rock-paper-scissors relationships between common army compositions. Note where micro skill can invert theoretical counters.
    - **Resource trade efficiency**: In engagements from replays, calculate resources lost by each side. A positive trade is meaningful only if it accounts for the replaceability of the units (mineral-heavy losses are more recoverable than gas-heavy losses).
    - **Patch impact**: Compare pre-patch and post-patch statistics for affected units. Use a **minimum 2-week post-patch window** before drawing conclusions (meta needs time to adapt).
13. Distinguish between **statistical imbalance** (the numbers show a significant skew) and **perceived imbalance** (players feel something is unfair but the data doesn't support it). Report both when relevant.

## Phase 6 — Statistical methodology

14. Apply these statistical standards throughout:
    - **Confidence intervals**: Always report uncertainty. A 55% win rate with n=50 is very different from 55% with n=5000.
    - **Wilson score interval** for proportions (win rates), not the naive normal approximation.
    - **Effect size matters**: A statistically significant 51% win rate is not practically meaningful for balance discussion. Note the practical significance alongside statistical significance.
    - **Elo/MMR adjustment**: When comparing players of different skill levels, note whether the data controls for MMR. Raw win rates across all skill levels are misleading.
    - **Multiple comparisons**: When testing many matchup/map combinations, note the risk of false positives. Apply Bonferroni correction or note the unadjusted nature of the analysis.
    - **Correlation ≠ causation**: A build's win rate correlation with a map does not prove the map causes the advantage. Note confounding factors (player preferences, map pool timing).

# Decision rules

- **Always report patch version**: Every statistic must be tagged with the game patch it comes from. Cross-patch comparisons must be explicitly flagged.
- **Stratify by skill level or don't report**: Aggregate win rates across all leagues are misleading because different strategies dominate at different skill levels. If stratification is not possible, state this limitation prominently.
- **Sample size gates**: n < 30 → do not report a win rate. n 30-99 → report with "preliminary" qualifier and wide confidence interval. n ≥ 100 → report normally. n ≥ 500 → high confidence.
- **Recency of data matters**: SC2 meta evolves within days of a patch. Data older than one patch is historical context, not current analysis. Label it clearly.
- **Professional ≠ ladder**: Professional tournament meta diverges significantly from ladder meta. Never generalize from one to the other without noting the context.
- **Micro vs macro**: When analyzing unit effectiveness, note whether the analysis assumes standard engagement patterns or optimal micro. A Blink Stalker army has very different effectiveness with and without active Blink micro.

# Output requirements

Deliver all applicable sections (not all analyses require all sections):

1. **Analysis Summary** — One-paragraph executive summary of findings and key conclusions.
2. **Data Scope** — Patch version(s), date range, sample size, skill bracket(s), data source(s).
3. **Build Order Report** (if replay analysis) — Per-player build order table: Supply | Timestamp | Action | Benchmark | Deviation. Key timing benchmarks. Archetype classification.
4. **Statistical Tables** (if meta/balance analysis) — Win rates with confidence intervals, sample sizes, stratified by relevant dimensions. Resource trade tables if applicable.
5. **Visualizations Description** — Describe recommended charts/graphs: supply-over-time curves, win rate bar charts with error bars, resource trade scatter plots, army composition pie charts. Include the data needed to produce them.
6. **Meta Trends** — Archetype popularity shifts, emerging strategies, declining strategies, and the likely drivers of change.
7. **Recommendations** — Actionable conclusions: build order improvements with specific timing targets, matchup-specific strategy suggestions with supporting evidence, or balance assessment with confidence level.

# Anti-patterns

- **Small sample conclusions**: Declaring a matchup "imbalanced" based on 20 games. Require statistical rigor and minimum sample sizes before making balance claims.
- **Ignoring skill brackets**: Reporting a 60% win rate for a strategy without noting it was measured in Gold league. Strategy effectiveness is highly skill-dependent.
- **Patch-crossing without notation**: Mixing data from before and after a balance patch without flagging the boundary. This invalidates most conclusions.
- **Anecdote as evidence**: "I saw a pro player lose to this build, so it must be OP" is not analysis. Require systematic data.
- **Ignoring map context**: Reporting strategy win rates without noting which maps the games were played on. Map geometry heavily influences strategy viability.
- **Confusing correlation with causation**: A race's win rate dropped after a patch that didn't change that race's units. The cause may be indirect (opponents got stronger) — don't attribute causation without mechanism.
- **APM worship**: High APM does not equal better play. Spam-clicking inflates APM without adding value. Focus on effective actions per minute and decision quality.
- **Single-game analysis overgeneralization**: Drawing broad meta conclusions from one replay or one tournament series. Single games illustrate; aggregates prove.

# Data sources

Reference these when conducting analysis:

- **s2protocol** (github.com/Blizzard/s2protocol) — Official Blizzard replay parser for .SC2Replay files.
- **Aligulac** (aligulac.com) — Professional and semi-professional match results, Elo ratings, player statistics. Best for pro-level analysis.
- **SC2ReplayStats** (sc2replaystats.com) — Ladder replay aggregation, build order statistics, per-league breakdowns.
- **Spawning Tool** (spawningtool.com) — Build order database with timing benchmarks, annotated builds, archetype classification.
- **Liquipedia** (liquipedia.net/starcraft2) — Tournament results, player profiles, patch notes history, map pool information.
- **Blizzard Patch Notes** — Official balance change documentation for identifying patch boundaries.

# Related skills

- `spreadsheet-analysis` — For advanced statistical modeling or visualization of SC2 datasets
- `worldbuilding-research` — For StarCraft lore and universe questions (not gameplay data)
- `competitor-teardown` — Analogous analytical methodology applied to business competitors instead of game opponents
- `market-research` — If analyzing the SC2 esports market or streaming ecosystem as a business

# Failure handling

- If the user provides a replay file but no specific analysis question, perform a standard replay breakdown: build orders, key timings, engagement outcomes, and notable decision points.
- If the data source is unavailable or the API is down, state which source is inaccessible and what analysis is blocked. Proceed with available data and note the limitation.
- If the sample size is below the minimum threshold for the requested analysis, report this clearly and offer to proceed with appropriate caveats rather than refusing entirely.
- If the user asks about a patch version the agent has no data for, state the knowledge boundary and recommend checking the specific data sources listed above.
- If the analysis spans multiple patches, split the results by patch and present them separately with a comparison section noting what changed.
