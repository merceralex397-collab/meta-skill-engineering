---
name: tight-routing-example
description: >
  Decomposes a monolithic service into bounded-context microservices by
  analyzing dependency graphs, data ownership, and API call patterns.
---

# Monolith Decomposition Planner

## Purpose

Analyze a monolithic codebase to identify natural service boundaries,
then produce a phased decomposition plan with extracted service specs,
inter-service API contracts, data ownership assignments, and a migration
sequence that avoids circular dependencies.

## When to use

- A monolith has been flagged for decomposition and the team needs a
  concrete plan before writing any extraction code.
- An existing partial decomposition has stalled and needs re-planning
  based on the current dependency state.
- Architecture review has identified a specific bounded context that
  should be extracted next and needs an impact analysis.

## When NOT to use

- For breaking a monolith's database into per-service databases without
  changing service boundaries — use `database-migration-planning` instead.
- For refactoring code within a monolith to improve modularity without
  extracting services — use `code-restructuring` instead.
- For designing greenfield microservices that have no monolith ancestry —
  use `service-design` instead.
- For deploying already-extracted services — use `deployment-orchestrator`
  instead.
- For evaluating whether decomposition is warranted at all — use
  `architecture-review` instead. This skill assumes the decision is made.

## Procedure

1. Build the static dependency graph by analyzing imports, function calls,
   and shared data access across all modules in the monolith. Use the
   project's language-specific tooling:
   - Python: `pydeps` or `import-linter`
   - Java: `jdeps` combined with ArchUnit dependency rules
   - TypeScript: `madge`

2. Overlay runtime call data if available. Pull the last 30 days of
   distributed traces from the observability platform and map them to
   the static graph to weight edges by actual call frequency.

3. Run community detection on the weighted graph (Louvain algorithm with
   resolution parameter 1.0) to propose candidate bounded contexts.
   Each community becomes a candidate service.

4. For each candidate service, compute:
   - Inbound coupling: number of modules outside the boundary that call
     into it.
   - Outbound coupling: number of modules inside the boundary that call
     outside it.
   - Data ownership: tables or collections primarily read/written by
     modules inside the boundary.

5. Resolve ownership conflicts where a table is written by modules in
   multiple candidate services. Apply the "writer owns" heuristic: the
   service with the most write operations owns the table; others access
   it via the owning service's API.

6. Produce the decomposition plan document with:
   - Service inventory: name, purpose, owned tables, API surface.
   - Extraction sequence: topologically sorted so that services with
     zero inbound cross-boundary calls are extracted first.
   - API contracts: OpenAPI stubs for each inter-service call, derived
     from the current internal function signatures.
   - Risk register: high-coupling edges that may cause issues, with
     mitigation strategies.

7. Validate the plan by checking that no extraction step introduces a
   circular dependency between the extracted service and the remaining
   monolith.

## Output contract

- `decomposition-plan.md` — Full plan document following the structure
  in step 6.
- `services/` directory with one `<service-name>/spec.yaml` per proposed
  service, containing API stubs and data ownership declarations.
- `dependency-graph.svg` — Visualized dependency graph with community
  coloring.
- Exit code 0 if the plan is consistent; exit code 2 if unresolvable
  circular dependencies exist.

## Failure handling

- If static analysis tooling is unavailable for the project's language,
  fall back to regex-based import scanning and note reduced accuracy in
  the plan document.
- If runtime trace data is unavailable, proceed with static analysis only
  and flag that edge weights are estimated, not measured.
- If community detection produces a single giant cluster (modularity
  score < 0.3), report that the monolith lacks clear boundaries and
  recommend manual boundary identification before re-running.

## Next steps

- Feed the decomposition plan into `ticket-pack-builder` to create
  extraction work items.
- Use `database-migration-planning` for each service's data extraction.
- Track extraction progress through `skill-lifecycle-management`.
