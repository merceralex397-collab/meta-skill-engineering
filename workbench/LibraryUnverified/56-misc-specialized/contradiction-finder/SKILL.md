---
name: contradiction-finder
description: Detects logical contradictions, incompatible expectations, and conflicting statements across specs, tickets, and docs. Trigger — "find contradictions", "check for conflicts", "are these specs consistent", "do these docs agree". Skip when reviewing code quality or doing single-document editing.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: contradiction-finder
  maturity: draft
  risk: low
  tags: [contradiction, finder]
---

# Purpose
Finds statements in a document or set of documents that directly or implicitly conflict with each other, making the plan internally inconsistent or impossible to satisfy simultaneously. This is formal consistency checking applied to planning artifacts: extract claims as propositions, check for logical contradictions, and report which cannot both be true.

# When to use this skill
Use when:
- The user says "find the contradictions", "check for inconsistencies", "does this hang together?", or "are these docs aligned?"
- A spec, ADR, or ticket set has grown large enough that conflicts have likely accumulated
- A plan references multiple earlier documents and alignment needs verification
- Multiple authors have contributed to related documents without coordination
- A codebase and its documentation are being compared for divergence (contrast with `drift-detection` which compares intent vs. implementation)

Do NOT use when:
- The user wants unstated assumptions surfaced (use `assumptions-audit`)
- The user wants to compare code implementation against docs (use `drift-detection`)
- The document is a single short coherent brief—no multi-source comparison is needed
- The user wants to attack the plan adversarially (use `red-team-challenge`)

# Operating procedure
1. **Index all claims**: For each document or section, extract the key factual, numerical, and policy claims as a list. Be precise:
   - "Latency target is 50ms"
   - "Service is stateless"
   - "Auth is handled by the API gateway"
   - "Feature X is out of scope"
   - "Launch date is March 15"

2. **Group by topic**: Cluster claims by subject area:
   - Performance targets
   - Ownership and responsibility
   - Sequencing and dependencies
   - Access model and auth
   - Data model and storage
   - Scope inclusions and exclusions
   - Timeline and milestones

3. **Compare within each cluster**: Look for:
   - **Direct contradictions**: A and not-A stated in different places
   - **Numerical inconsistencies**: 50ms vs 200ms for the same metric
   - **Scope conflicts**: Included in one doc, excluded in another
   - **Ownership conflicts**: Different owners claimed for the same thing

4. **Identify implicit contradictions**: Look for cases where two stated requirements, if both implemented, produce a logical impossibility:
   - "Zero downtime deployment" + "No rolling deployment support"
   - "Real-time sync" + "Batch processing only"
   - "Single source of truth" + "Each service owns its data copy"

5. **Classify each contradiction**:
   - **Direct**: The same claim is stated with different values or opposite polarity in two places
   - **Implicit**: Two requirements are logically incompatible even if no single sentence states it
   - **Temporal**: Earlier and later documents disagree, suggesting outdated content

6. **Cite source locations**: For each contradiction, quote both conflicting statements with their source document/section/line. Precision matters for resolution.

# Output defaults
A numbered list of contradictions, each with:
- **Type**: Direct / Implicit / Temporal
- **Statement A**: Exact quote with source
- **Statement B**: Exact quote with source
- **Resolution Needed**: What decision must be made to resolve this contradiction

End with a **Resolution Priority** section: which contradictions must be resolved before work can proceed (blocking) vs. can be resolved during implementation (non-blocking).

# Named failure modes of this method

- **False contradiction from ambiguity**: Flagging two statements as contradictory when they use the same word with different meanings (e.g., "real-time" meaning <1s in one doc and <1min in another). Fix: resolve terminology before declaring contradiction.
- **Missing temporal context**: Flagging an old decision and a new decision as contradictory when the new one intentionally supersedes the old. Fix: check document dates and change history before classifying as contradiction.
- **Shallow extraction**: Only catching direct word-level contradictions while missing implicit logical incompatibilities. Fix: always run the implicit contradiction check (step 4) even if direct contradictions are found.
- **Over-extraction**: Treating every minor inconsistency (formatting, naming conventions) as a meaningful contradiction. Fix: only flag contradictions that would cause different implementation choices.
- **Single-source blindness**: Reviewing one document thoroughly but skimming related documents. Fix: extract claims from all provided documents before comparing.

# References
- Formal logic: principle of non-contradiction (¬(A ∧ ¬A))
- Requirements engineering: consistency checking methodologies
- IEEE 830 — software requirements specification standards

# Failure handling
If documents are missing or inaccessible:
1. List which documents were reviewed
2. List which documents were referenced but not available
3. Note which topic areas cannot be evaluated without the missing sources
4. Proceed with partial analysis on available documents, marked as incomplete
