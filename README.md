# Carrom Board (Unity)

A 2D Carrom-style game built in **Unity 6** with **custom disc physics**, **event-driven gameplay**, **human vs AI** turns, scoring, and queen (red coin) cover rules. The project focuses on predictable simulation, a clean separation between physics, rules, and UI, and a **ScriptableObject**-driven AI profile.

---

## Requirements

| Item | Version / notes |
|------|------------------|
| **Unity Editor** | **6000.3.12f1** (see `ProjectSettings/ProjectVersion.txt`) |
| **Render pipeline** | URP (project uses Universal RP) |
| **Platform** | Tested in Editor; Android build settings may appear in the repo |

Open the project folder **`CarromBoard_Prototype`** (repository root) in Unity Hub and open **`Assets/Scenes/SampleScene.unity`**.

---

## How to play

1. **Move the striker** along your baseline with the **slider** (player side uses board markers `strikerLeftPos` / `strikerRightPos`; positions are **lerped in world space** between those transforms).
2. **Aim** by dragging inside the striker **aim trigger** (not from empty board space). Release to shoot.
3. **Pocket your colour** (white for player, black for AI in the current rules) to score and earn an **extra turn** if applicable.
4. **Pocket the red coin (queen)** then **cover** by pocketing your own coin on a later shot; otherwise the queen is respotted.

---

## Features (current)

### Physics and aiming

- **`CustomDiscPhysics2D`**: velocity, friction, border bounce, disc–disc collisions, stop threshold.
- **`StrikerAimController`**: drag aim, power clamp, **prediction dots** using the same `SimulatePath` rules as runtime (stops at first coin hit for preview).

### Pockets

- **`CarromHole`**: overlap test, center vs rim behaviour, speed-dependent alignment, rim bounce when the shot is not aligned into the pocket.

### Rules and flow

- **`TurnGameManager`**: human vs AI, shot resolution from pocket events, score, extra turn when pocketing own coins, opponent score when pocketing wrong colour, queen pocket/cover/respawn logic.
- **`GameReferences`**: static registration of board, striker, and per-disc metadata (type, spawn position) without attaching identity components to every disc.

### AI

- **`CarromAIConfig`** (ScriptableObject): accuracy, power limits, prediction samples, scoring weights, **thinking delay**, baseline **roam + aim wobble**, and **smooth lerp** to the chosen shot position (no instant teleport).
- **`EnemyController`**: evaluates shots along the opponent baseline toward pockets using `SimulatePath` and config weights.

### UI

- **`GameHudController`**: listens for score / turn / player data events (see scene **`GameHUD`** under Canvas in `SampleScene`).
- Menu stack via **`UIController`** / **`UIScreen`** (optional screens).

### Events and commands

- Central **`EventManager`** with `IGameEvent` payloads (`CarromBoardEvents.cs` and related).
- **`GameCommands`**: thin command wrappers (`PositionStrikerCommand`, `ShootStrikerCommand`) that raise events so AI and humans share the same shot pipeline.

---

## Architecture (high level)

```text
CarromBoardSetup          → spawns board, striker, coins; registers discs in GameReferences; raises spawn events
BoardController           → striker range events, motion stop → strike end; hole positions for AI
StrikerMoveHandler        → slider → StrikerPositionChangedEvent (world position on baseline)
StrikerAimController      → aim, shot, prediction; obeys RequestStrikerWorldPosition / Shot / AimDirection
TurnGameManager           → rules, AI coroutine, score broadcasts
EnemyController + CarromAIConfig → shot selection
CarromHole                → DiscPocketedEvent when a disc is pocketed
GameHudController         → TMP HUD bound in scene
```

**Important:** `GameReferences` is reset when the board is cleared in `CarromBoardSetup`.

---

## Configuration assets

| Asset | Purpose |
|--------|---------|
| **`CarromBoardData`** | Board prefab, disc prefabs (red / black / white / striker). |
| **`CarromAIConfig`** | AI tuning (delay, accuracy, roam, lerp to aim, prediction weights). Assign on **`TurnGameManager`** in the scene. |

---

## Demo

A video and thumbnail may live under `demo/` (e.g. `demo/carrom-demo-720p.mp4`). Paths are relative to the repository root.

---

## Repository layout (root)

```text
CarromBoard_Prototype/
├── Assets/                 # Unity assets, scripts, scenes, prefabs
├── Packages/               # Unity package manifest
├── ProjectSettings/        # Unity project settings
├── demo/                   # Optional demo media
└── README.md
```

---

## Development status

Core loop (shoot, pockets, turns, AI, scoring, queen rules, HUD) is implemented and evolving. Polish items (audio, VFX, online play, extra foul rules) are optional follow-ups.

---

## Contributing

1. Use the Unity version above to avoid project upgrade noise.
2. Prefer **events** and small, focused scripts when extending rules or UI.
3. Keep AI tuning in **`CarromAIConfig`** rather than hard-coding in `EnemyController` or `TurnGameManager`.
