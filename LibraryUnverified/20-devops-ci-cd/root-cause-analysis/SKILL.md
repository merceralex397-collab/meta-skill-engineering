---
name: root-cause-analysis
description: Investigates why a bug, outage, or process failure happened using 5 Whys and Ishikawa fishbone methods. Trigger — "why did this break", "root cause analysis", "5 whys", "what caused this failure", "post-mortem". Skip for forward-looking risk analysis — use failure-mode-analysis instead.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: root-cause-analysis
  maturity: draft
  risk: low
  tags: [root, cause, analysis]
---

# Purpose
Traces a specific failure—a bug, outage, process breakdown, or missed goal—back to its root cause rather than stopping at symptoms or proximate causes. This combines the 5 Whys technique with Ishikawa (fishbone) diagram thinking to systematically drill from symptom to root cause and identify corrective actions that prevent recurrence.

# When to use this skill
Use when:
- The user says "root cause this", "why did this happen?", "5 Whys", "post-mortem", or "incident analysis"
- A bug or outage has occurred and the real cause needs identification to prevent recurrence
- A process failure repeated despite apparent fixes—the deeper cause hasn't been found
- An agent or system produced unexpected output and the source of deviation needs tracing
- Writing incident retrospectives or post-mortems

Do NOT use when:
- The failure hasn't occurred yet (use `premortem` or `failure-mode-analysis`)
- The user wants a catalogue of all possible failures, not investigation of one specific event
- The failure is fully understood and the user wants remediation planning, not investigation
- The user wants to attack a plan adversarially (use `red-team-challenge`)

# Operating procedure
1. **State the symptom precisely**: Write one sentence describing the observable failure:
   - What happened (observable fact, not interpretation)
   - When it happened (timestamp, duration)
   - What was affected (users, data, services)
   - What was the impact (severity, blast radius)
   
   Avoid inferring cause in this statement. "The deployment failed" not "The deployment failed because of bad config."

2. **Collect available evidence**: List everything known:
   - Error messages and stack traces
   - Log entries around the failure time
   - Git blame for relevant code
   - Recent changes (deploys, config changes, dependency updates)
   - Inputs that triggered the failure
   - Who reported it and what they observed
   - Metrics and monitoring data

3. **Apply 5 Whys from the symptom**: Start with the symptom. Ask "Why did this happen?" Answer with the most proximate cause. Repeat.

   Example:
   1. Why did the API return 500? → Database connection timed out
   2. Why did the connection time out? → Connection pool exhausted
   3. Why was the pool exhausted? → Connections weren't being returned
   4. Why weren't connections returned? → Exception path skipped cleanup
   5. Why did exception path skip cleanup? → Try/finally wasn't used
   
   Keep going until you reach a cause that is within your control to fix and that, if fixed before the incident, would have prevented it.

4. **Cross-check with fishbone categories**: Ensure no contributing cause is missed by checking each Ishikawa category:
   - **People**: Wrong understanding, missing skill, miscommunication, handoff failure
   - **Process**: Missing step, unclear procedure, no review, no checklist
   - **Technology**: Bug, config error, version mismatch, capacity, race condition
   - **Environment**: Infrastructure, network, third-party service, region
   - **Data**: Corrupt input, missing data, wrong format, unexpected scale

5. **Validate the root cause**: Apply this test: "If we had fixed this cause before the incident, would the failure have occurred?"
   - If NO → This is a valid root cause
   - If YES → Keep going; this is a contributing cause, not root cause

6. **Distinguish root cause from contributing causes**:
   - **Root cause**: The single deepest cause that, if removed, prevents this class of failure
   - **Contributing causes**: Factors that made the failure more likely or more severe, but didn't cause it alone

7. **Propose corrective actions**:
   - For root cause: One specific action that prevents recurrence
   - For major contributing causes: Actions that reduce likelihood
   - Each action needs: Description, Owner, Deadline, Verification method

# Output defaults
A **Symptom** statement (one sentence, facts only).

A **5 Whys Chain** (numbered list showing the causal chain).

A **Fishbone Summary** table: Category | Contributing Factor | Present?

A **Root Cause** statement (one sentence, clearly labeled).

A **Contributing Causes** list (factors that made it worse).

A **Corrective Actions** table: Action | Type (Preventive/Detective/Corrective) | Owner | Deadline | Verification

# Named failure modes of this method

- **Stopping at the proximate cause**: Declaring the first plausible cause as root cause without asking further "why" questions. Fix: keep going until you reach a cause that is within your control and that, if fixed, would have prevented the failure.
- **Blame-as-root-cause**: Naming a person or team as the root cause instead of the systemic condition that allowed the error. Fix: root causes are always systemic—missing guardrails, unclear procedures, inadequate testing—not individual mistakes.
- **Multiple root causes dodge**: Listing 5+ "root causes" to avoid identifying the real one. Fix: there is usually one root cause and several contributing causes. Name the one that, if removed, breaks the causal chain.
- **Solution-first RCA**: Starting with the desired fix and reasoning backward to justify it. Fix: complete the 5 Whys and fishbone analysis before proposing any corrective action.
- **Missing the process failure**: Finding the technical cause while missing the process failure that allowed it (no code review, no testing, no monitoring). Fix: always check the fishbone People and Process categories.

# References
- https://en.wikipedia.org/wiki/Root_cause_analysis — overview of RCA techniques
- https://en.wikipedia.org/wiki/Five_whys — Toyota Production System technique
- https://en.wikipedia.org/wiki/Ishikawa_diagram — fishbone diagram methodology
- Google SRE Postmortem Culture (https://sre.google/workbook/postmortem-culture/) — blameless postmortem practices
- Allspaw, J. "Blameless PostMortems" — modern incident analysis culture

# Failure handling
If insufficient evidence is available:
1. List the specific evidence gaps that prevent completing the 5 Whys chain
2. Identify at which "Why" the chain breaks due to missing information
3. State what data needs to be collected (logs, metrics, interviews)
4. Provide partial analysis with clear "unknown beyond this point" marker
