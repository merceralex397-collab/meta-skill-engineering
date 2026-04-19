---
name: ai-npc-behavior
description: Implements NPC AI using behavior trees, FSMs, utility AI, GOAP, steering behaviors, and perception systems. Use when building enemy AI, companion logic, NPC patrol/combat routines, pathfinding, or agent decision-making in Unity, Unreal, or Godot.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ai-npc-behavior
  maturity: draft
  risk: low
  tags: [ai, npc, behavior, pathfinding, behavior-tree, fsm]
---

# Purpose

Implement NPC decision-making, navigation, and perception systems using proven game AI architectures: behavior trees, finite state machines, utility AI, GOAP, and steering behaviors.

# When to use this skill

- Building enemy AI, companion AI, or NPC routines (patrol, combat, idle)
- Implementing pathfinding with NavMesh, A* grid search, or flow fields
- Designing perception systems (sight cones, hearing, awareness levels)
- Creating behavior trees, FSMs, or utility-based scoring for agent decisions
- Integrating AI with engine-specific systems (UE5 AI Controller, Unity NavMeshAgent, Godot NavigationAgent3D)

# Do not use this skill when

- The task is purely UI/HUD work — use `game-ui-hud` instead
- The task is about player input handling — use `input-mapping-controller`
- The task is about general game system design without AI — use `game-design-systems`
- Building multiplayer netcode for player-controlled characters

# Operating procedure

1. **Identify the AI archetype**: classify the NPC role (enemy, companion, ambient, boss) and required behaviors (patrol, chase, attack, flee, interact).
2. **Select the decision architecture**:
   - **Behavior Tree** for complex hierarchical decisions — use Selector (fallback), Sequence (all-must-pass), Decorator (conditional guards), and Leaf (action/condition) nodes. Tick each frame or on-event.
   - **FSM** for NPCs with clear, discrete states — define states (Idle, Patrol, Chase, Attack), transitions with enter/exit/update callbacks.
   - **Utility AI** when NPCs must evaluate many possible actions — score each action using weighted response curves (linear, quadratic, logistic) across multiple axes (health, distance, ammo).
   - **GOAP** for NPCs that plan multi-step sequences — define actions with preconditions and effects, use A* to find cheapest plan to satisfy a goal.
3. **Implement perception**: sight cones via dot-product checks (`Vector3.Dot(forward, toTarget) > cos(halfAngle)`), hearing radius via `Physics.OverlapSphere`, awareness levels (unaware → suspicious → alert) with decay timers.
4. **Implement navigation**: use engine NavMesh (`NavMeshAgent.SetDestination()` in Unity, `UAITask_MoveTo` in UE5, `NavigationAgent3D.target_position` in Godot). For grid worlds, implement A* with Manhattan/octile heuristic. Apply path smoothing via funnel algorithm.
5. **Add steering behaviors** for dynamic movement: seek, flee, arrive (deceleration radius), wander (random jitter on circle), flocking (separation + alignment + cohesion weighted sum).
6. **Wire up the blackboard**: use a shared key-value store for behavior tree nodes to read/write data (target reference, last-known position, alert level). In UE5 use `UBlackboardComponent`; in Unity use a `Dictionary<string, object>` on the AI controller.
7. **Profile and tune**: cap behavior tree depth to ~15 nodes for readability, stagger AI ticks across frames to avoid spikes, use LOD-based AI (full logic near player, simplified at distance).

# Decision rules

- Prefer behavior trees for complex NPCs with many conditional branches; prefer FSMs for simple 3-5 state actors.
- Use utility AI when the NPC must weigh competing needs simultaneously (e.g., survival games).
- Use GOAP only when NPCs need emergent multi-step planning — it has higher CPU cost.
- Always separate perception from decision-making — perception feeds data to the blackboard, decisions read from it.
- Stagger AI updates: not every NPC needs to tick every frame. Use 0.1–0.5s intervals for non-critical agents.
- Prefer NavMesh over grid A* for 3D environments; use grid A* for 2D or tile-based games.

# Output requirements

1. `AI Architecture` — which pattern (BT/FSM/Utility/GOAP) and why
2. `State/Node Diagram` — text description of states or tree structure
3. `Perception Config` — sight range, angle, hearing radius, awareness thresholds
4. `Navigation Setup` — NavMesh bake settings or grid config, agent radius/speed
5. `Implementation Code` — engine-specific code with blackboard integration
6. `Performance Budget` — max concurrent AI agents, tick interval, LOD strategy

# References

- UE5 AI: `AIController`, `UBehaviorTree`, `UBlackboardComponent`, `UAIPerceptionComponent`
- Unity: `NavMeshAgent`, `NavMesh.SamplePosition()`, ML-Agents for learning-based AI
- Godot 4: `NavigationAgent3D`, `NavigationServer3D`, `Area3D` for perception zones
- Millington & Funge, *AI for Games* (behavior trees, steering, pathfinding)
- Orkin, *Applying GOAP to Games* (GDC, F.E.A.R. AI architecture)

# Related skills

- `game-design-systems` — enemy difficulty curves feed AI parameters
- `blueprint-patterns` — UE5 behavior trees are often wired in Blueprints
- `performance-profiling-games` — AI tick budgets and NavMesh query costs
- `input-mapping-controller` — player input vs AI input abstraction

# Failure handling

- If the NPC count exceeds performance budget, reduce tick frequency or switch distant NPCs to simplified FSM.
- If pathfinding fails (no valid path), implement fallback: teleport to nearest valid NavMesh point or enter idle state.
- If behavior tree grows beyond 20+ nodes and becomes unreadable, refactor into sub-trees.
- If the engine lacks a built-in behavior tree (e.g., Godot), recommend `LimboAI` addon or a custom lightweight BT implementation.
