---
name: mcp-python-fastmcp
description: "Build MCP servers in Python using the FastMCP high-level API from the official Python SDK. Use when creating MCP tools/resources/prompts in Python, applying FastMCP decorator patterns, configuring Python MCP server transport, or debugging Python-specific MCP issues."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-python-fastmcp
  maturity: stable
  risk: low
  tags: [mcp, python, fastmcp, sdk]
---

# Purpose

Build MCP servers in Python using the FastMCP API from the official `mcp` Python SDK. FastMCP uses Python type hints and docstrings to auto-generate tool schemas, making it the fastest way to create MCP servers in Python.

# When to use this skill

- Building an MCP server in Python
- Using FastMCP decorator patterns for tool/resource/prompt registration
- Configuring Python MCP server for stdio or Streamable HTTP transport
- Debugging Python-specific MCP server issues (logging, async, imports)

# Do not use this skill when

- Building in TypeScript → use `mcp-typescript-sdk`
- Building in Go → use `mcp-go-server`
- Need general MCP development guidance → use `mcp-development`

# Operating procedure

## Phase 1 — Project setup

```bash
uv init my-mcp-server && cd my-mcp-server
uv venv && source .venv/bin/activate
uv add "mcp[cli]"
```

Requirements: Python 3.10+, MCP SDK 1.2.0+

## Phase 2 — Create server with FastMCP

```python
from mcp.server.fastmcp import FastMCP

mcp = FastMCP("my-server")
```

FastMCP auto-generates JSON Schema from Python type hints and docstrings.

### Register tools

```python
@mcp.tool()
async def search_items(query: str, limit: int = 10) -> str:
    """Search for items by keyword.

    Args:
        query: Search query string
        limit: Maximum results to return (default 10)
    """
    results = await do_search(query, limit)
    return json.dumps(results)
```

**How it works:** FastMCP reads the function signature and docstring to generate:
- `inputSchema` from type hints (`query: str` → `{"type": "string"}`)
- `description` from the docstring's first line
- Parameter descriptions from the `Args:` section

### Register resources

```python
@mcp.resource("config://app-settings")
def get_app_settings() -> str:
    """Current application configuration."""
    return json.dumps(load_settings())

# Resource templates (parameterized)
@mcp.resource("file://{path}")
def read_file(path: str) -> str:
    """Read a file by path."""
    return Path(path).read_text()
```

### Register prompts

```python
@mcp.prompt()
def analyze_data(dataset: str) -> str:
    """Analyze a dataset using available tools."""
    return f"Please analyze the dataset '{dataset}' using the search and query tools available."
```

## Phase 3 — Error handling

**Tool execution errors** (business logic failures):
```python
@mcp.tool()
async def delete_item(item_id: str) -> str:
    """Delete an item by ID."""
    try:
        await api.delete(item_id)
        return f"Deleted item {item_id}"
    except NotFoundError:
        # Return error message — FastMCP will set isError: true
        raise McpError(ErrorCode.InvalidParams, f"Item {item_id} not found")
    except PermissionError:
        raise McpError(ErrorCode.InvalidRequest, "Insufficient permissions to delete")
```

**Critical:** Never use `print()` to stdout in stdio mode — it corrupts the JSON-RPC stream:
```python
import sys
import logging

# ✅ Good — stderr
print("Debug info", file=sys.stderr)
logging.info("Processing request")  # Goes to stderr by default

# ❌ Bad — corrupts stdio transport
print("Debug info")
```

## Phase 4 — Transport configuration

**stdio (default, for local servers):**
```python
if __name__ == "__main__":
    mcp.run(transport="stdio")
```

**Streamable HTTP (for remote servers):**
```python
if __name__ == "__main__":
    mcp.run(transport="streamable-http", host="0.0.0.0", port=8000)
```

**For deployment behind a reverse proxy:**
```python
# Get the ASGI app for mounting in an existing web framework
app = mcp.streamable_http_app()
```

## Phase 5 — Advanced patterns

### Pydantic models for complex input/output

```python
from pydantic import BaseModel, Field

class SearchParams(BaseModel):
    query: str = Field(description="Search query")
    filters: dict[str, str] = Field(default={}, description="Key-value filter pairs")
    page: int = Field(default=1, ge=1, description="Page number")

@mcp.tool()
async def advanced_search(params: SearchParams) -> str:
    """Search with advanced filtering and pagination."""
    results = await do_search(params)
    return json.dumps(results)
```

### Context and lifecycle

```python
from mcp.server.fastmcp import Context

@mcp.tool()
async def my_tool(query: str, ctx: Context) -> str:
    """A tool with access to MCP context."""
    await ctx.report_progress(0.5, "Halfway done")
    ctx.info(f"Processing query: {query}")  # Sends log to client
    return "Done"
```

## Phase 6 — Test

```bash
# Verify syntax
python -m py_compile server.py

# Test with MCP Inspector
npx @modelcontextprotocol/inspector uv run python server.py
```

# Decision rules

- Use FastMCP (not low-level `Server` class) unless you need custom protocol handling
- Type hints are mandatory — they generate the inputSchema. No type hints = no schema = broken tool.
- Docstrings are mandatory — they generate the tool description. No docstring = no description = LLM can't discover the tool.
- Use `async` for all tool handlers that do I/O (API calls, database, file reads)
- Use Pydantic models for tools with more than 3 parameters
- Always log to stderr in stdio mode

# Output requirements

1. Python MCP server with FastMCP instance
2. All tools have type hints and docstrings
3. Transport configured (stdio or streamable-http)
4. Passes MCP Inspector verification

# Related skills

- `mcp-typescript-sdk` — TypeScript alternative
- `mcp-go-server` — Go alternative
- `mcp-tool-design` — tool schema design patterns
- `mcp-inspector-debugging` — debugging Python MCP servers

# Failure handling

- If `uv add "mcp[cli]"` fails, try `pip install "mcp[cli]"` as fallback
- If Inspector shows no tools, verify `@mcp.tool()` decorator is present (not just `@mcp.tool`)
- If type errors occur, ensure Python 3.10+ for `X | Y` union syntax; use `Optional[X]` on 3.9
- If `async` errors occur, ensure tool handlers are `async def` when calling async APIs
