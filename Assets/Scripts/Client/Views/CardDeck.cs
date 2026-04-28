using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class CardDeck : MonoBehaviour
    {
        [SerializeField] private RectTransform cardParent;
        [SerializeField] private CardView cardPrefab;

        public IReadOnlyList<CardView> Cards => _cards;
        public bool IsEmpty => _cards.Count == 0;

        private readonly List<CardView> _cards = new();

        public void Initialize(IEnumerable<CardRank> cards)
        {
            foreach (var cardRank in cards)
            {
                var cardView = Instantiate(cardPrefab, cardParent);
                cardView.Initialize(cardRank, false);
                _cards.Add(cardView);
            }
        }

        public CardView GetCard()
        {
            if (IsEmpty)
                throw new Exception("Card Deck is empty");
            
            var topCard = _cards.Last();
            topCard.transform.SetParent(null);
            _cards.RemoveAt(_cards.Count - 1);
            return topCard;
        }

        public void Shuffle()
        {
            _cards.Shuffle();
        }
    }
}