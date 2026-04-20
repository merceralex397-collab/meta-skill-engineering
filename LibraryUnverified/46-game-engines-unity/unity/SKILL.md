---
name: unity
description: Guides Unity C# development — MonoBehaviour lifecycle, component architecture, ScriptableObjects, Addressables, New Input System, Physics, ECS/DOTS, coroutines, UI Toolkit, Assembly Definitions, prefab workflows, and common C# patterns like object pooling and SerializeField. Use when writing, reviewing, or architecting Unity project code.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: unity
  maturity: draft
  risk: low
  tags: [unity, csharp, game-engine, monobehaviour]
---

# Purpose

Provide concrete C# patterns, API usage, and architectural guidance for Unity projects — covering lifecycle methods, component design, asset management, input handling, physics, UI, and performance optimization.

# When to use this skill

- Writing or reviewing MonoBehaviour scripts using lifecycle methods (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDestroy`)
- Designing component architecture with `GetComponent<T>()`, `AddComponent<T>()`, `[RequireComponent]`
- Creating ScriptableObject data containers, shared configs, or event channels with `[CreateAssetMenu]`
- Configuring Addressables: `Addressables.LoadAssetAsync<T>()`, asset groups, labels, remote catalogs
- Setting up New Input System: `InputAction`, `PlayerInput` component, action maps, runtime rebinding
- Physics work: Rigidbody, colliders, triggers, layer masks, `Physics.Raycast()`, `OnCollisionEnter`
- ECS/DOTS: `IComponentData` structs, `SystemBase`/`ISystem`, `EntityManager`, Burst-compiled jobs
- UI Toolkit: USS styling, UXML layout, `VisualElement` hierarchy, data binding with `SerializedObject`
- Project structure: Assembly Definitions (`.asmdef`), platform defines (`#if UNITY_ANDROID`), prefab workflows

# Do not use this skill when

- The project uses Unreal Engine — use `unreal-engine` or `ue5-blueprint`
- The task is specifically about ScriptableObject event architecture — use `unity-scriptableobject-events`
- The task is about narrative/lore data, not engine code — use `worldbuilding-lore-systems`

# Operating procedure

1. **Respect the lifecycle.** `Awake` for self-initialization and `GetComponent` caching. `Start` for cross-object setup. `FixedUpdate` for physics. `Update` for input/frame logic. `LateUpdate` for camera follow. `OnDestroy` for cleanup and unsubscription.
2. **Compose with components.** Favor composition over inheritance. Use `[RequireComponent(typeof(Rigidbody))]` to enforce dependencies. Access siblings via cached `GetComponent<T>()` in `Awake`, never in `Update`.
3. **Use ScriptableObjects for data.** Create asset instances with `[CreateAssetMenu(fileName="New X", menuName="Game/X")]`. Use for shared config (weapon stats, enemy profiles), runtime sets, and event channels. Never store scene-specific state in SOs.
4. **Load assets with Addressables.** Mark assets addressable in the inspector. Load via `Addressables.LoadAssetAsync<GameObject>("key")`. Release with `Addressables.Release(handle)`. Group by load context, not asset type.
5. **New Input System.** Define actions in an InputActionAsset. Reference via `PlayerInput` component or generated C# class. Bind with `action.performed += ctx => { }`. Support rebinding with `InputActionRebindingExtensions`.
6. **Physics.** Apply forces in `FixedUpdate`. Use layers and `LayerMask` for selective raycasts. Prefer `Physics.Raycast(origin, dir, out hit, maxDist, layerMask)`. Use triggers (`OnTriggerEnter`) for detection zones.
7. **Coroutines and async.** Use `StartCoroutine` + `yield return new WaitForSeconds(t)` for simple delays. For complex async, use `async/await` with `UniTask` or `Awaitable` (Unity 2023+). Always stop coroutines on disable.
8. **Project structure.** Use Assembly Definitions to control compilation. Separate `Runtime/`, `Editor/`, `Tests/` assemblies. Use `#if UNITY_EDITOR` guards for editor-only code. Nest prefab variants for shared base + specialization.
9. **Common C# patterns.** Use `[SerializeField] private` over public fields. Group inspector fields with `[Header("Movement")]`. Implement object pools with `Queue<T>`. Use events (`System.Action`, `UnityEvent`) for decoupling. Avoid singletons except for service locators.
10. **Profile and optimize.** Use Unity Profiler and Frame Debugger. Minimize allocations in `Update` (no `GetComponent`, no LINQ, no string concat). Batch draw calls with static/dynamic batching. Use `ObjectPool<T>` from `UnityEngine.Pool`.

# Decision rules

- Cache all `GetComponent` calls in `Awake`; never call them per-frame.
- Prefer `[SerializeField] private` over `public` fields — expose to inspector without breaking encapsulation.
- Use ScriptableObjects for data shared across scenes; use MonoBehaviours for scene-specific behavior.
- Choose Addressables over `Resources.Load` for any project beyond a prototype.
- Use Assembly Definitions in any project with more than ~10 scripts to cut recompile times.
- For physics-driven gameplay, all movement goes through `Rigidbody` in `FixedUpdate`, never `Transform.Translate` in `Update`.

# Output requirements

1. `Script` — complete C# file with correct `using` statements, namespace, and Unity attributes
2. `Lifecycle` — which MonoBehaviour callbacks are used and why
3. `Dependencies` — required components, packages (Addressables, Input System, etc.)
4. `Inspector Setup` — serialized fields, headers, component configuration
5. `Performance Notes` — allocation concerns, caching strategy, profiler targets

# References

- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
- [Unity Manual — Order of Execution](https://docs.unity3d.com/Manual/ExecutionOrder.html)
- [Addressables Documentation](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [Input System Manual](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [UI Toolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- [DOTS/ECS](https://docs.unity3d.com/Packages/com.unity.entities@latest)

# Related skills

- `unity-scriptableobject-events` — SO-based event architecture patterns
- `unreal-engine` — when comparing Unity vs Unreal approaches
- `game-design-systems` — higher-level game system design

# Failure handling

- If `GetComponent` returns null, check that the component exists on the same GameObject or use `TryGetComponent`.
- If Addressables load fails, verify the asset is marked addressable and the catalog is built (`Addressables.InitializeAsync()`).
- If Input System actions don't fire, confirm the PlayerInput component is set to the correct action map and the InputActionAsset is assigned.
- If physics interactions are missed, check that at least one object has a non-kinematic Rigidbody, colliders are not set to trigger incorrectly, and layers are in the collision matrix.
