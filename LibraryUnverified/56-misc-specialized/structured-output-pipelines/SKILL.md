---
name: structured-output-pipelines
description: Enforce structured output from LLMs using JSON schema constraints, Pydantic model binding, function calling extraction, and retry/repair loops for malformed responses. Use when an LLM must produce typed JSON, fill a schema, or return structured data that downstream code parses. Do not use for free-form text generation, summarization, or chat responses where structure is not required.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: structured-output-pipelines
  maturity: draft
  risk: low
  tags: [structured-output, json-schema, pydantic, function-calling]
---

# Purpose

Use this skill to reliably extract structured data from LLM outputs — define JSON schemas or Pydantic models, choose the right extraction method (native JSON mode, function calling, or prompted extraction), implement retry and repair loops for malformed responses, and validate outputs before passing them to downstream code.

# When to use this skill

Use this skill when:

- an LLM must return valid JSON conforming to a specific schema (API responses, data extraction, form filling)
- defining Pydantic models that bind to LLM outputs via `instructor`, `langchain`, or `openai` function calling
- using OpenAI's `response_format: { type: "json_schema" }` or Anthropic's tool-use for structured extraction
- implementing function calling where the LLM selects and parameterizes tool calls
- building retry/repair loops that re-prompt the LLM when output fails validation
- extracting structured entities from unstructured text (names, dates, addresses, product attributes)
- debugging malformed LLM JSON output (trailing commas, unquoted keys, truncated responses)

# Do not use this skill when

- the LLM output is free-form text (summaries, creative writing, chat replies)
- the task is defining API request/response schemas without LLM involvement (use standard API design practices)
- the task is data validation for user input in a web form (use standard validation libraries)
- a narrower active skill already owns the problem

# Operating procedure

1. Define the target schema.
   Write a Pydantic model (v2 preferred) or JSON Schema that describes every field the LLM must produce. Use `Field(description="...")` on every field — these descriptions become part of the extraction prompt. Mark optional fields with `Optional[T]` and provide defaults. Use `Literal` or `Enum` for constrained-choice fields.

2. Choose the extraction method.
   - **Native JSON mode** (OpenAI `response_format`, Anthropic tool-use): Guarantees syntactically valid JSON. Use when the provider supports it and schema complexity is moderate.
   - **Function calling** (OpenAI functions/tools, Anthropic tool-use): The LLM "calls a function" with typed arguments matching your schema. Best for action-oriented extraction (tool selection, API parameter filling).
   - **Prompted extraction with parsing**: Include the JSON schema in the prompt and parse the response. Use as fallback for models without native JSON mode. Wrap with `json.loads()` and Pydantic `model_validate()`.
   - **Instructor library**: Wraps OpenAI/Anthropic clients with automatic Pydantic validation and retry. Use for rapid prototyping and when retry logic is needed.

3. Craft the extraction prompt.
   Place the schema definition or example output in the system prompt. Include 1–2 few-shot examples of input → expected JSON output. Explicitly instruct the model to output only JSON with no surrounding text, markdown fences, or explanation.

4. Implement validation.
   Parse the raw LLM response with `json.loads()`. Validate against the Pydantic model using `model_validate(data)`. Catch `ValidationError` and extract the specific field failures for the repair prompt.

5. Build the retry/repair loop.
   On validation failure, construct a repair prompt: include the original input, the malformed output, and the specific validation errors. Re-send to the LLM with instructions to fix only the errors. Limit retries to 3 attempts. If all retries fail, return a structured error object rather than crashing.

6. Handle edge cases.
   - **Truncated JSON**: If the response hit the `max_tokens` limit, increase the limit or simplify the schema.
   - **Extra fields**: Use `model_config = ConfigDict(extra="ignore")` in Pydantic to silently drop unexpected fields.
   - **Markdown wrapping**: Strip ` ```json ` fences before parsing if the model wraps output despite instructions.
   - **Null vs missing**: Distinguish between `null` values and missing keys — use Pydantic's `model_fields_set` to detect.

7. Test with diverse inputs.
   Run 20+ representative inputs through the full pipeline. Verify that every field populates correctly, edge cases (empty lists, null optionals, enum boundaries) are handled, and the retry loop activates and recovers on at least 80% of initial failures.

# Decision rules

- Use native JSON mode or function calling over prompted extraction whenever the provider supports it — parsing reliability is dramatically higher.
- Always validate with Pydantic, not just `json.loads()` — syntactically valid JSON can still violate the schema.
- Limit schema depth to 3 levels of nesting — deeper schemas increase extraction failure rates.
- Use `Literal["option1", "option2"]` instead of `str` for any field with a known finite set of values.
- Set `max_tokens` to at least 2x the expected output size to prevent truncation.
- When retry count exceeds 2 for >10% of requests, simplify the schema or add more few-shot examples rather than increasing retry budget.

# Output requirements

1. `Pydantic Model or JSON Schema` — complete schema with field descriptions and constraints
2. `Extraction Prompt` — system prompt, few-shot examples, and output instructions
3. `Pipeline Code` — LLM call → parse → validate → retry loop → output or error
4. `Validation Test Suite` — 10+ test cases covering happy path, edge cases, and expected failures
5. `Error Handling Spec` — what happens when all retries fail (fallback value, error response, alert)

# References

Read these only when relevant:

- `references/json-schema-best-practices.md`
- `references/function-calling-patterns.md`
- `references/retry-repair-strategies.md`

# Related skills

- `llm-integration`
- `tool-use-agents`
- `rag-retrieval`

# Anti-patterns

- Parsing LLM output with regex instead of `json.loads()` + Pydantic — brittle and fails on edge cases.
- Defining schemas without field descriptions — the LLM has no guidance on what each field should contain.
- Retrying with the exact same prompt on failure — the model will produce the same error; include the validation error in the retry prompt.
- Using `str` type for every field and parsing later — loses the schema constraint benefit and pushes validation downstream.
- Embedding the schema in the user message instead of the system prompt — reduces reliability on multi-turn conversations.

# Failure handling

- If the LLM consistently returns extra text around the JSON, add a post-processing step that extracts the first `{...}` or `[...]` block before parsing.
- If a specific field fails validation repeatedly, add a targeted few-shot example that demonstrates the correct format for that field.
- If function calling returns `null` arguments, verify the function schema matches the model's expected format and that required fields are marked correctly.
- If the retry loop cannot recover, log the original input, all attempts, and validation errors, then return a typed error object with `success: false` and the failure reason.
