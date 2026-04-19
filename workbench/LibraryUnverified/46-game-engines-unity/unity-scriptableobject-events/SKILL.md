---
name: unity-scriptableobject-events
description: Implements the ScriptableObject event channel architecture in Unity — GameEvent SOs with Raise/Register, GameEventListener MonoBehaviours, shared variable SOs (FloatVariable, IntVariable), RuntimeSet<T> patterns, and SO-based decoupling as an alternative to singletons. Use when designing event-driven communication between Unity systems without hard references.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: unity-scriptableobject-events
  maturity: draft
  risk: low
  tags: [unity, scriptableobject, events, architecture, decoupling]
---

# Purpose

Guide implementation of the ScriptableObject-based event architecture pattern in Unity — where SO assets act as event channels and data containers, enabling fully decoupled communication between systems without singletons or direct references. Based on Ryan Hipple's GDC talk "Game Architecture with ScriptableObjects."

# When to use this skill

- Designing event-driven communication between systems that should not reference each other directly
- Creating `GameEvent` ScriptableObjects that act as mediator channels between producers and consumers
- Building `GameEventListener` MonoBehaviours that register with a GameEvent and invoke a UnityEvent response
- Implementing shared variable SOs (`FloatVariable`, `IntVariable`, `StringVariable`) as observable data containers
- Creating `RuntimeSet<T>` ScriptableObjects to track collections of active objects (e.g., all enemies, all pickups)
- Deciding between SO events, C# events (`System.Action`), `UnityEvent`, or a message bus

# Do not use this skill when

- The task is general Unity scripting without an event architecture focus — use `unity`
- The task involves Unreal Engine event systems (delegates, dispatchers) — use `unreal-engine`
- The decoupling requirement is for network replication — use `multiplayer-netcode`

# Operating procedure

1. **Create the GameEvent SO.** Define a `ScriptableObject` with `[CreateAssetMenu(menuName="Events/GameEvent")]`. Include a `List<GameEventListener>` for registered listeners, a `Raise()` method that iterates listeners and calls `OnEventRaised()`, and `RegisterListener`/`UnregisterListener` methods.
2. **Create the GameEventListener MB.** A MonoBehaviour with a `GameEvent` field and a `UnityEvent` response. In `OnEnable`, call `gameEvent.RegisterListener(this)`. In `OnDisable`, call `gameEvent.UnregisterListener(this)`. Expose `OnEventRaised()` which invokes the UnityEvent.
3. **Wire in the inspector.** Create a GameEvent asset (right-click → Events → GameEvent). On the producer, reference the GameEvent and call `Raise()`. On the consumer, add a GameEventListener, assign the same GameEvent asset, and configure the UnityEvent response.
4. **Shared variable pattern.** Create `FloatVariable : ScriptableObject` with `[CreateAssetMenu]`, a `Value` field, and optionally an `OnValueChanged` event. Consumers read from the SO directly or listen for changes. Useful for HP bars, score displays, and settings.
5. **RuntimeSet pattern.** Create `RuntimeSet<T> : ScriptableObject` with a `List<T> Items`, `Add(T)`, and `Remove(T)`. Objects call `Add` in `OnEnable` and `Remove` in `OnDisable`. Systems iterate the set without `FindObjectsOfType`.
6. **Typed event variants.** For events with data, create `GameEvent<T>` generics: `IntGameEvent`, `FloatGameEvent`, `StringGameEvent`, `Vector3GameEvent`. Each has a corresponding typed listener with `UnityEvent<T>`.
7. **Debug and validate.** Add a `[TextArea] string developerDescription` to every SO event for documentation. Add editor buttons to Raise events from the inspector during play mode. Log event raises with listener counts.
8. **Choose the right event type.** SO events for designer-configurable, inspector-wired, cross-scene communication. C# `Action`/`event` for code-only, high-frequency, same-assembly events. `UnityEvent` for inspector-wired single-object callbacks. Enum-based event bus for rapid prototyping only.

# Decision rules

- Use SO events when designers need to wire connections in the inspector without code changes.
- Use C# `event Action` when performance matters (thousands of events/frame) and only programmers configure it.
- Use shared variable SOs over singletons for any data read by multiple systems (HP, score, settings).
- Use RuntimeSets over `FindObjectsOfType` — they're O(1) add/remove vs O(n) search.
- If an event carries complex data, create a typed SO event rather than passing multiple parameters.
- SO events survive scene loads if the asset is referenced by a persistent object or Addressable.

# Output requirements

1. `ScriptableObject Script` — complete C# class with `[CreateAssetMenu]`, fields, and methods
2. `Listener Script` — GameEventListener MonoBehaviour with registration lifecycle
3. `Inspector Setup` — how to create assets, assign references, configure UnityEvent callbacks
4. `Architecture Diagram` — which systems produce events, which consume, data flow direction
5. `Tradeoff Notes` — why SO events over alternatives for this specific use case

# References

- [Ryan Hipple — Game Architecture with ScriptableObjects (GDC 2017)](https://www.youtube.com/watch?v=raQ3iHhE_Kk)
- [Unity ScriptableObject Documentation](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [CreateAssetMenu Attribute](https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html)
- [UnityEvent Documentation](https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html)

# Related skills

- `unity` — general Unity patterns, lifecycle, and component architecture
- `ai-npc-behavior` — AI systems that consume SO events for perception/stimulus
- `multiplayer-netcode` — when events need network replication beyond local SO channels

# Failure handling

- If listeners don't receive events, verify `OnEnable`/`OnDisable` registration and that the same SO asset instance is referenced (not a duplicate).
- If SO values reset between scenes, ensure the SO asset is not instantiated at runtime — reference the project asset directly.
- If UnityEvent callbacks show as "Missing", the target GameObject was destroyed — use `OnDisable` unregistration.
- If event ordering matters, document the expected order or use a priority field in the listener.
