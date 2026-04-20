---
name: multiplayer-netcode
description: Implements multiplayer networking with client-server authority, state synchronization, lag compensation, and prediction across Unity Netcode, UE5 Replication, and Godot MultiplayerAPI. Use when building networked gameplay, syncing game state, adding prediction/rollback, or designing lobby systems. Do not use for single-player networking or REST API work.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: multiplayer-netcode
  maturity: draft
  risk: low
  tags: [multiplayer, netcode, networking, replication, prediction]
---

# Purpose

Provides architecture patterns and engine-specific APIs for multiplayer game networking: authority models, state replication, lag compensation, prediction, and matchmaking.

# When to use this skill

- Implementing client-server or peer-to-peer multiplayer architecture
- Synchronizing game state (positions, health, inventory) across networked players
- Adding client-side prediction, server reconciliation, or rollback netcode
- Building lobby, matchmaking, or session management systems
- Optimizing bandwidth with delta compression, priority-based sync, or tick rate tuning

# Do not use this skill when

- The networking task is a REST API or web service — use backend/API skills instead
- The task is about single-player game logic with no network component
- Input handling has no multiplayer aspect — prefer `input-mapping-controller`

# Operating procedure

1. **Choose an authority model.** Client-server authoritative is the default for competitive games — the server owns game state and validates all inputs. Peer-to-peer suits co-op or small lobbies but is vulnerable to cheating. Relay servers add NAT traversal without full authority.
2. **Implement state synchronization.**
   - **Unity Netcode for GameObjects:** Extend `NetworkBehaviour`. Use `NetworkVariable<T>` for synced state. Mark server logic with `[ServerRpc]` and client callbacks with `[ClientRpc]`. Spawn networked objects via `NetworkObject.Spawn()`.
   - **UE5 Replication:** Mark properties with `UPROPERTY(Replicated)` and implement `GetLifetimeReplicatedProps()`. Use `UFUNCTION(Server, Reliable)` for client-to-server calls, `UFUNCTION(Client, Reliable)` for server-to-client. Set `bReplicates = true` on Actors.
   - **Godot MultiplayerAPI:** Set `multiplayer_peer` on the scene tree. Use `@rpc("authority")` or `@rpc("any_peer")` annotations. Check `multiplayer.is_server()` for authority. Use `set_multiplayer_authority()` for ownership.
3. **Add lag compensation.** Implement client-side prediction: simulate movement locally using the same logic the server runs. On server correction, snap or interpolate back. For fighting/action games, consider rollback netcode (re-simulate N frames on correction).
4. **Interpolate remote entities.** Buffer received snapshots and interpolate between them (typically render one snapshot behind). Use snapshot interpolation at 20–60Hz tick rates to smooth visual positions.
5. **Optimize bandwidth.** Send delta-compressed state (only changed fields). Prioritize sync by relevance (nearby entities update more often). Target packet sizes under 1200 bytes to avoid fragmentation. Typical tick rates: 20Hz (casual), 30Hz (standard), 60Hz+ (competitive shooters).
6. **Validate on the server.** Never trust client-reported positions, damage, or inventory changes. Server checks movement speed limits, line-of-sight for hits, cooldown timers, and rate limits inputs to prevent spam.
7. **Implement lobby and matchmaking.** Use platform SDKs (Steam Networking, EOS, PlayFab) or custom relay. Handle host migration if the host disconnects in P2P. Separate lobby state from gameplay state.
8. **Choose transport.** UDP with a custom reliability layer (or libraries like ENet, LiteNetLib) for gameplay. WebRTC for browser-based multiplayer. TCP only for non-latency-sensitive data (chat, leaderboards).

# Decision rules

- Default to client-server authoritative — it prevents the most common cheats.
- Replicate only what each client needs (relevancy/interest management).
- Prediction is essential for player-controlled movement; optional for other entities.
- Use reliable RPCs for critical events (damage, death); unreliable for frequent state updates (position).
- Keep tick rate as low as acceptable — every tick costs bandwidth × player count.
- Test with simulated latency (100–300ms) and packet loss (1–5%) during development.

# Output requirements

1. `Architecture` — authority model, topology diagram, tick rate, transport protocol
2. `Replication Schema` — which state is synced, ownership, update frequency
3. `Prediction/Compensation` — what is predicted client-side, reconciliation strategy
4. `Validation` — latency simulation test results, bandwidth per-player measurement

# References

- Unity Netcode for GameObjects: https://docs-multiplayer.unity3d.com/netcode/current/about/
- UE5 Networking: https://docs.unrealengine.com/5.3/en-US/networking-and-multiplayer-in-unreal-engine/
- Godot High-Level Multiplayer: https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html
- Gabriel Gambetta netcode articles: https://www.gabrielgambetta.com/client-server-game-architecture.html

# Related skills

- `input-mapping-controller` — input prediction and network input handling
- `performance-profiling-games` — bandwidth and tick rate optimization
- `save-load-state` — persisting multiplayer match state and replays
- `game-design-systems` — designing systems that work under network constraints

# Failure handling

- If the target engine is unknown, clarify before writing netcode — APIs differ significantly.
- If latency exceeds 200ms in testing, increase interpolation buffer before adding more prediction complexity.
- If bandwidth is too high, audit replication: reduce tick rate, add delta compression, or tighten relevancy.
- If desync occurs, add server-side state checksums and reconciliation logging before debugging individual systems.
