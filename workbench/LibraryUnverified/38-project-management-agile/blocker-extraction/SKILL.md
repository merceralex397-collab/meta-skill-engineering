---
name: blocker-extraction
description: Scans plans, tickets, or status updates to identify true blockers and separate them from risks and unknowns. Trigger — "what's blocking us", "extract blockers", "what do we need before we can proceed", "unblock this". Skip when the goal is general risk analysis or root-cause investigation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: blocker-extraction
  maturity: draft
  risk: low
  tags: [blocker, extraction]
---

# Purpose
Scans a plan, ticket, or status document to identify what genuinely cannot proceed until something external resolves, separating true blockers from noise, unknowns, and deferrable risks. Teams often conflate "blocked" with "uncertain" or "risky"—this skill applies strict criteria to distinguish items that actually halt progress from items that merely add uncertainty.

# When to use this skill
Use when:
- The user says "what's blocking us?", "extract the blockers", "what do we need before we can proceed?", or "unblock this"
- A sprint, project, or ticket is stalled and the blockers need explicit identification
- A status update contains a mix of blockers, risks, and open questions that need untangling
- An agent workflow is stuck and the blocker needs to be identified before escalating
- Standup or retrospective prep to classify impediments

Do NOT use when:
- The user wants general risk analysis (use `premortem`)
- The goal is to prioritize work, not identify what blocks it
- Everything is flowing—no blockers exist
- The user wants to understand why something failed (use `root-cause-analysis`)

# Operating procedure
1. **Collect all stated problems**: Read the plan, tickets, or status. List every item flagged as blocked, unclear, waiting, or dependent. Include:
   - Explicit "blocked by" labels
   - "Waiting on" statements
   - "TBD" items
   - Dependencies mentioned without resolution
   - Questions without answers

2. **Apply the blocker test**: For each item, ask: "Can meaningful work proceed on this task without this being resolved?"
   - If YES → Not a blocker (work can continue with this uncertainty)
   - If NO → True blocker (work must stop until resolved)
   
   Be strict. Most "unknowns" are not true blockers—they add risk but don't halt progress.

3. **Classify each true blocker**:
   - **Decision blocker**: A choice must be made by a specific person before work can continue. Name the decision and the decision-maker.
   - **Dependency blocker**: An external system, team, or artifact must be delivered first. Name the dependency, its owner, and expected date.
   - **Information blocker**: A specific piece of data or evidence is required and does not yet exist. Name what's needed and who can provide it.
   - **Resource blocker**: People, budget, access, or equipment are unavailable. Name the specific resource and who controls it.
   - **Technical blocker**: A technical prerequisite must be completed first. Name the prerequisite and who owns it.

4. **Assess staleness**: How long has each blocker been active?
   - < 1 week: Fresh—normal escalation
   - 1-2 weeks: Stale—needs attention
   - \> 2 weeks: Chronic—may be accepted risk masquerading as blocker

5. **For each blocker, write an unblock action**: One specific action that would resolve the blocker:
   - Action (verb + object)
   - Owner (name or role)
   - Deadline (date)
   
   If no action exists, the blocker is either a risk to accept or a reason to descope.

6. **Separate non-blockers**: List items that appeared to be blockers but are not:
   - Deferred decisions (can proceed without)
   - Accepted unknowns (working around them)
   - Workaroundable items (alternative path exists)

# Output defaults
A **True Blockers** table with columns: Blocker | Type | Owner | Age | Unblock Action | Deadline

A **Non-Blockers** list with brief explanation of why each is not blocking.

A **Stale Blockers** callout for anything unresolved for more than two weeks—these need escalation or descoping.

A **Critical Path** note: which blocker, if unresolved, blocks the most other work.

# Named failure modes of this method

- **False blocker inflation**: Labeling every unknown or risk as a blocker, paralyzing progress. Fix: apply the strict test—"Can meaningful work proceed without this?"—and be honest about the answer.
- **Chronic blocker acceptance**: Blockers that have been "active" for weeks are often accepted constraints masquerading as impediments. Fix: if a blocker is older than 2 weeks with no unblock action, reclassify it as an accepted risk or a descoping trigger.
- **Missing the real blocker**: Documenting the stated blockers while missing the unstated one (often a decision nobody wants to make or a person nobody wants to escalate to). Fix: ask "who is avoiding what decision?" when all stated blockers seem addressable but progress remains stalled.
- **Workaround blindness**: Declaring something blocked when a workaround exists. Fix: for each blocker, explicitly ask "is there an alternative path that avoids this dependency?"
- **Owner-free blockers**: Listing blockers without naming who can unblock them. Fix: every blocker must have an owner and an action, or it's just a complaint.

# References
- Theory of Constraints (Goldratt, E.) — identifying true constraints vs. noise
- Kanban — WIP limits and blocker visualization practices
- JIRA/Linear blocker patterns — common taxonomy for impediments

# Failure handling
If the input does not contain enough status information to determine what is genuinely blocking versus what is merely uncertain:
1. List the three questions that would resolve the ambiguity
2. Make explicit which items cannot be classified without more information
3. Classify items that can be determined from available information
