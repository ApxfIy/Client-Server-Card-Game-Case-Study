using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using WarGame.Client.Animation;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class GameController : IDisposable
    {
        private readonly GameBoard _board;
        private readonly GameClient _client;
        private readonly InputManager _inputManager;

        private bool _isProcessingRound;

        public GameController(GameBoard board, GameClient client, InputManager inputManager)
        {
            _board        = board;
            _client       = client;
            _inputManager = inputManager;
        }

        public void Initialize()
        {
            _inputManager.OnInput += OnAnyInput;
        }

        public void Dispose()
        {
            _inputManager.OnInput -= OnAnyInput;
        }

        private void OnAnyInput()
        {
            if (_isProcessingRound) return;
            PlayRoundAsync().Forget();
        }

        private async UniTaskVoid PlayRoundAsync()
        {
            _isProcessingRound = true;
            _inputManager.DisableInput();

            var response = await _client.PlayRoundAsync();

            if (response.Status != ResponseStatus.Success)
            {
                Debug.LogError($"[Server] {response.ErrorMessage}");
                FinishRound();
                return;
            }

            // Game ended before any cards could be played (e.g. not enough cards for war)
            if (response.EarlyGameOver)
            {
                EndGame(response.FinalResult);
                return;
            }

            // Reshuffle captured → hand when the server reshuffled (instant, no animation)
            if (response.PlayerHandReshuffled)   AnimateReshuffle(_board.PlayerCapturedCards, _board.PlayerHand);
            if (response.OpponentHandReshuffled) AnimateReshuffle(_board.OpponentCapturedCards, _board.OpponentHand);

            // Face-down war cards (3 per side when continuing a war round)
            if (response.PlayerWarCardsPlayed > 0)
            {
                var warSeq = DOTween.Sequence();
                for (int i = 0; i < response.PlayerWarCardsPlayed; i++)
                    warSeq.Append(_board.GameBattleArea.AddCardToWarSlot(_board.PlayerHand.TakeTopCard()));
                for (int i = 0; i < response.OpponentWarCardsPlayed; i++)
                    warSeq.Append(_board.GameBattleArea.AddCardToWarSlot(_board.OpponentHand.TakeTopCard()));
                await warSeq.ToUniTask();
            }

            // Comparison cards slide to their slots
            var pCard = _board.PlayerHand.TakeTopCard();
            var oCard = _board.OpponentHand.TakeTopCard();

            await DOTween.Sequence()
                .Join(_board.GameBattleArea.AddCardToPlayerSlot(pCard))
                .Join(_board.GameBattleArea.AddCardToOpponentSlot(oCard))
                .ToUniTask();

            // Reveal ranks provided by the server
            await DOTween.Sequence()
                .Join(pCard.Reveal(response.PlayerCard))
                .Join(oCard.Reveal(response.OpponentCard))
                .ToUniTask();

            await UniTask.Delay(500);

            // Tie: server will handle war on the next round
            if (response.RoundResult == CompareResult.Tie)
            {
                FinishRound();
                return;
            }

            // Collect all table cards to the winner's pile
            var winnerPile = response.RoundResult == CompareResult.PlayerWins
                ? _board.PlayerCapturedCards
                : _board.OpponentCapturedCards;

            var tableCards = _board.GameBattleArea.TakeAllCards();
            var collectSeq = DOTween.Sequence();
            foreach (var card in tableCards)
            {
                card.Initialize(null, false);
                collectSeq.Join(winnerPile.AddCard(card, 0.3f));
            }
            await collectSeq.ToUniTask();

            if (response.IsGameOver)
            {
                EndGame(response.FinalResult);
                return;
            }

            FinishRound();
        }

        // Instantly moves all cards from one container to another (for captured → hand reshuffle)
        private void AnimateReshuffle(CardsContainerView from, CardsContainerView to)
        {
            var cards = from.TakeAllCards();
            cards.Shuffle();
            foreach (var card in cards)
            {
                card.Initialize(null, false);
                to.AddCardImmediate(card);
            }
        }

        private void FinishRound()
        {
            _isProcessingRound = false;
            _inputManager.EnableInput();
        }

        private void EndGame(GameResult result)
        {
            Debug.Log($"Game over: {result}");
            // TODO: show game-over UI
        }
    }
}
