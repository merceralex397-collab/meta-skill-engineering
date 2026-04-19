---
name: subagent-research-patterns
description: >
  Trigger — "delegate research to a sub-agent", "have an agent look this up",
  "split the research across agents", "gather evidence from multiple sources",
  "research question decomposition".
  Skip — task is a single lookup that takes one tool call; task mutates repo
  state (use a code-change skill instead); research scope is undefined and
  the user has not provided a concrete question.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: subagent-research-patterns
  maturity: draft
  risk: low
  tags: [subagent, research, delegation, evidence, synthesis]
---

# Purpose

Provides a repeatable procedure for decomposing a research question into
narrow sub-questions, delegating each to a read-only sub-agent, validating
the returned evidence, and synthesizing a consolidated answer — all without
any sub-agent mutating repository state.

# When to use

- A question requires evidence from 2+ distinct sources or domains.
- The lead agent's context window would overflow if it did all research inline.
- The user explicitly asks for delegated or parallel research.
- A planning phase needs facts gathered before decisions can be made.

# Do NOT use when

- The answer is retrievable with a single grep, glob, or web-fetch call.
- The task requires writing code, creating files, or mutating repo state.
- Research scope is undefined — ask the user to clarify before delegating.
- A more specific skill (e.g., `sc2-domain-expert`) already covers the domain.

# Operating procedure

1. **Extract the root question.** Copy the user's request verbatim into a
   `root_question` field. If it contains multiple questions, list each one.
2. **Decompose into sub-questions.** For each root question, write 1–5
   sub-questions that are independently answerable. Record each in a numbered
   list with an explicit scope boundary (files, APIs, docs to search).
3. **Define the evidence contract per sub-agent.** For every sub-question,
   specify: (a) allowed tool calls (grep, glob, view, web_search, web_fetch),
   (b) maximum sources to consult (default 3), (c) required output shape
   (quote + citation + confidence tag: high / medium / low).
4. **Launch sub-agents in parallel.** Use the `task` tool with `agent_type:
   explore` for each sub-question. Include the evidence contract verbatim in
   the prompt.
5. **Collect and validate returns.** For each sub-agent response, check:
   (a) at least one direct quote or data point is present, (b) every claim
   has a file path, URL, or commit SHA citation, (c) confidence tag is
   included. Reject any response missing these and re-prompt once.
6. **Cross-reference overlapping findings.** If two sub-agents return
   conflicting data, list both claims side-by-side with citations and flag
   the conflict explicitly — do not silently pick one.
7. **Synthesize the consolidated answer.** Merge validated findings into a
   single structured response: Summary → Evidence table (claim | source |
   confidence) → Open questions.
8. **Record the research trace.** Append a `## Research Log` section listing
   each sub-question, the sub-agent that handled it, and the verdict.

# Decision rules

- Never allow a sub-agent to run `bash`, `edit`, or `create` — research is
  read-only.
- If a sub-agent returns only a status phrase ("I found it", "looks good")
  with no evidence, mark the sub-question as FAILED and re-delegate once.
- If re-delegation also fails, escalate the sub-question to the lead agent
  with an explicit "evidence gap" note.
- Cap total sub-agents at 6 per research round to avoid context explosion.
- Prefer primary sources (code, docs, API responses) over secondary
  commentary (blog posts, Stack Overflow) unless no primary source exists.

# Output requirements

1. **Research Brief** — a markdown section with: Summary, Evidence Table
   (claim | source | confidence), Conflicts, Open Questions.
2. **Sub-agent Trace** — a collapsed details block listing each sub-question,
   its delegate, and pass/fail status.
3. **Confidence Rating** — an overall HIGH / MEDIUM / LOW tag on the brief.

# References

- `references/delegate-contracts.md` — template for sub-agent contracts.
- `references/failure-escalation.md` — when and how to escalate gaps.

# Related skills

- `swarm-patterns` — when research scales to many parallel agents.
- `verification-before-advance` — gating next steps on research quality.
- `workflow-state-memory` — persisting research results across sessions.
- `session-resume-rehydration` — resuming interrupted research.

# Failure handling

- **Sub-agent timeout:** If a sub-agent does not return within 120 seconds,
  cancel it and note the sub-question as TIMED_OUT in the trace.
- **All sub-agents fail:** Attempt the research inline with the lead agent.
  If the lead agent also cannot answer, return a frank "unable to determine"
  with the list of attempted sources.
- **Conflicting evidence with no resolution:** Present both sides to the
  user with citations and ask for a tiebreaker rather than guessing.
- **Scope creep:** If a sub-agent's findings suggest the root question needs
  reframing, pause synthesis and propose a revised decomposition to the user.
