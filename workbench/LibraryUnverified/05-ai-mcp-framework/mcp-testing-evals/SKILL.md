---
name: mcp-testing-evals
description: "Test MCP servers using the MCP Inspector, automated test harnesses, and evaluation suites. Use when writing tests for MCP tool behavior, creating evaluation question sets for MCP servers, building CI pipelines for MCP servers, or validating tool responses against expected schemas."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-testing-evals
  maturity: stable
  risk: low
  tags: [mcp, testing, evals, inspector, ci, quality]
---

# Purpose

Test MCP servers at multiple levels: unit tests for tool logic, integration tests via MCP Inspector, and evaluation suites that verify LLMs can use the tools effectively. This skill covers the full testing pyramid for MCP servers.

# When to use this skill

- Writing tests for MCP tool handlers
- Creating evaluation question sets for an MCP server
- Building CI/CD pipelines that test MCP servers
- Validating tool responses against `outputSchema`
- Pre-release quality verification

# Do not use this skill when

- Debugging a specific failure → use `mcp-inspector-debugging`
- Building the server itself → use `mcp-development`
- Designing tool schemas → use `mcp-tool-design`

# Testing pyramid for MCP servers

```
        /  Evals  \        ← LLM uses tools to answer complex questions
       /  Integration \    ← MCP Inspector verifies protocol compliance
      /   Unit Tests    \  ← Test tool handler logic directly
     /___________________\
```

# Operating procedure

## Level 1 — Unit tests (tool handler logic)

Test tool handlers as regular functions, mocking external dependencies:

**TypeScript:**
```typescript
import { describe, it, expect } from "vitest";

describe("search_items tool", () => {
  it("returns results matching query", async () => {
    const mockApi = { search: async (q) => [{ id: 1, name: "Test" }] };
    const result = await searchItemsHandler({ query: "test", limit: 10 }, mockApi);
    expect(result.content[0].text).toContain("Test");
    expect(result.isError).toBeFalsy();
  });

  it("returns error for invalid query", async () => {
    const result = await searchItemsHandler({ query: "", limit: 10 }, mockApi);
    expect(result.isError).toBe(true);
  });
});
```

**Python:**
```python
import pytest
from server import search_items_handler

async def test_search_returns_results():
    result = await search_items_handler(query="test", limit=10)
    assert "Test" in result
    assert not result.is_error

async def test_search_handles_empty_query():
    with pytest.raises(McpError):
        await search_items_handler(query="", limit=10)
```

**What to test at this level:**
- Happy path: valid inputs → expected output
- Edge cases: empty strings, boundary values, special characters
- Error handling: invalid inputs → appropriate error messages
- Input validation: path traversal, injection attempts
- Pagination: correct cursor handling, last page behavior

## Level 2 — Integration tests (MCP protocol compliance)

Use the MCP Inspector to verify the server speaks correct MCP protocol:

```bash
npx @modelcontextprotocol/inspector node dist/index.js
```

**Checklist for Inspector verification:**
- [ ] `initialize` completes with correct capabilities
- [ ] `tools/list` returns all registered tools with descriptions and schemas
- [ ] `tools/call` for each tool with valid arguments returns expected format
- [ ] `tools/call` with invalid arguments returns `isError: true`
- [ ] `resources/list` (if applicable) returns resources with URIs
- [ ] `resources/read` returns content in correct format
- [ ] `prompts/list` and `prompts/get` (if applicable) work correctly

**Automated integration tests using the SDK client:**

```typescript
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";

const transport = new StdioClientTransport({ command: "node", args: ["dist/index.js"] });
const client = new Client({ name: "test-client", version: "1.0.0" });
await client.connect(transport);

// Verify tools list
const tools = await client.listTools();
assert(tools.tools.length > 0);

// Verify tool call
const result = await client.callTool("search", { query: "test" });
assert(!result.isError);
assert(result.content.length > 0);

await client.close();
```

## Level 3 — Evaluation suite (LLM effectiveness)

Evals test whether an LLM can actually use the MCP server to accomplish real tasks.

### Creating evaluation questions

Write 10 complex questions that require using the MCP tools:

```xml
<evaluation>
  <qa_pair>
    <question>Find all open issues labeled "bug" in the repository and list how many were created in the last 7 days.</question>
    <answer>3</answer>
  </qa_pair>
  <qa_pair>
    <question>What is the total size in bytes of all TypeScript files in the src directory?</question>
    <answer>45230</answer>
  </qa_pair>
</evaluation>
```

**Eval question requirements:**
- Independent (no question depends on another)
- Read-only (only non-destructive operations)
- Complex (requires multiple tool calls)
- Realistic (based on real use cases)
- Verifiable (single, clear answer)
- Stable (answer doesn't change over time)

### Running evals

1. Connect the MCP server to an LLM-powered host
2. Present each question to the LLM
3. Compare the LLM's answer to the expected answer
4. Track pass/fail rate across all questions

## CI/CD pipeline integration

```yaml
# .github/workflows/mcp-test.yml
name: MCP Server Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: "22" }
      - run: npm ci && npm run build
      - run: npm test                        # Unit tests
      - run: |                               # Integration smoke test
          timeout 30 npx @modelcontextprotocol/inspector \
            --cli node dist/index.js \
            --method tools/list || true
```

# Decision rules

- Unit tests are mandatory for all tool handlers
- MCP Inspector verification is mandatory before any release
- Evals are recommended for published/shared servers
- Test error cases as thoroughly as happy paths — LLMs send unexpected inputs
- If a tool wraps an external API, mock the API in unit tests
- If CI times out on Inspector, use `--cli` mode for non-interactive testing

# Output requirements

1. Unit tests for all tool handlers (happy path + error cases)
2. MCP Inspector verification passing for all capabilities
3. Evaluation suite with 5-10 questions (for published servers)
4. CI pipeline configuration (optional but recommended)

# Related skills

- `mcp-inspector-debugging` — when tests reveal failures
- `mcp-tool-design` — designing testable tools
- `mcp-schema-contracts` — schema validation in tests
- `mcp-marketplace-publishing` — pre-publish quality gate

# Failure handling

- If unit tests pass but Inspector fails, the issue is in protocol handling (transport, JSON-RPC), not tool logic
- If Inspector passes but evals fail, the issue is in tool descriptions or response formatting — the LLM can't understand how to use the tools
- If CI timeouts occur, reduce the test scope or increase timeout — MCP Inspector startup can be slow
