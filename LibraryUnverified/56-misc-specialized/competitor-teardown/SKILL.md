---
name: competitor-teardown
description: >-
  Performs structured competitive analysis using SWOT, Porter's Five Forces, feature
  comparison matrices, pricing teardowns, UX audits, and technical stack identification.
  Trigger: "analyze this competitor", "competitor comparison", "competitive analysis",
  "SWOT analysis", "feature comparison", "pricing teardown", "how does X compare to Y",
  "what is competitor X's stack", "competitive landscape".
  Do NOT use for general market sizing or trend research (use market-research), for
  evaluating your own business idea (use business-idea-evaluation), or for property/real
  estate comparisons (use property-research).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: competitor-teardown
  maturity: draft
  risk: low
  tags: [competitive-analysis, swot, porters-five-forces, pricing, ux-audit, market-intelligence]
---

# Purpose

Provides a repeatable framework for dissecting a competitor's product, business model,
pricing, UX, and technology into structured, evidence-based analysis. Produces actionable
intelligence that informs strategic decisions — not just a feature list, but a weighted
assessment of competitive position with implications for your own strategy.

# When to use this skill

- User asks to analyze, compare, or tear down one or more competitors
- Task requires a SWOT analysis with evidence for each quadrant
- User needs a feature comparison matrix with weighted scoring
- Pricing analysis is requested: plan tiers, value metrics, hidden fees
- UX teardown needed: signup flow, time-to-value, friction points
- Technical stack identification or architecture inference is requested
- User asks "how does X compare to Y" or "what are X's weaknesses"
- Strategic planning requires a competitive landscape overview
- Porter's Five Forces analysis is requested for an industry or market

# Do not use this skill when

- The task is broad market sizing, TAM/SAM/SOM, or trend research — use `market-research`
- The user is evaluating their own business idea, not a competitor — use `business-idea-evaluation`
- The comparison is between real estate properties — use `property-research`
- The task is internal product strategy without a competitive frame — use `tradeoff-analysis`
- The user needs a quick factual lookup ("what does company X do?") with no analytical framework

# Operating procedure

## Step 1 — Define scope and competitors

1. **Identify the focal company** — the user's product or the product being benchmarked against.
2. **Select competitors** — categorize each as:
   - **Direct** — same product category, same target customer (e.g., Figma vs. Sketch)
   - **Indirect** — different product, same job-to-be-done (e.g., Figma vs. pen and paper)
   - **Aspirational** — market leaders to benchmark against even if not direct competitors
3. **Define comparison dimensions** — which aspects matter? (features, pricing, UX, technology, business model, go-to-market)
4. **Confirm the decision** this analysis supports — "should we build feature X?", "where should we position our pricing?", "which market segment is underserved?"

## Step 2 — Competitor profiling

For each competitor, build a structured profile:

| Field | What to capture |
|---|---|
| **Company** | Name, founded, HQ, funding stage/revenue if known |
| **Product** | Core product description in 1–2 sentences |
| **Target customer** | Primary persona, company size, industry vertical |
| **Value proposition** | How they describe their own differentiation (use their own words from marketing) |
| **Distribution** | Primary acquisition channels: PLG, sales-led, partnerships, marketplace |
| **Key metrics** (if available) | Users, revenue, growth rate, market share |
| **Recent moves** | Last 6 months: product launches, pricing changes, acquisitions, partnerships |

## Step 3 — SWOT analysis

For each competitor, produce a SWOT matrix. **Every cell must include evidence.**

| Quadrant | Definition | Evidence requirement |
|---|---|---|
| **Strengths** | Internal advantages the competitor has today | Observable facts: feature, patent, team, brand, distribution, data moat |
| **Weaknesses** | Internal disadvantages or gaps | Documented: missing features, known bugs, negative reviews, churn signals |
| **Opportunities** | External trends the competitor could exploit | Market data: growing segment, regulatory change, technology shift |
| **Threats** | External risks to the competitor's position | Market data: new entrants, substitutes, regulatory risk, platform dependency |

Format as a 2×2 matrix with bullet points. Each bullet = one claim + one evidence citation.

## Step 4 — Feature comparison matrix

1. **Define feature categories** — group features into logical categories (e.g., Core, Collaboration, Integrations, Analytics, Security, Support).
2. **List features** — enumerate specific features within each category.
3. **Score each feature** per competitor on a consistent scale:
   - `0` = absent
   - `1` = basic / partial implementation
   - `2` = solid implementation, meets expectations
   - `3` = best-in-class, exceeds expectations
4. **Weight categories** — assign importance weights (1–5) based on target customer priorities.
5. **Calculate weighted scores** — (score × weight) per category, summed for an overall weighted total.
6. **Highlight differentiators** — features where scoring gaps are ≥ 2 points between competitors.

Present as a table with competitors as columns and features as rows.

## Step 5 — Pricing teardown

Analyze each competitor's pricing structure:

| Dimension | What to capture |
|---|---|
| **Plan tiers** | Free, Starter, Pro, Enterprise — name and price point of each |
| **Value metric** | What do they charge per? (seat, usage, feature gate, flat rate) |
| **Per-unit economics** | Cost per seat/unit at each tier; how does unit cost change with scale? |
| **Free tier limits** | What's included for free? What triggers the upgrade? |
| **Hidden fees** | Overage charges, implementation fees, support tiers, API rate limits |
| **Annual vs. monthly** | Discount for annual commitment; typical discount is 15–20% |
| **Enterprise pricing** | "Contact sales" signals; estimate from public data or reviews if possible |
| **Value perception** | Price-to-feature ratio; are they positioned as premium, mid-market, or budget? |

Produce a pricing comparison table and a **price-per-feature-point** calculation using the weighted scores from Step 4.

## Step 6 — UX teardown

Evaluate the user experience through these lenses:

1. **Signup flow analysis**
   - Count steps from landing page to first value-delivering action
   - Note friction points: required credit card, email verification, mandatory onboarding
   - Measure (or estimate) time-to-value: how long until a new user achieves their first "aha" moment?

2. **Core workflow efficiency**
   - Map the primary user workflow (the job-to-be-done)
   - Count clicks/steps to complete the core task
   - Note UX friction: confusing navigation, missing defaults, excessive configuration

3. **Information architecture**
   - Evaluate navigation structure: depth, discoverability, consistency
   - Check for progressive disclosure vs. overwhelming dashboards

4. **Mobile / responsive experience**
   - Note if mobile is native, responsive web, or absent
   - Key functionality available on mobile?

5. **Documentation & support**
   - Quality of docs, tutorials, in-app help
   - Support channels: chat, email, community, phone

## Step 7 — Technical teardown

Infer and document the competitor's technology:

| Method | What it reveals |
|---|---|
| **BuiltWith / Wappalyzer** | Frontend framework, analytics, CDN, hosting provider |
| **Developer docs / API** | API design (REST/GraphQL), auth method, rate limits, SDK languages |
| **Job postings** | Backend language, database, infrastructure (AWS/GCP/Azure), team structure |
| **Open source contributions** | Internal tools, libraries, architectural preferences |
| **Network inspection** | API endpoints, request patterns, WebSocket usage, third-party services |
| **Performance** | Page load time (Lighthouse), bundle size, caching strategy |

Flag any technical choices that create **lock-in** (proprietary formats, closed APIs) or
**scalability concerns** (architectural limitations visible from the outside).

## Step 8 — Business model analysis

Map each competitor against these dimensions:

1. **Revenue model** — SaaS subscription, usage-based, marketplace commission, freemium + upsell, advertising, data licensing
2. **Customer segments** — SMB, mid-market, enterprise, consumer, developer, vertical-specific
3. **Distribution channels** — self-serve, inside sales, field sales, partner/channel, marketplace listing
4. **Key partnerships** — technology partners, integration ecosystem, reseller network
5. **Cost structure** — infrastructure-heavy vs. people-heavy; gross margin signals
6. **Network effects** — does the product get more valuable with more users? (marketplace, collaboration, data)
7. **Switching costs** — what locks customers in? (data, integrations, training, contracts)

## Step 9 — Porter's Five Forces

Apply Porter's framework to the competitive landscape:

| Force | Assessment (High/Medium/Low) | Evidence |
|---|---|---|
| **Threat of new entrants** | | Barriers: capital, technology, regulation, brand, network effects |
| **Bargaining power of buyers** | | Switching costs, concentration, price sensitivity, alternatives available |
| **Bargaining power of suppliers** | | Dependency on platforms (AWS, Apple, Google), talent scarcity, key vendor concentration |
| **Threat of substitutes** | | Alternative approaches to the same job-to-be-done; including "do nothing" |
| **Competitive rivalry** | | Number of competitors, differentiation level, market growth rate, exit barriers |

Conclude with an overall **industry attractiveness assessment**.

## Step 10 — Strategic synthesis

1. Identify the **top 3 competitive advantages** the focal company has (or could build).
2. Identify the **top 3 vulnerabilities** that competitors could exploit.
3. Map **white space opportunities** — underserved segments or unaddressed needs.
4. Recommend **strategic actions** — prioritized by impact and feasibility.
5. Flag what data is missing and how it would change the analysis.

# Decision rules

- **Every SWOT claim must cite evidence** — no unsupported assertions. "They have a great brand" requires proof (traffic data, NPS, press mentions).
- **Feature scores must be justified** — each score > 0 needs a one-line rationale; don't score features you haven't verified.
- **Weight categories before scoring** — unweighted feature counts are misleading; a security feature and a minor UI tweak should not count equally.
- **Include indirect competitors** — the biggest threats often come from adjacent categories; always include at least one indirect competitor.
- **Pricing must include hidden costs** — listed price is the floor; document overage, support, implementation, and migration costs.
- **UX claims require walkthrough evidence** — don't assess UX from screenshots alone; walk through the actual signup and core workflow.
- **Technical claims must cite source** — BuiltWith data, job postings, or API docs; don't guess the stack.
- **Time-bound the analysis** — state the date of research; competitive data decays quickly. Flag anything > 6 months old as potentially stale.
- **State confidence levels** — High (verified firsthand), Medium (inferred from public data), Low (estimated/rumored). Tag each major finding.

# Output structure

Deliver these sections in order:

## 1. Competitor Profiles

Structured profile table for each competitor from Step 2.

## 2. SWOT Matrix

2×2 matrix per competitor with evidence-backed bullet points.

## 3. Feature Comparison Matrix

Weighted scoring table with category weights, per-feature scores, weighted totals,
and highlighted differentiators.

## 4. Pricing Analysis

Comparison table with tier-by-tier breakdown, value metric analysis,
and price-per-feature-point calculation.

## 5. UX Audit

Signup flow step count, time-to-value estimate, friction point inventory,
and core workflow comparison per competitor.

## 6. Technical Landscape

Stack summary per competitor with confidence level, architectural inferences,
and lock-in / scalability flags.

## 7. Business Model Map

Revenue model, segments, distribution, partnerships, and moat analysis per competitor.

## 8. Porter's Five Forces Summary

Force-by-force assessment table with overall industry attractiveness conclusion.

## 9. Strategic Implications

Top advantages, vulnerabilities, white space opportunities, and prioritized action recommendations.

## 10. Confidence and Gaps

Table of data confidence levels per section and list of missing data with suggested sources.

# Anti-patterns

- **Cherry-picking weaknesses** — analyzing only what competitors do poorly while ignoring their strengths produces a dangerously biased picture. The SWOT must be balanced.
- **Ignoring indirect competitors** — Blockbuster didn't lose to another video store. Always map the job-to-be-done and identify non-obvious substitutes.
- **Static point-in-time analysis** — a snapshot without trajectory is misleading. Note growth rates, recent moves, and momentum direction for each competitor.
- **Feature-counting without weighting** — "we have 47 features and they have 32" is meaningless if their 32 include the 5 that matter most to customers. Always weight by customer priority.
- **Pricing comparison without value metric alignment** — comparing a per-seat price to a usage-based price is apples-to-oranges. Normalize to a common usage scenario.
- **Assuming public info is complete** — competitors don't advertise their best features on the pricing page. Test the product, read reviews, talk to customers of competitors.
- **Confusing correlation with causation** — "they grew because of feature X" needs evidence, not just temporal coincidence.
- **Analyzing only current competitors** — the next entrant matters too; assess barriers to entry and who might enter the market.
- **No "so what"** — analysis without strategic implications is an academic exercise. Every section must connect to an actionable recommendation.

# Related skills

- `market-research` — broader market sizing and trend analysis that provides context for competitive positioning
- `business-idea-evaluation` — evaluating your own product/idea viability, informed by competitive intelligence
- `tradeoff-analysis` — making strategic decisions between options using the competitive data
- `research-synthesis` — combining multiple research sources into coherent findings
- `spec-authoring` — translating competitive insights into product specifications
- `gap-analysis` — identifying capability gaps between your product and competitors

# Failure handling

- If no competitor is named, ask the user to identify at least the primary competitor and their own product/position.
- If the competitor is in stealth mode or has minimal public information, state this clearly, use what's available (job postings, LinkedIn, Crunchbase), and tag all findings as Low confidence.
- If the user wants a full teardown but provides no access to the competitor's product, limit the analysis to publicly observable information and recommend a hands-on trial for UX and feature sections.
- If the scope is too broad ("analyze the entire SaaS market"), narrow to 3–5 most relevant competitors and offer to expand in a follow-up.
- If data is stale (>6 months), flag specific sections that need refreshing and recommend re-running those steps with current data.
