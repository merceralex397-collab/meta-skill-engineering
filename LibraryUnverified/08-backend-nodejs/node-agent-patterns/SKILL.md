---
name: node-agent-patterns
description: Encodes Node.js/TypeScript patterns for building AI agents — tool registration, bounded subprocess execution, state management, and safe MCP server transport. Trigger on "Node.js agent", "TypeScript MCP server", "tool registration pattern", "agent subprocess". Do NOT use for mcp-protocol (protocol understanding), external-api-client (API consumption), or prompt-crafting (prompt engineering).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: node-agent-patterns
  maturity: draft
  risk: low
  tags: [node, agent, patterns]
---

# Purpose
Provides Node.js patterns for building agent implementations: tool registration, state management, error handling, subprocess execution, and transport layers. Focuses on practical patterns for building AI agents that run as processes, communicate via MCP/stdio, and safely execute bounded work.

# When to use this skill
Use when:
- Building an MCP server in Node.js/TypeScript
- Creating a tool-using agent with subprocess execution
- Implementing state management for multi-turn agent sessions
- Designing transport layers (stdio, HTTP/SSE) for agent communication

Do NOT use when:
- Using agents, not building them
- Working in Python, Rust, or other languages
- Building simple scripts without agent patterns

# Operating procedure

## 1. Project structure for agent projects
```
src/
├── index.ts           # Entry point, server setup
├── server.ts          # MCP server configuration
├── tools/             # Tool implementations
│   ├── index.ts       # Tool registry
│   ├── file-tools.ts  # File operations
│   └── bash-tool.ts   # Command execution
├── resources/         # Resource providers
├── state/             # State management
│   ├── session.ts     # Session state
│   └── persistence.ts # State persistence
└── utils/
    ├── errors.ts      # Error types
    └── validation.ts  # Input validation
```

## 2. Tool registration pattern
```typescript
// tools/index.ts
import { z } from 'zod';

interface Tool {
  name: string;
  description: string;
  inputSchema: z.ZodType;
  handler: (input: unknown) => Promise<ToolResult>;
}

const toolRegistry = new Map<string, Tool>();

export function registerTool(tool: Tool): void {
  if (toolRegistry.has(tool.name)) {
    throw new Error('Tool already registered: ' + tool.name);
  }
  toolRegistry.set(tool.name, tool);
}

export function getTool(name: string): Tool | undefined {
  return toolRegistry.get(name);
}

export function listTools(): Tool[] {
  return Array.from(toolRegistry.values());
}
```

## 3. Safe subprocess execution
```typescript
// tools/bash-tool.ts
import { spawn } from 'child_process';

interface ExecOptions {
  command: string;
  args?: string[];
  cwd?: string;
  timeout?: number;  // Always set a timeout
  maxBuffer?: number; // Limit output size
}

export async function execBounded(opts: ExecOptions): Promise<{
  stdout: string;
  stderr: string;
  exitCode: number;
}> {
  const timeout = opts.timeout ?? 30000; // Default 30s
  const maxBuffer = opts.maxBuffer ?? 1024 * 1024; // Default 1MB
  
  return new Promise((resolve, reject) => {
    const proc = spawn(opts.command, opts.args ?? [], {
      cwd: opts.cwd,
      shell: true,
      timeout,
    });
    
    let stdout = '';
    let stderr = '';
    
    proc.stdout.on('data', (data) => {
      if (stdout.length + data.length <= maxBuffer) {
        stdout += data;
      }
    });
    
    proc.stderr.on('data', (data) => {
      if (stderr.length + data.length <= maxBuffer) {
        stderr += data;
      }
    });
    
    proc.on('close', (code) => {
      resolve({ stdout, stderr, exitCode: code ?? 0 });
    });
    
    proc.on('error', reject);
  });
}
```

## 4. State management pattern
```typescript
// state/session.ts
interface SessionState {
  id: string;
  created: Date;
  lastActivity: Date;
  context: Map<string, unknown>;
  history: Message[];
}

class SessionManager {
  private sessions = new Map<string, SessionState>();
  
  create(): SessionState {
    const session: SessionState = {
      id: crypto.randomUUID(),
      created: new Date(),
      lastActivity: new Date(),
      context: new Map(),
      history: [],
    };
    this.sessions.set(session.id, session);
    return session;
  }
  
  get(id: string): SessionState | undefined {
    const session = this.sessions.get(id);
    if (session) {
      session.lastActivity = new Date();
    }
    return session;
  }
  
  // Cleanup stale sessions
  prune(maxAge: number = 3600000): void {
    const now = Date.now();
    for (const [id, session] of this.sessions) {
      if (now - session.lastActivity.getTime() > maxAge) {
        this.sessions.delete(id);
      }
    }
  }
}
```

## 5. Error handling pattern
```typescript
// utils/errors.ts
export class ToolError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly recoverable: boolean = false,
  ) {
    super(message);
    this.name = 'ToolError';
  }
}

export class ValidationError extends ToolError {
  constructor(message: string) {
    super(message, 'VALIDATION_ERROR', true);
  }
}

export class TimeoutError extends ToolError {
  constructor(operation: string, timeout: number) {
    super(
      operation + ' timed out after ' + timeout + 'ms',
      'TIMEOUT_ERROR',
      true,
    );
  }
}

// In tool handlers:
export async function handleTool(tool: Tool, input: unknown) {
  try {
    const validated = tool.inputSchema.parse(input);
    return await tool.handler(validated);
  } catch (error) {
    if (error instanceof z.ZodError) {
      throw new ValidationError(error.message);
    }
    if (error instanceof ToolError) {
      throw error;
    }
    throw new ToolError(
      error instanceof Error ? error.message : 'Unknown error',
      'INTERNAL_ERROR',
    );
  }
}
```

## 6. MCP server setup (stdio transport)
```typescript
// server.ts
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';

export async function createServer() {
  const server = new Server(
    { name: 'my-agent', version: '1.0.0' },
    { capabilities: { tools: {} } }
  );
  
  // Register tool handlers
  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: listTools().map(t => ({
      name: t.name,
      description: t.description,
      inputSchema: zodToJsonSchema(t.inputSchema),
    })),
  }));
  
  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const tool = getTool(request.params.name);
    if (!tool) {
      throw new Error('Unknown tool: ' + request.params.name);
    }
    return handleTool(tool, request.params.arguments);
  });
  
  return server;
}

// index.ts
const server = await createServer();
const transport = new StdioServerTransport();
await server.connect(transport);
```

## 7. Input validation pattern
```typescript
// utils/validation.ts
import { z } from 'zod';

// Always validate and sanitize inputs
export const FilePathSchema = z.string()
  .min(1)
  .max(4096)
  .refine(path => !path.includes('..'), 'Path traversal not allowed')
  .refine(path => !path.startsWith('/'), 'Absolute paths not allowed');

export const CommandSchema = z.string()
  .min(1)
  .max(10000)
  .refine(cmd => !cmd.includes('rm -rf /'), 'Dangerous command blocked');
```

# Output defaults
Agent implementations should provide:
- Typed tool definitions with Zod schemas
- Bounded subprocess execution (timeouts, output limits)
- Session state management
- Structured error handling
- Input validation on all external data

# References
- MCP SDK: https://modelcontextprotocol.io/docs/concepts/architecture
- Node.js child_process: https://nodejs.org/api/child_process.html
- Zod validation: https://zod.dev

# Failure handling
- **Subprocess hangs**: Always use timeouts; terminate after limit
- **Memory exhaustion**: Limit output buffers, prune old sessions
- **Uncaught errors**: Use global error handlers, return structured errors
- **State corruption**: Validate state on load, reset on corruption
