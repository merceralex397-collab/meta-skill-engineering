---
name: asset-pipeline
description: Manages game asset import, compression, bundling, streaming, and build optimization. Use when configuring texture compression (ASTC/BC7/ETC2), LOD generation, asset bundles, Addressables, streaming systems, audio formats, or reducing build size in Unity, Unreal, or Godot.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: asset-pipeline
  maturity: draft
  risk: low
  tags: [asset, pipeline, compression, bundling, lod, streaming]
---

# Purpose

Configure and optimize the full game asset pipeline: import settings, texture compression, mesh LOD, asset bundling and streaming, audio management, build size reduction, and version control for large binaries.

# When to use this skill

- Configuring texture import settings (compression format, max size, mipmaps)
- Setting up asset bundling (Unity Addressables/AssetBundles, UE5 Asset Manager, Godot PCK)
- Implementing streaming or lazy loading for open-world scenes
- Optimizing build size with asset stripping and platform-specific variants
- Managing audio assets (format selection, streaming vs preload decisions)
- Establishing asset naming conventions and folder structures
- Setting up Git LFS or Perforce for binary asset version control

# Do not use this skill when

- The task is about gameplay logic or systems design — use `game-design-systems`
- The task is about UI layout or HUD elements — use `game-ui-hud`
- The task is purely about shader/material authoring — use `shader-vfx`
- The task is about runtime rendering performance without asset changes — use `performance-profiling-games`

# Operating procedure

1. **Audit current asset inventory**: list asset types (textures, meshes, audio, animations), count, total size, and target platforms. Identify the largest contributors to build size.
2. **Configure texture compression per platform**:
   - Mobile: ASTC 6x6 (balanced) or ASTC 8x8 (smaller, lower quality). Fallback: ETC2 for older Android.
   - Desktop/Console: BC7 (high quality, good compression) or BC3/DXT5 for alpha textures.
   - Set max texture size: 2048 for hero assets, 1024 for props, 512 for distant/tiling.
   - Enable mipmaps for 3D textures; disable for UI sprites.
3. **Generate mesh LODs**: create 3–4 LOD levels per mesh. LOD0 = full detail, LOD1 = 50% triangles at 15m, LOD2 = 25% at 30m, LOD3 = 10% at 60m. Use engine auto-LOD (UE5 Nanite for supported meshes, Unity LODGroup, Godot `MeshInstance3D.lod_bias`).
4. **Set up asset bundling and loading**:
   - Unity: use Addressables with group labels by scene/area. Load via `Addressables.LoadAssetAsync<T>(key)`. Release with `Addressables.Release(handle)`.
   - UE5: use `FStreamableManager::RequestAsyncLoad` with `FSoftObjectPath`. Register primary assets in Asset Manager.
   - Godot: use `ResourceLoader.load_threaded_request()` for async loading; export PCK files for DLC.
5. **Configure audio pipeline**: use OGG Vorbis for music (streaming, ~128kbps), WAV/ADPCM for short SFX (preloaded), set audio import to mono for non-spatial sounds, stereo for music.
6. **Optimize build size**: strip unused assets with engine tools (`Resources.UnloadUnusedAssets()` in Unity, `Cook Only Maps` in UE5). Compress with LZ4/Zstd. Create platform-specific asset variants (lower res for mobile).
7. **Enforce naming and folder conventions**: `Assets/Textures/Characters/T_Hero_Albedo.png`, `Assets/Audio/SFX/SFX_Footstep_Dirt.ogg`. Prefix by type: `T_` textures, `M_` materials, `SM_` static mesh, `SK_` skeletal mesh, `SFX_` sound effects.
8. **Configure version control**: put all binary assets in Git LFS (`.gitattributes`: `*.png filter=lfs`, `*.fbx filter=lfs`, `*.wav filter=lfs`). For teams >5, consider Perforce for locking and large binary performance.

# Decision rules

- Choose compression format based on target platform first, quality second — a wrong format causes runtime decompression or GPU incompatibility.
- Prefer Addressables/Asset Manager over raw `Resources.Load<T>()` — Resources folder forces all assets into the build.
- Stream music and ambient audio; preload SFX under 500KB.
- Use texture atlases and sprite sheets to reduce draw calls for 2D games (target < 4 atlases per scene).
- Generate LODs for any mesh with >5K triangles visible in-game.
- Never commit uncompressed PSD/TIFF source files to the game repo — keep sources in a separate art repo or drive.

# Output requirements

1. `Asset Audit` — inventory of asset types, sizes, and platform targets
2. `Import Settings` — compression format, max size, mipmap config per asset category
3. `Bundling Strategy` — group structure, loading/unloading lifecycle, memory budget
4. `Naming Convention` — prefix rules, folder hierarchy, examples
5. `Build Report` — before/after build size, draw call counts, load time measurements
6. `VCS Config` — `.gitattributes` for LFS or Perforce workspace mapping

# References

- Unity: `Addressables`, `AssetBundle`, `TextureImporter`, `AudioImporter`, `EditorBuildSettings`
- UE5: `FStreamableManager`, `UAssetManager`, `UTexture2D::LODBias`, Nanite virtual geometry
- Godot 4: `ResourceLoader`, `PCKPacker`, `ImporterSettings`, `ResourceSaver`
- Git LFS documentation: track patterns, lock support, storage quotas

# Related skills

- `performance-profiling-games` — draw call budgets, memory profiling for assets
- `game-ui-hud` — UI texture atlasing and sprite sheet setup
- `shader-vfx` — material/texture dependencies in the pipeline
- `blueprint-patterns` — UE5 soft references and async loading in Blueprints

# Failure handling

- If build size exceeds target, run asset audit to find the top 10 largest assets and apply aggressive compression or resolution reduction.
- If async loading causes hitches, profile load times and add preload hints for critical paths.
- If Git LFS storage quota is reached, migrate to Perforce or prune unused tracked files with `git lfs prune`.
- If texture compression artifacts are visible, step up quality (e.g., ASTC 4x4 instead of 8x8) for affected assets only.
