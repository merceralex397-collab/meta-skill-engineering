# Validation Checklist — Realtime WebSocket

Use this checklist before deploying WebSocket services.

## Connection lifecycle

- [ ] Server handles `connection`, `message`, `close`, and `error` events
- [ ] `close` and `error` handlers both trigger connection cleanup (remove from rooms, release resources)
- [ ] Connection ID is assigned on connect and used for tracking
- [ ] Maximum concurrent connections per server is configured and monitored
- [ ] Maximum connections per user is enforced (prevent single user from opening thousands)
- [ ] Graceful shutdown sends close frame (code 1001) before terminating

## Heartbeat and liveness

- [ ] Server sends WebSocket ping frames at regular intervals (30-60 seconds)
- [ ] Server tracks last pong timestamp per connection
- [ ] Connections that miss heartbeat are terminated (not just closed — `ws.terminate()`)
- [ ] Client implements application-level ping if using browser WebSocket API (no native pong access)
- [ ] Heartbeat interval is shorter than proxy/load balancer idle timeout

## Authentication and authorization

- [ ] Authentication happens during HTTP upgrade, before WebSocket connection is established
- [ ] Token (JWT, session) is validated before calling `wss.handleUpgrade`
- [ ] Invalid/expired tokens result in `401 Unauthorized` response and `socket.destroy()`
- [ ] User identity is attached to the WebSocket connection object after auth
- [ ] Authorization is checked on sensitive message types (e.g., room admin actions)

## Message handling

- [ ] Message size limit is enforced (`maxPayload` option in ws, or manual check)
- [ ] Messages are parsed in try/catch — malformed messages don't crash the handler
- [ ] Unknown message types return an error response, not a crash
- [ ] Message protocol is documented (types, payload shapes, required fields)
- [ ] Binary and text frames are handled correctly based on `isBinary` flag

## Close codes

- [ ] `1000` used for normal closure (client intentionally disconnecting)
- [ ] `1001` used for server going away (shutdown, restart)
- [ ] `1008` used for policy violation (auth failure, banned user)
- [ ] `1009` used for message too large
- [ ] `1011` used for unexpected server error
- [ ] Custom codes (4000-4999) documented if used

## Rooms and broadcasting

- [ ] Clients are removed from all rooms on disconnect
- [ ] Broadcasting checks `readyState === OPEN` before sending
- [ ] Room membership is validated (user is authorized to join the room)
- [ ] Empty rooms are cleaned up (removed from memory)
- [ ] Room size limits are enforced if applicable

## Reconnection (client-side)

- [ ] Exponential backoff with jitter is implemented
- [ ] Maximum retry count or maximum delay is configured
- [ ] Reconnection does not happen for intentional close (code 1000)
- [ ] State is re-synced after reconnection (re-join rooms, fetch missed messages)
- [ ] Reconnection resets backoff counter on successful connection

## Scaling

- [ ] Multi-server deployment uses Redis pub/sub or message broker for cross-server broadcast
- [ ] Sticky sessions configured at load balancer (or connection state externalized)
- [ ] Load balancer WebSocket upgrade is configured (e.g., nginx `proxy_set_header Upgrade`)
- [ ] Load balancer idle timeout exceeds heartbeat interval
- [ ] Connection count monitoring and alerting is in place

## Backpressure

- [ ] `ws.bufferedAmount` is monitored for slow clients
- [ ] Slow clients are warned or disconnected when buffer exceeds threshold
- [ ] Server-side message queues are bounded (not unbounded arrays)
- [ ] Send rate limiting is considered for high-frequency message types
