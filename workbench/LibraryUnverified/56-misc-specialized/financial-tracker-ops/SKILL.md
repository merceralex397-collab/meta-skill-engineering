---
name: financial-tracker-ops
description: >-
  Maintains ledgers, cashflow projections, debt schedules, budgets, and
  reconciliation artifacts using double-entry bookkeeping principles. Trigger
  phrases: "cashflow forecast", "budget vs actual", "debt payoff", "ledger
  entry", "amortization schedule", "reconciliation", "13-week forecast",
  "variance analysis", "balance sheet", "income statement", "accounts
  receivable". Do NOT use for: market sizing or industry research (use
  market-research), building pivot tables or data transforms on raw
  spreadsheets (use spreadsheet-analysis), or investment portfolio analysis
  beyond basic tracking.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: financial-tracker-ops
  maturity: draft
  risk: low
  tags: [finance, bookkeeping, cashflow, budgeting, debt-tracking]
---

# Purpose

Create and maintain financial tracking artifacts — ledgers, cashflow forecasts, debt schedules, budgets, and reconciliation reports — using double-entry bookkeeping principles. Every financial artifact must balance, state its assumptions, and maintain an audit trail.

# When to use this skill

- The user asks to record transactions, maintain a ledger, or create journal entries.
- A task requires cashflow projection or a 13-week rolling forecast.
- The user needs debt tracking: amortization schedules, payoff comparisons (avalanche vs. snowball), or loan analysis.
- Budget-vs-actual variance analysis is requested.
- A repo contains financial tracking files (e.g., `finances/ledger.csv`, `budget/2024-Q3.md`) that need updating or auditing.
- Account reconciliation is needed — matching ledger entries against bank statements or external records.

# Do not use this skill when

- The task is market sizing, competitive analysis, or industry research — use `market-research`.
- The user wants to build pivot tables, clean CSV data, or perform spreadsheet transformations — use `spreadsheet-analysis`.
- The request is about investment portfolio optimization or securities analysis beyond basic position tracking.
- The user needs tax preparation or tax-code interpretation — flag as out-of-scope and recommend a qualified professional.
- The task is about building a financial software application — use coding-focused skills.

# Operating procedure

## Step 1 — Identify the financial artifact type

Determine which artifact the task requires:

| Artifact | When to use | Key principle |
|----------|------------|---------------|
| Ledger entry | Recording a transaction | Double-entry: debits = credits |
| Cashflow forecast | Projecting future cash position | 13-week rolling with weekly granularity |
| Debt schedule | Tracking loan repayment | Amortization with interest/principal split |
| Budget vs. actual | Comparing planned to real spending | Variance analysis with materiality threshold |
| Reconciliation | Matching two record sources | Every discrepancy must be classified |

## Step 2 — Apply double-entry bookkeeping principles

Every transaction must follow the fundamental accounting equation:

**Assets = Liabilities + Equity**

For every ledger entry:

1. Identify the **accounts affected** (at minimum two).
2. Determine which accounts are **debited** and which are **credited**.
3. Verify that **total debits = total credits** for the entry.
4. Classify each account: Asset, Liability, Equity, Revenue, or Expense.
5. Record date, description, reference number, and amounts.

Standard entry format:

```
Date: YYYY-MM-DD
Reference: [invoice/receipt/memo number]
Description: [what happened and why]
  Debit:  [Account Name]     $X,XXX.XX
  Credit: [Account Name]     $X,XXX.XX
```

If debits ≠ credits, the entry is **rejected** — do not proceed until balanced.

## Step 3 — Cashflow projection (13-week rolling forecast)

Build the forecast with weekly granularity:

1. **Opening balance:** Start with confirmed bank balance as of the most recent reconciliation date.
2. **Cash inflows:** List expected receipts by week — categorize as Confirmed (contract/invoice), Probable (>75% likelihood), or Possible (<75%). Only Confirmed and Probable enter the base case.
3. **Cash outflows:** List committed payments (payroll, rent, loan payments, subscriptions) and discretionary spending by week.
4. **Net cash flow:** Inflows − Outflows per week.
5. **Closing balance:** Opening + Net cash flow = Closing. This week's closing = next week's opening.
6. **Runway calculation:** At current burn rate, how many weeks until cash reaches zero or minimum threshold?

Update the forecast weekly: move actuals into the historical column, extend the projection window by one week, and adjust estimates based on new information.

## Step 4 — Debt tracking and payoff strategy

For each debt instrument, build an **amortization schedule**:

| Payment # | Date | Payment | Principal | Interest | Remaining balance |
|-----------|------|---------|-----------|----------|-------------------|

Compare payoff strategies when multiple debts exist:

- **Avalanche method:** Pay minimums on all debts; direct extra payments to the **highest-interest-rate** debt first. Minimizes total interest paid.
- **Snowball method:** Pay minimums on all debts; direct extra payments to the **smallest-balance** debt first. Maximizes psychological momentum via quick wins.

Present both strategies with: total interest paid, total time to payoff, and monthly payment requirements. Recommend avalanche unless the user has explicitly stated a preference for snowball's behavioral benefits.

## Step 5 — Budget vs. actual variance analysis

For each budget line item:

1. Record **budgeted amount** and **actual amount**.
2. Calculate **variance** (Actual − Budget) and **variance %** ((Actual − Budget) / Budget × 100).
3. Classify each variance:
   - **Favorable:** Actual spending below budget (expenses) or actual revenue above budget (income).
   - **Unfavorable:** The reverse.
4. Apply **materiality threshold:** Only investigate variances exceeding 5% AND $500 (or user-specified thresholds). Immaterial variances are noted but not analyzed.
5. For material variances, provide a **root cause** (price variance, volume variance, timing variance, or one-time event) and a **corrective action** if unfavorable.

## Step 6 — Reconciliation

Match ledger entries against an external source (bank statement, invoice register, etc.):

1. **Match confirmed items:** Entries that appear in both records with matching dates and amounts.
2. **Classify discrepancies:**
   - **Timing differences:** Transaction recorded in one period in the ledger, another in the bank (e.g., outstanding checks). Expected to clear; no action needed beyond tracking.
   - **Recording errors:** Wrong amount, wrong account, duplicate entry. Requires correcting journal entry.
   - **Unrecorded transactions:** Bank fees, interest, or automatic payments not yet in the ledger. Requires new journal entries.
   - **Unexplained differences:** Cannot be classified. Escalate for investigation.
3. **Reconciliation status:** RECONCILED (all items matched or explained) or UNRECONCILED (unexplained differences remain; list them).

# Decision rules

- **Debits must equal credits.** No exceptions. Reject any entry that doesn't balance.
- **Never mix personal and business transactions** in the same ledger or account. If encountered, flag immediately and recommend separation.
- **Forecasts must state assumptions.** Every cashflow projection number must trace to either a confirmed commitment or a stated assumption with a probability estimate.
- **Use the most conservative reasonable estimate** for cashflow forecasts: underestimate inflows, overestimate outflows. Optimistic forecasts that prove wrong cause more damage than conservative ones.
- **Materiality thresholds are mandatory** for variance analysis. Investigating every $5 variance wastes effort; define thresholds upfront.
- **Reconciliation must happen before reporting.** Never produce financial summaries from unreconciled ledgers. State reconciliation status prominently.
- **Debt payoff recommendations default to avalanche** unless the user explicitly requests snowball or states behavioral preference.

# Output structure

Every financial deliverable must use the appropriate format:

## Ledger Entry

```
## Journal Entry: [Reference #]
- Date: YYYY-MM-DD
- Description: [transaction description]
- Accounts:
  | Account          | Debit      | Credit     |
  |------------------|------------|------------|
  | [Account Name]   | $X,XXX.XX |            |
  | [Account Name]   |            | $X,XXX.XX |
- Balance check: Debits $X,XXX.XX = Credits $X,XXX.XX ✓
- Notes: [any additional context]
```

## Cashflow Forecast

```
## 13-Week Cashflow Forecast (as of YYYY-MM-DD)
- Opening balance: $XX,XXX
- Assumptions: [list key assumptions]
- Forecast:
  | Week | Inflows | Outflows | Net | Closing Balance |
  |------|---------|----------|-----|-----------------|
- Runway: XX weeks at current burn rate
- Risk scenarios: [best case / base case / worst case closing balance at week 13]
```

## Variance Report

```
## Budget vs. Actual: [Period]
- Materiality threshold: X% and $XXX
- Summary:
  | Category | Budget | Actual | Variance | Var % | Status |
  |----------|--------|--------|----------|-------|--------|
- Material variances requiring investigation:
  - [Category]: Root cause, corrective action
- Overall assessment: ON TRACK / CAUTION / OFF TRACK
```

## Reconciliation Status

```
## Reconciliation: [Account] as of YYYY-MM-DD
- Ledger balance: $XX,XXX.XX
- External balance: $XX,XXX.XX
- Difference: $XX.XX
- Classified items:
  - Timing differences: $XX.XX (X items)
  - Recording errors: $XX.XX (X items — correcting entries attached)
  - Unrecorded transactions: $XX.XX (X items — new entries attached)
  - Unexplained: $XX.XX (X items — ESCALATE)
- Status: RECONCILED / UNRECONCILED
```

# Anti-patterns

- **Mixing personal and business finances:** Commingling funds in a single ledger or account makes reconciliation unreliable and creates legal/tax exposure. Always separate.
- **Unreconciled balances treated as accurate:** Reporting from a ledger that hasn't been reconciled against bank statements. The numbers may be wrong. Reconcile first.
- **Forecasts without stated assumptions:** A cashflow projection that says "we'll receive $50K in Week 3" without stating whether that's confirmed revenue or an estimate. Every figure needs a basis.
- **Single-entry bookkeeping:** Recording only one side of a transaction (e.g., "paid $500 for supplies" without crediting the cash account). This breaks the accounting equation and makes error detection impossible.
- **Ignoring materiality:** Spending hours investigating a $12 variance while a $5,000 discrepancy sits unexamined. Set thresholds and enforce them.
- **Snowball-by-default:** Recommending the snowball method without disclosing that it costs more in total interest. Always present both options with total-cost comparison.

# Related skills

- `spreadsheet-analysis` — Transforming raw financial CSV/Excel data into analysis-ready formats before ledger entry.
- `market-research` — Providing revenue assumptions and market context that feed into cashflow forecasts.
- `business-idea-evaluation` — Using financial projections as input to go/no-go business decisions.
- `competitor-teardown` — Analyzing competitor pricing and margin structures to inform budget assumptions.

# Failure handling

- If the ledger doesn't balance, stop all downstream analysis (forecasts, variance reports) until the imbalance is resolved. Report the discrepancy amount and affected accounts.
- If reconciliation reveals unexplained differences exceeding the materiality threshold, escalate before producing any summary reports. Do not smooth over the gap.
- If the user provides incomplete transaction data (e.g., amount but no accounts), ask for the missing information rather than guessing. Incorrect account classification corrupts all downstream reporting.
- If the task requires tax advice, regulatory interpretation, or investment recommendations, state that this is outside the skill's scope and recommend consulting a qualified professional.
