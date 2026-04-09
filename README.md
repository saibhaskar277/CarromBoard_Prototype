# 🎯 Unity Carrom Game (In Development)

A **custom-built 2D Carrom game in Unity** featuring **custom physics, precise aiming prediction, realistic pocket logic, and config-driven board setup**.

> 🚧 **Project Status: In Development**
>
> Core gameplay systems are functional, and the project is actively evolving with improved physics accuracy, turn logic, scoring, and mobile polish.

---
## 🎥 Gameplay Demo
[![Watch Demo](demo/Carromboard 2D Remake - Samplescene.png)](demo/Carromboard 2D Remake - Samplescene.mp4)

# ✨ Features

## 🎯 Precision Striker Aim
- Drag-to-aim only from striker trigger area
- Dynamic **power line renderer**
- Power color feedback
  - 🟢 Low power
  - 🟡 Medium power
  - 🔴 High power
- Accurate drag-based force control

---

## 🔵 Custom Disc Physics
Built using a **fully custom physics system**, replacing Unity Rigidbody for predictable gameplay.

### Supports
- Velocity-based movement
- Friction
- Border reflection
- Coin-to-coin force transfer
- Energy loss on collision
- Stop threshold detection
- Shared impulse system

---

## 📍 Exact Prediction Path
- Multi-bounce prediction dots
- Uses the **same custom physics logic**
- Wall reflection prediction
- Stops at first coin collision
- Shared simulation system for exact aiming feedback

---

## 🕳️ Realistic Pocket Logic
Advanced **precision hole detection system**:

- Center-based pocket capture
- High-speed rim rejection
- Fast shots require better center accuracy
- Bad-angle shots bounce back
- Perfect center shots always pocket
- Realistic competitive carrom feel

---

## 🧩 Config-Driven Architecture
Uses a generic **Config Registry + Config Manager** system.

### Current Configs
- `CarromBoardData`
- Board prefab
- Disc prefab mapping
- Striker prefab
- Queen / white / black coin setup

This allows easy swapping of:
- boards
- themes
- disc sets
- rule presets

---

## 🏗️ Board Bootstrap System
`CarromBoardSetup` handles:

- board spawn
- striker spawn
- queen spawn
- black & white ring layout
- event-driven runtime setup

---

## 🔁 Turn Reset Logic
After all coins stop:

- striker auto resets
- velocity clears safely
- turn-end event fires
- ready for multiplayer turn system

---

# 🛠️ Tech Stack
- **Unity 6**
- **C#**
- **Custom 2D Physics**
- **ScriptableObject Config System**
- **Event-driven architecture**

---

# 🚀 Planned Features
- ✅ Pocket logic
- ✅ Prediction system
- ✅ Custom physics
- 🔄 Scoring system
- 🔄 Queen cover rules
- 🔄 Player turn system
- 🔄 Foul rules
- 🔄 AI opponent
- 🔄 Online multiplayer
- 🔄 Mobile optimization
- 🔄 Sound & VFX polish
- 🔄 Board skins / themes

---

# 📂 Project Architecture
```text
Scripts
├── Physics
│   └── CustomDiscPhysics2D
├── Board
│   ├── CarromBoardSetup
│   ├── BoardController
│   └── CarromHole
├── Input
│   └── StrikerAimController
├── Config
│   ├── ConfigRegistry
│   ├── ConfigManager
│   └── CarromBoardData
└── Events
