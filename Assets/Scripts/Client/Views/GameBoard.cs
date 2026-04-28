using DG.Tweening;
using UnityEngine;

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

        public Tween DealCardToPlayer() => DealCardTo(PlayerHand);
        public Tween DealCardToOpponent() => DealCardTo(OpponentHand);
        public Tween AddCapturedCardToPlayer(CardView cardView) => PlayerCapturedCards.AddCard(cardView, dealCardAnimationDuration);
        public Tween AddCapturedCardToOpponent(CardView cardView) => OpponentCapturedCards.AddCard(cardView, dealCardAnimationDuration);

        public Tween DealCardTo(CardsContainerView hand)
        {
            if (Deck.IsEmpty)
            {
                // throw?
                return DOTween.Sequence();
            }

            var card = Deck.GetCard();
            return hand.AddCard(card, dealCardAnimationDuration);
        }

        public Tween DealCardsToPlayers()
        {
            var sequence = DOTween.Sequence();

            while (deck.IsEmpty == false)
            {
                var internalSequence = DOTween.Sequence();
                internalSequence.Join(DealCardToPlayer());
                internalSequence.Join(DealCardToOpponent());
                sequence.Append(internalSequence);
            }
            
            return sequence;
        }
    }
}