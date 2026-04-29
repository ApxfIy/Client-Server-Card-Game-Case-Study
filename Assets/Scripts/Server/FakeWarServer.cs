using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WarGame.Shared;

namespace WarGame.Server
{
    public class FakeWarServer
    {
        private List<CardRank> _playerHand       = new();
        private List<CardRank> _opponentHand     = new();
        private List<CardRank> _playerCaptured   = new();
        private List<CardRank> _opponentCaptured = new();

        // War table state — kept separate so the client can restore visuals exactly
        private List<CardRank> _warFaceDown    = new(); // anonymous face-down pot cards
        private List<CardRank> _playerSlot     = new(); // revealed player comparison cards
        private List<CardRank> _opponentSlot   = new(); // revealed opponent comparison cards

        private bool _isWarActive;
        private bool _isGameOver;
        private int  _reshuffleCount;

        private readonly IDealStrategy _dealStrategy;

        public FakeWarServer(IDealStrategy strategy = null) =>
            _dealStrategy = strategy ?? new RandomDealStrategy();

        private const float MinDelayMs         = 150f;
        private const float MaxDelayMs         = 500f;
        private const float NetworkErrorChance = 0.05f;
        private const float TimeoutChance      = 0.02f;

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, "war_game_state.json");

        // ── Public API ────────────────────────────────────────────────────────

        public async UniTask<StartGameResponse> StartGameAsync()
        {
            await SimulateDelay();
            ThrowIfNetworkFault();

            bool isRestored = false;
            if (TryLoadState(out var saved))
            {
                if (saved.IsGameOver)
                {
                    DeleteSave();
                    InitNewGame();
                }
                else
                {
                    RestoreState(saved);
                    isRestored = true;
                }
            }
            else
            {
                InitNewGame();
            }

            if (isRestored)
            {
                string war = _isWarActive
                    ? $" | war active — pot: {_warFaceDown.Count} cards, slots: {_playerSlot.Count} each"
                    : " | no war pending";
                GameLogger.Server(
                    $"Restored from save — Player hand: {_playerHand.Count} captured: {_playerCaptured.Count} | " +
                    $"Opp hand: {_opponentHand.Count} captured: {_opponentCaptured.Count}{war}");
            }
            else
            {
                GameLogger.Server(
                    $"New game — Player: {_playerHand.Count} cards | Opponent: {_opponentHand.Count} cards");
            }

            return new StartGameResponse
            {
                Status                = ResponseStatus.Success,
                IsRestoredGame        = isRestored,
                PlayerHandCount       = _playerHand.Count,
                OpponentHandCount     = _opponentHand.Count,
                PlayerCapturedCount   = _playerCaptured.Count,
                OpponentCapturedCount = _opponentCaptured.Count,
                IsWarActive           = _isWarActive,
                WarFaceDownCount      = _warFaceDown.Count,
                PlayerSlotRanks       = _playerSlot.ToArray(),
                OpponentSlotRanks     = _opponentSlot.ToArray()
            };
        }

        public async UniTask<PlayRoundResponse> PlayRoundAsync()
        {
            await SimulateDelay();
            ThrowIfNetworkFault();

            int playerWarCards   = 0;
            int opponentWarCards = 0;
            bool pReshuffled = false;
            bool oReshuffled = false;

            if (_isWarActive)
            {
                int pTotal = _playerHand.Count + _playerCaptured.Count;
                int oTotal = _opponentHand.Count + _opponentCaptured.Count;

                if (pTotal < 4 || oTotal < 4)
                {
                    GameResult earlyResult;
                    if (pTotal < 4 && oTotal < 4) earlyResult = GameResult.Draw;
                    else if (pTotal < 4)           earlyResult = GameResult.OpponentWins;
                    else                           earlyResult = GameResult.PlayerWins;

                    GameLogger.Server(
                        $"Cannot continue war — Player total: {pTotal}, Opponent total: {oTotal} " +
                        $"→ {earlyResult} (early termination)");

                    _isGameOver = true;
                    SaveState();
                    return BuildGameOverResponse(earlyResult, earlyGameOver: true);
                }

                pReshuffled = EnsureCards(_playerHand, _playerCaptured, needed: 4);
                oReshuffled = EnsureCards(_opponentHand, _opponentCaptured, needed: 4);

                if (pReshuffled)
                    GameLogger.Server($"Player reshuffled captured pile into hand ({_playerHand.Count} cards)");
                if (oReshuffled)
                    GameLogger.Server($"Opponent reshuffled captured pile into hand ({_opponentHand.Count} cards)");

                for (int i = 0; i < 3; i++)
                {
                    _warFaceDown.Add(TakeTop(_playerHand));
                    _warFaceDown.Add(TakeTop(_opponentHand));
                }
                playerWarCards   = 3;
                opponentWarCards = 3;

                GameLogger.Server($"War cards dealt — 3 face-down per side (pot: {_warFaceDown.Count} cards total)");
            }
            else
            {
                pReshuffled = EnsureCards(_playerHand, _playerCaptured, needed: 1);
                oReshuffled = EnsureCards(_opponentHand, _opponentCaptured, needed: 1);

                if (pReshuffled)
                    GameLogger.Server($"Player reshuffled captured pile into hand ({_playerHand.Count} cards)");
                if (oReshuffled)
                    GameLogger.Server($"Opponent reshuffled captured pile into hand ({_opponentHand.Count} cards)");
            }

            var pCard = TakeTop(_playerHand);
            var oCard = TakeTop(_opponentHand);

            var result = pCard > oCard ? CompareResult.PlayerWins
                       : pCard < oCard ? CompareResult.OpponentWins
                       :                 CompareResult.Tie;

            GameLogger.Server(
                $"Compare: Player {pCard.RankToString()} vs Opponent {oCard.RankToString()} → {result}");

            if (result == CompareResult.Tie)
            {
                _playerSlot.Add(pCard);
                _opponentSlot.Add(oCard);
                _isWarActive = true;

                GameLogger.Server(
                    $"Tie — slot size: {_playerSlot.Count} card(s) each | " +
                    $"Player hand: {_playerHand.Count} captured: {_playerCaptured.Count} | " +
                    $"Opp hand: {_opponentHand.Count} captured: {_opponentCaptured.Count}");

                SaveState();
                return new PlayRoundResponse
                {
                    Status                 = ResponseStatus.Success,
                    PlayerCard             = pCard,
                    OpponentCard           = oCard,
                    PlayerWarCardsPlayed   = playerWarCards,
                    OpponentWarCardsPlayed = opponentWarCards,
                    PlayerHandReshuffled   = pReshuffled,
                    OpponentHandReshuffled = oReshuffled,
                    RoundResult            = CompareResult.Tie,
                    PlayerHandCount        = _playerHand.Count,
                    OpponentHandCount      = _opponentHand.Count,
                    PlayerCapturedCount    = _playerCaptured.Count,
                    OpponentCapturedCount  = _opponentCaptured.Count
                };
            }

            // Winner collects the entire pot (face-down war cards + slot cards + comparison cards)
            _isWarActive = false;
            var winnerCaptured = result == CompareResult.PlayerWins
                ? _playerCaptured : _opponentCaptured;

            int potSize = _warFaceDown.Count + _playerSlot.Count + _opponentSlot.Count + 2;
            winnerCaptured.AddRange(_warFaceDown);
            winnerCaptured.AddRange(_playerSlot);
            winnerCaptured.Add(pCard);
            winnerCaptured.AddRange(_opponentSlot);
            winnerCaptured.Add(oCard);

            _warFaceDown.Clear();
            _playerSlot.Clear();
            _opponentSlot.Clear();

            bool pOut = _playerHand.Count == 0 && _playerCaptured.Count == 0;
            bool oOut = _opponentHand.Count == 0 && _opponentCaptured.Count == 0;
            _isGameOver = pOut || oOut;

            GameResult? finalResult = null;
            if (_isGameOver)
            {
                if (pOut && oOut) finalResult = GameResult.Draw;
                else if (pOut)   finalResult = GameResult.OpponentWins;
                else             finalResult = GameResult.PlayerWins;
            }

            string winnerName = result == CompareResult.PlayerWins ? "Player" : "Opponent";
            GameLogger.Server(
                $"{winnerName} wins the round (+{potSize} cards) | " +
                $"Player hand: {_playerHand.Count} captured: {_playerCaptured.Count} | " +
                $"Opp hand: {_opponentHand.Count} captured: {_opponentCaptured.Count}");

            if (_isGameOver)
                GameLogger.Server($"Game over — {finalResult}");

            SaveState();

            return new PlayRoundResponse
            {
                Status                 = ResponseStatus.Success,
                PlayerCard             = pCard,
                OpponentCard           = oCard,
                PlayerWarCardsPlayed   = playerWarCards,
                OpponentWarCardsPlayed = opponentWarCards,
                PlayerHandReshuffled   = pReshuffled,
                OpponentHandReshuffled = oReshuffled,
                RoundResult            = result,
                PlayerHandCount        = _playerHand.Count,
                OpponentHandCount      = _opponentHand.Count,
                PlayerCapturedCount    = _playerCaptured.Count,
                OpponentCapturedCount  = _opponentCaptured.Count,
                IsGameOver             = _isGameOver,
                FinalResult            = finalResult ?? default
            };
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    GameLogger.Server("Deleted save file");
                }
            }
            catch (Exception e)
            {
                 /* best-effort */
            }
        }

        // ── Game init ─────────────────────────────────────────────────────────

        private void InitNewGame()
        {
            _playerHand.Clear();
            _opponentHand.Clear();
            _playerCaptured.Clear();
            _opponentCaptured.Clear();
            _warFaceDown.Clear();
            _playerSlot.Clear();
            _opponentSlot.Clear();
            _isWarActive    = false;
            _isGameOver     = false;
            _reshuffleCount = 0;

            _dealStrategy.Deal(_playerHand, _opponentHand);
        }

        private void RestoreState(SavedGameState s)
        {
            _playerHand       = new List<CardRank>(s.PlayerHand);
            _opponentHand     = new List<CardRank>(s.OpponentHand);
            _playerCaptured   = new List<CardRank>(s.PlayerCaptured);
            _opponentCaptured = new List<CardRank>(s.OpponentCaptured);
            _warFaceDown      = new List<CardRank>(s.WarFaceDown);
            _playerSlot       = new List<CardRank>(s.PlayerSlotRanks);
            _opponentSlot     = new List<CardRank>(s.OpponentSlotRanks);
            _isWarActive      = s.IsWarActive;
            _isGameOver       = s.IsGameOver;
            _reshuffleCount   = s.ReshuffleCount;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CardRank TakeTop(List<CardRank> list)
        {
            int last = list.Count - 1;
            var card = list[last];
            list.RemoveAt(last);
            return card;
        }

        // Reshuffles captured into hand when hand has fewer cards than needed.
        // Returns true if a reshuffle occurred.
        private bool EnsureCards(List<CardRank> hand, List<CardRank> captured, int needed)
        {
            if (hand.Count >= needed || captured.Count == 0) 
                return false;

            hand.AddRange(captured);
            captured.Clear();

            _dealStrategy.Shuffle(hand, ++_reshuffleCount);

            return true;
        }

        private PlayRoundResponse BuildGameOverResponse(GameResult result, bool earlyGameOver)
        {
            return new PlayRoundResponse
            {
                Status                = ResponseStatus.Success,
                IsGameOver            = true,
                EarlyGameOver         = earlyGameOver,
                FinalResult           = result,
                PlayerHandCount       = _playerHand.Count,
                OpponentHandCount     = _opponentHand.Count,
                PlayerCapturedCount   = _playerCaptured.Count,
                OpponentCapturedCount = _opponentCaptured.Count
            };
        }

        // ── Persistence ───────────────────────────────────────────────────────

        private void SaveState()
        {
            var state = new SavedGameState
            {
                PlayerHand        = _playerHand.ToArray(),
                OpponentHand      = _opponentHand.ToArray(),
                PlayerCaptured    = _playerCaptured.ToArray(),
                OpponentCaptured  = _opponentCaptured.ToArray(),
                IsWarActive       = _isWarActive,
                WarFaceDown       = _warFaceDown.ToArray(),
                PlayerSlotRanks   = _playerSlot.ToArray(),
                OpponentSlotRanks = _opponentSlot.ToArray(),
                IsGameOver        = _isGameOver,
                ReshuffleCount    = _reshuffleCount
            };
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(state));
                GameLogger.Server("State saved to disk");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Server] Failed to save state: {e.Message}");
            }
        }

        private static bool TryLoadState(out SavedGameState state)
        {
            if (!File.Exists(SavePath)) { state = null; return false; }
            try
            {
                state = JsonUtility.FromJson<SavedGameState>(File.ReadAllText(SavePath));
                return state != null;
            }
            catch { state = null; return false; }
        }

        // ── Network simulation ────────────────────────────────────────────────

        private static async UniTask SimulateDelay()
        {
            int ms = (int)UnityEngine.Random.Range(MinDelayMs, MaxDelayMs);
            await UniTask.Delay(ms);
        }

        private static void ThrowIfNetworkFault()
        {
            float roll = UnityEngine.Random.value;
            if (roll < TimeoutChance)
                throw new TimeoutException("Server timed out");
            if (roll < TimeoutChance + NetworkErrorChance)
                throw new Exception("Network unreachable");
        }
    }
}
