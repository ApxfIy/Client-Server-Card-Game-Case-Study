using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using WarGame.Client.Animation;

namespace WarGame.Client.Views
{
    public class GameController : IDisposable
    {
        private readonly GameBoard _board;

        public GameController(GameBoard board)
        {
            _board = board;
        }

        public void Initialize()
        {
            _board.PlayerHand.OnClick += OnPlayerCardClick;
            _board.OpponentHand.OnClick += OnOpponentCardClick;
        }

        public void Dispose()
        {
            _board.PlayerHand.OnClick -= OnPlayerCardClick;
            _board.OpponentHand.OnClick -= OnOpponentCardClick;
        }

        private void OnPlayerCardClick(CardView view, CardsContainerView hand)
        {
            Debug.Log($"Clicked card: {view} in hand: {hand.gameObject.name}");
            
            // TODO plug in fake server here
            hand.RemoveCard(view);

            DOTween.Sequence()
                   .Append(view.FaceUp())
                   .Append(_board.GameBattleArea.AddCardToPlayerSlot(view));
        }

        private async void OnOpponentCardClick(CardView view, CardsContainerView hand)
        {
            Debug.Log($"Clicked card: {view} in hand: {hand.gameObject.name}");
            
            // TODO plug in fake server here
            hand.RemoveCard(view);

            await DOTween.Sequence()
                   .Append(view.FaceUp())
                   .Append(_board.GameBattleArea.AddCardToOpponentSlot(view)).ToUniTask();

            if (_board.GameBattleArea.PlayerSlotCards.Count == 1 && _board.GameBattleArea.OpponentSlotCards.Count == 1)
            {
                var playerCard = _board.GameBattleArea.PlayerSlotCards.First();
                var opponentCard = _board.GameBattleArea.OpponentSlotCards.First();

                if (playerCard.Rank > opponentCard.Rank)
                {
                    _board.AddCapturedCardToPlayer(playerCard);
                    _board.AddCapturedCardToPlayer(opponentCard);
                }
                else if (playerCard.Rank < opponentCard.Rank)
                {
                    _board.AddCapturedCardToOpponent(playerCard);
                    _board.AddCapturedCardToOpponent(opponentCard);
                }
                else
                {
                    Debug.LogError("It's war");
                }
            }
        }
    }
}