---
name: ue5-blueprint
description: Guides UE5 Blueprint visual scripting — class selection (Actor, Pawn, Character, GameMode, Widget, AnimBP), event graphs, Blueprint-C++ interfaces via UFUNCTION macros, communication patterns (interfaces, dispatchers, casting), Enhanced Input, GAS abilities, widget binding, and Blueprint optimization/debugging. Use when designing or reviewing Blueprint graphs in Unreal Engine 5.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ue5-blueprint
  maturity: draft
  risk: low
  tags: [ue5, blueprint, visual-scripting, unreal]
---

# Purpose

Provide concrete guidance for UE5 Blueprint visual scripting: choosing the right Blueprint class, wiring event graphs, exposing C++ to Blueprints, inter-Blueprint communication, and optimizing graph performance.

# When to use this skill

- Task involves creating or modifying Blueprint classes (Actor, Pawn, Character, GameMode, Widget, AnimBP)
- Wiring event graphs: BeginPlay, Tick, input actions, collision/overlap, custom events
- Exposing C++ functions with `UFUNCTION(BlueprintCallable, Category="...")` or `BlueprintImplementableEvent`
- Setting up Enhanced Input in Blueprints: InputAction assets, InputMappingContext, Triggered/Completed pins
- Implementing Gameplay Ability System (GAS) abilities, gameplay effects, or gameplay tags in Blueprint
- Building Widget Blueprints: property bindings, widget animations, UI navigation focus

# Do not use this skill when

- The task is pure C++ with no Blueprint involvement — use `unreal-engine` instead
- The task is about lore, narrative, or world data — use `worldbuilding-lore-systems`
- The project targets Unity, Godot, or another engine

# Operating procedure

1. **Select Blueprint class type.** Actor for placed objects, Pawn/Character for possessed entities, GameMode for match rules, AnimBP for animation state machines, Widget for UI.
2. **Wire the event graph.** Start from lifecycle events (BeginPlay, EndPlay) or input events. Avoid Tick unless frame-rate-dependent logic is required; prefer timers (`Set Timer by Event/Function Name`) or event-driven updates.
3. **Define data flow.** Use variables (promote pin to variable), local variables for function scope, return nodes for pure functions. Keep exec pin chains short — extract to functions/macros for readability.
4. **Establish communication.** Choose the lightest coupling: Blueprint Interfaces for polymorphic calls, Event Dispatchers for one-to-many broadcast, direct references only when ownership is clear, Cast nodes as last resort (creates hard dependency).
5. **Expose C++ when needed.** Mark C++ functions `UFUNCTION(BlueprintCallable)` for Blueprint calls, `BlueprintImplementableEvent` for Blueprint overrides, `BlueprintNativeEvent` for C++ default + Blueprint override.
6. **Enhanced Input setup.** Create InputAction assets per action, group into InputMappingContext, add mapping context in BeginPlay via `AddMappingContext`, bind actions in Blueprint with `EnhancedInputAction` nodes.
7. **GAS in Blueprints.** Grant abilities via `GiveAbility`, activate with `TryActivateAbilityByClass`, apply GameplayEffects for stat changes, query tags with `HasMatchingGameplayTag`.
8. **Widget patterns.** Bind text/progress bars to functions or properties, drive animations with `PlayAnimation`/`PlayAnimationReverse`, handle navigation with `SetFocus` and `OnFocusReceived`.
9. **Optimize.** Disable Tick on actors that don't need it, use `DoOnce`/`Gate` nodes, nativize hot Blueprint paths to C++, avoid heavy operations in construction scripts.
10. **Debug.** Use Blueprint Debugger (F9 breakpoints, step through), Print String nodes with duration, Visual Logger for spatial debugging, watch variables in the debugger panel.

# Decision rules

- If a Blueprint graph exceeds ~50 nodes in one function, extract sub-functions or consider moving logic to C++.
- Use interfaces over casting when the caller doesn't need to know the concrete class.
- Prefer Event Dispatchers over direct references for loosely coupled systems (UI listening to gameplay).
- Gameplay-critical math or AI loops should be C++ with Blueprint-exposed results, not pure Blueprint.
- Widget Blueprints should bind to data, not poll in Tick — use property binding or event-driven updates.

# Output requirements

1. `Blueprint Class` — which class type and parent, with rationale
2. `Event Graph` — key events wired, communication pattern used
3. `C++ Interface` — any UFUNCTION macros needed and their specifiers
4. `Optimization Notes` — Tick usage, timer strategy, nativization candidates
5. `Testing` — PIE test steps, Blueprint debugger breakpoints to verify

# References

- [UE5 Blueprint Visual Scripting docs](https://docs.unrealengine.com/5.0/en-US/blueprints-visual-scripting-in-unreal-engine/)
- [Enhanced Input System](https://docs.unrealengine.com/5.0/en-US/enhanced-input-in-unreal-engine/)
- [Gameplay Ability System](https://docs.unrealengine.com/5.0/en-US/gameplay-ability-system-for-unreal-engine/)
- [UMG Widget Blueprints](https://docs.unrealengine.com/5.0/en-US/umg-ui-designer-for-unreal-engine/)

# Related skills

- `unreal-engine` — C++ side of the Blueprint-C++ boundary
- `blueprint-patterns` — reusable Blueprint architecture patterns
- `game-design-systems` — higher-level game design using Blueprint systems

# Failure handling

- If unsure whether logic belongs in Blueprint or C++, default to Blueprint for prototyping, note it as a nativization candidate.
- If a Cast node fails at runtime, check the actor class hierarchy and add an `IsValid` check before the cast.
- If Enhanced Input actions don't fire, verify the MappingContext was added in BeginPlay and the PlayerController has an EnhancedInputComponent.
- If GAS abilities won't activate, confirm the AbilitySystemComponent is initialized and the ability was granted before activation.
