---
name: red-team-challenge
description: Adopt an adversary's mindset and actively attempt to break, exploit, or defeat a design, plan, or system. Use when the user says "red-team this", "attack this design", "find the holes", "how would an adversary exploit this", or when a proposal needs adversarial stress-testing beyond risk listing. Do not use for cataloguing risks without exploitation (use premortem), doubting evidence quality (use skeptic-pass), or reconstructing strongest arguments (use steelman).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: red-team-challenge
  maturity: draft
  risk: low
  tags: [red-team, adversarial, security, critique]
---

# Purpose

Adopt the adversary's mindset and actively try to break the target. A red-team challenge does not list risks politely—it constructs specific attack scenarios, walks through exploitation step by step, and reports what actually breaks. The output is an attack report, not a worry list.

This differs from related skills:
- **premortem** imagines failure and works backward to causes. Red-team works forward from attack to breach.
- **skeptic-pass** doubts claims and demands evidence. Red-team actively exploits weaknesses.
- **reverse-brainstorming** asks "how to sabotage" then inverts. Red-team stays in attacker mode throughout.

# When to use this skill

Use when:
- the user says "red-team this", "attack this design", "find the holes", "how would an adversary exploit this"
- a security-sensitive design needs adversarial review before deployment
- a plan has passed normal review and needs deliberate adversarial stress-testing
- the team suspects the design has weaknesses but normal critique hasn't surfaced them
- an API, auth flow, data pipeline, or agent workflow needs abuse-case analysis

Do NOT use when:
- the task is listing risks with likelihood/severity ratings (use `failure-mode-analysis`)
- the task is doubting claims and checking evidence quality (use `skeptic-pass`)
- the failure already happened and needs investigation (use `root-cause-analysis`)
- the task is rebuilding an argument in its strongest form (use `steelman`)

# Operating procedure

1. **Define the target and threat model.**
   State what you are attacking in one sentence: "I am attacking [system/plan/design] as [adversary type]."
   Choose the adversary persona most relevant to the context:
   - External attacker (unauthorized access, injection, abuse)
   - Malicious insider (privilege abuse, data exfiltration)
   - Incompetent operator (misconfiguration, accidental damage)
   - Hostile competitor (sabotage, denial of service, IP theft)
   - Malicious user (abuse of legitimate features)

2. **Map the attack surface.**
   Enumerate every entry point, trust boundary, data flow, and exposed interface. For each, note:
   - What input does it accept?
   - What privilege does it assume?
   - What validation does it perform?
   - What happens if the input is malformed, oversized, duplicated, or replayed?

3. **Generate attack vectors.**
   For each surface element, brainstorm specific attacks. Use these categories as prompts:
   - **Injection**: Can untrusted input reach a parser, query engine, or command interpreter?
   - **Bypass**: Can authentication, authorization, or validation be circumvented?
   - **Abuse**: Can legitimate features be used in unintended harmful ways?
   - **Escalation**: Can low-privilege access be leveraged into high-privilege access?
   - **Denial**: Can the system be overwhelmed, starved, or deadlocked?
   - **Data compromise**: Can sensitive data be read, modified, or exfiltrated?
   - **Supply chain**: Can dependencies, build tools, or deployment pipelines be compromised?
   - **Social/process**: Can human processes (approvals, reviews, handoffs) be subverted?

4. **Walk through the top attacks step by step.**
   For each high-potential attack vector, write the exploitation narrative:
   - **Entry**: How the attacker gains initial access or triggers the vulnerability
   - **Exploitation**: What the attacker does once inside or once the weakness is triggered
   - **Impact**: What damage results—data loss, unauthorized access, service disruption, reputation harm
   - **Detection**: Would the current system detect this attack? How quickly?
   - **Evidence**: What traces would the attack leave?

5. **Chain attacks.**
   Look for combinations where one successful attack enables another:
   - Low-severity injection + missing rate limit = account takeover
   - Read-only data leak + social engineering = privilege escalation
   - Config misconfiguration + no monitoring = persistent undetected access

6. **Rate each finding.**
   - **Critical**: Exploitable now with high impact, no detection
   - **High**: Exploitable with moderate effort, significant impact
   - **Medium**: Requires specific conditions but impact is real
   - **Low**: Theoretical or requires unlikely conditions
   - **Informational**: Weakness exists but exploitation path is blocked

7. **Propose defenses.**
   For Critical and High findings, propose specific countermeasures:
   - Preventive control (block the attack)
   - Detective control (catch the attack in progress)
   - Corrective control (limit damage after breach)

# Output contract

Return an **Attack Report** with these sections:

1. `Target & Threat Model` — one-sentence target, adversary persona
2. `Attack Surface Map` — enumerated entry points and trust boundaries
3. `Attack Scenarios` — top 5-10 attacks, each with Entry → Exploitation → Impact → Detection
4. `Attack Chains` — compound attacks where findings combine
5. `Severity Ratings` — table: Finding | Severity | Exploitability | Impact | Detection Gap
6. `Recommended Defenses` — specific countermeasures for Critical/High findings
7. `Residual Risk` — what remains exploitable even after proposed defenses

# Named failure modes of this method

- **Tourist red-team**: Listing generic risks (e.g., "SQL injection is possible") without walking through the specific exploitation path in this system. Fix: every finding must include the concrete entry→exploit→impact chain.
- **Scope explosion**: Trying to attack everything instead of the highest-value targets. Fix: prioritize attack surface by value and exposure before generating vectors.
- **Friendly adversary**: Pulling punches or softening findings because the author is present. Fix: write the report as if you are briefing a security team, not the developer.
- **Checkbox security**: Only checking OWASP Top 10 without thinking about this system's specific abuse cases. Fix: generate system-specific abuse scenarios before reaching for standard checklists.
- **Missing chains**: Rating each finding independently without considering how they combine. Fix: always attempt to chain findings after individual assessment.

# References

- OWASP Testing Guide — structured web application security testing methodology
- OWASP ASVS (https://owasp.org/www-project-application-security-verification-standard/) — verification levels for security requirements
- NIST SP 800-115 — technical guide to information security testing
- MITRE ATT&CK — adversary tactics and techniques knowledge base
- Shostack, A. (2014). Threat Modeling: Designing for Security — STRIDE and attack trees

# Failure handling

- If the target is too vague to map an attack surface, ask for the specific system, API, flow, or design to attack before proceeding.
- If the target is a plan rather than a system, focus on process exploitation: what can go wrong if a hostile or careless actor touches this plan?
- If you find no Critical or High findings, say so explicitly—do not invent drama. A clean red-team report is a valid outcome.
