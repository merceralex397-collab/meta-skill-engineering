---
name: cv-cover-letter
description: >
  Tailors CVs and cover letters to specific job postings using STAR-method bullets,
  ATS-optimized formatting, and keyword-matched content. Trigger phrases: "write my CV",
  "update my resume", "cover letter for", "tailor my CV to", "help me apply for",
  "rewrite my experience section". Do NOT use for general career coaching, interview prep,
  salary negotiation, or LinkedIn profile optimization — those are out of scope.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cv-cover-letter
  maturity: draft
  risk: low
  tags: [cv, resume, cover-letter, ats, job-application, star-method]
---

# Purpose

Produce role-targeted CVs and cover letters that pass ATS screening, highlight quantified achievements using the STAR method, and maintain strict truthfulness about the candidate's experience.

# When to use this skill

- User asks to write, tailor, update, or review a CV or resume for a specific role
- User provides a job posting and wants application materials matched to it
- User wants a cover letter drafted or improved for a specific position
- User asks for keyword optimization or ATS compatibility review of existing CV
- A task requires mapping existing experience to a new role's requirements

# Do not use this skill when

- The task is interview preparation, behavioral question coaching, or mock interviews
- The user wants salary negotiation advice or offer comparison
- The request is about LinkedIn profile optimization or professional networking strategy
- The user needs general career path advice without a specific role target
- The task is better handled by `market-research` (industry analysis) or `business-idea-evaluation` (entrepreneurship)

# Operating procedure

## Phase 1 — Job posting extraction

1. Parse the target job posting to extract: role title, required skills, preferred qualifications, years of experience, key responsibilities, company values/mission keywords.
2. Categorize extracted requirements as **must-have** vs **nice-to-have**.
3. Identify industry-specific terminology and jargon the ATS will likely scan for.

## Phase 2 — Experience inventory and mapping

4. Review the user's existing CV, experience notes, or career history.
5. Map each user experience item to one or more job requirements using a **Requirement → Experience Matrix**.
6. For each mapped item, draft a STAR bullet:
   - **Situation**: Context and scope (team size, company type, scale).
   - **Task**: The specific responsibility or challenge assigned to you.
   - **Action**: What you personally did — concrete verbs, tools, methods.
   - **Result**: Quantified outcome (%, $, time saved, users impacted). If no hard number exists, use qualitative impact with scope.
7. Perform a **gap analysis**: identify job requirements with no matching experience. For each gap, note transferable skills or adjacent experience that partially addresses it.

## Phase 3 — CV construction (ATS-optimized)

8. Use standard ATS-parseable section headers: **Contact Information**, **Professional Summary**, **Experience**, **Education**, **Skills**, **Certifications** (if applicable).
9. Place the most relevant experience first within each section (relevance-ordered, not strictly reverse-chronological, unless the user prefers chronological).
10. Include exact keyword phrases from the job posting in natural context — do not keyword-stuff.
11. Use clean formatting: no tables, no columns, no headers-in-images, no text boxes. Plain text with consistent bullet markers.
12. Keep to 1 page for <10 years experience, 2 pages max for 10+ years, unless the user specifies otherwise.

## Phase 4 — Cover letter construction

13. Structure the cover letter in four parts:
    - **Hook** (1-2 sentences): Specific reference to the company/role that shows genuine interest — mention a product, initiative, or value.
    - **Relevance Bridge** (2-3 sentences): Connect your background to the role's core need. State what you bring, not what you want.
    - **Evidence** (1-2 paragraphs): 2-3 concrete examples from your experience that directly address the top job requirements. Use abbreviated STAR format.
    - **Call to Action** (1-2 sentences): Express enthusiasm, restate fit, invite next step.
14. Keep the cover letter under 400 words. Match the tone to the company culture (formal for finance/law, conversational for startups).

## Phase 5 — Keyword match report

15. Produce a **Keyword Match Report**: list every key term from the job posting, mark whether it appears in the CV (✅) or is missing (❌), and note where it was placed.
16. Target ≥80% keyword match rate for hard skills and ≥60% for soft skills.

# Decision rules

- **Truthfulness is non-negotiable**: Reframe experience to highlight relevance, but never fabricate roles, skills, or metrics. If the user lacks experience in an area, say so in the gap analysis and suggest how to address it (projects, certifications, honest framing).
- **Quantify or qualify**: Every STAR bullet must have a measurable result OR a qualitative impact statement with scope. "Improved performance" is unacceptable; "Reduced API response time by 40% across 3 microservices" is the target.
- **One CV per application**: Never produce a generic "one-size-fits-all" CV. Each output must be tailored to the specific posting.
- **ATS before aesthetics**: Formatting choices must prioritize parseability. If a design element might confuse an ATS parser, remove it.
- **Recency bias**: Weight recent experience (last 3-5 years) more heavily. Older roles get shorter treatment unless they are uniquely relevant.
- **Skill-level honesty**: Distinguish between "proficient" (daily use, 2+ years), "familiar" (project experience), and "exposure" (coursework, self-study). Do not list a skill without qualifying the level if the user's proficiency is ambiguous.

# Output requirements

Deliver all of the following as separate, clearly labeled sections:

1. **Tailored CV** — Full CV text, ATS-formatted, with STAR bullets for each experience entry.
2. **Cover Letter** — Complete letter following the Hook → Relevance Bridge → Evidence → Call to Action structure.
3. **Keyword Match Report** — Table of job posting keywords with match status and placement location.
4. **Gap Analysis** — List of unmatched requirements with recommended mitigation strategies (transferable skills, suggested learning, honest framing approaches).

# Anti-patterns

- **Generic cover letter**: Using the same letter for multiple applications with only the company name swapped. Every cover letter must reference specific company details.
- **Responsibilities over achievements**: Writing "Responsible for managing a team" instead of "Led a 6-person engineering team that shipped 3 features ahead of schedule, reducing customer churn by 12%."
- **Keyword stuffing**: Cramming job posting terms into a skills section without context. ATS systems increasingly penalize this, and human reviewers always do.
- **Fabricating experience**: Inventing roles, inflating titles, or manufacturing metrics. This is an absolute prohibition — redirect to gap analysis and honest framing instead.
- **Wall of text**: Paragraphs instead of bullets in the experience section. Recruiters scan for 6-7 seconds; bullets with bold lead-ins survive that scan.
- **Objective statements**: "Seeking a challenging role where I can grow" adds zero information. Replace with a Professional Summary that states what you offer.
- **Including references on the CV**: Wastes space. "References available upon request" is assumed and unnecessary.

# Related skills

- `competitor-teardown` — Research a target company's products and positioning before writing application materials
- `market-research` — Understand industry trends to contextualize your experience narrative
- `business-idea-evaluation` — If the user is considering entrepreneurship as an alternative to job applications

# Failure handling

- If no job posting is provided, ask for one before proceeding. A CV cannot be tailored without a target.
- If the user's experience is too sparse to fill a full CV, switch to a **functional/skills-based format** and note this in the output.
- If the gap analysis reveals the user meets <40% of must-have requirements, flag this honestly and suggest whether applying is a productive use of time.
- If the user insists on including fabricated experience, refuse and explain the professional and legal risks. Offer to reframe existing experience instead.
