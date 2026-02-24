# DesertArena

A mobile portrait top-down low-poly 3D arena survival shooter built in **Unity 6.3 LTS (6000.3.9f1)** with the Universal Render Pipeline (URP).

## Quick Start

1. Open Unity Hub → **Open** → select this repository root
2. Unity version: `6000.3.9f1` (URP template)
3. Read **[SETUP_GUIDE.md](SETUP_GUIDE.md)** for full scene setup, prefab wiring, and Inspector configuration
4. Follow the **Day 1–3 Roadmap** in the guide to reach a playable prototype

## Feature Overview

| System | Status |
|---|---|
| Floating joystick (spawn anywhere, non-UI) | ✅ Complete |
| Analog movement with deadzone + clamp | ✅ Complete |
| Angled top-down camera follow | ✅ Complete |
| Auto-fire with threat-priority targeting | ✅ Complete |
| Projectile with range / obstacle / enemy collision | ✅ Complete |
| HealthComponent + floating world-space HP bar | ✅ Complete |
| Melee / Sword / Ranged enemies | ✅ Complete |
| Boss (melee + burst-fire phases) | ✅ Complete |
| Wave-based spawner (Arena 1 Level 1 timing) | ✅ Complete |
| XP system + 3-4 upgrade triggers per level | ✅ Complete |
| Upgrade cards (flip animation, Blue/Purple/Red tiers) | ✅ Complete |
| Controlled RNG: 1 stat + 1 feature + 1 random | ✅ Complete |
| Reroll (1×/level, coin cost or rewarded ad) | ✅ Complete |
| Revive system (max 3/level, ad-gated, coin penalty) | ✅ Complete |
| Boss chest + reward table + "Open ×2 Ad" stub | ✅ Complete |
| Ad service interface + stub | ✅ Complete |
| Coin tracking (session + persistent) | ✅ Complete |
| Boss countdown UI (10-second overlay) | ✅ Complete |

## Scripts

```
Assets/Scripts/
├── Ads/        IAdService.cs  AdServiceStub.cs
├── Economy/    CoinManager.cs  ReviveManager.cs  ChestRewardSystem.cs
├── Enemies/    EnemyBase.cs  MeleeEnemy.cs  SwordEnemy.cs  RangedEnemy.cs  BossEnemy.cs
├── Health/     HealthComponent.cs  FloatingHealthBar.cs
├── Input/      FloatingJoystick.cs
├── Managers/   GameManager.cs
├── Player/     PlayerController.cs  CameraFollow.cs
├── UI/         XPBarUI.cs  UpgradeCard.cs  UpgradeScreenUI.cs
│               BossCountdownUI.cs  ReviveUI.cs  ChestUI.cs
├── Waves/      WaveManager.cs  EnemySpawner.cs
└── XP/         XPSystem.cs  UpgradeDefinition.cs  UpgradeManager.cs
```

## Architecture Notes

- **Singletons** (`GameManager`, `XPSystem`, `CoinManager`, etc.) use `DontDestroyOnLoad` only on `GameManager`. Others are scene-scoped and reset each level.
- **Events** connect systems loosely (e.g. `HealthComponent.OnDeath`, `XPSystem.OnUpgradeTriggered`).
- **No per-frame allocations** in hot paths — `TargetSelector` uses a pre-allocated `Collider[32]` buffer.
- **Ads** are fully stubbed via `IAdService` / `AdServiceStub`; swap for real SDK without changing callers.
- **UpgradeDefinition** is a `ScriptableObject` — create any number and configure in the Inspector.
