---
name: realtime-websocket
description: >-
  Implement WebSocket servers and clients with correct connection lifecycle, heartbeat/ping-pong,
  room/channel patterns, authentication on upgrade, reconnection with exponential backoff,
  and message serialization. Covers ws (Node.js), websockets (Python), gorilla/websocket (Go),
  and Socket.IO as a higher-level abstraction.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: realtime-websocket
  maturity: draft
  risk: medium
  tags: [websocket, realtime, ws, socket-io, connection-management]
---

# Purpose

Implement WebSocket servers and clients with correct connection lifecycle management, heartbeat/ping-pong for liveness detection, room/channel pub-sub patterns, authentication during the HTTP upgrade, reconnection with exponential backoff, message serialization (JSON, MessagePack, Protocol Buffers), and backpressure handling. This skill covers the `ws` library (Node.js), `websockets` (Python), `gorilla/websocket` (Go), and Socket.IO as a higher-level abstraction.

# When to use this skill

Use this skill when:

- implementing a WebSocket server with `ws`, `websockets`, `gorilla/websocket`, or Socket.IO
- building a WebSocket client with reconnection logic
- adding heartbeat/ping-pong to detect dead connections
- implementing room/channel join/leave patterns for pub-sub
- authenticating WebSocket connections during the HTTP upgrade handshake
- debugging connection drops, zombie connections, or message ordering issues
- scaling WebSocket servers with Redis pub/sub or sticky sessions

# Do not use this skill when

- using Firebase Firestore real-time listeners (onSnapshot) — use `firebase-sdk` instead
- implementing webhook endpoints (HTTP-based, not persistent connections) — use `webhooks-events` instead
- working on REST API request/response debugging — use `api-debugging` instead
- the task is about Server-Sent Events (SSE) — partial overlap, but SSE is unidirectional

# Operating procedure

1. **Choose the WebSocket library for your runtime.**

   | Runtime | Library | Install |
   |---------|---------|---------|
   | Node.js | `ws` | `npm install ws` |
   | Node.js (high-level) | `socket.io` | `npm install socket.io` |
   | Python | `websockets` | `pip install websockets` |
   | Go | `gorilla/websocket` | `go get github.com/gorilla/websocket` |
   | Browser | Native `WebSocket` API | Built-in |

2. **Implement the server with connection lifecycle handlers.**

   Node.js with `ws`:
   ```javascript
   const { WebSocketServer } = require('ws');
   const wss = new WebSocketServer({ port: 8080 });

   wss.on('connection', (ws, req) => {
     console.log('Client connected from', req.socket.remoteAddress);

     ws.on('message', (data, isBinary) => {
       const message = isBinary ? data : data.toString();
       // Route message by type
       const parsed = JSON.parse(message);
       handleMessage(ws, parsed);
     });

     ws.on('close', (code, reason) => {
       console.log(`Client disconnected: ${code} ${reason}`);
       cleanupConnection(ws);
     });

     ws.on('error', (err) => {
       console.error('WebSocket error:', err);
       cleanupConnection(ws);
     });
   });
   ```

3. **Add heartbeat/ping-pong.**

   Server-side (Node.js `ws`):
   ```javascript
   function startHeartbeat(wss) {
     const interval = setInterval(() => {
       wss.clients.forEach((ws) => {
         if (ws.isAlive === false) {
           console.log('Terminating dead connection');
           return ws.terminate();
         }
         ws.isAlive = false;
         ws.ping();
       });
     }, 30000);  // Every 30 seconds

     wss.on('close', () => clearInterval(interval));
   }

   wss.on('connection', (ws) => {
     ws.isAlive = true;
     ws.on('pong', () => { ws.isAlive = true; });
   });

   startHeartbeat(wss);
   ```

4. **Authenticate on the HTTP upgrade.**

   ```javascript
   const server = require('http').createServer();
   const wss = new WebSocketServer({ noServer: true });

   server.on('upgrade', (request, socket, head) => {
     // Extract token from query string or header
     const url = new URL(request.url, 'http://localhost');
     const token = url.searchParams.get('token');

     if (!token || !verifyToken(token)) {
       socket.write('HTTP/1.1 401 Unauthorized\r\n\r\n');
       socket.destroy();
       return;
     }

     wss.handleUpgrade(request, socket, head, (ws) => {
       ws.userId = decodeToken(token).userId;
       wss.emit('connection', ws, request);
     });
   });

   server.listen(8080);
   ```

5. **Implement room/channel patterns.**

   ```javascript
   class RoomManager {
     constructor() {
       this.rooms = new Map();  // roomId -> Set<ws>
     }

     join(roomId, ws) {
       if (!this.rooms.has(roomId)) this.rooms.set(roomId, new Set());
       this.rooms.get(roomId).add(ws);
       ws.rooms = ws.rooms || new Set();
       ws.rooms.add(roomId);
     }

     leave(roomId, ws) {
       this.rooms.get(roomId)?.delete(ws);
       ws.rooms?.delete(roomId);
       if (this.rooms.get(roomId)?.size === 0) this.rooms.delete(roomId);
     }

     broadcast(roomId, data, exclude = null) {
       const members = this.rooms.get(roomId);
       if (!members) return;
       const payload = typeof data === 'string' ? data : JSON.stringify(data);
       for (const ws of members) {
         if (ws !== exclude && ws.readyState === ws.OPEN) {
           ws.send(payload);
         }
       }
     }

     disconnectAll(ws) {
       if (ws.rooms) {
         for (const roomId of ws.rooms) this.leave(roomId, ws);
       }
     }
   }
   ```

6. **Implement client reconnection with exponential backoff.**

   ```javascript
   class ReconnectingWebSocket {
     constructor(url, options = {}) {
       this.url = url;
       this.maxRetries = options.maxRetries || 10;
       this.baseDelay = options.baseDelay || 1000;
       this.maxDelay = options.maxDelay || 30000;
       this.retries = 0;
       this.handlers = { message: [], open: [], close: [] };
       this.connect();
     }

     connect() {
       this.ws = new WebSocket(this.url);
       this.ws.onopen = () => {
         this.retries = 0;
         this.handlers.open.forEach(h => h());
       };
       this.ws.onmessage = (event) => {
         this.handlers.message.forEach(h => h(event));
       };
       this.ws.onclose = (event) => {
         this.handlers.close.forEach(h => h(event));
         if (event.code !== 1000 && this.retries < this.maxRetries) {
           const delay = Math.min(this.baseDelay * 2 ** this.retries, this.maxDelay);
           const jitter = delay * (0.5 + Math.random() * 0.5);
           setTimeout(() => this.connect(), jitter);
           this.retries++;
         }
       };
     }

     send(data) {
       if (this.ws.readyState === WebSocket.OPEN) {
         this.ws.send(typeof data === 'string' ? data : JSON.stringify(data));
       }
     }

     on(event, handler) { this.handlers[event].push(handler); }

     close() { this.ws.close(1000, 'Client closing'); }
   }
   ```

7. **Handle message routing by type.**

   ```javascript
   const messageHandlers = {
     'chat:send': handleChatMessage,
     'room:join': handleRoomJoin,
     'room:leave': handleRoomLeave,
     'typing:start': handleTypingStart,
     'typing:stop': handleTypingStop,
   };

   function handleMessage(ws, message) {
     const handler = messageHandlers[message.type];
     if (!handler) {
       ws.send(JSON.stringify({ type: 'error', message: 'Unknown message type' }));
       return;
     }
     handler(ws, message.payload);
   }
   ```

8. **For scaling: add Redis pub/sub for multi-server fanout.**

   ```javascript
   const Redis = require('ioredis');
   const pub = new Redis();
   const sub = new Redis();

   sub.subscribe('chat');
   sub.on('message', (channel, message) => {
     // Broadcast to all local WebSocket clients in the room
     rooms.broadcast(JSON.parse(message).roomId, message);
   });

   // When a local client sends a message, publish to Redis
   function handleChatMessage(ws, payload) {
     pub.publish('chat', JSON.stringify({ roomId: payload.roomId, ...payload }));
   }
   ```

9. **Graceful shutdown.**

   ```javascript
   process.on('SIGTERM', () => {
     wss.clients.forEach((ws) => {
       ws.close(1001, 'Server shutting down');
     });
     wss.close(() => {
       server.close(() => process.exit(0));
     });
   });
   ```

# Decision rules

- Use `ws` for Node.js when you need raw WebSocket control. Use Socket.IO when you need built-in rooms, namespaces, automatic reconnection, and fallback transports.
- Always implement ping/pong heartbeat. Without it, dead connections accumulate and consume resources.
- Authenticate during the HTTP upgrade, not after the WebSocket is open. This prevents unauthorized connections from consuming server resources.
- Use JSON for human-readable messages. Use MessagePack or Protocol Buffers for high-throughput binary data.
- Close connections with appropriate close codes: 1000 (normal), 1001 (going away), 1008 (policy violation), 1011 (server error).
- For horizontal scaling, use Redis pub/sub or a message broker (NATS, RabbitMQ) to fan out messages across server instances. Use sticky sessions at the load balancer if state is per-connection.
- Set `maxPayload` on the server to prevent oversized messages from crashing the process (`new WebSocketServer({ maxPayload: 1024 * 1024 })`).
- Always check `ws.readyState === WebSocket.OPEN` before sending to avoid errors on closing connections.

# Output requirements

1. `Server Implementation` — WebSocket server with lifecycle handlers and heartbeat
2. `Client Implementation` — client with reconnection logic
3. `Message Protocol` — documented message types and payloads
4. `Scaling Strategy` — how multi-server fanout works (Redis, sticky sessions, etc.)

# References

Read these when working on specific aspects:

- `references/implementation-patterns.md` — connection management, rooms, heartbeat, auth
- `references/validation-checklist.md` — production readiness checks
- `references/failure-modes.md` — connection issues, leaks, scaling problems

# Related skills

- `firebase-sdk` — Firebase real-time listeners as an alternative to raw WebSockets
- `rate-limits-retries` — rate limiting WebSocket connections and messages
- `webhooks-events` — HTTP-based event delivery (compare with WebSocket push)
- `api-debugging` — debugging WebSocket connection issues at the network level
- `api-contracts` — message schema design for WebSocket protocols

# Anti-patterns

- **No heartbeat.** Without ping/pong, dead connections (client crashed, network dropped) stay open indefinitely, consuming memory and file descriptors. Always implement heartbeat with a 30-60 second interval.
- **Authentication after connection.** Accepting the WebSocket connection first and then asking for auth allows unauthorized clients to consume server resources. Authenticate during the HTTP upgrade.
- **Broadcasting to all clients in a loop without readyState check.** Sending to a closing/closed connection throws errors. Always check `ws.readyState === WebSocket.OPEN`.
- **Unbounded message queues.** Queueing messages for slow clients without limits causes memory exhaustion. Implement backpressure: check `ws.bufferedAmount` and drop or disconnect slow clients.
- **Reconnecting without backoff.** If the server goes down and 10,000 clients all reconnect instantly, they create a thundering herd. Always use exponential backoff with jitter.
- **Storing state only in memory.** WebSocket connections are ephemeral. If the server restarts, all state is lost. Persist important state to a database and use connections only as delivery channels.
- **Not cleaning up on disconnect.** Failing to remove disconnected clients from rooms and data structures causes memory leaks and ghost broadcasts.

# Failure handling

- **Connection leak (no cleanup on error/close).** If the `close` or `error` handler doesn't clean up (remove from rooms, release resources), connections accumulate. Always call cleanup in both `close` and `error` handlers.
- **Zombie connections from missing heartbeat.** TCP half-open connections persist when the remote side crashes. Ping/pong at 30-second intervals detects these within one interval.
- **Thundering herd on reconnect.** When a server restarts, all clients reconnect simultaneously. Use jittered exponential backoff on clients to spread reconnections over time.
- **Message ordering issues.** WebSocket guarantees in-order delivery on a single connection, but if you use multiple connections or Redis pub/sub, messages may arrive out of order. Use sequence numbers or timestamps.
- **Proxy/load balancer killing idle connections.** Many load balancers (ALB, nginx) have idle timeouts (60-120 seconds). Without heartbeat traffic, they close idle connections. Configure proxy timeouts and use ping/pong.
- **Memory exhaustion from message buffering.** If a client is slow and `ws.bufferedAmount` grows unbounded, the server runs out of memory. Monitor buffered amount and terminate slow clients.
- **Cross-origin WebSocket blocked.** Browsers enforce origin checks. The server must validate the `Origin` header during upgrade if it wants to restrict connections.
