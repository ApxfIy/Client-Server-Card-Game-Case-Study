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

        public Tween AddCardToPlayerSlot(CardView card) => playerSlot.AddCard(card, animationDuration);
        public Tween AddCardToOpponentSlot(CardView card) => opponentSlot.AddCard(card, animationDuration);
        public Tween AddCardToWarSlot(CardView card) => warCardsContainer.AddCard(card, animationDuration);

        public void AddCardToPlayerSlotImmediate(CardView card) => playerSlot.AddCardImmediate(card);
        public void AddCardToOpponentSlotImmediate(CardView card) => opponentSlot.AddCardImmediate(card);
        public void AddCardToWarSlotImmediate(CardView card) => warCardsContainer.AddCardImmediate(card);

        public List<CardView> TakeAllCards()
        {
            var all = new List<CardView>();
            all.AddRange(playerSlot.TakeAllCards());
            all.AddRange(opponentSlot.TakeAllCards());
            all.AddRange(warCardsContainer.TakeAllCards());
            return all;
        }
    }
}