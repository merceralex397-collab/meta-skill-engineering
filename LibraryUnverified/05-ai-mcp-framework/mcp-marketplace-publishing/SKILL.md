---
name: mcp-marketplace-publishing
description: "Package and publish MCP servers for distribution via npm, PyPI, Docker, or MCP registries. Use when preparing an MCP server for public or organizational distribution, writing installation docs, or listing on the MCP server registry at registry.modelcontextprotocol.io."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: mcp-marketplace-publishing
  maturity: stable
  risk: low
  tags: [mcp, publishing, registry, npm, pypi, distribution]
---

# Purpose

Package an MCP server for distribution so others can install and configure it. Covers npm/PyPI publishing, Docker packaging, MCP registry listing, and writing the installation documentation that hosts need.

# When to use this skill

- Publishing an MCP server to npm or PyPI
- Creating a Docker image for a remote MCP server
- Listing a server on the MCP registry (registry.modelcontextprotocol.io)
- Writing installation and configuration documentation for end users
- Packaging a server as a standalone binary

# Do not use this skill when

- Building the server itself → use `mcp-development` or `mcp-builder`
- Deciding transport/auth → use `mcp-auth-transports`
- The server is project-internal with no distribution need

# Operating procedure

## Phase 1 — Package for distribution

### npm (TypeScript servers)

1. Set `"bin"` in `package.json` pointing to the compiled entry point:
```json
{
  "name": "@yourorg/mcp-server-myservice",
  "version": "1.0.0",
  "bin": { "mcp-server-myservice": "dist/index.js" },
  "files": ["dist"],
  "engines": { "node": ">=18" }
}
```

2. Add shebang to entry file: `#!/usr/bin/env node`
3. Build and publish: `npm run build && npm publish`
4. Users install with: `npx @yourorg/mcp-server-myservice` or `npm install -g @yourorg/mcp-server-myservice`

**Naming convention:** `mcp-server-<service>` or `@org/mcp-server-<service>`

### PyPI (Python servers)

1. Use `pyproject.toml` with entry points:
```toml
[project.scripts]
mcp-server-myservice = "myservice.server:main"

[project]
name = "mcp-server-myservice"
dependencies = ["mcp>=1.2.0"]
```

2. Build and publish: `uv build && uv publish` (or `pip install build && python -m build && twine upload dist/*`)

### Docker (remote servers)

```dockerfile
FROM node:22-slim
WORKDIR /app
COPY package*.json ./
RUN npm ci --production
COPY dist/ ./dist/
EXPOSE 3000
CMD ["node", "dist/index.js"]
```

## Phase 2 — Write installation docs

The README must include host-specific configuration snippets. Users need copy-paste configs:

```markdown
## Installation

### Claude Desktop
Add to `~/Library/Application Support/Claude/claude_desktop_config.json`:
\```json
{
  "mcpServers": {
    "myservice": {
      "command": "npx",
      "args": ["@yourorg/mcp-server-myservice"],
      "env": { "API_KEY": "your-key" }
    }
  }
}
\```

### VS Code / Copilot
Add to `.vscode/mcp.json`:
\```json
{
  "servers": {
    "myservice": {
      "command": "npx",
      "args": ["@yourorg/mcp-server-myservice"],
      "env": { "API_KEY": "your-key" }
    }
  }
}
\```
```

## Phase 3 — Register on MCP registry

The MCP registry at `registry.modelcontextprotocol.io` is a directory of MCP servers. To list your server:

1. Ensure the server repo is public on GitHub
2. Include a well-structured README with: purpose, tools list, install instructions, host configs
3. Submit via the registry's contribution process (typically a PR to the registry repo)

## Phase 4 — Quality checklist

Before publishing, verify:
- [ ] All tools have clear `description` and `inputSchema`
- [ ] `tools/list` returns complete tool definitions in MCP Inspector
- [ ] All tools handle errors gracefully (no unhandled exceptions reaching the client)
- [ ] Tool annotations (`readOnlyHint`, `destructiveHint`) are set correctly
- [ ] README includes install command, host configs, and required environment variables
- [ ] `package.json`/`pyproject.toml` has correct entry point and dependencies
- [ ] No hardcoded paths or machine-specific configuration

# Decision rules

- Use `npx` in host configs so users don't need global install
- For Python, prefer `uvx` or `pipx` over `pip install -g`
- Include all required env vars in the documentation with descriptions
- If the server needs API keys, document where to get them
- Semantic versioning: breaking changes to tool schemas = major version bump

# Output requirements

1. Publishable package (npm tarball, PyPI wheel, or Docker image)
2. README with tool list, install command, and host config snippets for at least 2 hosts
3. MCP Inspector verification passing
4. Published to registry (npm/PyPI and optionally MCP registry)

# Related skills

- `mcp-development` — server development lifecycle
- `mcp-host-integration` — host-specific configuration patterns
- `mcp-schema-contracts` — schema versioning for published servers
- `mcp-testing-evals` — pre-publish quality verification

# Failure handling

- If `npx` fails to run the package, verify the `bin` field in `package.json` and the shebang line
- If users report "tool not found", check that `tools` capability is declared in `initialize` response
- If the server works locally but fails for users, check env var documentation — missing API keys are the #1 issue
