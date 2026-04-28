using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace WarGame.Client.Views
{
    public class BattleTableView : MonoBehaviour
    {
        [SerializeField] private CardsContainerView playerSlot;
        [SerializeField] private CardsContainerView opponentSlot;
        [SerializeField] private CardsContainerView warCardsContainer;
        [SerializeField] private float animationDuration = 0.5f;

        public IReadOnlyList<CardView> PlayerSlotCards => playerSlot.Cards;
        public IReadOnlyList<CardView> OpponentSlotCards => opponentSlot.Cards;
        public IReadOnlyList<CardView> WarSlotCards => warCardsContainer.Cards;

        public Tween AddCardToPlayerSlot(CardView cardView) => playerSlot.AddCard(cardView, animationDuration);
        public Tween AddCardToOpponentSlot(CardView cardView) => opponentSlot.AddCard(cardView, animationDuration);
        public Tween AddCardToWarSlot(CardView cardView) => warCardsContainer.AddCard(cardView, animationDuration);
    }
}