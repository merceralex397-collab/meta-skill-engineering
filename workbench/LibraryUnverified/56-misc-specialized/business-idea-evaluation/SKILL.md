---
name: business-idea-evaluation
description: >
  Evaluates business ideas using Lean Canvas, unit economics, moat analysis, and
  structured risk assessment to produce a Go/No-Go recommendation. Trigger phrases:
  "evaluate this business idea", "is this idea viable", "should I build this", "business
  model analysis", "startup idea feedback", "lean canvas for". Do NOT use for existing
  business operations, financial accounting, marketing execution, or competitive
  intelligence on established companies — prefer `competitor-teardown` or
  `market-research` for those.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: business-idea-evaluation
  maturity: draft
  risk: low
  tags: [lean-canvas, unit-economics, startup, business-model, moat, risk-assessment]
---

# Purpose

Systematically evaluate a business idea's viability using the Lean Canvas framework, unit economics estimation, competitive moat analysis, and multi-dimensional risk scoring to produce an evidence-based Go/No-Go recommendation with clear next steps.

# When to use this skill

- User presents a business idea and wants structured feedback on viability
- User asks whether they should build or invest in a product/service concept
- User needs a Lean Canvas, business model analysis, or unit economics estimate
- User wants to compare two or more business ideas against each other
- A project is at the "should we build this?" stage and needs a decision framework

# Do not use this skill when

- The business already exists and the question is about optimizing operations or growth — that is execution, not evaluation
- The user needs competitive intelligence on specific companies — prefer `competitor-teardown`
- The request is about market sizing or industry trend analysis without a specific idea — prefer `market-research`
- The user needs financial modeling, bookkeeping, or tax analysis — prefer `financial-tracker-ops`
- The user wants to write a full business plan document (this skill produces an evaluation, not a plan)

# Operating procedure

## Phase 1 — Idea intake and clarification

1. Extract or ask for the core elements of the idea:
   - **What is the product/service?** One-sentence description.
   - **Who is the customer?** Specific segment, not "everyone".
   - **What problem does it solve?** Existing alternatives the customer currently uses.
   - **How does it make money?** Revenue model (subscription, transaction, advertising, licensing, etc.).
   - **What stage is this at?** Napkin idea, validated problem, prototype, or early revenue.
2. If the user cannot articulate the customer or problem, flag this as a critical gap before proceeding.

## Phase 2 — Lean Canvas construction

3. Build a complete **Lean Canvas** (Ash Maurya's adaptation of Business Model Canvas):

   | Block | Question to answer |
   |-------|--------------------|
   | **Problem** | Top 3 problems for the target customer. List existing alternatives. |
   | **Customer Segments** | Who specifically has this problem? Early adopters vs mainstream. |
   | **Unique Value Proposition** | Single clear sentence: why is this different AND worth paying for? |
   | **Solution** | Top 3 features that address the top 3 problems. |
   | **Channels** | How will you reach customers? (Content, paid ads, partnerships, sales, virality) |
   | **Revenue Streams** | Pricing model, price point, who pays. |
   | **Cost Structure** | Fixed costs, variable costs, key cost drivers. |
   | **Key Metrics** | The 3-5 numbers that determine if this business is healthy. |
   | **Unfair Advantage** | What cannot be easily copied or bought? (Be honest — most ideas have none yet.) |

4. Flag any canvas block where the answer is vague, assumed, or "TBD" — these are **validation priorities**.

## Phase 3 — Unit economics estimation

5. Estimate the following metrics (use ranges if exact numbers are unavailable):
   - **Customer Acquisition Cost (CAC)**: Estimated marketing + sales cost to acquire one paying customer. Break down by channel if possible.
   - **Lifetime Value (LTV)**: Average revenue per customer × gross margin × average customer lifespan. For subscription: (ARPU × gross margin) / monthly churn rate.
   - **LTV:CAC ratio**: Target is ≥3:1 for a sustainable business. Flag if <2:1.
   - **Payback period**: Months to recover CAC from a single customer. Target <12 months for bootstrapped, <18 months for funded.
   - **Gross margin**: Revenue minus direct costs of delivering the product/service. Software should target >70%; services typically 40-60%.
6. State all assumptions explicitly. Mark each estimate with a confidence level: **High** (based on data or close comparables), **Medium** (reasonable inference), or **Low** (educated guess).

## Phase 4 — Moat analysis

7. Assess each potential moat type and rate as **None / Weak / Moderate / Strong**:
   - **Network effects**: Does the product become more valuable as more people use it? (Direct: social networks. Indirect: marketplaces.)
   - **Switching costs**: How painful is it for a customer to leave? (Data lock-in, workflow integration, retraining cost.)
   - **Scale economies**: Does unit cost decrease meaningfully with volume? (Server costs, content amortization, supplier leverage.)
   - **Brand**: Is there a trust or recognition advantage that takes years to build? (Typically irrelevant for new ideas.)
   - **Regulatory/legal barriers**: Licenses, patents, regulatory approvals that block competitors. (Don't confuse regulation with moat — regulation can also block you.)
   - **Proprietary technology/data**: Unique algorithms, datasets, or IP that competitors cannot replicate easily.
8. If no moat exists today, assess whether one can be **built over time** and what it would take.

## Phase 5 — Risk matrix

9. Score each risk dimension 1-5 (1 = low risk, 5 = critical risk):

   | Risk type | What it means | Key questions |
   |-----------|--------------|---------------|
   | **Market risk** | Will anyone pay for this? | Is the problem validated? Is willingness to pay confirmed? |
   | **Execution risk** | Can the team build and deliver it? | Does the team have the skills? What are the hard technical challenges? |
   | **Technology risk** | Can it be built at all? | Are there unsolved technical problems? Dependency on immature tech? |
   | **Regulatory risk** | Could regulations block or constrain this? | Industry-specific compliance, data privacy, licensing requirements? |
   | **Competitive risk** | Can incumbents or well-funded startups crush this? | How fast can a competitor replicate this? Are there funded players? |
   | **Financial risk** | Can this be funded to sustainability? | Runway requirements, capital intensity, path to profitability? |

10. For any dimension scored ≥4, write a specific mitigation strategy or acknowledge it as an unresolved blocker.

## Phase 6 — Comparable analysis

11. Identify 3-5 comparable companies or products:
    - **Direct comparables**: Companies that tried to solve the same problem for the same customer.
    - **Adjacent comparables**: Companies in adjacent markets or solving analogous problems for different customers.
    - **Cautionary comparables**: Companies that tried something similar and failed — analyze why.
12. For each comparable, note: outcome (success/failure/pivot/acquired), business model, funding raised, key differentiator from the idea being evaluated.

## Phase 7 — Go/No-Go scoring

13. Score the idea across 8 weighted dimensions:

    | Dimension | Weight | Score (1-5) |
    |-----------|--------|-------------|
    | Problem severity & frequency | 20% | |
    | Market size & accessibility | 15% | |
    | Unit economics viability | 20% | |
    | Moat potential | 10% | |
    | Execution feasibility | 15% | |
    | Competitive landscape | 10% | |
    | Risk profile | 5% | |
    | Founder-market fit | 5% | |

14. Compute weighted score. Interpretation:
    - **4.0-5.0**: Strong Go — pursue aggressively, focus on execution.
    - **3.0-3.9**: Conditional Go — viable but has specific risks to mitigate first.
    - **2.0-2.9**: Weak — significant concerns. Recommend pivoting or shelving unless specific conditions change.
    - **1.0-1.9**: No-Go — fundamental viability problems.
15. A single dimension scored at 1 is an automatic flag regardless of composite score.

# Decision rules

- **Problem before solution**: If the user leads with a solution and cannot articulate the problem, force problem articulation first. Solution-first ideas fail at higher rates.
- **No assumed virality**: If the growth model depends on virality, require evidence (comparable viral coefficients, inherent shareability mechanics). "People will share it because it's cool" is not a growth strategy.
- **Customer validation beats logic**: A logically sound idea with zero customer evidence scores lower than a messy idea with paying customers or strong intent signals.
- **Honest moat assessment**: Most early-stage ideas have no moat. Saying "no moat yet, but here's how to build one" is more useful than inventing a moat that doesn't exist.
- **Revenue model required**: If the user says "we'll figure out monetization later," flag this as a critical gap. Evaluate based on the most likely revenue model, but note the uncertainty.
- **Beware of TAM fantasies**: "If we capture just 1% of a $50B market" is not a market sizing strategy. Demand bottom-up sizing: specific customers × realistic price × achievable penetration.

# Output requirements

Deliver all of the following as separate, clearly labeled sections:

1. **Idea Summary** — Restated idea in one paragraph, confirming understanding.
2. **Lean Canvas** — Complete 9-block canvas with flagged gaps.
3. **Unit Economics Estimate** — CAC, LTV, LTV:CAC, payback period, gross margin with assumptions and confidence levels.
4. **Moat Assessment** — Rating for each moat type with justification.
5. **Risk Matrix** — Scored risk table with mitigation strategies for high-risk dimensions.
6. **Comparable Analysis** — 3-5 comparables with outcomes and lessons.
7. **Go/No-Go Score** — Weighted score table, composite score, interpretation, and automatic flags.
8. **Next Steps** — 3-5 specific, actionable next steps ranked by impact and urgency. If Go: what to validate first. If No-Go: what would need to change.

# Anti-patterns

- **Solution-first thinking**: Building a product then searching for a problem. The evaluation must start with the problem and customer, not the technology or feature set.
- **Ignoring unit economics**: "We'll make it up in volume" is a red flag, not a strategy. If the unit economics don't work at small scale, they rarely work at large scale either.
- **Assuming virality**: Baking viral growth into the base case without evidence. Viral coefficients >1.0 are extraordinarily rare. Treat virality as upside, not baseline.
- **No customer validation**: An idea that has never been discussed with a real potential customer is an untested hypothesis, not a business. Score it accordingly.
- **TAM-down market sizing**: Starting with a massive market and assuming a tiny slice. Bottom-up sizing (specific reachable customers × realistic conversion) is the only credible approach.
- **Ignoring competitor dynamics**: "No one is doing this" usually means either the market doesn't exist or you haven't looked hard enough. Investigate existing alternatives thoroughly.
- **Conflating idea quality with execution ability**: A great idea executed poorly fails. Assess the team's ability to execute, not just the idea's theoretical merit.
- **Single-scenario planning**: Presenting only the optimistic case. Always include a realistic case and a pessimistic case for key metrics.

# Related skills

- `competitor-teardown` — Deep-dive competitive analysis for companies identified in the comparable analysis
- `market-research` — Broader market sizing and trend analysis to contextualize the opportunity
- `domain-scouting` — If the idea passes evaluation, find a name and domain for it
- `financial-tracker-ops` — Ongoing financial tracking once the idea moves to execution

# Failure handling

- If the user provides only a one-line idea with no context, ask clarifying questions (customer, problem, revenue model) before attempting evaluation. Garbage in, garbage out.
- If unit economics cannot be estimated even roughly (no pricing model, no comparable data), mark the Unit Economics section as "Not estimable — insufficient data" and explain what information is needed.
- If the idea is in a regulated industry the agent has no knowledge of, flag regulatory risk as "Unassessed — specialist input required" rather than guessing.
- If the user asks to evaluate more than 3 ideas at once, recommend evaluating them sequentially with full rigor rather than doing shallow passes on all of them.
