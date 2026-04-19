---
name: failure-mode-analysis
description: Enumerates how a system or agent workflow could fail and maps controls to each failure mode using FMEA methodology. Trigger — "what could go wrong", "failure mode analysis", "FMEA", "enumerate failure modes", "risk assessment". Skip for general risk brainstorming without structured severity/likelihood scoring.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: failure-mode-analysis
  maturity: draft
  risk: low
  tags: [failure, mode, analysis]
---

# Purpose
Systematically enumerates how a system, workflow, or component can fail, rates each failure mode by severity and likelihood, and identifies controls that reduce each risk. This adapts FMEA (Failure Mode and Effects Analysis)—a reliability engineering technique from the 1950s—for software systems, agent workflows, and LLM-based applications.

# When to use this skill
Use when:
- The user says "what are the failure modes?", "FMEA this", "how could this break in production?", or "what could go wrong technically?"
- A new system is about to be deployed and failure modes need cataloguing before go-live
- An orchestration system, agent, MCP tool, or LLM pipeline is being designed
- Post-incident: a system failed and all related failure modes need enumeration to prevent recurrence
- Designing monitoring, alerting, or circuit breakers

Do NOT use when:
- The user wants to trace a specific past failure to root cause (use `root-cause-analysis`)
- The user wants project-level risk analysis (use `premortem`)
- The system is conceptual—nothing concrete to analyze
- The user wants to compare options (use `tradeoff-analysis`)

# Operating procedure
1. **Decompose the system into components**: List every functional unit:
   - Services, APIs, workers
   - Databases, caches, queues
   - Agents, tools, MCP servers
   - External dependencies, third-party services
   - Human actors (operators, approvers)

2. **For each component, enumerate failure modes**: Ask "In what ways can this component fail to do its job?"
   
   Standard failure mode categories:
   - **Silent failure**: Appears to work, produces wrong/corrupted output
   - **Crash failure**: Component stops responding, process dies
   - **Slow failure**: Latency degrades until unusable, timeouts cascade
   - **Corrupt failure**: Data or state becomes inconsistent
   - **Cascade failure**: This component's failure causes downstream failures
   - **Security failure**: Component is exploited, produces unauthorized output
   - **Resource exhaustion**: Memory, disk, connections, rate limits exceeded

3. **For each failure mode, assess three dimensions**:
   - **Likelihood**: Frequent (weekly+), Occasional (monthly), Rare (yearly), Very Rare (never seen)
   - **Severity**: Critical (data loss, security breach), High (system down), Medium (degraded), Low (minor impact)
   - **Detectability**: Immediate (alert fires), Minutes (monitoring catches), Hours (user reports), Silent (never detected)

4. **Calculate Risk Priority Number (RPN)**: RPN = Likelihood × Severity × (inverse of Detectability)
   
   Use scale: Likelihood (1-4), Severity (1-4), Detectability (1-4 where 1=immediate, 4=silent)
   
   Focus effort on high-RPN items.

5. **Assign controls to high-priority failure modes**:
   - **Preventive control**: Stops failure from occurring (input validation, rate limiting, circuit breaker, redundancy)
   - **Detective control**: Catches failure quickly (health check, alerting, audit log, anomaly detection)
   - **Corrective control**: Recovers from failure (retry logic, fallback, rollback, graceful degradation)

6. **Identify Single Points of Failure (SPOF)**: Components whose failure has no fallback—entire system goes down. Mark these prominently.

7. **Identify cascade paths**: Which component failures trigger other component failures? Map the domino chains.

# Output defaults
A **Failure Mode Table** with columns: Component | Failure Mode | L | S | D | RPN | Controls

A **SPOF List** of single points of failure with mitigation recommendations.

A **Cascade Paths** section showing failure propagation chains.

A **Top 5 Risk Items** section: highest RPN items with specific control recommendations.

Example row:
| API Gateway | Rate limit exhaustion | 2 | 3 | 2 | 12 | Preventive: per-tenant limits; Detective: rate monitoring alert; Corrective: graceful 429 with retry-after |

# Named failure modes of this method

- **Component granularity mismatch**: Decomposing too coarsely (missing failure modes inside a component) or too finely (drowning in trivial modes). Fix: decompose to the level where each component has a single responsibility and clear failure behavior.
- **Happy-path FMEA**: Only analyzing failure modes during normal operation while missing failures during startup, shutdown, deployment, failover, or scaling events. Fix: enumerate lifecycle phases and check each.
- **Detection optimism**: Rating detectability as "Immediate" because an alert exists, without checking whether the alert actually fires for this failure mode. Fix: verify detection mechanisms against each specific failure mode, not just their existence.
- **Missing cascade analysis**: Rating each failure mode independently without tracing how one failure triggers others. Fix: always map the domino chains (step 7) after individual assessment.
- **Static analysis**: Performing FMEA once and treating it as permanent. Fix: FMEA must be revisited when the system changes—new components, new dependencies, new scale.

# References
- https://en.wikipedia.org/wiki/Failure_mode_and_effects_analysis — FMEA methodology
- IEC 60812 — international standard for FMEA
- NASA FMEA handbook — rigorous application to critical systems
- Google SRE Book — failure mode thinking for software systems (https://sre.google/workbook/postmortem-culture/)

# Failure handling
If component boundaries are unclear:
1. List components that could be identified
2. Note which areas of the system need clarification
3. Perform partial analysis on identifiable components
4. Request system documentation or architecture diagram to complete analysis
