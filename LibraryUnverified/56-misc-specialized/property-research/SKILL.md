---
name: property-research
description: >-
  Evaluates real estate investment opportunities through comparable sales analysis, financial
  modeling (cap rate, cash-on-cash, GRM), area profiling, and due diligence checklists.
  Trigger: "evaluate this property", "run comps", "rental yield", "cap rate analysis",
  "property due diligence", "neighborhood research", "investment property".
  Do NOT use for general market-trend research without a specific property or area
  (use market-research), commercial lease negotiation, or mortgage broker advice.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: property-research
  maturity: draft
  risk: low
  tags: [property, real-estate, investment-analysis, due-diligence, comps]
---

# Purpose

Provides a structured, repeatable framework for evaluating residential or small
commercial real estate. Covers property scoring, financial viability, comparable
sales analysis, area profiling, and risk assessment so that buy/hold/pass decisions
are grounded in evidence rather than gut feeling.

# When to use this skill

- User asks to evaluate a specific property or shortlist of properties
- Task requires comparable sales ("comps") analysis for a target address or area
- User needs a rental yield calculation, cap rate estimate, or cash-on-cash return model
- Due diligence checklist is needed before making an offer
- Area research is requested: crime stats, schools, transport, development pipeline
- Budget or renovation feasibility modeling for a potential acquisition

# Do not use this skill when

- The task is broad market-trend research with no specific property — use `market-research`
- The user needs a spreadsheet built or audited — use `spreadsheet-analysis`
- The task is competitive business analysis, not real estate — use `competitor-teardown`
- The user needs legal or tax advice — flag this as out of scope and recommend a professional
- The question is a simple factual lookup ("what's the average price in X?") with no decision support

# Operating procedure

## Step 1 — Define the investment thesis

1. Clarify the strategy: buy-and-hold rental, fix-and-flip, BRRRR, or owner-occupy.
2. Establish target metrics: minimum cap rate, target cash-on-cash return, max purchase price.
3. Document the buyer's financing assumptions: down payment %, interest rate, loan term.
4. Record any hard constraints: geography, property type, bedroom count, condition tolerance.

## Step 2 — Comparable sales analysis (comps)

1. Identify 3–6 recent sales (last 6 months, within 0.5 mile) of similar property type, size (±20% sq ft), and condition.
2. For each comp, record: address, sale price, price per sq ft, days on market, condition, date sold.
3. Calculate adjusted comp values: apply adjustments for differences in bedrooms, bathrooms, lot size, condition, and age.
4. Derive a fair market value range: low (worst comp adjusted), midpoint (median adjusted), high (best comp adjusted).
5. Flag any comps that are outliers and explain why (distressed sale, estate sale, off-market).

## Step 3 — Financial modeling

Calculate each metric and show the formula used:

| Metric | Formula |
|---|---|
| **Gross Rent Multiplier (GRM)** | Purchase Price ÷ Annual Gross Rent |
| **Cap Rate** | Net Operating Income (NOI) ÷ Purchase Price × 100 |
| **Cash-on-Cash Return** | Annual Pre-Tax Cash Flow ÷ Total Cash Invested × 100 |
| **Price per Sq Ft** | Purchase Price ÷ Livable Sq Ft |
| **Gross Yield** | Annual Gross Rent ÷ Purchase Price × 100 |
| **Net Yield** | (Annual Gross Rent − Annual Expenses) ÷ Purchase Price × 100 |

Expense line items to include in NOI calculation:
- Property tax, insurance, property management (8–10% of gross rent)
- Maintenance reserve (5–10% of gross rent), vacancy allowance (5–8%)
- HOA/body corporate fees, utilities (if landlord-paid), capex reserve

## Step 4 — Due diligence checklist

Work through each item; mark as ✅ verified, ⚠️ needs attention, or ❌ blocker:

1. **Title search** — confirm clean title, no liens, easements, or encumbrances
2. **Zoning verification** — confirm permitted use matches investment strategy
3. **Environmental assessment** — check flood zone, contamination history, asbestos/lead (pre-1978 builds)
4. **Building inspection** — structural, roof, plumbing, electrical, HVAC, pest
5. **Survey and boundaries** — confirm lot lines match listing
6. **Insurance quotes** — obtain quotes; flag if flood or special hazard insurance required
7. **Rent roll verification** (if tenanted) — confirm current leases, deposits, arrears
8. **Code compliance** — check for unpermitted work, outstanding violations

## Step 5 — Area profiling

Research and score each factor on a 1–5 scale with evidence:

| Factor | Data sources |
|---|---|
| **Crime stats** | Local police data, CrimeMapping, NeighborhoodScout |
| **School ratings** | GreatSchools, Ofsted (UK), state education dept |
| **Transport links** | Walk Score, Transit Score, proximity to highway/rail |
| **Employment** | Major employers, unemployment rate, job growth trend |
| **Development pipeline** | Council/city planning applications, new permits, infrastructure projects |
| **Demographics** | Population growth, median income, age distribution, renter vs owner ratio |
| **Amenities** | Grocery, healthcare, parks, restaurants within 1 mile |

## Step 6 — Budget modeling

Build a complete acquisition budget:

1. **Purchase price** — based on comp-supported offer price
2. **Closing costs** — estimate 2–5% of purchase price (stamp duty, legal, lender fees)
3. **Renovation estimate** — itemized by trade (cosmetic, structural, systems), with 15% contingency
4. **Carrying costs** — monthly holding cost × estimated months to stabilize or flip
5. **Total cash invested** — down payment + closing + renovation + carrying costs
6. **Exit strategy** — target resale price (flip) or stabilized annual cash flow (hold), with timeline

## Step 7 — Synthesize and recommend

1. Compile the Property Scorecard (see output structure).
2. State a clear recommendation: **Buy**, **Negotiate** (with target price), **Pass**, or **Needs more data**.
3. Identify the top 3 risks and what would change the recommendation.
4. If data is missing, list exactly what is needed and where to get it.

# Decision rules

- **Cap rate < 4%** in a buy-and-hold strategy → flag as low-return unless strong appreciation thesis exists.
- **Cash-on-cash < 8%** → requires explicit justification (appreciation play, value-add opportunity).
- **GRM > 15** → rental income unlikely to cover debt service; stress-test assumptions.
- **Vacancy assumption < 5%** → override to minimum 5%; use 8% for areas with > 10% local vacancy.
- **Maintenance reserve < 5%** of gross rent → override to minimum 5%; use 10% for properties > 30 years old.
- **Comps older than 12 months** → discount reliability; flag as stale data.
- **Environmental red flags** (flood zone, contamination) → escalate to professional assessment before proceeding.
- **Unpermitted work detected** → quantify cost to rectify or value discount before modeling.
- Never model with 100% occupancy — always apply a vacancy factor.
- Always separate the renovation budget from the purchase price in return calculations.

# Output structure

Deliver these sections in order:

## 1. Property Scorecard

| Dimension | Score (1–5) | Key evidence |
|---|---|---|
| Location | | |
| Condition | | |
| Financial return | | |
| Risk profile | | |
| Area fundamentals | | |
| **Overall** | | |

## 2. Comparable Sales Analysis

Table of 3–6 comps with adjustments and derived fair market value range.

## 3. Financial Model

Full income/expense breakdown, all six metrics calculated, sensitivity table showing
returns at ±5% purchase price and ±10% rent assumptions.

## 4. Area Profile

Scored factor table from Step 5 with data sources cited.

## 5. Due Diligence Status

Checklist from Step 4 with status markers and notes on any open items.

## 6. Budget Summary

Itemized budget from Step 6 with total cash required and contingency.

## 7. Risk Assessment

Top 3–5 risks ranked by likelihood × impact, with mitigation actions.

## 8. Recommendation

Clear Buy / Negotiate / Pass / Needs More Data verdict with reasoning.

# Anti-patterns

- **Ignoring maintenance reserves** — modeling with zero maintenance creates fantasy returns; always include 5–10% of gross rent.
- **Assuming 100% occupancy** — no market has zero vacancy; minimum 5% vacancy factor, higher in soft markets.
- **Skipping title search** — liens, easements, and encumbrances can destroy a deal post-closing.
- **Emotional bidding** — exceeding the comp-supported price range because "it feels right" is not analysis.
- **Using asking price as market value** — always validate against actual closed comps, not listings.
- **Single-comp analysis** — one comp is an anecdote; use 3–6 for statistical confidence.
- **Ignoring capex** — roofs, HVAC, and plumbing are when-not-if expenses; model them.
- **Projecting rent increases without evidence** — use historical rent growth data, not wishful thinking.

# Related skills

- `market-research` — broader market trend analysis that feeds into area profiling
- `spreadsheet-analysis` — build and audit the financial model workbook
- `financial-tracker-ops` — ongoing tracking of rental income and expenses post-acquisition
- `competitor-teardown` — analyzing competing listings or investment opportunities
- `document-to-structured-data` — extracting data from property listings, inspection reports, or title documents

# Failure handling

- If the user provides no address or area, ask for at least a city/suburb and property type before proceeding.
- If comp data is unavailable or stale (>12 months), explicitly state this, widen the search radius, and lower confidence.
- If key financial inputs are missing (purchase price, rent estimate), provide a template for the user to fill in rather than guessing.
- If due diligence reveals a potential blocker (title issue, environmental), halt the financial analysis and escalate the blocker first.
- If the investment strategy is unclear, present returns under both buy-and-hold and flip scenarios and ask the user to confirm.
