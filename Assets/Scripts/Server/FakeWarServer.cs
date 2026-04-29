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

                    _isGameOver = true;
                    SaveState();
                    return BuildGameOverResponse(earlyResult, earlyGameOver: true);
                }

                // Reshuffle upfront so we always have enough cards in hand
                pReshuffled = EnsureCards(_playerHand, _playerCaptured, needed: 4);
                oReshuffled = EnsureCards(_opponentHand, _opponentCaptured, needed: 4);

                for (int i = 0; i < 3; i++)
                {
                    _warFaceDown.Add(TakeTop(_playerHand));
                    _warFaceDown.Add(TakeTop(_opponentHand));
                }
                playerWarCards   = 3;
                opponentWarCards = 3;
            }
            else
            {
                pReshuffled = EnsureCards(_playerHand, _playerCaptured, needed: 1);
                oReshuffled = EnsureCards(_opponentHand, _opponentCaptured, needed: 1);
            }

            var pCard = TakeTop(_playerHand);
            var oCard = TakeTop(_opponentHand);

            var result = pCard > oCard ? CompareResult.PlayerWins
                       : pCard < oCard ? CompareResult.OpponentWins
                       :                 CompareResult.Tie;

            if (result == CompareResult.Tie)
            {
                _playerSlot.Add(pCard);
                _opponentSlot.Add(oCard);
                _isWarActive = true;
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

        // ── Game init ─────────────────────────────────────────────────────────

        private void InitNewGame()
        {
            var deck = new List<CardRank>();
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                for (int i = 0; i < 4; i++)
                    deck.Add(rank);

            deck.Shuffle();

            _playerHand.Clear();
            _opponentHand.Clear();
            _playerCaptured.Clear();
            _opponentCaptured.Clear();
            _warFaceDown.Clear();
            _playerSlot.Clear();
            _opponentSlot.Clear();
            _isWarActive = false;
            _isGameOver  = false;

            for (int i = 0; i < deck.Count; i++)
            {
                if (i % 2 == 0) _playerHand.Add(deck[i]);
                else            _opponentHand.Add(deck[i]);
            }
        }

        private void RestoreState(SavedGameState s)
        {
            _playerHand       = new List<CardRank>(s.PlayerHand       ?? Array.Empty<CardRank>());
            _opponentHand     = new List<CardRank>(s.OpponentHand     ?? Array.Empty<CardRank>());
            _playerCaptured   = new List<CardRank>(s.PlayerCaptured   ?? Array.Empty<CardRank>());
            _opponentCaptured = new List<CardRank>(s.OpponentCaptured ?? Array.Empty<CardRank>());
            _warFaceDown      = new List<CardRank>(s.WarFaceDown      ?? Array.Empty<CardRank>());
            _playerSlot       = new List<CardRank>(s.PlayerSlotRanks  ?? Array.Empty<CardRank>());
            _opponentSlot     = new List<CardRank>(s.OpponentSlotRanks ?? Array.Empty<CardRank>());
            _isWarActive      = s.IsWarActive;
            _isGameOver       = s.IsGameOver;
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
        private static bool EnsureCards(List<CardRank> hand, List<CardRank> captured, int needed)
        {
            if (hand.Count >= needed || captured.Count == 0) return false;
            hand.AddRange(captured);
            hand.Shuffle();
            captured.Clear();
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
                IsGameOver        = _isGameOver
            };
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(state));
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

        private static void DeleteSave()
        {
            try { File.Delete(SavePath); }
            catch { /* best-effort */ }
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
