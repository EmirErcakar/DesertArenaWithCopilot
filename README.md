# DesertArena — Mobile Top-Down Arena Survival Shooter

A mobile portrait top-down low-poly 3D arena survival shooter built with Unity 6.3 LTS (6000.3.9f1) using Universal Render Pipeline (URP).

## Game Overview

**DesertArena** is a wave-based survival shooter set in a minimal desert environment. The player faces increasingly challenging waves of melee and ranged enemies, with bosses appearing every 5 levels. The game features auto-fire combat, an XP upgrade system, and a deep progression loop across 20 arenas with 35 levels each.

## Project Structure

```
Assets/Scripts/
├── Core/               # GameManager (state machine) and EventBus (decoupled events)
├── Player/             # PlayerController, PlayerStats, PlayerTargeting, WeaponController
├── Camera/             # CameraFollow (angled top-down, portrait orientation)
├── Enemies/            # EnemyBase, MeleeEnemy, RangedEnemy, BossEnemy, EnemySpawner
├── Projectiles/        # Projectile physics and ProjectilePool (object pooling)
├── Levels/             # LevelManager and ArenaData (ScriptableObject)
├── XP/                 # XPSystem, UpgradeSystem, UpgradeCardData
├── UI/                 # FloatingJoystick, HUDManager, UpgradeCardUI, BossCountdownUI, etc.
├── Economy/            # CoinManager, BossChestSystem, DailyStreakSystem, MetaUpgradeSystem
├── Revive/             # ReviveSystem, AdStubManager (rewarded ad stubs)
├── Skins/              # SkinData (ScriptableObject) and SkinManager
├── Progression/        # ProgressionManager (20 arenas × 35 levels)
└── Utilities/          # Singleton<T> base class
```

## Core Systems

### Combat
- **Floating Joystick**: Spawns at touch position on any non-UI area, with analog movement (deadzone + clamp radius)
- **Auto-Fire**: Player fires continuously with no manual aim or reload
- **Threat-Priority Targeting**: Boss → Ranged enemies → Nearest enemy
- **Projectiles**: Travel until hitting enemy, obstacle, or exceeding weapon range

### Enemies
- **Melee Knife** (Arena 1, Phase 1): Short range (1.5u), fast
- **Melee Sword** (Arena 1, Phase 2): Medium range (2.5u)
- **Ranged** (Arena 1, Phase 3): Maintains distance (5-7u), fires projectiles
- **Boss**: Every 5 levels, multiple attack phases, countdown overlay ("BIG BOSS SPAWNING IN 10..9..")

### Level Pacing (~120 seconds per level)
- 0-35s: Melee knife enemies only
- 35-65s: Sword enemies join
- 65s+: Ranged enemies added
- Difficulty: easy → medium → brief relief → chaos → final spike

### XP & Upgrades
- Kill-based XP (~3 upgrades per level)
- 3 upgrade cards: 1 stat + 1 weapon feature + 1 random
- Card tiers: Blue (+3), Purple (+5), Red (weapon features)
- Duplicate limits: Same stat max 2 times per run
- Reroll: 1 per level (1500 coins or ad)

### Weapon Features (unlocked via XP)
- **Pierce**: Projectiles pass through enemies
- **Multishot**: Fire multiple projectiles
- **Ricochet**: Projectiles bounce between enemies
- **Explosive Rounds**: AoE damage on impact
- **Slow On Hit**: Debuff on hit enemies

### Revive System
- Max 3 revives per level (rewarded ad flow)
- 15% coin penalty per revive
- 3-second invulnerability shield
- XP and boss HP unchanged after revive

### Economy
- **Coins**: Earned from kills, reduced 25% payout when replaying completed arenas
- **Boss Chests**: Drop from every boss (60% coins, 25% skin fragments, 15% meta tokens)
- **Daily Streak**: 15-day cycle (100→1500 coins), "Claim x2" ad option
- **Meta Upgrades**: Persistent stat boosts (MaxHP, BaseDamage, MoveSpeed, CoinBonus) with caps

### Progression
- 20 arenas × 35 levels = 700 total levels
- Complete all 35 levels in an arena to unlock the next
- Previous arenas remain playable with reduced rewards
- Each arena introduces 1 new enemy type + 1 new weapon feature

### Skins
- All skins visible but locked (goal-oriented)
- Small non-P2W boosts (8-10% total cap)
- Purchase via coins, fragments, or direct purchase

## Setup Instructions

1. Open Unity Hub and create a new project using **Unity 6.3 LTS (6000.3.9f1)** with the **Universal 3D (URP)** template
2. Copy the `Assets/Scripts/` folder into your project's `Assets/` directory
3. Set up the scene hierarchy:
   - Create a Player GameObject with `PlayerController`, `PlayerStats`, `PlayerTargeting`, and `WeaponController`
   - Add `CharacterController` component to the Player
   - Create a Canvas with `FloatingJoystick` and `HUDManager`
   - Create an empty GameObject with `GameManager`, `LevelManager`, `EnemySpawner`
   - Add `CoinManager`, `ProgressionManager`, `SkinManager` singletons
4. Create enemy prefabs with `MeleeEnemy`, `RangedEnemy`, or `BossEnemy` components
5. Create a projectile prefab with `Projectile` component and Rigidbody
6. Set up NavMesh for enemy navigation
7. Configure `ArenaData` ScriptableObjects for each arena
8. Configure `SkinData` ScriptableObjects for each skin

## Architecture Notes

- **Event-Driven**: Systems communicate through `EventBus` (static events) for loose coupling
- **Singleton Pattern**: Managers use a generic `Singleton<T>` base with `DontDestroyOnLoad`
- **Object Pooling**: `ProjectilePool` pre-instantiates and recycles projectiles
- **ScriptableObjects**: `ArenaData` and `SkinData` define data-driven content
- **Ad Stubs**: `AdStubManager` provides simulated ad callbacks (replace with real SDK for production)
- **PlayerPrefs Persistence**: Coins, progression, skins, and daily streak saved locally

## Technology

- **Engine**: Unity 6.3 LTS (6000.3.9f1)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Platform**: Mobile (Portrait orientation)
- **Language**: C# (.NET Standard)
- **Navigation**: Unity NavMesh (enemy AI)
- **UI**: Unity UI (uGUI) with TextMeshPro