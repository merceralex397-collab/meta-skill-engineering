---
name: godot
description: Implements Godot 4 features using GDScript, scene tree composition, signals, resources, and node lifecycle hooks. Use when writing or reviewing GDScript code, designing scene hierarchies, wiring signals, creating custom Resources, or structuring a Godot project. Do not use for Unity or Unreal Engine work.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: godot
  maturity: draft
  risk: low
  tags: [godot, gdscript, scene-tree, signals]
---

# Purpose

Provides concrete GDScript patterns, scene tree architecture, signal wiring, resource management, and project organization conventions for Godot 4 development.

# When to use this skill

- Writing or reviewing GDScript (`.gd`) files or scene (`.tscn`) definitions
- Designing node hierarchies, signal connections, or custom Resource types
- Structuring a Godot project: autoloads, folder layout, scene composition
- Implementing gameplay systems: physics bodies, animation, state machines, UI

# Do not use this skill when

- The project uses Unity or Unreal Engine — prefer `unity` or `unreal-engine` skills
- The task is about general game design theory with no Godot-specific implementation
- Work is purely about 3D asset creation or shader authoring with no GDScript involvement

# Operating procedure

1. **Identify the node type.** Select the correct base: `Node2D`, `CharacterBody2D`, `Control`, `Node3D`, `CharacterBody3D`, etc. Prefer the most specific built-in node before creating custom ones.
2. **Use typed GDScript 2.0 syntax.** Declare `var speed: float = 200.0`, use `@export` for inspector properties, `@onready var sprite: Sprite2D = $Sprite2D` for deferred node refs.
3. **Wire signals with the new syntax.** Prefer `button.pressed.connect(_on_button_pressed)` over legacy string-based `connect()`. Define custom signals as `signal health_changed(new_hp: int)` and emit with `health_changed.emit(hp)`.
4. **Leverage the node lifecycle.** `_ready()` for initialization after tree entry, `_process(delta)` for per-frame logic, `_physics_process(delta)` for fixed-step physics, `_enter_tree()` / `_exit_tree()` for setup/teardown.
5. **Compose scenes over inheritance.** Build reusable behaviors as child scenes (component pattern). A `Player.tscn` contains `StateMachine`, `Hitbox`, `AnimationPlayer` as children rather than a deep class hierarchy.
6. **Use Resources for data.** Create custom `class_name` Resources (`.tres`) for configs: `class_name WeaponStats extends Resource` with `@export var damage: int`. Reference via `@export var weapon: WeaponStats`.
7. **Register autoloads for globals.** Use Project → Autoload for singletons: event bus (`Events.gd`), scene manager, global game state. Access as `Events.player_died.emit()`.
8. **Instantiate scenes correctly.** `var scene := preload("res://enemies/slime.tscn")` at load time, `var instance := scene.instantiate()`, then `add_child(instance)`.
9. **Implement state machines with enums.** `enum State { IDLE, RUN, JUMP, FALL }` with a `match` block in `_physics_process` for clear state transitions.
10. **Animate properly.** Use `AnimationPlayer` for complex sequences, `AnimationTree` with blend trees for character animation, `create_tween()` for procedural tweens.
11. **Handle physics correctly.** `CharacterBody2D.move_and_slide()` using `velocity` property. Use `Area2D` with `body_entered` / `body_exited` signals for triggers and hitboxes.
12. **Organize by feature.** `res://player/`, `res://enemies/`, `res://ui/` — co-locate `.tscn`, `.gd`, and `.tres` files per feature rather than by file type.

# Decision rules

- Prefer scene composition (child nodes) over deep class inheritance hierarchies.
- Use `@export` for designer-tunable values; hard-code only true constants.
- Choose `_physics_process` for movement/collision; `_process` for visuals and UI.
- Use Resources (`.tres`) for shared data configs; avoid storing game data in autoloads.
- Prefer `preload()` for known scenes; use `load()` only when the path is dynamic.
- Use signals for decoupled communication; direct node references (`$Path`) for tightly-coupled parent-child relationships.

# Output requirements

1. `Node Architecture` — scene tree diagram showing node types and hierarchy
2. `GDScript Implementation` — typed GDScript 2.0 code with `@export`, `@onready`, signals
3. `Signal Wiring` — list of signal connections and their callables
4. `Validation` — how to test in-editor: scene running, remote inspector, print debugging

# References

- Godot 4 documentation: https://docs.godotengine.org/en/stable/
- GDScript reference: https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/
- Node lifecycle: https://docs.godotengine.org/en/stable/tutorials/scripting/node_lifecycle.html

# Related skills

- `unity` — alternative engine, C# based
- `unreal-engine` — alternative engine, C++/Blueprint based
- `game-design-systems` — higher-level game mechanics design
- `save-load-state` — persistence patterns applicable to Godot's Resource system

# Failure handling

- If the Godot version is ambiguous, assume Godot 4.x and typed GDScript 2.0 syntax.
- If a node type is unclear, check the class hierarchy in docs before choosing a base.
- If performance issues arise, profile with Godot's built-in monitors before optimizing.
- If the task requires C# or GDExtension, note the limitation and suggest the appropriate approach.
