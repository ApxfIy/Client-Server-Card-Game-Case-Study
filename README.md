# War Card Game — Unity 2D

A 2D implementation of the classic **War** card game for two players, built with Unity 2022.3.21 and C#. The player competes against a computer opponent through a Fake Multiplayer architecture that simulates client-server communication.

---

## Features

- Full **War** card game rules: standard 52-card deck, war resolution, early-game-over on insufficient cards
- **Fake Multiplayer** — all game logic runs in a simulated server (`FakeWarServer`) with async RPC-style calls, random network errors, and timeouts
- **Client retry logic** — the client automatically retries failed requests (3 attempts, 600 ms apart) and surfaces network status to the player
- **Game state persistence** — the server saves state to disk after every round; the game resumes from where it left off on the next launch
- **Animated card movements** — all card transitions use DOTween sequences exposed as UniTask for clean async/await orchestration
- **War animation** — face-down war cards are dealt sequentially before the final face-up reveal
- **Reshuffle animation** — when a player's hand runs out, captured cards are visually reshuffled back into their deck
- **Debug presets** — inspector-selectable scenarios for testing specific game states without replaying the whole game
- **Color-coded logging** — server logs (orange) and client logs (blue) are visually distinguished in the Unity console

---

## Architecture

The project is divided into three strict layers. Client and Server share only a thin Shared layer of enums and DTOs.

```
Assets/Scripts/
├── Client/          # UI, input, animations, server communication
│   ├── Animation/
│   │   └── TweenExtensions.cs      — DOTween → UniTask bridge (extension methods)
│   ├── Views/
│   │   ├── GameEntryPoint.cs       — MonoBehaviour bootstrap: wires all services, starts game
│   │   ├── GameController.cs       — Gameplay loop: input → server call → animate result
│   │   ├── GameBoard.cs            — Visual root: owns all card containers, orchestrates deals
│   │   ├── BattleTableView.cs      — Battle area: player slot, opponent slot, war pot
│   │   ├── CardsContainerView.cs   — Generic animated card pile (hand, captured, slot, war)
│   │   ├── CardView.cs             — Single card: face-up/face-down flip, rank display, click
│   │   └── CardDeck.cs             — Pool of pre-instantiated card objects for deals
│   ├── GameClient.cs               — Async client: calls FakeWarServer, handles retries/errors
│   └── InputManager.cs             — Tap/click detection, fires OnInput event
│
├── Server/          # Game simulation — no Unity UI dependencies
│   ├── FakeWarServer.cs            — Full War rules engine + network simulation + persistence
│   ├── IDealStrategy.cs            — Strategy interface for initial card distribution
│   ├── RandomDealStrategy.cs       — Default: shuffled deck dealt to both players
│   ├── OneRoundWinStrategy.cs      — Preset: one player wins round 1 decisively
│   ├── TripleWarStrategy.cs        — Preset: three consecutive ties before resolution
│   ├── WarInsufficientCardsStrategy.cs — Preset: war triggered when a player has too few cards
│   ├── SavedGameState.cs           — JSON-serializable full game state snapshot
│   └── DebugPreset.cs              — Enum selecting which IDealStrategy to use
│
└── Shared/          # Minimal shared code — only enums and data models
    ├── CardRank.cs                 — Enum: Two (2) … Ace (13)
    ├── Enums.cs                    — GameResult, ResponseStatus, CompareResult
    ├── NetworkModels.cs            — DTOs: StartGameResponse, PlayRoundResponse
    ├── Extensions.cs               — RankToString, Fisher-Yates Shuffle, GetRandomItem
    └── GameLogger.cs               — Color-coded static logger (Server / Client)
```

### Data flow

```
Player tap
    └─► InputManager.OnInput
            └─► GameController.PlayRoundAsync()
                    ├─► GameClient.PlayRoundAsync()          (retry wrapper)
                    │       └─► FakeWarServer.PlayRoundAsync()
                    │               ├── simulate network delay (150–500 ms)
                    │               ├── random NetworkError (5%) / Timeout (2%)
                    │               ├── apply War game logic
                    │               ├── persist SavedGameState to disk
                    │               └── return PlayRoundResponse
                    └─► GameBoard  (animate result: deal, reveal, collect, reshuffle)
```

### Design patterns used

| Pattern | Where |
|---|---|
| **Strategy** | `IDealStrategy` — swappable card-deal algorithms for testing |
| **Observer** | `InputManager.OnInput`, `CardsContainerView.OnClick`, `GameClient.OnNetworkStatusChanged` |
| **Repository** | `FakeWarServer` saves/loads JSON game state via `SavedGameState` |
| **Service** | `GameClient` and `FakeWarServer` are plain C# classes injected via constructors |
| **Object Pool** | `CardDeck` pre-instantiates card prefabs and hands them out on demand |

---

## Configuring the Game via Unity Inspector

### GameEntryPoint (scene root object)

| Field | Type | Description |
|---|---|---|
| `Board` | `GameBoard` | Reference to the visual game board |
| `Debug Preset` | `DebugPreset` (enum) | Selects a deal strategy for testing (see below) |

**Debug Preset values:**

| Value | Behaviour |
|---|---|
| `None` | Normal random game (6-card demo deck by default; swap to full 52-card deck in `RandomDealStrategy`) |
| `PlayerWinsRound1` | Player holds 4 Aces, opponent holds 4 Twos — player wins immediately |
| `OpponentWinsRound1` | Opponent holds 4 Aces, player holds 4 Twos — opponent wins immediately |
| `TripleWar` | Forces three consecutive ties before the player wins all cards in the pot |
| `WarInsufficientCards` | Triggers a war when the opponent has too few cards to complete it — tests early game-over logic |

> **Tip:** Debug presets bypass the saved game state. To reset a normal game, use the Unity menu **Tools → Delete War Game Save**.

---

### GameBoard

| Field | Type | Description |
|---|---|---|
| `Deck` | `CardDeck` | Pool of card prefab instances used for initial deals |
| `Player Hand` | `CardsContainerView` | Player's hand pile (bottom of screen) |
| `Opponent Hand` | `CardsContainerView` | Opponent's hand pile (top of screen) |
| `Player Captured Cards` | `CardsContainerView` | Player's won-cards pile |
| `Opponent Captured Cards` | `CardsContainerView` | Opponent's won-cards pile |
| `Game Battle Area` | `BattleTableView` | Center area: comparison slots and war pot |
| `Deal Card Animation Duration` | `float` | Seconds per card during the initial deal animation (default `0.2`) |

---

### BattleTableView

| Field | Type | Description |
|---|---|---|
| `Player Slot` | `CardsContainerView` | Center slot for player's face-up comparison card |
| `Opponent Slot` | `CardsContainerView` | Center slot for opponent's face-up comparison card |
| `War Cards Container` | `CardsContainerView` | Shared pot for face-down war cards |
| `Animation Duration` | `float` | Seconds per card move in the battle area (default `0.5`) |

---

### CardView (card prefab)

| Field | Type | Description |
|---|---|---|
| `Root` | `RectTransform` | Transform used for movement animations |
| `Background` | `Image` | Card background image component |
| `Rank Text` | `TextMeshProUGUI` | Text component displaying the rank label |
| `Face Up Color` | `Color` | Background colour when card is revealed (default white) |
| `Face Down Color` | `Color` | Background colour when card is hidden (default blue `#2E5CB8`) |
| `Animation Duration` | `float` | Seconds for face-up/face-down flip animation (default `0.2`) |

---

### CardDeck

| Field | Type | Description |
|---|---|---|
| `Card Parent` | `RectTransform` | Parent transform where pooled card objects live |
| `Card Prefab` | `CardView` | Prefab instantiated to fill the pool |

---

## Network Simulation (FakeWarServer)

The server simulates real network conditions via private constants in `FakeWarServer.cs`:

| Constant | Default | Effect |
|---|---|---|
| `MinDelayMs` | `150` | Minimum simulated round-trip latency |
| `MaxDelayMs` | `500` | Maximum simulated round-trip latency |
| `NetworkErrorChance` | `5` (%) | Probability of a random network error response |
| `TimeoutChance` | `2` (%) | Probability of a simulated request timeout |

These can be adjusted directly in `FakeWarServer.cs` to stress-test client retry behaviour.

---

## Dependencies

- **Unity 2022.3.21**
- **DOTween** — card movement and flip animations
- **UniTask** — async/await without coroutines
- **TextMesh Pro** — card rank labels

---

## Running the Project

1. Open the project in **Unity 2022.3.21**.
2. Open the main scene.
3. Press **Play**.
4. Click anywhere on the screen to advance the game one round at a time.
5. To start a fresh game, use **Tools → Delete War Game Save** from the Unity menu bar, then press Play again.
