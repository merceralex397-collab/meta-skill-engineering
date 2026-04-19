---
name: spec-authoring
description: "Author technical specifications with clear requirements, API contracts, system design sections, acceptance criteria, and interface definitions that engineering teams can implement directly. Use when writing design specs, API specifications, system architecture documents, or feature specs with testable acceptance criteria. Do not use for user-facing documentation, meeting notes, or high-level strategy documents."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: spec-authoring
  maturity: draft
  risk: low
  tags: [spec, technical-spec, requirements, api-contracts, acceptance-criteria]
---

# Purpose

Author technical specifications that engineering teams can implement directly. Define requirements, API contracts, system design, data models, interface definitions, constraints, and testable acceptance criteria — producing documents that eliminate ambiguity and serve as the source of truth for implementation.

# When to use this skill

- Writing a design spec for a new feature, service, or system component before implementation begins.
- Defining API contracts (REST, GraphQL, gRPC) with endpoints, request/response schemas, error codes, and rate limits.
- Documenting system architecture with component boundaries, data flows, and integration points.
- Creating feature specs with user stories, requirements, and testable acceptance criteria.
- Writing interface definitions for libraries, SDKs, or internal service contracts.

# Do not use this skill

- For user-facing documentation, help articles, or tutorials — prefer `document-writing`.
- For meeting notes or decision logs — prefer `meeting-notes-decision-log`.
- For high-level strategy or vision documents without implementation detail.
- For operational runbooks — prefer `runbook-writing`.
- For architecture decision records — prefer `adr-rfc-writing` (though specs may reference ADRs).

# Operating procedure

1. **Define the problem statement.** Write 2-4 sentences describing what problem this spec solves, who it affects, and why it matters now. Link to the originating issue, ticket, or RFC.
2. **State goals and non-goals.** List 3-5 goals as specific, measurable outcomes. List non-goals explicitly — things this spec intentionally does not address. Non-goals prevent scope creep during implementation.
3. **Identify stakeholders and reviewers.** List the teams or individuals who must review and approve: engineering, product, security, infrastructure. Assign a spec owner responsible for updates.
4. **Describe the current state.** Document how the system works today (or state "greenfield" if new). Include a diagram or flow description showing the existing architecture relevant to this change.
5. **Design the proposed solution.** Break into subsections: System Architecture (component diagram), Data Model (schemas, relationships, migrations), API Contracts (endpoints, methods, payloads, errors), and Integration Points (external services, event buses, queues).
6. **Define API contracts in detail.** For each endpoint: HTTP method, path, request headers, request body schema (with types and required/optional flags), response body schema, error response codes with descriptions, and rate limits. Use JSON Schema or TypeScript interface notation.
7. **Specify data models.** Define database schemas with column names, types, constraints, indexes, and foreign keys. Document data lifecycle: creation, updates, deletion/archival, and retention policy.
8. **Write acceptance criteria.** For each requirement, write testable acceptance criteria in Given/When/Then format. Each criterion must be independently verifiable — no compound conditions. Include edge cases and error scenarios.
9. **Document constraints and assumptions.** List technical constraints (language, framework, infrastructure), performance requirements (latency P99, throughput), security requirements (auth, encryption, audit), and assumptions that must hold true.
10. **Define milestones and dependencies.** Break the implementation into phases if the spec is large. List external dependencies (other teams, third-party services) and their expected timelines.
11. **Add an open questions section.** List unresolved decisions with the responsible person and deadline for resolution. Do not leave ambiguity buried in prose — surface it here.
12. **Format and review.** Use consistent Markdown structure with numbered sections. Include a revision history table. Circulate for review with a clear deadline and feedback format.

# Decision rules

- Every requirement must be testable — if you cannot write an acceptance criterion for it, rewrite the requirement.
- Use precise language: "must" for requirements, "should" for recommendations, "may" for optional. Follow RFC 2119 keywords.
- Define error behavior explicitly for every API endpoint — do not leave error handling to implementer judgment.
- Include non-functional requirements (performance, security, scalability) alongside functional ones — they are equally binding.
- When two design options exist, present both with trade-offs and make a recommendation — do not defer the decision.
- Limit the spec to one logical unit of work — if it exceeds 20 pages, split into sub-specs with a parent overview.

# Output requirements

1. A Markdown document with sections: Problem Statement, Goals/Non-Goals, Proposed Design (Architecture, Data Model, API Contracts), Acceptance Criteria, Constraints, Open Questions, Revision History.
2. API contracts with request/response schemas, error codes, and examples.
3. Acceptance criteria in Given/When/Then format for every requirement.
4. Diagrams (Mermaid, PlantUML, or ASCII) for architecture and data flow.
5. A revision history table with Date, Author, and Change Summary columns.

# References

- RFC 2119 (requirement level keywords): https://www.rfc-editor.org/rfc/rfc2119
- Google Engineering Practices — Design Docs: https://google.github.io/eng-practices/
- OpenAPI Specification: https://spec.openapis.org/oas/latest.html
- JSON Schema: https://json-schema.org/

# Related skills

- `adr-rfc-writing` — for documenting architectural decisions referenced by specs.
- `research-synthesis` — for evidence gathering that informs spec decisions.
- `document-writing` — for user-facing documentation that follows from specs.

# Anti-patterns

- Writing specs after implementation — the spec becomes retroactive documentation, not a design tool.
- Using vague requirements like "the system should be fast" without quantified targets.
- Defining API endpoints without error response schemas — forces implementers to invent error formats.
- Omitting non-goals — leads to unbounded scope creep during implementation.
- Writing acceptance criteria that test implementation details rather than observable behavior.
- Producing a spec with no open questions section — either the spec is trivial or hard questions are being avoided.

# Failure handling

- If requirements are ambiguous after stakeholder input, write the two most likely interpretations and ask the product owner to choose.
- If an API contract depends on an external service not yet built, document the dependency, define a mock contract, and flag the integration as a risk.
- If acceptance criteria cannot be written for a requirement, the requirement is insufficiently defined — rewrite it or escalate to the product owner.
- If the spec exceeds the target length, extract detailed schemas into appendices and keep the main body focused on design decisions.
- If reviewers do not respond by the review deadline, escalate to the spec owner with a list of unreviewed sections and blocking decisions.
