# Implementation Patterns — Realtime WebSocket

## Connection manager pattern

Centralize connection tracking for broadcast, room management, and cleanup.

```javascript
class ConnectionManager {
  constructor() {
    this.connections = new Map();  // id -> { ws, userId, rooms, connectedAt }
    this.userConnections = new Map();  // userId -> Set<id>
  }

  add(id, ws, userId) {
    this.connections.set(id, {
      ws,
      userId,
      rooms: new Set(),
      connectedAt: Date.now(),
    });
    if (!this.userConnections.has(userId)) {
      this.userConnections.set(userId, new Set());
    }
    this.userConnections.get(userId).add(id);
  }

  remove(id) {
    const conn = this.connections.get(id);
    if (!conn) return;
    // Remove from all rooms
    for (const room of conn.rooms) {
      this.leaveRoom(id, room);
    }
    // Remove from user tracking
    const userConns = this.userConnections.get(conn.userId);
    if (userConns) {
      userConns.delete(id);
      if (userConns.size === 0) this.userConnections.delete(conn.userId);
    }
    this.connections.delete(id);
  }

  get(id) { return this.connections.get(id); }
  getByUser(userId) { return this.userConnections.get(userId) || new Set(); }
  count() { return this.connections.size; }
}
```

## Room/channel join and leave

```javascript
// Extending ConnectionManager
joinRoom(connId, roomId) {
  const conn = this.connections.get(connId);
  if (!conn) return;
  conn.rooms.add(roomId);
  if (!this.rooms) this.rooms = new Map();
  if (!this.rooms.has(roomId)) this.rooms.set(roomId, new Set());
  this.rooms.get(roomId).add(connId);
}

leaveRoom(connId, roomId) {
  const conn = this.connections.get(connId);
  if (conn) conn.rooms.delete(roomId);
  const room = this.rooms?.get(roomId);
  if (room) {
    room.delete(connId);
    if (room.size === 0) this.rooms.delete(roomId);
  }
}

broadcastToRoom(roomId, data, excludeConnId = null) {
  const room = this.rooms?.get(roomId);
  if (!room) return;
  const payload = typeof data === 'string' ? data : JSON.stringify(data);
  for (const connId of room) {
    if (connId === excludeConnId) continue;
    const conn = this.connections.get(connId);
    if (conn && conn.ws.readyState === 1) {  // WebSocket.OPEN = 1
      conn.ws.send(payload);
    }
  }
}
```

## Heartbeat implementation

### Server-side (Node.js ws)

```javascript
const HEARTBEAT_INTERVAL = 30000;  // 30 seconds
const HEARTBEAT_TIMEOUT = 10000;   // 10 seconds to respond

function setupHeartbeat(wss) {
  const interval = setInterval(() => {
    for (const [id, conn] of connectionManager.connections) {
      if (conn.lastPong && Date.now() - conn.lastPong > HEARTBEAT_INTERVAL + HEARTBEAT_TIMEOUT) {
        console.log(`Terminating unresponsive connection ${id}`);
        conn.ws.terminate();
        connectionManager.remove(id);
        continue;
      }
      if (conn.ws.readyState === 1) {
        conn.ws.ping();
      }
    }
  }, HEARTBEAT_INTERVAL);

  wss.on('close', () => clearInterval(interval));
  return interval;
}

// On connection setup:
ws.on('pong', () => {
  const conn = connectionManager.get(connId);
  if (conn) conn.lastPong = Date.now();
});
```

### Client-side (browser application-level ping)

Browser WebSocket API does not expose ping/pong frames. Use application-level heartbeat:

```javascript
class HeartbeatClient {
  constructor(ws, interval = 30000) {
    this.ws = ws;
    this.interval = interval;
    this.timeout = null;
    this.pingInterval = null;
  }

  start() {
    this.pingInterval = setInterval(() => {
      if (this.ws.readyState === WebSocket.OPEN) {
        this.ws.send(JSON.stringify({ type: 'ping' }));
        this.timeout = setTimeout(() => {
          console.warn('Server unresponsive, closing connection');
          this.ws.close(4000, 'Heartbeat timeout');
        }, 5000);
      }
    }, this.interval);
  }

  handlePong() {
    if (this.timeout) { clearTimeout(this.timeout); this.timeout = null; }
  }

  stop() {
    if (this.pingInterval) clearInterval(this.pingInterval);
    if (this.timeout) clearTimeout(this.timeout);
  }
}
```

## Message routing by type

Define a message protocol with type-based routing:

```javascript
// Protocol definition
const MessageType = {
  CHAT_SEND: 'chat:send',
  CHAT_HISTORY: 'chat:history',
  ROOM_JOIN: 'room:join',
  ROOM_LEAVE: 'room:leave',
  PRESENCE_UPDATE: 'presence:update',
  TYPING_START: 'typing:start',
  TYPING_STOP: 'typing:stop',
  ERROR: 'error',
  PING: 'ping',
  PONG: 'pong',
};

// Message envelope
function createMessage(type, payload, metadata = {}) {
  return JSON.stringify({
    type,
    payload,
    timestamp: Date.now(),
    id: crypto.randomUUID(),
    ...metadata,
  });
}

// Server-side router
const handlers = new Map();
handlers.set(MessageType.CHAT_SEND, handleChatSend);
handlers.set(MessageType.ROOM_JOIN, handleRoomJoin);
handlers.set(MessageType.ROOM_LEAVE, handleRoomLeave);
handlers.set(MessageType.PING, (ws) => ws.send(createMessage(MessageType.PONG, {})));

function routeMessage(ws, rawData) {
  try {
    const msg = JSON.parse(rawData);
    const handler = handlers.get(msg.type);
    if (!handler) {
      ws.send(createMessage(MessageType.ERROR, { message: `Unknown type: ${msg.type}` }));
      return;
    }
    handler(ws, msg.payload, msg);
  } catch (err) {
    ws.send(createMessage(MessageType.ERROR, { message: 'Invalid message format' }));
  }
}
```

## Authentication middleware on upgrade

### JWT verification during HTTP upgrade

```javascript
const jwt = require('jsonwebtoken');
const { WebSocketServer } = require('ws');

const wss = new WebSocketServer({ noServer: true });

server.on('upgrade', async (request, socket, head) => {
  try {
    // Token from Authorization header or query parameter
    const token = request.headers['authorization']?.replace('Bearer ', '')
      || new URL(request.url, 'http://localhost').searchParams.get('token');

    if (!token) {
      socket.write('HTTP/1.1 401 Unauthorized\r\n\r\n');
      socket.destroy();
      return;
    }

    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    request.userId = decoded.userId;
    request.roles = decoded.roles || [];

    wss.handleUpgrade(request, socket, head, (ws) => {
      wss.emit('connection', ws, request);
    });
  } catch (err) {
    socket.write('HTTP/1.1 401 Unauthorized\r\n\r\n');
    socket.destroy();
  }
});
```

## Binary vs text frame selection

```javascript
// Text frames — for JSON messages
ws.send(JSON.stringify({ type: 'chat', text: 'hello' }));

// Binary frames — for binary data (images, audio, encoded messages)
const msgpack = require('msgpack-lite');
ws.send(msgpack.encode({ type: 'audio', data: audioBuffer }), { binary: true });

// Receiving — check isBinary flag
ws.on('message', (data, isBinary) => {
  if (isBinary) {
    const decoded = msgpack.decode(data);
    handleBinaryMessage(decoded);
  } else {
    const parsed = JSON.parse(data.toString());
    handleTextMessage(parsed);
  }
});
```

## Connection state machine

```
  CONNECTING ──→ OPEN ──→ CLOSING ──→ CLOSED
       │           │          ↑
       │           │          │
       └─→ CLOSED  └──────────┘
                   (error)
```

States map to `WebSocket.readyState`:
- `0` CONNECTING — connection not yet open
- `1` OPEN — ready to communicate
- `2` CLOSING — close frame sent, waiting for response
- `3` CLOSED — connection is closed

Always check `readyState === 1` before `send()`.

## Graceful shutdown with close frame

```javascript
async function gracefulShutdown(wss, connectionManager) {
  console.log('Shutting down WebSocket server...');

  // Send close frame to all connected clients
  const closePromises = [];
  for (const [id, conn] of connectionManager.connections) {
    if (conn.ws.readyState === 1) {
      closePromises.push(new Promise((resolve) => {
        conn.ws.on('close', resolve);
        conn.ws.close(1001, 'Server shutting down');
        // Force terminate after 5 seconds if client doesn't respond
        setTimeout(() => { conn.ws.terminate(); resolve(); }, 5000);
      }));
    }
  }

  await Promise.allSettled(closePromises);
  wss.close();
  console.log('WebSocket server shut down');
}

process.on('SIGTERM', () => gracefulShutdown(wss, connectionManager));
process.on('SIGINT', () => gracefulShutdown(wss, connectionManager));
```
