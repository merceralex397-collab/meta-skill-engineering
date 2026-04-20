---
name: domain-scouting
description: >
  Generates, evaluates, and recommends domain names for projects or businesses using
  brandability scoring, availability checking, and trademark risk assessment. Trigger
  phrases: "find a domain for", "domain name ideas", "is this domain available",
  "name my project", "brand name search", "TLD recommendation". Do NOT use for DNS
  configuration, web hosting setup, domain transfer procedures, or SEO strategy — those
  are infrastructure or marketing tasks, not naming tasks.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: domain-scouting
  maturity: draft
  risk: low
  tags: [domain-names, branding, naming, tld, trademark, availability]
---

# Purpose

Generate scored domain name candidates for a project or business, assess availability and trademark risk, and deliver a ranked recommendation with clear justification for the top pick.

# When to use this skill

- User needs domain name ideas for a new project, product, or company
- User wants to evaluate whether a specific domain name is good (brandability, spelling, conflicts)
- User asks for TLD recommendations (.com vs .io vs .dev etc.)
- User needs a trademark conflict pre-screen before purchasing a domain
- A naming brainstorm needs structure: generation → filtering → scoring → recommendation

# Do not use this skill when

- The task is DNS record configuration, nameserver setup, or domain transfer mechanics — that is infrastructure work
- The user needs web hosting recommendations or SSL certificate setup
- The request is about SEO keyword strategy (even if domain-related) — prefer `market-research`
- The user needs a full brand identity (logo, colors, voice) — this skill covers only the name and domain
- The task is app store naming or publishing — prefer `app-publishing`

# Operating procedure

## Phase 1 — Requirements gathering

1. Clarify the naming brief:
   - **Project/product description**: What does it do? Who is the audience?
   - **Naming preferences**: Any syllable count, language, or style preferences? Must-include or must-avoid words?
   - **Budget tier**: Standard registration (~$10-15/yr), premium ($50-500), or aftermarket (negotiable, $500+)?
   - **TLD preferences**: .com required, or open to alternatives?
   - **Geographic scope**: Global audience (avoid language traps) or single-market?

## Phase 2 — Name generation

2. Generate 15-25 candidate names using multiple techniques:
   - **Descriptive**: Directly describes what the product does (e.g., "CodeReview", "QuickShip").
   - **Compound**: Two real words merged (e.g., "MailChimp", "SalesForce").
   - **Portmanteau**: Blended word fragments (e.g., "Pinterest" = pin + interest, "Groupon" = group + coupon).
   - **Abstract/coined**: Invented words with phonetic appeal (e.g., "Zillow", "Hulu"). Ensure pronounceability.
   - **Acronym**: Only if the expanded form is meaningful and the acronym is memorable (e.g., "AWS"). Avoid forced acronyms.
   - **Modifier + noun**: Adjective/verb + core noun (e.g., "FastAPI", "BrightData").
3. For each candidate, verify basic pronounceability: read it aloud. If it requires explanation to pronounce, penalize it.

## Phase 3 — Brandability scoring

4. Score each candidate on a 1-5 scale across these dimensions:

   | Criterion | 1 (Poor) | 5 (Excellent) |
   |-----------|----------|---------------|
   | **Length** | >15 chars | ≤8 chars |
   | **Memorability** | Forgettable, generic | Distinctive, sticky |
   | **Spelling clarity** | Multiple plausible spellings | One obvious spelling |
   | **Pronounceability** | Requires explanation | Instant, unambiguous |
   | **Brandability** | Descriptive/generic | Unique, ownable |
   | **Domain hackability** | No good TLD options | Perfect .com or clever hack |

5. Compute a weighted composite score: Length (15%) + Memorability (25%) + Spelling (20%) + Pronounceability (15%) + Brandability (15%) + Domain hackability (10%).

## Phase 4 — Availability checking

6. For the top 10 scored candidates, check availability:
   - **WHOIS lookup**: Check if the .com (and preferred TLD) is registered.
   - **Registrar search**: Cross-reference with a major registrar (Namecheap, Cloudflare, Google Domains) for pricing.
   - **Aftermarket check**: If registered, check if listed on Sedo, Afternic, Dan.com, or GoDaddy Auctions with a price.
   - **Social handle check**: Note availability of matching handles on GitHub, Twitter/X, and the primary social platform for the target audience.
7. Categorize each domain as: **Available** (standard price), **Premium** (registry premium), **Aftermarket** (listed for sale), or **Taken** (registered, not for sale).

## Phase 5 — Trademark risk assessment

8. For the top 5 available candidates:
   - Search the **USPTO TESS** database for exact and phonetic matches in relevant Nice Classes.
   - Check the **EUIPO TMView** if the product will operate in Europe.
   - Search Google for existing businesses using the name in the same industry.
   - Flag any candidate with an exact trademark match in the same class as **HIGH RISK — do not use**.
   - Flag phonetic or visual similarity matches as **MEDIUM RISK — needs attorney review**.
9. Note: This is a pre-screen, not legal advice. Recommend professional trademark search for the final pick.

## Phase 6 — TLD strategy recommendation

10. Apply TLD selection logic:
    - **.com** is always the default recommendation if available and affordable.
    - **.io** is acceptable for developer tools, APIs, and tech-focused B2B products.
    - **.dev** is acceptable for developer tools and open-source projects (requires HTTPS).
    - **.app** is acceptable for mobile-first products (requires HTTPS).
    - **.co** is acceptable as a .com alternative for startups, but note the Colombia confusion risk.
    - **Country-code TLDs** (.uk, .de, .au) are appropriate only if the business is explicitly single-market.
    - **Novelty TLDs** (.xyz, .ninja, .rocks) should be flagged as risky for credibility unless the brand is explicitly playful.

# Decision rules

- **Never recommend a name with a HIGH RISK trademark conflict**, regardless of how good the brandability score is.
- **.com or walk away**: If the budget allows and the project is a commercial venture targeting a broad audience, strongly prefer .com. Recommend alternatives only with explicit justification.
- **Spelling test**: If you have to spell the domain for someone over the phone, it fails. Penalize double letters (e.g., "pressstart.com"), homophones (e.g., "write/right"), and ambiguous letter combos (e.g., "clearchoice" — is it "clear-choice" or "clerc-hoice"?).
- **Hyphens are almost always wrong**: Hyphens in domain names hurt memorability, look unprofessional, and are confused in speech. Only recommend if the unhyphenated form is truly unusable.
- **Numbers are almost always wrong**: Unless the number is part of an established brand (e.g., "7zip"), avoid them. "4u", "2go" patterns are dated.
- **Renewal cost matters**: Flag domains with first-year promotional pricing that jumps significantly on renewal (common with novelty TLDs).

# Output requirements

Deliver all of the following as separate, clearly labeled sections:

1. **Naming Brief Summary** — Restated requirements and constraints as understood.
2. **Candidate List** — All generated names in a scored table: Name | Composite Score | Length | Memorability | Spelling | Pronounceability | Brandability | Domain Hackability.
3. **Availability Report** — Top 10 candidates with: Domain | TLD | Status (Available/Premium/Aftermarket/Taken) | Estimated Cost | Social Handle Availability.
4. **Trademark Risk Assessment** — Top 5 available candidates with: Name | USPTO Result | EUIPO Result | Web Search Result | Risk Level (Low/Medium/High).
5. **Recommendation** — Top 1-3 picks with rationale. Include the recommended TLD, estimated total first-year cost, and any caveats.

# Anti-patterns

- **Trademark-infringing names**: Suggesting names confusingly similar to established brands (e.g., "Googlr", "NetFlicks"). Always run the trademark check before recommending.
- **Hard-to-spell domains**: Names with ambiguous spelling cost traffic and credibility. "Lyft" succeeded despite this — your client's startup probably won't.
- **Ignoring renewal costs**: Recommending a $0.99 first-year `.xyz` domain without mentioning the $12+/yr renewal that follows.
- **Overvaluing cleverness**: A portmanteau that requires explanation is worse than a straightforward compound word.
- **Assuming .com is dead**: .com still carries trust and recognition advantages for commercial ventures. Don't dismiss it because alternatives are trendier.
- **Generating without constraints**: Producing 50 random names without first understanding the brief. Quantity without relevance wastes everyone's time.
- **Ignoring social handle conflicts**: A domain is less valuable if the matching Twitter/GitHub handle is taken by an active, unrelated account.

# Related skills

- `business-idea-evaluation` — Evaluate the business before naming it; naming should follow concept validation
- `market-research` — Understand the competitive landscape to avoid naming collisions
- `competitor-teardown` — Analyze how competitors named and branded themselves
- `app-publishing` — App store naming has different constraints (character limits, keyword optimization)

# Failure handling

- If the user provides no project description, ask for one before generating names. Naming without context produces noise.
- If all top candidates have trademark conflicts, report this clearly and generate a second round with more abstract/coined names.
- If the user's budget is standard registration only and all good .com options are taken, explicitly present the trade-off: weaker name with .com vs stronger name with alternative TLD.
- If the user insists on a name flagged as HIGH RISK trademark, document the risk clearly and recommend they consult a trademark attorney before proceeding.
