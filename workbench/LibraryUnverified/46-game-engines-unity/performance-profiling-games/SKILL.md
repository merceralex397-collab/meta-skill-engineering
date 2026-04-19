---
name: performance-profiling-games
description: Diagnoses and resolves game performance bottlenecks using frame budgets, CPU/GPU profilers, draw call analysis, memory tracking, and engine-specific tools like Unity Profiler, Unreal Insights, and RenderDoc. Use when investigating frame drops, high draw calls, memory spikes, or platform-specific perf issues. Do not use for general backend performance tuning.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: performance-profiling-games
  maturity: draft
  risk: low
  tags: [performance, profiling, gpu, cpu, frame-budget, optimization]
---

# Purpose

Provides systematic procedures for profiling game performance — frame time analysis, CPU/GPU bottleneck diagnosis, draw call reduction, memory optimization, and engine-specific tooling.

# When to use this skill

- Frame rate drops below target (60fps = 16.67ms budget, 30fps = 33.33ms budget)
- Investigating whether the game is CPU-bound or GPU-bound
- Reducing draw calls, overdraw, or shader complexity
- Diagnosing memory spikes, GC pauses, or texture memory overflow
- Optimizing for specific platforms: mobile thermal throttling, console memory limits, VR frame timing
- Setting up LOD, occlusion culling, or instancing systems

# Do not use this skill when

- The performance issue is in a web backend or API server — use backend profiling skills
- The task is about asset creation pipeline (not runtime performance) — prefer `asset-pipeline`
- The optimization is purely algorithmic with no rendering or engine component

# Operating procedure

1. **Establish the frame budget.** Define the target: 16.67ms for 60fps, 33.33ms for 30fps, 11.11ms for 90fps VR. All optimization work must reference this budget.
2. **Determine CPU vs GPU bound.** If reducing render resolution doesn't improve FPS → CPU-bound. If simplifying game logic doesn't help → GPU-bound. Use engine tools to confirm:
   - **Unity:** Open Profiler (Window → Analysis → Profiler). Check CPU and GPU timing in the frame breakdown. Look for `PlayerLoop`, `Rendering`, `Scripts` categories.
   - **UE5:** Use Unreal Insights (`-trace=default` launch arg). Check `stat unit` for Game, Draw, GPU thread times. `stat scenerendering` for draw call counts.
   - **Godot:** Enable Debug → Monitors. Check Physics Process, Process, and render times.
3. **Profile the GPU.** Use RenderDoc for draw-call-level analysis on any engine. NVIDIA Nsight for NVIDIA-specific optimization. Xcode GPU Debugger for Apple platforms. Look for overdraw (too many overlapping translucent layers), excessive fill rate, and expensive shader passes.
4. **Reduce draw calls.** Batch static geometry (Unity: Static Batching, SRP Batcher; UE5: Nanite, ISM/HISM). Use GPU instancing for repeated meshes. Merge materials where possible. Target <2000 draw calls on mobile, <5000 on PC.
5. **Implement LOD and culling.** Set up mesh LOD chains (3–4 levels). Use HLOD for distant geometry clusters. Enable frustum culling (usually automatic). Add occlusion culling volumes for interior scenes. Use impostor sprites for very distant foliage.
6. **Optimize shaders.** Minimize texture fetches per pixel. Avoid dynamic branching in fragment shaders. Use half-precision floats where possible. Pre-compute values in vertex shader when they don't vary per-pixel.
7. **Manage memory.** Track texture memory (often the largest consumer). Compress textures (BC/ASTC formats). Use texture streaming. Reduce mesh vertex counts at lower LODs. In Unity, monitor GC allocations — avoid `new` in `Update()`; use object pooling.
8. **Address platform-specific issues.** Mobile: monitor thermal throttling, reduce sustained GPU load, use adaptive resolution. Console: respect fixed memory budgets, use async compute. VR: maintain frame timing or reprojection kicks in — single-pass stereo, foveated rendering.
9. **Validate with profiling tools.** Tracy profiler for custom C/C++ engines. Unity Profiler with Deep Profile for per-method timing. UE5 `stat` commands and Insights for thread-level analysis. Always profile release builds, not editor/debug builds.

# Decision rules

- Always profile before optimizing — never guess at bottlenecks.
- Fix the biggest bottleneck first; re-profile after each change.
- CPU-bound → reduce script complexity, physics objects, or draw call submission. GPU-bound → reduce resolution, shader complexity, or overdraw.
- GC spikes (Unity) → pool objects, cache references, avoid allocations in hot paths.
- If frame time is inconsistent (spikes), look for GC, asset loading, or physics step spikes rather than average load.
- Profile on the target hardware, not just the development machine.

# Output requirements

1. `Profiling Summary` — target FPS, measured frame time, CPU vs GPU bound determination
2. `Bottleneck Analysis` — ranked list of bottlenecks with measured impact (ms saved)
3. `Optimization Plan` — specific changes with expected frame time improvement
4. `Validation` — before/after profiler captures confirming improvement

# References

- Unity Profiler: https://docs.unity3d.com/Manual/Profiler.html
- Unreal Insights: https://docs.unrealengine.com/5.3/en-US/unreal-insights-in-unreal-engine/
- RenderDoc: https://renderdoc.org/docs/
- Tracy Profiler: https://github.com/wolfpld/tracy

# Related skills

- `asset-pipeline` — optimizing asset formats and import settings
- `input-mapping-controller` — input latency as a performance concern
- `multiplayer-netcode` — tick rate and bandwidth optimization
- `save-load-state` — async loading and streaming during gameplay

# Failure handling

- If profiling tools are unavailable, use engine built-in stats overlays (`stat fps`, Profiler, Monitors) as a baseline.
- If the bottleneck is unclear, bisect: disable systems one at a time and re-profile.
- If optimization breaks visual quality, use A/B screenshots and document the tradeoff.
- If the target frame budget is unachievable, recommend reducing scope (draw distance, entity count) with data to support the recommendation.
