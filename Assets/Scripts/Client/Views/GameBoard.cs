using DG.Tweening;
using UnityEngine;
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
        [SerializeField] private float dealCardAnimationDuration = 0.2f;

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

        // Animates dealing playerCount cards to the player and opponentCount to the opponent,
        // interleaving them one pair at a time (matching the visual deal rhythm).
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

        // Instantly restores visual state from a server-provided snapshot (no animation).
        // Cards are placed face-down (unknown rank) except for revealed war slot cards.
        public void RestoreFromState(StartGameResponse state)
        {
            for (var i = 0; i < state.PlayerHandCount; i++)
                playerHand.AddCardImmediate(deck.GetCard());

            for (var i = 0; i < state.OpponentHandCount; i++)
                opponentHand.AddCardImmediate(deck.GetCard());

            for (var i = 0; i < state.PlayerCapturedCount; i++)
                playerCapturedCards.AddCardImmediate(deck.GetCard());

            for (var i = 0; i < state.OpponentCapturedCount; i++)
                opponentCapturedCards.AddCardImmediate(deck.GetCard());

            if (!state.IsWarActive) return;

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

            return hand.AddCard(deck.GetCard(), dealCardAnimationDuration);
        }
    }
}