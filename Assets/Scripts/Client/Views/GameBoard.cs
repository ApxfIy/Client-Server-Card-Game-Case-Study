using DG.Tweening;
using UnityEngine;
using WarGame.Client.Animation;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private CardDeck deck;
        [SerializeField] private CardsContainerView playerHand;
        [SerializeField] private CardsContainerView opponentHand;
        [SerializeField] private CardsContainerView playerCapturedCards;
        [SerializeField] private CardsContainerView opponentCapturedCards;
        [SerializeField] private BattleTableView gameBattleArea;
        [SerializeField] private AnimationConfig animationConfig;

        public CardDeck Deck => deck;
        public CardsContainerView PlayerHand => playerHand;
        public CardsContainerView OpponentHand => opponentHand;
        public CardsContainerView PlayerCapturedCards => playerCapturedCards;
        public CardsContainerView OpponentCapturedCards => opponentCapturedCards;
        public BattleTableView GameBattleArea => gameBattleArea;

        public Tween DealCardToPlayer()
        {
            return DealCardTo(playerHand);
        }

        public Tween DealCardToOpponent()
        {
            return DealCardTo(opponentHand);
        }

        public Tween DealCardsToPlayers(int playerCount, int opponentCount)
        {
            var sequence = DOTween.Sequence();
            var total = Mathf.Max(playerCount, opponentCount);

            for (var i = 0; i < total; i++)
            {
                var inner = DOTween.Sequence();

                if (i < playerCount)
                    inner.Join(DealCardToPlayer());

                if (i < opponentCount)
                    inner.Join(DealCardToOpponent());

                sequence.Append(inner);
            }

            return sequence;
        }

        public void RestoreFromState(StartGameResponse state)
        {
            RestoreHandsAndCapturedPiles(state);

            if (state.IsWarActive)
                RestoreWarBattleArea(state);
        }

        private void RestoreHandsAndCapturedPiles(StartGameResponse state)
        {
            for (var i = 0; i < state.PlayerHandCount; i++)
                playerHand.AddCardImmediate(deck.GetCard());

            for (var i = 0; i < state.OpponentHandCount; i++)
                opponentHand.AddCardImmediate(deck.GetCard());

            for (var i = 0; i < state.PlayerCapturedCount; i++)
                playerCapturedCards.AddCardImmediate(deck.GetCard());

            for (var i = 0; i < state.OpponentCapturedCount; i++)
                opponentCapturedCards.AddCardImmediate(deck.GetCard());
        }

        private void RestoreWarBattleArea(StartGameResponse state)
        {
            for (var i = 0; i < state.WarFaceDownCount; i++)
                gameBattleArea.AddCardToWarSlotImmediate(deck.GetCard());

            foreach (var rank in state.PlayerSlotRanks)
            {
                var card = deck.GetCard();
                card.Initialize(rank, true);
                gameBattleArea.AddCardToPlayerSlotImmediate(card);
            }

            foreach (var rank in state.OpponentSlotRanks)
            {
                var card = deck.GetCard();
                card.Initialize(rank, true);
                gameBattleArea.AddCardToOpponentSlotImmediate(card);
            }
        }

        private Tween DealCardTo(CardsContainerView hand)
        {
            if (deck.IsEmpty)
                return DOTween.Sequence();

            return hand.AddCard(deck.GetCard(), animationConfig.DealCardDuration);
        }
    }
}