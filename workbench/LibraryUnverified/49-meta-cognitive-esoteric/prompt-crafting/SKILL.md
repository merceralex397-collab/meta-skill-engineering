---
name: prompt-crafting
description: Write, rewrite, or improve agent prompts, system instructions, tool descriptions, and output templates for reliable behavior. Use when the user says "write a prompt", "improve prompt", "fix agent prompt", "system prompt", "tool description wording", or when an agent's output is drifting, hallucinating, or under/over-triggering. Do not use for context-intelligence (context window management), planning (task decomposition), or agent-prompt-engineering (hardening prompts for multi-model reliability).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: prompt-crafting
  maturity: draft
  risk: low
  tags: [prompt, crafting]
---

# Purpose
Writes, rewrites, or improves prompts to produce reliable, consistent agent behavior. Applies prompt engineering principles: clear role assignment, explicit constraints, structured output format, few-shot examples where needed, and evidence anchoring.

# When to use this skill
Use when:
- User says "write a prompt for X", "improve this prompt", "fix this agent", "why is this producing bad output"
- Agent instruction is vague, inconsistent, hallucinating, or drifting from intended behavior
- New command, tool, or workflow needs its prompt written from scratch
- Existing AGENTS.md, tool description, or system prompt needs sharpening

Do NOT use when:
- User wants to write a full SKILL.md with frontmatter/evals (use `skill-authoring`)
- Problem is the tool's implementation, not the prompt
- Agent works correctly—no change needed

# Operating procedure
1. **Identify prompt type** (each has different optimization targets):
   - **System prompt**: Sets role, constraints, general behavior
   - **Tool description**: Routing logic—when to invoke, what it does
   - **User-facing command**: What human types to trigger action
   - **Output template**: Exact format of response
2. **State intended behavior in one sentence**: "When triggered, the agent should [specific action] and return [specific output]"
3. **Diagnose current failure** (if rewriting):
   - Scope drift? (does more/less than intended)
   - Wrong format? (structured vs prose, missing sections)
   - Hallucination? (invents facts, cites nonexistent sources)
   - Under-triggering? (doesn't fire when it should)
   - Over-triggering? (fires when it shouldn't)
4. **Apply core prompt engineering principles**:
   - **Role first**: "You are a [specific role] responsible for [specific scope]"
   - **Constraint before freedom**: State what NOT to do before what to do
   - **Positive imperatives**: "Always X" not "Try to X" or "You might want to X"
   - **Explicit output format**: Show example or schema, use XML tags for structure
   - **Evidence anchoring**: "Quote the relevant section before answering" if citing needed
5. **Add few-shot examples** (when format is complex or behavior is subtle):
   - 3-5 examples covering normal case + edge cases
   - Wrap in `<example>` tags to separate from instructions
   - Make examples diverse to avoid overfitting
6. **For routing/trigger text**:
   - Lead with most discriminating signal word
   - Include "Use when: [specific triggers]"
   - Include "Do NOT use when: [common confusion cases]"
7. **Remove filler**: Cut "please", "try to", "it would be ideal if", "you may want to consider"
8. **Test mentally**: Walk through the failure case from step 3—does the new prompt prevent it?

# Output defaults
```
## Change Summary
[What was changed and why, 2-3 sentences]

## Before
[Original prompt if rewriting]

## After
[New prompt]

## Rationale
- [Specific change 1]: [Why it helps]
- [Specific change 2]: [Why it helps]
```

For new prompts, omit "Before" section.

# References
- https://docs.anthropic.com/en/docs/build-with-claude/prompt-engineering/overview — Anthropic prompt engineering guide
- https://docs.anthropic.com/en/docs/build-with-claude/prompt-engineering/claude-prompting-best-practices — detailed best practices
- Key techniques: XML structuring, role prompting, few-shot examples, chain-of-thought, explicit constraints

# Failure handling
- **Intended behavior unknown**: Return three questions: "Who uses this?", "What output is expected?", "What failure mode should this prevent?"
- **Multiple conflicting goals**: Split into separate prompts or use conditional structure
- **Prompt too long**: Extract reference material to separate file, keep core instructions concise
- **Can't reproduce failure**: Ask for specific input that caused the problem
