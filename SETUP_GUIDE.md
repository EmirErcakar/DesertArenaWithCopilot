# Desert Arena — Setup Guide

## Project Settings (Unity 6.3 LTS / 6000.3.9f1)

### 1. Open / Create the Project
1. Open Unity Hub → **New Project**
2. Template: **Universal 3D (URP)**
3. Project name: `DesertArena`
4. Unity version: `6000.3.9f1`
5. Create project then **copy all folders from this repository into the new project root**.

---

## 2. Build Settings — Portrait Mobile
`File → Build Settings → Android` (or iOS)
- **Orientation**: Portrait
- `Player Settings → Resolution and Presentation → Default Orientation`: Portrait

---

## 3. Physics Layers
`Edit → Project Settings → Tags and Layers`

| Layer # | Name              |
|---------|-------------------|
| 6       | Player            |
| 7       | Enemy             |
| 8       | Obstacle          |
| 9       | PlayerProjectile  |
| 10      | EnemyProjectile   |

**Layer Collision Matrix** (`Edit → Project Settings → Physics → Layer Collision Matrix`):
- `PlayerProjectile` ↔ `Enemy` ✅
- `PlayerProjectile` ↔ `Obstacle` ✅
- `EnemyProjectile` ↔ `Player` ✅
- `EnemyProjectile` ↔ `Obstacle` ✅
- All other inter-category collisions: ❌ (uncheck to avoid unwanted hits)

---

## 4. Scene Hierarchy — Arena1_Level1

```
Scene: Arena1_Level1
│
├── [CAMERA]
│    └── Main Camera
│         └── CameraFollow (script)
│
├── [LIGHTING]
│    └── Directional Light
│
├── [ARENA]
│    ├── Ground           Plane, scale (20, 1, 20), Layer: Default
│    ├── Wall_North       BoxCollider, invisible, Layer: Obstacle
│    ├── Wall_South       BoxCollider, invisible, Layer: Obstacle
│    ├── Wall_East        BoxCollider, invisible, Layer: Obstacle
│    ├── Wall_West        BoxCollider, invisible, Layer: Obstacle
│    └── Obstacles        static cylinders/cubes, Layer: Obstacle
│
├── [PLAYER]
│    └── Player           Tag: "Player", Layer: Player
│         ├── Components: Rigidbody, CapsuleCollider, PlayerController,
│         │               TargetSelector, WeaponController, HealthComponent
│         ├── FirePoint    (empty child, offset forward ~0.5 units)
│         └── HealthBar    (world-space canvas, FloatingHealthBar script)
│
├── [SPAWN POINTS]
│    ├── SpawnPoint_NE
│    ├── SpawnPoint_NW
│    ├── SpawnPoint_SE
│    └── SpawnPoint_SW
│
├── [SPAWNERS]
│    ├── KnifeSpawner     EnemySpawner (MeleeEnemy prefab, all 4 spawn points)
│    ├── SwordSpawner     EnemySpawner (SwordEnemy prefab)
│    ├── RangedSpawner    EnemySpawner (RangedEnemy prefab)
│    └── BossSpawner      EnemySpawner (BossEnemy prefab, centre spawn)
│
├── [WAVE MANAGER]
│    └── WaveManager      (WaveManager script)
│         Phase 0: startTime=0,  endTime=35,  spawners=[KnifeSpawner]
│         Phase 1: startTime=35, endTime=65,  spawners=[KnifeSpawner, SwordSpawner]
│         Phase 2: startTime=65, endTime=0,   spawners=[KnifeSpawner, SwordSpawner, RangedSpawner]
│
├── [MANAGERS]
│    └── Managers         (single empty GameObject)
│         Components: GameManager, CoinManager, XPSystem, UpgradeManager,
│                     ReviveManager, ChestRewardSystem, AdServiceStub
│
└── [UI CANVAS]
     └── HUDCanvas        Canvas (Screen Space - Overlay), CanvasScaler
          ├── XPBar            (XPBarUI) — top centre, thin yellow bar
          ├── JoystickPanel    (FloatingJoystick) — full screen, alpha 0
          │    └── JoystickContainer  (outer circle, inactive by default)
          │         └── Knob         (inner circle)
          ├── UpgradeScreen    (UpgradeScreenUI) — hidden by default
          │    ├── DarkOverlay
          │    ├── Card1 (UpgradeCard)
          │    ├── Card2 (UpgradeCard)
          │    ├── Card3 (UpgradeCard)
          │    └── RerollButton
          ├── BossCountdown    (BossCountdownUI) — hidden by default
          ├── ReviveUI         (ReviveUI) — hidden by default
          └── ChestUI          (ChestUI) — hidden by default
```

---

## 5. Prefab Setup

### Player Prefab
1. Create Capsule → rename `Player`
2. Add components: `Rigidbody`, `CapsuleCollider`, `PlayerController`, `TargetSelector`,
   `WeaponController`, `HealthComponent`
3. Rigidbody: `Constraints → Freeze Rotation X, Z`; Collision Detection: **Continuous**
4. Set layer to **Player**, tag to **Player**
5. Create empty child `FirePoint` — position it at the front (Z +0.5)
6. For HealthBar: add child `HealthBar_WS`, add `Canvas` (World Space, sort order 1),
   add `FloatingHealthBar` script. Assign fill image.

### PlayerProjectile Prefab
1. Create Sphere → scale (0.15, 0.15, 0.15)
2. Add `Rigidbody` (Is Kinematic = true, no gravity), `SphereCollider` (isTrigger = true)
3. Add `Projectile` script → assign `enemyLayer` (Enemy) and `obstacleLayer` (Obstacle)
4. Set layer to **PlayerProjectile**
5. Add URP Lit material → colour bright yellow/white

### EnemyProjectile Prefab (used by RangedEnemy + BossEnemy)
1. Same as above but smaller (0.1, 0.1, 0.1)
2. `Projectile` script → `enemyLayer` = Player, `obstacleLayer` = Obstacle
3. Set layer to **EnemyProjectile**
4. Red/orange material

### MeleeEnemy Prefab
1. Capsule → scale (0.8, 0.8, 0.8), red-ish URP material
2. Layer: **Enemy**, Tag: **Enemy**
3. Components: `Rigidbody` (no gravity, freeze rotation), `CapsuleCollider`, `HealthComponent`,
   `MeleeEnemy`, `FloatingHealthBar`
4. Inspector values: moveSpeed=2.5, attackRange=1.0, attackDamage=8, attackCooldown=1.0,
   xpReward=8, coinReward=3

### SwordEnemy Prefab
1. Capsule (slightly taller), orange material
2. Same as MeleeEnemy but: moveSpeed=2.2, attackRange=1.8, attackDamage=12,
   attackCooldown=1.2, xpReward=12, coinReward=5

### RangedEnemy Prefab
1. Capsule, purple material
2. Add `FirePoint` child at front
3. Components: `HealthComponent`, `RangedEnemy`, `FloatingHealthBar`
4. Inspector: moveSpeed=2.0, attackRange=8, attackDamage=6, attackCooldown=2.0,
   preferredDistance=5, projectileSpeed=7
5. Assign `enemyProjectilePrefab`

### BossEnemy Prefab
1. Capsule, scale (2, 2, 2), dark red material
2. `HealthComponent` maxHealth = 500
3. Components: `BossEnemy`, `FloatingHealthBar`
4. Inspector: moveSpeed=3.5, attackRange=2.0, attackDamage=20, attackCooldown=0.8,
   rangedPhaseDistance=4.0, phaseSwitchInterval=5.0, projectileSpeed=6.0
5. Assign `enemyProjectilePrefab`, create `FirePoint` child

---

## 6. Upgrade Definitions (ScriptableObjects)

Create via: Right-click in Project → **Create → DesertArena → Upgrade Definition**

Suggested starter set:

| Name              | Type    | Stat         | Tier   | Value | maxTimesPerRun |
|-------------------|---------|--------------|--------|-------|----------------|
| Attack Boost I    | Stat    | Attack       | Blue   | 3     | 2              |
| Attack Boost II   | Stat    | Attack       | Purple | 5     | 1              |
| Attack Speed I    | Stat    | AttackSpeed  | Blue   | 0.5   | 2              |
| Attack Speed II   | Stat    | AttackSpeed  | Purple | 1.0   | 1              |
| Move Speed I      | Stat    | MoveSpeed    | Blue   | 0.5   | 2              |
| Move Speed II     | Stat    | MoveSpeed    | Purple | 1.0   | 1              |
| Max Health I      | Stat    | Health       | Blue   | 20    | 2              |
| Max Health II     | Stat    | Health       | Purple | 40    | 1              |
| Piercing Shot     | Feature | None         | Red    | 1     | 1              |
| Multi-Shot        | Feature | None         | Red    | 1     | 1              |

Drag all of them into `UpgradeManager.allUpgrades` in the Inspector.

---

## 7. ChestRewardSystem Reward Table

Select the `Managers` GameObject, find `ChestRewardSystem`, and add entries:

| Name            | Type          | Min | Max | Weight |
|-----------------|---------------|-----|-----|--------|
| Coins           | Coins         | 200 | 500 | 60     |
| Skin Fragments  | SkinFragments | 1   | 3   | 30     |
| Meta Token      | MetaToken     | 1   | 1   | 10     |

---

## 8. Camera Setup
Select `Main Camera`:
- Attach `CameraFollow` script
- Set **offset** to `(0, 14, -8)`
- Set **lookDownAngle** to `60`
- Set **smoothSpeed** to `8`

---

## 9. UI Canvas Setup
1. Create `Canvas`: Render Mode = **Screen Space – Overlay**
2. Add `Canvas Scaler`: Scale With Screen Size, Reference = `1080 × 1920`, Match = `0.5`
3. Add `GraphicRaycaster`

### XP Bar
- Full-width Image (anchor: top stretch, height ~10 px)
- Background: dark grey; Fill: yellow Image (Fill Method = Horizontal)
- Attach `XPBarUI`, drag fill Image

### Joystick Panel
- Full-screen transparent Image (Raycast Target = **true**)
- Attach `FloatingJoystick` script
- Create child `JoystickContainer` (outer circle Image, alpha 0.4, inactive)
  - Child: `Knob` (inner circle Image)
- Assign references in Inspector

### Upgrade Screen
- Full-screen panel, dark overlay Image (alpha 0.8), hidden by default
- 3 × `UpgradeCard` prefab children
- `RerollButton` at bottom
- Attach `UpgradeScreenUI`

---

## 10. Day 1–3 Roadmap to Playable Core

### Day 1 — Core Movement & Shooting
- [ ] Create scene with ground, walls, player
- [ ] Set up Canvas with joystick panel
- [ ] Confirm player moves with joystick
- [ ] Add 1 MeleeEnemy manually in scene
- [ ] Confirm auto-fire shoots at it and it dies

### Day 2 — Waves & XP
- [ ] Set up spawn points and spawners
- [ ] Configure WaveManager with 3 phases
- [ ] Add XP bar + verify it fills on kills
- [ ] Show upgrade screen with 3 cards; pick a card; verify game resumes

### Day 3 — Boss & Polish
- [ ] Enable isBossLevel on WaveManager; add BossSpawner
- [ ] Verify boss countdown, boss spawns, boss dies → ChestUI shows
- [ ] Add ReviveUI; kill player → revive prompt appears; watch ad → revive
- [ ] Play-test full 120 s level

---

## Folder Structure

```
Assets/
├── Scripts/
│   ├── Ads/            IAdService.cs, AdServiceStub.cs
│   ├── Economy/        CoinManager.cs, ReviveManager.cs, ChestRewardSystem.cs
│   ├── Enemies/        EnemyBase.cs, MeleeEnemy.cs, SwordEnemy.cs,
│   │                   RangedEnemy.cs, BossEnemy.cs
│   ├── Health/         HealthComponent.cs, FloatingHealthBar.cs
│   ├── Input/          FloatingJoystick.cs
│   ├── Managers/       GameManager.cs
│   ├── Player/         PlayerController.cs, CameraFollow.cs
│   ├── UI/             XPBarUI.cs, UpgradeCard.cs, UpgradeScreenUI.cs,
│   │                   BossCountdownUI.cs, ReviveUI.cs, ChestUI.cs
│   ├── Waves/          WaveManager.cs, EnemySpawner.cs
│   └── XP/             XPSystem.cs, UpgradeDefinition.cs, UpgradeManager.cs
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Projectiles/
│   └── UI/
├── Scenes/
│   └── Arena1_Level1.unity
├── Materials/          (URP Lit materials)
├── Settings/           (URP Pipeline Asset, URP Renderer)
└── UI/                 (sprites, icons for cards)
```
