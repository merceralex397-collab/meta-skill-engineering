---
name: save-load-state
description: Implements game save/load systems including serialization, versioned save files, autosave, cloud sync, and schema migration across Unity, UE5, and Godot. Use when building save file structures, adding serialization, implementing autosave/checkpoints, migrating save formats, or integrating cloud saves. Do not use for general database persistence or backend storage.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: save-load-state
  maturity: draft
  risk: low
  tags: [save, load, serialization, persistence, cloud-save]
---

# Purpose

Provides patterns for game state serialization, versioned save files, autosave systems, cloud save integration, and schema migration across Unity, UE5, and Godot.

# When to use this skill

- Designing a save file format (what to persist, structure, versioning)
- Implementing serialization: JSON, binary, MessagePack, or protobuf for game state
- Building autosave with checkpoint triggers and corruption prevention
- Migrating save files when the game updates change the schema
- Integrating cloud saves (Steam Cloud, PlayFab, console platform APIs)
- Adding save file validation or anti-tamper measures

# Do not use this skill when

- The persistence task is a server-side database or REST API — use backend skills
- The task is about runtime game state management with no disk persistence
- Settings/preferences are the only data saved — this may be simpler than a full save system

# Operating procedure

1. **Define what to save.** Categorize state: player data (position, health, level, inventory), world state (enemy positions, door states, destructibles), quest/progression flags, and settings. Settings should be stored separately from game saves.
2. **Design the save file structure.** Use a header + payload format:
   - **Header:** save version number (integer), timestamp, playtime, checksum/hash.
   - **Payload:** serialized game state.
   - **Format choice:** JSON for debugging and modding support. Binary (MessagePack, protobuf, or `BinaryFormatter` alternatives) for production — smaller, faster, harder to tamper with.
3. **Implement engine-specific serialization.**
   - **Unity:** Use `JsonUtility.ToJson()` for simple objects. For complex graphs, use Newtonsoft JSON or a binary serializer. Avoid `BinaryFormatter` (security risk). Use `PlayerPrefs` only for non-critical settings, never for game saves. Write to `Application.persistentDataPath`.
   - **UE5:** Create a `USaveGame` subclass with `UPROPERTY(SaveGame)` fields. Save with `UGameplayStatics::SaveGameToSlot(SaveObj, SlotName, UserIndex)`. Load with `LoadGameFromSlot()`. For advanced needs, use `FArchive` with `FMemoryWriter`/`FMemoryReader`.
   - **Godot:** Use `FileAccess.open("user://save.json", FileAccess.WRITE)`. Serialize with `JSON.stringify(data)`. For binary, use `store_var()`. For config-style saves, use `ConfigFile`. Resources can be saved with `ResourceSaver.save()`.
4. **Version your save format.** Include a `save_version: int` field. On load, check the version and run migration functions sequentially: `if version < 2: migrate_v1_to_v2(data)`. Never delete fields — deprecate them and add new ones.
5. **Implement autosave.** Trigger on checkpoints, zone transitions, or timed intervals (every 3–5 min). Write to a rotating slot (autosave_0, autosave_1) to prevent corruption from mid-write crashes. Perform serialization and disk I/O on a background thread where possible.
6. **Prevent corruption.** Write to a temp file first, then rename atomically. Verify checksum on load. Keep one backup save slot. If the primary save fails validation, attempt to load the backup.
7. **Integrate cloud saves.** Use platform APIs: Steam `ISteamRemoteStorage`, PlayFab Entity Files, console-specific APIs. Handle conflict resolution: compare timestamps or playtime, prompt the player to choose, or merge non-conflicting fields.
8. **Secure competitive saves.** For games with leaderboards or competitive elements: add HMAC validation, encrypt save payload, validate values server-side on upload. For single-player, light validation (checksum) is usually sufficient.

# Decision rules

- JSON for development and moddable games; binary for production and competitive games.
- Always version save files from day one — migration is inevitable.
- Separate settings from game saves — settings persist across save slots.
- Autosave to a rotating slot, never overwrite the player's manual save.
- Write-then-rename for atomic saves; never write in-place.
- Test save/load with saves from every previous version during QA.

# Output requirements

1. `Save Schema` — data structure with field types, version number, what each section contains
2. `Serialization Code` — engine-specific implementation for write and read
3. `Migration Plan` — version upgrade path with migration functions
4. `Validation` — test: save → load round-trip, corrupt file handling, version migration

# References

- Unity persistent data: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
- UE5 SaveGame: https://docs.unrealengine.com/5.3/en-US/saving-and-loading-your-game-in-unreal-engine/
- Godot File I/O: https://docs.godotengine.org/en/stable/tutorials/io/saving_games.html
- Steam Cloud: https://partner.steamgames.com/doc/features/cloud

# Related skills

- `input-mapping-controller` — persisting rebinding preferences alongside settings
- `performance-profiling-games` — async save I/O to avoid frame hitches
- `multiplayer-netcode` — saving/replaying multiplayer match state
- `game-design-systems` — determining what game state matters for progression

# Failure handling

- If save file is corrupted on load, attempt backup slot before reporting failure to the player.
- If schema migration fails, log the version mismatch and offer to start fresh rather than crashing.
- If cloud save conflicts arise, always prompt the player — never silently overwrite.
- If save I/O causes frame hitches, move serialization to a background thread and show a save indicator.
