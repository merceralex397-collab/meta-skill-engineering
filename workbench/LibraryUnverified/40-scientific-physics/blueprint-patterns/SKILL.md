---
name: blueprint-patterns
description: Implements clean Unreal Engine 5 Blueprint architecture including BP types, event flow, BP-C++ boundaries, interfaces, macros vs functions, casting, and performance. Use when designing or refactoring UE5 Blueprints, wiring GameplayAbilitySystem or Enhanced Input in BP, or fixing spaghetti Blueprint graphs.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: blueprint-patterns
  maturity: draft
  risk: low
  tags: [unreal, blueprint, ue5, visual-scripting, gameplay]
---

# Purpose

Design and implement clean, performant Unreal Engine 5 Blueprint architectures: correct BP type selection, event-driven communication, BP-C++ interop, interface decoupling, and performance optimization for visual scripting graphs.

# When to use this skill

- Creating or refactoring UE5 Blueprint actors, widgets, or components
- Wiring Blueprint-to-C++ boundaries (`UFUNCTION`, `UPROPERTY` macros)
- Designing Blueprint interfaces for decoupled actor communication
- Integrating GameplayAbilitySystem (GAS) or Enhanced Input via Blueprints
- Debugging Blueprint execution flow, breakpoints, or watch values
- Optimizing Blueprint performance (replacing Tick, nativizing hot paths)

# Do not use this skill when

- Working in Unity or Godot — this is UE5-specific
- Writing pure C++ without Blueprint exposure — use general C++ patterns
- The task is about NPC AI logic — use `ai-npc-behavior` (though BT integration overlaps)
- The task is about asset import/compression — use `asset-pipeline`

# Operating procedure

1. **Choose the correct Blueprint type**:
   - **Actor BP**: for placed-in-world entities (characters, pickups, triggers).
   - **Component BP**: for reusable behavior attached to any actor (health component, inventory).
   - **Widget BP**: for UMG UI elements (HUD, menus, popups).
   - **Interface BP**: for defining contracts without coupling (IDamageable, IInteractable).
   - **Function Library**: for pure static utility functions (math helpers, string formatters).
   - **Macro Library**: for reusable graph snippets that inline-expand (multi-output control flow).
2. **Design event flow**:
   - Use `BeginPlay` for initialization, avoid `Tick` unless per-frame updates are truly needed.
   - Prefer **Timers** (`SetTimerByEvent`) over Tick for periodic logic.
   - Use **Event Dispatchers** (multicast delegates) for one-to-many notification (e.g., OnHealthChanged).
   - Use **Custom Events** for internal Blueprint communication.
   - Bind dispatchers in BeginPlay, unbind in EndPlay to prevent dangling references.
3. **Set up BP-C++ boundaries**:
   - `UFUNCTION(BlueprintCallable)`: C++ function callable from BP.
   - `UFUNCTION(BlueprintImplementableEvent)`: C++ declares, BP implements.
   - `UFUNCTION(BlueprintNativeEvent)`: C++ provides default, BP can override.
   - `UPROPERTY(EditAnywhere, BlueprintReadWrite)`: expose variables to BP editor.
   - Keep heavy computation in C++; keep configuration and wiring in BP.
4. **Use interfaces for decoupling**: create a Blueprint Interface (e.g., `BPI_Interactable`) with `Interact(AActor* Caller)`. Implement on any actor. Call via `Does Implement Interface` → `Interact` message. Never hard-cast to concrete types for communication.
5. **Understand macros vs functions**: Macros inline-expand (support multiple exec outputs, no call overhead, no breakpoints). Functions use call stack (debuggable, support local variables, cause one function call). Use macros only for flow control; use functions for everything else.
6. **Manage casting and references**: avoid frequent `CastTo<>` calls — cache results. Use **Soft Object References** (`TSoftObjectPtr`) for assets to prevent hard loading. Use `IsValid()` checks before dereferencing.
7. **Integrate GAS in Blueprints**: create GameplayAbilities as BP subclasses, bind to `InputAction` via Enhanced Input. Use `AbilitySystemComponent->TryActivateAbility()`. Wire gameplay effects and attribute sets via BP-exposed properties.
8. **Debug and profile**: set BP breakpoints (F9), add watch values, step through execution. Use `stat game` and Blueprint profiler to find hot nodes. Nativize performance-critical BPs via `NativizeAssets` project setting.

# Decision rules

- If a BP graph exceeds ~50 nodes, split into functions or sub-Blueprints.
- If the same logic appears in 3+ Blueprints, extract to a Component BP or Function Library.
- If an actor needs to communicate without knowing the receiver type, use a Blueprint Interface.
- If Tick is used, justify it — most logic should be event-driven or timer-based.
- If performance profiling shows BP overhead >1ms/frame, move the hot path to C++ with `BlueprintCallable`.
- Prefer Event Dispatchers over direct references for observer-pattern communication.
- Use `Collapsed Graphs` and `Comment Boxes` to organize complex BP graphs visually.

# Output requirements

1. `Blueprint Type` — which BP type and rationale
2. `Event Flow Diagram` — text description of event chain (BeginPlay → Bind → Dispatch → Handle)
3. `C++ Interface` — any UFUNCTION/UPROPERTY declarations needed
4. `Blueprint Graph Description` — key nodes, connections, and variable names
5. `Performance Notes` — Tick usage, cast frequency, nativization candidates
6. `Debug Steps` — breakpoint locations and expected execution flow

# References

- UE5 Docs: Blueprint Visual Scripting, Blueprint Best Practices, Blueprint Communication
- UE5 API: `UBlueprintFunctionLibrary`, `UBlueprintInterface`, `UGameplayAbility`
- UE5 Enhanced Input: `UInputAction`, `UInputMappingContext`, BP binding
- UE5 GAS: `UAbilitySystemComponent`, `UGameplayEffect`, `FGameplayAttribute`
- Epic Games, *Blueprint Nativization* documentation

# Related skills

- `ai-npc-behavior` — UE5 behavior trees are authored in BP + Blackboard
- `game-ui-hud` — UMG widgets are Blueprint-based
- `save-load-state` — save game objects exposed to BP via `USaveGame`
- `multiplayer-netcode` — replicated properties and RPCs in BP

# Failure handling

- If a Blueprint compile error occurs, check for broken references — use `Find References` and `Redirect` tools.
- If casting fails at runtime, add `IsValid` checks and log the failing actor class.
- If BP performance is unacceptable, identify the node via Blueprint Profiler and move to C++.
- If spaghetti is already severe, refactor incrementally: extract one function per PR, add comment boxes, replace casts with interfaces.
