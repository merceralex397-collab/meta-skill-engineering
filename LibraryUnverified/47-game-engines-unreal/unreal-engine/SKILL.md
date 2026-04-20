---
name: unreal-engine
description: Guides Unreal Engine 5 C++ development — UObject reflection system (UCLASS, UPROPERTY, UFUNCTION), AActor lifecycle, Gameplay Framework (GameMode, PlayerController, PlayerState), components, delegates, replication, Enhanced Input, Gameplay Ability System, asset management with soft/hard references, Blueprint-C++ boundary, build system modules, and UE memory model. Use when writing, reviewing, or architecting UE5 C++ code.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: unreal-engine
  maturity: draft
  risk: low
  tags: [unreal, ue5, cpp, game-engine, gameplay-framework]
---

# Purpose

Provide concrete C++ patterns, API usage, and architectural guidance for Unreal Engine 5 projects — covering the UObject system, gameplay framework, replication, GAS, input, asset management, and the Blueprint-C++ boundary.

# When to use this skill

- Writing or reviewing C++ classes using `UCLASS()`, `UPROPERTY()`, `UFUNCTION()` reflection macros
- Implementing the AActor lifecycle: `BeginPlay()`, `Tick()`, `EndPlay()`, component initialization
- Using the Gameplay Framework: AGameModeBase, AGameStateBase, APlayerState, APlayerController, APawn/ACharacter
- Creating components with `UActorComponent`, `USceneComponent`, `CreateDefaultSubobject<T>(TEXT("Name"))`
- Setting up delegates: `DECLARE_DYNAMIC_MULTICAST_DELEGATE`, `DECLARE_DELEGATE`, `BindDynamic()`
- Implementing replication: `UPROPERTY(Replicated)`, `GetLifetimeReplicatedProps()`, `UFUNCTION(Server, Reliable)`
- Using GAS: `UGameplayAbility`, `UGameplayEffect`, `FGameplayTag`, `UAbilitySystemComponent`
- Configuring Enhanced Input: `UInputAction`, `UInputMappingContext`, `BindAction()` in `SetupPlayerInputComponent`
- Managing assets: `TSoftObjectPtr<T>`, `FSoftObjectPath`, `StreamableManager`, async loading
- Build system: `.Build.cs` module rules, `.Target.cs`, plugin descriptors, module dependencies

# Do not use this skill when

- The task is Blueprint-only with no C++ — use `ue5-blueprint`
- The project uses Unity or Godot — use `unity` or `godot`
- The task is about narrative design or lore — use `worldbuilding-lore-systems`

# Operating procedure

1. **UObject system.** Every gameplay class derives from UObject. Use `UCLASS(Blueprintable)` for Blueprint subclassing. Mark properties with `UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Stats")` for editor/Blueprint access and GC protection. Use `UFUNCTION(BlueprintCallable)` to expose methods.
2. **Actor lifecycle.** Constructor for `CreateDefaultSubobject` and defaults only — no gameplay logic. `BeginPlay()` for initialization. `Tick()` for per-frame updates (disable with `PrimaryActorTick.bCanEverTick = false`). `EndPlay()` for cleanup.
3. **Gameplay Framework.** GameMode owns match rules (server-only). GameState holds replicated match data. PlayerState holds per-player replicated data. PlayerController handles input and UI. Pawn/Character is the physical avatar. Understand which classes exist on server, client, or both.
4. **Components.** Create in constructor: `UStaticMeshComponent* Mesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"))`. Set root with `SetRootComponent(Mesh)`. Attach children with `SetupAttachment(RootComponent)`.
5. **Delegates.** `DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnHealthChanged, float, NewHealth)` for Blueprint-compatible events. Bind with `OnHealthChanged.AddDynamic(this, &AMyActor::HandleHealthChanged)`. Use non-dynamic delegates for C++-only, higher-performance events.
6. **Replication.** Mark replicated properties: `UPROPERTY(ReplicatedUsing=OnRep_Health)`. Implement `GetLifetimeReplicatedProps` with `DOREPLIFETIME(AMyActor, Health)`. Server RPCs: `UFUNCTION(Server, Reliable)`. Client RPCs: `UFUNCTION(Client, Reliable)`. Multicast: `UFUNCTION(NetMulticast, Unreliable)`.
7. **Enhanced Input.** Create `UInputAction` and `UInputMappingContext` assets. In `SetupPlayerInputComponent`, cast to `UEnhancedInputComponent`, call `BindAction(InputAction, ETriggerEvent::Triggered, this, &AMyChar::Move)`. Add mapping context via `UEnhancedInputLocalPlayerSubsystem`.
8. **GAS.** Add `UAbilitySystemComponent` to character. Define abilities as `UGameplayAbility` subclasses. Apply effects via `UGameplayEffect`. Tag everything with `FGameplayTag`. Grant abilities with `GiveAbility(FGameplayAbilitySpec)`. Activate with `TryActivateAbilityByClass`.
9. **Asset management.** Use `TSoftObjectPtr<UStaticMesh>` for lazy loading. Resolve with `LoadSynchronous()` or async via `FStreamableManager::RequestAsyncLoad`. Hard references (`UStaticMesh*` UPROPERTY) force load — use only for always-needed assets.
10. **Blueprint-C++ boundary.** `BlueprintCallable` — C++ implements, Blueprint calls. `BlueprintImplementableEvent` — Blueprint implements, C++ calls. `BlueprintNativeEvent` — C++ provides default, Blueprint can override via `_Implementation`.
11. **Build system.** `.Build.cs` declares module dependencies (`PublicDependencyModuleNames.Add("EnhancedInput")`). `.Target.cs` configures build target. Organize into modules: core gameplay, UI, networking. Plugins get `.uplugin` descriptors.
12. **Memory.** `UPROPERTY()` prevents GC of UObject pointers. For non-UObject shared ownership use `TSharedPtr<T>`/`TWeakPtr<T>`. Use `TUniquePtr<T>` for exclusive ownership. Never use raw `new`/`delete` on UObjects — use `NewObject<T>()` and let GC handle destruction.

# Decision rules

- Every `UObject*` member must be `UPROPERTY()` or it will be garbage collected unexpectedly.
- Disable Tick on actors that don't need per-frame updates — use timers or event-driven updates instead.
- GameMode exists only on the server — never access it from client code directly.
- Use `TSoftObjectPtr` for any asset over 1MB that isn't needed at spawn time.
- Prefer `BlueprintNativeEvent` over `BlueprintImplementableEvent` when C++ needs a default implementation.
- All replicated properties need `GetLifetimeReplicatedProps` — forgetting this is the #1 replication bug.

# Output requirements

1. `Header (.h)` — class declaration with UCLASS, UPROPERTY, UFUNCTION macros, includes, forward declarations
2. `Source (.cpp)` — implementation with constructor, lifecycle, and gameplay logic
3. `Build.cs Changes` — any new module dependencies required
4. `Replication Notes` — which properties replicate, which RPCs are needed, authority model
5. `Blueprint Exposure` — which functions/properties are Blueprint-accessible and why

# References

- [UE5 Programming Guide](https://docs.unrealengine.com/5.0/en-US/programming-and-scripting-in-unreal-engine/)
- [Gameplay Framework](https://docs.unrealengine.com/5.0/en-US/gameplay-framework-in-unreal-engine/)
- [Networking and Multiplayer](https://docs.unrealengine.com/5.0/en-US/networking-and-multiplayer-in-unreal-engine/)
- [Gameplay Ability System](https://docs.unrealengine.com/5.0/en-US/gameplay-ability-system-for-unreal-engine/)
- [Enhanced Input](https://docs.unrealengine.com/5.0/en-US/enhanced-input-in-unreal-engine/)
- [UE5 API Reference](https://docs.unrealengine.com/5.0/en-US/API/)

# Related skills

- `ue5-blueprint` — Blueprint visual scripting side of the C++ boundary
- `blueprint-patterns` — reusable Blueprint architecture patterns
- `multiplayer-netcode` — advanced networking beyond basic replication
- `game-design-systems` — higher-level game system design

# Failure handling

- If a `UPROPERTY` pointer is null at runtime, check that `CreateDefaultSubobject` is in the constructor and the property is initialized.
- If replication doesn't work, verify `bReplicates = true` on the actor, `GetLifetimeReplicatedProps` includes the property, and the actor has authority.
- If Enhanced Input doesn't fire, confirm the mapping context was added to the local player subsystem and the input action asset is assigned.
- If GAS abilities fail to activate, check that the AbilitySystemComponent is initialized on `PossessedBy`/`OnRep_PlayerState` and abilities are granted.
