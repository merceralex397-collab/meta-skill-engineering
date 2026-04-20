---
name: architecture-review
description: Evaluates system boundaries, dependencies, and component responsibilities using C4-model structural review. Trigger — "review this architecture", "critique the system design", "check component boundaries", "does this design make sense". Skip when the review is about code quality, security-specific analysis, or implementation details.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: architecture-review
  maturity: draft
  risk: low
  tags: [architecture, review]
---

# Purpose
Evaluates a system architecture for structural problems: wrong boundaries, missing seams, circular dependencies, misplaced responsibilities, coupling/cohesion issues, and operational blind spots. Uses the C4 model's hierarchical lens (System → Container → Component → Code) to systematically check that the architecture makes sense at each level of abstraction.

# When to use this skill
Use when:
- The user says "review this architecture", "does this design make sense?", "critique the system design", or "check the boundaries"
- A new system is being designed and needs structural review before build
- An existing system is being refactored and the proposed new shape needs evaluation
- An architecture doc, diagram, or C4 model is present and needs critical assessment
- The team is debating component boundaries and needs structured analysis

Do NOT use when:
- The review is about code quality or implementation details (use a code-review skill)
- The user wants security-specific analysis (use `security-review`)
- The user wants failure mode enumeration (use `failure-mode-analysis`)
- No architecture document or description exists—gather it first

# Operating procedure
1. **Map the components at each C4 level**:
   - **System context**: What external systems and users interact with this system?
   - **Containers**: What deployable units exist (services, databases, queues, apps)?
   - **Components**: Within key containers, what logical components exist?
   
   If a diagram exists, enumerate its nodes. If not, extract from description.

2. **Identify responsibilities per component**: State in one sentence what each component is responsible for. Flag problems:
   - Vague responsibility ("handles user stuff")
   - Compound responsibility with "and" ("handles auth AND billing AND notifications")
   - Circular definition ("service A calls service B which calls service A")

3. **Check boundary integrity**: For each boundary between components:
   - Does data crossing this boundary require transformation?
   - Is the direction of dependency correct? (Higher-level depends on lower-level abstractions, not vice versa)
   - Are there bidirectional dependencies that should be unidirectional?
   - Is the boundary stable or likely to move?

4. **Find missing seams**: Look for:
   - Logic that sits in the wrong component
   - Shared state with no clear owner
   - Implicit dependencies not shown in the diagram
   - God components that do too much
   - Missing components (conceptual work with no home)

5. **Evaluate coupling and cohesion**:
   - **Cohesion**: Are things that change together grouped together?
   - **Coupling**: Are unrelated concerns mixed in a single component?
   - **Connascence**: Are there hidden dependencies beyond explicit APIs?

6. **Check operational concerns**:
   - Can components be deployed independently?
   - Can components scale independently?
   - Are there observable failure modes (health checks, metrics)?
   - Are there single points of failure (SPOF)?
   - Is there a clear data flow for debugging/tracing?

7. **Rate overall health**: Assign one of:
   - **Solid**: No critical issues, minor improvements possible
   - **Has Issues**: Significant problems that should be addressed
   - **Needs Redesign**: Fundamental structural problems that will cause ongoing pain

   Justify the rating in one paragraph.

8. **Prioritize findings**:
   - **Critical**: Redesign needed to proceed safely
   - **Major**: Significant rework should happen before or during implementation
   - **Minor**: Improvements worth making when convenient

# Output defaults
A **Component Inventory** table with: Name | Level (System/Container/Component) | Responsibility | Issues

A findings list organized by severity.

An **Overall Health** rating with justification paragraph.

A **Top 3 Structural Changes** section with specific recommendations.

# Named failure modes of this method

- **Diagram worship**: Reviewing the diagram instead of the actual system. Fix: validate that the diagram matches reality before critiquing it.
- **Level confusion**: Mixing system-context concerns with component-level details in the same critique. Fix: stay at one C4 level per pass, then drill down.
- **Coupling blindness**: Only checking explicit dependencies while missing runtime coupling through shared databases, message formats, or configuration. Fix: ask "what breaks if I change this component?" for each component.
- **Greenfield bias**: Reviewing as if the system is being built from scratch when it's a retrofit with existing constraints. Fix: identify what cannot change before recommending what should.
- **Armchair architecture**: Proposing ideal-state redesigns without acknowledging migration cost. Fix: every "needs redesign" finding must include a migration path or acknowledge its cost.

# References
- https://c4model.com/ — C4 model for visualizing software architecture (Simon Brown)
- https://arc42.org/ — arc42 architecture documentation template
- Brown, S. (2018). Software Architecture for Developers — C4 in depth
- https://martinfowler.com/articles/microservices.html — microservices patterns and anti-patterns
- Parnas, D. (1972). "On the Criteria To Be Used in Decomposing Systems into Modules"

# Failure handling
If the architecture description is incomplete:
1. List what components/boundaries are present
2. List what is missing (no data flows, no boundary definitions, unclear responsibilities)
3. State which review categories cannot be completed
4. Proceed with partial analysis on available information
