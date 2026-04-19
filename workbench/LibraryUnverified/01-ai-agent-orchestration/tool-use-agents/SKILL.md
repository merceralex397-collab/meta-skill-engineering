---
name: tool-use-agents
description: Design and implement LLM tool-use and agent workflows — register tool schemas, build agent control loops, parse observations, handle multi-step reasoning chains, and manage tool errors and retries. Use when building agents that call external tools/APIs, designing function-calling schemas, or debugging agent loop behavior. Do not use for simple prompt-response LLM calls, RAG pipelines without tool use, or rule-based automation without LLM reasoning.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: tool-use-agents
  maturity: draft
  risk: low
  tags: [tool-use, agents, function-calling, agent-loop]
---

# Purpose

Use this skill to build LLM-driven agents that invoke external tools — define tool schemas with clear input/output contracts, implement the observe-think-act control loop, handle tool execution errors and retries, enforce safety limits on iterations and cost, and debug common agent failure modes like infinite loops and hallucinated tool calls.

# When to use this skill

Use this skill when:

- registering tools with the LLM via function/tool calling schemas (OpenAI tools, Anthropic tool-use, open-source function-calling models)
- building the agent control loop (prompt → LLM → tool call → execute tool → observation → prompt)
- designing tool schemas with typed parameters, descriptions, and required/optional fields
- implementing multi-step agent workflows where the LLM chains multiple tool calls to complete a task
- parsing tool observations and formatting them back into the conversation context
- adding safety limits (max iterations, max tokens, timeout, cost cap) to agent loops
- debugging agent failures (infinite loops, hallucinated tool names, malformed arguments, stuck states)

# Do not use this skill when

- the task is a single prompt → response LLM call with no tool invocation
- the task is a RAG pipeline where retrieval happens once before generation (use `rag-retrieval`)
- the task is rule-based automation or workflow orchestration without LLM decision-making
- the task is defining API endpoints that happen to be called by agents (use the relevant API skill)
- a narrower active skill already owns the problem

# Operating procedure

1. Define the tool inventory.
   List every tool the agent can call. For each tool, write a JSON schema with `name`, `description`, and `parameters` (typed, with per-field descriptions). Include `required` vs optional parameters. Keep tool count under 20 — more tools degrade selection accuracy.

2. Write clear tool descriptions.
   Each tool description must state: what the tool does, when to use it, and what it returns. Avoid vague descriptions like "useful helper" — the LLM uses these to decide when to call the tool. Include a one-line example of valid arguments.

3. Implement the agent control loop.
   Loop: send messages + tool schemas → LLM → if tool calls, execute each and append results as tool-result messages → repeat. If no tool calls, return the final answer. Always append tool results as tool-result messages, not user messages.

4. Validate tool arguments before execution.
   Parse tool call arguments with the tool's schema (Pydantic or JSON Schema validation). Reject malformed arguments with a clear error message returned as the tool result — do not crash the loop. Let the LLM retry with corrected arguments.

5. Handle tool execution errors.
   Wrap every tool execution in try/except. Return error details as the tool result (e.g., `{"error": "HTTP 429 rate limited", "retry_after": 30}`). The LLM can decide to retry, use a different tool, or give up. Never let tool exceptions crash the agent loop.

6. Enforce safety limits.
   - **Max iterations**: Cap at 10–25 depending on task complexity. Return a timeout message when exceeded.
   - **Max tokens**: Track cumulative token usage across all LLM calls in the loop. Abort if budget exceeded.
   - **Cost cap**: Estimate cost per iteration and set a dollar limit for the full agent run.
   - **Timeout**: Set a wall-clock timeout for the entire agent run (e.g., 120 seconds).

7. Implement observation formatting.
   Truncate long tool outputs to fit within the context window. For API responses, extract only the relevant fields rather than including the full payload. For file contents, include line numbers and truncate to the relevant section.

8. Test with multi-step scenarios.
   Create 5–10 test scenarios requiring 2–5 tool calls each. Verify the agent selects the correct tools in the right order, handles intermediate errors gracefully, and produces the correct final answer. Include at least one adversarial scenario where the correct action is to stop early.

# Decision rules

- Give each tool a distinct, non-overlapping purpose — ambiguous tool boundaries cause the LLM to pick randomly.
- Return tool errors as tool results, never as exceptions — the LLM must see errors to reason about them.
- Prefer parallel tool calls when supported (OpenAI parallel function calling) to reduce round trips.
- Use `enum` types in tool schemas for constrained parameters — reduces hallucinated argument values.
- If the agent loops more than 3 times without making progress (same tool call repeated), inject a "step back and reconsider" message or abort.
- Never let the agent call tools with side effects (write, delete, send) without explicit confirmation gates in production.

# Output requirements

1. `Tool Schema Registry` — JSON schemas for all registered tools with descriptions and typed parameters
2. `Agent Loop Implementation` — control loop code with iteration, token, and cost limits
3. `Error Handling Matrix` — for each tool, the possible errors and how the agent should respond
4. `Test Scenarios` — multi-step test cases with expected tool call sequences and final answers
5. `Safety Configuration` — max iterations, token budget, cost cap, and timeout values

# References

Read these only when relevant:

- `references/tool-schema-design.md`
- `references/agent-loop-patterns.md`
- `references/tool-error-handling.md`

# Related skills

- `llm-integration`
- `structured-output-pipelines`
- `safety-guardrails`

# Anti-patterns

- Registering too many tools (>20) — the LLM's tool selection accuracy drops and it may hallucinate tool names.
- Using vague tool descriptions like "general helper function" — the LLM cannot distinguish when to use it.
- Appending tool results as user messages instead of tool-result messages — confuses the conversation structure and degrades reasoning.
- No iteration limit on the agent loop — allows infinite loops that burn tokens and cost.
- Letting the agent call destructive tools (delete, send email, deploy) without a confirmation gate — one bad tool call has irreversible consequences.
- Returning full API responses as tool observations without truncation — fills the context window and crowds out reasoning.

# Failure handling

- If the agent enters an infinite loop (same tool call 3+ times), inject a system message summarizing what has been tried and instruct the model to either try a different approach or return a final answer.
- If the agent hallucinates a tool name that does not exist, return an error listing available tools and their descriptions — the LLM will self-correct on the next iteration.
- If tool execution exceeds its timeout, return a timeout error as the tool result and let the agent decide whether to retry or skip.
- If the agent exhausts its iteration budget without a final answer, return the last tool observations and a message explaining the limit was reached — never silently drop the conversation.
