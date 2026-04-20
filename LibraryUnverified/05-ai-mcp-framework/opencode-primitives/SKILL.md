---
name: opencode-primitives
description: "Reference for OpenCode's file paths, naming conventions, and configuration primitives that other MCP skills depend on. Use when needing exact paths for OpenCode config files, understanding OpenCode's agent/skill/tool directory structure, or setting up an OpenCode project with MCP support."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: opencode-primitives
  maturity: stable
  risk: low
  tags: [opencode, primitives, paths, config, conventions]
---

# Purpose

Authoritative reference for OpenCode's directory structure, configuration file locations, naming conventions, and the primitives that other MCP skills assume when referencing OpenCode. This is a lookup skill — it provides exact paths and formats rather than procedures.

# When to use this skill

- Need exact paths for OpenCode configuration files
- Setting up a project directory for OpenCode with MCP support
- Need to understand OpenCode's agent/skill/command/tool directory structure
- Other skills reference "OpenCode conventions" and you need the specifics

# Do not use this skill when

- Connecting an MCP server to OpenCode → use `opencode-bridge`
- Building an MCP server → use `mcp-development`
- General MCP onboarding → use `get-started`

# OpenCode directory structure

```
project-root/
├── .opencode/
│   ├── config.json          # Project-level OpenCode configuration
│   ├── agents/              # Custom agent definitions
│   │   └── my-agent.md
│   ├── commands/            # Custom slash commands
│   │   └── my-command.md
│   ├── skills/              # Project-local skills
│   │   └── my-skill/
│   │       └── SKILL.md
│   └── tools/               # Custom tool definitions
│       └── my-tool.md
```

# Configuration file: `.opencode/config.json`

Primary project configuration for OpenCode:

```json
{
  "mcpServers": {
    "server-name": {
      "command": "node",
      "args": ["/absolute/path/to/server.js"],
      "env": {
        "API_KEY": "value"
      }
    }
  }
}
```

**MCP server config fields:**
- `command` — executable to launch (e.g., `node`, `python`, `uv`)
- `args` — array of arguments (use absolute paths)
- `env` — environment variables passed to the server process
- `url` — for remote Streamable HTTP servers (alternative to `command`/`args`)
- `headers` — HTTP headers for remote servers

# Naming conventions

| Entity | Convention | Example |
|--------|-----------|---------|
| Agent files | `kebab-case.md` | `code-reviewer.md` |
| Skill directories | `kebab-case/` | `my-skill/` |
| Skill entry point | `SKILL.md` | `my-skill/SKILL.md` |
| Command files | `kebab-case.md` | `deploy-staging.md` |
| Tool files | `kebab-case.md` | `run-tests.md` |
| MCP server names | `kebab-case` | `my-mcp-server` |
| Config key | `camelCase` | `mcpServers` |

# Key paths

| Path | Purpose |
|------|---------|
| `.opencode/config.json` | Project MCP server config and settings |
| `.opencode/agents/` | Custom agent definitions |
| `.opencode/skills/` | Project-local skill packages |
| `.opencode/commands/` | Custom slash commands |
| `.opencode/tools/` | Custom tool definitions |
| `AGENTS.md` | Top-level agent instructions (project root) |

# MCP integration points

OpenCode connects to MCP servers at startup based on `.opencode/config.json`. The integration follows standard MCP protocol:

1. **Launch:** OpenCode starts each configured server as a subprocess (stdio) or connects via URL (HTTP)
2. **Initialize:** Standard MCP `initialize` handshake with capability negotiation
3. **Discover:** `tools/list` to discover available tools
4. **Use:** Agent calls tools during task execution via `tools/call`
5. **Shutdown:** Clean disconnect when OpenCode exits

# Decision rules

- All paths in `config.json` should be absolute to avoid working-directory issues
- MCP server names in config should be unique kebab-case identifiers
- Skills that reference OpenCode should use the exact paths listed here
- Prefer stdio transport for local MCP servers with OpenCode

# Output requirements

This is a reference skill. It produces no artifacts directly — it provides lookup information for other skills and procedures.

# Related skills

- `opencode-bridge` — connecting MCP servers to OpenCode
- `mcp-host-integration` — general host configuration patterns
- `get-started` — initial MCP setup including OpenCode

# Failure handling

- If config file is not found, OpenCode runs without MCP servers — no error, just no tools
- If a server fails to start, OpenCode logs the error and continues with other servers
- If paths are relative, they resolve from OpenCode's working directory (usually project root) — but absolute is safer
