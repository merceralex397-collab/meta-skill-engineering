---
name: market-research
description: >-
  Sizes markets (TAM/SAM/SOM), maps competitive landscapes, and synthesizes
  industry trends into structured investment or go-to-market recommendations.
  Trigger phrases: "market size", "TAM SAM SOM", "competitive analysis",
  "market opportunity", "industry analysis", "addressable market", "market
  landscape", "Porter's Five Forces", "go-to-market research".
  Do NOT use for: single-company financial analysis (use financial-tracker-ops),
  building spreadsheets from raw data (use spreadsheet-analysis), one-off fact
  lookups with no synthesis, or competitor UX teardowns (use competitor-teardown).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: market-research
  maturity: draft
  risk: low
  tags: [market-sizing, competitive-analysis, industry-research, strategy]
---

# Purpose

Produce structured, evidence-backed market analyses that size an opportunity, map the competitive landscape, identify trends and risks, and deliver an actionable recommendation. Every claim must trace to a source with an explicit quality tier.

# When to use this skill

- The user asks to size a market, estimate TAM/SAM/SOM, or evaluate a market opportunity.
- A task requires competitive landscape mapping — identifying direct, indirect, and substitute competitors.
- The user needs a Porter's Five Forces analysis or an industry structure assessment.
- A go-to-market plan, pitch deck, or business case needs supporting market data.
- A repo contains business-plan artifacts (e.g., `docs/market-analysis.md`) that need updating with fresh research.

# Do not use this skill when

- The task is a single-company financial deep-dive — use `financial-tracker-ops`.
- The user wants to build or transform spreadsheet data — use `spreadsheet-analysis`.
- The request is a UX/product feature comparison of a single competitor — use `competitor-teardown`.
- The question is a quick factual lookup ("What is Company X's revenue?") with no synthesis needed.
- The user is scouting domain names or digital assets — use `domain-scouting`.

# Operating procedure

## Step 1 — Define the research question and scope

- State the **specific decision** the research must inform (e.g., "Should we enter the US pet-insurance market?").
- Define geographic scope, time horizon, and customer segment boundaries.
- Agree on the level of precision required: order-of-magnitude vs. bottom-up model.

## Step 2 — Gather and tier sources

Apply the **source quality hierarchy** to every piece of evidence:

| Tier | Source type | Example | Reliability |
|------|-----------|---------|-------------|
| 1 | Government / regulatory data | Census Bureau, SEC filings, Eurostat | Highest |
| 2 | Industry association reports | IBIS World, Gartner, IDC | High |
| 3 | Analyst estimates / equity research | Morgan Stanley blue papers, CB Insights | Medium-high |
| 4 | Company disclosures (non-audited) | Press releases, investor decks | Medium |
| 5 | Journalistic / blog sources | TechCrunch, industry blogs | Low — corroborate before citing |

Distinguish **primary research** (surveys, interviews, proprietary data) from **secondary research** (published reports, databases). Note which type backs each claim.

## Step 3 — Size the market (TAM / SAM / SOM)

1. **TAM (Total Addressable Market):** Estimate the total revenue opportunity if 100% market share were achieved globally (or within the defined geography). Use **top-down** (industry report total × relevant segment %) AND **bottom-up** (unit price × total potential buyers) approaches; compare the two and explain variance.
2. **SAM (Serviceable Addressable Market):** Narrow TAM by the segments the product can actually reach given current capabilities, channels, and geography.
3. **SOM (Serviceable Obtainable Market):** Estimate realistic capture within SAM based on competitive position, go-to-market capacity, and comparable company benchmarks. SOM should never exceed 5–15% of SAM for an early entrant without extraordinary differentiation.

State every assumption explicitly. Provide low / base / high scenarios.

## Step 4 — Map the competitive landscape

Classify every identified competitor:

- **Direct competitors:** Same product, same customer segment.
- **Indirect competitors:** Different product, same customer need.
- **Substitute competitors:** Different category that eliminates the need entirely.

For each competitor capture: name, estimated revenue or funding, primary value proposition, key differentiator, and weakness.

## Step 5 — Conduct Porter's Five Forces analysis

Assess the industry across all five forces, rating each **Low / Medium / High**:

1. **Threat of new entrants** — barriers to entry, capital requirements, regulatory hurdles.
2. **Bargaining power of suppliers** — supplier concentration, switching costs, input uniqueness.
3. **Bargaining power of buyers** — buyer concentration, price sensitivity, switching costs.
4. **Threat of substitutes** — availability of alternatives, relative price-performance.
5. **Competitive rivalry** — number of competitors, industry growth rate, exit barriers.

Summarize overall industry attractiveness.

## Step 6 — Identify trends, risks, and timing

- List 3–5 macro trends affecting the market (regulatory, technological, demographic, behavioral).
- Identify the top 3 risks with likelihood and impact ratings.
- Assess market timing: is the window opening, peak, or closing?

## Step 7 — Synthesize recommendation

Produce a clear, directional recommendation tied to the original decision question. State confidence level (High / Medium / Low) and list the **top 3 things that would change this recommendation** if new evidence emerged.

# Decision rules

- Never cite a Tier 5 source as the sole evidence for a quantitative claim; corroborate with Tier 1–3.
- If top-down and bottom-up TAM estimates diverge by more than 3×, investigate and explain the gap before proceeding.
- SOM estimates must include an explicit comparables basis — cite at least one analogous company's market-share trajectory.
- If fewer than 3 independent sources confirm a market-size figure, flag it as "low-confidence estimate."
- When data is older than 2 years, apply a stated growth rate to extrapolate and mark the figure as "projected."
- Separate fact from inference: use "data shows" for sourced claims and "this suggests" for interpretive conclusions.

# Output structure

Every market research deliverable must include these sections in order:

1. **Market Definition** — Decision question, scope boundaries, customer segment, geography, time horizon.
2. **Size Estimate (TAM / SAM / SOM)** — Three-tier sizing with methodology, assumptions, low/base/high scenarios, and source citations.
3. **Competitive Map** — Table of direct, indirect, and substitute competitors with key attributes.
4. **Industry Structure (Porter's Five Forces)** — Force-by-force ratings with supporting evidence.
5. **Trends & Timing** — Macro trends, market-timing assessment.
6. **Risks** — Top risks with likelihood × impact matrix.
7. **Recommendation & Confidence** — Directional recommendation, confidence level, reversal conditions.

# Anti-patterns

- **Vanity metrics:** Citing the largest possible TAM number without narrowing to SAM/SOM to make an opportunity look bigger than it is. Always size down to the obtainable slice.
- **Confirmation bias:** Selectively citing sources that support a predetermined conclusion while ignoring contradictory data. Present the strongest counter-evidence explicitly.
- **Extrapolating from anecdote:** Treating a single customer interview or case study as representative of the entire market. Require quantitative corroboration for claims about market behavior.
- **Stale data presented as current:** Using 5-year-old figures without adjustment or caveat. Always note data vintage and apply growth-rate extrapolation where needed.
- **Ignoring substitutes:** Mapping only direct competitors and missing the substitute products that could collapse the category entirely.
- **Precision theater:** Presenting a TAM as "$4.237B" when the underlying data supports only an order-of-magnitude estimate. Match precision to evidence quality.

# Related skills

- `competitor-teardown` — Deep-dive on a single competitor's product, pricing, and positioning.
- `financial-tracker-ops` — Financial modeling and projection once market sizing is complete.
- `spreadsheet-analysis` — Structuring raw data into analysis-ready formats.
- `domain-scouting` — Evaluating digital asset opportunities adjacent to a market entry.
- `business-idea-evaluation` — Applying market research findings to a go/no-go decision on a specific idea.

# Failure handling

- If the market is too nascent for reliable sizing data, state that explicitly, provide analogous-market comparisons, and lower confidence to "Low."
- If competitive data is sparse (e.g., private companies with no disclosures), note gaps in the competitive map and suggest primary-research methods to fill them.
- If the decision question is ambiguous, restate it as a concrete yes/no or rank-order question and confirm scope before proceeding.
- If the task is better served by a single-competitor deep-dive, redirect to `competitor-teardown`.
