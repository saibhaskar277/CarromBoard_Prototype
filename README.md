# 🏆 Carrom Board (Unity 6)

A **2D Carrom-style game prototype built in Unity 6**, featuring **custom disc physics, event-driven gameplay architecture, AI turns, scoring logic, and queen cover rules**.

This project focuses on:
- 🎯 **Predictable custom physics simulation**
- 🧠 **ScriptableObject-driven AI behavior**
- 🧩 **Clean separation of gameplay systems**
- ⚡ **Event-based game flow**
- 📱 **Expandable architecture for mobile-ready gameplay**

Built as a **gameplay systems + architecture showcase project**.

---

## ✨ Highlights

- ✅ Custom **2D disc physics**
- ✅ Accurate **shot prediction dots**
- ✅ **Human vs AI** gameplay loop
- ✅ **Queen cover & respawn rules**
- ✅ **Turn-based scoring system**
- ✅ **ScriptableObject AI tuning**
- ✅ **Event-driven UI updates**
- ✅ Clean modular architecture
- ✅ Ready for feature expansion

---

## 🛠 Tech Stack

| Item | Details |
|---|---|
| **Engine** | Unity 6 |
| **Version** | `6000.3.12f1` |
| **Language** | C# |
| **Platform** | Editor tested, Android-ready structure |
| **Architecture** | Event-driven modular systems |

---

## 🎮 Gameplay

### Player Flow
1. Move the striker along the baseline using the **UI slider**
2. Drag inside the striker **aim trigger**
3. Release to shoot
4. Pocket your assigned color
5. Cover the **queen** after pocketing it
6. Earn extra turns when valid

### AI Flow
The AI:
- Evaluates multiple shot samples
- Predicts possible pocket paths
- Scores each shot using weighted heuristics
- Adds realistic **thinking delay**
- Uses **roam + aim wobble**
- Smoothly lerps before shooting for natural behavior

---

## 🚀 Core Systems

---

### ⚙️ Custom Physics
### `CustomDiscPhysics2D`
Handles:
- velocity simulation
- friction
- collision response
- border bounce
- disc-to-disc collisions
- stopping thresholds

Designed for **stable and predictable arcade-style board physics**.

---

### 🎯 Aiming & Prediction
### `StrikerAimController`
Features:
- drag-to-aim controls
- power clamping
- shot direction handling
- prediction dots
- runtime path simulation parity

Prediction uses the same **`SimulatePath()`** logic as live gameplay.

---

### 🕳 Pocket Logic
### `CarromHole`
Implements:
- overlap detection
- center vs rim pocket checks
- high-speed alignment validation
- realistic rim bounce rejection
- precision center-based success detection

This improves realism compared to simple trigger-only pocketing.

---

### 🎮 Rules & Turn System
### `TurnGameManager`
Controls:
- player ↔ AI turn switching
- scoring
- extra turns
- wrong-color fouls
- queen cover validation
- queen respawn rules
- shot resolution

This is the **main gameplay rule orchestrator**.

---

### 🧠 AI
### `EnemyController + CarromAIConfig`
AI behavior is fully configurable through ScriptableObjects.

Configurable parameters:
- accuracy
- shot power
- sample density
- thinking delay
- aim randomness
- scoring weights
- movement smoothing

This keeps tuning fully **designer-friendly**.

---

### 📡 Event System
### `EventManager`
Centralized event bus using:
- `IGameEvent`
- gameplay events
- UI events
- board lifecycle events
- score broadcasts

This keeps systems **loosely coupled and scalable**.

---

## 🏗 Architecture Overview

```text
CarromBoardSetup
 ├── Spawns board + discs
 ├── Registers GameReferences
 └── Raises spawn events

BoardController
 ├── Baseline positions
 ├── Hole references
 └── Strike end detection

Input Layer
 ├── StrikerMoveHandler
 └── StrikerAimController

Game Flow
 └── TurnGameManager

AI Layer
 ├── EnemyController
 └── CarromAIConfig

UI Layer
 └── GameHudController
