---
name: input-mapping-controller
description: Implements action-based input mapping, controller support, rebinding, and multi-device handling across Unity, UE5, and Godot. Use when designing input systems, adding gamepad support, implementing rebinding, handling input buffering, or switching input contexts. Do not use for general UI layout or non-input game logic.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: input-mapping-controller
  maturity: draft
  risk: low
  tags: [input, controller, rebinding, gamepad, input-system]
---

# Purpose

Provides engine-specific patterns for action-based input mapping, runtime rebinding, gamepad/touch/keyboard support, input buffering, and context switching across Unity, UE5, and Godot.

# When to use this skill

- Implementing an input system that maps physical buttons to game actions
- Adding gamepad support with dead zones, analog curves, or rumble/haptics
- Building runtime key rebinding with serialization and persistence
- Handling context switching (gameplay vs menu vs vehicle input maps)
- Implementing input buffering, coyote time, or other responsive-feel techniques
- Supporting multiple simultaneous input devices (keyboard+mouse alongside gamepad)

# Do not use this skill when

- The task is about UI layout or HUD design with no input system work — prefer `game-ui-hud`
- The task is about network input synchronization — prefer `multiplayer-netcode`
- The task is purely about animation driven by input (no input mapping changes needed)

# Operating procedure

1. **Define actions, not keys.** Create an abstract action layer: `Jump`, `Attack`, `Move` — never reference physical keys in gameplay code. This is the foundation of every cross-platform input system.
2. **Use the engine's input system.**
   - **Unity New Input System:** Create an `.inputactions` asset. Define `InputActionMap` groups (Player, UI, Vehicle). Use `PlayerInput` component or generate a C# class. Read via `action.ReadValue<Vector2>()` or callback `action.performed += ctx => { }`.
   - **UE5 Enhanced Input:** Define `UInputAction` assets per action. Create `UInputMappingContext` to bind keys. In the PlayerController: `EnhancedInputComponent->BindAction(JumpAction, ETriggerEvent::Started, this, &AMyChar::Jump)`.
   - **Godot Input Map:** Configure in Project → Input Map. Read via `Input.is_action_just_pressed("jump")` or handle `InputEvent` in `_unhandled_input(event)`.
3. **Implement runtime rebinding.**
   - Listen for the next input event during rebind mode.
   - Store bindings as a serializable map: `{ "jump": "Keyboard/Space", "attack": "Gamepad/ButtonSouth" }`.
   - Save/load bindings to JSON or the engine's settings system (Unity `PlayerPrefs`, UE5 `GameUserSettings`, Godot `ConfigFile`).
4. **Configure gamepad correctly.** Set dead zones (typically 0.15–0.25 for sticks). Apply response curves (linear, quadratic, or custom) for analog movement. Enable rumble via engine APIs: Unity `Gamepad.current.SetMotorSpeeds()`, UE5 `PlayDynamicForceFeedback()`.
5. **Handle context switching.** Enable/disable action maps when entering menus, vehicles, or cutscenes. Unity: `playerInput.SwitchCurrentActionMap("UI")`. UE5: add/remove `InputMappingContext` via `AddMappingContext()` / `RemoveMappingContext()`.
6. **Implement input buffering.** Store the last N frames of input. Allow jump if pressed within a buffer window (e.g., 100ms before landing). Implement coyote time: allow jump for ~80ms after leaving a platform edge.
7. **Support accessibility.** Allow hold-vs-toggle options, adjustable stick sensitivity, remappable controls for all actions, and separate mouse sensitivity settings.

# Decision rules

- Always decouple physical input from game actions — never hard-code `KeyCode.Space` in gameplay logic.
- Default to the engine's built-in input system before building custom solutions.
- Treat gamepad and keyboard as equal citizens; test both paths during development.
- Buffer inputs for action games; skip buffering for strategy or menu-heavy games.
- Persist rebindings separately from game save data — they are user preferences, not game state.
- When supporting multiple devices simultaneously, use the last-active device to drive UI prompt icons.

# Output requirements

1. `Action Map` — list of abstract actions with default bindings per device
2. `Implementation` — engine-specific code for reading actions and handling callbacks
3. `Rebinding Flow` — how bindings are captured, stored, serialized, and restored
4. `Validation` — test matrix: keyboard, gamepad, rebind → save → reload, context switch

# References

- Unity Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/
- UE5 Enhanced Input: https://docs.unrealengine.com/5.3/en-US/enhanced-input-in-unreal-engine/
- Godot InputEvent: https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html

# Related skills

- `save-load-state` — persisting rebinding preferences
- `game-ui-hud` — displaying context-sensitive button prompts
- `performance-profiling-games` — input latency measurement
- `multiplayer-netcode` — input prediction and synchronization

# Failure handling

- If the target engine is unknown, ask before proceeding — input APIs differ fundamentally.
- If rebinding conflicts arise (two actions on same key), warn the player and allow override or swap.
- If gamepad is not detected at runtime, fall back gracefully to keyboard+mouse with no errors.
- If input feels unresponsive, check buffer window sizes and dead zone thresholds before rewriting logic.
