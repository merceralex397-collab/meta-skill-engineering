---
name: safety-guardrails
description: Implement input and output guardrails for LLM applications — add prompt injection detection, PII scrubbing, toxicity filtering, content classification, output validation, and refusal handling. Use when hardening an LLM-powered feature against adversarial input, data leakage, or harmful output. Do not use for application-level auth/authz, network security, or non-LLM input validation.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: safety-guardrails
  maturity: draft
  risk: low
  tags: [safety, guardrails, prompt-injection, pii, content-filter]
---

# Purpose

Use this skill to add defensive layers around LLM-powered features — detect and block prompt injection attempts, scrub PII from inputs and outputs, classify and filter harmful content, validate output structure and safety, and handle refusal paths gracefully.

# When to use this skill

Use this skill when:

- adding prompt injection detection to an LLM input pipeline (system prompt extraction, jailbreak attempts, indirect injection via retrieved documents)
- implementing PII detection and scrubbing (names, emails, phone numbers, SSNs, addresses) before sending user data to an LLM or logging LLM outputs
- adding toxicity or harmful content filtering on LLM outputs (hate speech, self-harm, violence, explicit content)
- building content classification layers that route or block requests based on topic (e.g., medical advice, legal guidance, financial recommendations)
- implementing output validation that checks LLM responses against safety policies before returning to users
- designing refusal handling — what the system says and does when a guardrail triggers

# Do not use this skill when

- the task is application-level authentication, authorization, or RBAC
- the task is network security, WAF configuration, or infrastructure hardening
- the task is input validation for non-LLM endpoints (form validation, API schema validation)
- the task is content moderation for user-generated content without LLM involvement
- a narrower active skill already owns the problem

# Operating procedure

1. Inventory the threat surface.
   Map every point where untrusted input reaches the LLM: user messages, retrieved documents, tool outputs, file uploads, URL contents. Map every point where LLM output reaches users or external systems.

2. Implement input guardrails.
   - **Prompt injection detection**: Run a classifier (e.g., `rebuff`, `protectai/deberta-v3-base-prompt-injection`, or rule-based heuristics) on user input before it reaches the LLM. Flag inputs that attempt to override the system prompt, extract system instructions, or inject new instructions.
   - **PII scrubbing**: Use a NER model or regex-based detector (e.g., `presidio`, `scrubadub`) to identify and redact PII in user input before sending to the LLM. Log redacted versions only.
   - **Input length and rate limiting**: Cap input length to prevent context window abuse. Rate-limit per user to prevent automated probing.

3. Implement output guardrails.
   - **Toxicity filtering**: Run LLM output through a toxicity classifier (e.g., `Detoxify`, OpenAI moderation endpoint, Perspective API) before returning to the user. Block or flag outputs above the threshold.
   - **PII leakage detection**: Scan LLM output for PII that should not be exposed — especially when the model has access to sensitive retrieved documents.
   - **Content policy validation**: Check output against domain-specific policies (e.g., no medical diagnoses, no specific investment advice, no legal conclusions) using keyword rules or a lightweight classifier.

4. Design the refusal path.
   When a guardrail triggers, return a safe default response that acknowledges the request without revealing why it was blocked. Log the full context (input, guardrail triggered, confidence score) for review. Never expose guardrail implementation details to the user.

5. Handle indirect prompt injection.
   For RAG systems, scan retrieved documents for injection payloads before including them in the prompt. Treat all retrieved content as untrusted — delimit it clearly in the prompt with instruction-hierarchy markers.

6. Build a test suite of adversarial inputs.
   Create a dataset of 50+ prompt injection attempts (jailbreaks, system prompt extraction, role-play escapes, indirect injection via documents). Include benign inputs that resemble attacks to test for false positives. Run this suite on every guardrail change.

7. Monitor and iterate.
   Log all guardrail activations with timestamps, input hashes, and confidence scores. Review false positive and false negative rates weekly. Tune thresholds based on production data.

# Decision rules

- Apply guardrails as middleware layers, not inline in application code — guardrails must be modular and independently testable.
- Default to blocking and logging when a guardrail triggers — false negatives are worse than false positives for safety.
- Use a dedicated moderation model or API for toxicity — do not rely on the primary LLM to self-censor.
- Never trust the LLM to detect its own prompt injection — use an independent classifier or rule system.
- PII scrubbing must happen before the LLM call, not after — once PII reaches the model, it may appear in logs, caches, or fine-tuning data.

# Output requirements

1. `Threat Surface Map` — diagram or list of all input/output points with trust boundaries
2. `Guardrail Pipeline Config` — ordered list of input and output guardrails with thresholds and actions
3. `Refusal Response Templates` — safe default messages for each guardrail trigger category
4. `Adversarial Test Suite` — JSONL dataset with attack inputs, expected guardrail behavior, and benign control inputs
5. `Monitoring Dashboard Spec` — metrics to track (activation rate, false positive rate, latency overhead)

# References

Read these only when relevant:

- `references/prompt-injection-taxonomy.md`
- `references/pii-detection-tools.md`
- `references/content-classification-models.md`

# Related skills

- `llm-integration`
- `tool-use-agents`
- `structured-output-pipelines`

# Anti-patterns

- Relying on the system prompt alone to prevent jailbreaks — system prompts are not a security boundary and can be overridden.
- Running guardrails only on input but not on output — the model can generate harmful content from benign prompts.
- Logging full user inputs with PII for guardrail debugging — creates a data leakage risk in log storage.
- Using a single toxicity threshold for all content categories — different categories (self-harm vs profanity) need different sensitivity levels.
- Hardcoding guardrail rules without a test suite — rules drift and break silently as prompts and models change.

# Failure handling

- If the prompt injection classifier has a high false positive rate (>5%), add a confidence threshold and route borderline cases to a secondary check rather than blocking.
- If PII scrubbing misses a category (e.g., non-US phone formats), add format-specific regex rules to the detection pipeline and add test cases.
- If the toxicity filter adds >200ms latency, switch to a lighter model or run it asynchronously with a hold-and-release pattern on the response.
- If guardrail bypass is discovered in production, immediately add the payload to the adversarial test suite, patch the rule or model, and audit logs for similar attempts.
