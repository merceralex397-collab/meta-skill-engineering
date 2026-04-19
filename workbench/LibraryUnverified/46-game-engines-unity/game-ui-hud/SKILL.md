---
name: game-ui-hud
description: Implements game UI and HUD systems including health bars, minimaps, menus, inventory screens, and responsive layouts. Use when building HUD elements, menu systems, screen-space or world-space UI, UI animation, input focus navigation, accessibility features, or optimizing canvas/widget performance in Unity, Unreal, or Godot.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: game-ui-hud
  maturity: draft
  risk: low
  tags: [game, ui, hud, menu, widget, ugui, umg, accessibility]
---

# Purpose

Build and optimize game UI and HUD systems: in-game overlays (health, ammo, minimap), menu screens (main, pause, settings, inventory), responsive layouts, world-space UI, animation/juice, multi-input support, accessibility, and rendering performance.

# When to use this skill

- Building HUD elements: health bars, ammo counters, minimaps, crosshairs, status icons
- Creating menu systems: main menu, pause, settings, inventory, shop screens
- Implementing UI in Unity (UI Toolkit or UGUI), UE5 (UMG/Widget Blueprints), or Godot (Control nodes)
- Making UI responsive across resolutions, aspect ratios, and safe areas
- Adding world-space UI: damage numbers, nameplates, interaction prompts
- Implementing UI animation, tweening, and micro-interactions
- Supporting multiple input methods (mouse, gamepad, touch) with focus navigation
- Adding accessibility: colorblind modes, text scaling, screen reader hooks

# Do not use this skill when

- The task is about game mechanics or balance math — use `game-design-systems`
- The task is about web/app UI with no game engine — use standard frontend skills
- The task is about 3D rendering or shaders — use `shader-vfx`
- The task is about asset import/compression for UI sprites — use `asset-pipeline`

# Operating procedure

1. **Choose the UI framework**:
   - **Unity UI Toolkit**: USS (styling) + UXML (structure), data binding, `VisualElement` hierarchy. Preferred for new Unity projects.
   - **Unity UGUI**: `Canvas` + `RectTransform`, `Image`, `Text`/`TextMeshPro`, `Button`. Mature, wide community support.
   - **UE5 UMG**: Widget Blueprints, `UUserWidget`, `UHorizontalBox`/`UVerticalBox`/`UOverlay`, bind to `UMG Animation`.
   - **Godot**: `Control` node tree, `VBoxContainer`/`HBoxContainer`, `Theme` resources, `Label`, `TextureRect`.
2. **Design the HUD layer**:
   - Health bar: use a filled `Image` (Unity) or `ProgressBar` (UE5) bound to `currentHP / maxHP`. Tween color from green → yellow → red.
   - Minimap: render a top-down camera to `RenderTexture` (Unity) or `SceneCaptureComponent2D` (UE5), display as circular masked UI element.
   - Ammo counter: bind text to weapon data, flash on low ammo (<20%), pulse on reload.
   - Crosshair: screen-center overlay, scale with movement accuracy, hide during ADS.
3. **Build menu systems**:
   - Use a **UI Manager** singleton to control screen stack: `Push(screen)`, `Pop()`, `PopToRoot()`. Only the top screen receives input.
   - Main menu: Play, Settings, Credits, Quit. Animate transitions with slide or fade (200–400ms).
   - Pause menu: freeze `Time.timeScale = 0` (Unity) or `SetGamePaused(true)` (UE5). Ensure UI animations use unscaled time.
   - Settings: resolution dropdown, fullscreen toggle, volume sliders, key rebinding. Save to `PlayerPrefs` (Unity) or `GameUserSettings` (UE5).
4. **Make UI responsive**:
   - Set canvas to `Scale With Screen Size` (Unity) at reference 1920×1080, match 0.5 width/height.
   - Anchor HUD elements to screen edges (health=bottom-left, minimap=top-right).
   - Respect safe areas on mobile/console: use `Screen.safeArea` (Unity) or `GetDisplayMetrics` (UE5).
   - Support 16:9, 21:9, and 4:3 — test with letterboxing and pillarboxing.
5. **Implement world-space UI**:
   - Damage numbers: spawn `TextMeshPro` (Unity) or `UWidgetComponent` (UE5) at hit point, animate upward with fade-out over 0.8s. Pool instances.
   - Nameplates: attach `WorldSpace Canvas` to actor, billboard toward camera, fade by distance.
   - Interaction prompts: show "Press E" when player overlaps trigger, hide on exit. Swap icon based on active input device.
6. **Add animation and juice**:
   - Use tweening (DOTween in Unity, UMG animations in UE5, `Tween` in Godot) for all transitions.
   - Micro-interactions: button scale on hover (1.0 → 1.05, 100ms), click punch (1.05 → 0.95 → 1.0).
   - Screen shake on damage: offset camera or UI parent by random amount, decay over 0.3s.
   - Number countup for score changes: interpolate displayed value over 0.5s.
7. **Handle multi-input and focus**:
   - Detect active input device: if gamepad input received, switch to focus-based navigation and show gamepad icons.
   - Set `firstSelectedGameObject` (Unity) or `SetKeyboardFocus` (UE5) when opening a menu.
   - Ensure all interactive elements are navigable via D-pad/stick.
   - For touch: increase hit targets to minimum 44×44dp, add touch feedback ripple.
8. **Implement accessibility**:
   - Colorblind mode: provide icon/shape differentiation in addition to color. Offer Deuteranopia/Protanopia/Tritanopia filters.
   - Text scaling: support 80%–150% UI scale slider. Screen reader: tag elements with `AccessibleText` (UE5).
9. **Optimize performance**:
   - Unity UGUI: split canvases (static HUD vs dynamic elements) to reduce rebuild cost. Disable `Raycast Target` on non-interactive elements. Use `Canvas.willRenderCanvases` profiler marker.
   - UE5 UMG: use widget pooling (pre-create and show/hide) instead of Create/Destroy. Mark static widgets `IsVolatile = false`.
   - All engines: use dirty-flag redraw — only update elements when underlying data changes, not every frame.
   - Batch draw calls: use texture atlases for UI sprites, minimize unique materials.

# Decision rules

- Prefer UI Toolkit over UGUI for new Unity projects (better performance, CSS-like styling).
- Always use TextMeshPro (Unity) or UMG RichText (UE5) — never legacy `Text` component.
- Split HUD into separate canvases/widgets by update frequency (health = event-driven, timer = per-second, FPS = per-frame).
- World-space UI must be pooled if >20 instances are possible (damage numbers, nameplates).
- All UI text must go through localization system from day one — never hardcode strings.
- Every interactive element must be reachable by both mouse/touch and gamepad.

# Output requirements

1. `Screen List` — all UI screens with hierarchy (HUD, MainMenu, Pause, Settings, Inventory)
2. `Layout Spec` — anchoring, reference resolution, safe area handling
3. `Widget/Element List` — each element with data binding source and update frequency
4. `Animation Spec` — tween targets, durations, easing curves for transitions
5. `Input Map` — how each screen handles mouse, gamepad, and touch
6. `Accessibility Checklist` — colorblind support, text scaling, focus order
7. `Performance Budget` — target draw calls for UI, max canvas rebuilds per frame

# References

- Unity: `UI Toolkit` (USS/UXML), `Canvas`, `TextMeshPro`, `EventSystem`, `DOTween`
- UE5: `UUserWidget`, `UWidgetComponent`, `UMG Animation`, `CommonUI` plugin
- Godot 4: `Control`, `Theme`, `Container` nodes, `SceneTreeTween`
- Xbox Accessibility Guidelines (XAG), Google Material Design touch targets (48dp min)

# Related skills

- `game-design-systems` — UI displays progression, economy, and loot data
- `asset-pipeline` — UI sprite atlases and texture optimization
- `blueprint-patterns` — UE5 widget Blueprints and event binding
- `input-mapping-controller` — input device detection feeds UI icon swapping

# Failure handling

- If UI feels sluggish, profile canvas rebuild frequency (Unity) or widget tick cost (UE5) and split/pool accordingly.
- If layout breaks at extreme aspect ratios, add anchored fallback positions and test 21:9 and 32:9.
- If gamepad navigation skips elements, verify focus order and set explicit navigation targets.
- If draw calls from UI exceed budget, consolidate sprites into atlases and reduce unique materials.
