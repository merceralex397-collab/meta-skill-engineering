---
name: research-synthesis
description: "Synthesize research from multiple sources into structured summaries — extract key findings, compare methodologies, identify gaps, organize by theme, and produce literature reviews with proper citations. Use when reviewing academic papers, comparing technical approaches, or building evidence-based recommendations from multiple sources. Do not use for single-document summarization or opinion pieces without source evidence."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: research-synthesis
  maturity: draft
  risk: low
  tags: [research, synthesis, literature-review, citations, gap-analysis]
---

# Purpose

Synthesize research from multiple sources into structured, evidence-based summaries. Extract key findings, compare methodologies across studies, identify gaps in the literature, organize insights by theme, and produce literature reviews or technical comparison documents with proper citations and traceable provenance.

# When to use this skill

- Reviewing multiple academic papers, technical reports, or documentation sources to produce a unified summary.
- Comparing competing technical approaches, libraries, or architectures with evidence from benchmarks, case studies, or published evaluations.
- Building evidence-based recommendations where conclusions must trace back to specific sources.
- Conducting a literature review or state-of-the-art survey for a design document or RFC.
- Identifying gaps, contradictions, or open questions across a body of research.

# Do not use this skill

- For summarizing a single document — that is a summarization task, not synthesis.
- For writing opinion pieces or position papers without source evidence.
- For architecture decision records — prefer `adr-rfc-writing`.
- For writing technical specifications — prefer `spec-authoring`.
- For extracting data from tables in documents — prefer `table-extraction`.

# Operating procedure

1. **Define the research question.** State the specific question the synthesis must answer. Scope the inquiry: what is in bounds, what is excluded, and what level of evidence is required (peer-reviewed, grey literature, industry reports).
2. **Collect and catalog sources.** Gather all source documents. Create a source registry table with columns: Source ID, Title, Authors, Year, Type (journal, conference, blog, report), and Relevance Score (1-3). Assign each source a short citation key (e.g., `[Smith2023]`).
3. **Extract findings per source.** For each source, extract: primary claim, methodology, key data points, limitations stated by authors, and limitations you identify. Record these in a structured format (table or per-source summary block).
4. **Identify themes and categories.** Group extracted findings by theme (e.g., performance, scalability, developer experience, cost). Create a theme matrix showing which sources contribute to each theme.
5. **Compare methodologies.** For each theme, compare how different sources approached the question: sample sizes, experimental design, evaluation metrics, and reproducibility. Note where methodologies are incomparable.
6. **Identify agreements and contradictions.** Flag findings where sources agree, partially agree, or directly contradict each other. For contradictions, analyze whether differences stem from methodology, scope, or context.
7. **Perform gap analysis.** List questions that the collected sources do not address. Identify themes with only a single source (weak evidence). Note areas where evidence is outdated (>3 years for fast-moving fields).
8. **Draft the synthesis document.** Structure as: Executive Summary, Methodology (how sources were selected), Thematic Findings (one section per theme), Contradictions and Open Questions, Gap Analysis, Recommendations, and Source Bibliography.
9. **Cite every claim.** Every factual statement must include a citation key linking to the source registry. Use inline citations `[AuthorYear]` and provide a full bibliography at the end.
10. **Review for bias and completeness.** Check that the synthesis does not over-represent a single source. Verify that contradictory evidence is presented fairly. Confirm all sources in the registry appear in the bibliography.

# Decision rules

- Weight peer-reviewed sources higher than blog posts or vendor documentation unless the blog provides unique empirical data.
- When sources contradict, present both positions with their evidence rather than choosing a winner.
- Mark any conclusion drawn from fewer than 3 independent sources as "limited evidence."
- When a finding is based on a single source, explicitly state this and flag it for further validation.
- Organize by theme rather than by source — readers need answers, not a list of papers.
- Prefer quantitative evidence over qualitative when both are available for the same claim.

# Output requirements

1. A structured Markdown document with Executive Summary, Thematic Findings, Gap Analysis, and Recommendations sections.
2. A source registry table listing all sources with citation keys, type, and relevance.
3. Inline citations for every factual claim, traceable to the source registry.
4. A Contradictions section listing conflicting findings with analysis of why they differ.
5. A Gap Analysis section listing unanswered questions and weakly supported themes.

# References

- Cochrane Handbook for systematic review methodology: https://training.cochrane.org/handbook
- PRISMA guidelines for transparent reporting: https://www.prisma-statement.org/
- Zotero reference manager: https://www.zotero.org/
- BibTeX citation format: https://www.bibtex.org/

# Related skills

- `document-writing` — for producing the final narrative document from synthesis findings.
- `adr-rfc-writing` — for converting research conclusions into architectural decision records.
- `spec-authoring` — for translating research into technical specifications.

# Anti-patterns

- Cherry-picking sources that support a predetermined conclusion while ignoring contradictory evidence.
- Presenting a list of paper summaries instead of synthesizing across sources by theme.
- Using vague citations like "studies show" without linking to specific sources.
- Treating all sources as equally authoritative regardless of methodology quality or publication venue.
- Confusing correlation findings with causal claims from the source material.

# Failure handling

- If fewer than 3 sources are available, note this as a limitation and recommend expanding the search before drawing conclusions.
- If sources use incompatible metrics (e.g., latency vs. throughput), normalize where possible and flag where normalization is infeasible.
- If a source is paywalled or inaccessible, record it in the registry as "Not Accessed" and exclude its claims from the synthesis.
- If the research question is too broad, propose 2-3 narrower scoping options and ask for selection before proceeding.
- If contradictions cannot be resolved from available evidence, state the contradiction clearly and recommend specific follow-up research.
